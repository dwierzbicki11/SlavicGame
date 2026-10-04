using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Animation;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.World;

internal static class EnemyHitReactionPresentationRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var enemy = new EnemyAgent("presentation-target", Vector3.Zero);
        var presenter = new EnemyAnimationPresenter();
        presenter.Update(enemy);
        if (presenter.StateMachine.CurrentState != EnemyAnimationPresenter.Patrol)
            throw new InvalidOperationException("Enemy presentation did not start in patrol.");

        enemy.Restore(new EnemySnapshot(enemy.Id, Vector3.Zero, enemy.MaxHealth, EnemyState.Chase));
        var attack = new AttackDefinition(
            "presentation-hit",
            Damage: 10f,
            StaminaCost: 0f,
            Range: 1.8f,
            WindupSeconds: 0.1,
            RecoverySeconds: 0.2,
            DamageType.Physical);
        DamageApplication.ApplyMeleeHit(attack, enemy.Id, enemy);
        presenter.Update(enemy);
        if (enemy.State != EnemyState.Chase || presenter.StateMachine.CurrentState != EnemyAnimationPresenter.HitReact)
            throw new InvalidOperationException("Hit reaction presentation did not override preserved Chase state.");

        var world = new WorldState();
        enemy.Update(world, 0.181);
        presenter.Update(enemy);
        if (enemy.IsHitReacting || presenter.StateMachine.CurrentState != EnemyAnimationPresenter.Attack)
            throw new InvalidOperationException("Presentation did not resume the live AI state after hit reaction.");

        DamageApplication.ApplyMeleeHit(new AttackDefinition(
            "presentation-lethal",
            Damage: enemy.MaxHealth,
            StaminaCost: 0f,
            Range: 1.8f,
            WindupSeconds: 0.1,
            RecoverySeconds: 0.2,
            DamageType.Physical), enemy.Id, enemy);
        presenter.Update(enemy);
        if (presenter.StateMachine.CurrentState != EnemyAnimationPresenter.Dead)
            throw new InvalidOperationException("Death presentation did not override transient combat presentation.");
    }
}