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
    private double _fpsAccumulator;
    private int _fpsFrames;
    private double _displayFps;
    private bool _mouseLookActive;

    public GameTime Time => _time;
    public GameWindow Window => _window;
    public VeldridRenderer Renderer => _renderer;
    public WorldState World => _world;

    public GameEngine(EngineConfig config)
    {
        _window = new GameWindow(config);
        _window.Resized += OnWindowResized;
        _window.Closing += () => EngineLog.Info("Closing SlavicGame.");
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        EngineLog.Info("Starting SlavicGame engine.");
        _renderer.Initialize(_window, _world, true);
        _initialized = true;
        EngineLog.Info("Engine initialization complete.");
    }

    public void Run()
    {
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

            var currentSeconds = stopwatch.Elapsed.TotalSeconds;
            var deltaSeconds = currentSeconds - previousSeconds;
            previousSeconds = currentSeconds;

            if (_window.ConsumeKeyPress(Key.F11))
            {
                _window.ToggleFullscreen();
            }

            _time.Advance(deltaSeconds);
            UpdateFps(deltaSeconds);
            HandleInput(deltaSeconds);
            _world.Update(deltaSeconds);

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
        UpdateMouseLook();

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
    }

    private void UpdateMouseLook()
    {
        var wantsMouseLook = _window.IsMouseButtonDown(MouseButton.Right);

        if (wantsMouseLook && !_mouseLookActive)
        {
            _mouseLookActive = true;
            _window.SetMouseLookActive(true);
        }
        else if (!wantsMouseLook && _mouseLookActive)
        {
            _mouseLookActive = false;
            _window.SetMouseLookActive(false);
        }

        if (!_mouseLookActive)
        {
            return;
        }

        var mouse = _window.MouseDelta;
        _camera.Update(_world.PlayerPosition, mouse.X, mouse.Y);
        _window.CenterMouse();
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
        _renderer.Dispose();
        _window.Dispose();
    }
}
