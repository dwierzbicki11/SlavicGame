namespace SlavicGame.Engine.Renderer;

/// <summary>
/// Keeps unfinished temporal plumbing out of the shipping spatial path. The
/// native FSR 3 backend will enable these inputs automatically once connected;
/// developers can validate them earlier with SLAVICGAME_TEMPORAL_INPUTS=1.
/// </summary>
public static class TemporalInputPolicy
{
    public const string EnvironmentVariable =
        "SLAVICGAME_TEMPORAL_INPUTS";

    public static bool IsEnabled() =>
        string.Equals(
            Environment.GetEnvironmentVariable(EnvironmentVariable),
            "1",
            StringComparison.Ordinal) ||
        string.Equals(
            Environment.GetEnvironmentVariable(EnvironmentVariable),
            "true",
            StringComparison.OrdinalIgnoreCase);
}
