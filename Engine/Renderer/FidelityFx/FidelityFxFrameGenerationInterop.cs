using System.Runtime.InteropServices;

namespace SlavicGame.Engine.Renderer.FidelityFx;

internal static class FidelityFxFrameGenerationNative
{
    public const uint JitterMotionVectors = 1u << 2;
    public const uint BackBufferTransferSrgb = 0u;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SlavicFgCreateDesc
{
    public nint VkDevice;
    public nint VkPhysicalDevice;
    public nint VkDeviceProcAddr;
    public uint MaxRenderWidth;
    public uint MaxRenderHeight;
    public uint DisplayWidth;
    public uint DisplayHeight;
    public uint BackBufferFormat;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SlavicFgFloat3
{
    public float X;
    public float Y;
    public float Z;

    public SlavicFgFloat3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
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
    public SlavicFgFloat3 CameraPosition;
    public SlavicFgFloat3 CameraUp;
    public SlavicFgFloat3 CameraRight;
    public SlavicFgFloat3 CameraForward;
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
    ref SlavicFgCreateDesc description,
    out nint context);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint SlavicFgPrepareDelegate(
    nint context,
    ref SlavicFgPrepareDesc description);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint SlavicFgDispatchDelegate(
    nint context,
    ref SlavicFgDispatchDesc description);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void SlavicFgDestroyDelegate(nint context);
