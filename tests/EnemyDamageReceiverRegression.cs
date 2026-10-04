using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.World;

internal static class EnemyDamageReceiverRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var enemy = new EnemyAgent("damage-target", Vector3.Zero);
        IDamageReceiver receiver = enemy;
        var attack = new AttackDefinition(
            "enemy-damage-regression",
            Damage: 24f,
            StaminaCost: 5f,
            Range: 1.8f,
            WindupSeconds: 0.1,
            RecoverySeconds: 0.2,
            DamageType.Physical);

        var first = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (first.Damage != 24f || first.DamageType != DamageType.Physical || first.Killed)
            throw new InvalidOperationException("Enemy did not receive typed melee damage through IDamageReceiver.");
        if (enemy.Health != enemy.MaxHealth - 24f || enemy.State != EnemyState.Alert)
            throw new InvalidOperationException("A living enemy did not enter Alert after non-lethal melee damage.");
        if (!enemy.IsHitReacting)
            throw new InvalidOperationException("A living enemy did not expose a hit reaction after non-lethal damage.");

        enemy.Restore(new EnemySnapshot(enemy.Id, Vector3.Zero, enemy.MaxHealth, EnemyState.Chase));
        DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Damage interrupted an active Chase by resetting the enemy to Alert.");

        var world = new WorldState();
        enemy.Update(world, 0.10);
        if (!enemy.IsHitReacting || enemy.State != EnemyState.Chase || enemy.Position != Vector3.Zero)
            throw new InvalidOperationException("Hit reaction did not briefly suspend active chase behavior.");

        enemy.Update(world, 0.08);
        if (enemy.IsHitReacting || enemy.State != EnemyState.Attack)
            throw new InvalidOperationException("Enemy did not resume its preserved engagement after hit reaction elapsed.");

        enemy.Restore(new EnemySnapshot(enemy.Id, Vector3.Zero, enemy.MaxHealth, EnemyState.Attack));
        DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (enemy.State != EnemyState.Attack || !enemy.IsHitReacting)
            throw new InvalidOperationException("Damage did not preserve Attack while starting a hit reaction.");

        DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        var lethal = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (!lethal.Killed || enemy.IsAlive || enemy.Health != 0f || enemy.State != EnemyState.Dead)
            throw new InvalidOperationException("Lethal melee damage did not transition EnemyAgent to Dead.");
        if (enemy.IsHitReacting)
            throw new InvalidOperationException("Lethal damage left a transient hit reaction active after death.");

        var afterDeath = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (afterDeath.Damage != 0f || enemy.Health != 0f || enemy.State != EnemyState.Dead)
            throw new InvalidOperationException("Dead EnemyAgent accepted additional melee damage.");
    }
}
