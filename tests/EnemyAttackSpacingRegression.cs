using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.World;

internal static class EnemyAttackSpacingRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var world = new WorldState();
        world.Initialize();
        world.SetPlayerPosition(new Vector3(1.2f, 0f, 0f));

        var enemy = new EnemyAgent("attack-spacing", Vector3.Zero);

        // Detection must still respect the alert phase before committing to a chase.
        enemy.Update(world, 0.1);
        if (enemy.State != EnemyState.Alert)
            throw new InvalidOperationException("Enemy did not enter Alert after detecting the player.");

        enemy.Update(world, 0.6);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Enemy did not commit from Alert to Chase.");

        // Chase enters Attack only when the player is actually inside AttackRange.
        enemy.Update(world, 0.01);
        if (enemy.State != EnemyState.Attack)
            throw new InvalidOperationException("Enemy did not enter Attack inside melee range.");

        var healthBefore = world.Player.Health;
        enemy.Update(world, 0.01);
        if (world.Player.Health >= healthBefore)
            throw new InvalidOperationException("Enemy in Attack did not damage the player.");

        var healthAfterFirstHit = world.Player.Health;
        enemy.Update(world, 0.5);
        if (world.Player.Health != healthAfterFirstHit)
            throw new InvalidOperationException("Enemy ignored its attack cooldown.");

        // The exit threshold is intentionally wider than AttackRange to prevent state
        // thrashing while the player hovers around the melee boundary.
        world.SetPlayerPosition(enemy.Position + new Vector3(1.7f, 0f, 0f));
        enemy.Update(world, 0.1);
        if (enemy.State != EnemyState.Attack)
            throw new InvalidOperationException("Enemy left Attack inside the attack hysteresis band.");

        world.SetPlayerPosition(enemy.Position + new Vector3(2.1f, 0f, 0f));
        enemy.Update(world, 0.1);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Enemy did not return to Chase after the player left melee spacing.");

        var beforeChase = enemy.Position;
        enemy.Update(world, 0.25);
        if (enemy.State != EnemyState.Chase || Vector3.Distance(enemy.Position, world.PlayerPosition) >= Vector3.Distance(beforeChase, world.PlayerPosition))
            throw new InvalidOperationException("Enemy did not resume pursuit after leaving Attack.");
    }
}
