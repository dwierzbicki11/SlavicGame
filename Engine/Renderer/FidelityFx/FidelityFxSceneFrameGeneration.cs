using Veldrid;

namespace SlavicGame.Engine.Renderer.FidelityFx;

/// <summary>
/// Owns the display-sized, HUD-free scene and generated image. All producer,
/// layout, AMD and consumer submissions use Veldrid's graphics queue. The
/// output stays alive until resize/cleanup waits for its consumers as well as
/// the generator. This adapter does not acquire or present swapchain images.
/// </summary>
internal sealed class FidelityFxSceneFrameGeneration : IDisposable
{
    internal const string SceneValidationVariable = "SLAVICGAME_FSR3_FG_SCENE";
    private readonly GraphicsDevice _device;
    private readonly FidelityFxFrameGeneration _generator;
    private Texture? _color;
    private TextureView? _colorView;
    private Framebuffer? _framebuffer;
    private Texture? _output;
    private TextureView? _outputView;
    private bool _disposed;

    private FidelityFxSceneFrameGeneration(GraphicsDevice device, FidelityFxFrameGeneration generator)
    {
        _device = device;
        _generator = generator;
    }

    internal static bool IsSceneValidationRequested() =>
        Environment.GetEnvironmentVariable(SceneValidationVariable) is "1" or "true";

    internal static bool TryCreate(GraphicsDevice device,
        out FidelityFxSceneFrameGeneration? scene, out string diagnostic)
    {
        scene = null;
        if (!FidelityFxFrameGeneration.TryCreate(device, out var generator, out diagnostic)) return false;
        scene = new(device, generator!);
        return true;
    }

    internal Framebuffer HudlessFramebuffer => _framebuffer
        ?? throw new InvalidOperationException("FG scene target has not been allocated.");
    internal Texture HudlessColor => _color!;
    internal TextureView HudlessView => _colorView!;
    internal Texture GeneratedColor => _output!;
    internal TextureView GeneratedView => _outputView!;
    internal bool HasGeneratedFrame { get; private set; }
    internal ulong LastFrameId { get; private set; }
    internal ulong DispatchCount { get; private set; }
    internal ulong GeneratedCount { get; private set; }
    internal bool LastDispatchUsedExternalColor { get; private set; }

    internal void EnsureDisplaySize(uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (width == 0 || height == 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (_color is { } color && color.Width == width && color.Height == height) return;

        // Only a size/lifetime boundary waits, never an ordinary frame. AMD's
        // fences cover its writes; Veldrid consumers may still sample output.
        _generator.WaitForIdle();
        _device.WaitForIdle();
        ReleaseTargets();
        var factory = _device.ResourceFactory;
        _color = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R16_G16_B16_A16_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
        _colorView = factory.CreateTextureView(_color);
        _framebuffer = factory.CreateFramebuffer(new FramebufferDescription(null, _color));
        _output = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R16_G16_B16_A16_Float, TextureUsage.Storage | TextureUsage.Sampled));
        _outputView = factory.CreateTextureView(_output);
        RequestReset();
    }

    /// <summary>Call after submitting the HUD-free display-color pass.</summary>
    internal bool Dispatch(Texture depth, Texture motion, in TemporalFrameData frame,
        ulong frameId, float seconds, float near, float far, float verticalFov,
        Texture? colorOverride = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_output is null)
            throw new InvalidOperationException("FG output target has not been allocated.");

        var color = colorOverride ?? _color;
        if (color is null)
            throw new InvalidOperationException("FG needs a submitted HUD-free scene target.");
        if (color.Format != PixelFormat.R16_G16_B16_A16_Float ||
            color.SampleCount != TextureSampleCount.Count1 ||
            color.Width != _output.Width || color.Height != _output.Height ||
            ReferenceEquals(color, _output))
            throw new ArgumentException("Scene FG requires a separate display-size RGBA16F HUD-free color input.");
        if (depth.Format != PixelFormat.R32_Float || (depth.Usage & TextureUsage.DepthStencil) == 0 ||
            motion.Format != PixelFormat.R16_G16_Float || depth.SampleCount != TextureSampleCount.Count1 ||
            motion.SampleCount != TextureSampleCount.Count1 ||
            depth.Width != motion.Width || depth.Height != motion.Height ||
            depth.Width > color.Width || depth.Height > color.Height)
            throw new ArgumentException("Scene FG requires single-sample R32 depth and matching RG16F render-size motion.");

        LastDispatchUsedExternalColor = colorOverride is not null;
        HasGeneratedFrame = false;
        var vk = _device.GetVulkanInfo();
        vk.TransitionImageLayout(color, 5); // SHADER_READ_ONLY_OPTIMAL
        vk.TransitionImageLayout(depth, 5);
        vk.TransitionImageLayout(motion, 5);
        vk.TransitionImageLayout(_output, 1); // GENERAL
        try
        {
            HasGeneratedFrame = _generator.Dispatch(
                Wrap(color, FfxApi.FormatR16G16B16A16Float, FfxApi.ResourceUsageReadOnly),
                Wrap(depth, FfxApi.FormatR32Float, FfxApi.ResourceUsageDepthTarget),
                Wrap(motion, FfxApi.FormatR16G16Float, FfxApi.ResourceUsageReadOnly),
                Wrap(_output, FfxApi.FormatR16G16B16A16Float, FfxApi.ResourceUsageUav),
                frame, frameId, seconds, near, far, verticalFov);
            LastFrameId = frameId;
            DispatchCount++;
            if (HasGeneratedFrame) GeneratedCount++;
            return HasGeneratedFrame;
        }
        finally
        {
            // The provider restores inputs to compute-read and leaves output
            // in GENERAL, including submitted error cleanup. Synchronize the
            // public Veldrid tracker before any sampling/copy on that queue.
            vk.OverrideImageLayout(color, 5);
            vk.OverrideImageLayout(depth, 5);
            vk.OverrideImageLayout(motion, 5);
            vk.OverrideImageLayout(_output, 1);
            // A cached Veldrid resource set may already expect this layout;
            // changing only the tracker does not record the AMD write-to-draw
            // dependency. Queue the real barrier before the WSI consumer.
            vk.TransitionImageLayout(_output, 5);
            vk.SynchronizeNativeDispatch();
        }
    }

    internal void RequestReset()
    {
        HasGeneratedFrame = false;
        _generator.RequestReset();
    }

    private FfxApiResource Wrap(Texture texture, uint format, uint usage) => new()
    {
        Resource = unchecked((nint)(long)_device.GetVulkanInfo().GetVkImage(texture)),
        Description = new FfxApiResourceDescription {
            Type = FfxApi.ResourceTypeTexture2D, Format = format, Width = texture.Width,
            Height = texture.Height, Depth = 1, MipCount = 1, Usage = usage },
        State = usage == FfxApi.ResourceUsageUav
            ? FfxApi.ResourceStateUnorderedAccess : FfxApi.ResourceStateComputeRead
    };

    private void ReleaseTargets()
    {
        _outputView?.Dispose();
        _output?.Dispose();
        _framebuffer?.Dispose();
        _colorView?.Dispose();
        _color?.Dispose();
        _outputView = null;
        _output = null;
        _framebuffer = null;
        _colorView = null;
        _color = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _generator.WaitForIdle();
        _device.WaitForIdle();
        _generator.Dispose();
        ReleaseTargets();
        _disposed = true;
        HasGeneratedFrame = false;
    }
}
