using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Combat;

public enum MeleeAttackState
{
    Free,
    Windup,
    ActiveAttack,
    Recovery,
    Dead
}

/// <summary>
/// Deterministic first-pass melee attack state machine. Attack stamina is paid once
/// when the attack starts; damage eligibility exists only during ActiveAttack.
/// </summary>
public sealed class MeleeAttackController
{
    private readonly HashSet<string> _hitTargets = new(StringComparer.Ordinal);
    private AttackDefinition? _attack;
    private double _stateRemaining;

    public MeleeAttackState State { get; private set; } = MeleeAttackState.Free;
    public AttackDefinition? CurrentAttack => _attack;
    public bool CanDealDamage => State == MeleeAttackState.ActiveAttack;

    public bool TryStart(AttackDefinition attack, PlayerVitals vitals)
    {
        ArgumentNullException.ThrowIfNull(attack);
        ArgumentNullException.ThrowIfNull(vitals);
        attack.Validate();

        if (!vitals.IsAlive)
        {
            State = MeleeAttackState.Dead;
            return false;
        }

        if (State != MeleeAttackState.Free || !vitals.TrySpendStamina(attack.StaminaCost))
            return false;

        _attack = attack;
        _hitTargets.Clear();
        State = MeleeAttackState.Windup;
        _stateRemaining = attack.WindupSeconds;
        AdvanceZeroLengthStates();
        return true;
    }

    public void Update(PlayerVitals vitals, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(vitals);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
            return;

        if (!vitals.IsAlive)
        {
            State = MeleeAttackState.Dead;
            _attack = null;
            _hitTargets.Clear();
            _stateRemaining = 0;
            return;
        }

        var remaining = deltaSeconds;
        while (remaining > 0 && State is not (MeleeAttackState.Free or MeleeAttackState.Dead))
        {
            if (_stateRemaining > remaining)
            {
                _stateRemaining -= remaining;
                break;
            }

            remaining -= _stateRemaining;
            Transition();
            AdvanceZeroLengthStates();
        }
    }

    public bool TryRegisterHit(string targetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        return CanDealDamage && _hitTargets.Add(targetId);
    }

    private void Transition()
    {
        switch (State)
        {
            case MeleeAttackState.Windup:
                State = MeleeAttackState.ActiveAttack;
                _stateRemaining = 0.12;
                break;
            case MeleeAttackState.ActiveAttack:
                State = MeleeAttackState.Recovery;
                _stateRemaining = _attack?.RecoverySeconds ?? 0;
                break;
            case MeleeAttackState.Recovery:
                State = MeleeAttackState.Free;
                _stateRemaining = 0;
                _attack = null;
                _hitTargets.Clear();
                break;
        }
    }

    private void AdvanceZeroLengthStates()
    {
        while (_stateRemaining <= 0 && State is not (MeleeAttackState.Free or MeleeAttackState.Dead))
            Transition();
    }
}
