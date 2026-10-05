using System.Numerics;

namespace SlavicGame.Engine.AI;

/// <summary>
/// Stable, allocation-light view of combat-relevant enemy state for debug overlays and telemetry.
/// Keeps diagnostics out of the decision loop while exposing enough state to explain AI behaviour.
/// </summary>
public readonly record struct EnemyDebugSnapshot(
    string Id,
    EnemyState State,
    Vector3 Position,
    Vector3 FacingDirection,
    float Health,
    float MaxHealth,
    bool IsHitReacting,
    bool IsAttackWindingUp,
    EnemyAttackPhase AttackPhase)
{
    public static EnemyDebugSnapshot Capture(EnemyAgent enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        return new EnemyDebugSnapshot(
            enemy.Id,
            enemy.State,
            enemy.Position,
            enemy.FacingDirection,
            enemy.Health,
            enemy.MaxHealth,
            enemy.IsHitReacting,
            enemy.IsAttackWindingUp,
            enemy.Attack.Phase);
    }

    public string ToDebugLine() =>
        $"{Id} state={State} hp={Health:0.#}/{MaxHealth:0.#} attack={AttackPhase} hitReact={IsHitReacting} facing=({FacingDirection.X:0.00},{FacingDirection.Z:0.00}) pos=({Position.X:0.0},{Position.Y:0.0},{Position.Z:0.0})";
}
