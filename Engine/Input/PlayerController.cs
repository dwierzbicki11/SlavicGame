using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Input;

public static class PlayerController
{
    private const double MaxAirMovementStep = 1.0 / 120.0;
    public static void Update(WorldState world, Camera3D camera, PlayerInput input, double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d) return;
        // Look and movement belong to the same frame, even while a movement key is held.
        camera.Rotate(input.LookDelta.X, input.LookDelta.Y);

        if (!world.Player.IsAlive)
        {
            world.Dodge.Reset();
            world.Jump.Update(world, deltaSeconds);
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
        if (input.JumpPressed) world.Jump.TryStart(world);
        if (input.DodgePressed)
            world.Dodge.TryStart(world, isMoving ? move : -camera.GetMoveForward());

        var dodgeSeconds = world.Dodge.Update(world, deltaSeconds);
        var movementSeconds = Math.Max(0d, deltaSeconds - dodgeSeconds);
        var waterDepth = world.Jump.IsAirborne ? 0f : WaterInteractionState.DepthAt(
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

        var jumpSeconds = 0d;
        world.PlayerIsMoving = isMoving;
        world.PlayerIsSprinting = sprinting;
        if (isMoving)
        {
            move = Vector3.Normalize(move);
            var speed = (sprinting ? 9f : 5f) * waterSpeedMultiplier;
            world.PlayerVelocity = move * speed;
            world.PlayerMotionDistance += speed * (float)movementSeconds;
            if (world.Jump.IsAirborne)
            {
                // Advance horizontal travel and gravity together. Comparing the first
                // vertical step against the whole frame's uphill endpoint lands too early.
                while (jumpSeconds < movementSeconds - 0.000000001)
                {
                    var step = Math.Min(MaxAirMovementStep, movementSeconds - jumpSeconds);
                    world.MovePlayerPosition(world.PlayerPosition + move * speed * (float)step);
                    world.Jump.Update(world, step);
                    jumpSeconds += step;
                }
            }
            else
            {
                world.MovePlayerPosition(world.PlayerPosition + move * speed * (float)movementSeconds);
            }
        }
        else
        {
            world.PlayerVelocity = Vector3.Zero;
        }

        if (world.Bow.IsAiming || world.Melee.Controller.State != SlavicGame.Engine.Combat.MeleeAttackState.Free)
        {
            world.PlayerFacingYaw = camera.Yaw;
        }
        else if (isMoving)
        {
            var desiredYaw = MathF.Atan2(move.X, move.Z);
            world.PlayerFacingYaw = TurnTowards(world.PlayerFacingYaw, desiredYaw, (float)deltaSeconds * 14f);
        }

        camera.IsAiming = world.Bow.IsAiming;

        var currentVelocity = world.Jump.IsAirborne ? Vector3.Zero :
            WaterInteractionState.CurrentVelocityAt(
                world,
                world.PlayerPosition);
        if (currentVelocity.LengthSquared() > 0.000001f)
        {
            world.MovePlayerPosition(
                world.PlayerPosition +
                currentVelocity * (float)deltaSeconds);
        }

        world.Jump.Update(world, Math.Max(0d, deltaSeconds - jumpSeconds));

        var finalWaterDepth = world.Jump.IsAirborne ? 0f :
            WaterInteractionState.DepthAt(
                world,
                world.PlayerPosition);

        var recoveryMultiplier = WaterInteractionState.StaminaRecoveryMultiplier(
            finalWaterDepth,
            world.WaterInteraction.Wetness,
            world.WaterInteraction.Chill);
        if (world.Springs.HasBlessedWater)
        {
            recoveryMultiplier *= SlavicSpringSystem.StaminaRecoveryMultiplier;
        }
        world.Player.UpdateStamina(false, dodgeSeconds, recoveryMultiplier: recoveryMultiplier);
        world.Player.UpdateStamina(
            sprinting,
            movementSeconds,
            WaterInteractionState.StaminaDrainMultiplierForDepth(finalWaterDepth),
            recoveryMultiplier);
        camera.Follow(world.PlayerPosition, (float)deltaSeconds, world.Terrain);
    }

    private static float TurnTowards(float current, float target, float maxDelta)
    {
        var diff = MathF.IEEERemainder(target - current, MathF.PI * 2f);
        if (MathF.Abs(diff) <= maxDelta)
            return target;
        return current + MathF.Sign(diff) * maxDelta;
    }
}
