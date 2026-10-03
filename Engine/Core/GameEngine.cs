using System.Diagnostics;
using System.Numerics;
using Veldrid;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.UI;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Core;

public sealed class GameEngine : IDisposable
{
    private readonly GameTime _time = new();
    private readonly GameWindow _window;
    private readonly VeldridRenderer _renderer = new();
    private readonly Camera3D _camera = new() { Mode = CameraMode.FirstPerson };
    private readonly WorldState _world = WorldGenerator.Generate();
    private readonly FrontendController _frontend = new();
    private readonly GameSettings _settings;
    private readonly GameSettingsStore _settingsStore;

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

    public GameEngine(
        EngineConfig config,
        GameSettings settings,
        GameSettingsStore settingsStore)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settingsStore);
        _settings = settings;
        _settingsStore = settingsStore;
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
        _renderer.Initialize(
            _window,
            _world,
            _vsync,
            _settings.TextureQuality,
            _settings.Msaa);
        ApplySettings();
        _camera.Follow(_world.PlayerPosition, 0f, _world.Terrain);
        _window.SetMouseCapture(false);
        _initialized = true;
        EngineLog.Info("Engine initialization complete.");
    }

    public void Run()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_initialized)
            throw new InvalidOperationException("Call Initialize() before Run().");

        var stopwatch = Stopwatch.StartNew();
        var previousSeconds = stopwatch.Elapsed.TotalSeconds;
        var loggedFirstFrame = false;

        while (_window.Exists)
        {
            var frameStartTimestamp = Stopwatch.GetTimestamp();

            _window.PumpEvents();
            if (!_window.Exists) break;

            var currentSeconds = stopwatch.Elapsed.TotalSeconds;
            var deltaSeconds = currentSeconds - previousSeconds;
            previousSeconds = currentSeconds;

            _time.Advance(deltaSeconds);
            UpdateFps(deltaSeconds);

            if (_frontend.IsPlaying)
            {
                if (_window.ConsumeKeyPress(Key.Escape))
                {
                    _frontend.OpenMainMenu();
                    _window.SetMouseCapture(false);
                }
                else
                {
                    if (_window.ConsumeKeyPress(Key.F11))
                    {
                        _settings.Fullscreen = !_window.IsFullscreen;
                        ApplySettings();
                        _settingsStore.Save(_settings);
                    }

                    HandleInput(_time.DeltaSeconds);
                    _world.Update(_time.DeltaSeconds);
                }
            }
            else
            {
                var action = _frontend.HandleInput(_window, _settings);
                switch (action)
                {
                    case FrontendAction.StartGame:
                        ApplySettings();
                        _window.SetMouseCapture(true);
                        break;

                    case FrontendAction.SettingsChanged:
                        ApplySettings();
                        _settingsStore.Save(_settings);
                        break;

                    case FrontendAction.Exit:
                        _window.Close();
                        continue;
                }
            }

            if (!loggedFirstFrame)
            {
                loggedFirstFrame = true;
                EngineLog.Info("Main loop is running.");
            }

            var menuView = _frontend.IsPlaying
                ? null
                : _frontend.BuildView(_settings);

            _renderer.Render(
                _world,
                _camera,
                _displayFps,
                _time.TotalSeconds,
                _settings,
                menuView);

            ApplyFrameRateLimit(frameStartTimestamp);
        }
    }

    private void ApplyFrameRateLimit(long frameStartTimestamp)
    {
        var targetFps =
            GraphicsQualityCatalog.FrameRate(_settings.FpsLimit);
        if (targetFps <= 0 || _settings.VSync)
            return;

        var targetSeconds = 1.0 / targetFps;

        while (true)
        {
            var elapsedSeconds =
                (Stopwatch.GetTimestamp() - frameStartTimestamp) /
                (double)Stopwatch.Frequency;
            var remaining = targetSeconds - elapsedSeconds;

            if (remaining <= 0)
                break;

            if (remaining > 0.0025)
            {
                Thread.Sleep(
                    Math.Max(
                        0,
                        (int)((remaining - 0.0015) * 1000.0)));
            }
            else
            {
                Thread.SpinWait(64);
            }
        }
    }

    private void ApplySettings()
    {
        _settings.Normalize();

        var manualRenderResolution = _settings.ResolutionSize;
        var windowResolution = _settings.WindowResolutionSize;

        if (_window.IsFullscreen != _settings.Fullscreen)
            _window.SetFullscreen(_settings.Fullscreen);

        if (!_settings.Fullscreen)
            _window.SetWindowedSize(
                windowResolution.Width,
                windowResolution.Height);

        var renderResolution =
            _settings.Upscaler == UpscalerMode.Fsr1
                ? GraphicsQualityCatalog.FsrRenderResolution(
                    Math.Max(1, _window.Width),
                    Math.Max(1, _window.Height),
                    _settings.FsrQuality,
                    manualRenderResolution)
                : manualRenderResolution;

        _renderer.SetRenderResolution(
            renderResolution.Width,
            renderResolution.Height);
        _renderer.SetShadowResolution(
            GraphicsQualityCatalog.ShadowMapSize(_settings.ShadowQuality));
        _renderer.SetShadowDistance(
            GraphicsQualityCatalog.ShadowDistance(_settings.ShadowDistance));
        _renderer.SetTextureQuality(_settings.TextureQuality);
        _renderer.SetVSync(_settings.VSync);

        _camera.FieldOfView =
            MathF.PI / 180f * _settings.FieldOfViewDegrees;
        _camera.FarPlane =
            GraphicsQualityCatalog.RenderDistance(_settings.RenderDistance) + 50f;
        _camera.MouseSensitivity =
            0.0035f * _settings.MouseSensitivity;
        _camera.VerticalSensitivity =
            0.0025f * _settings.MouseSensitivity;
        _camera.Mode = _settings.Camera == CameraPreference.FirstPerson
            ? CameraMode.FirstPerson
            : CameraMode.ThirdPerson;
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
