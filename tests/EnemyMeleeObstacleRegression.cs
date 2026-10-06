using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.World;

internal static class EnemyMeleeObstacleRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var world = new WorldState();
        world.Initialize();

        // The shrine stone is a real solid WorldObstacle. Keep both actors within melee
        // range while its collider intersects the strike segment so the regression isolates
        // impact occlusion rather than aggro/chase behavior.
        var enemyPosition = new Vector3(-85.6f, 0f, 55f);
        enemyPosition.Y = world.Terrain.SampleHeight(enemyPosition);
        var playerPosition = new Vector3(-84.4f, 0f, 55f);
        playerPosition.Y = world.Terrain.SampleHeight(playerPosition);
        world.SetPlayerPosition(playerPosition);

        var enemy = new EnemyAgent("melee-obstacle", enemyPosition);
        enemy.Restore(new EnemySnapshot(enemy.Id, enemyPosition, enemy.MaxHealth, EnemyState.Attack));
        var healthBefore = world.Player.Health;

        // Restore intentionally leaves an interrupted attack in recovery. Finish recovery,
        // then run a complete fresh windup through its impact frame.
        enemy.Update(world, EnemyAttackController.RecoverySeconds);
        enemy.Update(world, EnemyAttackController.WindupSeconds);

        if (world.Player.Health != healthBefore)
            throw new InvalidOperationException("Enemy melee impact damaged the player through a solid world obstacle.");

        if (enemy.Attack.Phase != EnemyAttackPhase.Recovery)
            throw new InvalidOperationException("Occluded melee swing should still commit and enter recovery without dealing damage.");
    }
}
