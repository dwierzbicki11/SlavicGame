using System.Diagnostics;
using System.Numerics;
using Veldrid;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Audio;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Save;
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
    private readonly SaveSlotService _saveSlots = new();
    private VoiceOverService? _voice;
    private readonly VoiceUsageScope _voiceUsage = VoiceUsage.FromEnvironment();

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
    private double _playTimeSeconds;
    private double _autosaveSeconds;
    private bool _settingsReapplyPending;
    private bool _applyingSettings;
    private string? _voicedCinematicId;
    private int _voicedCinematicShot = -1;
    private const double AutosaveIntervalSeconds = 120.0;

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
        _voice ??= VoiceOverService.CreateFromEnvironment();
        _renderer.Initialize(
            _window,
            _world,
            _vsync,
            _settings.TextureQuality,
            _settings.Msaa);
        ApplySettings();
        _camera.Follow(_world.PlayerPosition, 0f, _world.Terrain);
        _window.SetMouseCapture(false);
        RefreshContinueMenu();
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

            if (_settingsReapplyPending && !_applyingSettings)
            {
                _settingsReapplyPending = false;
                ApplySettings();
            }

            var currentSeconds = stopwatch.Elapsed.TotalSeconds;
            var deltaSeconds = currentSeconds - previousSeconds;
            previousSeconds = currentSeconds;

            _time.Advance(deltaSeconds);
            UpdateFps(deltaSeconds);

            if (_frontend.IsPlaying)
            {
                if (_world.Cinematics.IsPlaying)
                {
                    if (_window.ConsumeKeyPress(Key.Escape) || _window.ConsumeKeyPress(Key.Space))
                        _world.Cinematics.Finish(_world);
                    else
                        _world.Cinematics.Update(_world, _time.DeltaSeconds);
                    if (_world.Cinematics.IsPlaying)
                        _camera.SetCinematicPose(_world.Cinematics.CameraPosition, _world.Cinematics.CameraTarget);
                    else
                        _camera.ResumeFollow(_world.PlayerPosition, _world.Terrain);
                }
                else if (_window.ConsumeKeyPress(Key.Escape))
                {
                    TryAutosave("pause");
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
                    if (!_world.Cinematics.IsPlaying)
                    {
                        _world.Update(_time.DeltaSeconds);
                        _world.Melee.Update(
                            _world,
                            _camera.GetMoveForward(),
                            _time.DeltaSeconds);
                        _world.Magic.Update(_world, _time.DeltaSeconds);
                        _world.Rituals.Update(_world, _time.DeltaSeconds);

                        if (_world.Rituals.ConsumeCompletionSignal())
                            _world.Cinematics.TryStartById(_world, "contract-resolution");

                        if (!_world.Cinematics.IsPlaying &&
                            Vector3.Distance(_world.PlayerPosition, new Vector3(-85f, _world.PlayerPosition.Y, 55f)) < 18f)
                            _world.Cinematics.TryStart(_world, CinematicPlayer.Shrine);
                    }
                    if (_world.Cinematics.IsPlaying)
                        _camera.SetCinematicPose(_world.Cinematics.CameraPosition, _world.Cinematics.CameraTarget);

                    _playTimeSeconds += _time.DeltaSeconds;
                    _autosaveSeconds += _time.DeltaSeconds;
                    if (_autosaveSeconds >= AutosaveIntervalSeconds)
                    {
                        TryAutosave("interval");
                    }
                }
            }
            else
            {
                var action = _frontend.HandleInput(_window, _settings);
                switch (action)
                {
                    case FrontendAction.StartGame:
                        _world.Cinematics.TryStart(_world, CinematicPlayer.Arrival);
                        if (_world.Cinematics.IsPlaying)
                            _camera.SetCinematicPose(_world.Cinematics.CameraPosition, _world.Cinematics.CameraTarget);
                        ApplySettings();
                        _window.SetMouseCapture(true);
                        break;

                    case FrontendAction.ContinueGame:
                        if (_saveSlots.TryLoadLatest(_world, out var loaded) &&
                            loaded is not null)
                        {
                            _playTimeSeconds = loaded.Slot.PlayTimeSeconds;
                            _autosaveSeconds = 0.0;
                            _camera.Follow(
                                _world.PlayerPosition,
                                0f,
                                _world.Terrain);
                            ApplySettings();
                            _window.SetMouseCapture(true);
                            RefreshContinueMenu();
                        }
                        else
                        {
                            EngineLog.Warn("No valid autosave could be loaded.");
                            _frontend.CancelLoadedSession();
                            RefreshContinueMenu();
                            _window.SetMouseCapture(false);
                        }
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

            SyncCinematicVoice();

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

    private void TryAutosave(string reason)
    {
        try
        {
            _saveSlots.SaveAutosave(
                _world,
                _playTimeSeconds);
            _autosaveSeconds = 0.0;
            RefreshContinueMenu();
            EngineLog.Info($"Autosave completed ({reason}).");
        }
        catch (Exception exception)
        {
            EngineLog.Warn(
                $"Autosave failed ({reason}): {exception.Message}");
        }
    }

    private void RefreshContinueMenu()
    {
        var latest = _saveSlots
            .ListAutosaves()
            .FirstOrDefault();

        if (latest is null)
        {
            _frontend.SetContinueInfo(false);
            return;
        }

        var localTime = latest.SavedAtUtc.ToLocalTime();
        var playMinutes = (int)Math.Floor(
            latest.PlayTimeSeconds / 60.0);

        _frontend.SetContinueInfo(
            true,
            $"OSTATNI ZAPIS {localTime:dd.MM HH:mm}  CZAS {playMinutes} MIN");
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
        if (_applyingSettings)
            return;

        _applyingSettings = true;
        try
        {
            _settings.Normalize();

            var manualRenderResolution = _settings.ResolutionSize;
            var windowResolution = _settings.WindowResolutionSize;

            if (_window.IsFullscreen != _settings.Fullscreen)
                _window.SetFullscreen(_settings.Fullscreen);

            if (!_settings.Fullscreen)
            {
                _window.SetWindowedSize(
                    windowResolution.Width,
                    windowResolution.Height);
            }

            // SDL can deliver the real borderless-fullscreen dimensions on a
            // resize event after WindowState changes. OnWindowResized schedules
            // a second apply, so FSR/internal resolution is always recomputed
            // from the final drawable size rather than a stale window size.
            var outputWidth = Math.Max(1, _window.Width);
            var outputHeight = Math.Max(1, _window.Height);

            var renderResolution =
                _settings.Upscaler == UpscalerMode.Fsr1
                    ? GraphicsQualityCatalog.FsrRenderResolution(
                        outputWidth,
                        outputHeight,
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

            EngineLog.Info(
                $"Settings applied: output={outputWidth}x{outputHeight}, " +
                $"internal={renderResolution.Width}x{renderResolution.Height}, " +
                $"upscaler={_settings.Upscaler}/{_settings.FsrQuality}, " +
                $"preset={GraphicsPresetCatalog.DetectName(_settings)}.");
        }
        finally
        {
            _applyingSettings = false;
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
        if (_window.ConsumeKeyPress(Key.Q) && !_world.Rituals.IsPerforming)
            _world.Magic.SelectNext(_world);

        if (_window.ConsumeKeyPress(Key.L) && !_world.Rituals.IsPerforming && !_world.Magic.IsCasting)
            _world.SpellLearning.TryLearnCurrent(_world);

        if (_window.ConsumeKeyPress(Key.E) &&
            !_world.Rituals.IsPerforming &&
            !_world.Magic.IsCasting &&
            !_world.Cinematics.IsPlaying)
        {
            _world.QuestInteractions.TryInteract(_world);
        }

        if (_window.ConsumeKeyPress(Key.F) && !_world.Rituals.IsPerforming)
        {
            var spell = _world.Magic.Current;
            if (_world.Magic.TryStart(_world, _camera.GetLookDirection()))
            {
                _voice?.Speak(BuildSpellVoiceRequest(spell));
            }
        }

        if (_window.ConsumeKeyPress(Key.R) && !_world.Rituals.IsPerforming)
            _world.Rituals.TryStart(_world);

        if (_window.ConsumeLeftMousePress())
            _world.Melee.TryStart(_world);

        if (_window.ConsumeKeyPress(Key.C) && !_world.Rituals.IsPerforming)
        {
            _world.Cinematics.TryStart(_world, CinematicPlayer.Arrival);
            if (_world.Cinematics.IsPlaying) return;
        }
        var canMove = !_world.Magic.IsCasting && !_world.Rituals.IsPerforming;
        var input = new PlayerInput(
            canMove && _window.IsKeyDown(Key.W),
            canMove && _window.IsKeyDown(Key.S),
            canMove && _window.IsKeyDown(Key.D),
            canMove && _window.IsKeyDown(Key.A),
            canMove && _window.IsKeyDown(Key.ShiftLeft),
            _window.MouseDelta);
        PlayerController.Update(_world, _camera, input, deltaSeconds);
        if (_inputDiagnostics) LogInput(input, deltaSeconds);
    }

    private static VoiceRequest BuildSpellVoiceRequest(SlavicGame.Engine.Magic.CastSpell spell)
    {
        var direction = spell.Effect switch
        {
            SlavicGame.Engine.Magic.SpellEffect.Spark => new VoiceDirection(
                VoiceEmotion.Urgent,
                0.82f,
                1.08f,
                "protagonist casting combat magic; short, forceful and controlled"),
            SlavicGame.Engine.Magic.SpellEffect.Mend => new VoiceDirection(
                VoiceEmotion.Solemn,
                0.58f,
                0.90f,
                "protagonist casting restorative magic; steady breath and focused concentration"),
            SlavicGame.Engine.Magic.SpellEffect.Reveal => new VoiceDirection(
                VoiceEmotion.Mystical,
                0.74f,
                0.84f,
                "protagonist casting perception magic; low ritual delivery, nearly whispered"),
            _ => new VoiceDirection(
                VoiceEmotion.Mystical,
                0.65f,
                0.92f,
                "protagonist speaking a deliberate magical incantation")
        };

        return new VoiceRequest(
            $"magic.{spell.Id}",
            spell.Incantation,
            direction,
            VoiceUsage.ProtagonistVoiceId);
    }

    private void SyncCinematicVoice()
    {
        if (_voice is null || _voiceUsage != VoiceUsageScope.All)
            return;

        var cinematics = _world.Cinematics;
        if (!cinematics.IsPlaying)
        {
            if (_voicedCinematicId is not null)
            {
                _voice.Stop();
                _voicedCinematicId = null;
                _voicedCinematicShot = -1;
            }
            return;
        }

        var activeId = cinematics.ActiveId;
        var shot = cinematics.CurrentShot;
        if (activeId is null || shot is null)
            return;

        if (_voicedCinematicId == activeId &&
            _voicedCinematicShot == cinematics.ShotIndex)
            return;

        _voicedCinematicId = activeId;
        _voicedCinematicShot = cinematics.ShotIndex;

        _voice.Speak(new VoiceRequest(
            $"cinematic.{activeId}.{cinematics.ShotIndex}",
            shot.Subtitle,
            shot.Voice ?? new VoiceDirection(
                VoiceEmotion.Solemn,
                0.5f,
                0.95f,
                "cinematic narration; natural human delivery")));
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

        _renderer.Resize(
            (uint)Math.Max(1, _window.Width),
            (uint)Math.Max(1, _window.Height));

        // Recompute internal/FSR resolution after SDL/Vulkan has committed the
        // new drawable size. This is especially important for borderless
        // fullscreen where the monitor resolution arrives asynchronously.
        _settingsReapplyPending = true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _voice?.Dispose();
            _renderer.Dispose();
        }
        finally
        {
            _window.Dispose();
        }
    }
}
