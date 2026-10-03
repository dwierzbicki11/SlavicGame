using System.Numerics;

namespace SlavicGame.Engine.Combat;

/// <summary>
/// Connects the melee attack state machine to spatial hit detection. Only targets
/// inside the current attack's active window can be resolved, and each target is
/// emitted at most once per attack through MeleeAttackController.TryRegisterHit.
/// </summary>
public static class MeleeHitResolver
{
    public static IReadOnlyList<MeleeHitCandidate> ResolveActiveHits(
        MeleeAttackController controller,
        Vector3 attackerPosition,
        Vector3 attackerForward,
        float halfAngleDegrees,
        IEnumerable<MeleeHitCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(candidates);

        if (!controller.CanDealDamage || controller.CurrentAttack is not { } attack)
            return Array.Empty<MeleeHitCandidate>();

        var detected = MeleeHitDetection.FindTargets(
            attackerPosition,
            attackerForward,
            attack.Range,
            halfAngleDegrees,
            candidates);

        if (detected.Count == 0)
            return Array.Empty<MeleeHitCandidate>();

        var resolved = new List<MeleeHitCandidate>(detected.Count);
        foreach (var candidate in detected)
        {
            if (controller.TryRegisterHit(candidate.TargetId))
                resolved.Add(candidate);
        }

        return resolved;
    }
}
