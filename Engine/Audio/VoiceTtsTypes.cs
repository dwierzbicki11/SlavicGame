namespace SlavicGame.Engine.Audio;

public enum VoiceEmotion
{
    Neutral,
    Calm,
    Warm,
    Uneasy,
    Fearful,
    Angry,
    Sad,
    Whisper,
    Solemn,
    Mystical,
    Urgent
}

public sealed record VoiceDirection(
    VoiceEmotion Emotion = VoiceEmotion.Neutral,
    float Intensity = 0.55f,
    float Speed = 1f,
    string? Persona = null)
{
    public VoiceDirection Validate()
    {
        if (!float.IsFinite(Intensity) || Intensity < 0f || Intensity > 1f)
            throw new ArgumentOutOfRangeException(nameof(Intensity));
        if (!float.IsFinite(Speed) || Speed < 0.5f || Speed > 1.5f)
            throw new ArgumentOutOfRangeException(nameof(Speed));
        return this;
    }
}

public sealed record VoiceRequest(
    string Id,
    string Text,
    VoiceDirection Direction,
    string? Voice = null)
{
    public VoiceRequest Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(Text);
        Direction.Validate();
        return this;
    }
}

public sealed record PcmAudio(
    byte[] Data,
    int SampleRate = 24000,
    int Channels = 1,
    int BitsPerSample = 16)
{
    public PcmAudio Validate()
    {
        ArgumentNullException.ThrowIfNull(Data);
        if (Data.Length == 0) throw new ArgumentException("PCM audio is empty.", nameof(Data));
        if (SampleRate <= 0 || Channels <= 0 || BitsPerSample != 16)
            throw new ArgumentOutOfRangeException(nameof(SampleRate), "Unsupported PCM format.");
        return this;
    }
}

public interface ITextToSpeechProvider
{
    Task<PcmAudio> SynthesizeAsync(VoiceRequest request, CancellationToken cancellationToken);
}

public enum VoiceUsageScope
{
    SpellsOnly,
    All
}

public static class VoiceUsage
{
    public static VoiceUsageScope FromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable("SLAVICGAME_TTS_SCOPE");
        return string.Equals(value, "all", StringComparison.OrdinalIgnoreCase)
            ? VoiceUsageScope.All
            : VoiceUsageScope.SpellsOnly;
    }

    public static string ProtagonistVoiceId =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SLAVICGAME_PLAYER_VOICE"))
            ? "protagonist"
            : Environment.GetEnvironmentVariable("SLAVICGAME_PLAYER_VOICE")!.Trim();
}
