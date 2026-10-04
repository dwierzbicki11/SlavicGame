using System.Runtime.InteropServices;

namespace SlavicGame.Engine.Renderer.FidelityFx;

/// <summary>
/// Runtime loader for the ABI-stable FidelityFX API. The native SDK is kept
/// optional: machines without the official Vulkan runtime stay on the existing
/// FSR1/native presentation path instead of failing at process startup.
/// </summary>
public sealed class FidelityFxNativeLibrary : IDisposable
{
    private static readonly string[] RequiredExports =
    [
        "ffxCreateContext",
        "ffxDestroyContext",
        "ffxDispatch",
        "ffxQuery",
        "ffxConfigure"
    ];

    private nint _handle;
    private bool _disposed;

    private FidelityFxNativeLibrary(nint handle, string path)
    {
        _handle = handle;
        Path = path;
    }

    public string Path { get; }
    public bool IsLoaded => _handle != 0 && !_disposed;

    public static bool TryLoad(
        out FidelityFxNativeLibrary? library,
        out string diagnostic)
    {
        library = null;
        var candidates = BuildCandidates().Distinct(
            StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            if (!NativeLibrary.TryLoad(candidate, out var handle))
                continue;

            var missing = RequiredExports
                .Where(name =>
                    !NativeLibrary.TryGetExport(handle, name, out _))
                .ToArray();
            if (missing.Length > 0)
            {
                NativeLibrary.Free(handle);
                diagnostic =
                    $"FidelityFX library '{candidate}' is missing exports: " +
                    string.Join(", ", missing);
                continue;
            }

            library = new FidelityFxNativeLibrary(handle, candidate);
            diagnostic =
                $"FidelityFX Vulkan runtime loaded from '{candidate}'.";
            return true;
        }

        diagnostic =
            "FidelityFX Vulkan runtime was not found. " +
            "FSR3 remains unavailable and the renderer will use its fallback.";
        return false;
    }

    public nint GetExport(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (_handle == 0 ||
            !NativeLibrary.TryGetExport(_handle, name, out var address))
        {
            throw new MissingMethodException(
                $"FidelityFX export '{name}' is unavailable.");
        }

        return address;
    }

    private static IEnumerable<string> BuildCandidates()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var localDirectory = System.IO.Path.Combine(
            baseDirectory,
            "native",
            "fidelityfx");

        if (OperatingSystem.IsWindows())
        {
            yield return System.IO.Path.Combine(
                localDirectory,
                "amd_fidelityfx_vk.dll");
            yield return System.IO.Path.Combine(
                baseDirectory,
                "amd_fidelityfx_vk.dll");
            yield return "amd_fidelityfx_vk.dll";
        }
        else if (OperatingSystem.IsLinux())
        {
            // No official v1.1.4 Linux binary is shipped by AMD, but retain a
            // conventional name for a future source-built Vulkan provider.
            yield return System.IO.Path.Combine(
                localDirectory,
                "libamd_fidelityfx_vk.so");
            yield return System.IO.Path.Combine(
                baseDirectory,
                "libamd_fidelityfx_vk.so");
            yield return "libamd_fidelityfx_vk.so";
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_handle != 0)
        {
            NativeLibrary.Free(_handle);
            _handle = 0;
        }
    }
}
