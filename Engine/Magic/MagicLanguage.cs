namespace SlavicGame.Engine.Magic;

public sealed record MagicWordDefinition(
    string Id,
    string Spoken,
    string Function,
    string VoiceCueId);

public sealed class IncantationPhrase
{
    public string Id { get; }
    public IReadOnlyList<MagicWordDefinition> Words { get; }
    public double DurationSeconds { get; }
    public string Text { get; }

    internal IncantationPhrase(
        string id,
        IReadOnlyList<MagicWordDefinition> words,
        double durationSeconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(words);
        if (words.Count == 0)
            throw new ArgumentException("Incantation requires at least one word.", nameof(words));
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));

        Id = id;
        Words = words;
        DurationSeconds = durationSeconds;
        Text = string.Join(" ", words.Select(word => word.Spoken));
    }

    public string CueAt(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds)) elapsedSeconds = 0;
        var progress = Math.Clamp(elapsedSeconds / DurationSeconds, 0.0, 1.0);
        var count = progress >= 1.0
            ? Words.Count
            : Math.Clamp((int)Math.Floor(progress * Words.Count) + 1, 1, Words.Count);
        return string.Join(" ", Words.Take(count).Select(word => word.Spoken));
    }
}

public static class MagicLanguage
{
    // Entirely fictional vocabulary for SlavicGame. These words are not presented
    // as reconstructed or historical Slavic ritual language.
    public static MagicWordDefinition Zar { get; } =
        new("force-heat", "ZAR", "force / heat / ignition", "magic.word.zar");
    public static MagicWordDefinition Vek { get; } =
        new("direction-release", "VEK", "direction / release", "magic.word.vek");
    public static MagicWordDefinition Ziva { get; } =
        new("life-pattern", "ZIVA", "life / living pattern", "magic.word.ziva");
    public static MagicWordDefinition Dar { get; } =
        new("exchange-restore", "DAR", "exchange / restoration", "magic.word.dar");
    public static MagicWordDefinition Veda { get; } =
        new("recognition-reveal", "VEDA", "recognition / reveal", "magic.word.veda");
    public static MagicWordDefinition Naw { get; } =
        new("echo-beyond", "NAW", "echo / trace beyond the boundary", "magic.word.naw");

    public static IReadOnlyList<MagicWordDefinition> Words { get; } =
        Array.AsReadOnly(new[] { Zar, Vek, Ziva, Dar, Veda, Naw });

    public static IncantationPhrase Spark { get; } =
        new("incantation.spark", Array.AsReadOnly(new[] { Zar, Vek }), 0.9);
    public static IncantationPhrase Mend { get; } =
        new("incantation.mend", Array.AsReadOnly(new[] { Ziva, Dar }), 0.9);
    public static IncantationPhrase RevealTrace { get; } =
        new("incantation.reveal-trace", Array.AsReadOnly(new[] { Veda, Naw }), 0.9);

    public static IReadOnlyList<IncantationPhrase> Phrases { get; } =
        Array.AsReadOnly(new[] { Spark, Mend, RevealTrace });
}
