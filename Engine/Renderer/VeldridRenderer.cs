using Veldrid;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public sealed class VeldridRenderer : IDisposable
{
    private GraphicsDevice? _graphicsDevice;
    private CommandList? _commandList;

    public GraphicsDevice GraphicsDevice =>
        _graphicsDevice ?? throw new InvalidOperationException("Renderer has not been initialized.");

    public void Initialize(GameWindow window, bool vsync)
    {
        if (_graphicsDevice is not null)
        {
            return;
        }

        var options = new GraphicsDeviceOptions
        {
            Debug = false,
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true
        };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(
            window.NativeWindow,
            options,
            GraphicsBackend.Vulkan);

        _graphicsDevice.SyncToVerticalBlank = vsync;
        _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

        EngineLog.Info($"Veldrid renderer initialized with {_graphicsDevice.BackendType}.");
        EngineLog.Info($"Graphics device: {_graphicsDevice.DeviceName}.");
    }

    public void Render(WorldTime worldTime)
    {
        if (_graphicsDevice is null || _commandList is null)
        {
            throw new InvalidOperationException("Renderer has not been initialized.");
        }

        var framebuffer = _graphicsDevice.SwapchainFramebuffer;
        if (framebuffer is null)
        {
            return;
        }

        _commandList.Begin();
        _commandList.SetFramebuffer(framebuffer);
        var atmosphere = GetAtmosphereColor(worldTime);
        _commandList.ClearColorTarget(0, atmosphere);
        _commandList.End();

        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }

    private static RgbaFloat GetAtmosphereColor(WorldTime time)
    {
        if (time.IsNight)
        {
            return new RgbaFloat(0.012f, 0.018f, 0.035f, 1f);
        }

        var daylight = (float)Math.Clamp(Math.Sin((time.TimeOfDayHours - 6.0) / 14.0 * Math.PI), 0.0, 1.0);
        return new RgbaFloat(
            0.025f + daylight * 0.055f,
            0.045f + daylight * 0.075f,
            0.065f + daylight * 0.095f,
            1f);
    }

    public void Resize(uint width, uint height)
    {
        if (_graphicsDevice is null || width == 0 || height == 0)
        {
            return;
        }

        _graphicsDevice.ResizeMainWindow(width, height);
    }

    public void Dispose()
    {
        if (_graphicsDevice is null)
        {
            return;
        }

        _graphicsDevice.WaitForIdle();
        _commandList?.Dispose();
        _graphicsDevice.Dispose();
        _commandList = null;
        _graphicsDevice = null;
    }
}
