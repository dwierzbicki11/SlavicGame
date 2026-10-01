namespace SlavicGame.Engine.Gameplay;

public sealed class PlayerVitals
{
    private const float SprintDrainPerSecond = 24f;
    private const float StaminaRecoveryPerSecond = 18f;
    private const double StaminaRecoveryDelaySeconds = 0.75;

    private double _recoveryDelayRemaining;

    public float MaxHealth { get; } = 100f;
    public float MaxStamina { get; } = 100f;
    public float Health { get; private set; } = 100f;
    public float Stamina { get; private set; } = 100f;

    public bool IsAlive => Health > 0f;
    public bool CanSprint => IsAlive && Stamina > 0.1f;

    public void UpdateStamina(bool sprinting, double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
        {
            return;
        }

        if (sprinting && CanSprint)
        {
            Stamina = MathF.Max(0f, Stamina - SprintDrainPerSecond * (float)deltaSeconds);
            _recoveryDelayRemaining = StaminaRecoveryDelaySeconds;
            return;
        }

        var recoverySeconds = deltaSeconds;
        if (_recoveryDelayRemaining > 0.0)
        {
            var waitingSeconds = Math.Min(_recoveryDelayRemaining, recoverySeconds);
            _recoveryDelayRemaining -= waitingSeconds;
            recoverySeconds -= waitingSeconds;
        }

        if (recoverySeconds > 0.0)
        {
            Stamina = MathF.Min(
                MaxStamina,
                Stamina + StaminaRecoveryPerSecond * (float)recoverySeconds);
        }
    }

    public void TakeDamage(float amount)
    {
        ValidateAmount(amount);
        Health = MathF.Max(0f, Health - amount);
    }

    public void Heal(float amount)
    {
        ValidateAmount(amount);
        Health = MathF.Min(MaxHealth, Health + amount);
    }

    public void Restore()
    {
        Health = MaxHealth;
        Stamina = MaxStamina;
        _recoveryDelayRemaining = 0;
    }

    private static void ValidateAmount(float amount)
    {
        if (!float.IsFinite(amount) || amount < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }
    }
}
