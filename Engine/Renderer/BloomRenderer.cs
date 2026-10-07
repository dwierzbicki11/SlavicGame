using System.Numerics;
using SlavicGame.Engine.Settings;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class BloomRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private PixelFormat _colorFormat;
    private TextureView? _sourceView;

    private Texture? _textureA;
    private Texture? _textureB;
    private TextureView? _viewA;
    private TextureView? _viewB;
    private Framebuffer? _framebufferA;
    private Framebuffer? _framebufferB;

    private DeviceBuffer? _paramsBuffer;
    private ResourceLayout? _layout;
    private ResourceSet? _sourceSet;
    private ResourceSet? _setA;
    private ResourceSet? _setB;
    private Pipeline? _pipeline;
    private Shader[]? _shaders;

    private uint _sourceWidth;
    private uint _sourceHeight;
    private uint _bloomWidth;
    private uint _bloomHeight;
    private int _downsampleFactor;
    private bool _disposed;

    public TextureView OutputView =>
        _viewA ?? throw new InvalidOperationException(
            "Bloom renderer is not initialized.");

    public void Initialize(GraphicsDevice graphicsDevice, OutputDescription sceneOutput, TextureView sourceView, uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(sourceView);
        if (sceneOutput.ColorAttachments.Length == 0) throw new InvalidOperationException("Scene output has no color attachment.");
        _graphicsDevice = graphicsDevice;
        _colorFormat = sceneOutput.ColorAttachments[0].Format;
        var factory = graphicsDevice.ResourceFactory;
        _paramsBuffer = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("BloomParameters", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("SourceColor", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("BloomSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
        _shaders = ShaderLibrary.LoadPair(factory, "bloom");
        SetSource(sourceView, width, height, BloomQuality.Medium);
        var preserveTextureRows =
            graphicsDevice.BackendType == GraphicsBackend.Vulkan &&
            !graphicsDevice.IsClipSpaceYInverted;
        _pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
            BlendStateDescription.SingleOverrideBlend, DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
            PrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                Array.Empty<VertexLayoutDescription>(),
                _shaders,
                [new SpecializationConstant(0, preserveTextureRows ? 1u : 0u)]),
            [_layout], _framebufferA!.OutputDescription));
    }

    public void SetSource(TextureView sourceView, uint width, uint height, BloomQuality quality)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(sourceView);
        _sourceView = sourceView;
        _sourceWidth = Math.Max(1u, width);
        _sourceHeight = Math.Max(1u, height);
        EnsureTargets(quality);
        RebindSets();
    }

    public TextureView Render(CommandList commandList, BloomQuality quality, float threshold)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        if (_graphicsDevice is null || _paramsBuffer is null || _pipeline is null || _sourceSet is null || _setA is null || _setB is null || _framebufferA is null || _framebufferB is null)
            throw new InvalidOperationException("Bloom renderer is not initialized.");
        EnsureTargets(quality);
        if (quality == BloomQuality.Off) return OutputView;

        WriteParameters(commandList, 1f / _sourceWidth, 1f / _sourceHeight, Math.Clamp(threshold, 0f, 4f), 0f, 0f, 0f, 1f);
        Draw(commandList, _framebufferA, _sourceSet);

        // Low is intentionally a single-pass quarter-resolution bloom. The 4-tap
        // threshold/downsample already produces a soft highlight and avoiding two
        // extra fullscreen blur passes is important on fill-rate/bandwidth-limited iGPUs.
        if (quality == BloomQuality.Low) return OutputView;

        var iterations = quality == BloomQuality.High ? 2 : 1;
        for (var iteration = 0; iteration < iterations; iteration++)
        {
            var radius = quality == BloomQuality.High ? 2.8f + iteration * 0.8f : 2.25f;
            WriteParameters(commandList, 1f / _bloomWidth, 1f / _bloomHeight, 0f, 1f, 1f, 0f, radius);
            Draw(commandList, _framebufferB, _setA);
            WriteParameters(commandList, 1f / _bloomWidth, 1f / _bloomHeight, 0f, 1f, 0f, 1f, radius);
            Draw(commandList, _framebufferA, _setB);
        }
        return OutputView;
    }

    private void Draw(CommandList commandList, Framebuffer framebuffer, ResourceSet resourceSet)
    {
        commandList.SetFramebuffer(framebuffer); commandList.SetFullViewports(); commandList.SetFullScissorRects();
        commandList.SetPipeline(_pipeline); commandList.SetGraphicsResourceSet(0, resourceSet); commandList.Draw(3);
    }

    private void WriteParameters(CommandList commandList, float texelX, float texelY, float threshold, float mode, float directionX, float directionY, float radius)
    {
        if (_paramsBuffer is null) throw new InvalidOperationException("Bloom parameter buffer is missing.");
        commandList.UpdateBuffer(_paramsBuffer, 0, new Vector4(texelX, texelY, threshold, mode));
        commandList.UpdateBuffer(_paramsBuffer, 16, new Vector4(directionX, directionY, radius, 0f));
    }

    private void EnsureTargets(BloomQuality quality)
    {
        if (_graphicsDevice is null) throw new InvalidOperationException("Bloom renderer is not initialized.");
        var factor = quality switch { BloomQuality.Low => 4, BloomQuality.Medium => 2, BloomQuality.High => 2, _ => 4 };
        var width = Math.Max(1u, _sourceWidth / checked((uint)factor));
        var height = Math.Max(1u, _sourceHeight / checked((uint)factor));
        if (_textureA is not null && _bloomWidth == width && _bloomHeight == height && _downsampleFactor == factor) return;
        _graphicsDevice.WaitForIdle();
        _sourceSet?.Dispose(); _setA?.Dispose(); _setB?.Dispose(); _framebufferA?.Dispose(); _framebufferB?.Dispose();
        _viewA?.Dispose(); _viewB?.Dispose(); _textureA?.Dispose(); _textureB?.Dispose();
        var factory = _graphicsDevice.ResourceFactory;
        _textureA = CreateTarget(factory, width, height); _textureB = CreateTarget(factory, width, height);
        _viewA = factory.CreateTextureView(_textureA); _viewB = factory.CreateTextureView(_textureB);
        _framebufferA = factory.CreateFramebuffer(new FramebufferDescription(null, [new FramebufferAttachmentDescription(_textureA, 0)]));
        _framebufferB = factory.CreateFramebuffer(new FramebufferDescription(null, [new FramebufferAttachmentDescription(_textureB, 0)]));
        _bloomWidth = width; _bloomHeight = height; _downsampleFactor = factor; RebindSets();
    }

    private Texture CreateTarget(ResourceFactory factory, uint width, uint height) => factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, _colorFormat, TextureUsage.RenderTarget | TextureUsage.Sampled));

    private void RebindSets()
    {
        if (_graphicsDevice is null || _layout is null || _paramsBuffer is null || _sourceView is null || _viewA is null || _viewB is null) return;
        _sourceSet?.Dispose(); _setA?.Dispose(); _setB?.Dispose();
        var factory = _graphicsDevice.ResourceFactory;
        _sourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout, _paramsBuffer, _sourceView, _graphicsDevice.LinearSampler));
        _setA = factory.CreateResourceSet(new ResourceSetDescription(_layout, _paramsBuffer, _viewA, _graphicsDevice.LinearSampler));
        _setB = factory.CreateResourceSet(new ResourceSetDescription(_layout, _paramsBuffer, _viewB, _graphicsDevice.LinearSampler));
    }

    public void Dispose()
    {
        if (_disposed) return; _disposed = true;
        _pipeline?.Dispose(); _sourceSet?.Dispose(); _setA?.Dispose(); _setB?.Dispose(); _layout?.Dispose(); _paramsBuffer?.Dispose();
        _framebufferA?.Dispose(); _framebufferB?.Dispose(); _viewA?.Dispose(); _viewB?.Dispose(); _textureA?.Dispose(); _textureB?.Dispose();
        if (_shaders is not null) foreach (var shader in _shaders) shader.Dispose();
        _pipeline = null; _sourceSet = null; _setA = null; _setB = null; _layout = null; _paramsBuffer = null;
        _framebufferA = null; _framebufferB = null; _viewA = null; _viewB = null; _textureA = null; _textureB = null; _shaders = null; _sourceView = null; _graphicsDevice = null;
    }
}
