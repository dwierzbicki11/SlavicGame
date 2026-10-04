using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Combat;

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

        enemy.Restore(new EnemySnapshot(enemy.Id, Vector3.Zero, enemy.MaxHealth, EnemyState.Chase));
        DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (enemy.State != EnemyState.Chase)
            throw new InvalidOperationException("Damage interrupted an active Chase by resetting the enemy to Alert.");

        enemy.Restore(new EnemySnapshot(enemy.Id, Vector3.Zero, enemy.MaxHealth, EnemyState.Attack));
        DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (enemy.State != EnemyState.Attack)
            throw new InvalidOperationException("Damage interrupted an active Attack by resetting the enemy to Alert.");

        DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        var lethal = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (!lethal.Killed || enemy.IsAlive || enemy.Health != 0f || enemy.State != EnemyState.Dead)
            throw new InvalidOperationException("Lethal melee damage did not transition EnemyAgent to Dead.");

        var afterDeath = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (afterDeath.Damage != 0f || enemy.Health != 0f || enemy.State != EnemyState.Dead)
            throw new InvalidOperationException("Dead EnemyAgent accepted additional melee damage.");
    }
}
