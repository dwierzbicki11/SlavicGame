using System.Runtime.InteropServices;

namespace SlavicGame.Engine.Renderer;

public static class VulkanRuntimeCompatibility
{
    private static readonly Lazy<bool> Initialized = new(() =>
    {
        if (OperatingSystem.IsLinux())
        {
            // Vk 1.0.25 imports "libdl", while current glibc distributions
            // provide libdl.so.2 without an unversioned libdl.so symlink.
            NativeLibrary.SetDllImportResolver(typeof(Vulkan.VulkanNative).Assembly,
                (name, assembly, paths) => name == "libdl" &&
                    NativeLibrary.TryLoad("libdl.so.2", out var handle) ? handle : 0);
        }
        return true;
    });

    public static void EnsureInitialized() => _ = Initialized.Value;
}
