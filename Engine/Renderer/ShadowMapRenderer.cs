using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.World;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class ShadowMapRenderer : IDisposable
{
    public const uint DefaultMapSize = 2048;
    public const float DefaultWorldSpan = 420f;

    private GraphicsDevice? _graphicsDevice;
    private uint _mapSize = DefaultMapSize;
    private float _worldSpan = DefaultWorldSpan;

    // Active shadow map (used by main scene sampling)
    private Texture? _depthTexture;
    private TextureView? _depthView;
    private Framebuffer? _framebuffer;

    // Static shadow map cache (holds terrain + static PBR models)
    private Texture? _staticDepthTexture;
    private TextureView? _staticDepthView;
    private Framebuffer? _staticFramebuffer;

    // Static shadow cache invalidation trackers
    private bool _staticCacheValid;
    private Matrix4x4 _cachedMatrix;
    private Vector3 _cachedFocus;
    private Vector3 _cachedSunDirection;
    private TerrainDetailQuality _cachedTerrainDetail;
    private ModelLodQuality _cachedModelLod;
    private float _cachedShadowDistance;
    private float _cachedVegetationDistance;
    private float _cachedGroundClutterDistance;
    private FarVegetationMode _cachedFarVegetation;
    private int _framesSinceStaticUpdate;

    private DeviceBuffer? _depthMatrixBuffer;
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
    public float WorldSpan => _worldSpan;
    public float HalfWorldSpan => _worldSpan * 0.5f;
    public bool StaticCacheValid => _staticCacheValid;

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

        // Active depth texture
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

        // Static cache depth texture
        _staticDepthTexture = factory.CreateTexture(TextureDescription.Texture2D(
            _mapSize,
            _mapSize,
            mipLevels: 1,
            arrayLayers: 1,
            PixelFormat.R32_Float,
            TextureUsage.DepthStencil | TextureUsage.Sampled));
        _staticDepthView = factory.CreateTextureView(_staticDepthTexture);
        _staticFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            new FramebufferAttachmentDescription(_staticDepthTexture, 0),
            Array.Empty<FramebufferAttachmentDescription>()));

        _depthMatrixBuffer = factory.CreateBuffer(new BufferDescription(
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
            _depthMatrixBuffer,
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
                    "TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription(
                    "WindWeight", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1)));

        _actorPipeline = CreateDepthPipeline(
            factory,
            new VertexLayoutDescription(
                new VertexElementDescription(
                    "Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "Color", VertexElementSemantic.Color, VertexElementFormat.Float3),
                new VertexElementDescription(
                    "Normal", VertexElementSemantic.Normal, VertexElementFormat.Float3)));

        _staticCacheValid = false;

        EngineLog.Info(
            $"Sun shadow map initialized: {_mapSize}x{_mapSize}, world span {_worldSpan:0}m (Static Cache Enabled).");
    }

    public bool ShouldUpdateStaticShadow(
        Vector3 focusPosition,
        Vector3 sunDirection,
        TerrainDetailQuality terrainDetail,
        float shadowDistance,
        float vegetationDistance,
        float groundClutterDistance,
        ModelLodQuality modelLod,
        FarVegetationMode farVegetation)
    {
        if (!_staticCacheValid ||
            _staticFramebuffer is null ||
            _staticDepthTexture is null)
        {
            return true;
        }

        if (_cachedTerrainDetail != terrainDetail ||
            _cachedModelLod != modelLod ||
            _cachedFarVegetation != farVegetation ||
            MathF.Abs(_cachedShadowDistance - shadowDistance) > 1f ||
            MathF.Abs(_cachedVegetationDistance - vegetationDistance) > 1f ||
            MathF.Abs(_cachedGroundClutterDistance - groundClutterDistance) > 1f)
        {
            return true;
        }

        // Sun angular movement threshold (~1 deg)
        var sunNormalized = Vector3.Normalize(sunDirection);
        if (Vector3.Dot(sunNormalized, _cachedSunDirection) < 0.99985f)
        {
            return true;
        }

        // Camera movement threshold: 10.0 meters in world space.
        // Static shadow geometry is rendered with a +10m..32m boundary margin.
        if (Vector3.DistanceSquared(focusPosition, _cachedFocus) > 100.0f)
        {
            return true;
        }

        // Periodic static shadow refresh (every 480 frames ~8s)
        if (_framesSinceStaticUpdate > 480)
        {
            return true;
        }

        return false;
    }

    public Matrix4x4 BeginStaticPass(
        CommandList commandList,
        Vector3 focusPosition,
        Vector3 sunDirection,
        TerrainDetailQuality terrainDetail,
        float shadowDistance,
        float vegetationDistance,
        float groundClutterDistance,
        ModelLodQuality modelLod,
        FarVegetationMode farVegetation)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_staticFramebuffer is null || _depthMatrixBuffer is null)
            throw new InvalidOperationException("Shadow map renderer is not initialized.");

        var matrix = CalculateLightViewProjection(
            focusPosition,
            sunDirection,
            _mapSize,
            _worldSpan);

        commandList.UpdateBuffer(_depthMatrixBuffer, 0, matrix);

        _cachedMatrix = matrix;
        _cachedFocus = focusPosition;
        _cachedSunDirection = Vector3.Normalize(sunDirection);
        _cachedTerrainDetail = terrainDetail;
        _cachedShadowDistance = shadowDistance;
        _cachedVegetationDistance = vegetationDistance;
        _cachedGroundClutterDistance = groundClutterDistance;
        _cachedModelLod = modelLod;
        _cachedFarVegetation = farVegetation;
        _framesSinceStaticUpdate = 0;

        commandList.SetFramebuffer(_staticFramebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.ClearDepthStencil(1f);

        return matrix;
    }

    public void EndStaticPassAndPrepareActive(CommandList commandList)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_depthTexture is null || _staticDepthTexture is null || _framebuffer is null)
            throw new InvalidOperationException("Shadow map renderer is not initialized.");

        // Fast GPU copy of static geometry depth into active shadow map
        commandList.CopyTexture(_staticDepthTexture, _depthTexture);

        // Bind active shadow framebuffer without clearing so loadOp = Load preserves static depth
        commandList.SetFramebuffer(_framebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();

        _staticCacheValid = true;
    }

    public void BeginActivePassFromCache(CommandList commandList)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_depthTexture is null || _staticDepthTexture is null || _framebuffer is null)
            throw new InvalidOperationException("Shadow map renderer is not initialized.");

        _framesSinceStaticUpdate++;

        // Fast GPU copy of cached static depth into active shadow map
        commandList.CopyTexture(_staticDepthTexture, _depthTexture);

        // Bind active shadow framebuffer without clearing so dynamic actors can render on top
        commandList.SetFramebuffer(_framebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
    }

    public Matrix4x4 UpdateLight(
        CommandList commandList,
        Vector3 focusPosition,
        Vector3 sunDirection)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_depthMatrixBuffer is null)
            throw new InvalidOperationException("Shadow map renderer is not initialized.");

        var matrix = CalculateLightViewProjection(
            focusPosition,
            sunDirection,
            _mapSize,
            _worldSpan);
        commandList.UpdateBuffer(_depthMatrixBuffer, 0, matrix);
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
        commandList.SetFullScissorRects();
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
            _depthMatrixBuffer is null)
        {
            _mapSize = mapSize;
            return;
        }

        _graphicsDevice.WaitForIdle();

        _sampleSet?.Dispose();
        _framebuffer?.Dispose();
        _depthView?.Dispose();
        _depthTexture?.Dispose();

        _staticFramebuffer?.Dispose();
        _staticDepthView?.Dispose();
        _staticDepthTexture?.Dispose();

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

        _staticDepthTexture = factory.CreateTexture(TextureDescription.Texture2D(
            mapSize,
            mapSize,
            mipLevels: 1,
            arrayLayers: 1,
            PixelFormat.R32_Float,
            TextureUsage.DepthStencil | TextureUsage.Sampled));
        _staticDepthView = factory.CreateTextureView(_staticDepthTexture);
        _staticFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            new FramebufferAttachmentDescription(_staticDepthTexture, 0),
            Array.Empty<FramebufferAttachmentDescription>()));

        _sampleSet = factory.CreateResourceSet(new ResourceSetDescription(
            _sampleLayout,
            _depthMatrixBuffer,
            _depthView,
            _graphicsDevice.PointSampler));

        _mapSize = mapSize;
        _staticCacheValid = false;
        EngineLog.Info($"Sun shadow map resolution changed to {_mapSize}x{_mapSize}.");
    }

    public void SetWorldSpan(float worldSpan)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var clamped = Math.Clamp(worldSpan, 80f, DefaultWorldSpan);
        if (MathF.Abs(_worldSpan - clamped) > 0.1f)
        {
            _worldSpan = clamped;
            _staticCacheValid = false;
        }
    }

    public void InvalidateStaticCache()
    {
        _staticCacheValid = false;
    }

    public static Matrix4x4 CalculateLightViewProjection(
        Vector3 focusPosition,
        Vector3 sunDirection) =>
        CalculateLightViewProjection(
            focusPosition,
            sunDirection,
            DefaultMapSize,
            DefaultWorldSpan);

    public static Matrix4x4 CalculateLightViewProjection(
        Vector3 focusPosition,
        Vector3 sunDirection,
        uint mapSize) =>
        CalculateLightViewProjection(
            focusPosition,
            sunDirection,
            mapSize,
            DefaultWorldSpan);

    public static Matrix4x4 CalculateLightViewProjection(
        Vector3 focusPosition,
        Vector3 sunDirection,
        uint mapSize,
        float worldSpan)
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

        mapSize = Math.Clamp(mapSize, 512u, 4096u);
        worldSpan = Math.Clamp(worldSpan, 80f, DefaultWorldSpan);
        var texelWorldSize = worldSpan / mapSize;
        var up = MathF.Abs(Vector3.Dot(sunDirection, Vector3.UnitY)) > 0.96f
            ? Vector3.UnitZ
            : Vector3.UnitY;

        // Anchor the texel grid in light space. World X/Z snapping does not
        // align with the shadow texture for oblique sunlight, and leaves
        // vertical camera movement completely unstabilized.
        var lightRight = Vector3.Normalize(Vector3.Cross(up, sunDirection));
        var lightUp = Vector3.Cross(sunDirection, lightRight);
        var focusRight = Vector3.Dot(focusPosition, lightRight);
        var focusUp = Vector3.Dot(focusPosition, lightUp);
        var focus = focusPosition
            + lightRight * (MathF.Round(focusRight / texelWorldSize) * texelWorldSize - focusRight)
            + lightUp * (MathF.Round(focusUp / texelWorldSize) * texelWorldSize - focusUp);

        var eye = focus + sunDirection * 520f;
        var lightView = Matrix4x4.CreateLookAt(eye, focus, up);
        var lightProjection = Matrix4x4.CreateOrthographic(
            worldSpan,
            worldSpan,
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
        _depthMatrixBuffer?.Dispose();

        _framebuffer?.Dispose();
        _depthView?.Dispose();
        _depthTexture?.Dispose();

        _staticFramebuffer?.Dispose();
        _staticDepthView?.Dispose();
        _staticDepthTexture?.Dispose();

        _terrainPipeline = null;
        _pbrPipeline = null;
        _actorPipeline = null;
        _shaders = null;
        _sampleSet = null;
        _depthSet = null;
        _sampleLayout = null;
        _depthLayout = null;
        _depthMatrixBuffer = null;
        _framebuffer = null;
        _depthView = null;
        _depthTexture = null;
        _staticFramebuffer = null;
        _staticDepthView = null;
        _staticDepthTexture = null;
        _graphicsDevice = null;
    }
}
