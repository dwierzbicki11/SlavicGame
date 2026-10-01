using System.Numerics;

namespace SlavicGame.Engine.World;

public static class StaticWorldMesh
{
    public static void Build(WorldState world, out TerrainVertex[] vertices, out uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        TerrainMesh.Build(world.Terrain, out var terrainVertices, out var terrainIndices);

        var vertexList = new List<TerrainVertex>(
            terrainVertices.Length + world.Obstacles.Count * 8);
        var indexList = new List<uint>(
            terrainIndices.Length + world.Obstacles.Count * 36);

        vertexList.AddRange(terrainVertices);
        indexList.AddRange(terrainIndices);

        foreach (var obstacle in world.Obstacles)
        {
            AddBox(obstacle, vertexList, indexList);
        }

        vertices = vertexList.ToArray();
        indices = indexList.ToArray();
    }

    private static void AddBox(
        WorldObstacle obstacle,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var start = (uint)vertices.Count;
        var x0 = obstacle.Position.X - obstacle.HalfSize.X;
        var x1 = obstacle.Position.X + obstacle.HalfSize.X;
        var z0 = obstacle.Position.Z - obstacle.HalfSize.Y;
        var z1 = obstacle.Position.Z + obstacle.HalfSize.Y;
        var y0 = obstacle.Position.Y;
        var y1 = y0 + obstacle.Height;
        var color = obstacle.Color;

        vertices.Add(new TerrainVertex(new Vector3(x0, y0, z0), color));
        vertices.Add(new TerrainVertex(new Vector3(x1, y0, z0), color));
        vertices.Add(new TerrainVertex(new Vector3(x1, y0, z1), color));
        vertices.Add(new TerrainVertex(new Vector3(x0, y0, z1), color));
        vertices.Add(new TerrainVertex(new Vector3(x0, y1, z0), color));
        vertices.Add(new TerrainVertex(new Vector3(x1, y1, z0), color));
        vertices.Add(new TerrainVertex(new Vector3(x1, y1, z1), color));
        vertices.Add(new TerrainVertex(new Vector3(x0, y1, z1), color));

        AddFace(0, 1, 2, 3);
        AddFace(4, 7, 6, 5);
        AddFace(0, 4, 5, 1);
        AddFace(1, 5, 6, 2);
        AddFace(2, 6, 7, 3);
        AddFace(3, 7, 4, 0);

        void AddFace(uint a, uint b, uint c, uint d)
        {
            indices.Add(start + a);
            indices.Add(start + b);
            indices.Add(start + c);
            indices.Add(start + a);
            indices.Add(start + c);
            indices.Add(start + d);
        }
    }
}
