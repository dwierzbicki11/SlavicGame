using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Input;

public static class PlayerController
{
    public static void Update(WorldState world, Camera3D camera, PlayerInput input, double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d) return;
        // Look and movement belong to the same frame, even while a movement key is held.
        camera.Rotate(input.LookDelta.X, input.LookDelta.Y);

        if (!world.Player.IsAlive)
        {
            world.Dodge.Reset();
            world.Player.UpdateStamina(false, deltaSeconds);
            camera.Follow(world.PlayerPosition, (float)deltaSeconds, world.Terrain);
            return;
        }

        var move = Vector3.Zero;
        if (input.Forward) move += camera.GetMoveForward();
        if (input.Backward) move -= camera.GetMoveForward();
        if (input.Right) move += camera.GetMoveRight();
        if (input.Left) move -= camera.GetMoveRight();
        if (PlayerDodge.MovementBlocked(world)) move = Vector3.Zero;

        var isMoving = move.LengthSquared() > 0.001f;
        if (input.DodgePressed)
            world.Dodge.TryStart(world, isMoving ? move : -camera.GetMoveForward());

        var dodgeSeconds = world.Dodge.Update(world, deltaSeconds);
        var movementSeconds = Math.Max(0d, deltaSeconds - dodgeSeconds);
        var waterDepth = WaterInteractionState.DepthAt(
            world,
            world.PlayerPosition);
        var waterSpeedMultiplier =
            WaterInteractionState.MovementSpeedMultiplierForDepth(
                waterDepth);
        var sprinting =
            isMoving &&
            movementSeconds > 0d &&
            input.Running &&
            world.Player.CanSprint &&
            WaterInteractionState.CanSprintAtDepth(waterDepth);

        if (isMoving)
        {
            move = Vector3.Normalize(move);
            var speed = (sprinting ? 9f : 5f) * waterSpeedMultiplier;
            world.SetPlayerPosition(
                world.PlayerPosition +
                move * speed * (float)movementSeconds);
        }

        var currentVelocity =
            WaterInteractionState.CurrentVelocityAt(
                world,
                world.PlayerPosition);
        if (currentVelocity.LengthSquared() > 0.000001f)
        {
            world.SetPlayerPosition(
                world.PlayerPosition +
                currentVelocity * (float)deltaSeconds);
        }

        var finalWaterDepth =
            WaterInteractionState.DepthAt(
                world,
                world.PlayerPosition);

        var recoveryMultiplier = WaterInteractionState.StaminaRecoveryMultiplier(
            finalWaterDepth,
            world.WaterInteraction.Wetness,
            world.WaterInteraction.Chill);
        world.Player.UpdateStamina(false, dodgeSeconds, recoveryMultiplier: recoveryMultiplier);
        world.Player.UpdateStamina(
            sprinting,
            movementSeconds,
            WaterInteractionState.StaminaDrainMultiplierForDepth(finalWaterDepth),
            recoveryMultiplier);
        camera.Follow(world.PlayerPosition, (float)deltaSeconds, world.Terrain);
    }
}
