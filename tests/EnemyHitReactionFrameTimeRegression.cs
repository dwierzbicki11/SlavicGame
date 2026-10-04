using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.World;

internal static class EnemyHitReactionFrameTimeRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var world = new WorldState();
        world.Initialize();
        var home = Vector3.Zero;
        home.Y = world.Terrain.SampleHeight(home);
        world.SetPlayerPosition(new Vector3(10f, world.Terrain.SampleHeight(new Vector3(10f, 0f, 0f)), 0f));

        var enemy = new EnemyAgent("hit-reaction-time", home);
        enemy.Restore(new EnemySnapshot(enemy.Id, home, enemy.MaxHealth, EnemyState.Chase));
        enemy.TakeDamage(1f);

        enemy.Update(world, 0.20);

        if (enemy.IsHitReacting)
            throw new InvalidOperationException("Hit reaction should finish inside a 0.20 s frame.");

        var moved = Vector2.Distance(new Vector2(home.X, home.Z), new Vector2(enemy.Position.X, enemy.Position.Z));
        const float expectedMove = 2.6f * 0.02f;
        if (MathF.Abs(moved - expectedMove) > 0.01f)
            throw new InvalidOperationException($"Hit reaction frame time was double-simulated: moved {moved:0.000} m, expected about {expectedMove:0.000} m from leftover frame time only.");
    }
}
