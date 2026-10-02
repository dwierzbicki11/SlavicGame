using System.Numerics;

namespace SlavicGame.Engine.World;

public static class WorldDecorationGenerator
{
    private const int Seed = 0x534C4156;

    private static readonly string[] MixedTrees =
    [
        "models/static/dab_stary_r0_01.glb",
        "models/static/dab_stary_r0_02.glb",
        "models/static/brzoza_r0_02.glb",
        "models/static/sosna_r0_02.glb"
    ];

    private static readonly string[] OakTrees =
    [
        "models/static/dab_stary_r0_01.glb",
        "models/static/dab_stary_r0_02.glb",
        "models/static/dab_r0_02.glb",
        "models/static/brzoza_r0_02.glb"
    ];

    private static readonly string[] PineTrees =
    [
        "models/static/sosna_r0_02.glb",
        "models/static/sosna_r0_01.glb",
        "models/static/dab_stary_r0_02.glb"
    ];

    private static readonly string[] BirchTrees =
    [
        "models/static/brzoza_r0_02.glb",
        "models/static/brzoza_r0_01.glb",
        "models/static/dab_stary_r0_01.glb"
    ];

    private static readonly string[] WetlandTrees =
    [
        "models/static/olsza_bagienna_r0_01.glb",
        "models/static/brzoza_r0_02.glb",
        "models/static/dab_stary_r0_02.glb"
    ];

    private static readonly string[] ForestUnderstory =
    [
        "models/static/krzak_r0_02.glb",
        "models/static/krzak_r0_03.glb",
        "models/static/krzak_r0_04.glb",
        "models/static/paproc_r0_01.glb",
        "models/static/korzenie_r0_01.glb",
        "models/static/grzyby_r0_01.glb"
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
        var result = new List<WorldModelInstance>(720);

        ScatterDisc(
            result, terrain, random,
            prefix: "starting-forest-tree",
            center: Vector2.Zero,
            radius: 88f,
            count: 35,
            assets: MixedTrees,
            minScale: 0.82f,
            maxScale: 1.30f,
            exclusionRadius: 18f,
            avoidTrails: false);

        ScatterDisc(
            result, terrain, random,
            prefix: "starting-forest-understory",
            center: Vector2.Zero,
            radius: 82f,
            count: 25,
            assets: ForestUnderstory,
            minScale: 0.72f,
            maxScale: 1.48f,
            exclusionRadius: 12f,
            avoidTrails: false);

        ScatterDisc(
            result, terrain, random,
            prefix: "swamp-growth",
            center: new Vector2(95f, 35f),
            radius: 52f,
            count: 25,
            assets: SwampVegetation,
            minScale: 0.76f,
            maxScale: 1.46f,
            exclusionRadius: 8f,
            avoidTrails: false);

        // Sparse transition woods make travel between named forests feel continuous
        // without turning the entire map into a wall of trees.
        ScatterRing(
            result, terrain, random,
            prefix: "transition-tree",
            center: Vector2.Zero,
            innerRadius: 180f,
            outerRadius: 850f,
            count: 55,
            assets: MixedTrees,
            minScale: 0.78f,
            maxScale: 1.32f);

        foreach (var zone in ForestLayout.Zones)
            ScatterForestZone(result, terrain, random, zone);

        return result;
    }

    private static void ScatterForestZone(
        List<WorldModelInstance> output,
        Terrain terrain,
        Random random,
        ForestZone zone)
    {
        var treeAssets = zone.Biome switch
        {
            ForestBiome.Oak => OakTrees,
            ForestBiome.Pine => PineTrees,
            ForestBiome.Birch => BirchTrees,
            ForestBiome.Wetland => WetlandTrees,
            _ => MixedTrees
        };

        var understoryAssets = zone.Biome == ForestBiome.Wetland
            ? SwampVegetation
            : ForestUnderstory;

        ScatterDisc(
            output, terrain, random,
            prefix: $"{zone.Id}-tree",
            center: zone.Center,
            radius: zone.Radius,
            count: zone.TreeCount,
            assets: treeAssets,
            minScale: zone.Biome == ForestBiome.Pine ? 0.92f : 0.80f,
            maxScale: zone.Biome == ForestBiome.Pine ? 1.48f : 1.38f,
            exclusionRadius: zone.ClearingRadius,
            avoidTrails: true);

        ScatterDisc(
            output, terrain, random,
            prefix: $"{zone.Id}-understory",
            center: zone.Center,
            radius: zone.Radius * 0.94f,
            count: zone.UnderstoryCount,
            assets: understoryAssets,
            minScale: 0.68f,
            maxScale: 1.55f,
            exclusionRadius: zone.ClearingRadius * 0.75f,
            avoidTrails: true);
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
        float exclusionRadius,
        bool avoidTrails)
    {
        var placed = 0;
        var attempts = 0;
        var maxAttempts = Math.Max(64, count * 40);

        while (placed < count && attempts++ < maxAttempts)
        {
            var angle = random.NextSingle() * MathF.Tau;
            var distance = MathF.Sqrt(random.NextSingle()) * radius;
            var point = center + new Vector2(
                MathF.Cos(angle) * distance,
                MathF.Sin(angle) * distance);

            if (Vector2.Distance(point, center) < exclusionRadius)
                continue;
            if (avoidTrails && ForestLayout.IsTrailCorridor(point))
                continue;

            AddInstance(
                output,
                terrain,
                random,
                $"{prefix}-{placed:000}",
                assets[random.Next(assets.Count)],
                point,
                minScale,
                maxScale);
            placed++;
        }

        if (placed != count)
            throw new InvalidOperationException(
                $"Could not place requested forest decoration '{prefix}': {placed}/{count}.");
    }

    private static void ScatterRing(
        List<WorldModelInstance> output,
        Terrain terrain,
        Random random,
        string prefix,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        int count,
        IReadOnlyList<string> assets,
        float minScale,
        float maxScale)
    {
        var placed = 0;
        var attempts = 0;

        while (placed < count && attempts++ < count * 50)
        {
            var angle = random.NextSingle() * MathF.Tau;
            var t = random.NextSingle();
            var radiusSquared =
                innerRadius * innerRadius +
                (outerRadius * outerRadius - innerRadius * innerRadius) * t;
            var distance = MathF.Sqrt(radiusSquared);
            var point = center + new Vector2(
                MathF.Cos(angle) * distance,
                MathF.Sin(angle) * distance);

            if (ForestLayout.IsTrailCorridor(point))
                continue;

            AddInstance(
                output,
                terrain,
                random,
                $"{prefix}-{placed:000}",
                assets[random.Next(assets.Count)],
                point,
                minScale,
                maxScale);
            placed++;
        }

        if (placed != count)
            throw new InvalidOperationException(
                $"Could not place requested transition decoration: {placed}/{count}.");
    }

    private static void AddInstance(
        List<WorldModelInstance> output,
        Terrain terrain,
        Random random,
        string id,
        string asset,
        Vector2 point,
        float minScale,
        float maxScale)
    {
        var position = new Vector3(point.X, 0f, point.Y);
        position.Y = terrain.SampleHeight(position);

        var uniformScale = minScale + random.NextSingle() * (maxScale - minScale);
        var scale = new Vector3(
            uniformScale * (0.93f + random.NextSingle() * 0.14f),
            uniformScale * (0.90f + random.NextSingle() * 0.22f),
            uniformScale * (0.93f + random.NextSingle() * 0.14f));

        output.Add(new WorldModelInstance(
            id,
            asset,
            position,
            scale,
            random.NextSingle() * MathF.Tau,
            Vector3.One));
    }
}
