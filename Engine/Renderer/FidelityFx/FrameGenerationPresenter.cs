using System.Diagnostics;
using Veldrid;

namespace SlavicGame.Engine.Renderer.FidelityFx;

internal enum PresentedFrameKind { Generated, Rendered }

internal readonly record struct FramePresentation(ulong SceneFrameId, PresentedFrameKind Kind,
    uint ImageIndex, ulong SwapchainGeneration, uint PresentMode, long Timestamp, double MinimumSpacingSeconds);

/// <summary>
/// The OS swapchain endpoint for real AMD output. Each call signals Veldrid's
/// per-image semaphore and presents that image. No simulation or input runs
/// here. CPU deadlines keep immediate-mode output from collapsing into a
/// back-to-back pair; FIFO additionally enforces the display's VSync cadence.
/// </summary>
internal sealed class FrameGenerationPresenter
{
    private readonly BackendInfoVulkan _vk;
    private readonly Swapchain _swapchain;
    private long _lastPresentation;
    private double _pacingSeconds;
    private double _minimumSpacing;
    internal ulong RenderedCount { get; private set; }
    internal ulong GeneratedCount { get; private set; }
    internal ulong PresentedCount => RenderedCount + GeneratedCount;
    internal event Action<FramePresentation>? Presented;

    internal FrameGenerationPresenter(GraphicsDevice device)
    {
        _vk = device.GetVulkanInfo();
        _swapchain = device.MainSwapchain;
        _vk.ConfigureFrameGenerationPresentation(_swapchain, true);
    }

    internal void BeginFrame(double frameDeltaSeconds, int renderedFrameLimit)
    {
        // Exclude this presenter's preceding CPU delay from the estimate.
        // Otherwise a slow frame feeds its own delay into the next frame and
        // progressively lowers the render rate. A user limit denotes rendered
        // frames; two output slots fit within its requested interval.
        var interval = double.IsFinite(frameDeltaSeconds) && frameDeltaSeconds > 0 ? frameDeltaSeconds : 1.0 / 60;
        var workInterval = Math.Max(0.002, interval - _pacingSeconds);
        if (renderedFrameLimit > 0) workInterval = Math.Max(workInterval, 1.0 / renderedFrameLimit);
        _minimumSpacing = Math.Clamp(workInterval * 0.5, 0.001, 0.05);
        _pacingSeconds = 0;
    }

    internal bool Submit(CommandList commands, PresentedFrameKind kind, ulong sceneFrameId, bool paired)
    {
        // Pace both boundaries of a pair, including real(n-1) -> generated(n).
        // Late render/GPU work may miss a deadline; never catch up by bursting.
        if (paired && _lastPresentation != 0)
        {
            var start = Stopwatch.GetTimestamp();
            var deadline = _lastPresentation + (long)(_minimumSpacing * Stopwatch.Frequency);
            while (true)
            {
                var remaining = (deadline - Stopwatch.GetTimestamp()) / (double)Stopwatch.Frequency;
                if (remaining <= 0) break;
                if (remaining > 0.0025) Thread.Sleep(Math.Max(1, (int)((remaining - 0.0015) * 1000)));
                else Thread.SpinWait(64);
            }
            _pacingSeconds += (Stopwatch.GetTimestamp() - start) / (double)Stopwatch.Frequency;
        }
        var result = _vk.SubmitCommandsAndPresent(commands, _swapchain);
        if (!result.Presented) return false;
        _lastPresentation = result.Timestamp;
        if (kind == PresentedFrameKind.Generated) GeneratedCount++; else RenderedCount++;
        Presented?.Invoke(new(sceneFrameId, kind, result.ImageIndex, result.Generation,
            result.PresentMode, result.Timestamp, paired ? _minimumSpacing : 0));
        return true;
    }

    internal void ResetPacing()
    {
        _lastPresentation = 0;
        _pacingSeconds = 0;
    }
}
