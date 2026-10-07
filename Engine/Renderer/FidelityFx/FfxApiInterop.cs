using System.Runtime.InteropServices;

namespace SlavicGame.Engine.Renderer.FidelityFx;

internal static class FfxApi
{
    public const ulong CreateContextUpscale = 0x00010000u;
    public const ulong DispatchUpscale = 0x00010001u;
    public const ulong CreateBackendVulkan = 0x00000003u;

    public const uint EnableMotionVectorsJitterCancellation = 1u << 2;
    public const uint EnableAutoExposure = 1u << 5;
    public const uint EnableDynamicResolution = 1u << 6;
    public const uint EnableNonLinearColorspace = 1u << 8;
    public const uint DispatchNonLinearColorSrgb = 1u << 1;
    public const uint ResourceUsageReadOnly = 0u;
    public const uint ResourceUsageUav = 1u << 1;
    public const uint ResourceUsageDepthTarget = 1u << 2;

    public const uint ResourceStateCommon = 1u << 0;
    public const uint ResourceStateUnorderedAccess = 1u << 1;
    public const uint ResourceStateComputeRead = 1u << 2;

    public const uint ResourceTypeTexture2D = 2u;

    public const uint FormatUnknown = 0u;
    public const uint FormatR16G16B16A16Float = 4u;
    public const uint FormatR8G8B8A8Unorm = 10u;
    public const uint FormatR8G8B8A8Srgb = 12u;
    public const uint FormatB8G8R8A8Unorm = 14u;
    public const uint FormatB8G8R8A8Srgb = 15u;
    public const uint FormatR16G16Float = 18u;
    public const uint FormatR8Unorm = 25u;
    public const uint FormatR32Float = 28u;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxApiHeader
{
    public ulong Type;
    public nint Next;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxApiDimensions2D
{
    public uint Width;
    public uint Height;

    public FfxApiDimensions2D(uint width, uint height)
    {
        Width = width;
        Height = height;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxApiFloatCoords2D
{
    public float X;
    public float Y;

    public FfxApiFloatCoords2D(float x, float y)
    {
        X = x;
        Y = y;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxApiResourceDescription
{
    public uint Type;
    public uint Format;
    public uint Width;
    public uint Height;
    public uint Depth;
    public uint MipCount;
    public uint Flags;
    public uint Usage;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxApiResource
{
    public nint Resource;
    public FfxApiResourceDescription Description;
    public uint State;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxCreateBackendVkDesc
{
    public FfxApiHeader Header;
    public nint Device;
    public nint PhysicalDevice;
    public nint DeviceProcAddr;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxCreateContextDescUpscale
{
    public FfxApiHeader Header;
    public uint Flags;
    public FfxApiDimensions2D MaxRenderSize;
    public FfxApiDimensions2D MaxUpscaleSize;
    public nint MessageCallback;
}

[StructLayout(LayoutKind.Sequential)]
internal struct FfxDispatchDescUpscale
{
    public FfxApiHeader Header;
    public nint CommandList;
    public FfxApiResource Color;
    public FfxApiResource Depth;
    public FfxApiResource MotionVectors;
    public FfxApiResource Exposure;
    public FfxApiResource Reactive;
    public FfxApiResource TransparencyAndComposition;
    public FfxApiResource Output;
    public FfxApiFloatCoords2D JitterOffset;
    public FfxApiFloatCoords2D MotionVectorScale;
    public FfxApiDimensions2D RenderSize;
    public FfxApiDimensions2D UpscaleSize;

    [MarshalAs(UnmanagedType.I1)]
    public bool EnableSharpening;

    public float Sharpness;
    public float FrameTimeDelta;
    public float PreExposure;

    [MarshalAs(UnmanagedType.I1)]
    public bool Reset;

    public float CameraNear;
    public float CameraFar;
    public float CameraFovAngleVertical;
    public float ViewSpaceToMetersFactor;
    public uint Flags;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint FfxCreateContextDelegate(
    nint context,
    nint descriptor,
    nint allocationCallbacks);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint FfxDestroyContextDelegate(
    nint context,
    nint allocationCallbacks);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate uint FfxDispatchDelegate(
    nint context,
    nint descriptor);
