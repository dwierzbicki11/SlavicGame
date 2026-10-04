using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

public sealed class ResolutionScalerRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private PixelFormat _colorFormat;
    private TextureSampleCount _sceneSampleCount = TextureSampleCount.Count1;

    private Texture? _colorTexture;
    private Texture? _resolvedColorTexture;
    private Texture? _depthTexture;
    private TextureView? _depthView;
    private TextureView? _resolvedColorView;
    private Framebuffer? _sceneFramebuffer;

    private Texture? _fsrTexture;
    private TextureView? _fsrView;
    private Framebuffer? _fsrFramebuffer;
    private Texture? _fsr3OutputTexture;
    private TextureView? _fsr3OutputView;

    private DeviceBuffer? _easuConstants;
    private DeviceBuffer? _rcasConstants;

    private ResourceLayout? _bilinearLayout;
    private ResourceLayout? _easuLayout;
    private ResourceLayout? _rcasLayout;

    private ResourceSet? _bilinearSet;
    private ResourceSet? _easuSet;
    private ResourceSet? _rcasSet;
    private TextureView? _presentationSource;

    private Pipeline? _bilinearPipeline;
    private Pipeline? _easuPipeline;
    private Pipeline? _easuSwapchainPipeline;
    private Pipeline? _rcasPipeline;

    private Shader[]? _bilinearShaders;
    private Shader[]? _easuShaders;
    private Shader[]? _rcasShaders;

    private bool _disposed;
    private bool _singlePassEasuCompatibility;
    private uint _width;
    private uint _height;
    private uint _outputWidth;
    private uint _outputHeight;
    private uint _fsr3OutputWidth;
    private uint _fsr3OutputHeight;

    public Framebuffer SceneFramebuffer =>
        _sceneFramebuffer ??
        throw new InvalidOperationException("Resolution scaler is not initialized.");

    public TextureView ResolvedSceneView =>
        _resolvedColorView ??
        throw new InvalidOperationException("Resolution scaler is not initialized.");

    public TextureView? SampleableDepthView =>
        _sceneSampleCount == TextureSampleCount.Count1
            ? _depthView
            : null;

    public Texture? SampleableDepthTexture =>
        _sceneSampleCount == TextureSampleCount.Count1
            ? _depthTexture
            : null;

    public Texture ResolvedSceneTexture =>
        _resolvedColorTexture ??
        throw new InvalidOperationException("Resolution scaler is not initialized.");

    public Texture Fsr3OutputTexture =>
        _fsr3OutputTexture ??
        throw new InvalidOperationException("FSR3 output is not initialized.");

    public TextureView Fsr3OutputView =>
        _fsr3OutputView ??
        throw new InvalidOperationException("FSR3 output is not initialized.");

    public uint Width => _width;
    public uint Height => _height;
    public TextureSampleCount SceneSampleCount => _sceneSampleCount;

    public void Initialize(
        GraphicsDevice graphicsDevice,
        OutputDescription swapchainOutput,
        uint width,
        uint height,
        MsaaQuality msaa)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        if (swapchainOutput.ColorAttachments.Length == 0)
            throw new InvalidOperationException("Swapchain has no color attachment.");

        _graphicsDevice = graphicsDevice;
        _colorFormat = swapchainOutput.ColorAttachments[0].Format;
        _sceneSampleCount = SelectSupportedSampleCount(
            graphicsDevice,
            GraphicsQualityCatalog.MsaaSamples(msaa));
        _singlePassEasuCompatibility =
            FsrPresentationPolicy.UsesSinglePassEasuCompatibility(
                graphicsDevice.BackendType == GraphicsBackend.Vulkan);

        var factory = graphicsDevice.ResourceFactory;

        _bilinearLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "SceneColor",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "SceneSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment)));

        _easuLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "FsrEasuConstants",
                ResourceKind.UniformBuffer,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "SceneColor",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "SceneSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment)));

        _rcasLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "FsrRcasConstants",
                ResourceKind.UniformBuffer,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "EasuedColor",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment),
            new ResourceLayoutElementDescription(
                "EasuedSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment)));

        _easuConstants = factory.CreateBuffer(new BufferDescription(
            80,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _rcasConstants = factory.CreateBuffer(new BufferDescription(
            16,
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        _bilinearShaders = ShaderLibrary.LoadPair(factory, "present");
        _easuShaders = ShaderLibrary.LoadPair(factory, "fsr_easu");
        _rcasShaders = ShaderLibrary.LoadPair(factory, "fsr_rcas");

        RecreateSceneTarget(width, height);
        RecreateFsrTarget(width, height);

        _bilinearPipeline = CreateFullscreenPipeline(
            factory,
            _bilinearShaders,
            _bilinearLayout,
            swapchainOutput);

        _easuPipeline = CreateFullscreenPipeline(
            factory,
            _easuShaders,
            _easuLayout,
            _fsrFramebuffer!.OutputDescription);

        _easuSwapchainPipeline = CreateFullscreenPipeline(
            factory,
            _easuShaders,
            _easuLayout,
            swapchainOutput);

        _rcasPipeline = CreateFullscreenPipeline(
            factory,
            _rcasShaders,
            _rcasLayout,
            swapchainOutput);

        SetPresentationSource(ResolvedSceneView);

        EngineLog.Info(
            $"Scene MSAA: requested={GraphicsQualityCatalog.MsaaSamples(msaa)}x, " +
            $"active={(int)_sceneSampleCount}x.");

        EngineLog.Info(
            $"Vulkan clip/UV state: clipYInverted={graphicsDevice.IsClipSpaceYInverted}, " +
            $"uvOriginTopLeft={graphicsDevice.IsUvOriginTopLeft}, " +
            $"FSR single-pass EASU={_singlePassEasuCompatibility}.");
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

        if (_graphicsDevice is null)
            throw new InvalidOperationException("Resolution scaler is not initialized.");

        _graphicsDevice.WaitForIdle();
        RecreateSceneTarget(width, height);
        SetPresentationSource(ResolvedSceneView);

        EngineLog.Info($"Internal render resolution: {_width}x{_height}.");
    }

    public void ResolveScene(CommandList commandList)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);

        if (_sceneSampleCount == TextureSampleCount.Count1)
            return;

        if (_colorTexture is null || _resolvedColorTexture is null)
            throw new InvalidOperationException("MSAA resolve resources are missing.");

        commandList.ResolveTexture(_colorTexture, _resolvedColorTexture);
    }

    public void Present(
        CommandList commandList,
        Framebuffer swapchainFramebuffer,
        UpscalerMode upscaler,
        float fsrSharpness,
        TextureView? sourceOverride = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        ArgumentNullException.ThrowIfNull(swapchainFramebuffer);

        var source = sourceOverride ?? ResolvedSceneView;
        SetPresentationSource(source);

        // Vulkan fullscreen passes use one canonical UV orientation. Do not
        // inject manual Y flips based on MSAA, preset or upscaler state.
        // Ultra already proved that the unflipped path is the correct one;
        // all lower presets and FSR now follow the same convention.

        if (_graphicsDevice is null ||
            _bilinearPipeline is null ||
            _bilinearSet is null)
        {
            throw new InvalidOperationException("Resolution scaler is not initialized.");
        }

        var outputWidth = Math.Max(1u, swapchainFramebuffer.Width);
        var outputHeight = Math.Max(1u, swapchainFramebuffer.Height);

        if (FsrPresentationPolicy.UsesUpscalePass(
                upscaler,
                _width,
                _height,
                outputWidth,
                outputHeight))
        {
            if (!_singlePassEasuCompatibility)
                EnsureFsrTarget(outputWidth, outputHeight);

            PresentFsr1(
                commandList,
                swapchainFramebuffer,
                outputWidth,
                outputHeight,
                fsrSharpness);
            return;
        }

        commandList.SetFramebuffer(swapchainFramebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.SetPipeline(_bilinearPipeline);
        commandList.SetGraphicsResourceSet(0, _bilinearSet);
        commandList.Draw(3);
    }

    private void PresentFsr1(
        CommandList commandList,
        Framebuffer swapchainFramebuffer,
        uint outputWidth,
        uint outputHeight,
        float sharpness)
    {
        if (_easuSet is null ||
            _easuConstants is null)
        {
            throw new InvalidOperationException("FSR1 EASU resources are not initialized.");
        }

        var easu = BuildEasuConstants(
            _width,
            _height,
            outputWidth,
            outputHeight);
        commandList.UpdateBuffer(_easuConstants, 0, easu);

        if (_singlePassEasuCompatibility)
        {
            if (_easuSwapchainPipeline is null)
                throw new InvalidOperationException("FSR1 direct EASU pipeline is not initialized.");

            commandList.SetFramebuffer(swapchainFramebuffer);
            commandList.SetFullViewports();
            commandList.SetFullScissorRects();
            commandList.SetPipeline(_easuSwapchainPipeline);
            commandList.SetGraphicsResourceSet(0, _easuSet);
            commandList.Draw(3);
            return;
        }

        if (_easuPipeline is null ||
            _rcasPipeline is null ||
            _rcasSet is null ||
            _fsrFramebuffer is null ||
            _rcasConstants is null)
        {
            throw new InvalidOperationException("FSR1 RCAS resources are not initialized.");
        }

        var rcas = BuildRcasConstants(sharpness);
        commandList.UpdateBuffer(_rcasConstants, 0, rcas);

        commandList.SetFramebuffer(_fsrFramebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.SetPipeline(_easuPipeline);
        commandList.SetGraphicsResourceSet(0, _easuSet);
        commandList.Draw(3);

        commandList.SetFramebuffer(swapchainFramebuffer);
        commandList.SetFullViewports();
        commandList.SetFullScissorRects();
        commandList.SetPipeline(_rcasPipeline);
        commandList.SetGraphicsResourceSet(0, _rcasSet);
        commandList.Draw(3);
    }

    private void SetPresentationSource(TextureView source)
    {
        if (_graphicsDevice is null ||
            _bilinearLayout is null ||
            _easuLayout is null ||
            _easuConstants is null)
        {
            throw new InvalidOperationException("Resolution scaler layouts are not initialized.");
        }

        if (ReferenceEquals(_presentationSource, source) &&
            _bilinearSet is not null &&
            _easuSet is not null)
        {
            return;
        }

        _bilinearSet?.Dispose();
        _easuSet?.Dispose();

        var factory = _graphicsDevice.ResourceFactory;
        _bilinearSet = factory.CreateResourceSet(new ResourceSetDescription(
            _bilinearLayout,
            source,
            _graphicsDevice.LinearSampler));

        _easuSet = factory.CreateResourceSet(new ResourceSetDescription(
            _easuLayout,
            _easuConstants,
            source,
            _graphicsDevice.LinearSampler));

        _presentationSource = source;
    }

    private void RecreateSceneTarget(uint width, uint height)
    {
        if (_graphicsDevice is null)
            throw new InvalidOperationException("Resolution scaler is not initialized.");

        var factory = _graphicsDevice.ResourceFactory;

        _bilinearSet?.Dispose();
        _easuSet?.Dispose();
        _bilinearSet = null;
        _easuSet = null;
        _presentationSource = null;

        _sceneFramebuffer?.Dispose();
        _resolvedColorView?.Dispose();
        if (_resolvedColorTexture is not null &&
            !ReferenceEquals(_resolvedColorTexture, _colorTexture))
        {
            _resolvedColorTexture.Dispose();
        }
        _depthView?.Dispose();
        _depthTexture?.Dispose();
        _colorTexture?.Dispose();

        var multisampled = _sceneSampleCount != TextureSampleCount.Count1;

        _colorTexture = factory.CreateTexture(TextureDescription.Texture2D(
            width,
            height,
            mipLevels: 1,
            arrayLayers: 1,
            _colorFormat,
            TextureUsage.RenderTarget,
            _sceneSampleCount));

        var depthUsage = TextureUsage.DepthStencil;
        if (_sceneSampleCount == TextureSampleCount.Count1)
            depthUsage |= TextureUsage.Sampled;

        _depthTexture = factory.CreateTexture(TextureDescription.Texture2D(
            width,
            height,
            mipLevels: 1,
            arrayLayers: 1,
            PixelFormat.R32_Float,
            depthUsage,
            _sceneSampleCount));
        _depthView = _sceneSampleCount == TextureSampleCount.Count1
            ? factory.CreateTextureView(_depthTexture)
            : null;

        if (multisampled)
        {
            _resolvedColorTexture = factory.CreateTexture(TextureDescription.Texture2D(
                width,
                height,
                mipLevels: 1,
                arrayLayers: 1,
                _colorFormat,
                TextureUsage.RenderTarget | TextureUsage.Sampled));
        }
        else
        {
            // Single-sample target is directly sampleable after the scene pass.
            _colorTexture.Dispose();
            _colorTexture = factory.CreateTexture(TextureDescription.Texture2D(
                width,
                height,
                mipLevels: 1,
                arrayLayers: 1,
                _colorFormat,
                TextureUsage.RenderTarget | TextureUsage.Sampled));
            _resolvedColorTexture = _colorTexture;
        }

        _resolvedColorView = factory.CreateTextureView(_resolvedColorTexture);
        _sceneFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            new FramebufferAttachmentDescription(_depthTexture, 0),
            [new FramebufferAttachmentDescription(_colorTexture, 0)]));

        _width = width;
        _height = height;
    }

    public void EnsureFsr3Output(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        width = Math.Max(1u, width);
        height = Math.Max(1u, height);

        if (_fsr3OutputTexture is not null &&
            _fsr3OutputWidth == width &&
            _fsr3OutputHeight == height)
        {
            return;
        }

        if (_graphicsDevice is null)
            throw new InvalidOperationException("Resolution scaler is not initialized.");

        _graphicsDevice.WaitForIdle();
        _fsr3OutputView?.Dispose();
        _fsr3OutputTexture?.Dispose();

        _fsr3OutputTexture =
            _graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    mipLevels: 1,
                    arrayLayers: 1,
                    PixelFormat.R16_G16_B16_A16_Float,
                    TextureUsage.Sampled | TextureUsage.Storage));
        _fsr3OutputView =
            _graphicsDevice.ResourceFactory.CreateTextureView(
                _fsr3OutputTexture);
        _fsr3OutputWidth = width;
        _fsr3OutputHeight = height;

        EngineLog.Info(
            $"FSR3 native output target: {width}x{height} RGBA16F.");
    }

    private void EnsureFsrTarget(uint width, uint height)
    {
        if (_fsrFramebuffer is not null &&
            _outputWidth == width &&
            _outputHeight == height)
        {
            return;
        }

        if (_graphicsDevice is null)
            throw new InvalidOperationException("Resolution scaler is not initialized.");

        _graphicsDevice.WaitForIdle();
        RecreateFsrTarget(width, height);
    }

    private void RecreateFsrTarget(uint width, uint height)
    {
        if (_graphicsDevice is null ||
            _rcasLayout is null ||
            _rcasConstants is null)
        {
            throw new InvalidOperationException("FSR1 layouts are not initialized.");
        }

        var factory = _graphicsDevice.ResourceFactory;

        _rcasSet?.Dispose();
        _fsrFramebuffer?.Dispose();
        _fsrView?.Dispose();
        _fsrTexture?.Dispose();

        _fsrTexture = factory.CreateTexture(TextureDescription.Texture2D(
            width,
            height,
            mipLevels: 1,
            arrayLayers: 1,
            _colorFormat,
            TextureUsage.RenderTarget | TextureUsage.Sampled));
        _fsrView = factory.CreateTextureView(_fsrTexture);

        _fsrFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
            null,
            [new FramebufferAttachmentDescription(_fsrTexture, 0)]));

        _rcasSet = factory.CreateResourceSet(new ResourceSetDescription(
            _rcasLayout,
            _rcasConstants,
            _fsrView,
            _graphicsDevice.PointSampler));

        _outputWidth = width;
        _outputHeight = height;
    }

    private TextureSampleCount SelectSupportedSampleCount(
        GraphicsDevice graphicsDevice,
        int requested)
    {
        graphicsDevice.GetPixelFormatSupport(
            _colorFormat,
            TextureType.Texture2D,
            TextureUsage.RenderTarget,
            out var colorSupport);
        graphicsDevice.GetPixelFormatSupport(
            PixelFormat.R32_Float,
            TextureType.Texture2D,
            TextureUsage.DepthStencil,
            out var depthSupport);

        foreach (var candidate in Candidates(requested))
        {
            if (colorSupport.IsSampleCountSupported(candidate) &&
                depthSupport.IsSampleCountSupported(candidate))
            {
                return candidate;
            }
        }

        return TextureSampleCount.Count1;
    }

    private static IEnumerable<TextureSampleCount> Candidates(int requested)
    {
        if (requested >= 4)
            yield return TextureSampleCount.Count4;
        if (requested >= 2)
            yield return TextureSampleCount.Count2;
        yield return TextureSampleCount.Count1;
    }

    private static Pipeline CreateFullscreenPipeline(
        ResourceFactory factory,
        Shader[] shaders,
        ResourceLayout layout,
        OutputDescription output)
    {
        return factory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
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
                shaders),
            [layout],
            output));
    }

    private static uint[] BuildEasuConstants(
        uint inputWidth,
        uint inputHeight,
        uint outputWidth,
        uint outputHeight)
    {
        var iw = Math.Max(1f, inputWidth);
        var ih = Math.Max(1f, inputHeight);
        var ow = Math.Max(1f, outputWidth);
        var oh = Math.Max(1f, outputHeight);

        var sx = iw / ow;
        var sy = ih / oh;
        var rcpIw = 1f / iw;
        var rcpIh = 1f / ih;

        return
        [
            Bits(sx),
            Bits(sy),
            Bits(0.5f * sx - 0.5f),
            Bits(0.5f * sy - 0.5f),

            Bits(rcpIw),
            Bits(rcpIh),
            Bits(rcpIw),
            Bits(-rcpIh),

            Bits(-rcpIw),
            Bits(2f * rcpIh),
            Bits(rcpIw),
            Bits(2f * rcpIh),

            Bits(0f),
            Bits(4f * rcpIh),
            0u,
            0u,

            outputWidth,
            outputHeight,
            0u,
            0u
        ];
    }

    private static uint[] BuildRcasConstants(float sharpness)
    {
        sharpness = Math.Clamp(sharpness, 0f, 1f);

        var stops = 4f * (1f - sharpness);
        var linear = MathF.Pow(2f, -stops);

        return [Bits(linear), 0u, 0u, 0u];
    }

    private static uint Bits(float value) =>
        BitConverter.SingleToUInt32Bits(value);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _graphicsDevice?.WaitForIdle();

        _bilinearPipeline?.Dispose();
        _easuPipeline?.Dispose();
        _easuSwapchainPipeline?.Dispose();
        _rcasPipeline?.Dispose();

        _bilinearSet?.Dispose();
        _easuSet?.Dispose();
        _rcasSet?.Dispose();

        _bilinearLayout?.Dispose();
        _easuLayout?.Dispose();
        _rcasLayout?.Dispose();

        _easuConstants?.Dispose();
        _rcasConstants?.Dispose();

        _sceneFramebuffer?.Dispose();
        _fsrFramebuffer?.Dispose();
        _resolvedColorView?.Dispose();
        _fsrView?.Dispose();
        _depthView?.Dispose();
        _depthTexture?.Dispose();

        if (_resolvedColorTexture is not null &&
            !ReferenceEquals(_resolvedColorTexture, _colorTexture))
        {
            _resolvedColorTexture.Dispose();
        }

        _colorTexture?.Dispose();
        _fsrTexture?.Dispose();
        _fsr3OutputView?.Dispose();
        _fsr3OutputTexture?.Dispose();

        DisposeShaders(_bilinearShaders);
        DisposeShaders(_easuShaders);
        DisposeShaders(_rcasShaders);

        _bilinearPipeline = null;
        _easuPipeline = null;
        _easuSwapchainPipeline = null;
        _rcasPipeline = null;
        _bilinearSet = null;
        _easuSet = null;
        _rcasSet = null;
        _bilinearLayout = null;
        _easuLayout = null;
        _rcasLayout = null;
        _easuConstants = null;
        _rcasConstants = null;
        _sceneFramebuffer = null;
        _fsrFramebuffer = null;
        _resolvedColorView = null;
        _fsrView = null;
        _depthTexture = null;
        _colorTexture = null;
        _resolvedColorTexture = null;
        _fsrTexture = null;
        _fsr3OutputView = null;
        _fsr3OutputTexture = null;
        _presentationSource = null;
        _bilinearShaders = null;
        _easuShaders = null;
        _rcasShaders = null;
        _graphicsDevice = null;
    }

    private static void DisposeShaders(Shader[]? shaders)
    {
        if (shaders is null) return;
        foreach (var shader in shaders)
            shader.Dispose();
    }
}
