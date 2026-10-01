using System.Numerics;
using SlavicGame.Engine.AI;

namespace SlavicGame.Engine.World;

public static class ActorMesh
{
    public static void Build(
        IReadOnlyList<EnemyAgent> enemies,
        out TerrainVertex[] vertices,
        out uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(enemies);

        var aliveCount = enemies.Count(enemy => enemy.IsAlive);
        var vertexList = new List<TerrainVertex>(aliveCount * 8);
        var indexList = new List<uint>(aliveCount * 36);

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            AddEnemyBox(enemy, vertexList, indexList);
        }

        vertices = vertexList.ToArray();
        indices = indexList.ToArray();
    }

    private static void AddEnemyBox(
        EnemyAgent enemy,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var start = (uint)vertices.Count;
        var radius = enemy.Radius;
        var x0 = enemy.Position.X - radius;
        var x1 = enemy.Position.X + radius;
        var z0 = enemy.Position.Z - radius;
        var z1 = enemy.Position.Z + radius;
        var y0 = enemy.Position.Y;
        var y1 = y0 + enemy.Height;
        var color = GetStateColor(enemy.State);

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

    private static Vector3 GetStateColor(EnemyState state) => state switch
    {
        EnemyState.Patrol => new Vector3(0.42f, 0.28f, 0.16f),
        EnemyState.Alert => new Vector3(0.64f, 0.42f, 0.10f),
        EnemyState.Chase => new Vector3(0.62f, 0.17f, 0.12f),
        EnemyState.Attack => new Vector3(0.78f, 0.08f, 0.06f),
        EnemyState.Return => new Vector3(0.34f, 0.32f, 0.28f),
        _ => new Vector3(0.18f, 0.18f, 0.18f)
    };
}
