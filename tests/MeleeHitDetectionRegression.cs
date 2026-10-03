using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.Combat;

internal static class MeleeHitDetectionRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var hits = MeleeHitDetection.FindTargets(
            Vector3.Zero,
            Vector3.UnitZ,
            2f,
            50f,
            [
                new("front-near", new Vector3(0f, 0f, 1f), 0.2f),
                new("front-far", new Vector3(0.5f, 0f, 1.7f), 0.2f),
                new("behind", new Vector3(0f, 0f, -1f), 0.2f),
                new("too-far", new Vector3(0f, 0f, 2.5f), 0.1f),
                new("side", new Vector3(1.5f, 0f, 0.2f), 0.1f)
            ]);

        if (hits.Count != 2 || hits[0].TargetId != "front-near" || hits[1].TargetId != "front-far")
            throw new InvalidOperationException("Melee front-arc query selected incorrect targets.");

        var radiusHit = MeleeHitDetection.FindTargets(
            Vector3.Zero,
            Vector3.UnitZ,
            2f,
            45f,
            [new("radius-overlap", new Vector3(0f, 0f, 2.3f), 0.35f)]);
        if (radiusHit.Count != 1)
            throw new InvalidOperationException("Melee query ignored target collider radius.");

        var noForward = MeleeHitDetection.FindTargets(
            Vector3.Zero,
            Vector3.Zero,
            2f,
            45f,
            [new("target", Vector3.UnitZ)]);
        if (noForward.Count != 0)
            throw new InvalidOperationException("Degenerate melee facing must not produce hits.");
    }
}
