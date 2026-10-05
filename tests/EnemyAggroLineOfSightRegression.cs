using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.World;

internal static class EnemyAggroLineOfSightRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var world = new WorldState();
        world.Initialize();

        var blockedHome = new Vector3(-10f, 0f, -95f);
        blockedHome.Y = world.Terrain.SampleHeight(blockedHome);
        var blockedEnemy = new EnemyAgent("los-blocked", blockedHome);

        var behindHut = new Vector3(-10f, 0f, -82f);
        behindHut.Y = world.Terrain.SampleHeight(behindHut);
        world.SetPlayerPosition(behindHut);
        blockedEnemy.Update(world, 0.1);

        if (blockedEnemy.State != EnemyState.Patrol)
            throw new InvalidOperationException("Enemy detected the player through a solid world obstacle.");

        var clearHome = new Vector3(-24f, 0f, -95f);
        clearHome.Y = world.Terrain.SampleHeight(clearHome);
        var clearEnemy = new EnemyAgent("los-clear", clearHome);
        var clearPlayer = new Vector3(-24f, 0f, -84f);
        clearPlayer.Y = world.Terrain.SampleHeight(clearPlayer);
        world.SetPlayerPosition(clearPlayer);
        clearEnemy.Update(world, 0.1);

        if (clearEnemy.State != EnemyState.Alert)
            throw new InvalidOperationException("Clear line of sight did not trigger normal detection.");

        blockedEnemy.TakeDamage(1f);
        if (blockedEnemy.State != EnemyState.Alert)
            throw new InvalidOperationException("Damage provocation must bypass visual detection occlusion.");
    }
}
