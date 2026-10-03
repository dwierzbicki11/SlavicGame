using System.Numerics;

namespace SlavicGame.Engine.World;

public static class CinematicCatalog
{
    public static CinematicDefinition Arrival { get; } = new("arrival", Array.AsReadOnly(new[]
    {
        new CinematicShot(new(-22, 9, -20), new(-8, 5, -12), new(0, 2, 0), 4,
            "POGRANICZE ZARNOWCA. LAS PAMIETA KAZDY KROK."),
        new CinematicShot(new(12, 5, -8), new(5, 3, -4), new(0, 1.5f, 0), 4,
            "ZAR VEK. ZIVA DAR. VEDA NAW. SLOWA MAJA CENE.")
    }));

    public static CinematicDefinition Shrine { get; } = new("shrine", Array.AsReadOnly(new[]
    {
        new CinematicShot(new(-105, 9, 38), new(-99, 6, 46), new(-85, 2, 55), 4,
            "KAMIENNY KRAG. GRANICA JEST TUTAJ CIENSZA."),
        new CinematicShot(new(-72, 6, 68), new(-75, 4, 64), new(-85, 2, 55), 4,
            "VEDA NAW - ODSLON TO, CO POZOSTALO.")
    }));

    public static CinematicDefinition ContractAccepted { get; } = Relative("contract-accepted",
        "ZLECENIE PRZYJETE. MOKRADLO CZEKA.",
        "NAJPIERW SLADY. POTEM WNIOSKI.");

    public static CinematicDefinition FirstNightAnomaly { get; } = Relative("first-night-anomaly",
        "NOC ZMIENIA MOKRADLO.",
        "NIE KAZDY BLASK JEST OGNISKIEM.");

    public static CinematicDefinition AnchorRevealed { get; } = Relative("anchor-revealed",
        "PRZEDMIOT TRZYMA ECHO PRZY TYM MIEJSCU.",
        "TOZSAMOSC CELU MA ZNACZENIE.");

    public static CinematicDefinition RitualPreparation { get; } = Relative("ritual-preparation",
        "TOZSAMOSC. GRANICA. WARUNEK.",
        "KOLEJNOSC MA ZNACZENIE.");

    public static CinematicDefinition ContractResolution { get; } = Relative("contract-resolution",
        "MOKRADLO UCICHLO.",
        "SKUTEK DECYZJI POZOSTANIE W SWIECIE.");

    public static CinematicDefinition DivineManifestation { get; } = Relative("divine-manifestation",
        "COS ODPOWIADA NA WEZWANIE.",
        "ODPOWIEDZ NIE OZNACZA POSLUSZENSTWA.");

    public static IReadOnlyList<CinematicDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        Arrival,
        Shrine,
        ContractAccepted,
        FirstNightAnomaly,
        AnchorRevealed,
        RitualPreparation,
        ContractResolution,
        DivineManifestation
    });

    private static readonly IReadOnlyDictionary<string, CinematicDefinition> ById =
        All.ToDictionary(definition => definition.Id, StringComparer.Ordinal);

    public static bool TryGet(string id, out CinematicDefinition definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return ById.TryGetValue(id, out definition!);
    }

    public static CinematicDefinition Get(string id) =>
        TryGet(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Cinematic '{id}' is not registered.");

    private static CinematicDefinition Relative(string id, string first, string second) =>
        new(id, Array.AsReadOnly(new[]
        {
            new CinematicShot(new(-6, 3.5f, -5), new(-3, 2.8f, -2), new(0, 1.6f, 0), 3.5,
                first, CinematicSpace.PlayerRelative),
            new CinematicShot(new(4, 3, -3), new(2, 2.5f, -1), new(0, 1.5f, 0), 3.5,
                second, CinematicSpace.PlayerRelative)
        }));
}
