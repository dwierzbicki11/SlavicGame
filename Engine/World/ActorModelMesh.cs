using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Characters;

namespace SlavicGame.Engine.World;

public static class ActorModelMesh
{
    public const float AmbientNpcRenderDistance = NpcCharacterModel.AmbientNpcRenderDistance;

    public static void Build(
        WorldState world,
        GlbModel playerModel,
        IReadOnlyDictionary<string, GlbModel> npcModels,
        GlbModel enemyModel,
        Vector3 viewerPosition,
        double animationSeconds,
        float playerYaw,
        bool includePlayer,
        out TerrainVertex[] vertices,
        out uint[] indices)
    {
        var vertexList = new List<TerrainVertex>();
        var indexList = new List<uint>();
        Build(
            world,
            playerModel,
            npcModels,
            enemyModel,
            viewerPosition,
            animationSeconds,
            playerYaw,
            includePlayer,
            vertexList,
            indexList);
        vertices = vertexList.ToArray();
        indices = indexList.ToArray();
    }

    public static void Build(
        WorldState world,
        GlbModel playerModel,
        IReadOnlyDictionary<string, GlbModel> npcModels,
        GlbModel enemyModel,
        Vector3 viewerPosition,
        double animationSeconds,
        float playerYaw,
        bool includePlayer,
        List<TerrainVertex> vertexList,
        List<uint> indexList)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(playerModel);
        ArgumentNullException.ThrowIfNull(npcModels);
        ArgumentNullException.ThrowIfNull(enemyModel);
        ArgumentNullException.ThrowIfNull(vertexList);
        ArgumentNullException.ThrowIfNull(indexList);

        vertexList.Clear();
        indexList.Clear();
        var time = (float)Math.Max(0d, animationSeconds);

        PlayerCharacterModel.Append(
            world,
            playerModel,
            playerYaw,
            time,
            includePlayer,
            vertexList,
            indexList);

        NpcCharacterModel.Append(
            world,
            npcModels,
            viewerPosition,
            time,
            vertexList,
            indexList);

        EnemyCharacterModel.Append(
            world,
            enemyModel,
            time,
            vertexList,
            indexList);
    }
}
