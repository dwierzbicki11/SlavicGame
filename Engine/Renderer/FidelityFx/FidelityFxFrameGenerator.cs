using System.Numerics;
using System.Runtime.InteropServices;
using SlavicGame.Engine.Diagnostics;
using Veldrid;

namespace SlavicGame.Engine.Renderer.FidelityFx;

public static class FidelityFxFrameGeneratorPolicy
{
    public const string EnvironmentVariable = "SLAVICGAME_FSR3_FG";

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
/// Experimental FSR 3.1.4 Frame Generation runtime. This stage deliberately
/// generates an offscreen frame only; presentation/frame pacing is integrated
/// separately after the compute path has proven stable in the game loop.
/// </summary>
public sealed class FidelityFxFrameGenerator : IDisposable
{
    private const uint VkImageLayoutGeneral = 1;
    private const uint VkImageLayoutShaderReadOnlyOptimal = 5;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly BackendInfoVulkan _vk;
    private readonly FidelityFxNativeLibrary _library;
    private readonly FidelityFxVulkanCommands _commands;
    private readonly SlavicFgCreateDelegate _create;
    private readonly SlavicFgPrepareDelegate _prepare;
    private readonly SlavicFgDispatchDelegate _dispatch;
    private readonly SlavicFgDestroyDelegate _destroy;

    private nint _context;
    private Texture? _generatedTexture;
    private TextureView? _generatedView;
    private uint _renderWidth;
    private uint _renderHeight;
    private uint _displayWidth;
    private uint _displayHeight;
    private bool _disposed;

    private FidelityFxFrameGenerator(
        GraphicsDevice graphicsDevice,
        FidelityFxNativeLibrary library)
    {
        _graphicsDevice = graphicsDevice;
        _vk = graphicsDevice.GetVulkanInfo();
        _library = library;

        var handles = FidelityFxVulkanInterop.GetDeviceHandles(graphicsDevice);
        _commands = new FidelityFxVulkanCommands(handles);

        _create = Marshal.GetDelegateForFunctionPointer<SlavicFgCreateDelegate>(
            library.GetExport("slavicFgCreate"));
        _prepare = Marshal.GetDelegateForFunctionPointer<SlavicFgPrepareDelegate>(
            library.GetExport("slavicFgPrepare"));
        _dispatch = Marshal.GetDelegateForFunctionPointer<SlavicFgDispatchDelegate>(
            library.GetExport("slavicFgDispatch"));
        _destroy = Marshal.GetDelegateForFunctionPointer<SlavicFgDestroyDelegate>(
            library.GetExport("slavicFgDestroy"));
    }

    public bool IsReady =>
        !_disposed &&
        _context != 0 &&
        _generatedTexture is not null;

    public Texture GeneratedTexture =>
        _generatedTexture ??
        throw new InvalidOperationException(
            "FSR3 Frame Generation output is not initialized.");

    public TextureView GeneratedView =>
        _generatedView ??
        throw new InvalidOperationException(
            "FSR3 Frame Generation output view is not initialized.");

    public static bool TryCreate(
        GraphicsDevice graphicsDevice,
        uint renderWidth,
        uint renderHeight,
        uint displayWidth,
        uint displayHeight,
        out FidelityFxFrameGenerator? frameGenerator,
        out string diagnostic)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        frameGenerator = null;

        if (!OperatingSystem.IsLinux())
        {
            diagnostic =
                "FSR3 Frame Generation game-loop runtime currently uses the " +
                "source-built Linux Vulkan provider; Windows integration is a later stage.";
            return false;
        }

        if (graphicsDevice.BackendType != GraphicsBackend.Vulkan)
        {
            diagnostic = "FSR3 Frame Generation requires Vulkan.";
            return false;
        }

        if (!FidelityFxNativeLibrary.TryLoad(
                out var library,
                out diagnostic) ||
            library is null)
        {
            return false;
        }

        FidelityFxFrameGenerator? created = null;
        try
        {
            created = new FidelityFxFrameGenerator(
                graphicsDevice,
                library);
            created.CreateContext(
                renderWidth,
                renderHeight,
                displayWidth,
                displayHeight);
            frameGenerator = created;
            diagnostic =
                $"AMD FSR 3.1.4 Frame Generation offscreen runtime ready: " +
                $"render={renderWidth}x{renderHeight}, " +
                $"display={displayWidth}x{displayHeight}.";
            return true;
        }
        catch (Exception ex)
        {
            if (created is not null)
                created.Dispose();
            else
                library.Dispose();

            diagnostic =
                "AMD FSR3 Frame Generation runtime creation failed: " +
                ex.Message;
            return false;
        }
    }

    public void EnsureDimensions(
        uint renderWidth,
        uint renderHeight,
        uint displayWidth,
        uint displayHeight)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        renderWidth = Math.Max(1u, renderWidth);
        renderHeight = Math.Max(1u, renderHeight);
        displayWidth = Math.Max(1u, displayWidth);
        displayHeight = Math.Max(1u, displayHeight);

        if (_context != 0 &&
            _renderWidth == renderWidth &&
            _renderHeight == renderHeight &&
            _displayWidth == displayWidth &&
            _displayHeight == displayHeight)
        {
            return;
        }

        _commands.WaitAll();
        _graphicsDevice.WaitForIdle();
        DestroyContext();
        CreateContext(
            renderWidth,
            renderHeight,
            displayWidth,
            displayHeight);
    }

    public void Dispatch(
        Texture depth,
        Texture motionVectors,
        Texture presentationColor,
        in TemporalFrameData frame,
        Camera3D camera,
        float frameTimeSeconds)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(depth);
        ArgumentNullException.ThrowIfNull(motionVectors);
        ArgumentNullException.ThrowIfNull(presentationColor);
        ArgumentNullException.ThrowIfNull(camera);

        if (!IsReady)
            throw new InvalidOperationException(
                "FSR3 Frame Generation context is not initialized.");

        EnsureDimensions(
            depth.Width,
            depth.Height,
            presentationColor.Width,
            presentationColor.Height);

        _vk.TransitionImageLayout(
            depth,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.TransitionImageLayout(
            motionVectors,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.TransitionImageLayout(
            presentationColor,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.TransitionImageLayout(
            GeneratedTexture,
            VkImageLayoutGeneral);

        var forward = camera.GetLookDirection();
        var right = Vector3.Cross(forward, Vector3.UnitY);
        if (right.LengthSquared() < 0.000001f)
            right = Vector3.UnitX;
        else
            right = Vector3.Normalize(right);
        var up = Vector3.Normalize(Vector3.Cross(right, forward));

        var commandBuffer = _commands.Begin();
        var prepare = new SlavicFgPrepareDesc
        {
            CommandList = commandBuffer,
            Depth = WrapTexture(
                depth,
                FfxApi.ResourceUsageDepthTarget,
                FfxApi.ResourceStateComputeRead),
            MotionVectors = WrapTexture(
                motionVectors,
                FfxApi.ResourceUsageReadOnly,
                FfxApi.ResourceStateComputeRead),
            RenderWidth = depth.Width,
            RenderHeight = depth.Height,
            JitterX = frame.JitterPixels.X,
            JitterY = -frame.JitterPixels.Y,
            MotionVectorScaleX = depth.Width,
            MotionVectorScaleY = depth.Height,
            FrameTimeDelta =
                Math.Clamp(frameTimeSeconds * 1000f, 0.01f, 1000f),
            CameraNear = camera.NearPlane,
            CameraFar = camera.FarPlane,
            ViewSpaceToMetersFactor = 1f,
            CameraFovAngleVertical = camera.FieldOfView,
            FrameId = frame.FrameIndex,
            CameraPosition = Float3(camera.Position),
            CameraUp = Float3(up),
            CameraRight = Float3(right),
            CameraForward = Float3(forward)
        };

        var result = _prepare(_context, ref prepare);
        if (result != 0)
        {
            _commands.EndAndSubmit(commandBuffer);
            throw new InvalidOperationException(
                $"slavicFgPrepare returned FfxErrorCode {result}.");
        }

        var dispatch = new SlavicFgDispatchDesc
        {
            CommandList = commandBuffer,
            // FSR3 output is deliberately HUD-less at this point in the
            // renderer. This keeps UI out of optical-flow interpolation.
            CurrentBackBuffer = WrapTexture(
                presentationColor,
                FfxApi.ResourceUsageReadOnly,
                FfxApi.ResourceStateComputeRead),
            CurrentBackBufferHudless = default,
            Output = WrapTexture(
                GeneratedTexture,
                FfxApi.ResourceUsageUav,
                FfxApi.ResourceStateUnorderedAccess),
            FrameId = frame.FrameIndex,
            Reset = frame.ResetHistory ? 1u : 0u,
            BackBufferTransferFunction =
                FidelityFxFrameGenerationNative.BackBufferTransferSrgb,
            MinLuminance = 0f,
            MaxLuminance = 1f
        };

        result = _dispatch(_context, ref dispatch);
        _commands.EndAndSubmit(commandBuffer);

        if (result != 0)
            throw new InvalidOperationException(
                $"slavicFgDispatch returned FfxErrorCode {result}.");

        _vk.OverrideImageLayout(
            depth,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.OverrideImageLayout(
            motionVectors,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.OverrideImageLayout(
            presentationColor,
            VkImageLayoutShaderReadOnlyOptimal);
        _vk.OverrideImageLayout(
            GeneratedTexture,
            VkImageLayoutGeneral);
    }

    private void CreateContext(
        uint renderWidth,
        uint renderHeight,
        uint displayWidth,
        uint displayHeight)
    {
        renderWidth = Math.Max(1u, renderWidth);
        renderHeight = Math.Max(1u, renderHeight);
        displayWidth = Math.Max(1u, displayWidth);
        displayHeight = Math.Max(1u, displayHeight);

        var handles =
            FidelityFxVulkanInterop.GetDeviceHandles(_graphicsDevice);

        var create = new SlavicFgCreateDesc
        {
            VkDevice = handles.Device,
            VkPhysicalDevice = handles.PhysicalDevice,
            VkDeviceProcAddr = _commands.DeviceProcAddr,
            MaxRenderWidth = renderWidth,
            MaxRenderHeight = renderHeight,
            DisplayWidth = displayWidth,
            DisplayHeight = displayHeight,
            BackBufferFormat = FfxApi.FormatR16G16B16A16Float,
            Flags = FidelityFxFrameGenerationNative.JitterMotionVectors
        };

        var result = _create(ref create, out _context);
        if (result != 0 || _context == 0)
        {
            _context = 0;
            throw new InvalidOperationException(
                $"slavicFgCreate returned FfxErrorCode {result}.");
        }

        var factory = _graphicsDevice.ResourceFactory;
        _generatedTexture = factory.CreateTexture(
            TextureDescription.Texture2D(
                displayWidth,
                displayHeight,
                mipLevels: 1,
                arrayLayers: 1,
                PixelFormat.R16_G16_B16_A16_Float,
                TextureUsage.Sampled | TextureUsage.Storage));
        _generatedView = factory.CreateTextureView(_generatedTexture);

        _renderWidth = renderWidth;
        _renderHeight = renderHeight;
        _displayWidth = displayWidth;
        _displayHeight = displayHeight;

        EngineLog.Info(
            $"FSR3 Frame Generation offscreen target: " +
            $"{displayWidth}x{displayHeight} RGBA16F.");
    }

    private void DestroyContext()
    {
        _generatedView?.Dispose();
        _generatedTexture?.Dispose();
        _generatedView = null;
        _generatedTexture = null;

        if (_context != 0)
        {
            _destroy(_context);
            _context = 0;
        }

        _renderWidth = 0;
        _renderHeight = 0;
        _displayWidth = 0;
        _displayHeight = 0;
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
            Resource = unchecked((nint)(long)image.Image),
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

    private static uint SurfaceFormat(PixelFormat format) =>
        format switch
        {
            PixelFormat.R16_G16_B16_A16_Float =>
                FfxApi.FormatR16G16B16A16Float,
            PixelFormat.R16_G16_Float =>
                FfxApi.FormatR16G16Float,
            PixelFormat.R32_Float =>
                FfxApi.FormatR32Float,
            _ => throw new NotSupportedException(
                $"FSR3 Frame Generation does not map Veldrid format {format}.")
        };

    private static SlavicFgFloat3 Float3(Vector3 value) =>
        new(value.X, value.Y, value.Z);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _commands.WaitAll();
        _graphicsDevice.WaitForIdle();
        DestroyContext();
        _commands.Dispose();
        _library.Dispose();
    }
}
