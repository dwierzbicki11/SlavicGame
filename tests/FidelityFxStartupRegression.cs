using SlavicGame.Engine.Renderer.FidelityFx;
using SlavicGame.Engine.Settings;

internal static class FidelityFxStartupRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var configured in Enum.GetValues<MsaaQuality>())
        {
            check(FidelityFxStartupPolicy.EffectiveMsaa(configured, true, true, true)
                    == MsaaQuality.Off,
                $"FSR3 obtains single-sample depth with {configured} configured");
            check(FidelityFxStartupPolicy.EffectiveMsaa(configured, false, true, true)
                    == configured,
                $"Normal presentation preserves {configured}");
            check(FidelityFxStartupPolicy.EffectiveMsaa(configured, true, false, true)
                    == configured,
                $"Unsupported FSR3 platform preserves {configured}");
            check(FidelityFxStartupPolicy.EffectiveMsaa(configured, true, true, false)
                    == configured,
                $"Missing FSR3 provider preserves {configured}");
        }

        check(FidelityFxStartupPolicy.NeedsTemporalInputs(false, true, false),
            "Requested FSR3 produces temporal inputs before dispatch");
        check(!FidelityFxStartupPolicy.NeedsTemporalInputs(false, true, true),
            "Failed FSR3 stops unused temporal passes on fallback");
        check(FidelityFxStartupPolicy.NeedsTemporalInputs(true, true, true),
            "Explicit temporal validation survives FSR3 failure");
        check(!FidelityFxStartupPolicy.NeedsTemporalInputs(false, false, false),
            "Normal spatial presentation avoids temporal passes");
    }
}
