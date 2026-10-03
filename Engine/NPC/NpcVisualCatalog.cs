using System.Numerics;
using SlavicGame.Engine.NPC;

namespace SlavicGame.Engine.World;

public enum NpcAccessoryKind
{
    None,
    Shawl,
    Satchel,
    Staff,
    Spear,
    Hood,
    ToolBundle,
    Basket,
    BeltPouch
}

public sealed record NpcVisualProfile(
    Vector3 BodyScale,
    Vector3 BaseColor,
    Vector3 AccentColor,
    NpcAccessoryKind PrimaryAccessory,
    NpcAccessoryKind SecondaryAccessory = NpcAccessoryKind.None,
    float AnimationSpeed = 1f);

public static class NpcVisualCatalog
{
    private static readonly IReadOnlyDictionary<string, NpcVisualProfile> ById =
        new Dictionary<string, NpcVisualProfile>(StringComparer.Ordinal)
        {
            ["missing-family"] = new(
                new Vector3(0.97f, 1.00f, 0.97f),
                new Vector3(0.46f, 0.31f, 0.18f),
                new Vector3(0.62f, 0.48f, 0.29f),
                NpcAccessoryKind.Shawl,
                NpcAccessoryKind.BeltPouch,
                0.92f),

            ["crossing-keeper"] = new(
                new Vector3(1.03f, 1.04f, 1.03f),
                new Vector3(0.28f, 0.36f, 0.22f),
                new Vector3(0.43f, 0.31f, 0.16f),
                NpcAccessoryKind.Staff,
                NpcAccessoryKind.ToolBundle,
                1.00f),

            ["herbalist"] = new(
                new Vector3(0.93f, 0.98f, 0.93f),
                new Vector3(0.25f, 0.46f, 0.22f),
                new Vector3(0.62f, 0.55f, 0.23f),
                NpcAccessoryKind.Hood,
                NpcAccessoryKind.Satchel,
                0.90f),

            ["community-guard"] = new(
                new Vector3(1.06f, 1.08f, 1.06f),
                new Vector3(0.36f, 0.32f, 0.28f),
                new Vector3(0.50f, 0.18f, 0.11f),
                NpcAccessoryKind.Spear,
                NpcAccessoryKind.BeltPouch,
                1.05f),

            ["shrine-keeper"] = new(
                new Vector3(0.98f, 1.05f, 0.98f),
                new Vector3(0.30f, 0.28f, 0.45f),
                new Vector3(0.52f, 0.43f, 0.62f),
                NpcAccessoryKind.Hood,
                NpcAccessoryKind.Staff,
                0.88f),

            ["settler-farmer-01"] = new(
                new Vector3(1.02f, 1.02f, 1.00f),
                new Vector3(0.40f, 0.31f, 0.18f),
                new Vector3(0.55f, 0.44f, 0.25f),
                NpcAccessoryKind.ToolBundle,
                NpcAccessoryKind.BeltPouch,
                0.98f),

            ["settler-farmer-02"] = new(
                new Vector3(0.96f, 0.99f, 0.96f),
                new Vector3(0.34f, 0.39f, 0.21f),
                new Vector3(0.55f, 0.49f, 0.31f),
                NpcAccessoryKind.Basket,
                NpcAccessoryKind.Shawl,
                0.94f),

            ["settler-woodworker-01"] = new(
                new Vector3(1.05f, 1.03f, 1.05f),
                new Vector3(0.38f, 0.27f, 0.16f),
                new Vector3(0.50f, 0.35f, 0.18f),
                NpcAccessoryKind.ToolBundle,
                NpcAccessoryKind.Satchel,
                1.02f),

            ["settler-potter-01"] = new(
                new Vector3(0.95f, 1.00f, 0.97f),
                new Vector3(0.46f, 0.33f, 0.24f),
                new Vector3(0.67f, 0.48f, 0.33f),
                NpcAccessoryKind.Shawl,
                NpcAccessoryKind.Basket,
                0.93f),

            ["settler-trader-01"] = new(
                new Vector3(1.00f, 1.01f, 1.00f),
                new Vector3(0.31f, 0.37f, 0.44f),
                new Vector3(0.63f, 0.48f, 0.20f),
                NpcAccessoryKind.Satchel,
                NpcAccessoryKind.BeltPouch,
                1.00f),

            ["settler-carrier-01"] = new(
                new Vector3(1.04f, 1.05f, 1.04f),
                new Vector3(0.35f, 0.30f, 0.20f),
                new Vector3(0.48f, 0.40f, 0.26f),
                NpcAccessoryKind.Basket,
                NpcAccessoryKind.BeltPouch,
                1.04f),

            ["settler-elder-01"] = new(
                new Vector3(0.94f, 0.96f, 0.94f),
                new Vector3(0.33f, 0.32f, 0.29f),
                new Vector3(0.49f, 0.44f, 0.38f),
                NpcAccessoryKind.Staff,
                NpcAccessoryKind.Shawl,
                0.82f),

            ["settler-traveler-01"] = new(
                new Vector3(1.01f, 1.03f, 1.01f),
                new Vector3(0.29f, 0.34f, 0.27f),
                new Vector3(0.48f, 0.35f, 0.19f),
                NpcAccessoryKind.Satchel,
                NpcAccessoryKind.Hood,
                1.08f),
        };

    public static NpcVisualProfile For(
        string id,
        NpcRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (ById.TryGetValue(id, out var profile))
            return profile;

        var baseColor = NpcPresentation.RoleColor(role);
        return role switch
        {
            NpcRole.Worker => new(
                Vector3.One,
                baseColor,
                new Vector3(0.52f, 0.39f, 0.22f),
                NpcAccessoryKind.ToolBundle),

            NpcRole.Trader => new(
                Vector3.One,
                baseColor,
                new Vector3(0.61f, 0.46f, 0.23f),
                NpcAccessoryKind.Satchel),

            NpcRole.Traveler => new(
                Vector3.One,
                baseColor,
                new Vector3(0.44f, 0.39f, 0.29f),
                NpcAccessoryKind.Hood,
                NpcAccessoryKind.Satchel),

            _ => new(
                Vector3.One,
                baseColor,
                Vector3.Lerp(baseColor, Vector3.One, 0.20f),
                NpcAccessoryKind.BeltPouch)
        };
    }

    public static string AnimationClip(
        string activity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activity);

        return activity switch
        {
            "patrol" or
            "night-watch" or
            "carry-goods" or
            "arrive-and-trade" or
            "go-to-fields" => "Walk",

            _ => "Idle"
        };
    }
}
