using Veldrid;

namespace SlavicGame.Engine.Renderer.FidelityFx;

public readonly record struct FidelityFxVulkanDeviceHandles(
    nint Instance,
    nint PhysicalDevice,
    nint Device,
    nint GraphicsQueue,
    uint GraphicsQueueFamilyIndex);

public readonly record struct FidelityFxVulkanImageHandle(
    ulong Image,
    uint Width,
    uint Height,
    PixelFormat Format);

/// <summary>
/// Thin, allocation-free adapter between Veldrid's Vulkan backend and the
/// native FidelityFX Vulkan API. It deliberately exposes only handles which
/// Veldrid documents through BackendInfoVulkan; no reflection into Veldrid
/// internals is used.
/// </summary>
public static class FidelityFxVulkanInterop
{
    public static FidelityFxVulkanDeviceHandles GetDeviceHandles(
        GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        if (graphicsDevice.BackendType != GraphicsBackend.Vulkan)
        {
            throw new NotSupportedException(
                "FidelityFX Vulkan interop requires a Vulkan GraphicsDevice.");
        }

        var info = graphicsDevice.GetVulkanInfo();
        return new FidelityFxVulkanDeviceHandles(
            info.Instance,
            info.PhysicalDevice,
            info.Device,
            info.GraphicsQueue,
            info.GraphicsQueueFamilyIndex);
    }

    public static FidelityFxVulkanImageHandle GetImageHandle(
        GraphicsDevice graphicsDevice,
        Texture texture)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(texture);
        if (graphicsDevice.BackendType != GraphicsBackend.Vulkan)
        {
            throw new NotSupportedException(
                "FidelityFX Vulkan interop requires a Vulkan GraphicsDevice.");
        }

        var info = graphicsDevice.GetVulkanInfo();
        return new FidelityFxVulkanImageHandle(
            info.GetVkImage(texture),
            texture.Width,
            texture.Height,
            texture.Format);
    }
}
