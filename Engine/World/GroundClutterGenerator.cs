using System.Numerics;

namespace SlavicGame.Engine.World;

public static class GroundClutterGenerator
{
    private const int Seed = 0x47524F55;

    private static readonly string[] ForestClutter =
    [
        "models/static/ground_clutter/grass_tuft.glb",
        "models/static/ground_clutter/fern_clump.glb",
        "models/static/ground_clutter/mushroom_cluster.glb",
        "models/static/ground_clutter/exposed_roots.glb",
        "models/static/ground_clutter/small_stones.glb",
        "models/static/ground_clutter/leaf_litter_patch.glb"
    ];

    private static readonly string[] WetlandClutter =
    [
        "models/static/ground_clutter/reeds_cluster.glb",
        "models/static/ground_clutter/mud_puddle.glb",
        "models/static/ground_clutter/exposed_roots.glb",
        "models/static/ground_clutter/small_stones.glb"
    ];

    public static IReadOnlyList<WorldModelInstance> Generate(Terrain terrain)
    {
        ArgumentNullException.ThrowIfNull(terrain);

        var random = new Random(Seed);
        var result = new List<WorldModelInstance>(320);

        Scatter(
            result,
            terrain,
            random,
            "starting-ground",
            Vector2.Zero,
            radius: 86f,
            count: 70,
            ForestClutter,
            minScale: 0.72f,
            maxScale: 1.32f,
            avoidTrails: false);

        foreach (var zone in ForestLayout.Zones)
        {
            var assets = zone.Biome == ForestBiome.Wetland
                ? WetlandClutter
                : ForestClutter;

            Scatter(
                result,
                terrain,
                random,
                $"{zone.Id}-ground",
                zone.Center,
                radius: zone.Radius * 0.56f,
                count: zone.Biome == ForestBiome.Wetland ? 58 : 46,
                assets,
                minScale: 0.70f,
                maxScale: 1.45f,
                avoidTrails: true);
        }

        Scatter(
            result,
            terrain,
            random,
            "swamp-ground",
            new Vector2(95f, 35f),
            radius: 55f,
            count: 62,
            WetlandClutter,
            minScale: 0.78f,
            maxScale: 1.42f,
            avoidTrails: false);

        return result;
    }

    private static void Scatter(
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
        bool avoidTrails)
    {
        var placed = 0;
        var attempts = 0;
        var maxAttempts = Math.Max(100, count * 50);

        while (placed < count && attempts++ < maxAttempts)
        {
            var angle = random.NextSingle() * MathF.Tau;
            var distance = MathF.Sqrt(random.NextSingle()) * radius;
            var point = center + new Vector2(
                MathF.Cos(angle) * distance,
                MathF.Sin(angle) * distance);

            if (avoidTrails && ForestLayout.IsTrailCorridor(point))
                continue;

            var position = new Vector3(point.X, 0f, point.Y);
            position.Y = terrain.SampleHeight(position) + 0.015f;

            var scaleValue =
                minScale + random.NextSingle() * (maxScale - minScale);
            var scale = new Vector3(
                scaleValue * (0.88f + random.NextSingle() * 0.24f),
                scaleValue * (0.88f + random.NextSingle() * 0.24f),
                scaleValue * (0.88f + random.NextSingle() * 0.24f));

            output.Add(new WorldModelInstance(
                $"{prefix}-{placed:000}",
                assets[random.Next(assets.Count)],
                position,
                scale,
                random.NextSingle() * MathF.Tau,
                Vector3.One));
            placed++;
        }

        if (placed != count)
            throw new InvalidOperationException(
                $"Could not place ground clutter '{prefix}': {placed}/{count}.");
    }
}
