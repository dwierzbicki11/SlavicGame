namespace SlavicGame.Engine.Renderer.FidelityFx;

public readonly record struct FidelityFxVulkanDeviceHandles(
    nint Instance,
    nint PhysicalDevice,
    nint Device,
    nint GraphicsQueue,
    uint GraphicsQueueFamilyIndex);
