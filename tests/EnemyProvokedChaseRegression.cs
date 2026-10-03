using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.World;

internal static class EnemyProvokedChaseRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var world = new WorldState();
        world.Initialize();
        world.SetPlayerPosition(new Vector3(25f, 0f, 0f));

        var enemy = new EnemyAgent("provoked-chase", Vector3.Zero);
        enemy.ApplyDamage(1f, DamageType.Physical);
        if (enemy.State != EnemyState.Alert)
            throw new InvalidOperationException("Damage did not alert the enemy.");

        enemy.Update(world, 0.6);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("A provoked enemy outside normal detection range did not enter Chase.");

        var before = enemy.Position;
        enemy.Update(world, 0.5);
        if (enemy.State != EnemyState.Chase || Vector3.Distance(enemy.Position, world.PlayerPosition) >= Vector3.Distance(before, world.PlayerPosition))
            throw new InvalidOperationException("A provoked enemy did not pursue the player after entering Chase.");

        // Measure disengage from the enemy's current position, not its spawn point: the
        // preceding chase step has already moved it toward the player.
        world.SetPlayerPosition(enemy.Position + new Vector3(31f, 0f, 0f));
        enemy.Update(world, 0.1);
        if (enemy.State != EnemyState.Return)
            throw new InvalidOperationException("Provoked chase ignored its extended disengage boundary.");
    }
}
