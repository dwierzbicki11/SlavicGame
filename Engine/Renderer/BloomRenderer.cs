using System.Numerics;
using System.Runtime.InteropServices;
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

    private readonly DeviceBuffer?[] _passBuffers = new DeviceBuffer?[5];
    private ResourceLayout? _layout;
    private readonly ResourceSet?[] _passSets = new ResourceSet?[5];
    private Pipeline? _pipeline;
    private Shader[]? _shaders;

    private uint _sourceWidth;
    private uint _sourceHeight;
    private uint _bloomWidth;
    private uint _bloomHeight;
    private int _downsampleFactor;
    private uint _lastParamSourceWidth;
    private uint _lastParamSourceHeight;
    private uint _lastParamBloomWidth;
    private uint _lastParamBloomHeight;
    private BloomQuality _lastParamQuality;
    private float _lastParamThreshold = float.NaN;
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
        for (var i = 0; i < _passBuffers.Length; i++)
            _passBuffers[i] = factory.CreateBuffer(new BufferDescription(32, BufferUsage.UniformBuffer));
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
        if (_graphicsDevice is null || _pipeline is null || _framebufferA is null || _framebufferB is null)
            throw new InvalidOperationException("Bloom renderer is not initialized.");
        EnsureTargets(quality);
        if (quality == BloomQuality.Off) return OutputView;

        var clampedThreshold = Math.Clamp(threshold, 0f, 4f);
        EnsurePassParameters(quality, clampedThreshold);

        Draw(commandList, _framebufferA, _passSets[0]!);

        // Low is intentionally a single-pass quarter-resolution bloom. The 4-tap
        // threshold/downsample already produces a soft highlight and avoiding two
        // extra fullscreen blur passes is important on fill-rate/bandwidth-limited iGPUs.
        if (quality == BloomQuality.Low) return OutputView;

        Draw(commandList, _framebufferB, _passSets[1]!);
        Draw(commandList, _framebufferA, _passSets[2]!);
        return OutputView;
    }

    private void Draw(CommandList commandList, Framebuffer framebuffer, ResourceSet resourceSet)
    {
        commandList.SetFramebuffer(framebuffer); commandList.SetFullViewports(); commandList.SetFullScissorRects();
        commandList.SetPipeline(_pipeline); commandList.SetGraphicsResourceSet(0, resourceSet); commandList.Draw(3);
    }

    private void EnsurePassParameters(BloomQuality quality, float threshold)
    {
        if (_graphicsDevice is null) return;
        if (_lastParamSourceWidth == _sourceWidth &&
            _lastParamSourceHeight == _sourceHeight &&
            _lastParamBloomWidth == _bloomWidth &&
            _lastParamBloomHeight == _bloomHeight &&
            _lastParamQuality == quality &&
            MathF.Abs(_lastParamThreshold - threshold) < 0.0001f)
        {
            return;
        }

        UpdatePassBuffer(0, 1f / _sourceWidth, 1f / _sourceHeight, threshold, 0f, 0f, 0f, 1f);
        var radius0 = quality == BloomQuality.High ? 2.8f : 2.25f;
        UpdatePassBuffer(1, 1f / _bloomWidth, 1f / _bloomHeight, 0f, 1f, 1f, 0f, radius0);
        UpdatePassBuffer(2, 1f / _bloomWidth, 1f / _bloomHeight, 0f, 1f, 0f, 1f, radius0);
        var radius1 = 3.6f;
        UpdatePassBuffer(3, 1f / _bloomWidth, 1f / _bloomHeight, 0f, 1f, 1f, 0f, radius1);
        UpdatePassBuffer(4, 1f / _bloomWidth, 1f / _bloomHeight, 0f, 1f, 0f, 1f, radius1);

        _lastParamSourceWidth = _sourceWidth;
        _lastParamSourceHeight = _sourceHeight;
        _lastParamBloomWidth = _bloomWidth;
        _lastParamBloomHeight = _bloomHeight;
        _lastParamQuality = quality;
        _lastParamThreshold = threshold;
    }

    private void UpdatePassBuffer(int passIndex, float texelX, float texelY, float threshold, float mode, float directionX, float directionY, float radius)
    {
        var buffer = _passBuffers[passIndex] ?? throw new InvalidOperationException("Bloom parameter buffer is missing.");
        var uniform = new BloomUniform(
            new Vector4(texelX, texelY, threshold, mode),
            new Vector4(directionX, directionY, radius, 0f));
        _graphicsDevice!.UpdateBuffer(buffer, 0, ref uniform);
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct BloomUniform(Vector4 thresholdAndTexel, Vector4 blurDirectionAndRadius)
    {
        public readonly Vector4 ThresholdAndTexel = thresholdAndTexel;
        public readonly Vector4 BlurDirectionAndRadius = blurDirectionAndRadius;
    }

    private void EnsureTargets(BloomQuality quality)
    {
        if (_graphicsDevice is null) throw new InvalidOperationException("Bloom renderer is not initialized.");
        var factor = 4;
        var width = Math.Max(1u, _sourceWidth / checked((uint)factor));
        var height = Math.Max(1u, _sourceHeight / checked((uint)factor));
        if (_textureA is not null && _bloomWidth == width && _bloomHeight == height && _downsampleFactor == factor) return;
        _graphicsDevice.WaitForIdle();
        DisposeSets();
        _framebufferA?.Dispose(); _framebufferB?.Dispose();
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
        if (_graphicsDevice is null || _layout is null || _passBuffers[0] is null || _sourceView is null || _viewA is null || _viewB is null) return;
        DisposeSets();
        var factory = _graphicsDevice.ResourceFactory;
        var sampler = _graphicsDevice.LinearSampler;
        _passSets[0] = factory.CreateResourceSet(new ResourceSetDescription(_layout, _passBuffers[0]!, _sourceView, sampler));
        _passSets[1] = factory.CreateResourceSet(new ResourceSetDescription(_layout, _passBuffers[1]!, _viewA, sampler));
        _passSets[2] = factory.CreateResourceSet(new ResourceSetDescription(_layout, _passBuffers[2]!, _viewB, sampler));
        _passSets[3] = factory.CreateResourceSet(new ResourceSetDescription(_layout, _passBuffers[3]!, _viewA, sampler));
        _passSets[4] = factory.CreateResourceSet(new ResourceSetDescription(_layout, _passBuffers[4]!, _viewB, sampler));
    }

    private void DisposeSets()
    {
        for (var i = 0; i < _passSets.Length; i++)
        {
            _passSets[i]?.Dispose();
            _passSets[i] = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return; _disposed = true;
        _pipeline?.Dispose();
        DisposeSets();
        _layout?.Dispose();
        for (var i = 0; i < _passBuffers.Length; i++)
        {
            _passBuffers[i]?.Dispose();
            _passBuffers[i] = null;
        }
        _framebufferA?.Dispose(); _framebufferB?.Dispose(); _viewA?.Dispose(); _viewB?.Dispose(); _textureA?.Dispose(); _textureB?.Dispose();
        if (_shaders is not null) foreach (var shader in _shaders) shader.Dispose();
        _pipeline = null; _layout = null;
        _framebufferA = null; _framebufferB = null; _viewA = null; _viewB = null; _textureA = null; _textureB = null; _shaders = null; _sourceView = null; _graphicsDevice = null;
    }
}
