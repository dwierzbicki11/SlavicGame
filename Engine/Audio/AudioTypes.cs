namespace SlavicGame.Engine.Audio;

public enum AudioBus
{
    Master,
    Music,
    Ambience,
    Effects,
    Voice,
    Ui
}

public sealed record AudioCue(
    string Id,
    AudioBus Bus,
    float DefaultVolume = 1f,
    bool Loop = false)
{
    public AudioCue Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        if (!float.IsFinite(DefaultVolume) || DefaultVolume < 0f || DefaultVolume > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(DefaultVolume));
        }

        return this;
    }
}

public sealed class AudioSettings
{
    private readonly Dictionary<AudioBus, float> _volumes = [];

    public float GetVolume(AudioBus bus) => _volumes.GetValueOrDefault(bus, 1f);

    public void SetVolume(AudioBus bus, float volume)
    {
        if (!float.IsFinite(volume) || volume < 0f || volume > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(volume));
        }

        _volumes[bus] = volume;
    }
}
