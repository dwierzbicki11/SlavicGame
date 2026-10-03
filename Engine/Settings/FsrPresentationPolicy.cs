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

    // Vulkan render targets and swapchain presentation use the same canonical
    // fullscreen UV orientation in SlavicGame. FSR must not add an extra Y
    // inversion in EASU or RCAS.
    public static bool RequiresFinalRcasYFlip(
        UpscalerMode upscaler,
        uint inputWidth,
        uint inputHeight,
        uint outputWidth,
        uint outputHeight) => false;
}
