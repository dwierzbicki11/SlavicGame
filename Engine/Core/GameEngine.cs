using System.Diagnostics;
using System.Numerics;
using Veldrid;
using SlavicGame.Engine.Diagnostics;
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

            _renderer.Render(_world.Time, _camera, _displayFps);
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
        var mouse = _window.MouseDelta;
        _camera.Rotate(mouse.X, mouse.Y);

        var move = Vector3.Zero;
        if (_window.IsKeyDown(Key.W)) move += _camera.GetMoveForward();
        if (_window.IsKeyDown(Key.S)) move -= _camera.GetMoveForward();
        if (_window.IsKeyDown(Key.D)) move += _camera.GetMoveRight();
        if (_window.IsKeyDown(Key.A)) move -= _camera.GetMoveRight();

        if (move.LengthSquared() > 0.001f)
        {
            move = Vector3.Normalize(move);
            var speed = _window.IsKeyDown(Key.ShiftLeft) ? 9f : 5f;
            _world.SetPlayerPosition(_world.PlayerPosition + move * speed * (float)deltaSeconds);
        }

        _camera.Follow(_world.PlayerPosition, (float)deltaSeconds);
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
