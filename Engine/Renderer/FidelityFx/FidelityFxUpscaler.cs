using System.Runtime.InteropServices;
using SlavicGame.Engine.Diagnostics;
using Veldrid;

namespace SlavicGame.Engine.Renderer.FidelityFx;

public static class FidelityFxUpscalerPolicy
{
    public const string EnvironmentVariable = "SLAVICGAME_FSR3";

    public static bool IsRequested() =>
        string.Equals(
            Environment.GetEnvironmentVariable(EnvironmentVariable),
            "1",
            StringComparison.Ordinal) ||
        string.Equals(
            Environment.GetEnvironmentVariable(EnvironmentVariable),
            "true",
            StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Native AMD FSR 3.1.4 upscaler context for Vulkan. The path is deliberately
/// opt-in until runtime validation is complete on real hardware.
/// </summary>
public sealed class FidelityFxUpscaler : IDisposable
{
    private const uint VkImageLayoutGeneral = 1;
    private const uint VkImageLayoutShaderReadOnlyOptimal = 5;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly BackendInfoVulkan _vk;
    private readonly FidelityFxNativeLibrary _library;
    private readonly FidelityFxVulkanCommands _commands;
    private readonly FfxCreateContextDelegate _createContext;
    private readonly FfxDestroyContextDelegate _destroyContext;
    private readonly FfxDispatchDelegate _dispatch;

    private nint _contextMemory;
    private nint _backendDescriptorMemory;
    private nint _createDescriptorMemory;
    private uint _maxUpscaleWidth;
    private uint _maxUpscaleHeight;
    private bool _contextCreated;
    private bool _disposed;

    private FidelityFxUpscaler(
        GraphicsDevice graphicsDevice,
        FidelityFxNativeLibrary library)
    {
        _graphicsDevice = graphicsDevice;
        _vk = graphicsDevice.GetVulkanInfo();
        _library = library;

        var handles =
            FidelityFxVulkanInterop.GetDeviceHandles(graphicsDevice);
        _commands = new FidelityFxVulkanCommands(handles);

        _createContext =
            Marshal.GetDelegateForFunctionPointer<FfxCreateContextDelegate>(
                library.GetExport("ffxCreateContext"));
        _destroyContext =
            Marshal.GetDelegateForFunctionPointer<FfxDestroyContextDelegate>(
                library.GetExport("ffxDestroyContext"));
        _dispatch =
            Marshal.GetDelegateForFunctionPointer<FfxDispatchDelegate>(
                library.GetExport("ffxDispatch"));
    }

    public bool IsReady => _contextCreated && !_disposed;

    public static bool TryCreate(
        GraphicsDevice graphicsDevice,
        uint outputWidth,
        uint outputHeight,
        out FidelityFxUpscaler? upscaler,
        out string diagnostic)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        upscaler = null;

        if (!OperatingSystem.IsWindows())
        {
            diagnostic =
                "Native FSR3 dispatch is currently enabled only for the official " +
                "Windows/Vulkan AMD FidelityFX 1.1.4 provider.";
            return false;
        }

        if (graphicsDevice.BackendType != GraphicsBackend.Vulkan)
        {
            diagnostic = "FSR3 native dispatch requires Vulkan.";
            return false;
        }

        if (!FidelityFxNativeLibrary.TryLoad(
                out var library,
                out diagnostic) ||
            library is null)
        {
            return false;
        }

        try
        {
            var created = new FidelityFxUpscaler(
                graphicsDevice,
                library);
            created.CreateContext(outputWidth, outputHeight);
            upscaler = created;
            diagnostic =
                $"AMD FSR 3.1.4 Vulkan context ready at " +
                $"{outputWidth}x{outputHeight}.";
            return true;
        }
        catch (Exception ex)
        {
            library.Dispose();
            diagnostic =
                $"AMD FSR 3.1.4 context creation failed: {ex.Message}";
            return false;
        }
    }

    public void EnsureOutputSize(
        uint outputWidth,
        uint outputHeight)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        outputWidth = Math.Max(1u, outputWidth);
        outputHeight = Math.Max(1u, outputHeight);

        if (_contextCreated &&
            outputWidth <= _maxUpscaleWidth &&
            outputHeight <= _maxUpscaleHeight)
        {
            return;
        }

        _commands.WaitAll();
        DestroyContext();
        CreateContext(outputWidth, outputHeight);
    }

    public void Dispatch(
        Texture color,
        Texture depth,
        Texture motionVectors,
        Texture reactive,
        Texture output,
        in TemporalFrameData frame,
        float frameTimeSeconds,
        float cameraNear,
        float cameraFar,
        float verticalFov,
        float sharpness)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(color);
        ArgumentNullException.ThrowIfNull(depth);
        ArgumentNullException.ThrowIfNull(motionVectors);
        ArgumentNullException.ThrowIfNull(reactive);
        ArgumentNullException.ThrowIfNull(output);

        if (!_contextCreated)
            throw new InvalidOperationException(
                "FidelityFX upscaler context is not initialized.");

        EnsureOutputSize(output.Width, output.Height);

        // These transitions are queued after Veldrid's scene submission and
        // before the native FSR command buffer on the exact same VkQueue.
        _vk.TransitionImageLayout(
            color,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.TransitionImageLayout(
            depth,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.TransitionImageLayout(
            motionVectors,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.TransitionImageLayout(
            reactive,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.TransitionImageLayout(
            output,
            VkImageLayoutGeneral);

        var commandBuffer = _commands.Begin();
        var desc = new FfxDispatchDescUpscale
        {
            Header = new FfxApiHeader
            {
                Type = FfxApi.DispatchUpscale,
                Next = 0
            },
            CommandList = commandBuffer,
            Color = WrapTexture(
                color,
                FfxApi.ResourceUsageReadOnly,
                FfxApi.ResourceStateComputeRead),
            Depth = WrapTexture(
                depth,
                FfxApi.ResourceUsageDepthTarget,
                FfxApi.ResourceStateComputeRead),
            MotionVectors = WrapTexture(
                motionVectors,
                FfxApi.ResourceUsageReadOnly,
                FfxApi.ResourceStateComputeRead),
            Exposure = default,
            Reactive = WrapTexture(
                reactive,
                FfxApi.ResourceUsageReadOnly,
                FfxApi.ResourceStateComputeRead),
            TransparencyAndComposition = default,
            Output = WrapTexture(
                output,
                FfxApi.ResourceUsageUav,
                FfxApi.ResourceStateUnorderedAccess),
            // The renderer stores subpixel jitter in render-pixel units.
            // FSR uses the inverse Y convention relative to our camera jitter.
            JitterOffset = new FfxApiFloatCoords2D(
                frame.JitterPixels.X,
                -frame.JitterPixels.Y),
            // Our velocity texture is normalized UV displacement
            // (previous-current), so render size converts it to pixels.
            MotionVectorScale = new FfxApiFloatCoords2D(
                color.Width,
                color.Height),
            RenderSize = new FfxApiDimensions2D(
                color.Width,
                color.Height),
            UpscaleSize = new FfxApiDimensions2D(
                output.Width,
                output.Height),
            EnableSharpening = sharpness > 0.001f,
            Sharpness = Math.Clamp(sharpness, 0f, 1f),
            FrameTimeDelta =
                Math.Clamp(frameTimeSeconds * 1000f, 0.01f, 1000f),
            PreExposure = 1f,
            Reset = frame.ResetHistory,
            CameraNear = cameraNear,
            CameraFar = cameraFar,
            CameraFovAngleVertical = verticalFov,
            ViewSpaceToMetersFactor = 1f,
            Flags = 0
        };

        var descriptorMemory =
            Marshal.AllocHGlobal(
                Marshal.SizeOf<FfxDispatchDescUpscale>());
        try
        {
            Marshal.StructureToPtr(
                desc,
                descriptorMemory,
                false);
            var result = _dispatch(
                _contextMemory,
                descriptorMemory);

            // Even on an API error the provider may have recorded valid
            // cleanup/barrier work, so finish the command buffer before
            // reporting the error and advancing the ring.
            _commands.EndAndSubmit(commandBuffer);

            if (result != 0)
            {
                throw new InvalidOperationException(
                    $"ffxDispatch returned FfxApiReturnCode {result}.");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(descriptorMemory);
        }

        // FidelityFX leaves the output in UAV/general state. Keep Veldrid's
        // layout tracker synchronized before its display-resolution sampling
        // pass records the next transition.
        _vk.OverrideImageLayout(output, VkImageLayoutGeneral);
        _vk.OverrideImageLayout(
            color,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.OverrideImageLayout(
            depth,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.OverrideImageLayout(
            motionVectors,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.OverrideImageLayout(
            reactive,
            VkImageLayoutShaderReadOnlyOptimal);
    }

    private void CreateContext(
        uint outputWidth,
        uint outputHeight)
    {
        outputWidth = Math.Max(1u, outputWidth);
        outputHeight = Math.Max(1u, outputHeight);

        var handles =
            FidelityFxVulkanInterop.GetDeviceHandles(
                _graphicsDevice);

        _backendDescriptorMemory =
            Marshal.AllocHGlobal(
                Marshal.SizeOf<FfxCreateBackendVkDesc>());
        _createDescriptorMemory =
            Marshal.AllocHGlobal(
                Marshal.SizeOf<FfxCreateContextDescUpscale>());
        _contextMemory =
            Marshal.AllocHGlobal(nint.Size);
        Marshal.WriteIntPtr(_contextMemory, 0);

        var backend = new FfxCreateBackendVkDesc
        {
            Header = new FfxApiHeader
            {
                Type = FfxApi.CreateBackendVulkan,
                Next = 0
            },
            Device = handles.Device,
            PhysicalDevice = handles.PhysicalDevice,
            DeviceProcAddr = _commands.DeviceProcAddr
        };
        Marshal.StructureToPtr(
            backend,
            _backendDescriptorMemory,
            false);

        var create = new FfxCreateContextDescUpscale
        {
            Header = new FfxApiHeader
            {
                Type = FfxApi.CreateContextUpscale,
                Next = _backendDescriptorMemory
            },
            Flags = FfxApi.EnableAutoExposure,
            MaxRenderSize = new FfxApiDimensions2D(
                outputWidth,
                outputHeight),
            MaxUpscaleSize = new FfxApiDimensions2D(
                outputWidth,
                outputHeight),
            MessageCallback = 0
        };
        Marshal.StructureToPtr(
            create,
            _createDescriptorMemory,
            false);

        var result = _createContext(
            _contextMemory,
            _createDescriptorMemory,
            0);
        if (result != 0)
        {
            FreeContextMemory();
            throw new InvalidOperationException(
                $"ffxCreateContext returned FfxApiReturnCode {result}.");
        }

        _contextCreated = true;
        _maxUpscaleWidth = outputWidth;
        _maxUpscaleHeight = outputHeight;

        EngineLog.Info(
            $"AMD FidelityFX FSR 3.1.4 context created: " +
            $"maxRender/maxUpscale={outputWidth}x{outputHeight}.");
    }

    private FfxApiResource WrapTexture(
        Texture texture,
        uint usage,
        uint state)
    {
        var image =
            FidelityFxVulkanInterop.GetImageHandle(
                _graphicsDevice,
                texture);

        return new FfxApiResource
        {
            Resource = unchecked(
                (nint)(long)image.Image),
            Description = new FfxApiResourceDescription
            {
                Type = FfxApi.ResourceTypeTexture2D,
                Format = SurfaceFormat(texture.Format),
                Width = texture.Width,
                Height = texture.Height,
                Depth = Math.Max(1u, texture.ArrayLayers),
                MipCount = Math.Max(1u, texture.MipLevels),
                Flags = 0,
                Usage = usage
            },
            State = state
        };
    }

    private static uint SurfaceFormat(
        PixelFormat format) =>
        format switch
        {
            PixelFormat.R8_G8_B8_A8_UNorm =>
                FfxApi.FormatR8G8B8A8Unorm,
            PixelFormat.R8_G8_B8_A8_UNorm_SRgb =>
                FfxApi.FormatR8G8B8A8Srgb,
            PixelFormat.B8_G8_R8_A8_UNorm =>
                FfxApi.FormatB8G8R8A8Unorm,
            PixelFormat.B8_G8_R8_A8_UNorm_SRgb =>
                FfxApi.FormatB8G8R8A8Srgb,
            PixelFormat.R16_G16_B16_A16_Float =>
                FfxApi.FormatR16G16B16A16Float,
            PixelFormat.R16_G16_Float =>
                FfxApi.FormatR16G16Float,
            PixelFormat.R8_UNorm =>
                FfxApi.FormatR8Unorm,
            PixelFormat.R32_Float =>
                FfxApi.FormatR32Float,
            _ => throw new NotSupportedException(
                $"FidelityFX does not map Veldrid format {format}.")
        };

    private void DestroyContext()
    {
        if (_contextCreated && _contextMemory != 0)
        {
            var result = _destroyContext(
                _contextMemory,
                0);
            if (result != 0)
            {
                EngineLog.Warn(
                    $"ffxDestroyContext returned {result}.");
            }
        }

        _contextCreated = false;
        _maxUpscaleWidth = 0;
        _maxUpscaleHeight = 0;
        FreeContextMemory();
    }

    private void FreeContextMemory()
    {
        if (_contextMemory != 0)
        {
            Marshal.FreeHGlobal(_contextMemory);
            _contextMemory = 0;
        }
        if (_createDescriptorMemory != 0)
        {
            Marshal.FreeHGlobal(_createDescriptorMemory);
            _createDescriptorMemory = 0;
        }
        if (_backendDescriptorMemory != 0)
        {
            Marshal.FreeHGlobal(_backendDescriptorMemory);
            _backendDescriptorMemory = 0;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _commands.WaitAll();
        DestroyContext();
        _commands.Dispose();
        _library.Dispose();
    }
}
