using System.Numerics;
using SlavicGame.Engine.Settings;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class PostProcessRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private Texture? _targetTexture;
    private TextureView? _targetView;
    private Framebuffer? _framebuffer;
    private DeviceBuffer? _paramsBuffer;
    private ResourceLayout? _layout;
    private ResourceSet? _set;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;
    private TextureView? _sourceView;
    private TextureView? _bloomView;
    private uint _width;
    private uint _height;
    private bool _disposed;

    public TextureView OutputView => _targetView ?? throw new InvalidOperationException("Post-process renderer is not initialized.");

    public void Initialize(GraphicsDevice graphicsDevice, OutputDescription sceneOutput, TextureView sourceView, uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(sourceView);
        _graphicsDevice = graphicsDevice;
        var factory = graphicsDevice.ResourceFactory;
        _paramsBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("PostProcessParameters", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("SceneColor", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("SceneSampler", ResourceKind.Sampler, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("BloomColor", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));
        _shaders = ShaderLibrary.LoadPair(factory, "postprocess");
        RecreateTarget(width, height, sceneOutput.ColorAttachments[0].Format);
        RebindSources(sourceView, sourceView);
        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend, DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), _shaders), [_layout], _framebuffer!.OutputDescription));
    }

    public void SetSources(TextureView sourceView, TextureView bloomView, uint width, uint height, PixelFormat colorFormat)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(sourceView);
        ArgumentNullException.ThrowIfNull(bloomView);
        if (_width != width || _height != height) RecreateTarget(width, height, colorFormat);
        if (!ReferenceEquals(_sourceView, sourceView) || !ReferenceEquals(_bloomView, bloomView) || _set is null) RebindSources(sourceView, bloomView);
    }

    public bool IsNeeded(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        // MSAA is resolved by ResolutionScalerRenderer before this stage. Only FXAA
        // needs this shader; treating MSAA as post-processing caused a redundant
        // full-resolution texture read/write pass on every otherwise-default frame.
        return settings.AntiAliasing == AntiAliasingMode.Fxaa ||
               settings.Bloom != BloomQuality.Off ||
               MathF.Abs(settings.Brightness - 1f) > 0.001f ||
               MathF.Abs(settings.Gamma - 2.2f) > 0.001f;
    }

    public TextureView Render(CommandList commandList, GameSettings settings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        ArgumentNullException.ThrowIfNull(settings);
        if (_paramsBuffer is null || _framebuffer is null || _pipeline is null || _set is null) throw new InvalidOperationException("Post-process renderer is not initialized.");
        var texel = new Vector4(1f / Math.Max(1u, _width), 1f / Math.Max(1u, _height), settings.AntiAliasing == AntiAliasingMode.Fxaa ? 1f : 0f, 0f);
        var controls = new Vector4(settings.BloomStrength, 0.72f, settings.Brightness, settings.Gamma);
        commandList.UpdateBuffer(_paramsBuffer, 0, texel);
        commandList.UpdateBuffer(_paramsBuffer, 16, controls);
        commandList.SetFramebuffer(_framebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.SetPipeline(_pipeline);
        commandList.SetGraphicsResourceSet(0, _set);
        commandList.Draw(3);
        return OutputView;
    }

    private void RecreateTarget(uint width, uint height, PixelFormat colorFormat)
    {
        if (_graphicsDevice is null) throw new InvalidOperationException("Post-process renderer is not initialized.");
        _set?.Dispose(); _set = null; _framebuffer?.Dispose(); _targetView?.Dispose(); _targetTexture?.Dispose();
        var factory = _graphicsDevice.ResourceFactory;
        _targetTexture = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, colorFormat, TextureUsage.RenderTarget | TextureUsage.Sampled));
        _targetView = factory.CreateTextureView(_targetTexture);
        _framebuffer = factory.CreateFramebuffer(new FramebufferDescription(null, [new FramebufferAttachmentDescription(_targetTexture, 0)]));
        _width = width; _height = height;
    }

    private void RebindSources(TextureView sourceView, TextureView bloomView)
    {
        if (_graphicsDevice is null || _layout is null || _paramsBuffer is null) throw new InvalidOperationException("Post-process renderer is not initialized.");
        _set?.Dispose();
        _set = _graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_layout, _paramsBuffer, sourceView, _graphicsDevice.LinearSampler, bloomView));
        _sourceView = sourceView; _bloomView = bloomView;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pipeline?.Dispose(); _set?.Dispose(); _layout?.Dispose(); _paramsBuffer?.Dispose(); _framebuffer?.Dispose(); _targetView?.Dispose(); _targetTexture?.Dispose();
        if (_shaders is not null) foreach (var shader in _shaders) shader.Dispose();
        _pipeline = null; _set = null; _layout = null; _paramsBuffer = null; _framebuffer = null; _targetView = null; _targetTexture = null; _shaders = null; _sourceView = null; _bloomView = null; _graphicsDevice = null;
    }
}
