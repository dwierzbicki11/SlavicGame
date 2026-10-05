using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace SlavicGame.Engine.Renderer.FidelityFx;

/// <summary>
/// Linux FG runtime, independent of the temporal upscaler. The caller supplies
/// HUD-free, display-resolution color and render-resolution depth/velocity on
/// the same Vulkan graphics queue. Input images must be shader-read-only and
/// the output must be in GENERAL; the caller owns their lifetime/layout tracker.
/// Device creation must enable shaderStorageImageExtendedFormats, as both the
/// current Veldrid device factory and the native regression fixture do.
/// This stage generates offscreen images; it does not present extra frames.
/// </summary>
internal sealed class FidelityFxFrameGeneration : IDisposable
{
    internal const uint MinimumVulkanVersion = (1u << 22) | (1u << 12);
    private const uint JitterMotionVectors = 1u << 2;
    private const uint TransferFunctionSrgb = 0;

    private readonly FidelityFxNativeLibrary _library;
    private readonly FidelityFxVulkanDeviceHandles _handles;
    private readonly FidelityFxVulkanCommands _commands;
    private readonly SlavicFgCreateDelegate _create;
    private readonly SlavicFgPrepareDelegate _prepare;
    private readonly SlavicFgDispatchDelegate _dispatch;
    private readonly SlavicFgDestroyDelegate _destroy;
    private nint _context;
    private uint _renderWidth;
    private uint _renderHeight;
    private uint _displayWidth;
    private uint _displayHeight;
    private uint _format;
    private bool _hasHistory;
    private bool _resetRequested = true;
    private ulong _lastFrameId;
    private bool _disposed;

    private FidelityFxFrameGeneration(
        FidelityFxVulkanDeviceHandles handles,
        FidelityFxNativeLibrary library)
    {
        _handles = handles;
        _library = library;
        // Validate and bind every export before allocating Vulkan resources.
        ValidateAbi(library);
        if (Bind<IsSupportedDelegate>(library, "slavicFgIsSupported")(handles.PhysicalDevice) == 0)
            throw new NotSupportedException("FG needs Vulkan 1.1 compute subgroup arithmetic and extended storage formats.");
        _create = Bind<SlavicFgCreateDelegate>(library, "slavicFgCreate");
        _prepare = Bind<SlavicFgPrepareDelegate>(library, "slavicFgPrepare");
        _dispatch = Bind<SlavicFgDispatchDelegate>(library, "slavicFgDispatch");
        _destroy = Bind<SlavicFgDestroyDelegate>(library, "slavicFgDestroy");
        _commands = new FidelityFxVulkanCommands(handles);
    }

    public bool IsReady => _context != 0 && !_disposed;

    public static bool TryCreate(GraphicsDevice device,
        out FidelityFxFrameGeneration? generator, out string diagnostic)
    {
        generator = null;
        if (device.BackendType != GraphicsBackend.Vulkan)
        {
            diagnostic = "FSR3 FG requires a Vulkan device.";
            return false;
        }
        var info = device.GetVulkanInfo();
        var reason = VulkanDeviceFactory.FrameGenerationUnavailableReason(info);
        if (reason is not null)
        {
            diagnostic = reason;
            return false;
        }
        return TryCreate(FidelityFxVulkanInterop.GetDeviceHandles(device),
            info.InstanceApiVersion, out generator, out diagnostic);
    }

    public static bool TryCreate(
        FidelityFxVulkanDeviceHandles handles,
        uint requestedVulkanVersion,
        out FidelityFxFrameGeneration? generator,
        out string diagnostic)
    {
        generator = null;
        // Use the instance's requested API version, not the driver's advertised
        // version. The raw-handle entry point is for externally owned devices
        // whose creator guarantees the extended storage feature is enabled.
        if (requestedVulkanVersion < MinimumVulkanVersion)
        {
            diagnostic = "FSR3 FG requires a Vulkan 1.1 instance; upscaling remains available.";
            return false;
        }
        if (!OperatingSystem.IsLinux())
        {
            diagnostic = "This FG bridge currently supports the source-built Linux provider.";
            return false;
        }
        if (handles.Instance == 0 || handles.Device == 0 || handles.PhysicalDevice == 0 || handles.GraphicsQueue == 0)
        {
            diagnostic = "FSR3 FG requires valid Vulkan device and queue handles.";
            return false;
        }
        if (!FidelityFxNativeLibrary.TryLoad(out var library, out diagnostic) || library is null)
            return false;
        try
        {
            generator = new FidelityFxFrameGeneration(handles, library);
            diagnostic = "AMD FSR 3.1.4 managed FG runtime ready for offscreen dispatch.";
            return true;
        }
        catch (Exception exception)
        {
            library.Dispose();
            diagnostic = $"FSR3 FG unavailable: {exception.Message}";
            return false;
        }
    }

    /// <returns>Whether the output can be used as an intermediate frame.
    /// A first/reset/disjoint frame initializes history and returns false.</returns>
    public bool Dispatch(
        in FfxApiResource hudlessColor,
        in FfxApiResource depth,
        in FfxApiResource motionVectors,
        in FfxApiResource output,
        in TemporalFrameData frame,
        ulong frameId,
        float frameTimeSeconds,
        float cameraNear,
        float cameraFar,
        float verticalFov)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ValidateResources(hudlessColor, depth, motionVectors, output);
        if (!float.IsFinite(frameTimeSeconds) || frameTimeSeconds < 0 ||
            !float.IsFinite(cameraNear) || cameraNear <= 0 ||
            !float.IsFinite(cameraFar) || cameraFar <= cameraNear ||
            !float.IsFinite(verticalFov) || verticalFov <= 0 || verticalFov >= MathF.PI)
            throw new ArgumentException("FG requires finite frame timing and a valid camera projection.");
        if (!Matrix4x4.Invert(frame.View, out var inverseView))
            throw new ArgumentException("FG camera view must be invertible.", nameof(frame));
        EnsureSize(depth.Description.Width, depth.Description.Height,
            output.Description.Width, output.Description.Height, output.Description.Format);

        var reset = _resetRequested || frame.ResetHistory || !_hasHistory ||
            frameId != unchecked(_lastFrameId + 1);
        var command = _commands.Begin();
        var prepare = new SlavicFgPrepareDesc
        {
            CommandList = command,
            Depth = depth,
            MotionVectors = motionVectors,
            RenderWidth = depth.Description.Width,
            RenderHeight = depth.Description.Height,
            JitterX = frame.JitterPixels.X,
            JitterY = -frame.JitterPixels.Y,
            MotionVectorScaleX = depth.Description.Width,
            MotionVectorScaleY = depth.Description.Height,
            FrameTimeDelta = Math.Clamp(frameTimeSeconds * 1000f, 0.01f, 1000f),
            CameraNear = cameraNear,
            CameraFar = cameraFar,
            ViewSpaceToMetersFactor = 1,
            CameraFovAngleVertical = verticalFov,
            FrameId = frameId,
            CameraPosition = inverseView.Translation,
            CameraUp = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitY, inverseView)),
            CameraRight = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitX, inverseView)),
            CameraForward = Vector3.Normalize(Vector3.TransformNormal(-Vector3.UnitZ, inverseView))
        };
        var dispatch = new SlavicFgDispatchDesc
        {
            CommandList = command,
            CurrentBackBuffer = hudlessColor,
            CurrentBackBufferHudless = hudlessColor,
            Output = output,
            FrameId = frameId,
            Reset = reset ? 1u : 0u,
            BackBufferTransferFunction = TransferFunctionSrgb,
            MinLuminance = 0,
            MaxLuminance = 1
        };

        // An SDK error may still leave barrier/cleanup work in the command
        // buffer. Submit it and retain fence ownership before reporting failure.
        _resetRequested = true;
        var operation = "slavicFgPrepare";
        var result = _prepare(_context, prepare);
        if (result == 0)
        {
            operation = "slavicFgDispatch";
            result = _dispatch(_context, dispatch);
        }
        _commands.EndAndSubmit(command);
        if (result != 0)
        {
            _resetRequested = true;
            throw new InvalidOperationException($"{operation} returned FfxErrorCode {result}.");
        }
        _hasHistory = true;
        _resetRequested = false;
        _lastFrameId = frameId;
        return !reset;
    }

    public void RequestReset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _resetRequested = true;
    }

    public void WaitForIdle()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _commands.WaitAll();
    }

    private void EnsureSize(uint renderWidth, uint renderHeight,
        uint displayWidth, uint displayHeight, uint format)
    {
        if (_context != 0 && _renderWidth == renderWidth && _renderHeight == renderHeight &&
            _displayWidth == displayWidth && _displayHeight == displayHeight && _format == format)
            return;

        _commands.WaitAll();
        DestroyContext();
        var desc = new SlavicFgCreateDesc
        {
            Device = _handles.Device,
            PhysicalDevice = _handles.PhysicalDevice,
            DeviceProcAddr = _commands.DeviceProcAddr,
            MaxRenderWidth = renderWidth,
            MaxRenderHeight = renderHeight,
            DisplayWidth = displayWidth,
            DisplayHeight = displayHeight,
            BackBufferFormat = format,
            Flags = JitterMotionVectors
        };
        var result = _create(desc, ref _context);
        if (result != 0 || _context == 0)
        {
            DestroyContext();
            throw new InvalidOperationException($"slavicFgCreate returned FfxErrorCode {result}.");
        }
        _renderWidth = renderWidth;
        _renderHeight = renderHeight;
        _displayWidth = displayWidth;
        _displayHeight = displayHeight;
        _format = format;
    }

    private static void ValidateResources(in FfxApiResource color, in FfxApiResource depth,
        in FfxApiResource motion, in FfxApiResource output)
    {
        ValidateResource(color, FfxApi.ResourceStateComputeRead);
        ValidateResource(depth, FfxApi.ResourceStateComputeRead);
        ValidateResource(motion, FfxApi.ResourceStateComputeRead);
        ValidateResource(output, FfxApi.ResourceStateUnorderedAccess);
        if (color.Description.Width != output.Description.Width ||
            color.Description.Height != output.Description.Height ||
            color.Description.Format != output.Description.Format ||
            depth.Description.Width != motion.Description.Width ||
            depth.Description.Height != motion.Description.Height ||
            depth.Description.Width > output.Description.Width ||
            depth.Description.Height > output.Description.Height || color.Resource == output.Resource)
            throw new ArgumentException("FG color/output and depth/motion extents must match; output must be separate.");
        if ((output.Description.Usage & FfxApi.ResourceUsageUav) == 0 ||
            (depth.Description.Usage & FfxApi.ResourceUsageDepthTarget) == 0)
            throw new ArgumentException("FG output must support UAV writes and depth must describe a depth target.");
    }

    private static void ValidateResource(in FfxApiResource resource, uint state)
    {
        var desc = resource.Description;
        if (resource.Resource == 0 || desc.Type != FfxApi.ResourceTypeTexture2D ||
            desc.Width == 0 || desc.Height == 0 || desc.Depth != 1 || desc.MipCount != 1 ||
            desc.Format == FfxApi.FormatUnknown || resource.State != state)
            throw new ArgumentException("FG requires valid single-mip 2D images with the declared Vulkan layouts.");
    }

    private static T Bind<T>(FidelityFxNativeLibrary library, string name) where T : Delegate =>
        Marshal.GetDelegateForFunctionPointer<T>(library.GetExport(name));

    private static void ValidateAbi(FidelityFxNativeLibrary library)
    {
        var layout = Bind<AbiLayoutDelegate>(library, "slavicFgAbiLayout");
        nuint[] expected =
        [
            1, (nuint)Marshal.SizeOf<SlavicFgCreateDesc>(),
            (nuint)Marshal.SizeOf<SlavicFgPrepareDesc>(),
            (nuint)Marshal.SizeOf<SlavicFgDispatchDesc>(),
            (nuint)Marshal.OffsetOf<SlavicFgPrepareDesc>(nameof(SlavicFgPrepareDesc.FrameId)),
            (nuint)Marshal.OffsetOf<SlavicFgPrepareDesc>(nameof(SlavicFgPrepareDesc.CameraPosition)),
            (nuint)Marshal.OffsetOf<SlavicFgDispatchDesc>(nameof(SlavicFgDispatchDesc.FrameId)),
            (nuint)Marshal.OffsetOf<SlavicFgDispatchDesc>(nameof(SlavicFgDispatchDesc.Reset))
        ];
        for (uint entry = 0; entry < expected.Length; entry++)
            if (layout(entry) != expected[entry])
                throw new InvalidOperationException($"FG provider ABI mismatch at entry {entry}.");
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nuint AbiLayoutDelegate(uint entry);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint IsSupportedDelegate(nint physicalDevice);

    private void DestroyContext()
    {
        if (_context != 0) _destroy(_context);
        _context = 0;
        _hasHistory = false;
        _resetRequested = true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _commands.WaitAll();
        DestroyContext();
        _commands.Dispose();
        _library.Dispose();
        _disposed = true;
    }
}
