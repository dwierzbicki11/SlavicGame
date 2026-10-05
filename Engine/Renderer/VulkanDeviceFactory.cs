using System.Runtime.InteropServices;
using Veldrid;

namespace SlavicGame.Engine.Renderer;

/// <summary>One device creation path shared by the renderer and GPU regression tests.</summary>
public static class VulkanDeviceFactory
{
    public const uint Vulkan10 = 1u << 22;
    public const uint Vulkan11 = Vulkan10 | (1u << 12);

    public static uint SelectInstanceVersion(uint loaderVersion, bool linux) =>
        linux && loaderVersion >= Vulkan11 ? Vulkan11 : Vulkan10;

    public static VulkanDeviceOptions GetOptions() => new()
    {
        InstanceApiVersion = SelectInstanceVersion(GetLoaderVersion(), OperatingSystem.IsLinux()),
        EnableShaderStorageImageExtendedFormats = true
    };

    public static GraphicsDevice Create(GraphicsDeviceOptions options, SwapchainDescription? swapchain = null)
    {
        VulkanRuntimeCompatibility.EnsureInitialized();
        var vk = GetOptions();
        GraphicsDevice CreateDevice() => swapchain is { } sc
                ? GraphicsDevice.CreateVulkan(options, sc, vk)
                : GraphicsDevice.CreateVulkan(options, vk);
        try { return CreateDevice(); }
        catch (VulkanInstanceCreationException ex) when (ex.ResultCode == -9 && vk.InstanceApiVersion > Vulkan10)
        {
            // A newer loader can wrap an older ICD. Retry only an API-version
            // rejection, before the failed constructor has created a surface/device.
            vk.InstanceApiVersion = Vulkan10;
            return CreateDevice();
        }
    }

    public static uint GetLoaderVersion()
    {
        string path = OperatingSystem.IsWindows() ? "vulkan-1.dll" : "libvulkan.so.1";
        if (!NativeLibrary.TryLoad(path, out var library)) return Vulkan10;
        try
        {
            // A Vulkan 1.0 loader has no enumerate-instance-version export.
            if (!NativeLibrary.TryGetExport(library, "vkEnumerateInstanceVersion", out var export))
                return Vulkan10;
            var enumerate = Marshal.GetDelegateForFunctionPointer<EnumerateVersion>(export);
            return enumerate(out var version) == 0 ? version : Vulkan10;
        }
        finally { NativeLibrary.Free(library); }
    }

    public static string? FrameGenerationUnavailableReason(BackendInfoVulkan info)
    {
        if (info.InstanceApiVersion < Vulkan11)
            return "FG requires a Vulkan 1.1 instance; the renderer created Vulkan 1.0.";
        if (info.PhysicalDeviceApiVersion < Vulkan11)
            return "FG requires a Vulkan 1.1 physical device.";
        if (!info.ShaderStorageImageExtendedFormatsEnabled)
            return "FG requires shaderStorageImageExtendedFormats enabled on the logical device.";
        // The native FG bridge separately checks compute subgroup basic/arithmetic.
        return null;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int EnumerateVersion(out uint version);
}
