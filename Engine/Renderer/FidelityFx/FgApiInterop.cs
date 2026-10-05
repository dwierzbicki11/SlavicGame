using System.Numerics;
using System.Runtime.InteropServices;

namespace SlavicGame.Engine.Renderer.FidelityFx;

// Matches frame_generation.h. All fields are blittable, including the native
// uint32_t reset flag; no per-dispatch unmanaged allocation is needed.
[StructLayout(LayoutKind.Sequential)]
internal struct SlavicFgCreateDesc
{
    public nint Device;
    public nint PhysicalDevice;
    public nint DeviceProcAddr;
    public uint MaxRenderWidth;
    public uint MaxRenderHeight;
    public uint DisplayWidth;
    public uint DisplayHeight;
    public uint BackBufferFormat;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SlavicFgPrepareDesc
{
    public nint CommandList;
    public FfxApiResource Depth;
    public FfxApiResource MotionVectors;
    public uint RenderWidth;
    public uint RenderHeight;
    public float JitterX;
    public float JitterY;
    public float MotionVectorScaleX;
    public float MotionVectorScaleY;
    public float FrameTimeDelta;
    public float CameraNear;
    public float CameraFar;
    public float ViewSpaceToMetersFactor;
    public float CameraFovAngleVertical;
    public ulong FrameId;
    public Vector3 CameraPosition;
    public Vector3 CameraUp;
    public Vector3 CameraRight;
    public Vector3 CameraForward;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SlavicFgDispatchDesc
{
    public nint CommandList;
    public FfxApiResource CurrentBackBuffer;
    public FfxApiResource CurrentBackBufferHudless;
    public FfxApiResource Output;
    public ulong FrameId;
    public uint Reset;
    public uint BackBufferTransferFunction;
    public float MinLuminance;
    public float MaxLuminance;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint SlavicFgCreateDelegate(
    in SlavicFgCreateDesc descriptor, ref nint context);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint SlavicFgPrepareDelegate(
    nint context, in SlavicFgPrepareDesc descriptor);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint SlavicFgDispatchDelegate(
    nint context, in SlavicFgDispatchDesc descriptor);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void SlavicFgDestroyDelegate(nint context);
