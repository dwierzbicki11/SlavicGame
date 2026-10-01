using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Windowing;

public sealed class GameWindow : IDisposable
{
    private readonly Sdl2Window _window;
    private readonly HashSet<Key> _keysDown = [];

    public Sdl2Window NativeWindow => _window;
    public bool Exists => _window.Exists;
    public int Width => _window.Bounds.Width;
    public int Height => _window.Bounds.Height;
    public Vector2 MouseDelta => _window.MouseDelta;

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
            WindowTitle = config.WindowTitle,
            WindowInitialState = config.Fullscreen
                ? WindowState.BorderlessFullScreen
                : WindowState.Normal
        };

        _window = VeldridStartup.CreateWindow(ref windowCreateInfo);
        _window.KeyDown += OnKeyDown;
        _window.KeyUp += OnKeyUp;
        _window.Resized += () => Resized?.Invoke();
        _window.Closing += () => Closing?.Invoke();
        _window.CursorVisible = false;

        EngineLog.Info($"Created SDL2 window {Width}x{Height} ({_window.WindowState}).");
    }

    public void PumpEvents() => _window.PumpEvents();

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);

    private void OnKeyDown(KeyEvent keyEvent) => _keysDown.Add(keyEvent.Key);
    private void OnKeyUp(KeyEvent keyEvent) => _keysDown.Remove(keyEvent.Key);

    public void Dispose() => _window.Close();
}
