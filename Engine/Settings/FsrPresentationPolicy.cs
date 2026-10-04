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

    // FSR1 always runs the complete EASU -> RCAS chain on every backend.
    // Fullscreen passes use the renderer's canonical UV convention, so Vulkan
    // must not bypass RCAS as an orientation workaround.
    public static bool UsesSinglePassEasuCompatibility(bool isVulkan) => false;

    public static bool RequiresFinalRcasYFlip(
        UpscalerMode upscaler,
        uint inputWidth,
        uint inputHeight,
        uint outputWidth,
        uint outputHeight) => false;
}
