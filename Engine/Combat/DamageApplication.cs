namespace SlavicGame.Engine.Combat;

/// <summary>
/// Minimal combat-facing contract for anything that can receive damage.
/// Keeping damage type in the request preserves the information needed by
/// later armour, resistance and monster-specific reaction systems.
/// </summary>
public interface IDamageReceiver
{
    bool IsAlive { get; }
    void ApplyDamage(float amount, DamageType damageType);
}

public readonly record struct DamageResult(
    string TargetId,
    float Damage,
    DamageType DamageType,
    bool Killed);

public static class DamageApplication
{
    public static DamageResult ApplyMeleeHit(
        AttackDefinition attack,
        string targetId,
        IDamageReceiver target)
    {
        ArgumentNullException.ThrowIfNull(attack);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        ArgumentNullException.ThrowIfNull(target);
        attack.Validate();

        if (!target.IsAlive)
            return new DamageResult(targetId, 0f, attack.DamageType, Killed: true);

        target.ApplyDamage(attack.Damage, attack.DamageType);
        return new DamageResult(targetId, attack.Damage, attack.DamageType, !target.IsAlive);
    }
}
