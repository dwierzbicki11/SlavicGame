using System.Numerics;
using SlavicGame.Engine.Diagnostics;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

/// <summary>
/// Produces normalized screen-space camera motion from the scene depth buffer.
/// This is the first temporal-input stage for FSR 2/3. Dynamic per-object
/// velocity can be layered on top later; camera motion is available now for
/// terrain and all static world geometry without adding MRT bandwidth to the
/// main scene pass.
/// </summary>
public sealed class MotionVectorRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private Texture? _motionTexture;
    private TextureView? _motionView;
    private Framebuffer? _framebuffer;
    private DeviceBuffer? _constants;
    private ResourceLayout? _layout;
    private ResourceSet? _set;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private TextureView? _depthSource;
    private uint _width;
    private uint _height;
    private bool _disposed;

    public bool IsInitialized => _pipeline is not null;

    public TextureView MotionVectorView =>
        _motionView ?? throw new InvalidOperationException(
            "Motion-vector renderer is not initialized.");

    public void Initialize(
        GraphicsDevice graphicsDevice,
        TextureView depthSource,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(depthSource);

        _graphicsDevice = graphicsDevice;
        _depthSource = depthSource;
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

        _shaders = ShaderLibrary.LoadPair(factory, "motion_vectors");
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

        Rebind();

        EngineLog.Info(
            $"Temporal camera-motion target initialized: {_width}x{_height} RG16F.");
    }

    public void SetSource(
        TextureView depthSource,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(depthSource);
        if (_graphicsDevice is null)
            throw new InvalidOperationException(
                "Motion-vector renderer is not initialized.");

        _depthSource = depthSource;
        width = Math.Max(1u, width);
        height = Math.Max(1u, height);
        if (_width != width || _height != height)
        {
            _graphicsDevice.WaitForIdle();
            RecreateTarget(width, height);
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

    private void RecreateTarget(uint width, uint height)
    {
        if (_graphicsDevice is null)
            throw new InvalidOperationException(
                "Motion-vector renderer is not initialized.");

        width = Math.Max(1u, width);
        height = Math.Max(1u, height);

        _set?.Dispose();
        _set = null;
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

        _pipeline?.Dispose();
        _set?.Dispose();
        _layout?.Dispose();
        _constants?.Dispose();
        _framebuffer?.Dispose();
        _motionView?.Dispose();
        _motionTexture?.Dispose();
        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _pipeline = null;
        _set = null;
        _layout = null;
        _constants = null;
        _framebuffer = null;
        _motionView = null;
        _motionTexture = null;
        _shaders = null;
        _depthSource = null;
        _graphicsDevice = null;
    }
}
