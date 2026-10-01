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
    private bool _relativeMouseEnabled;
    private bool _discardMouseDelta;
    private readonly HashSet<Key> _keysDown = [];
    private readonly HashSet<Key> _keysPressed = [];

    public Sdl2Window NativeWindow => _window;
    public bool Exists => _window.Exists;
    public int Width => _window.Bounds.Width;
    public int Height => _window.Bounds.Height;
    public Vector2 MouseDelta { get; private set; }
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
        if (!_window.Exists) return;

        // Read relative motion once per frame; keyboard repeat events do not consume it.
        var delta = _relativeMouseEnabled
            ? Sdl2NativeCompat.GetRelativeMouseDelta()
            : _window.MouseDelta;
        MouseDelta = _window.Focused && !_discardMouseDelta ? delta : Vector2.Zero;
        _discardMouseDelta = false;
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
        MouseDelta = Vector2.Zero;
        _discardMouseDelta = true;
        try
        {
            var result = Sdl2NativeCompat.SetRelativeMouseMode(enabled);

            if (result != 0)
            {
                EngineLog.Warn($"SDL relative mouse mode returned {result}.");
            }

            _relativeMouseEnabled = enabled && result == 0;
            _window.CursorVisible = !_relativeMouseEnabled;
        }
        catch (InvalidOperationException exception)
        {
            _relativeMouseEnabled = false;
            _window.CursorVisible = true;
            EngineLog.Warn($"SDL relative mouse input unavailable: {exception.Message}");
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
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetRelativeMouseModeDelegate(int enabled);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint GetRelativeMouseStateDelegate(out int x, out int y);

        // Resolve both functions through Veldrid's loader so the window, capture mode
        // and relative-motion accumulator always use the same SDL library instance.
        private static readonly SetRelativeMouseModeDelegate? SetMode =
            Sdl2Native.LoadFunction<SetRelativeMouseModeDelegate>("SDL_SetRelativeMouseMode");
        private static readonly GetRelativeMouseStateDelegate? GetState =
            Sdl2Native.LoadFunction<GetRelativeMouseStateDelegate>("SDL_GetRelativeMouseState");

        public static int SetRelativeMouseMode(bool enabled)
        {
            if (SetMode is null || GetState is null)
                throw new InvalidOperationException("SDL relative mouse functions could not be loaded.");
            return SetMode(enabled ? 1 : 0);
        }

        public static Vector2 GetRelativeMouseDelta()
        {
            if (GetState is null) return Vector2.Zero;
            GetState(out var x, out var y);
            return new Vector2(x, y);
        }
    }
}
