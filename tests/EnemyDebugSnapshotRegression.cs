using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;

internal static class EnemyDebugSnapshotRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var enemy = new EnemyAgent("debug-enemy", new Vector3(3f, 0f, -2f));
        var initial = EnemyDebugSnapshot.Capture(enemy);

        if (initial.Id != enemy.Id || initial.State != EnemyState.Patrol ||
            initial.Health != enemy.Health || initial.MaxHealth != enemy.MaxHealth ||
            initial.AttackPhase != EnemyAttackPhase.Ready)
            throw new InvalidOperationException("Enemy debug snapshot did not preserve initial combat state.");

        enemy.TakeDamage(7f);
        var hit = EnemyDebugSnapshot.Capture(enemy);
        if (hit.State != EnemyState.Alert || !hit.IsHitReacting || hit.Health != 53f)
            throw new InvalidOperationException("Enemy debug snapshot did not expose hit reaction state.");

        var line = hit.ToDebugLine();
        if (!line.Contains("debug-enemy", StringComparison.Ordinal) ||
            !line.Contains("state=Alert", StringComparison.Ordinal) ||
            !line.Contains("hp=53/60", StringComparison.Ordinal) ||
            !line.Contains("hitReact=True", StringComparison.Ordinal))
            throw new InvalidOperationException("Enemy debug line omitted combat-critical state.");
    }
}
