using System.Numerics;

namespace SlavicGame.Engine.World;

// Shallow fordable river. The terrain owns the channel; the water ribbon follows
// the channel's local downstream level instead of floating at one global Y.
public static class WaterLandscape
{
    public const float Level = -3f;
    public const float BedDepth = 1.30f;
    public const float ShoreDepth = 0.08f;
    public const float BankInset = 1.40f;
    public const float BankShoulderWidth = 8f;
    public const float DownstreamSlope = 0.0012f;
    public const float R0FordZ = -110f;
    public const float R0BypassZ = -72f;

    private const float SurfaceStep = 4f;
    private const int SurfaceColumns = 5;

    public static float CenterX(float z) =>
        220f +
        48f * MathF.Sin(z * 0.007f) +
        18f * MathF.Sin(z * 0.019f);

    public static float HalfWidth(float z) =>
        14f + 3f * MathF.Sin(z * 0.011f);

    public static float SurfaceHalfWidth(float z) =>
        MathF.Max(2f, HalfWidth(z) - BankInset);

    public static float WaterLevel(float z) =>
        Level - z * DownstreamSlope;

    public static float BedLevel(float z) =>
        WaterLevel(z) - ChannelDepth(z);

    public static float ChannelDepth(float z)
    {
        var depth = BedDepth;
        depth = MathF.Min(
            depth,
            LocalCrossingDepth(
                z,
                R0FordZ,
                shallowDepth: 0.28f,
                halfLength: 7.5f));
        depth = MathF.Min(
            depth,
            LocalCrossingDepth(
                z,
                R0BypassZ,
                shallowDepth: 0.36f,
                halfLength: 6.0f));
        return depth;
    }

    public static float BankDistance(Vector2 point) =>
        MathF.Abs(point.X - CenterX(point.Y)) - HalfWidth(point.Y);

    public static Vector2 FlowDirection(float z)
    {
        const float sample = 1f;
        var previous = new Vector2(CenterX(z - sample), z - sample);
        var next = new Vector2(CenterX(z + sample), z + sample);
        var direction = next - previous;
        return direction.LengthSquared() > 0.000001f
            ? Vector2.Normalize(direction)
            : Vector2.UnitY;
    }

    public static float ShapeHeight(float x, float z, float original)
    {
        var offset = MathF.Abs(x - CenterX(z));
        var halfWidth = HalfWidth(z);
        var channelHalfWidth = SurfaceHalfWidth(z);
        var water = WaterLevel(z);

        if (offset <= channelHalfWidth)
        {
            var normalized = Math.Clamp(
                offset / MathF.Max(channelHalfWidth, 0.001f),
                0f,
                1f);
            var edgeBlend = SmoothStep(0.58f, 1f, normalized);
            var target = Lerp(
                water - ChannelDepth(z),
                water - ShoreDepth,
                edgeBlend);

            // Carve only downward inside the actual wetted channel.
            return MathF.Min(original, target);
        }

        var shoulderDistance = offset - channelHalfWidth;
        if (shoulderDistance >= BankShoulderWidth)
            return original;

        // Rise from the shallow wet edge back to untouched terrain. Unlike the
        // old implementation this never forces the dry bank below water level.
        var shoulderBlend = SmoothStep(
            0f,
            1f,
            shoulderDistance / BankShoulderWidth);
        var shore = water - ShoreDepth;
        return Lerp(shore, original, shoulderBlend);
    }

    public static void AppendSurface(
        Terrain terrain,
        float seconds,
        Vector3 camera,
        float range,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        var extentZ = (terrain.Depth - 1) * terrain.CellSize * 0.5f;
        var extentX = (terrain.Width - 1) * terrain.CellSize * 0.5f;

        for (var z = -extentZ; z < extentZ; z += SurfaceStep)
        {
            var end = MathF.Min(z + SurfaceStep, extentZ);
            var center = CenterX(z);
            var endCenter = CenterX(end);
            var width = SurfaceHalfWidth(z);
            var endWidth = SurfaceHalfWidth(end);

            if (center - width < -extentX || center + width > extentX)
                continue;
            if (endCenter - endWidth < -extentX || endCenter + endWidth > extentX)
                continue;

            var midpoint = new Vector2(
                (center + endCenter) * 0.5f,
                (z + end) * 0.5f);
            if (Vector2.Distance(
                    midpoint,
                    new Vector2(camera.X, camera.Z)) > range + 30f)
            {
                continue;
            }

            var startIndex = (uint)output.Count;
            for (var column = 0; column < SurfaceColumns; column++)
            {
                var across = column / (float)(SurfaceColumns - 1) * 2f - 1f;
                AddWaterVertex(center + across * width, z, across);
            }

            for (var column = 0; column < SurfaceColumns; column++)
            {
                var across = column / (float)(SurfaceColumns - 1) * 2f - 1f;
                AddWaterVertex(endCenter + across * endWidth, end, across);
            }

            for (uint column = 0; column < SurfaceColumns - 1; column++)
            {
                var a = startIndex + column;
                var b = a + 1;
                var d = startIndex + SurfaceColumns + column;
                var c = d + 1;

                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(a);
                triangles.Add(d);
                triangles.Add(c);
            }
        }

        AppendFoamStreaks(
            output,
            triangles,
            seconds,
            camera,
            range,
            extentZ,
            extentX);

        vertices = output.ToArray();
        indices = triangles.ToArray();

        void AddWaterVertex(float x, float z, float across)
        {
            var primaryPhase = z * 0.34f - seconds * 2.35f + across * 1.7f;
            var secondaryPhase = z * 0.71f - seconds * 3.7f - across * 2.2f;
            var primary = 0.5f + 0.5f * MathF.Sin(primaryPhase);
            var secondary = 0.5f + 0.5f * MathF.Sin(secondaryPhase);
            var flow = Math.Clamp(primary * 0.72f + secondary * 0.28f, 0f, 1f);

            // Tiny moving surface displacement makes the downstream motion
            // readable even without expensive normal/reflection passes.
            var wave =
                MathF.Sin(primaryPhase) * 0.025f +
                MathF.Sin(secondaryPhase) * 0.012f;

            var edge = MathF.Abs(across);
            var deep = new Vector3(0.025f, 0.135f, 0.18f);
            var bright = new Vector3(0.085f, 0.31f, 0.34f);
            var shallow = new Vector3(0.13f, 0.30f, 0.27f);
            var color = Vector3.Lerp(deep, bright, flow * 0.72f);
            color = Vector3.Lerp(color, shallow, edge * 0.30f);

            output.Add(new TerrainVertex(
                new Vector3(x, WaterLevel(z) + wave, z),
                color,
                Vector3.UnitY));
        }
    }

    private static void AppendFoamStreaks(
        List<TerrainVertex> vertices,
        List<uint> indices,
        float seconds,
        Vector3 camera,
        float range,
        float extentZ,
        float extentX)
    {
        const float step = 32f;
        const float startZ = -920f;

        for (var z = startZ; z <= extentZ; z += step)
        {
            if (z < -extentZ)
                continue;

            var centerX = CenterX(z);
            var halfWidth = SurfaceHalfWidth(z);
            var flow = FlowDirection(z);
            var tangent = Vector2.Normalize(flow);
            var sideVector = new Vector2(-tangent.Y, tangent.X);

            for (var side = -1; side <= 1; side += 2)
            {
                var x = centerX + side * (halfWidth - 0.28f);
                if (x < -extentX || x > extentX)
                    continue;

                var position2 = new Vector2(x, z);
                if (Vector2.Distance(
                        position2,
                        new Vector2(camera.X, camera.Z)) > range + 20f)
                {
                    continue;
                }

                var pulse =
                    0.5f +
                    0.5f * MathF.Sin(
                        z * 0.17f -
                        seconds * 2.9f +
                        side * 0.8f);
                var length = 1.8f + pulse * 1.6f;
                var width = 0.07f + pulse * 0.08f;
                var along = tangent * (length * 0.5f);
                var across = sideVector * (width * side);
                var baseCenter = position2 + tangent * (pulse - 0.5f) * 0.8f;
                var y = WaterLevel(z) + 0.055f;

                var a2 = baseCenter - along - across;
                var b2 = baseCenter - along + across;
                var c2 = baseCenter + along + across;
                var d2 = baseCenter + along - across;
                var color = Vector3.Lerp(
                    new Vector3(0.26f, 0.47f, 0.48f),
                    new Vector3(0.62f, 0.79f, 0.77f),
                    0.35f + pulse * 0.45f);

                var start = checked((uint)vertices.Count);
                vertices.Add(new TerrainVertex(new Vector3(a2.X, y, a2.Y), color, Vector3.UnitY));
                vertices.Add(new TerrainVertex(new Vector3(b2.X, y, b2.Y), color, Vector3.UnitY));
                vertices.Add(new TerrainVertex(new Vector3(c2.X, y, c2.Y), color, Vector3.UnitY));
                vertices.Add(new TerrainVertex(new Vector3(d2.X, y, d2.Y), color, Vector3.UnitY));

                indices.Add(start);
                indices.Add(start + 2);
                indices.Add(start + 1);
                indices.Add(start);
                indices.Add(start + 3);
                indices.Add(start + 2);
            }
        }
    }

    private static float LocalCrossingDepth(
        float z,
        float centerZ,
        float shallowDepth,
        float halfLength)
    {
        var distance = MathF.Abs(z - centerZ);
        if (distance >= halfLength)
            return BedDepth;

        var t = SmoothStep(
            0f,
            1f,
            distance / halfLength);
        return Lerp(shallowDepth, BedDepth, t);
    }

    private static float Lerp(float a, float b, float t) =>
        a + (b - a) * Math.Clamp(t, 0f, 1f);

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        if (edge1 <= edge0)
            return value >= edge1 ? 1f : 0f;

        var t = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}
