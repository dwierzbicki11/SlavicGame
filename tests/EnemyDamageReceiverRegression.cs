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
            DamageType.Slash);

        var first = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (first.Damage != 24f || first.DamageType != DamageType.Slash || first.Killed)
            throw new InvalidOperationException("Enemy did not receive typed melee damage through IDamageReceiver.");
        if (enemy.Health != enemy.MaxHealth - 24f || enemy.State == EnemyState.Dead)
            throw new InvalidOperationException("Enemy health/state did not reflect non-lethal melee damage.");

        DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        var lethal = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (!lethal.Killed || enemy.IsAlive || enemy.Health != 0f || enemy.State != EnemyState.Dead)
            throw new InvalidOperationException("Lethal melee damage did not transition EnemyAgent to Dead.");

        var afterDeath = DamageApplication.ApplyMeleeHit(attack, enemy.Id, receiver);
        if (afterDeath.Damage != 0f || enemy.Health != 0f || enemy.State != EnemyState.Dead)
            throw new InvalidOperationException("Dead EnemyAgent accepted additional melee damage.");
    }
}
