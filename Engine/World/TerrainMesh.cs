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
    public static void Build(Terrain terrain, out TerrainVertex[] vertices, out uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        vertices = new TerrainVertex[checked(terrain.Width * terrain.Depth)];

        for (var x = 0; x < terrain.Width; x++)
        for (var z = 0; z < terrain.Depth; z++)
        {
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

            vertices[x * terrain.Depth + z] =
                new TerrainVertex(new Vector3(px, height, pz), color, normal);
        }

        indices = new uint[checked((terrain.Width - 1) * (terrain.Depth - 1) * 6)];
        var index = 0;

        for (var x = 0; x < terrain.Width - 1; x++)
        for (var z = 0; z < terrain.Depth - 1; z++)
        {
            var a = (uint)(x * terrain.Depth + z);
            var b = (uint)((x + 1) * terrain.Depth + z);
            var c = (uint)((x + 1) * terrain.Depth + z + 1);
            var d = (uint)(x * terrain.Depth + z + 1);

            indices[index++] = a;
            indices[index++] = b;
            indices[index++] = c;
            indices[index++] = a;
            indices[index++] = c;
            indices[index++] = d;
        }
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
