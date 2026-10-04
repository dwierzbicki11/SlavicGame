using SlavicGame.Engine.AI;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Audio;

public sealed class AudioDirector : IDisposable
{
    private const string DayMusic = "assets/audio/music/r0_day_woodland.wav";
    private const string NightMusic = "assets/audio/music/r0_night_marsh.wav";
    private const string CombatMusic = "assets/audio/music/combat_predator.wav";
    private const string RitualMusic = "assets/audio/music/ritual_threshold.wav";

    private static readonly IReadOnlyDictionary<string, string> EffectAssets =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.confirm"] = "assets/audio/sfx/ui_confirm.wav",
            ["ui.back"] = "assets/audio/sfx/ui_back.wav",
            ["melee.swing"] = "assets/audio/sfx/melee_swing.wav",
            ["bow.draw"] = "assets/audio/sfx/bow_draw.wav",
            ["bow.release"] = "assets/audio/sfx/bow_release.wav",
            ["magic.cast"] = "assets/audio/sfx/magic_cast.wav",
            ["ritual.start"] = "assets/audio/sfx/ritual_start.wav",
            ["interaction.pickup"] = "assets/audio/sfx/interaction_pickup.wav",
            ["water.splash"] = "assets/audio/sfx/water_splash.wav",
            ["impact.hit"] = "assets/audio/sfx/impact_hit.wav",
            ["jump"] = "assets/audio/sfx/jump.wav"
        };

    private readonly SdlPcmPlayer? _musicPlayer;
    private readonly SdlPcmPlayer?[] _effectPlayers;
    private readonly SdlPcmPlayer? _uiPlayer;
    private readonly Dictionary<string, PcmAudio> _cache =
        new(StringComparer.Ordinal);

    private string? _currentMusic;
    private double _musicRemaining;
    private double _waterSfxCooldown;
    private int _effectChannel;
    private bool _disposed;

    private AudioDirector(
        SdlPcmPlayer? musicPlayer,
        SdlPcmPlayer?[] effectPlayers,
        SdlPcmPlayer? uiPlayer)
    {
        _musicPlayer = musicPlayer;
        _effectPlayers = effectPlayers;
        _uiPlayer = uiPlayer;
    }

    public bool IsEnabled =>
        _musicPlayer is not null ||
        _effectPlayers.Any(player => player is not null) ||
        _uiPlayer is not null;

    public static AudioDirector TryCreate()
    {
        try
        {
            var director = new AudioDirector(
                SdlPcmPlayer.TryCreate("music"),
                [
                    SdlPcmPlayer.TryCreate("effects channel A"),
                    SdlPcmPlayer.TryCreate("effects channel B")
                ],
                SdlPcmPlayer.TryCreate("UI effects"));

            if (director.IsEnabled)
                EngineLog.Info("SlavicGame music/SFX director enabled.");

            return director;
        }
        catch (Exception exception)
        {
            EngineLog.Warn(
                $"Music/SFX director partially disabled: {exception.Message}");

            return new AudioDirector(null, [null, null], null);
        }
    }

    public void Update(
        WorldState world,
        double deltaSeconds,
        bool playing,
        GameSettings settings)
    {
        if (_disposed)
            return;

        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(settings);

        var safeDelta = Math.Max(0d, deltaSeconds);
        _musicRemaining -= safeDelta;
        _waterSfxCooldown -= safeDelta;

        var targetMusic = SelectMusic(world, playing);
        if (!string.Equals(
                targetMusic,
                _currentMusic,
                StringComparison.Ordinal) ||
            _musicRemaining <= 0.08d)
        {
            StartMusic(targetMusic, settings);
        }

        if (playing &&
            world.WaterInteraction.SplashPulse >= 0.55f &&
            _waterSfxCooldown <= 0d)
        {
            PlayEffect("water.splash", settings);
            _waterSfxCooldown = 0.22d;
        }
    }

    public void PlayEffect(
        string effectId,
        GameSettings settings,
        float gain = 1f)
    {
        if (_disposed ||
            !EffectAssets.TryGetValue(effectId, out var path))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(settings);

        var volume =
            settings.MasterVolume *
            settings.EffectsVolume *
            Math.Clamp(gain, 0f, 1f);

        var audio = WavPcmLoader.ApplyGain(
            Load(path),
            volume);

        var available = _effectPlayers
            .Where(player => player is not null)
            .ToArray();

        if (available.Length == 0)
            return;

        _effectChannel =
            (_effectChannel + 1) % available.Length;
        available[_effectChannel]!.Play(audio);
    }

    public void PlayUi(
        string effectId,
        GameSettings settings)
    {
        if (_disposed || _uiPlayer is null ||
            !EffectAssets.TryGetValue(effectId, out var path))
        {
            return;
        }

        var volume =
            settings.MasterVolume *
            settings.UiVolume;

        _uiPlayer.Play(
            WavPcmLoader.ApplyGain(
                Load(path),
                volume));
    }

    public void StopMusic()
    {
        _musicPlayer?.Clear();
        _currentMusic = null;
        _musicRemaining = 0d;
    }

    private void StartMusic(
        string path,
        GameSettings settings)
    {
        if (_musicPlayer is null)
            return;

        var source = Load(path);
        var volume =
            settings.MasterVolume *
            settings.MusicVolume;

        _musicPlayer.Play(
            WavPcmLoader.ApplyGain(
                source,
                volume));

        _currentMusic = path;
        _musicRemaining =
            WavPcmLoader.DurationSeconds(source);
    }

    private PcmAudio Load(string path)
    {
        if (_cache.TryGetValue(path, out var cached))
            return cached;

        var audio = WavPcmLoader.LoadAsset(path);
        _cache[path] = audio;
        return audio;
    }

    private static string SelectMusic(
        WorldState world,
        bool playing)
    {
        if (!playing)
            return DayMusic;

        if (world.Rituals.IsPerforming)
            return RitualMusic;

        var inCombat = world.Enemies.Any(enemy =>
            enemy.IsAlive &&
            (enemy.State is EnemyState.Alert or
                            EnemyState.Chase or
                            EnemyState.Attack) &&
            System.Numerics.Vector3.DistanceSquared(
                enemy.Position,
                world.PlayerPosition) <= 45f * 45f);

        if (inCombat)
            return CombatMusic;

        return world.Time.IsNight
            ? NightMusic
            : DayMusic;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _musicPlayer?.Dispose();

        foreach (var player in _effectPlayers)
            player?.Dispose();

        _uiPlayer?.Dispose();
        _cache.Clear();
    }
}
