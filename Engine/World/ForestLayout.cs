using System.Numerics;

namespace SlavicGame.Engine.World;

public enum ForestBiome
{
    Mixed,
    Oak,
    Pine,
    Birch,
    Wetland
}

public sealed record ForestZone(
    string Id,
    string Name,
    ForestBiome Biome,
    Vector2 Center,
    float Radius,
    int TreeCount,
    int UnderstoryCount,
    float ClearingRadius);

public sealed record ForestTrail(
    string Id,
    Vector2 Start,
    Vector2 End,
    float CorridorHalfWidth,
    float SwayAmplitude,
    float SwayFrequency,
    float SwayPhase);

public static class ForestLayout
{
    public static readonly Vector2 VillageCenter = new(0f, -85f);
    public static readonly Vector2 VillageNorthGate = new(0f, -61f);
    public static readonly Vector2 VillageSouthGate = new(0f, -108f);
    public static readonly Vector2 ShrineCenter = new(-85f, 55f);
    public static readonly Vector2 SwampCenter = new(95f, 35f);
    public static readonly Vector2 HunterCampCenter = new(37f, 21f);

    // Major Slavic Settlements, Outposts & Crossings
    public static readonly Vector2 StrongholdCenter = new(-320f, 220f);
    public static readonly Vector2 StrongholdGate = new(-318f, 188f);
    public static readonly Vector2 FishingHamletCenter = new(148f, -180f);
    public static readonly Vector2 RiverBridgeCenter = new(179.25f, -180f);
    public static readonly Vector2 WoodcutterCampCenter = new(-175f, -280f);
    public static readonly Vector2 CaravanCampCenter = new(-155f, 95f);

    public static IReadOnlyList<ForestZone> Zones { get; } =
    [
        new ForestZone(
            "deep-oak-forest",
            "Dębowa Knieja",
            ForestBiome.Oak,
            new Vector2(-520f, 430f),
            320f,
            750,
            450,
            26f),
        new ForestZone(
            "perun-pinewood",
            "Bór Perunowy",
            ForestBiome.Pine,
            new Vector2(-535f, -430f),
            305f,
            800,
            450,
            24f),
        new ForestZone(
            "birch-grove",
            "Brzozowe Łęgi",
            ForestBiome.Birch,
            new Vector2(505f, 435f),
            275f,
            650,
            400,
            28f),
        new ForestZone(
            "wet-forest",
            "Mokry Bór",
            ForestBiome.Wetland,
            new Vector2(530f, -360f),
            290f,
            600,
            450,
            25f)
    ];

    public static IReadOnlyList<ForestTrail> Trails { get; } =
    [
        // Main settlement road through Puszcza Żywia and Żarnowiec
        new ForestTrail("road-starting-to-village-north", Vector2.Zero, VillageNorthGate, 7.5f, 3.2f, 2.0f, 0.4f),
        new ForestTrail("road-village-internal", VillageNorthGate, VillageSouthGate, 9.5f, 1.2f, 1.0f, 0.0f),
        new ForestTrail("road-village-south-outskirts", VillageSouthGate, new Vector2(0f, -165f), 7.0f, 3.5f, 1.5f, 1.1f),

        // Sacred paths to Kamienny Krąg (Stone Circle)
        new ForestTrail("trail-clearing-to-shrine", Vector2.Zero, ShrineCenter, 6.5f, 4.8f, 2.2f, 0.9f),
        new ForestTrail("trail-village-to-shrine", VillageNorthGate, ShrineCenter, 6.2f, 5.4f, 2.5f, 2.1f),

        // Paths to Hunter Camp and Black Swamp crossing
        new ForestTrail("trail-clearing-to-camp", Vector2.Zero, HunterCampCenter, 5.8f, 2.8f, 1.8f, 1.6f),
        new ForestTrail("trail-camp-to-swamp", HunterCampCenter, SwampCenter, 6.0f, 3.4f, 2.0f, 0.7f),
        new ForestTrail("trail-village-to-swamp", VillageNorthGate, SwampCenter, 6.0f, 5.2f, 2.4f, 2.8f),

        // Great primeval forest dukty (tracks radiating to the four outer forest biomes)
        new ForestTrail("dukt-deep-oak", ShrineCenter, new Vector2(-520f, 430f), 8.5f, 9.5f, 3.5f, 0.5f),
        new ForestTrail("dukt-perun-pine", new Vector2(0f, -155f), new Vector2(-535f, -430f), 8.5f, 9.0f, 3.2f, 1.7f),
        new ForestTrail("dukt-birch-grove", HunterCampCenter, new Vector2(505f, 435f), 8.5f, 9.5f, 3.4f, 2.4f),
        new ForestTrail("dukt-wet-forest", SwampCenter, new Vector2(530f, -360f), 8.5f, 8.8f, 3.1f, 3.0f),

        // Direct radial forest corridors from origin
        new ForestTrail("radial-deep-oak", Vector2.Zero, new Vector2(-520f, 430f), 8.5f, 7.5f, 3.0f, 0.2f),
        new ForestTrail("radial-perun-pine", Vector2.Zero, new Vector2(-535f, -430f), 8.5f, 7.5f, 3.0f, 1.3f),
        new ForestTrail("radial-birch-grove", Vector2.Zero, new Vector2(505f, 435f), 8.5f, 7.5f, 3.0f, 2.5f),
        new ForestTrail("radial-wet-forest", Vector2.Zero, new Vector2(530f, -360f), 8.5f, 7.5f, 3.0f, 3.7f),

        // Trade Trakt to Gród Borzywoj (West Stronghold)
        new ForestTrail("trakt-shrine-to-caravan", ShrineCenter, CaravanCampCenter, 7.5f, 3.8f, 2.0f, 0.8f),
        new ForestTrail("trakt-caravan-to-stronghold", CaravanCampCenter, StrongholdGate, 8.0f, 4.6f, 2.2f, 1.4f),
        new ForestTrail("trakt-stronghold-to-oak-heart", StrongholdCenter, new Vector2(-520f, 430f), 8.5f, 5.5f, 2.5f, 2.0f),

        // Forest Trakt to Osada Smolników (Woodcutters & Charcoal Burners)
        new ForestTrail("trakt-village-to-woodcutter", new Vector2(0f, -155f), WoodcutterCampCenter, 7.5f, 4.8f, 2.1f, 1.8f),
        new ForestTrail("trakt-woodcutter-to-perun", WoodcutterCampCenter, new Vector2(-535f, -430f), 8.0f, 5.2f, 2.3f, 0.6f),

        // River Trakt to Przystań Rybacka and Eastern Bridge Crossing
        new ForestTrail("trakt-village-to-fishing", new Vector2(0f, -155f), FishingHamletCenter, 7.5f, 4.2f, 2.0f, 1.2f),
        new ForestTrail("trakt-fishing-to-bridge", FishingHamletCenter, RiverBridgeCenter, 8.0f, 1.2f, 1.0f, 0.0f),
        new ForestTrail("trakt-bridge-to-wet-forest", RiverBridgeCenter, new Vector2(530f, -360f), 8.0f, 6.2f, 2.6f, 2.2f),
        new ForestTrail("trakt-bridge-to-birch", RiverBridgeCenter, new Vector2(505f, 435f), 8.0f, 7.0f, 2.8f, 1.9f)
    ];

    public static bool IsTrailCorridor(Vector2 point)
    {
        foreach (var zone in Zones)
        {
            if (DistanceToSegment(point, Vector2.Zero, zone.Center) < 11f)
                return true;
        }

        foreach (var trail in Trails)
        {
            if (DistanceToMeanderingTrail(point, trail) < trail.CorridorHalfWidth)
                return true;
        }

        return false;
    }

    public static bool IsSettlementOrClearing(Vector2 point)
    {
        if (Vector2.Distance(point, VillageCenter) < 54f)
            return true;
        if (Vector2.Distance(point, Vector2.Zero) < 14f)
            return true;
        if (Vector2.Distance(point, ShrineCenter) < 15.5f)
            return true;
        if (Vector2.Distance(point, HunterCampCenter) < 9.5f)
            return true;
        if (Vector2.Distance(point, SwampCenter) < 13f)
            return true;
        if (Vector2.Distance(point, StrongholdCenter) < 46f)
            return true;
        if (Vector2.Distance(point, FishingHamletCenter) < 32f)
            return true;
        if (Vector2.Distance(point, WoodcutterCampCenter) < 28f)
            return true;
        if (Vector2.Distance(point, CaravanCampCenter) < 16f)
            return true;

        return false;
    }

    public static float TrailDistance(Vector2 point)
    {
        var best = float.MaxValue;
        foreach (var trail in Trails)
        {
            var d = DistanceToMeanderingTrail(point, trail);
            if (d < best)
                best = d;
        }

        foreach (var zone in Zones)
        {
            var d = DistanceToSegment(point, Vector2.Zero, zone.Center);
            if (d < best)
                best = d;
        }

        return best;
    }

    public static Vector2 SampleMeanderingTrailPoint(ForestTrail trail, float t, float lateralOffset = 0f)
    {
        t = Math.Clamp(t, 0f, 1f);
        var ab = trail.End - trail.Start;
        var len = ab.Length();
        if (len < 0.001f)
            return trail.Start;

        var dir = ab / len;
        var perp = new Vector2(-dir.Y, dir.X);
        var envelope = MathF.Sin(t * MathF.PI);
        var sway = MathF.Sin(t * MathF.PI * trail.SwayFrequency + trail.SwayPhase) * trail.SwayAmplitude * envelope;
        return trail.Start + ab * t + perp * (sway + lateralOffset);
    }

    private static float DistanceToMeanderingTrail(Vector2 point, ForestTrail trail)
    {
        var ab = trail.End - trail.Start;
        var lengthSquared = ab.LengthSquared();
        if (lengthSquared < 0.0001f)
            return Vector2.Distance(point, trail.Start);

        var t = Math.Clamp(Vector2.Dot(point - trail.Start, ab) / lengthSquared, 0f, 1f);
        var centerPoint = SampleMeanderingTrailPoint(trail, t);
        return Vector2.Distance(point, centerPoint);
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var lengthSquared = ab.LengthSquared();
        if (lengthSquared < 0.0001f)
            return Vector2.Distance(point, a);

        var t = Math.Clamp(Vector2.Dot(point - a, ab) / lengthSquared, 0f, 1f);
        return Vector2.Distance(point, a + ab * t);
    }
}
