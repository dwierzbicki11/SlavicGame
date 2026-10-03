using System.Runtime.CompilerServices;
using SlavicGame.Engine.Combat;

internal static class DamageApplicationRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var attack = new AttackDefinition(
            "typed-damage-regression",
            Damage: 12f,
            StaminaCost: 5f,
            Range: 1.8f,
            WindupSeconds: 0.1,
            RecoverySeconds: 0.2,
            DamageType.Spirit);

        var target = new RegressionTarget(20f);
        var first = DamageApplication.ApplyMeleeHit(attack, "target", target);
        if (first.Damage != 12f || first.DamageType != DamageType.Spirit || first.Killed || target.Health != 8f)
            throw new InvalidOperationException("Typed melee damage was not applied exactly once.");
        if (target.LastDamageType != DamageType.Spirit)
            throw new InvalidOperationException("Damage type was lost before reaching the target.");

        var lethal = DamageApplication.ApplyMeleeHit(attack, "target", target);
        if (!lethal.Killed || target.Health != 0f || target.IsAlive)
            throw new InvalidOperationException("Lethal melee damage did not produce death state.");

        var afterDeath = DamageApplication.ApplyMeleeHit(attack, "target", target);
        if (afterDeath.Damage != 0f || target.ApplyCount != 2)
            throw new InvalidOperationException("Dead target received additional melee damage.");
    }

    private sealed class RegressionTarget(float health) : IDamageReceiver
    {
        public float Health { get; private set; } = health;
        public DamageType? LastDamageType { get; private set; }
        public int ApplyCount { get; private set; }
        public bool IsAlive => Health > 0f;

        public void ApplyDamage(float amount, DamageType damageType)
        {
            ApplyCount++;
            LastDamageType = damageType;
            Health = MathF.Max(0f, Health - amount);
        }
    }
}
