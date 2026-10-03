using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Gameplay;

internal static class MeleeHitResolverRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var attack = new AttackDefinition(
            "regression-swing",
            Damage: 12f,
            StaminaCost: 10f,
            Range: 2f,
            WindupSeconds: 0.1,
            RecoverySeconds: 0.2,
            DamageType.Physical);
        var vitals = new PlayerVitals();
        var controller = new MeleeAttackController();
        var candidates = new[]
        {
            new MeleeHitCandidate("front", new Vector3(0f, 0f, 1.25f), 0.2f),
            new MeleeHitCandidate("behind", new Vector3(0f, 0f, -1f), 0.2f)
        };

        if (!controller.TryStart(attack, vitals))
            throw new InvalidOperationException("Regression attack did not start.");

        var duringWindup = MeleeHitResolver.ResolveActiveHits(
            controller, Vector3.Zero, Vector3.UnitZ, 50f, candidates);
        if (duringWindup.Count != 0)
            throw new InvalidOperationException("Melee resolver emitted damage during windup.");

        controller.Update(vitals, 0.1);
        if (!controller.CanDealDamage)
            throw new InvalidOperationException("Regression attack did not enter active window.");

        var firstPass = MeleeHitResolver.ResolveActiveHits(
            controller, Vector3.Zero, Vector3.UnitZ, 50f, candidates);
        if (firstPass.Count != 1 || firstPass[0].TargetId != "front")
            throw new InvalidOperationException("Active melee resolver selected incorrect targets.");

        var duplicatePass = MeleeHitResolver.ResolveActiveHits(
            controller, Vector3.Zero, Vector3.UnitZ, 50f, candidates);
        if (duplicatePass.Count != 0)
            throw new InvalidOperationException("Melee resolver emitted the same target twice in one swing.");

        controller.Update(vitals, 0.12);
        var duringRecovery = MeleeHitResolver.ResolveActiveHits(
            controller, Vector3.Zero, Vector3.UnitZ, 50f, candidates);
        if (duringRecovery.Count != 0)
            throw new InvalidOperationException("Melee resolver emitted damage during recovery.");
    }
}
