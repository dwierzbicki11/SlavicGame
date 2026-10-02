using System.Numerics;

namespace SlavicGame.Engine.World;

public readonly record struct TerrainSurfaceWeights(
    float Grass,
    float ForestLitter,
    float Path,
    float Mud,
    float Swamp,
    float Rock)
{
    public Vector3 Primary => new(Grass, ForestLitter, Path);
    public Vector3 Secondary => new(Mud, Swamp, Rock);
}

public static class TerrainSurfaceClassifier
{
    private static readonly Vector2 VillageCenter = new(0f, -85f);
    private static readonly Vector2 SwampCenter = new(95f, 35f);

    public static TerrainSurfaceWeights Classify(Vector3 position, Vector3 normal)
    {
        var point = new Vector2(position.X, position.Z);
        var slope = 1f - Math.Clamp(normal.Y, 0f, 1f);

        var forestMask = CircleMask(point, Vector2.Zero, 68f, 115f);
        foreach (var zone in ForestLayout.Zones)
        {
            forestMask = MathF.Max(
                forestMask,
                CircleMask(
                    point,
                    zone.Center,
                    zone.Radius * 0.48f,
                    zone.Radius * 0.82f));
        }

        var trailDistance = DistanceToSegment(point, Vector2.Zero, VillageCenter);
        foreach (var zone in ForestLayout.Zones)
            trailDistance = MathF.Min(
                trailDistance,
                DistanceToSegment(point, Vector2.Zero, zone.Center));

        var pathMask = 1f - SmoothStep(4.2f, 11.5f, trailDistance);
        pathMask = MathF.Max(
            pathMask,
            CircleMask(point, VillageCenter, 18f, 42f) * 0.78f);

        var swampMask = CircleMask(point, SwampCenter, 38f, 78f);
        var moistureNoise =
            0.5f +
            0.25f * MathF.Sin(position.X * 0.083f) +
            0.25f * MathF.Cos(position.Z * 0.071f);
        moistureNoise = Math.Clamp(moistureNoise, 0f, 1f);

        var rock = SmoothStep(0.18f, 0.50f, slope);
        var mud = swampMask * (0.28f + moistureNoise * 0.46f);
        var swamp = swampMask * (1f - mud * 0.52f);
        var path = pathMask * (1f - swampMask * 0.72f);

        var litterVariation =
            0.76f +
            0.14f * MathF.Sin(position.X * 0.037f + position.Z * 0.029f);
        var litter = forestMask * Math.Clamp(litterVariation, 0.58f, 0.92f);
        var grass = MathF.Max(0.08f, 1f - litter * 0.78f);

        // Strong semantic surfaces suppress the generic forest/grass base.
        var swampSuppression = 1f - Math.Clamp(swampMask, 0f, 1f);
        grass *= swampSuppression;
        litter *= swampSuppression;

        var pathSuppression = 1f - Math.Clamp(path, 0f, 0.94f);
        grass *= pathSuppression;
        litter *= pathSuppression;

        var rockSuppression = 1f - rock;
        grass *= rockSuppression;
        litter *= rockSuppression;
        path *= rockSuppression;
        mud *= rockSuppression;
        swamp *= rockSuppression;

        return Normalize(new TerrainSurfaceWeights(
            grass,
            litter,
            path,
            mud,
            swamp,
            rock));
    }

    private static TerrainSurfaceWeights Normalize(TerrainSurfaceWeights weights)
    {
        var total =
            weights.Grass +
            weights.ForestLitter +
            weights.Path +
            weights.Mud +
            weights.Swamp +
            weights.Rock;

        if (total <= 0.0001f)
            return new TerrainSurfaceWeights(1f, 0f, 0f, 0f, 0f, 0f);

        return new TerrainSurfaceWeights(
            weights.Grass / total,
            weights.ForestLitter / total,
            weights.Path / total,
            weights.Mud / total,
            weights.Swamp / total,
            weights.Rock / total);
    }

    private static float CircleMask(
        Vector2 point,
        Vector2 center,
        float innerRadius,
        float outerRadius)
    {
        var distance = Vector2.Distance(point, center);
        return 1f - SmoothStep(innerRadius, outerRadius, distance);
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var lengthSquared = ab.LengthSquared();
        if (lengthSquared < 0.0001f)
            return Vector2.Distance(point, a);

        var t = Math.Clamp(
            Vector2.Dot(point - a, ab) / lengthSquared,
            0f,
            1f);
        return Vector2.Distance(point, a + ab * t);
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var t = Math.Clamp(
            (value - edge0) / MathF.Max(edge1 - edge0, 0.0001f),
            0f,
            1f);
        return t * t * (3f - 2f * t);
    }
}
