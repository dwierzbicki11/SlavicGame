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

    // AMD FSR1 is EASU followed by RCAS on every backend. Keep one canonical
    // fullscreen UV convention instead of bypassing RCAS to work around a
    // backend-specific presentation orientation.
    public static bool UsesSinglePassEasuCompatibility(bool isVulkan) => false;

    public static bool RequiresFinalRcasYFlip(
        UpscalerMode upscaler,
        uint inputWidth,
        uint inputHeight,
        uint outputWidth,
        uint outputHeight) => false;
}
