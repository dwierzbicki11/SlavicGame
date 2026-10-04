using System.Numerics;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Audio;

public sealed class RiverAmbienceService : IDisposable
{
    private readonly SdlPcmPlayer? _player;
    private double _refreshSeconds;
    private int _segmentIndex;
    private bool _disposed;
    private bool _playing;
    private double _splashCooldownSeconds;

    private RiverAmbienceService(SdlPcmPlayer? player)
    {
        _player = player;
    }

    public bool IsEnabled => _player is not null;

    public static RiverAmbienceService TryCreate()
    {
        try
        {
            var player = SdlPcmPlayer.TryCreate("river ambience");
            if (player is not null)
                EngineLog.Info("Procedural river ambience enabled.");
            return new RiverAmbienceService(player);
        }
        catch (Exception exception)
        {
            EngineLog.Warn($"River ambience disabled: {exception.Message}");
            return new RiverAmbienceService(null);
        }
    }

    public void Update(
        Vector3 listener,
        double deltaSeconds,
        bool enabled,
        float splashIntensity = 0f,
        float volume = 1f)
    {
        if (_disposed || _player is null)
            return;

        if (!enabled)
        {
            Stop();
            return;
        }

        var distance = RiverAmbienceSynthesizer.DistanceToRiver(listener);
        var attenuation = RiverAmbienceSynthesizer.Attenuation(distance);
        var targetVolume =
            attenuation *
            0.34f *
            Math.Clamp(volume, 0f, 1f);

        if (targetVolume <= 0.012f)
        {
            Stop();
            return;
        }

        var safeDelta = Math.Max(0d, deltaSeconds);
        _refreshSeconds -= safeDelta;
        _splashCooldownSeconds -= safeDelta;

        var splashTriggered =
            splashIntensity >= 0.45f &&
            _splashCooldownSeconds <= 0d;

        if (_playing &&
            _refreshSeconds > 0d &&
            !splashTriggered)
        {
            return;
        }

        var audio = RiverAmbienceSynthesizer.Generate(
            targetVolume,
            _segmentIndex++,
            splashTriggered ? splashIntensity : 0f);
        _player.Play(audio);
        _playing = true;
        _refreshSeconds =
            RiverAmbienceSynthesizer.SegmentSeconds - 0.08;

        if (splashTriggered)
            _splashCooldownSeconds = 0.22d;
    }

    public void Stop()
    {
        if (_disposed || _player is null || !_playing)
            return;

        _player.Clear();
        _playing = false;
        _refreshSeconds = 0d;
        _splashCooldownSeconds = 0d;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _player?.Dispose();
    }
}
