using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.World;

internal static class EnemySearchLastKnownPositionRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var world = new WorldState();
        world.Initialize();

        var home = new Vector3(-10f, 0f, -95f);
        home.Y = world.Terrain.SampleHeight(home);
        var enemy = new EnemyAgent("search-last-known", home);

        var visible = new Vector3(-4f, 0f, -92f);
        visible.Y = world.Terrain.SampleHeight(visible);
        world.SetPlayerPosition(visible);
        enemy.Update(world, 0.1);
        enemy.Update(world, 0.6);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Enemy did not enter chase before search regression.");

        // Close most of the distance while the target is still visible so the
        // remembered point can be reached before chase memory expires.
        enemy.Update(world, 1.45);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Enemy left chase before line of sight was broken.");

        var hidden = new Vector3(-10f, 0f, -82f);
        hidden.Y = world.Terrain.SampleHeight(hidden);
        world.SetPlayerPosition(hidden);

        for (var i = 0; i < 8 && enemy.State == EnemyState.Chase; i++)
            enemy.Update(world, 0.2);

        if (enemy.State != EnemyState.Search)
            throw new InvalidOperationException("Enemy did not search after reaching the last known target position.");

        var searchPosition = enemy.Position;
        enemy.Update(world, 0.5);
        if (enemy.State != EnemyState.Search)
            throw new InvalidOperationException("Enemy abandoned search immediately instead of observing the search window.");
        if (Vector3.Distance(searchPosition, enemy.Position) > 0.01f)
            throw new InvalidOperationException("Enemy wandered away from the last known position while searching.");

        enemy.Update(world, 0.8);
        if (enemy.State != EnemyState.Return)
            throw new InvalidOperationException("Enemy did not return after the search window expired.");
    }
}
