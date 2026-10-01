using System.Numerics;

namespace SlavicGame.Engine.World;

public readonly struct TerrainVertex
{
    public const uint SizeInBytes = 24;

    public readonly Vector3 Position;
    public readonly Vector3 Color;

    public TerrainVertex(Vector3 position, Vector3 color)
    {
        Position = position;
        Color = color;
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

            var normalized = Math.Clamp((height + 5f) / 10f, 0f, 1f);
            var color = Vector3.Lerp(
                new Vector3(0.16f, 0.12f, 0.07f),
                new Vector3(0.22f, 0.38f, 0.16f),
                normalized);

            vertices[x * terrain.Depth + z] =
                new TerrainVertex(new Vector3(px, height, pz), color);
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
}
