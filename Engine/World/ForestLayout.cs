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

public static class ForestLayout
{
    public static IReadOnlyList<ForestZone> Zones { get; } =
    [
        new ForestZone(
            "deep-oak-forest",
            "Dębowa Knieja",
            ForestBiome.Oak,
            new Vector2(-520f, 430f),
            300f,
            105,
            48,
            28f),
        new ForestZone(
            "perun-pinewood",
            "Bór Perunowy",
            ForestBiome.Pine,
            new Vector2(-535f, -430f),
            285f,
            115,
            42,
            24f),
        new ForestZone(
            "birch-grove",
            "Brzozowe Łęgi",
            ForestBiome.Birch,
            new Vector2(505f, 435f),
            250f,
            82,
            44,
            30f),
        new ForestZone(
            "wet-forest",
            "Mokry Bór",
            ForestBiome.Wetland,
            new Vector2(530f, -360f),
            270f,
            74,
            52,
            26f)
    ];

    public static bool IsTrailCorridor(Vector2 point)
    {
        foreach (var zone in Zones)
        {
            if (DistanceToSegment(point, Vector2.Zero, zone.Center) < 11f)
                return true;
        }

        return false;
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
