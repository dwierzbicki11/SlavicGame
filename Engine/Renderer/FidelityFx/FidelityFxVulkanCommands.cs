using System.Runtime.InteropServices;

namespace SlavicGame.Engine.Renderer.FidelityFx;

/// <summary>
/// Small Vulkan command-buffer ring used only by native FidelityFX dispatches.
/// Work is submitted to Veldrid's graphics queue, so queue ordering keeps the
/// scene -> FSR -> presentation dependency without a per-frame device idle.
/// </summary>
internal sealed class FidelityFxVulkanCommands : IDisposable
{
    private const uint CommandPoolResetCommandBufferBit = 0x2;
    private const uint CommandBufferUsageOneTimeSubmitBit = 0x1;
    private const uint FenceCreateSignaledBit = 0x1;
    private const uint CommandBufferPrimary = 0;

    private const uint StructureTypeSubmitInfo = 4;
    private const uint StructureTypeFenceCreateInfo = 8;
    private const uint StructureTypeCommandPoolCreateInfo = 39;
    private const uint StructureTypeCommandBufferAllocateInfo = 40;
    private const uint StructureTypeCommandBufferBeginInfo = 42;

    private const int RingSize = 3;

    private readonly nint _vulkanLibrary;
    private readonly nint _device;
    private readonly nint _queue;
    private readonly VkDestroyCommandPool _destroyCommandPool;
    private readonly VkDestroyFence _destroyFence;
    private readonly VkWaitForFences _waitForFences;
    private readonly VkResetFences _resetFences;
    private readonly VkResetCommandBuffer _resetCommandBuffer;
    private readonly VkBeginCommandBuffer _beginCommandBuffer;
    private readonly VkEndCommandBuffer _endCommandBuffer;
    private readonly VkQueueSubmit _queueSubmit;
    private readonly nint[] _commandBuffers = new nint[RingSize];
    private readonly ulong[] _fences = new ulong[RingSize];

    private ulong _commandPool;
    private int _slot;
    private bool _disposed;

    public FidelityFxVulkanCommands(
        FidelityFxVulkanDeviceHandles handles)
    {
        _device = handles.Device;
        _queue = handles.GraphicsQueue;

        _vulkanLibrary = LoadVulkanLibrary();
        if (!NativeLibrary.TryGetExport(
                _vulkanLibrary,
                "vkGetDeviceProcAddr",
                out var getDeviceProcAddrPointer))
        {
            throw new MissingMethodException(
                "Vulkan loader export 'vkGetDeviceProcAddr' is unavailable.");
        }
        DeviceProcAddr = getDeviceProcAddrPointer;
        var getDeviceProcAddr =
            Marshal.GetDelegateForFunctionPointer<VkGetDeviceProcAddr>(
                getDeviceProcAddrPointer);

        T Load<T>(string name) where T : Delegate
        {
            var pointer = getDeviceProcAddr(_device, name);
            if (pointer == 0)
                throw new MissingMethodException(
                    $"Vulkan device export '{name}' is unavailable.");
            return Marshal.GetDelegateForFunctionPointer<T>(pointer);
        }

        var createCommandPool = Load<VkCreateCommandPool>(
            "vkCreateCommandPool");
        _destroyCommandPool = Load<VkDestroyCommandPool>(
            "vkDestroyCommandPool");
        var allocateCommandBuffers = Load<VkAllocateCommandBuffers>(
            "vkAllocateCommandBuffers");
        var createFence = Load<VkCreateFence>("vkCreateFence");
        _destroyFence = Load<VkDestroyFence>("vkDestroyFence");
        _waitForFences = Load<VkWaitForFences>("vkWaitForFences");
        _resetFences = Load<VkResetFences>("vkResetFences");
        _resetCommandBuffer = Load<VkResetCommandBuffer>(
            "vkResetCommandBuffer");
        _beginCommandBuffer = Load<VkBeginCommandBuffer>(
            "vkBeginCommandBuffer");
        _endCommandBuffer = Load<VkEndCommandBuffer>(
            "vkEndCommandBuffer");
        _queueSubmit = Load<VkQueueSubmit>("vkQueueSubmit");

        var poolInfo = new VkCommandPoolCreateInfo
        {
            SType = StructureTypeCommandPoolCreateInfo,
            Flags = CommandPoolResetCommandBufferBit,
            QueueFamilyIndex = handles.GraphicsQueueFamilyIndex
        };
        Check(
            createCommandPool(
                _device,
                ref poolInfo,
                0,
                out _commandPool),
            "vkCreateCommandPool");

        var bufferMemory = Marshal.AllocHGlobal(
            nint.Size * RingSize);
        try
        {
            var allocateInfo = new VkCommandBufferAllocateInfo
            {
                SType = StructureTypeCommandBufferAllocateInfo,
                CommandPool = _commandPool,
                Level = CommandBufferPrimary,
                CommandBufferCount = RingSize
            };
            Check(
                allocateCommandBuffers(
                    _device,
                    ref allocateInfo,
                    bufferMemory),
                "vkAllocateCommandBuffers");

            for (var i = 0; i < RingSize; i++)
                _commandBuffers[i] =
                    Marshal.ReadIntPtr(bufferMemory, i * nint.Size);
        }
        finally
        {
            Marshal.FreeHGlobal(bufferMemory);
        }

        for (var i = 0; i < RingSize; i++)
        {
            var fenceInfo = new VkFenceCreateInfo
            {
                SType = StructureTypeFenceCreateInfo,
                Flags = FenceCreateSignaledBit
            };
            Check(
                createFence(
                    _device,
                    ref fenceInfo,
                    0,
                    out _fences[i]),
                "vkCreateFence");
        }

    }

    /// <summary>
    /// Exact vkGetDeviceProcAddr pointer required by ffxCreateBackendVKDesc.
    /// </summary>
    public nint DeviceProcAddr { get; }

    public nint Begin()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var fence = _fences[_slot];
        Check(
            _waitForFences(
                _device,
                1,
                ref fence,
                1,
                ulong.MaxValue),
            "vkWaitForFences");
        Check(
            _resetFences(_device, 1, ref fence),
            "vkResetFences");

        var commandBuffer = _commandBuffers[_slot];
        Check(
            _resetCommandBuffer(commandBuffer, 0),
            "vkResetCommandBuffer");

        var beginInfo = new VkCommandBufferBeginInfo
        {
            SType = StructureTypeCommandBufferBeginInfo,
            Flags = CommandBufferUsageOneTimeSubmitBit
        };
        Check(
            _beginCommandBuffer(commandBuffer, ref beginInfo),
            "vkBeginCommandBuffer");
        return commandBuffer;
    }

    public void EndAndSubmit(nint commandBuffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (commandBuffer != _commandBuffers[_slot])
            throw new InvalidOperationException(
                "FidelityFX command-buffer ring slot mismatch.");

        Check(_endCommandBuffer(commandBuffer), "vkEndCommandBuffer");

        var commandBufferMemory = Marshal.AllocHGlobal(nint.Size);
        try
        {
            Marshal.WriteIntPtr(commandBufferMemory, commandBuffer);
            var submit = new VkSubmitInfo
            {
                SType = StructureTypeSubmitInfo,
                CommandBufferCount = 1,
                CommandBuffers = commandBufferMemory
            };

            Check(
                _queueSubmit(
                    _queue,
                    1,
                    ref submit,
                    _fences[_slot]),
                "vkQueueSubmit");
        }
        finally
        {
            Marshal.FreeHGlobal(commandBufferMemory);
        }

        _slot = (_slot + 1) % RingSize;
    }

    public void WaitAll()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        for (var i = 0; i < RingSize; i++)
        {
            var fence = _fences[i];
            if (fence == 0)
                continue;
            Check(
                _waitForFences(
                    _device,
                    1,
                    ref fence,
                    1,
                    ulong.MaxValue),
                "vkWaitForFences");
        }
    }

    private static nint LoadVulkanLibrary()
    {
        var names = OperatingSystem.IsWindows()
            ? new[] { "vulkan-1.dll" }
            : new[] { "libvulkan.so.1", "libvulkan.so" };

        foreach (var name in names)
            if (NativeLibrary.TryLoad(name, out var handle))
                return handle;

        throw new DllNotFoundException(
            "The Vulkan loader required by FidelityFX was not found.");
    }

    private static T GetExport<T>(
        nint library,
        string name)
        where T : Delegate
    {
        if (!NativeLibrary.TryGetExport(
                library,
                name,
                out var pointer))
        {
            throw new MissingMethodException(
                $"Vulkan loader export '{name}' is unavailable.");
        }

        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }

    private static void Check(int result, string operation)
    {
        if (result != 0)
            throw new InvalidOperationException(
                $"{operation} failed with VkResult {result}.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        for (var i = 0; i < RingSize; i++)
        {
            var fence = _fences[i];
            if (fence != 0)
            {
                _waitForFences(
                    _device,
                    1,
                    ref fence,
                    1,
                    ulong.MaxValue);
                _destroyFence(_device, fence, 0);
                _fences[i] = 0;
            }
        }

        if (_commandPool != 0)
        {
            _destroyCommandPool(_device, _commandPool, 0);
            _commandPool = 0;
        }

        if (_vulkanLibrary != 0)
            NativeLibrary.Free(_vulkanLibrary);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct VkCommandPoolCreateInfo
    {
        public uint SType;
        public nint PNext;
        public uint Flags;
        public uint QueueFamilyIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct VkCommandBufferAllocateInfo
    {
        public uint SType;
        public nint PNext;
        public ulong CommandPool;
        public uint Level;
        public uint CommandBufferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct VkCommandBufferBeginInfo
    {
        public uint SType;
        public nint PNext;
        public uint Flags;
        public nint InheritanceInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct VkFenceCreateInfo
    {
        public uint SType;
        public nint PNext;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct VkSubmitInfo
    {
        public uint SType;
        public nint PNext;
        public uint WaitSemaphoreCount;
        public nint WaitSemaphores;
        public nint WaitDstStageMask;
        public uint CommandBufferCount;
        public nint CommandBuffers;
        public uint SignalSemaphoreCount;
        public nint SignalSemaphores;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint VkGetDeviceProcAddr(
        nint device,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkCreateCommandPool(
        nint device,
        ref VkCommandPoolCreateInfo createInfo,
        nint allocator,
        out ulong commandPool);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void VkDestroyCommandPool(
        nint device,
        ulong commandPool,
        nint allocator);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkAllocateCommandBuffers(
        nint device,
        ref VkCommandBufferAllocateInfo allocateInfo,
        nint commandBuffers);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkCreateFence(
        nint device,
        ref VkFenceCreateInfo createInfo,
        nint allocator,
        out ulong fence);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void VkDestroyFence(
        nint device,
        ulong fence,
        nint allocator);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkWaitForFences(
        nint device,
        uint fenceCount,
        ref ulong fences,
        uint waitAll,
        ulong timeout);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkResetFences(
        nint device,
        uint fenceCount,
        ref ulong fences);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkResetCommandBuffer(
        nint commandBuffer,
        uint flags);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkBeginCommandBuffer(
        nint commandBuffer,
        ref VkCommandBufferBeginInfo beginInfo);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkEndCommandBuffer(
        nint commandBuffer);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int VkQueueSubmit(
        nint queue,
        uint submitCount,
        ref VkSubmitInfo submits,
        ulong fence);
}
