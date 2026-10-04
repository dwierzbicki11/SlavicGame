using System.Numerics;
using SlavicGame.Engine.Diagnostics;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

/// <summary>
/// Produces normalized screen-space motion for the FSR temporal path. A
/// fullscreen depth-reconstruction pass covers camera motion for static world
/// geometry, then a depth-tested geometry pass overwrites visible dynamic
/// actors with their real per-object velocity.
/// </summary>
public sealed class MotionVectorRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private Texture? _motionTexture;
    private TextureView? _motionView;
    private Framebuffer? _framebuffer;
    private Framebuffer? _dynamicFramebuffer;

    private DeviceBuffer? _constants;
    private ResourceLayout? _layout;
    private ResourceSet? _set;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;

    private DeviceBuffer? _dynamicConstants;
    private ResourceLayout? _dynamicLayout;
    private ResourceSet? _dynamicSet;
    private Pipeline? _dynamicPipeline;
    private Shader[]? _dynamicShaders;

    private TextureView? _depthSource;
    private Texture? _depthTexture;
    private uint _width;
    private uint _height;
    private bool _disposed;

    public bool IsInitialized =>
        _pipeline is not null &&
        _dynamicPipeline is not null;

    public TextureView MotionVectorView =>
        _motionView ?? throw new InvalidOperationException(
            "Motion-vector renderer is not initialized.");

    public Texture MotionVectorTexture =>
        _motionTexture ?? throw new InvalidOperationException(
            "Motion-vector renderer is not initialized.");

    public void Initialize(
        GraphicsDevice graphicsDevice,
        TextureView depthSource,
        Texture depthTexture,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(depthSource);
        ArgumentNullException.ThrowIfNull(depthTexture);

        _graphicsDevice = graphicsDevice;
        _depthSource = depthSource;
        _depthTexture = depthTexture;
        var factory = graphicsDevice.ResourceFactory;

        _constants = factory.CreateBuffer(new BufferDescription(
            144,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "TemporalMatrices",
                ResourceKind.UniformBuffer,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "SceneDepth",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "PointSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment)));

        _dynamicConstants = factory.CreateBuffer(new BufferDescription(
            144,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _dynamicLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "DynamicMotionMatrices",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex)));
        _dynamicSet = factory.CreateResourceSet(
            new ResourceSetDescription(
                _dynamicLayout,
                _dynamicConstants));

        _shaders = ShaderLibrary.LoadPair(factory, "motion_vectors");
        _dynamicShaders = ShaderLibrary.LoadPair(factory, "motion_dynamic");
        RecreateTarget(width, height);

        _pipeline = factory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(
                    FaceCullMode.None,
                    PolygonFillMode.Solid,
                    FrontFace.Clockwise,
                    true,
                    false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    Array.Empty<VertexLayoutDescription>(),
                    _shaders),
                [_layout],
                _framebuffer!.OutputDescription));

        var dynamicVertexLayout = new VertexLayoutDescription(
            new VertexElementDescription(
                "CurrentPosition",
                VertexElementSemantic.Position,
                VertexElementFormat.Float3),
            new VertexElementDescription(
                "PreviousPosition",
                VertexElementSemantic.TextureCoordinate,
                VertexElementFormat.Float3));

        _dynamicPipeline = factory.CreateGraphicsPipeline(
            new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqualRead,
                new RasterizerStateDescription(
                    FaceCullMode.None,
                    PolygonFillMode.Solid,
                    FrontFace.Clockwise,
                    true,
                    false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    [dynamicVertexLayout],
                    _dynamicShaders),
                [_dynamicLayout],
                _dynamicFramebuffer!.OutputDescription));

        Rebind();

        EngineLog.Info(
            $"Temporal motion target initialized: {_width}x{_height} RG16F " +
            "(camera reconstruction + dynamic object velocity).");
    }

    public void SetSource(
        TextureView depthSource,
        Texture depthTexture,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(depthSource);
        ArgumentNullException.ThrowIfNull(depthTexture);
        if (_graphicsDevice is null)
            throw new InvalidOperationException(
                "Motion-vector renderer is not initialized.");

        _depthSource = depthSource;
        _depthTexture = depthTexture;
        width = Math.Max(1u, width);
        height = Math.Max(1u, height);
        if (_width != width || _height != height)
        {
            _graphicsDevice.WaitForIdle();
            RecreateTarget(width, height);
        }
        else
        {
            RecreateDynamicFramebuffer();
        }

        Rebind();
    }

    public TextureView Render(
        CommandList commandList,
        in TemporalFrameData frame)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_constants is null ||
            _framebuffer is null ||
            _pipeline is null ||
            _set is null ||
            _graphicsDevice is null)
        {
            throw new InvalidOperationException(
                "Motion-vector renderer is not initialized.");
        }

        commandList.UpdateBuffer(
            _constants,
            0,
            frame.InverseCurrentViewProjection);
        commandList.UpdateBuffer(
            _constants,
            64,
            frame.PreviousViewProjection);

        var parameters = new Vector4(
            _width,
            _height,
            _graphicsDevice.IsClipSpaceYInverted ? -1f : 1f,
            frame.ResetHistory ? 1f : 0f);
        commandList.UpdateBuffer(_constants, 128, parameters);

        commandList.SetFramebuffer(_framebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.ClearColorTarget(0, RgbaFloat.Black);
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _set);
        commandList.Draw(3);

        return MotionVectorView;
    }

    public void RenderDynamic(
        CommandList commandList,
        DeviceBuffer vertexBuffer,
        DeviceBuffer indexBuffer,
        uint indexCount,
        in TemporalFrameData frame)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(indexBuffer);

        if (indexCount == 0)
            return;

        if (_graphicsDevice is null ||
            _dynamicConstants is null ||
            _dynamicSet is null ||
            _dynamicPipeline is null ||
            _dynamicFramebuffer is null)
        {
            throw new InvalidOperationException(
                "Dynamic motion-vector renderer is not initialized.");
        }

        commandList.UpdateBuffer(
            _dynamicConstants,
            0,
            frame.CurrentViewProjection);
        commandList.UpdateBuffer(
            _dynamicConstants,
            64,
            frame.PreviousViewProjection);
        commandList.UpdateBuffer(
            _dynamicConstants,
            128,
            new Vector4(
                _width,
                _height,
                _graphicsDevice.IsClipSpaceYInverted ? -1f : 1f,
                frame.ResetHistory ? 1f : 0f));

        commandList.SetFramebuffer(_dynamicFramebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.SetPipeline(_dynamicPipeline);
        commandList.SetGraphicsResourceSet(0, _dynamicSet);
        commandList.SetVertexBuffer(0, vertexBuffer);
        commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt32);
        commandList.DrawIndexed(indexCount);
    }

    private void RecreateTarget(uint width, uint height)
    {
        if (_graphicsDevice is null)
            throw new InvalidOperationException(
                "Motion-vector renderer is not initialized.");

        width = Math.Max(1u, width);
        height = Math.Max(1u, height);

        _set?.Dispose();
        _set = null;
        _dynamicFramebuffer?.Dispose();
        _framebuffer?.Dispose();
        _motionView?.Dispose();
        _motionTexture?.Dispose();

        var factory = _graphicsDevice.ResourceFactory;
        _motionTexture = factory.CreateTexture(TextureDescription.Texture2D(
            width,
            height,
            1,
            1,
            PixelFormat.R16_G16_Float,
            TextureUsage.RenderTarget | TextureUsage.Sampled));
        _motionView = factory.CreateTextureView(_motionTexture);
        _framebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            null,
            [new FramebufferAttachmentDescription(_motionTexture, 0)]));

        _width = width;
        _height = height;
        RecreateDynamicFramebuffer();
    }

    private void RecreateDynamicFramebuffer()
    {
        if (_graphicsDevice is null ||
            _depthTexture is null ||
            _motionTexture is null)
        {
            return;
        }

        _dynamicFramebuffer?.Dispose();
        _dynamicFramebuffer =
            _graphicsDevice.ResourceFactory.CreateFramebuffer(
                new FramebufferDescription(
                    _depthTexture,
                    _motionTexture));
    }

    private void Rebind()
    {
        if (_graphicsDevice is null ||
            _layout is null ||
            _constants is null ||
            _depthSource is null)
        {
            return;
        }

        _set?.Dispose();
        _set = _graphicsDevice.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                _layout,
                _constants,
                _depthSource,
                _graphicsDevice.PointSampler));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _dynamicPipeline?.Dispose();
        _pipeline?.Dispose();
        _dynamicSet?.Dispose();
        _dynamicLayout?.Dispose();
        _dynamicConstants?.Dispose();
        _set?.Dispose();
        _layout?.Dispose();
        _constants?.Dispose();
        _dynamicFramebuffer?.Dispose();
        _framebuffer?.Dispose();
        _motionView?.Dispose();
        _motionTexture?.Dispose();

        if (_dynamicShaders is not null)
            foreach (var shader in _dynamicShaders)
                shader.Dispose();
        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _dynamicPipeline = null;
        _pipeline = null;
        _dynamicSet = null;
        _dynamicLayout = null;
        _dynamicConstants = null;
        _set = null;
        _layout = null;
        _constants = null;
        _dynamicFramebuffer = null;
        _framebuffer = null;
        _motionView = null;
        _motionTexture = null;
        _dynamicShaders = null;
        _shaders = null;
        _depthSource = null;
        _depthTexture = null;
        _graphicsDevice = null;
    }
}
