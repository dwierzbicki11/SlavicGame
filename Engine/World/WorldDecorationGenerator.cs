using System.Numerics;

namespace SlavicGame.Engine.World;

public static class WorldDecorationGenerator
{
    private const int Seed = 0x534C4156;

    private static readonly string[] MixedTrees =
    [
        "models/static/dab_stary_r0_01.glb",
        "models/static/dab_stary_r0_02.glb",
        "models/static/dab_r0_02.glb",
        "models/static/brzoza_r0_01.glb",
        "models/static/brzoza_r0_02.glb",
        "models/static/sosna_r0_01.glb",
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
        "models/static/grzyby_r0_01.glb",
        "models/static/powalone_drzewo_r0_01.glb"
    ];

    private static readonly string[] TrailVergeUnderstory =
    [
        "models/static/paproc_r0_01.glb",
        "models/static/krzak_r0_02.glb",
        "models/static/krzak_r0_03.glb",
        "models/static/korzenie_r0_01.glb",
        "models/static/grzyby_r0_01.glb",
        "models/static/powalone_drzewo_r0_01.glb"
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
        var result = new List<WorldModelInstance>(10200);

        // 1. Extremely dense primeval Puszcza Żywia around the starting clearing
        ScatterDisc(
            result, terrain, random,
            prefix: "starting-forest-tree",
            center: Vector2.Zero,
            radius: 115f,
            count: 450,
            assets: MixedTrees,
            minScale: 1.05f,
            maxScale: 1.75f,
            exclusionRadius: 14f,
            avoidTrails: true);

        ScatterDisc(
            result, terrain, random,
            prefix: "starting-forest-understory",
            center: Vector2.Zero,
            radius: 110f,
            count: 450,
            assets: ForestUnderstory,
            minScale: 0.95f,
            maxScale: 1.75f,
            exclusionRadius: 12f,
            avoidTrails: true);

        // 2. Continuous impenetrable R0 puszcza belt wrapping around Żarnowiec, Kamienny Krąg, and the river
        ScatterRing(
            result, terrain, random,
            prefix: "r0-puszcza-tree",
            center: Vector2.Zero,
            innerRadius: 68f,
            outerRadius: 240f,
            count: 850,
            assets: MixedTrees,
            minScale: 1.08f,
            maxScale: 1.80f);

        ScatterRing(
            result, terrain, random,
            prefix: "r0-puszcza-understory",
            center: Vector2.Zero,
            innerRadius: 64f,
            outerRadius: 235f,
            count: 750,
            assets: ForestUnderstory,
            minScale: 0.92f,
            maxScale: 1.70f);

        // 3. Dense ancient oak grove (Święty Gaj) encircling Kamienny Krąg
        ScatterDisc(
            result, terrain, random,
            prefix: "shrine-sacred-grove-tree",
            center: ForestLayout.ShrineCenter,
            radius: 65f,
            count: 220,
            assets: OakTrees,
            minScale: 1.15f,
            maxScale: 1.85f,
            exclusionRadius: 16.5f,
            avoidTrails: true);

        ScatterDisc(
            result, terrain, random,
            prefix: "shrine-sacred-grove-understory",
            center: ForestLayout.ShrineCenter,
            radius: 62f,
            count: 180,
            assets: ForestUnderstory,
            minScale: 0.95f,
            maxScale: 1.70f,
            exclusionRadius: 15.5f,
            avoidTrails: true);

        // 4. Trailside arched canopy & thick undergrowth forming a green tunnel over all paths
        ScatterTrailVerges(
            result, terrain, random,
            prefix: "trail-verge-tree",
            count: 320,
            assets: MixedTrees,
            minExtraOffset: 0.8f,
            maxExtraOffset: 6.2f,
            minScale: 1.10f,
            maxScale: 1.75f);

        ScatterTrailVerges(
            result, terrain, random,
            prefix: "trail-verge-understory",
            count: 380,
            assets: TrailVergeUnderstory,
            minExtraOffset: 0.2f,
            maxExtraOffset: 4.5f,
            minScale: 0.90f,
            maxScale: 1.65f);

        // 5. Impenetrable Black Swamp alder thicket, snags, and reed beds
        ScatterDisc(
            result, terrain, random,
            prefix: "swamp-growth",
            center: ForestLayout.SwampCenter,
            radius: 62f,
            count: 240,
            assets: SwampVegetation,
            minScale: 0.95f,
            maxScale: 1.70f,
            exclusionRadius: 11f,
            avoidTrails: true);

        // 6. Deep continuous woodland connecting the central valley to the four outer forest biomes
        ScatterRing(
            result, terrain, random,
            prefix: "transition-tree",
            center: Vector2.Zero,
            innerRadius: 200f,
            outerRadius: 880f,
            count: 750,
            assets: MixedTrees,
            minScale: 1.05f,
            maxScale: 1.75f);

        ScatterRing(
            result, terrain, random,
            prefix: "transition-understory",
            center: Vector2.Zero,
            innerRadius: 200f,
            outerRadius: 840f,
            count: 450,
            assets: ForestUnderstory,
            minScale: 0.90f,
            maxScale: 1.65f);

        // 7. Four massive outer Slavic primeval forests (Dębowa Knieja, Bór Perunowy, Brzozowe Łęgi, Mokry Bór)
        foreach (var zone in ForestLayout.Zones)
            ScatterForestZone(result, terrain, random, zone);

        // 8. Riparian reeds and alder groves following both riverbanks
        for (var i = 0; i < 100; i++)
        {
            var z = -940f + i * 19f;
            var side = i % 2 == 0 ? -1f : 1f;
            var x = WaterLandscape.CenterX(z) + side * (WaterLandscape.HalfWidth(z) + 5f + random.NextSingle() * 9f);
            AddInstance(result, terrain, random, $"riverbank-{i:000}",
                i % 5 == 0 ? WetlandTrees[0] : SwampVegetation[1 + i % 3],
                new Vector2(x, z), 0.95f, 1.50f);
        }

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
            radius: zone.Radius * 0.74f,
            count: zone.TreeCount,
            assets: treeAssets,
            minScale: zone.Biome == ForestBiome.Pine ? 1.15f : 1.05f,
            maxScale: zone.Biome == ForestBiome.Pine ? 1.82f : 1.74f,
            exclusionRadius: zone.ClearingRadius,
            avoidTrails: true);

        ScatterDisc(
            output, terrain, random,
            prefix: $"{zone.Id}-understory",
            center: zone.Center,
            radius: zone.Radius * 0.78f,
            count: zone.UnderstoryCount,
            assets: understoryAssets,
            minScale: 0.88f,
            maxScale: 1.70f,
            exclusionRadius: zone.ClearingRadius * 0.75f,
            avoidTrails: true);
    }

    private static void ScatterTrailVerges(
        List<WorldModelInstance> output,
        Terrain terrain,
        Random random,
        string prefix,
        int count,
        IReadOnlyList<string> assets,
        float minExtraOffset,
        float maxExtraOffset,
        float minScale,
        float maxScale)
    {
        var placed = 0;
        var attempts = 0;
        var maxAttempts = Math.Max(256, count * 150);
        var trails = ForestLayout.Trails;

        while (placed < count && attempts++ < maxAttempts)
        {
            var trailIndex = random.NextSingle() < 0.72f
                ? random.Next(Math.Min(7, trails.Count))
                : random.Next(trails.Count);
            var trail = trails[trailIndex];
            if (trail.Id == "road-village-internal")
                continue;

            var t = 0.06f + random.NextSingle() * 0.88f;
            var side = random.Next(2) == 0 ? -1f : 1f;
            var offset = trail.CorridorHalfWidth + minExtraOffset + random.NextSingle() * (maxExtraOffset - minExtraOffset);
            var point = ForestLayout.SampleMeanderingTrailPoint(trail, t, side * offset);

            if (WaterLandscape.BankDistance(point) < 4.5f)
                continue;
            if (ForestLayout.IsSettlementOrClearing(point))
                continue;
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
                $"Could not place requested trail verge decoration '{prefix}': {placed}/{count}.");
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
        var maxAttempts = Math.Max(256, count * 150);

        while (placed < count && attempts++ < maxAttempts)
        {
            var angle = random.NextSingle() * MathF.Tau;
            var distance = MathF.Sqrt(random.NextSingle()) * radius;
            var point = center + new Vector2(
                MathF.Cos(angle) * distance,
                MathF.Sin(angle) * distance);

            if (WaterLandscape.BankDistance(point) < 4f)
                continue;
            if (Vector2.Distance(point, center) < exclusionRadius)
                continue;
            if (ForestLayout.IsSettlementOrClearing(point))
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
        var maxAttempts = count * 150;

        while (placed < count && attempts++ < maxAttempts)
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

            if (WaterLandscape.BankDistance(point) < 4f)
                continue;
            if (ForestLayout.IsSettlementOrClearing(point))
                continue;
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
        // Slightly embed base into terrain so roots and stems never hover on micro-slopes
        position.Y = terrain.SampleHeight(position) - GetPlacementSinkOffset(asset);

        var isTree = IsTreeAsset(asset);
        var tierMultiplier = 1.0f;
        if (isTree)
        {
            var roll = random.NextSingle();
            tierMultiplier = roll switch
            {
                < 0.32f => 1.38f + random.NextSingle() * 0.44f, // Towering ancient canopy giants
                < 0.82f => 1.02f + random.NextSingle() * 0.32f, // Mature dense forest trees
                _       => 0.72f + random.NextSingle() * 0.28f  // Mid-canopy saplings & pole timber
            };
        }

        var uniformScale = (minScale + random.NextSingle() * (maxScale - minScale)) * tierMultiplier;
        var speciesProfile = TreeScaleProfile(asset);
        var scale = new Vector3(
            uniformScale * (0.93f + random.NextSingle() * 0.14f) * speciesProfile.X,
            uniformScale * (0.90f + random.NextSingle() * 0.22f) * speciesProfile.Y,
            uniformScale * (0.93f + random.NextSingle() * 0.14f) * speciesProfile.Z);

        output.Add(new WorldModelInstance(
            id,
            asset,
            position,
            scale,
            random.NextSingle() * MathF.Tau,
            Vector3.One));
    }

    private static float GetPlacementSinkOffset(string asset)
    {
        var name = Path.GetFileNameWithoutExtension(asset);

        if (name.StartsWith("grzyby_", StringComparison.OrdinalIgnoreCase))
            return 0.005f;

        if (name.StartsWith("paproc_", StringComparison.OrdinalIgnoreCase))
            return 0.015f;

        if (name.StartsWith("krzak_", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("korzenie_", StringComparison.OrdinalIgnoreCase))
            return 0.030f;

        if (name.StartsWith("trzciny_", StringComparison.OrdinalIgnoreCase))
            return 0.040f;

        return 0.080f;
    }

    private static Vector3 TreeScaleProfile(string asset)
    {
        var name = Path.GetFileNameWithoutExtension(asset);

        if (name.StartsWith("sosna_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.65f, 3.15f, 1.65f);

        if (name.StartsWith("brzoza_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.60f, 2.50f, 1.60f);

        if (name.StartsWith("olsza_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(2.05f, 2.10f, 2.05f);

        if (name.StartsWith("dab_", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("dab_stary_", StringComparison.OrdinalIgnoreCase))
        {
            return new Vector3(2.45f, 2.30f, 2.45f);
        }

        if (name.StartsWith("paproc_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.85f, 1.65f, 1.85f);

        if (name.StartsWith("krzak_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(2.05f, 1.75f, 2.05f);

        if (name.StartsWith("trzciny_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.20f, 1.55f, 1.20f);

        if (name.StartsWith("pien_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.50f, 1.25f, 1.50f);

        if (name.StartsWith("grzyby_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.45f, 1.25f, 1.45f);

        if (name.StartsWith("korzenie_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.75f, 1.25f, 1.75f);

        if (name.StartsWith("powalone_drzewo_", StringComparison.OrdinalIgnoreCase))
            return new Vector3(1.75f, 1.50f, 1.75f);

        return Vector3.One;
    }

    private static bool IsTreeAsset(string assetPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(assetPath);
        return fileName.StartsWith("dab_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("dab_stary_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("brzoza_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("sosna_", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("olsza_", StringComparison.OrdinalIgnoreCase);
    }
}
