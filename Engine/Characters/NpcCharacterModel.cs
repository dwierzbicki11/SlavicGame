using System.Numerics;
using SlavicGame.Engine.Animation;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.NPC;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Characters;

public static class NpcCharacterModel
{
    public const float AmbientNpcRenderDistance = 95f;

    public static void Append(
        WorldState world,
        IReadOnlyDictionary<string, GlbModel> npcModels,
        Vector3 viewerPosition,
        float timeSeconds,
        List<TerrainVertex> vertexList,
        List<uint> indexList)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(npcModels);
        ArgumentNullException.ThrowIfNull(vertexList);
        ArgumentNullException.ThrowIfNull(indexList);

        foreach (var npc in world.NpcWorld.Actors)
        {
            if (npc.Id.StartsWith("settler-", StringComparison.Ordinal) &&
                HorizontalDistanceSquared(npc.Position, viewerPosition) >
                    AmbientNpcRenderDistance * AmbientNpcRenderDistance)
            {
                continue;
            }

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

            var clip = npc.Reaction switch
            {
                NpcReactionKind.FleeThreat or
                NpcReactionKind.AvoidPlayer => "Run",

                NpcReactionKind.GuardThreat => "Walk",

                NpcReactionKind.WatchPlayer => "Idle",

                _ => npc.IsMoving
                    ? "Walk"
                    : NpcVisualCatalog.AnimationClip(npc.Activity)
            };

            if (!npcModel.AnimationNames.Contains(clip))
                clip = npc.IsMoving ? "Walk" : "Idle";

            var animationTime =
                (timeSeconds + StableAnimationOffset(npc.Id)) *
                profile.AnimationSpeed;

            npcModel.AppendTerrainMesh(
                npcTransform,
                profile.BaseColor,
                vertexList,
                indexList,
                clip,
                animationTime,
                sourceIsZUp: true);

            NpcAccessoryMeshBuilder.AppendAccessory(
                profile.PrimaryAccessory,
                npcTransform,
                profile.AccentColor,
                vertexList,
                indexList);

            if (profile.SecondaryAccessory != NpcAccessoryKind.None)
            {
                NpcAccessoryMeshBuilder.AppendAccessory(
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
    }

    private static float HorizontalDistanceSquared(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dz = a.Z - b.Z;
        return dx * dx + dz * dz;
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
}
