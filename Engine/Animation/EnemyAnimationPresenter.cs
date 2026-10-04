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

        var state = !enemy.IsAlive || enemy.State == EnemyState.Dead
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

        StateMachine.Play(state);
    }
}