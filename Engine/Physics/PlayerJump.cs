using SlavicGame.Engine.Combat;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Physics;

public sealed class PlayerJump
{
    public const float LaunchSpeed = 6.5f;
    public const float Gravity = 16f;
    private const double MaxStep = 1.0 / 120.0;
    private double _height;
    private double _velocity;

    public bool IsAirborne { get; private set; }
    public float VerticalVelocity => (float)_velocity;

    public bool TryStart(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (IsAirborne || PlayerDodge.MovementBlocked(world) || world.Dodge.IsActive ||
            world.Melee.Controller.State != MeleeAttackState.Free ||
            !WaterInteractionState.CanSprintAtDepth(WaterInteractionState.DepthAt(world, world.PlayerPosition)))
            return false;

        IsAirborne = true;
        _height = world.PlayerPosition.Y;
        _velocity = LaunchSpeed;
        return true;
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d) return;
        var remaining = deltaSeconds;
        while (IsAirborne && remaining > 0.000000001)
        {
            var step = Math.Min(MaxStep, remaining);
            // Exact constant-acceleration integration, independent of the frame rate.
            _height += _velocity * step - 0.5 * Gravity * step * step;
            _velocity -= Gravity * step;
            var ground = world.Terrain.SampleHeight(world.PlayerPosition);
            if (_height <= ground)
            {
                world.SetPlayerHeight(ground);
                Reset();
                break;
            }
            world.SetPlayerHeight((float)_height);
            remaining -= step;
        }
    }

    public void Reset()
    {
        IsAirborne = false;
        _height = 0d;
        _velocity = 0d;
    }
}
