using System.Numerics;

namespace SlavicGame.Engine.Combat;

public readonly record struct MeleeHitCandidate(
    string TargetId,
    Vector3 Position,
    float Radius = 0f);

public static class MeleeHitDetection
{
    /// <summary>
    /// Selects targets intersecting a horizontal melee sector. Results are ordered by
    /// distance and then target id so hit processing remains deterministic.
    /// </summary>
    public static IReadOnlyList<MeleeHitCandidate> FindTargets(
        Vector3 attackerPosition,
        Vector3 attackerForward,
        float range,
        float halfAngleDegrees,
        IEnumerable<MeleeHitCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (!IsFinite(attackerPosition) || !IsFinite(attackerForward))
            throw new ArgumentException("Melee hit query requires finite vectors.");
        if (!float.IsFinite(range) || range <= 0f)
            throw new ArgumentOutOfRangeException(nameof(range));
        if (!float.IsFinite(halfAngleDegrees) || halfAngleDegrees <= 0f || halfAngleDegrees > 180f)
            throw new ArgumentOutOfRangeException(nameof(halfAngleDegrees));

        var forward = new Vector2(attackerForward.X, attackerForward.Z);
        if (forward.LengthSquared() < 0.000001f)
            return Array.Empty<MeleeHitCandidate>();
        forward = Vector2.Normalize(forward);

        var cosineLimit = MathF.Cos(halfAngleDegrees * MathF.PI / 180f);
        var hits = new List<(MeleeHitCandidate Candidate, float DistanceSquared)>();

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate.TargetId) ||
                !IsFinite(candidate.Position) ||
                !float.IsFinite(candidate.Radius) || candidate.Radius < 0f)
            {
                continue;
            }

            var delta = candidate.Position - attackerPosition;
            var horizontal = new Vector2(delta.X, delta.Z);
            var distanceSquared = horizontal.LengthSquared();
            var effectiveRange = range + candidate.Radius;
            if (distanceSquared > effectiveRange * effectiveRange)
                continue;

            // A collider overlapping the attack origin is always inside the sector.
            if (distanceSquared > 0.000001f)
            {
                var direction = horizontal / MathF.Sqrt(distanceSquared);
                if (Vector2.Dot(forward, direction) < cosineLimit)
                    continue;
            }

            hits.Add((candidate, distanceSquared));
        }

        return hits
            .OrderBy(hit => hit.DistanceSquared)
            .ThenBy(hit => hit.Candidate.TargetId, StringComparer.Ordinal)
            .Select(hit => hit.Candidate)
            .ToArray();
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
