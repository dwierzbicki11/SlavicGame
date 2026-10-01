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
    private bool _disposed;
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
        ArgumentNullException.ThrowIfNull(config);
        if (config.Width <= 0 || config.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(config), "Window dimensions must be positive.");
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
        _keysPressed.Clear();
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
        _keysDown.Clear();
        _keysPressed.Clear();
        SetRelativeMouseMode(false);
    }

    private void SetRelativeMouseMode(bool enabled)
    {
        // Veldrid exposes SDL's relative mouse delta through MouseDelta.
        // We only enable/disable SDL relative mode here; the delta itself
        // always comes from Veldrid so keyboard and mouse processing stay
        // independent.
        try
        {
            var result = Sdl2NativeCompat.SetRelativeMouseMode(enabled);

            if (result != 0)
            {
                EngineLog.Warn($"SDL relative mouse mode returned {result}.");
            }

            _window.CursorVisible = !enabled;
        }
        catch (DllNotFoundException exception)
        {
            _window.CursorVisible = true;
            EngineLog.Warn($"SDL2 library not found: {exception.Message}");
        }
        catch (EntryPointNotFoundException exception)
        {
            _window.CursorVisible = true;
            EngineLog.Warn($"SDL_SetRelativeMouseMode not found: {exception.Message}");
        }
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
        if (_disposed) return;
        _disposed = true;
        if (!_window.Exists) return;
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
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetRelativeMouseMode")]
        private static extern int SetRelativeMouseModeWindows(int enabled);

        [DllImport("libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetRelativeMouseMode")]
        private static extern int SetRelativeMouseModeLinux(int enabled);

        public static int SetRelativeMouseMode(bool enabled)
            => OperatingSystem.IsLinux()
                ? SetRelativeMouseModeLinux(enabled ? 1 : 0)
                : SetRelativeMouseModeWindows(enabled ? 1 : 0);
    }
}
