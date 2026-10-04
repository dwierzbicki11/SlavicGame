using SlavicGame.Engine.AI;

namespace SlavicGame.Engine.Animation;

/// <summary>
/// Maps EnemyAgent gameplay state to presentation-only animation states.
/// Hit reaction deliberately overrides locomotion/combat presentation without
/// mutating the underlying AI engagement state.
/// </summary>
public sealed class EnemyAnimationPresenter
{
    public const string Patrol = "enemy.patrol";
    public const string Alert = "enemy.alert";
    public const string Chase = "enemy.chase";
    public const string Attack = "enemy.attack";
    public const string Return = "enemy.return";
    public const string HitReact = "enemy.hit-react";
    public const string Dead = "enemy.dead";

    public AnimationStateMachine StateMachine { get; } = new();

    public EnemyAnimationPresenter()
    {
        StateMachine.Register(Patrol);
        StateMachine.Register(Alert);
        StateMachine.Register(Chase);
        StateMachine.Register(Attack);
        StateMachine.Register(Return);
        StateMachine.Register(HitReact);
        StateMachine.Register(Dead);
    }

    public void Update(EnemyAgent enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);

        StateMachine.Play(StateFor(enemy));
    }

    public static string StateFor(EnemyAgent enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        return !enemy.IsAlive || enemy.State == EnemyState.Dead
            ? Dead
            : enemy.IsHitReacting
                ? HitReact
                : enemy.State switch
                {
                    EnemyState.Patrol => Patrol,
                    EnemyState.Alert => Alert,
                    EnemyState.Chase => Chase,
                    EnemyState.Attack => Attack,
                    EnemyState.Return => Return,
                    _ => Patrol
                };

    }

    public static string ClipFor(EnemyAgent enemy) => StateFor(enemy) switch
    {
        HitReact => "Hit",
        Chase => "Run",
        Attack => "Attack",
        Patrol or Return => "Walk",
        Dead => "Death",
        _ => "Idle"
    };

    public static float ClipTimeFor(EnemyAgent enemy, float worldAnimationTime, float hitClipDuration, float attackClipDuration = 0f)
    {
        if (!enemy.IsHitReacting || !enemy.IsAlive)
        {
            if (enemy.IsAlive && enemy.State == EnemyState.Attack && float.IsFinite(attackClipDuration) && attackClipDuration > 0f)
                return MathF.Min(enemy.Attack.AnimationProgress * attackClipDuration, MathF.BitDecrement(attackClipDuration));
            return worldAnimationTime;
        }
        if (!float.IsFinite(hitClipDuration) || hitClipDuration <= 0f) return 0f;
        // Reaction plays once from the hit; global animation time would sample an arbitrary pose.
        return MathF.Min(enemy.HitReactionProgress * hitClipDuration, MathF.BitDecrement(hitClipDuration));
    }
}