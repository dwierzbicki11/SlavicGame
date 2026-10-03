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

public static class VoiceDirectionPrompts
{
    public static string Build(VoiceDirection direction)
    {
        direction.Validate();
        var intensity = direction.Intensity switch
        {
            < 0.25f => "very subtle",
            < 0.5f => "restrained",
            < 0.75f => "clearly expressive",
            _ => "strong but believable"
        };

        var emotion = direction.Emotion switch
        {
            VoiceEmotion.Calm => "calm, grounded and reassuring",
            VoiceEmotion.Warm => "warm, human and compassionate",
            VoiceEmotion.Uneasy => "uneasy and watchful, with slight tension",
            VoiceEmotion.Fearful => "fearful and tense without melodrama",
            VoiceEmotion.Angry => "angry and controlled, with sharp emphasis",
            VoiceEmotion.Sad => "sad and subdued, with genuine weight",
            VoiceEmotion.Whisper => "quiet, intimate and close to a whisper",
            VoiceEmotion.Solemn => "solemn, grave and ceremonial",
            VoiceEmotion.Mystical => "mysterious, deliberate and ritual-like",
            VoiceEmotion.Urgent => "urgent and focused, breathing slightly faster",
            _ => "natural and conversational"
        };

        var persona = string.IsNullOrWhiteSpace(direction.Persona)
            ? ""
            : $" Character direction: {direction.Persona.Trim()}.";

        return
            "Speak in natural Polish. Sound like a skilled human voice actor, never like a navigation system or robotic narrator. " +
            $"Delivery should be {emotion}; emotional intensity is {intensity}. " +
            "Use believable pauses, sentence rhythm, breath and emphasis. Do not add, remove or paraphrase any words. " +
            "For fictional magic words, pronounce them deliberately exactly as written." +
            persona;
    }
}
