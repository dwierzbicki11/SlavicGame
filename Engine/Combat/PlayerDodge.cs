using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Combat;

public sealed class PlayerDodge
{
    public const float StaminaCost = 20f;
    public const float Distance = 3f;
    public const double DurationSeconds = 0.25;
    public const double RecoverySeconds = 0.55;
    private const double CycleSeconds = DurationSeconds + RecoverySeconds;
    // Small position steps keep the existing obstacle resolver from crossing thin walls.
    private const double MaxMovementStep = 1.0 / 120.0;
    private const double Epsilon = 0.000000001;
    private double _elapsed = CycleSeconds;
    private Vector3 _direction;

    public bool IsActive => _elapsed < DurationSeconds - Epsilon;
    public double CooldownRemaining => Math.Max(0d, CycleSeconds - _elapsed);

    public static bool MovementBlocked(WorldState world) =>
        !world.Player.IsAlive || world.Magic.IsCasting || world.Rituals.IsPerforming ||
        world.Cinematics.IsPlaying || world.Dialogue.IsOpen || world.Vendors.IsOpen ||
        world.Crafting.IsOpen || world.Loot.IsOpen;

    public bool TryStart(WorldState world, Vector3 direction)
    {
        ArgumentNullException.ThrowIfNull(world);
        direction.Y = 0f;
        var lengthSquared = direction.LengthSquared();
        if (CooldownRemaining > Epsilon || MovementBlocked(world) ||
            world.Melee.Controller.State != MeleeAttackState.Free ||
            !WaterInteractionState.CanSprintAtDepth(WaterInteractionState.DepthAt(world, world.PlayerPosition)) ||
            !float.IsFinite(direction.X) || !float.IsFinite(direction.Z) ||
            !float.IsFinite(lengthSquared) || lengthSquared < 0.001f ||
            !world.Player.TrySpendStamina(StaminaCost))
            return false;

        _direction = Vector3.Normalize(direction);
        _elapsed = 0d;
        world.Bow.SetAiming(world, false);
        return true;
    }

    /// <returns>The part of this frame occupied by dodge movement, excluding recovery.</returns>
    public double Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d) return 0d;
        if (!world.Player.IsAlive) { Reset(); return 0d; }
        if (MovementBlocked(world)) Cancel();

        var movementSeconds = 0d;
        while (IsActive && movementSeconds < deltaSeconds - Epsilon)
        {
            var depth = WaterInteractionState.DepthAt(world, world.PlayerPosition);
            if (!WaterInteractionState.CanSprintAtDepth(depth)) { Cancel(); break; }
            var step = Math.Min(MaxMovementStep,
                Math.Min(deltaSeconds - movementSeconds, DurationSeconds - _elapsed));
            var speed = Distance / (float)DurationSeconds *
                WaterInteractionState.MovementSpeedMultiplierForDepth(depth);
            world.SetPlayerPosition(world.PlayerPosition + _direction * speed * (float)step);
            _elapsed = Math.Min(DurationSeconds, _elapsed + step);
            movementSeconds += step;
        }

        _elapsed = Math.Min(CycleSeconds, _elapsed + deltaSeconds - movementSeconds);
        return movementSeconds;
    }

    public void Cancel()
    {
        if (IsActive) _elapsed = DurationSeconds;
    }

    public void Reset()
    {
        _elapsed = CycleSeconds;
        _direction = Vector3.Zero;
    }

    public string HudText(WorldState world)
    {
        if (IsActive) return "UNIK";
        if (CooldownRemaining > Epsilon) return $"UNIK / ODNOWIENIE {CooldownRemaining:0.0} S";
        if (MovementBlocked(world) || world.Melee.Controller.State != MeleeAttackState.Free)
            return "UNIK TERAZ NIEDOSTEPNY";
        if (!WaterInteractionState.CanSprintAtDepth(WaterInteractionState.DepthAt(world, world.PlayerPosition)))
            return "UNIK NIEDOSTEPNY W GLEBOKIEJ WODZIE";
        return world.Player.Stamina < StaminaCost
            ? "SPACJA UNIK / ZA MALO STAMINY (20)"
            : "SPACJA + WASD UNIK / KOSZT 20 STAMINY";
    }
}
