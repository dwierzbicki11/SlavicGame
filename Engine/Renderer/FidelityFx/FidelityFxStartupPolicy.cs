using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.Renderer.FidelityFx;

public static class FidelityFxStartupPolicy
{
    public static MsaaQuality EffectiveMsaa(
        MsaaQuality configured,
        bool requested,
        bool dispatchSupported,
        bool providerAvailable) =>
        requested && dispatchSupported && providerAvailable
            ? MsaaQuality.Off
            : configured;

    public static bool NeedsTemporalInputs(
        bool explicitInputValidation,
        bool fsrRequested,
        bool fsrFailed) =>
        explicitInputValidation || (fsrRequested && !fsrFailed);
}
