using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Characters;

public static class PlayerCharacterModel
{
    public static readonly Vector3 DefaultHunterTunicColor = new(0.24f, 0.36f, 0.18f);

    public static void Append(
        WorldState world,
        GlbModel playerModel,
        float playerYaw,
        float timeSeconds,
        bool includePlayer,
        List<TerrainVertex> vertexList,
        List<uint> indexList)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(playerModel);
        ArgumentNullException.ThrowIfNull(vertexList);
        ArgumentNullException.ThrowIfNull(indexList);

        if (!includePlayer)
            return;

        var playerTransform =
            Matrix4x4.CreateRotationY(playerYaw) *
            Matrix4x4.CreateTranslation(world.PlayerPosition);

        var clip = ResolveClip(world, playerModel, timeSeconds);

        var animTime = clip switch
        {
            "Run" when world.PlayerMotionDistance > 0f => world.PlayerMotionDistance * 0.28f,
            "Walk" when world.PlayerMotionDistance > 0f => world.PlayerMotionDistance * 0.38f,
            _ => timeSeconds
        };

        playerModel.AppendTerrainMesh(
            playerTransform,
            DefaultHunterTunicColor,
            vertexList,
            indexList,
            clip,
            animTime,
            sourceIsZUp: true);

        if (world.Player.IsAlive)
        {
            PlayerHunterAccessoryBuilder.AppendHunterEquipment(
                world,
                playerTransform,
                vertexList,
                indexList);
        }
    }

    public static string ResolveClip(
        WorldState world,
        GlbModel playerModel,
        float timeSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(playerModel);

        if (!world.Player.IsAlive)
        {
            if (playerModel.AnimationNames.Contains("Death"))
                return "Death";
            return "Idle";
        }

        if (world.Dodge.IsActive)
        {
            if (playerModel.AnimationNames.Contains("Run"))
                return "Run";
        }

        if (timeSeconds > 0.0001f &&
            world.Melee.Controller.State != SlavicGame.Engine.Combat.MeleeAttackState.Free &&
            playerModel.AnimationNames.Contains("Attack"))
        {
            return "Attack";
        }

        if (world.Bow.IsAiming || world.Bow.IsDrawing)
        {
            if (playerModel.AnimationNames.Contains("Attack"))
                return "Attack";
        }

        if (world.PlayerIsMoving || world.PlayerVelocity.LengthSquared() > 0.04f)
        {
            if (world.PlayerIsSprinting && playerModel.AnimationNames.Contains("Run"))
                return "Run";

            if (playerModel.AnimationNames.Contains("Walk"))
                return "Walk";
        }

        return "Idle";
    }
}
