using System.Numerics;

namespace SlavicGame.Engine.World;

public readonly struct TerrainVertex
{
    public const uint SizeInBytes = 36;

    public readonly Vector3 Position;
    public readonly Vector3 Color;
    public readonly Vector3 Normal;

    public TerrainVertex(Vector3 position, Vector3 color)
        : this(position, color, Vector3.UnitY)
    {
    }

    public TerrainVertex(Vector3 position, Vector3 color, Vector3 normal)
    {
        Position = position;
        Color = color;
        Normal = normal.LengthSquared() > 0.000001f
            ? Vector3.Normalize(normal)
            : Vector3.UnitY;
    }
}

public static class TerrainMesh
{
    public static void Build(Terrain terrain, out TerrainVertex[] vertices, out uint[] indices) =>
        Build(terrain, 1, out vertices, out indices);

    public static void Build(
        Terrain terrain,
        int gridStep,
        out TerrainVertex[] vertices,
        out uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        if (gridStep < 1)
            throw new ArgumentOutOfRangeException(nameof(gridStep));

        var xs = BuildSampleIndices(terrain.Width, gridStep);
        var zs = BuildSampleIndices(terrain.Depth, gridStep);
        vertices = new TerrainVertex[checked(xs.Length * zs.Length)];

        for (var xi = 0; xi < xs.Length; xi++)
        for (var zi = 0; zi < zs.Length; zi++)
        {
            var x = xs[xi];
            var z = zs[zi];
            var px = (x - (terrain.Width - 1) * 0.5f) * terrain.CellSize;
            var pz = (z - (terrain.Depth - 1) * 0.5f) * terrain.CellSize;
            var height = terrain.GetHeight(x, z);
            var normal = CalculateNormal(terrain, x, z);

            var normalizedHeight = Math.Clamp((height + 5f) / 10f, 0f, 1f);
            var slope = 1f - Math.Clamp(normal.Y, 0f, 1f);

            var lowland = new Vector3(0.115f, 0.105f, 0.060f);
            var meadow = new Vector3(0.155f, 0.285f, 0.105f);
            var rocky = new Vector3(0.265f, 0.255f, 0.225f);

            var color = Vector3.Lerp(lowland, meadow, normalizedHeight);
            color = Vector3.Lerp(color, rocky, MathF.Pow(slope, 1.65f) * 0.72f);

            vertices[xi * zs.Length + zi] =
                new TerrainVertex(new Vector3(px, height, pz), color, normal);
        }

        indices = new uint[checked((xs.Length - 1) * (zs.Length - 1) * 6)];
        var index = 0;

        for (var xi = 0; xi < xs.Length - 1; xi++)
        for (var zi = 0; zi < zs.Length - 1; zi++)
        {
            var a = (uint)(xi * zs.Length + zi);
            var b = (uint)((xi + 1) * zs.Length + zi);
            var c = (uint)((xi + 1) * zs.Length + zi + 1);
            var d = (uint)(xi * zs.Length + zi + 1);

            indices[index++] = a;
            indices[index++] = b;
            indices[index++] = c;
            indices[index++] = a;
            indices[index++] = c;
            indices[index++] = d;
        }
    }

    private static int[] BuildSampleIndices(int size, int gridStep)
    {
        var samples = new List<int>();
        for (var value = 0; value < size - 1; value += gridStep)
            samples.Add(value);

        if (samples.Count == 0 || samples[^1] != size - 1)
            samples.Add(size - 1);

        return samples.ToArray();
    }

    private static Vector3 CalculateNormal(Terrain terrain, int x, int z)
    {
        var left = terrain.GetHeight(Math.Max(0, x - 1), z);
        var right = terrain.GetHeight(Math.Min(terrain.Width - 1, x + 1), z);
        var back = terrain.GetHeight(x, Math.Max(0, z - 1));
        var front = terrain.GetHeight(x, Math.Min(terrain.Depth - 1, z + 1));

        var span = terrain.CellSize * 2f;
        var tangentX = new Vector3(span, right - left, 0f);
        var tangentZ = new Vector3(0f, front - back, span);
        var normal = Vector3.Cross(tangentZ, tangentX);

        return normal.LengthSquared() > 0.000001f
            ? Vector3.Normalize(normal)
            : Vector3.UnitY;
    }
}
