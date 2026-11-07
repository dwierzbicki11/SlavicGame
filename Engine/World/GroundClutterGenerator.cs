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

    private static readonly string[] TrailVergeClutter =
    [
        "models/static/ground_clutter/fern_clump.glb",
        "models/static/ground_clutter/exposed_roots.glb",
        "models/static/ground_clutter/mushroom_cluster.glb",
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
        var result = new List<WorldModelInstance>(2600);

        Scatter(
            result,
            terrain,
            random,
            "starting-ground",
            Vector2.Zero,
            radius: 110f,
            count: 380,
            ForestClutter,
            minScale: 0.85f,
            maxScale: 1.62f,
            avoidTrails: true);

        Scatter(
            result,
            terrain,
            random,
            "r0-puszcza-ground",
            Vector2.Zero,
            radius: 235f,
            count: 480,
            ForestClutter,
            minScale: 0.86f,
            maxScale: 1.65f,
            avoidTrails: true);

        Scatter(
            result,
            terrain,
            random,
            "shrine-grove-ground",
            ForestLayout.ShrineCenter,
            radius: 62f,
            count: 160,
            ForestClutter,
            minScale: 0.85f,
            maxScale: 1.62f,
            avoidTrails: true);

        ScatterTrailVerges(
            result,
            terrain,
            random,
            "trail-verge-ground",
            count: 320,
            TrailVergeClutter,
            minScale: 0.82f,
            maxScale: 1.55f);

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
                radius: zone.Radius * 0.70f,
                count: zone.Biome == ForestBiome.Wetland ? 220 : 200,
                assets,
                minScale: 0.82f,
                maxScale: 1.65f,
                avoidTrails: true);
        }

        Scatter(
            result,
            terrain,
            random,
            "swamp-ground",
            ForestLayout.SwampCenter,
            radius: 58f,
            count: 220,
            WetlandClutter,
            minScale: 0.88f,
            maxScale: 1.62f,
            avoidTrails: true);

        return result;
    }

    private static void ScatterTrailVerges(
        List<WorldModelInstance> output,
        Terrain terrain,
        Random random,
        string prefix,
        int count,
        IReadOnlyList<string> assets,
        float minScale,
        float maxScale)
    {
        var placed = 0;
        var attempts = 0;
        var maxAttempts = Math.Max(256, count * 150);
        var trails = ForestLayout.Trails;

        while (placed < count && attempts++ < maxAttempts)
        {
            var trailIndex = random.NextSingle() < 0.78f
                ? random.Next(Math.Min(7, trails.Count))
                : random.Next(trails.Count);
            var trail = trails[trailIndex];
            if (trail.Id == "road-village-internal")
                continue;

            var t = 0.06f + random.NextSingle() * 0.88f;
            var side = random.Next(2) == 0 ? -1f : 1f;
            var offset = trail.CorridorHalfWidth + 0.35f + random.NextSingle() * 3.8f;
            var point = ForestLayout.SampleMeanderingTrailPoint(trail, t, side * offset);

            if (ForestLayout.IsSettlementOrClearing(point))
                continue;
            if (ForestLayout.IsTrailCorridor(point))
                continue;

            var position = new Vector3(point.X, 0f, point.Y);
            position.Y = terrain.SampleHeight(position) + 0.015f;

            var scaleValue = minScale + random.NextSingle() * (maxScale - minScale);
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
                $"Could not place trail verge ground clutter '{prefix}': {placed}/{count}.");
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
        var maxAttempts = Math.Max(256, count * 150);

        while (placed < count && attempts++ < maxAttempts)
        {
            var angle = random.NextSingle() * MathF.Tau;
            var distance = MathF.Sqrt(random.NextSingle()) * radius;
            var point = center + new Vector2(
                MathF.Cos(angle) * distance,
                MathF.Sin(angle) * distance);

            if (ForestLayout.IsSettlementOrClearing(point))
                continue;
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
