namespace SlavicGame.Engine.AI;

public enum EnemyAttackPhase { Ready, Windup, Recovery }

/// <summary>One impact per committed swing; leftover frame time carries through recovery.</summary>
public sealed class EnemyAttackController
{
    public const double WindupSeconds = 0.4;
    public const double RecoverySeconds = 0.8;
    private const double CycleSeconds = WindupSeconds + RecoverySeconds;
    // The predator's authored Attack clip reaches contact at 0.35 / 0.8 seconds.
    private const float ContactClipFraction = 0.4375f;
    private double _elapsed = CycleSeconds;
    private bool _impactDelivered = true;

    public EnemyAttackPhase Phase => _elapsed >= CycleSeconds
        ? EnemyAttackPhase.Ready
        : _elapsed < WindupSeconds ? EnemyAttackPhase.Windup : EnemyAttackPhase.Recovery;
    public float AnimationProgress => Phase switch
    {
        EnemyAttackPhase.Windup => (float)(_elapsed / WindupSeconds) * ContactClipFraction,
        EnemyAttackPhase.Recovery => ContactClipFraction + (float)((_elapsed - WindupSeconds) / RecoverySeconds) * (1f - ContactClipFraction),
        _ => 0f
    };

    public bool TryStart()
    {
        if (Phase != EnemyAttackPhase.Ready) return false;
        _elapsed = 0d;
        _impactDelivered = false;
        return true;
    }

    public double Advance(double deltaSeconds, out bool impact)
    {
        impact = false;
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d) return 0d;
        var consumed = Math.Min(deltaSeconds, CycleSeconds - _elapsed);
        _elapsed = Math.Min(CycleSeconds, _elapsed + consumed);
        if (!_impactDelivered && _elapsed >= WindupSeconds)
        {
            _impactDelivered = true;
            impact = true;
        }
        return Math.Max(0d, deltaSeconds - consumed);
    }

    public void Interrupt()
    {
        // No pending impact survives interruption or restore; preserve a recovery delay.
        _elapsed = WindupSeconds;
        _impactDelivered = true;
    }

    public void Reset()
    {
        _elapsed = CycleSeconds;
        _impactDelivered = true;
    }
}
