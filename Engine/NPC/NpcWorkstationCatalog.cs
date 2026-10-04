using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record NpcWorkstationProp(
    string Id,
    string AssetPath,
    Vector2 Position,
    Vector3 Scale,
    float YawRadians,
    float YOffset = 0f);

public sealed record NpcWorkstationDefinition(
    string NpcId,
    string Activity,
    Vector2 WorkerPosition,
    Vector2 FacingTarget,
    double CycleHours,
    float WorkFraction,
    IReadOnlyList<NpcWorkstationProp> Props);

public static class NpcWorkstationCatalog
{
    public static IReadOnlyList<NpcWorkstationDefinition> Workstations { get; } =
    [
        new(
            "settler-farmer-01",
            "field-work",
            new Vector2(-29.0f, -69.4f),
            new Vector2(-30.0f, -68.7f),
            0.31,
            0.62f,
            [
                new("workstation-farmer-01-basket", "models/static/basket_r0_01.glb",
                    new Vector2(-30.1f, -68.7f), new Vector3(0.78f), 0.42f, 0.02f),
                new("workstation-farmer-01-sack", "models/static/sack_r0_01.glb",
                    new Vector2(-29.6f, -68.2f), new Vector3(0.82f), -0.20f, 0.02f)
            ]),
        new(
            "settler-farmer-02",
            "field-work",
            new Vector2(-34.1f, -67.2f),
            new Vector2(-35.1f, -66.5f),
            0.34,
            0.60f,
            [
                new("workstation-farmer-02-hay", "models/static/bela_siana_r0_01.glb",
                    new Vector2(-35.3f, -66.4f), new Vector3(0.72f), 0.31f),
                new("workstation-farmer-02-basket", "models/static/basket_r0_01.glb",
                    new Vector2(-34.8f, -65.8f), new Vector3(0.72f), -0.48f, 0.02f)
            ]),
        new(
            "settler-woodworker-01",
            "wood-work",
            new Vector2(15.2f, -97.2f),
            new Vector2(15.1f, -96.3f),
            0.24,
            0.72f,
            [
                new("workstation-woodworker-bench", "models/static/stol_warsztatowy_r0_01.glb",
                    new Vector2(15.1f, -96.2f), new Vector3(0.95f), 0.05f),
                new("workstation-woodworker-axe", "models/static/siekiera_r0_01.glb",
                    new Vector2(15.8f, -96.1f), new Vector3(0.82f), -0.32f, 0.72f)
            ]),
        new(
            "settler-potter-01",
            "craft-work",
            new Vector2(-6.7f, -74.8f),
            new Vector2(-6.7f, -73.9f),
            0.29,
            0.70f,
            [
                new("workstation-potter-table", "models/static/stol_warsztatowy_r0_01.glb",
                    new Vector2(-6.7f, -73.8f), new Vector3(0.88f), 0.02f),
                new("workstation-potter-vessels", "models/static/naczynia_r0_01.glb",
                    new Vector2(-6.9f, -73.6f), new Vector3(0.70f), 0.26f, 0.78f),
                new("workstation-potter-pot", "models/static/garnek_gliniany_r0_01.glb",
                    new Vector2(-6.2f, -73.6f), new Vector3(0.62f), -0.40f, 0.78f)
            ]),
        new(
            "settler-trader-01",
            "market-trade",
            new Vector2(2.7f, -75.2f),
            new Vector2(1.4f, -75.8f),
            0.30,
            0.64f,
            [
                new("workstation-trader-chest", "models/static/skrzynia_kufer_r0_01.glb",
                    new Vector2(2.0f, -76.5f), new Vector3(0.72f), 0.15f),
                new("workstation-trader-basket", "models/static/basket_r0_01.glb",
                    new Vector2(1.7f, -75.3f), new Vector3(0.72f), -0.35f, 0.02f)
            ]),
        new(
            "settler-smith-helper-01",
            "forge-work",
            new Vector2(20.1f, -89.3f),
            new Vector2(19.0f, -88.0f),
            0.25,
            0.74f,
            [
                new("workstation-smith-bucket", "models/static/wiadro_r0_01.glb",
                    new Vector2(20.4f, -88.5f), new Vector3(0.70f), 0.12f, 0.02f),
                new("workstation-smith-axe", "models/static/siekiera_r0_02.glb",
                    new Vector2(19.7f, -88.5f), new Vector3(0.76f), 0.72f, 0.58f)
            ]),
        new(
            "settler-weaver-01",
            "weave-work",
            new Vector2(-16.6f, -78.2f),
            new Vector2(-17.6f, -78.4f),
            0.32,
            0.76f,
            [
                new("workstation-weaver-loom", "models/static/krosno_r0_01.glb",
                    new Vector2(-17.7f, -78.4f), new Vector3(0.92f), 0.08f),
                new("workstation-weaver-stool", "models/static/taboret_r0_01.glb",
                    new Vector2(-16.9f, -79.0f), new Vector3(0.74f), -0.28f)
            ]),
        new(
            "settler-gatherer-01",
            "sort-herbs",
            new Vector2(7.0f, -90.0f),
            new Vector2(7.0f, -89.1f),
            0.28,
            0.72f,
            [
                new("workstation-herbs-table", "models/static/stol_warsztatowy_r0_01.glb",
                    new Vector2(7.0f, -89.0f), new Vector3(0.86f), -0.04f),
                new("workstation-herbs-bundle", "models/static/peczek_ziol_r0_01.glb",
                    new Vector2(6.7f, -88.8f), new Vector3(0.82f), 0.28f, 0.77f),
                new("workstation-herbs-dried", "models/static/ziola_suszone_r0_01.glb",
                    new Vector2(7.3f, -88.8f), new Vector3(0.78f), -0.18f, 0.77f)
            ]),
        new(
            "settler-fisher-01",
            "mend-nets",
            new Vector2(20.0f, -75.3f),
            new Vector2(20.7f, -74.7f),
            0.31,
            0.70f,
            [
                new("workstation-fisher-bench", "models/static/lawa_wnetrze_r0_01.glb",
                    new Vector2(20.8f, -74.8f), new Vector3(0.82f), 0.50f),
                new("workstation-fisher-rope", "models/static/rope_coil_r0_01.glb",
                    new Vector2(20.5f, -75.0f), new Vector3(0.78f), -0.12f, 0.18f),
                new("workstation-fisher-trap", "models/static/pulapka_rybacka_r0_01.glb",
                    new Vector2(21.4f, -74.7f), new Vector3(0.72f), 0.28f)
            ]),
        new(
            "herbalist",
            "trade-and-prepare",
            new Vector2(5.0f, -92.0f),
            new Vector2(5.7f, -91.3f),
            0.33,
            0.68f,
            [
                new("workstation-herbalist-table", "models/static/stol_warsztatowy_r0_01.glb",
                    new Vector2(5.7f, -91.2f), new Vector3(0.82f), 0.38f),
                new("workstation-herbalist-basket", "models/static/basket_r0_01.glb",
                    new Vector2(6.0f, -91.0f), new Vector3(0.68f), -0.32f, 0.02f),
                new("workstation-herbalist-herbs", "models/static/peczek_ziol_r0_01.glb",
                    new Vector2(5.6f, -91.0f), new Vector3(0.76f), 0.22f, 0.74f)
            ]),
        new(
            "crossing-keeper",
            "maintain-crossing",
            new Vector2(94.2f, 40.8f),
            new Vector2(95.1f, 41.3f),
            0.30,
            0.58f,
            [
                new("workstation-crossing-rope", "models/static/rope_coil_r0_01.glb",
                    new Vector2(94.8f, 40.9f), new Vector3(0.78f), 0.18f, 0.02f),
                new("workstation-crossing-bucket", "models/static/wiadro_r0_01.glb",
                    new Vector2(93.9f, 40.4f), new Vector3(0.72f), -0.30f, 0.02f)
            ])
    ];

    public static IReadOnlyList<WorldModelInstance> BuildModels(Terrain terrain)
    {
        ArgumentNullException.ThrowIfNull(terrain);

        var result = new List<WorldModelInstance>();
        foreach (var workstation in Workstations)
        {
            foreach (var prop in workstation.Props)
            {
                var position = new Vector3(
                    prop.Position.X,
                    0f,
                    prop.Position.Y);
                position.Y =
                    terrain.SampleHeight(position) +
                    prop.YOffset;

                result.Add(new WorldModelInstance(
                    prop.Id,
                    prop.AssetPath,
                    position,
                    prop.Scale,
                    prop.YawRadians,
                    Vector3.One));
            }
        }

        return result;
    }

    public static bool TrySample(
        string npcId,
        string activity,
        double timeOfDayHours,
        out NpcRoutineSample sample)
    {
        var definition = Workstations.FirstOrDefault(workstation =>
            string.Equals(workstation.NpcId, npcId, StringComparison.Ordinal) &&
            string.Equals(workstation.Activity, activity, StringComparison.Ordinal));

        if (definition is null)
        {
            sample = default;
            return false;
        }

        var phase = PositiveModulo(
            timeOfDayHours / definition.CycleHours +
            StablePhase(npcId + ":" + activity),
            1.0);

        if (phase >= definition.WorkFraction)
        {
            sample = default;
            return false;
        }

        var forward =
            definition.FacingTarget -
            definition.WorkerPosition;
        if (forward.LengthSquared() > 0.000001f)
            forward = Vector2.Normalize(forward);
        else
            forward = Vector2.UnitY;

        sample = new NpcRoutineSample(
            definition.WorkerPosition,
            forward,
            false);
        return true;
    }

    public static NpcWorkstationDefinition? Find(
        string npcId,
        string activity) =>
        Workstations.FirstOrDefault(workstation =>
            string.Equals(workstation.NpcId, npcId, StringComparison.Ordinal) &&
            string.Equals(workstation.Activity, activity, StringComparison.Ordinal));

    private static double StablePhase(string value)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in value)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            return (hash % 1000u) / 1000.0;
        }
    }

    private static double PositiveModulo(
        double value,
        double modulus)
    {
        var result = value % modulus;
        return result < 0d
            ? result + modulus
            : result;
    }
}
