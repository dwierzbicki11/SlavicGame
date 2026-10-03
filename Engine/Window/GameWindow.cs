using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Input;

namespace SlavicGame.Engine.Windowing;

public sealed class GameWindow : IDisposable
{
    private readonly Sdl2Window _window;
    private bool _disposed;
    private bool _relativeMouseEnabled;
    private bool _discardMouseDelta;
    private bool _relativeMouseRequested;
    private readonly HashSet<Key> _keysDown = [];
    private readonly HashSet<Key> _keysPressed = [];
    private uint _mouseButtonsDown;
    private uint _mouseButtonsPressed;
    private uint _mouseButtonsReleased;
    private const uint LeftMouseMask = 1u;
    private const uint RightMouseMask = 4u;

    public Sdl2Window NativeWindow => _window;
    public bool Exists => _window.Exists;
    public int Width => _window.Bounds.Width;
    public int Height => _window.Bounds.Height;
    public Vector2 MouseDelta { get; private set; }
    public Vector2 EventMouseDelta { get; private set; }
    public Vector2 PolledMouseDelta { get; private set; }
    public bool RelativeMouseEnabled => _relativeMouseEnabled;
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

        if (config.UseMouseWarp)
            Sdl2Native.SDL_SetHint("SDL_MOUSE_RELATIVE_MODE_WARP", "1");
        _relativeMouseRequested = false;
        SetRelativeMouseMode(false);
        EngineLog.Info($"Input pipeline v3; mouse capture: {(config.UseMouseWarp ? "warp" : "SDL default")}.");
        EngineLog.Info($"Created SDL2 window {Width}x{Height} ({_window.WindowState}).");
    }

    public void PumpEvents()
    {
        _keysPressed.Clear();
        _mouseButtonsPressed = 0u;
        _mouseButtonsReleased = 0u;
        _window.PumpEvents();
        if (!_window.Exists) return;

        // Sample both sources once per frame without double-counting their motion.
        EventMouseDelta = _window.MouseDelta;
        var relativeSample = Sdl2NativeCompat.GetRelativeMouseSample();
        PolledMouseDelta = _relativeMouseEnabled ? relativeSample.Delta : Vector2.Zero;
        var delta = MouseMotion.Select(EventMouseDelta, PolledMouseDelta);
        MouseDelta = _window.Focused && !_discardMouseDelta ? delta : Vector2.Zero;
        UpdateMouseButtons(relativeSample.Buttons);
        _discardMouseDelta = false;
    }

    public bool IsKeyDown(Key key) => _keysDown.Contains(key);

    public bool ConsumeKeyPress(Key key) => _keysPressed.Remove(key);

    public bool IsLeftMouseDown =>
        (_mouseButtonsDown & LeftMouseMask) != 0u;

    public bool IsRightMouseDown =>
        (_mouseButtonsDown & RightMouseMask) != 0u;

    public bool ConsumeLeftMousePress()
    {
        if ((_mouseButtonsPressed & LeftMouseMask) == 0u)
            return false;

        _mouseButtonsPressed &= ~LeftMouseMask;
        return true;
    }

    public bool ConsumeLeftMouseRelease()
    {
        if ((_mouseButtonsReleased & LeftMouseMask) == 0u)
            return false;

        _mouseButtonsReleased &= ~LeftMouseMask;
        return true;
    }

    public void ToggleFullscreen() => SetFullscreen(!IsFullscreen);

    public void SetFullscreen(bool enabled)
    {
        _window.WindowState = enabled
            ? WindowState.BorderlessFullScreen
            : WindowState.Normal;

        EngineLog.Info($"Fullscreen: {IsFullscreen}.");
    }

    public void SetWindowedSize(int width, int height)
    {
        if (IsFullscreen)
            return;

        width = Math.Clamp(width, 640, 7680);
        height = Math.Clamp(height, 360, 4320);

        if (_window.Width == width && _window.Height == height)
            return;

        _window.Width = width;
        _window.Height = height;
        EngineLog.Info($"Windowed resolution: {width}x{height}.");
    }

    public void SetMouseCapture(bool enabled)
    {
        _relativeMouseRequested = enabled;
        SetRelativeMouseMode(enabled && _window.Focused);
    }

    public void Close() => _window.Close();

    private void OnFocusGained()
    {
        SetRelativeMouseMode(_relativeMouseRequested);
    }

    private void OnFocusLost()
    {
        _keysDown.Clear();
        _keysPressed.Clear();
        _mouseButtonsDown = 0u;
        _mouseButtonsPressed = 0u;
        _mouseButtonsReleased = 0u;
        SetRelativeMouseMode(false);
    }

    private void UpdateMouseButtons(uint buttons)
    {
        _mouseButtonsPressed |= buttons & ~_mouseButtonsDown;
        _mouseButtonsReleased |= _mouseButtonsDown & ~buttons;
        _mouseButtonsDown = buttons;
    }

    private void SetRelativeMouseMode(bool enabled)
    {
        // Reacquiring existing capture must not discard this frame's mouse movement.
        if (Sdl2NativeCompat.IsRelativeMouseModeEnabled() == enabled &&
            _relativeMouseEnabled == enabled)
            return;
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
        private delegate int GetRelativeMouseModeDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint GetRelativeMouseStateDelegate(out int x, out int y);

        // Resolve both functions through Veldrid's loader so the window, capture mode
        // and relative-motion accumulator always use the same SDL library instance.
        private static readonly SetRelativeMouseModeDelegate? SetMode =
            Sdl2Native.LoadFunction<SetRelativeMouseModeDelegate>("SDL_SetRelativeMouseMode");
        private static readonly GetRelativeMouseModeDelegate? GetMode =
            Sdl2Native.LoadFunction<GetRelativeMouseModeDelegate>("SDL_GetRelativeMouseMode");
        private static readonly GetRelativeMouseStateDelegate? GetState =
            Sdl2Native.LoadFunction<GetRelativeMouseStateDelegate>("SDL_GetRelativeMouseState");

        public static bool IsRelativeMouseModeEnabled() => GetMode is not null && GetMode() != 0;

        public static int SetRelativeMouseMode(bool enabled)
        {
            if (SetMode is null || GetState is null)
                throw new InvalidOperationException("SDL relative mouse functions could not be loaded.");
            return SetMode(enabled ? 1 : 0);
        }

        public static (Vector2 Delta, uint Buttons) GetRelativeMouseSample()
        {
            if (GetState is null) return (Vector2.Zero, 0u);
            var buttons = GetState(out var x, out var y);
            return (new Vector2(x, y), buttons);
        }
    }
}
