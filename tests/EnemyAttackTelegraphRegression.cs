using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.World;

internal static class EnemyAttackTelegraphRegression
{
    public static void Run(Action<bool, string> check)
    {
        var (world, enemy) = Fight();
        var before = world.Player.Health;
        enemy.Update(world, 0.2);
        check(enemy.IsAttackWindingUp && world.Player.Health == before,
            "Enemy windup warns before any melee damage");
        enemy.Update(world, 0.19);
        check(world.Player.Health == before, "No damage arrives before the windup contact boundary");
        enemy.Update(world, 0.02);
        check(world.Player.Health == before - 8f && enemy.Attack.Phase == EnemyAttackPhase.Recovery,
            "Committed attack delivers exactly one hit at contact and enters recovery");
        enemy.Update(world, 0.5);
        check(world.Player.Health == before - 8f, "Recovery cannot duplicate attack damage");

        (world, enemy) = Fight(); before = world.Player.Health;
        enemy.Update(world, 0.2);
        world.SetPlayerPosition(enemy.Position + Vector3.UnitX * 2.5f);
        enemy.Update(world, 0.21);
        check(world.Player.Health == before, "Retreating beyond melee reach avoids the committed hit");
        enemy.Update(world, 0.01);
        check(enemy.State == EnemyState.Chase, "Enemy resumes pursuit after a missed swing and leaving attack spacing");

        (world, enemy) = Fight(); before = world.Player.Health;
        enemy.Update(world, 0.2);
        world.SetPlayerPosition(enemy.Position - Vector3.UnitX * 1.2f);
        enemy.Update(world, 0.21);
        check(world.Player.Health == before && enemy.FacingDirection.X > 0.99f,
            "Moving behind the locked strike direction avoids a swing without reducing target distance");

        var entryWorld = new WorldState(); entryWorld.Initialize();
        var entryPosition = new Vector3(0f, entryWorld.Terrain.SampleHeight(Vector3.Zero), 0f);
        var entryEnemy = new EnemyAgent("entry-facing-target", entryPosition);
        entryWorld.SetPlayerPosition(entryPosition + Vector3.UnitZ * 1.2f);
        entryEnemy.Restore(new EnemySnapshot(entryEnemy.Id, entryPosition, entryEnemy.MaxHealth, EnemyState.Chase));
        entryEnemy.Update(entryWorld, 0d);
        check(entryEnemy.State == EnemyState.Attack && entryEnemy.FacingDirection.Z > 0.99f,
            "Entering melee range locks facing immediately for a readable committed strike");

        (world, enemy) = Fight(); before = world.Player.Health;
        enemy.Update(world, 0.2);
        enemy.TakeDamage(1f);
        enemy.Update(world, 0.2);
        enemy.Update(world, 0.1);
        check(world.Player.Health == before && enemy.State == EnemyState.Attack && !enemy.IsAttackWindingUp,
            "Damage interrupts the pending swing while retaining Attack engagement");

        (world, enemy) = Fight(); before = world.Player.Health;
        enemy.Update(world, 0.2);
        enemy.TakeDamage(enemy.MaxHealth);
        enemy.Update(world, 2d);
        check(world.Player.Health == before && enemy.Attack.Phase == EnemyAttackPhase.Ready && !enemy.IsAlive,
            "A killed enemy never delivers its pending strike");

        (world, enemy) = Fight(); before = world.Player.Health;
        enemy.Update(world, 0.2);
        enemy.Restore(enemy.Capture());
        enemy.Update(world, 0.3);
        check(world.Player.Health == before && !enemy.IsAttackWindingUp && enemy.Attack.Phase == EnemyAttackPhase.Recovery,
            "Restore discards a pending strike and requires a fresh windup after recovery");

        check(DamageAfter(0.1) == 16f && DamageAfter(1.0 / 120.0) == 16f,
            "Two complete attack cycles deal equal damage at 10 and 120 FPS");

        float DamageAfter(double step)
        {
            var (timedWorld, timedEnemy) = Fight();
            var health = timedWorld.Player.Health;
            var remaining = 2.4;
            while (remaining > 0.000001)
            {
                var dt = Math.Min(remaining, step);
                timedEnemy.Update(timedWorld, dt);
                remaining -= dt;
            }
            return health - timedWorld.Player.Health;
        }
    }

    private static (WorldState, EnemyAgent) Fight()
    {
        var world = new WorldState(); world.Initialize();
        var position = new Vector3(0f, world.Terrain.SampleHeight(Vector3.Zero), 0f);
        var enemy = new EnemyAgent("telegraph-target", position);
        world.SetPlayerPosition(position + Vector3.UnitX * 1.2f);
        enemy.Restore(new EnemySnapshot(enemy.Id, position, enemy.MaxHealth, EnemyState.Chase));
        enemy.Update(world, 0d);
        return (world, enemy);
    }
}
