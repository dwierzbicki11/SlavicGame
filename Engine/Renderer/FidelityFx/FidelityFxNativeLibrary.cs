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
        string? loadFailure = null;
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
                loadFailure =
                    $"FidelityFX library '{candidate}' is missing exports: " +
                    string.Join(", ", missing);
                continue;
            }

            if (OperatingSystem.IsLinux() && !ValidateLinuxAbi(handle, out var abiError))
            {
                NativeLibrary.Free(handle);
                loadFailure = $"FidelityFX library '{candidate}' rejected: {abiError}";
                continue;
            }

            library = new FidelityFxNativeLibrary(handle, candidate);
            diagnostic =
                $"FidelityFX Vulkan runtime loaded from '{candidate}'.";
            return true;
        }

        diagnostic = loadFailure ??
            "FidelityFX Vulkan runtime was not found. " +
            "FSR3 remains unavailable and the renderer will use its fallback.";
        return false;
    }

    private static bool ValidateLinuxAbi(nint handle, out string diagnostic)
    {
        if (!NativeLibrary.TryGetExport(handle, "slavicFsrLinuxVersion", out var versionPointer) ||
            !NativeLibrary.TryGetExport(handle, "slavicFsrAbiLayout", out var layoutPointer))
        {
            diagnostic = "Missing SlavicGame Linux provider identity/ABI exports.";
            return false;
        }
        var version = Marshal.GetDelegateForFunctionPointer<LinuxVersionDelegate>(versionPointer);
        var layout = Marshal.GetDelegateForFunctionPointer<LinuxLayoutDelegate>(layoutPointer);
        if (version() != 0x030104)
        {
            diagnostic = "Linux provider does not implement pinned FSR 3.1.4.";
            return false;
        }
        var expected = new nuint[]
        {
            (nuint)Marshal.SizeOf<FfxApiHeader>(),
            (nuint)Marshal.SizeOf<FfxCreateBackendVkDesc>(),
            (nuint)Marshal.SizeOf<FfxCreateContextDescUpscale>(),
            (nuint)Marshal.SizeOf<FfxApiResource>(),
            (nuint)Marshal.SizeOf<FfxDispatchDescUpscale>(),
            (nuint)Marshal.OffsetOf<FfxDispatchDescUpscale>(nameof(FfxDispatchDescUpscale.Reset)),
            (nuint)Marshal.OffsetOf<FfxDispatchDescUpscale>(nameof(FfxDispatchDescUpscale.CameraNear))
        };
        for (uint i = 0; i < expected.Length; i++)
        {
            if (layout(i) == expected[i]) continue;
            diagnostic = $"Linux provider ABI layout mismatch at entry {i}.";
            return false;
        }
        diagnostic = "Linux FSR 3.1.4 provider ABI validated.";
        return true;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint LinuxVersionDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nuint LinuxLayoutDelegate(uint entry);

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
            // Our source-built upscaler adapter, verified by version and ABI.
            yield return System.IO.Path.Combine(
                localDirectory,
                "libslavic_fsr3_vk.so");
            yield return System.IO.Path.Combine(
                baseDirectory,
                "libslavic_fsr3_vk.so");
            yield return "libslavic_fsr3_vk.so";
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
