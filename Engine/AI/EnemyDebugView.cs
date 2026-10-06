using System.Globalization;

namespace SlavicGame.Engine.AI;

/// <summary>
/// Stable diagnostic view of combat AI state for HUDs, logs and regression failures.
/// Diagnostics stay separate from behavior decisions so observing an enemy cannot change simulation state.
/// </summary>
public static class EnemyDebugView
{
    public static string Describe(EnemyAgent enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);

        return string.Create(CultureInfo.InvariantCulture,
            $"{enemy.Id} state={enemy.State} hp={enemy.Health:0.0}/{enemy.MaxHealth:0.0} " +
            $"attack={enemy.Attack.Phase} hitReact={enemy.IsHitReacting} " +
            $"pos=({enemy.Position.X:0.00},{enemy.Position.Y:0.00},{enemy.Position.Z:0.00}) " +
            $"facing=({enemy.FacingDirection.X:0.00},{enemy.FacingDirection.Z:0.00})");
    }
}
