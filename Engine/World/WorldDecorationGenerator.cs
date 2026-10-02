using System.Numerics;

namespace SlavicGame.Engine.World;

public static class WorldDecorationGenerator
{
    private const int Seed = 0x534C4156;

    private static readonly string[] ForestTrees =
    [
        "models/static/dab_stary_r0_01.glb",
        "models/static/dab_stary_r0_02.glb",
        "models/static/brzoza_r0_02.glb",
        "models/static/sosna_r0_02.glb"
    ];

    private static readonly string[] ForestUnderstory =
    [
        "models/static/krzak_r0_02.glb",
        "models/static/krzak_r0_03.glb",
        "models/static/krzak_r0_04.glb",
        "models/static/paproc_r0_01.glb",
        "models/static/korzenie_r0_01.glb"
    ];

    private static readonly string[] SwampVegetation =
    [
        "models/static/olsza_bagienna_r0_01.glb",
        "models/static/trzciny_r0_01.glb",
        "models/static/trzciny_r0_02.glb",
        "models/static/trzciny_r0_03.glb",
        "models/static/pien_bagienny_r0_01.glb"
    ];

    public static IReadOnlyList<WorldModelInstance> Generate(Terrain terrain)
    {
        ArgumentNullException.ThrowIfNull(terrain);

        var random = new Random(Seed);
        var result = new List<WorldModelInstance>(48);

        ScatterDisc(
            result, terrain, random,
            prefix: "forest-tree",
            center: Vector2.Zero,
            radius: 50f,
            count: 18,
            assets: ForestTrees,
            minScale: 0.82f,
            maxScale: 1.28f,
            exclusionRadius: 10f);

        ScatterDisc(
            result, terrain, random,
            prefix: "forest-understory",
            center: Vector2.Zero,
            radius: 47f,
            count: 14,
            assets: ForestUnderstory,
            minScale: 0.75f,
            maxScale: 1.45f,
            exclusionRadius: 6f);

        ScatterDisc(
            result, terrain, random,
            prefix: "swamp-growth",
            center: new Vector2(95f, 35f),
            radius: 39f,
            count: 16,
            assets: SwampVegetation,
            minScale: 0.78f,
            maxScale: 1.42f,
            exclusionRadius: 7f);

        return result;
    }

    private static void ScatterDisc(
        List<WorldModelInstance> output,
        Terrain terrain,
        Random random,
        string prefix,
        Vector2 center,
        float radius,
        int count,
        IReadOnlyList<string> assets,
        float minScale,
        float maxScale,
        float exclusionRadius)
    {
        for (var i = 0; i < count; i++)
        {
            Vector2 point;
            var attempts = 0;
            do
            {
                var angle = random.NextSingle() * MathF.Tau;
                var distance = MathF.Sqrt(random.NextSingle()) * radius;
                point = center + new Vector2(
                    MathF.Cos(angle) * distance,
                    MathF.Sin(angle) * distance);
                attempts++;
            }
            while (Vector2.Distance(point, center) < exclusionRadius && attempts < 16);

            var position = new Vector3(point.X, 0f, point.Y);
            position.Y = terrain.SampleHeight(position);

            var uniformScale = minScale + random.NextSingle() * (maxScale - minScale);
            var scale = new Vector3(
                uniformScale * (0.94f + random.NextSingle() * 0.12f),
                uniformScale * (0.92f + random.NextSingle() * 0.18f),
                uniformScale * (0.94f + random.NextSingle() * 0.12f));

            output.Add(new WorldModelInstance(
                $"{prefix}-{i:00}",
                assets[random.Next(assets.Count)],
                position,
                scale,
                random.NextSingle() * MathF.Tau,
                Vector3.One));
        }
    }
}
