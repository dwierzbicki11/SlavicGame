using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Windowing;

public sealed class GameWindow : IDisposable
{
    private readonly Sdl2Window _window;

    public Sdl2Window NativeWindow => _window;
    public bool Exists => _window.Exists;
    public int Width => _window.Bounds.Width;
    public int Height => _window.Bounds.Height;

    public event Action? Resized;
    public event Action? Closing;

    public GameWindow(EngineConfig config)
    {
        var windowCreateInfo = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = config.Width,
            WindowHeight = config.Height,
            WindowTitle = config.WindowTitle
        };

        _window = VeldridStartup.CreateWindow(ref windowCreateInfo);
        _window.Resized += () => Resized?.Invoke();
        _window.Closing += () => Closing?.Invoke();

        EngineLog.Info($"Created SDL2 window {Width}x{Height}.");
    }

    public void PumpEvents()
    {
        _window.PumpEvents();
    }

    public void Dispose()
    {
        _window.Close();
    }
}
