using System.Numerics;

namespace SlavicGame.Engine.Interaction;

public enum InteractionKind
{
    Inspect,
    Talk,
    Take,
    Use,
    Activate,
    Rest,
    Trade
}

public sealed record InteractionTarget(
    string Id,
    Vector3 Position,
    InteractionKind Kind,
    string Prompt,
    bool Enabled = true);

public static class InteractionSystem
{
    public static InteractionTarget? FindNearest(
        Vector3 origin,
        IEnumerable<InteractionTarget> targets,
        float maxDistance)
    {
        ArgumentNullException.ThrowIfNull(targets);
        if (!float.IsFinite(maxDistance) || maxDistance < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDistance));
        }

        InteractionTarget? nearest = null;
        var nearestDistanceSquared = maxDistance * maxDistance;

        foreach (var target in targets)
        {
            if (!target.Enabled)
            {
                continue;
            }

            var distanceSquared = Vector3.DistanceSquared(origin, target.Position);
            if (distanceSquared > nearestDistanceSquared)
            {
                continue;
            }

            nearest = target;
            nearestDistanceSquared = distanceSquared;
        }

        return nearest;
    }
}
