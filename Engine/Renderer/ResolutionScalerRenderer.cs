using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;
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

    private Texture? _fsrTexture;
    private TextureView? _fsrView;
    private Framebuffer? _fsrFramebuffer;

    private DeviceBuffer? _easuConstants;
    private DeviceBuffer? _rcasConstants;

    private ResourceLayout? _bilinearLayout;
    private ResourceLayout? _easuLayout;
    private ResourceLayout? _rcasLayout;

    private ResourceSet? _bilinearSet;
    private ResourceSet? _easuSet;
    private ResourceSet? _rcasSet;

    private Pipeline? _bilinearPipeline;
    private Pipeline? _easuPipeline;
    private Pipeline? _rcasPipeline;

    private Shader[]? _bilinearShaders;
    private Shader[]? _easuShaders;
    private Shader[]? _rcasShaders;

    private bool _disposed;
    private uint _width;
    private uint _height;
    private uint _outputWidth;
    private uint _outputHeight;

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
            64,
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

        _rcasPipeline = CreateFullscreenPipeline(
            factory,
            _rcasShaders,
            _rcasLayout,
            swapchainOutput);
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

        EngineLog.Info($"Internal render resolution: {_width}x{_height}.");
    }

    public void Present(
        CommandList commandList,
        Framebuffer swapchainFramebuffer,
        UpscalerMode upscaler,
        float fsrSharpness)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(commandList);
        ArgumentNullException.ThrowIfNull(swapchainFramebuffer);

        if (_graphicsDevice is null ||
            _bilinearPipeline is null ||
            _bilinearSet is null)
        {
            throw new InvalidOperationException("Resolution scaler is not initialized.");
        }

        var outputWidth = Math.Max(1u, swapchainFramebuffer.Width);
        var outputHeight = Math.Max(1u, swapchainFramebuffer.Height);

        if (upscaler == UpscalerMode.Fsr1)
        {
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
        if (_easuPipeline is null ||
            _easuSet is null ||
            _rcasPipeline is null ||
            _rcasSet is null ||
            _fsrFramebuffer is null ||
            _easuConstants is null ||
            _rcasConstants is null)
        {
            throw new InvalidOperationException("FSR1 resources are not initialized.");
        }

        var easu = BuildEasuConstants(
            _width,
            _height,
            outputWidth,
            outputHeight);
        var rcas = BuildRcasConstants(sharpness);

        commandList.UpdateBuffer(_easuConstants, 0, easu);
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

    private void RecreateSceneTarget(uint width, uint height)
    {
        if (_graphicsDevice is null ||
            _bilinearLayout is null ||
            _easuLayout is null ||
            _easuConstants is null)
        {
            throw new InvalidOperationException("Resolution scaler layouts are not initialized.");
        }

        var factory = _graphicsDevice.ResourceFactory;

        _bilinearSet?.Dispose();
        _easuSet?.Dispose();
        _sceneFramebuffer?.Dispose();
        _colorView?.Dispose();
        _depthTexture?.Dispose();
        _colorTexture?.Dispose();

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

        _bilinearSet = factory.CreateResourceSet(new ResourceSetDescription(
            _bilinearLayout,
            _colorView,
            _graphicsDevice.LinearSampler));

        _easuSet = factory.CreateResourceSet(new ResourceSetDescription(
            _easuLayout,
            _easuConstants,
            _colorView,
            _graphicsDevice.LinearSampler));

        _width = width;
        _height = height;
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
            0u
        ];
    }

    private static uint[] BuildRcasConstants(float sharpness)
    {
        sharpness = Math.Clamp(sharpness, 0f, 1f);

        // AMD RCAS expresses sharpness in stops: 0 = strongest.
        // Keep our UI intuitive: 0 = subtle, 1 = strongest.
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
        _colorView?.Dispose();
        _fsrView?.Dispose();
        _depthTexture?.Dispose();
        _colorTexture?.Dispose();
        _fsrTexture?.Dispose();

        DisposeShaders(_bilinearShaders);
        DisposeShaders(_easuShaders);
        DisposeShaders(_rcasShaders);

        _bilinearPipeline = null;
        _easuPipeline = null;
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
        _colorView = null;
        _fsrView = null;
        _depthTexture = null;
        _colorTexture = null;
        _fsrTexture = null;
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
