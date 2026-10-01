namespace SlavicGame.Engine.Combat;

public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Poison,
    Spirit,
    Divine,
    Ancient
}

public enum AttackDirection
{
    Neutral,
    Left,
    Right,
    Overhead,
    Thrust
}

public sealed record AttackDefinition(
    string Id,
    float Damage,
    float StaminaCost,
    float Range,
    double WindupSeconds,
    double RecoverySeconds,
    DamageType DamageType,
    AttackDirection Direction = AttackDirection.Neutral)
{
    public AttackDefinition Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        if (!float.IsFinite(Damage) || Damage < 0f ||
            !float.IsFinite(StaminaCost) || StaminaCost < 0f ||
            !float.IsFinite(Range) || Range <= 0f ||
            !double.IsFinite(WindupSeconds) || WindupSeconds < 0 ||
            !double.IsFinite(RecoverySeconds) || RecoverySeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Damage));
        }
        return this;
    }
}

public sealed record StatusEffectDefinition(
    string Id,
    double DurationSeconds,
    float Magnitude,
    DamageType? PeriodicDamageType = null);
