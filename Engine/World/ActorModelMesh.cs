using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Assets;

namespace SlavicGame.Engine.World;

public static class ActorModelMesh
{
    public static void Build(
        WorldState world,
        GlbModel playerModel,
        GlbModel enemyModel,
        double animationSeconds,
        float playerYaw,
        out TerrainVertex[] vertices,
        out uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(playerModel);
        ArgumentNullException.ThrowIfNull(enemyModel);

        var vertexList = new List<TerrainVertex>();
        var indexList = new List<uint>();
        var time = (float)Math.Max(0d, animationSeconds);

        if (world.Player.IsAlive)
        {
            var playerTransform =
                Matrix4x4.CreateRotationY(playerYaw) *
                Matrix4x4.CreateTranslation(world.PlayerPosition);
            Append(
                playerModel.BuildMesh(playerTransform, "Idle", time, sourceIsZUp: true),
                new Vector3(0.24f, 0.36f, 0.18f),
                vertexList,
                indexList);
        }

        foreach (var enemy in world.Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            var clip = enemy.State switch
            {
                EnemyState.Chase => "Run",
                EnemyState.Attack => "Attack",
                EnemyState.Return => "Walk",
                EnemyState.Patrol => "Walk",
                EnemyState.Alert => "Idle",
                _ => "Idle"
            };
            var transform = Matrix4x4.CreateTranslation(enemy.Position);
            Append(
                enemyModel.BuildMesh(transform, clip, time, sourceIsZUp: true),
                enemy.State == EnemyState.Attack
                    ? new Vector3(0.62f, 0.12f, 0.08f)
                    : new Vector3(0.30f, 0.20f, 0.13f),
                vertexList,
                indexList);
        }

        vertices = vertexList.ToArray();
        indices = indexList.ToArray();
    }

    private static void Append(
        MeshGeometry mesh,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var start = checked((uint)vertices.Count);
        foreach (var position in mesh.Positions)
            vertices.Add(new TerrainVertex(position, color));
        foreach (var index in mesh.Indices)
            indices.Add(start + index);
    }
}
