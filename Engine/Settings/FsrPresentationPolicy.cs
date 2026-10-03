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

    // The EASU result is another offscreen image. Whenever the real FSR
    // upscale path runs, RCAS is the final offscreen->swapchain transition
    // and owns one vertical orientation correction.
    public static bool RequiresFinalRcasYFlip(
        UpscalerMode upscaler,
        uint inputWidth,
        uint inputHeight,
        uint outputWidth,
        uint outputHeight) =>
        UsesUpscalePass(upscaler, inputWidth, inputHeight, outputWidth, outputHeight);
}
