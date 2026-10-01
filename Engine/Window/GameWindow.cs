using System.Numerics;
using System.Runtime.InteropServices;
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
    private readonly HashSet<Key> _keysPressed = [];

    public Sdl2Window NativeWindow => _window;
    public bool Exists => _window.Exists;
    public int Width => _window.Bounds.Width;
    public int Height => _window.Bounds.Height;
    public Vector2 MouseDelta => _window.MouseDelta;
    public bool IsFullscreen =>
        _window.WindowState == WindowState.FullScreen ||
        _window.WindowState == WindowState.BorderlessFullScreen;

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
        _window.FocusGained += OnFocusGained;
        _window.FocusLost += OnFocusLost;

        SetRelativeMouseMode(true);
        EngineLog.Info($"Created SDL2 window {Width}x{Height} ({_window.WindowState}).");
    }

    public void PumpEvents()
    {
        _window.PumpEvents();
    }

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);

    public bool ConsumeKeyPress(Key key) => _keysPressed.Remove(key);

    public void ToggleFullscreen()
    {
        _window.WindowState = IsFullscreen
            ? WindowState.Normal
            : WindowState.BorderlessFullScreen;

        SetRelativeMouseMode(true);
        EngineLog.Info($"Fullscreen: {IsFullscreen}.");
    }

    private void OnFocusGained()
    {
        SetRelativeMouseMode(true);
    }

    private void OnFocusLost()
    {
        SetRelativeMouseMode(false);
    }

    private void SetRelativeMouseMode(bool enabled)
    {
        // Veldrid's Sdl2Window already reports relative mouse motion through
        // MouseDelta (SDL mouse motion xrel/yrel). Do not call SDL directly
        // here: mixing another SDL P/Invoke path can interfere with Veldrid's
        // event processing and make camera input appear blocked while moving.
        _window.CursorVisible = !enabled;
    }

    private void OnKeyDown(KeyEvent keyEvent)
    {
        if (_keysDown.Add(keyEvent.Key))
        {
            _keysPressed.Add(keyEvent.Key);
        }
    }

    private void OnKeyUp(KeyEvent keyEvent) => _keysDown.Remove(keyEvent.Key);

    public void Dispose()
    {
        try
        {
            SetRelativeMouseMode(false);
        }
        finally
        {
            _window.Close();
        }
    }

    private static class Sdl2NativeCompat
    {
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetRelativeMouseMode(byte enabled);

        public static int SetRelativeMouseMode(bool enabled)
            => SDL_SetRelativeMouseMode(enabled ? (byte)1 : (byte)0);

        [DllImport("libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetRelativeMouseMode")]
        private static extern int SDL_SetRelativeMouseModeLinux(byte enabled);

        public static int SetRelativeMouseModeLinux(bool enabled)
            => SDL_SetRelativeMouseModeLinux(enabled ? (byte)1 : (byte)0);
    }
}
