using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.World;

internal static class EnemyChaseLineOfSightRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var world = new WorldState();
        world.Initialize();

        var home = new Vector3(-10f, 0f, -95f);
        home.Y = world.Terrain.SampleHeight(home);
        var enemy = new EnemyAgent("chase-los", home);

        // Acquire the player from a clear position offset to the enemy's right.
        var visible = new Vector3(-4f, 0f, -92f);
        visible.Y = world.Terrain.SampleHeight(visible);
        world.SetPlayerPosition(visible);
        enemy.Update(world, 0.1);
        if (enemy.State != EnemyState.Alert)
            throw new InvalidOperationException("Enemy did not acquire the visible chase target.");

        enemy.Update(world, 0.6);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Enemy did not enter chase after alert.");

        var beforeOcclusion = enemy.Position;

        // The hut blocks this position from the enemy. A clairvoyant chase would
        // head toward X=-10; the remembered visible position requires movement
        // toward increasing X instead.
        var hidden = new Vector3(-10f, 0f, -82f);
        hidden.Y = world.Terrain.SampleHeight(hidden);
        world.SetPlayerPosition(hidden);
        enemy.Update(world, 0.25);

        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Enemy abandoned chase immediately after losing line of sight.");
        if (enemy.Position.X <= beforeOcclusion.X + 0.01f)
            throw new InvalidOperationException("Enemy used the hidden live target position instead of its last known position.");
    }
}
