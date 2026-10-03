using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Assets;

namespace SlavicGame.Engine.World;

public static class ActorModelMesh
{
    public static void Build(
        WorldState world,
        GlbModel playerModel,
        IReadOnlyDictionary<string, GlbModel> npcModels,
        GlbModel enemyModel,
        double animationSeconds,
        float playerYaw,
        bool includePlayer,
        out TerrainVertex[] vertices,
        out uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(playerModel);
        ArgumentNullException.ThrowIfNull(npcModels);
        ArgumentNullException.ThrowIfNull(enemyModel);

        var vertexList = new List<TerrainVertex>();
        var indexList = new List<uint>();
        var time = (float)Math.Max(0d, animationSeconds);

        if (includePlayer && world.Player.IsAlive)
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

        foreach (var npc in world.NpcWorld.Actors)
        {
            var profile = NpcVisualCatalog.For(npc.Id, npc.Role);
            var modelFile = NpcVisualCatalog.ModelFile(npc.Id, npc.Role);
            if (!npcModels.TryGetValue(modelFile, out var npcModel))
            {
                throw new KeyNotFoundException(
                    $"NPC model '{modelFile}' for '{npc.Id}' was not loaded.");
            }

            var npcTransform =
                Matrix4x4.CreateScale(profile.BodyScale) *
                Matrix4x4.CreateRotationY(npc.YawRadians) *
                Matrix4x4.CreateTranslation(npc.Position);
            var clip = npc.IsMoving
                ? "Walk"
                : NpcVisualCatalog.AnimationClip(npc.Activity);
            if (!npcModel.AnimationNames.Contains(clip))
                clip = npc.IsMoving ? "Walk" : "Idle";

            var animationTime =
                (time + StableAnimationOffset(npc.Id)) *
                profile.AnimationSpeed;

            Append(
                npcModel.BuildMesh(
                    npcTransform,
                    clip,
                    animationTime,
                    sourceIsZUp: true),
                profile.BaseColor,
                vertexList,
                indexList);

            AppendAccessory(
                profile.PrimaryAccessory,
                npcTransform,
                profile.AccentColor,
                vertexList,
                indexList);

            if (profile.SecondaryAccessory != NpcAccessoryKind.None)
            {
                AppendAccessory(
                    profile.SecondaryAccessory,
                    npcTransform,
                    Vector3.Lerp(
                        profile.AccentColor,
                        profile.BaseColor,
                        0.24f),
                    vertexList,
                    indexList);
            }
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

    private static void AppendAccessory(
        NpcAccessoryKind accessory,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        switch (accessory)
        {
            case NpcAccessoryKind.None:
                return;

            case NpcAccessoryKind.Shawl:
                AddBox(
                    new Vector3(0f, 1.43f, 0.015f),
                    new Vector3(0.39f, 0.10f, 0.24f),
                    transform,
                    color,
                    vertices,
                    indices);
                return;

            case NpcAccessoryKind.Satchel:
                AddBox(
                    new Vector3(0.33f, 0.92f, 0.06f),
                    new Vector3(0.18f, 0.25f, 0.10f),
                    transform,
                    color,
                    vertices,
                    indices);
                AddRod(
                    new Vector3(-0.22f, 1.40f, 0.03f),
                    new Vector3(0.30f, 0.92f, 0.05f),
                    0.022f,
                    transform,
                    color,
                    vertices,
                    indices);
                return;

            case NpcAccessoryKind.Staff:
                AddRod(
                    new Vector3(0.42f, 0.05f, 0.10f),
                    new Vector3(0.42f, 1.82f, 0.10f),
                    0.035f,
                    transform,
                    color,
                    vertices,
                    indices);
                return;

            case NpcAccessoryKind.Spear:
                AddRod(
                    new Vector3(0.46f, 0.02f, 0.04f),
                    new Vector3(0.46f, 2.05f, 0.04f),
                    0.030f,
                    transform,
                    color,
                    vertices,
                    indices);
                AddDiamond(
                    new Vector3(0.46f, 2.12f, 0.04f),
                    0.09f,
                    transform,
                    new Vector3(0.52f, 0.54f, 0.50f),
                    vertices,
                    indices);
                return;

            case NpcAccessoryKind.Hood:
                AddBox(
                    new Vector3(0f, 1.70f, 0.02f),
                    new Vector3(0.27f, 0.24f, 0.25f),
                    transform,
                    color,
                    vertices,
                    indices);
                return;

            case NpcAccessoryKind.ToolBundle:
                AddRod(
                    new Vector3(-0.37f, 0.70f, 0.12f),
                    new Vector3(-0.28f, 1.45f, 0.10f),
                    0.028f,
                    transform,
                    color,
                    vertices,
                    indices);
                AddRod(
                    new Vector3(-0.28f, 0.72f, 0.10f),
                    new Vector3(-0.18f, 1.37f, 0.08f),
                    0.024f,
                    transform,
                    Vector3.Lerp(color, Vector3.One, 0.16f),
                    vertices,
                    indices);
                return;

            case NpcAccessoryKind.Basket:
                AddBox(
                    new Vector3(0.40f, 0.72f, 0.05f),
                    new Vector3(0.23f, 0.22f, 0.18f),
                    transform,
                    color,
                    vertices,
                    indices);
                return;

            case NpcAccessoryKind.BeltPouch:
                AddBox(
                    new Vector3(-0.31f, 0.88f, 0.08f),
                    new Vector3(0.13f, 0.16f, 0.09f),
                    transform,
                    color,
                    vertices,
                    indices);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(accessory));
        }
    }

    private static void AddBox(
        Vector3 center,
        Vector3 halfExtent,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        Span<Vector3> local =
        [
            center + new Vector3(-halfExtent.X, -halfExtent.Y, -halfExtent.Z),
            center + new Vector3( halfExtent.X, -halfExtent.Y, -halfExtent.Z),
            center + new Vector3( halfExtent.X, -halfExtent.Y,  halfExtent.Z),
            center + new Vector3(-halfExtent.X, -halfExtent.Y,  halfExtent.Z),
            center + new Vector3(-halfExtent.X,  halfExtent.Y, -halfExtent.Z),
            center + new Vector3( halfExtent.X,  halfExtent.Y, -halfExtent.Z),
            center + new Vector3( halfExtent.X,  halfExtent.Y,  halfExtent.Z),
            center + new Vector3(-halfExtent.X,  halfExtent.Y,  halfExtent.Z)
        ];

        var start = checked((uint)vertices.Count);
        foreach (var point in local)
        {
            vertices.Add(new TerrainVertex(
                Vector3.Transform(point, transform),
                color,
                Vector3.UnitY));
        }

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

    private static void AddRod(
        Vector3 from,
        Vector3 to,
        float radius,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var direction = to - from;
        if (direction.LengthSquared() < 0.000001f)
            return;

        var center = (from + to) * 0.5f;
        var length = direction.Length();
        var localUp = Vector3.Normalize(direction);
        var reference =
            MathF.Abs(Vector3.Dot(localUp, Vector3.UnitY)) > 0.95f
                ? Vector3.UnitX
                : Vector3.UnitY;
        var localRight = Vector3.Normalize(Vector3.Cross(reference, localUp));
        var localForward = Vector3.Normalize(Vector3.Cross(localUp, localRight));

        var rodTransform =
            new Matrix4x4(
                localRight.X, localRight.Y, localRight.Z, 0f,
                localUp.X, localUp.Y, localUp.Z, 0f,
                localForward.X, localForward.Y, localForward.Z, 0f,
                center.X, center.Y, center.Z, 1f) *
            transform;

        AddBox(
            Vector3.Zero,
            new Vector3(radius, length * 0.5f, radius),
            rodTransform,
            color,
            vertices,
            indices);
    }

    private static void AddDiamond(
        Vector3 center,
        float radius,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var start = checked((uint)vertices.Count);
        var localOffsets = new[]
        {
            Vector3.UnitY,
            -Vector3.UnitY,
            Vector3.UnitX,
            Vector3.UnitZ,
            -Vector3.UnitX,
            -Vector3.UnitZ
        };

        foreach (var offset in localOffsets)
        {
            vertices.Add(new TerrainVertex(
                Vector3.Transform(center + offset * radius, transform),
                color,
                offset));
        }

        for (uint i = 0; i < 4; i++)
        {
            var a = start + 2 + i;
            var b = start + 2 + (i + 1) % 4;
            indices.Add(start);
            indices.Add(a);
            indices.Add(b);
            indices.Add(start + 1);
            indices.Add(b);
            indices.Add(a);
        }
    }

    private static float StableAnimationOffset(string id)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in id)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            return (hash % 1000u) / 1000f * 2.5f;
        }
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
