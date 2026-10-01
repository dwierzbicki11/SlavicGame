using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Renderer.Vulkan;

public unsafe sealed class VulkanContext : IDisposable
{
    private Vk? _vk;
    private Instance _instance;
    private PhysicalDevice _physicalDevice;

    public Vk Vk => _vk ?? throw new InvalidOperationException("Vulkan has not been initialized.");
    public Instance Instance => _instance;
    public PhysicalDevice PhysicalDevice => _physicalDevice;

    public void Initialize()
    {
        if (_vk is not null)
        {
            return;
        }

        _vk = Vk.GetApi();
        CreateInstance();
        SelectPhysicalDevice();

        EngineLog.Info("Vulkan context initialized.");
    }

    private void CreateInstance()
    {
        var applicationName = SilkMarshal.StringToPtr("SlavicGame");
        var engineName = SilkMarshal.StringToPtr("SlavicGame Engine");

        try
        {
            var applicationInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)applicationName,
                ApplicationVersion = new Version32(0, 1, 0),
                PEngineName = (byte*)engineName,
                EngineVersion = new Version32(0, 1, 0),
                ApiVersion = Vk.Version10
            };

            var createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo
            };

            var result = Vk.CreateInstance(in createInfo, null, out _instance);

            if (result != Result.Success)
            {
                throw new InvalidOperationException($"vkCreateInstance failed with {result}.");
            }
        }
        finally
        {
            SilkMarshal.Free(applicationName);
            SilkMarshal.Free(engineName);
        }
    }

    private void SelectPhysicalDevice()
    {
        var devices = Vk.GetPhysicalDevices(_instance);

        if (devices.Length == 0)
        {
            throw new InvalidOperationException("No Vulkan-compatible physical device was found.");
        }

        _physicalDevice = devices[0];

        for (var index = 0; index < devices.Length; index++)
        {
            var device = devices[index];
            Vk.GetPhysicalDeviceProperties(device, out var properties);

            EngineLog.Info(
                $"GPU {index}: type={properties.DeviceType}, " +
                $"vendor=0x{properties.VendorID:X4}, " +
                $"device=0x{properties.DeviceID:X4}, " +
                $"api={properties.ApiVersion}");
        }
    }

    public void Dispose()
    {
        if (_vk is null)
        {
            return;
        }

        if (_instance.Handle != 0)
        {
            _vk.DestroyInstance(_instance, null);
            _instance = default;
        }

        _vk.Dispose();
        _vk = null;
    }
}
