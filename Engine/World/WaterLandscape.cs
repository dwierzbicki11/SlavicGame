using System.Numerics;

namespace SlavicGame.Engine.World;

// A shallow, fordable river: water is visual; swimming is a future mechanic.
public static class WaterLandscape
{
    public const float Level = -3f;
    public static float CenterX(float z) => 220f + 48f * MathF.Sin(z * 0.007f) + 18f * MathF.Sin(z * 0.019f);
    public static float HalfWidth(float z) => 14f + 3f * MathF.Sin(z * 0.011f);
    public static float BankDistance(Vector2 point) => MathF.Abs(point.X - CenterX(point.Y)) - HalfWidth(point.Y);

    public static float ShapeHeight(float x, float z, float original)
    {
        var distance = BankDistance(new Vector2(x, z));
        var blend = Math.Clamp((18f - distance) / 18f, 0f, 1f);
        blend = blend * blend * (3f - 2f * blend);
        // Cap banks below the water at the channel edge, even on high ground.
        return original + (MathF.Min(original, Level - 0.7f) - original) * blend;
    }

    public static void AppendSurface(Terrain terrain, float seconds, Vector3 camera, float range,
        ref TerrainVertex[] vertices, ref uint[] indices)
    {
        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        var extentZ = (terrain.Depth - 1) * terrain.CellSize * 0.5f;
        var extentX = (terrain.Width - 1) * terrain.CellSize * 0.5f;
        // Two triangles per 8 metres, reusing the existing actor draw call.
        for (var z = -extentZ; z < extentZ; z += 8f)
        {
            var end = MathF.Min(z + 8f, extentZ);
            var x = CenterX(z);
            if (x - HalfWidth(z) < -extentX || x + HalfWidth(z) > extentX) continue;
            if (CenterX(end) - HalfWidth(end) < -extentX || CenterX(end) + HalfWidth(end) > extentX) continue;
            if (Vector2.Distance(new Vector2(x, z), new Vector2(camera.X, camera.Z)) > range + 30f) continue;
            var start = (uint)output.Count;
            Add(x - HalfWidth(z), z);
            Add(x + HalfWidth(z), z);
            Add(CenterX(end) + HalfWidth(end), end);
            Add(CenterX(end) - HalfWidth(end), end);
            triangles.AddRange(new uint[] { start, start + 2, start + 1, start, start + 3, start + 2 });
        }
        vertices = output.ToArray();
        indices = triangles.ToArray();

        void Add(float x, float z)
        {
            var shimmer = 0.5f + 0.5f * MathF.Sin(z * 0.32f - seconds * 1.6f);
            output.Add(new TerrainVertex(new Vector3(x, Level, z),
                Vector3.Lerp(new Vector3(0.035f, 0.16f, 0.19f), new Vector3(0.10f, 0.28f, 0.30f), shimmer)));
        }
    }
}
