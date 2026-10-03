using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.World;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class ShadowMapRenderer : IDisposable
{
    public const uint DefaultMapSize = 2048;
    public const float WorldSpan = 420f;
    public const float HalfWorldSpan = WorldSpan * 0.5f;

    private GraphicsDevice? _graphicsDevice;
    private uint _mapSize = DefaultMapSize;
    private Texture? _depthTexture;
    private TextureView? _depthView;
    private Framebuffer? _framebuffer;
    private DeviceBuffer? _depthMatrixBuffer;
    private DeviceBuffer? _sampleMatrixBuffer;
    private ResourceLayout? _depthLayout;
    private ResourceLayout? _sampleLayout;
    private ResourceSet? _depthSet;
    private ResourceSet? _sampleSet;
    private Pipeline? _terrainPipeline;
    private Pipeline? _pbrPipeline;
    private Pipeline? _actorPipeline;
    private Shader[]? _shaders;
    private bool _disposed;

    public uint MapSize => _mapSize;

    public ResourceLayout SampleLayout =>
        _sampleLayout ?? throw new InvalidOperationException("Shadow map renderer is not initialized.");

    public ResourceSet SampleSet =>
        _sampleSet ?? throw new InvalidOperationException("Shadow map renderer is not initialized.");

    public ResourceSet DepthSet =>
        _depthSet ?? throw new InvalidOperationException("Shadow map renderer is not initialized.");

    public Pipeline TerrainPipeline =>
        _terrainPipeline ?? throw new InvalidOperationException("Shadow map renderer is not initialized.");

    public Pipeline PbrPipeline =>
        _pbrPipeline ?? throw new InvalidOperationException("Shadow map renderer is not initialized.");

    public Pipeline ActorPipeline =>
        _actorPipeline ?? throw new InvalidOperationException("Shadow map renderer is not initialized.");

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        _graphicsDevice = graphicsDevice;
        var factory = graphicsDevice.ResourceFactory;

        _depthTexture = factory.CreateTexture(TextureDescription.Texture2D(
            _mapSize,
            _mapSize,
            mipLevels: 1,
            arrayLayers: 1,
            PixelFormat.R32_Float,
            TextureUsage.DepthStencil | TextureUsage.Sampled));

        _depthView = factory.CreateTextureView(_depthTexture);
        _framebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            new FramebufferAttachmentDescription(_depthTexture, 0),
            Array.Empty<FramebufferAttachmentDescription>()));

        _depthMatrixBuffer = factory.CreateBuffer(new BufferDescription(
            64,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _sampleMatrixBuffer = factory.CreateBuffer(new BufferDescription(
            64,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _depthLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "ShadowDepth",
                ResourceKind.UniformBuffer,
                ShaderStages.Vertex)));

        _sampleLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "ShadowData",
                ResourceKind.UniformBuffer,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "ShadowMap",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "ShadowSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment)));

        _depthSet = factory.CreateResourceSet(new ResourceSetDescription(
            _depthLayout,
            _depthMatrixBuffer));

        _sampleSet = factory.CreateResourceSet(new ResourceSetDescription(
            _sampleLayout,
            _sampleMatrixBuffer,
            _depthView,
            graphicsDevice.PointSampler));

        _shaders = ShaderLibrary.LoadPair(factory, "shadow_depth");

        _terrainPipeline = CreateDepthPipeline(
            factory,
            new VertexLayoutDescription(
                new VertexElementDescription(
                    "Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "PrimaryWeights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "SecondaryWeights", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)));

        _pbrPipeline = CreateDepthPipeline(
            factory,
            new VertexLayoutDescription(
                new VertexElementDescription(
                    "Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)));

        _actorPipeline = CreateDepthPipeline(
            factory,
            new VertexLayoutDescription(
                new VertexElementDescription(
                    "Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "Color", VertexElementSemantic.Color, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3)));

        EngineLog.Info(
            $"Sun shadow map initialized: {_mapSize}x{_mapSize}, world span {WorldSpan:0}m.");
    }

    public Matrix4x4 UpdateLight(
        CommandList commandList,
        Vector3 focusPosition,
        Vector3 sunDirection)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_depthMatrixBuffer is null || _sampleMatrixBuffer is null)
            throw new InvalidOperationException("Shadow map renderer is not initialized.");

        var matrix = CalculateLightViewProjection(
            focusPosition,
            sunDirection,
            _mapSize);
        commandList.UpdateBuffer(_depthMatrixBuffer, 0, matrix);
        commandList.UpdateBuffer(_sampleMatrixBuffer, 0, matrix);
        return matrix;
    }

    public void BeginDepthPass(CommandList commandList)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_framebuffer is null)
            throw new InvalidOperationException("Shadow map renderer is not initialized.");

        commandList.SetFramebuffer(_framebuffer);
        commandList.SetFullViewports();
        commandList.ClearDepthStencil(1f);
    }

    public void RenderActors(
        CommandList commandList,
        DeviceBuffer vertexBuffer,
        DeviceBuffer indexBuffer,
        uint indexCount)
    {
        if (indexCount == 0)
            return;

        commandList.SetPipeline(ActorPipeline);
        commandList.SetGraphicsResourceSet(0, DepthSet);
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);
        commandList.DrawIndexed(indexCount);
    }

    public void SetMapSize(uint mapSize)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        mapSize = Math.Clamp(mapSize, 512u, 4096u);
        if (_mapSize == mapSize && _depthTexture is not null)
            return;

        if (_graphicsDevice is null ||
            _sampleLayout is null ||
            _sampleMatrixBuffer is null)
        {
            _mapSize = mapSize;
            return;
        }

        _graphicsDevice.WaitForIdle();

        _sampleSet?.Dispose();
        _framebuffer?.Dispose();
        _depthView?.Dispose();
        _depthTexture?.Dispose();

        var factory = _graphicsDevice.ResourceFactory;
        _depthTexture = factory.CreateTexture(TextureDescription.Texture2D(
            mapSize,
            mapSize,
            mipLevels: 1,
            arrayLayers: 1,
            PixelFormat.R32_Float,
            TextureUsage.DepthStencil | TextureUsage.Sampled));
        _depthView = factory.CreateTextureView(_depthTexture);
        _framebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            new FramebufferAttachmentDescription(_depthTexture, 0),
            Array.Empty<FramebufferAttachmentDescription>()));
        _sampleSet = factory.CreateResourceSet(new ResourceSetDescription(
            _sampleLayout,
            _sampleMatrixBuffer,
            _depthView,
            _graphicsDevice.PointSampler));

        _mapSize = mapSize;
        EngineLog.Info($"Sun shadow map resolution changed to {_mapSize}x{_mapSize}.");
    }

    public static Matrix4x4 CalculateLightViewProjection(
        Vector3 focusPosition,
        Vector3 sunDirection) =>
        CalculateLightViewProjection(
            focusPosition,
            sunDirection,
            DefaultMapSize);

    public static Matrix4x4 CalculateLightViewProjection(
        Vector3 focusPosition,
        Vector3 sunDirection,
        uint mapSize)
    {
        if (!float.IsFinite(focusPosition.X) ||
            !float.IsFinite(focusPosition.Y) ||
            !float.IsFinite(focusPosition.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(focusPosition));
        }

        if (!float.IsFinite(sunDirection.X) ||
            !float.IsFinite(sunDirection.Y) ||
            !float.IsFinite(sunDirection.Z) ||
            sunDirection.LengthSquared() < 0.000001f)
        {
            throw new ArgumentOutOfRangeException(nameof(sunDirection));
        }

        sunDirection = Vector3.Normalize(sunDirection);

        // Snap the shadow focus to the shadow texel footprint. This greatly
        // reduces shimmering while the camera moves slowly.
        mapSize = Math.Clamp(mapSize, 512u, 4096u);
        var texelWorldSize = WorldSpan / mapSize;
        var focus = new Vector3(
            MathF.Round(focusPosition.X / texelWorldSize) * texelWorldSize,
            focusPosition.Y,
            MathF.Round(focusPosition.Z / texelWorldSize) * texelWorldSize);

        var up = MathF.Abs(Vector3.Dot(sunDirection, Vector3.UnitY)) > 0.96f
            ? Vector3.UnitZ
            : Vector3.UnitY;

        var eye = focus + sunDirection * 520f;
        var lightView = Matrix4x4.CreateLookAt(eye, focus, up);
        var lightProjection = Matrix4x4.CreateOrthographic(
            WorldSpan,
            WorldSpan,
            2f,
            1050f);

        return lightView * lightProjection;
    }

    private Pipeline CreateDepthPipeline(
        ResourceFactory factory,
        VertexLayoutDescription vertexLayout)
    {
        if (_framebuffer is null || _depthLayout is null || _shaders is null)
            throw new InvalidOperationException("Shadow map resources are incomplete.");

        return factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.Empty,
            DepthStencilStateDescription.DepthOnlyLessEqual,
            new RasterizerStateDescription(
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription([vertexLayout], _shaders),
            [_depthLayout],
            _framebuffer.OutputDescription));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _terrainPipeline?.Dispose();
        _pbrPipeline?.Dispose();
        _actorPipeline?.Dispose();

        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _sampleSet?.Dispose();
        _depthSet?.Dispose();
        _sampleLayout?.Dispose();
        _depthLayout?.Dispose();
        _sampleMatrixBuffer?.Dispose();
        _depthMatrixBuffer?.Dispose();
        _framebuffer?.Dispose();
        _depthView?.Dispose();
        _depthTexture?.Dispose();

        _terrainPipeline = null;
        _pbrPipeline = null;
        _actorPipeline = null;
        _shaders = null;
        _sampleSet = null;
        _depthSet = null;
        _sampleLayout = null;
        _depthLayout = null;
        _sampleMatrixBuffer = null;
        _depthMatrixBuffer = null;
        _framebuffer = null;
        _depthView = null;
        _depthTexture = null;
        _graphicsDevice = null;
    }
}
