using System.Diagnostics;
using System.Numerics;
using Veldrid;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Core;

public sealed class GameEngine : IDisposable
{
    private readonly GameTime _time = new();
    private readonly GameWindow _window;
    private readonly VeldridRenderer _renderer = new();
    private readonly Camera3D _camera = new();
    private readonly WorldState _world = WorldGenerator.Generate();

    private bool _initialized;
    private bool _disposed;
    private readonly bool _vsync;
    private readonly bool _inputDiagnostics;
    private double _inputLogSeconds;
    private int _movingFrames;
    private int _movingLookFrames;
    private Vector2 _eventPixels;
    private Vector2 _polledPixels;
    private Vector2 _appliedPixels;
    private double _fpsAccumulator;
    private int _fpsFrames;
    private double _displayFps;

    public GameTime Time => _time;
    public GameWindow Window => _window;
    public VeldridRenderer Renderer => _renderer;
    public WorldState World => _world;

    public GameEngine(EngineConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _vsync = config.VSync;
        _inputDiagnostics = config.InputDiagnostics;
        _window = new GameWindow(config);
        _window.Resized += OnWindowResized;
        _window.Closing += () => EngineLog.Info("Closing SlavicGame.");
    }

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized)
        {
            return;
        }

        EngineLog.Info("Starting SlavicGame engine.");
        _renderer.Initialize(_window, _world, _vsync);
        _initialized = true;
        EngineLog.Info("Engine initialization complete.");
    }

    public void Run()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_initialized)
        {
            throw new InvalidOperationException("Call Initialize() before Run().");
        }

        var stopwatch = Stopwatch.StartNew();
        var previousSeconds = stopwatch.Elapsed.TotalSeconds;
        var loggedFirstFrame = false;

        while (_window.Exists)
        {
            _window.PumpEvents();
            if (!_window.Exists) break;
            if (_window.ConsumeKeyPress(Key.Escape)) break;

            var currentSeconds = stopwatch.Elapsed.TotalSeconds;
            var deltaSeconds = currentSeconds - previousSeconds;
            previousSeconds = currentSeconds;

            if (_window.ConsumeKeyPress(Key.F11))
            {
                _window.ToggleFullscreen();
            }

            _time.Advance(deltaSeconds);
            UpdateFps(deltaSeconds);
            HandleInput(_time.DeltaSeconds);
            _world.Update(_time.DeltaSeconds);

            if (!loggedFirstFrame)
            {
                loggedFirstFrame = true;
                EngineLog.Info("Main loop is running.");
            }

            _renderer.Render(_world, _camera, _displayFps, _time.TotalSeconds);
        }
    }

    private void UpdateFps(double deltaSeconds)
    {
        if (deltaSeconds <= 0)
        {
            return;
        }

        _fpsAccumulator += deltaSeconds;
        _fpsFrames++;

        if (_fpsAccumulator >= 0.5)
        {
            _displayFps = _fpsFrames / _fpsAccumulator;
            _fpsAccumulator = 0;
            _fpsFrames = 0;
        }
    }

    private void HandleInput(double deltaSeconds)
    {
        var input = new PlayerInput(
            _window.IsKeyDown(Key.W),
            _window.IsKeyDown(Key.S),
            _window.IsKeyDown(Key.D),
            _window.IsKeyDown(Key.A),
            _window.IsKeyDown(Key.ShiftLeft),
            _window.MouseDelta);
        PlayerController.Update(_world, _camera, input, deltaSeconds);
        if (_inputDiagnostics) LogInput(input, deltaSeconds);
    }

    private void LogInput(PlayerInput input, double deltaSeconds)
    {
        var moving = input.Forward || input.Backward || input.Right || input.Left;
        if (moving) _movingFrames++;
        if (moving && input.LookDelta != Vector2.Zero) _movingLookFrames++;
        _eventPixels += Vector2.Abs(_window.EventMouseDelta);
        _polledPixels += Vector2.Abs(_window.PolledMouseDelta);
        _appliedPixels += Vector2.Abs(input.LookDelta);
        _inputLogSeconds += deltaSeconds;
        if (_inputLogSeconds < 0.5) return;

        EngineLog.Info($"INPUT v3 focus={_window.NativeWindow.Focused} relative={_window.RelativeMouseEnabled} " +
            $"movingFrames={_movingFrames} movingLookFrames={_movingLookFrames} " +
            $"eventPixels={_eventPixels} polledPixels={_polledPixels} appliedPixels={_appliedPixels} " +
            $"yaw={_camera.Yaw:F3} pitch={_camera.Pitch:F3}");
        _inputLogSeconds = 0;
        _movingFrames = _movingLookFrames = 0;
        _eventPixels = _polledPixels = _appliedPixels = Vector2.Zero;
    }

    private void OnWindowResized()
    {
        if (!_initialized)
        {
            return;
        }

        _renderer.Resize((uint)Math.Max(1, _window.Width), (uint)Math.Max(1, _window.Height));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _renderer.Dispose(); }
        finally { _window.Dispose(); }
    }
}
