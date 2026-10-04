using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record BestiaryVisualDefinition(
    string Id,
    string ModelFile,
    Vector3 Scale,
    Vector3 BaseColor,
    IReadOnlyList<string> RequiredClips);

public static class BestiaryVisualCatalog
{
    private static readonly string[] HumanoidCombatClips =
    [
        "Idle",
        "Walk",
        "Run",
        "Attack",
        "Hit",
        "Death",
        "Interact"
    ];

    public static IReadOnlyList<BestiaryVisualDefinition> Definitions { get; } =
    [
        new(
            "topielec",
            "topielec_animated.glb",
            new Vector3(1.02f),
            new Vector3(0.18f, 0.34f, 0.30f),
            HumanoidCombatClips),
        new(
            "poludnica",
            "poludnica_animated.glb",
            new Vector3(1.00f),
            new Vector3(0.72f, 0.65f, 0.42f),
            HumanoidCombatClips),
        new(
            "upior",
            "upior_animated.glb",
            new Vector3(1.03f),
            new Vector3(0.39f, 0.34f, 0.42f),
            HumanoidCombatClips),
        new(
            "boginka",
            "boginka_animated.glb",
            new Vector3(0.98f),
            new Vector3(0.28f, 0.46f, 0.34f),
            HumanoidCombatClips),
        new(
            "forest-guardian",
            "forest_guardian_animated.glb",
            new Vector3(1.10f),
            new Vector3(0.29f, 0.39f, 0.23f),
            HumanoidCombatClips)
    ];

    public static IReadOnlyList<string> RequiredModelFiles { get; } =
        Definitions
            .Select(definition => definition.ModelFile)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(file => file, StringComparer.Ordinal)
            .ToArray();

    public static BestiaryVisualDefinition Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return Definitions.FirstOrDefault(definition =>
                   string.Equals(definition.Id, id, StringComparison.Ordinal))
               ?? throw new KeyNotFoundException(
                   $"Unknown bestiary visual id '{id}'.");
    }
}
