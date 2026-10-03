namespace SlavicGame.Engine.Settings;

public static class FsrPresentationPolicy
{
    public static bool UsesUpscalePass(
        UpscalerMode upscaler,
        uint inputWidth,
        uint inputHeight,
        uint outputWidth,
        uint outputHeight) =>
        upscaler == UpscalerMode.Fsr1 &&
        outputWidth >= inputWidth &&
        outputHeight >= inputHeight &&
        (outputWidth > inputWidth || outputHeight > inputHeight);

    // Vulkan compatibility path: EASU renders directly to the swapchain.
    // This avoids the second fullscreen offscreen->swapchain pass (RCAS),
    // which can invert presentation on drivers where clip-space Y differs
    // from the requested standard convention.
    public static bool UsesSinglePassEasuCompatibility(bool isVulkan) => isVulkan;

    public static bool RequiresFinalRcasYFlip(
        UpscalerMode upscaler,
        uint inputWidth,
        uint inputHeight,
        uint outputWidth,
        uint outputHeight) => false;
}
