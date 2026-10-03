using SlavicGame.Engine.Diagnostics;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class ResolutionScalerRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private PixelFormat _colorFormat;
    private Texture? _colorTexture;
    private Texture? _depthTexture;
    private TextureView? _colorView;
    private Framebuffer? _sceneFramebuffer;
    private ResourceLayout? _layout;
    private ResourceSet? _set;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private bool _disposed;
    private uint _width;
    private uint _height;

    public Framebuffer SceneFramebuffer =>
        _sceneFramebuffer ??
        throw new InvalidOperationException("Resolution scaler is not initialized.");

    public uint Width => _width;
    public uint Height => _height;

    public void Initialize(
        GraphicsDevice graphicsDevice,
        OutputDescription swapchainOutput,
        uint width,
        uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (swapchainOutput.ColorAttachments.Length == 0)
            throw new InvalidOperationException("Swapchain has no color attachment.");

        _graphicsDevice = graphicsDevice;
        _colorFormat = swapchainOutput.ColorAttachments[0].Format;
        var factory = graphicsDevice.ResourceFactory;

        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "SceneColor",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "SceneSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment)));

        _shaders = ShaderLibrary.LoadPair(factory, "present");

        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                Array.Empty<VertexLayoutDescription>(),
                _shaders),
            [_layout],
            swapchainOutput));

        SetResolution(width, height);
    }

    public void SetResolution(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        width = Math.Clamp(width, 640u, 7680u);
        height = Math.Clamp(height, 360u, 4320u);

        if (_sceneFramebuffer is not null &&
            _width == width &&
            _height == height)
        {
            return;
        }

        if (_graphicsDevice is null || _layout is null)
            throw new InvalidOperationException("Resolution scaler is not initialized.");

        _graphicsDevice.WaitForIdle();

        _set?.Dispose();
        _sceneFramebuffer?.Dispose();
        _colorView?.Dispose();
        _depthTexture?.Dispose();
        _colorTexture?.Dispose();

        var factory = _graphicsDevice.ResourceFactory;

        _colorTexture = factory.CreateTexture(TextureDescription.Texture2D(
            width,
            height,
            mipLevels: 1,
            arrayLayers: 1,
            _colorFormat,
            TextureUsage.RenderTarget | TextureUsage.Sampled));

        _depthTexture = factory.CreateTexture(TextureDescription.Texture2D(
            width,
            height,
            mipLevels: 1,
            arrayLayers: 1,
            PixelFormat.R32_Float,
            TextureUsage.DepthStencil));

        _colorView = factory.CreateTextureView(_colorTexture);
        _sceneFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            new FramebufferAttachmentDescription(_depthTexture, 0),
            [new FramebufferAttachmentDescription(_colorTexture, 0)]));

        _set = factory.CreateResourceSet(new ResourceSetDescription(
            _layout,
            _colorView,
            _graphicsDevice.LinearSampler));

        _width = width;
        _height = height;

        EngineLog.Info($"Internal render resolution: {_width}x{_height}.");
    }

    public void Present(
        CommandList commandList,
        Framebuffer swapchainFramebuffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        ArgumentNullException.ThrowIfNull(swapchainFramebuffer);

        if (_pipeline is null || _set is null)
            throw new InvalidOperationException("Resolution scaler is not initialized.");

        commandList.SetFramebuffer(swapchainFramebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _set);
        commandList.Draw(3);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _graphicsDevice?.WaitForIdle();

        _pipeline?.Dispose();
        _set?.Dispose();
        _layout?.Dispose();
        _sceneFramebuffer?.Dispose();
        _colorView?.Dispose();
        _depthTexture?.Dispose();
        _colorTexture?.Dispose();

        if (_shaders is not null)
            foreach (var shader in _shaders)
                shader.Dispose();

        _pipeline = null;
        _set = null;
        _layout = null;
        _sceneFramebuffer = null;
        _colorView = null;
        _depthTexture = null;
        _colorTexture = null;
        _shaders = null;
        _graphicsDevice = null;
    }
}
