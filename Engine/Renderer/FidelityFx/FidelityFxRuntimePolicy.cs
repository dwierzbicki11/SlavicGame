namespace SlavicGame.Engine.Renderer.FidelityFx;

public enum FidelityFxRuntimeStatus
{
    Available,
    UnsupportedPlatform,
    NativeLibraryMissing
}

public readonly record struct FidelityFxRuntimeProbe(
    FidelityFxRuntimeStatus Status,
    string Diagnostic)
{
    public bool IsAvailable => Status == FidelityFxRuntimeStatus.Available;
}

public static class FidelityFxRuntimePolicy
{
    public static FidelityFxRuntimeProbe Probe()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
        {
            return new(
                FidelityFxRuntimeStatus.UnsupportedPlatform,
                "FidelityFX Vulkan runtime is supported only on Vulkan desktop targets.");
        }

        if (FidelityFxNativeLibrary.TryLoad(
                out var library,
                out var diagnostic))
        {
            library?.Dispose();
            return new(
                FidelityFxRuntimeStatus.Available,
                diagnostic);
        }

        return new(
            FidelityFxRuntimeStatus.NativeLibraryMissing,
            diagnostic);
    }
}
