using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.NPC;

namespace SlavicGame.Engine.World;

public enum NpcReactionKind
{
    None,
    WatchPlayer,
    AvoidPlayer,
    FleeThreat,
    GuardThreat
}

public readonly record struct NpcReactionSample(
    NpcReactionKind Kind,
    Vector2 Position,
    Vector2 Forward,
    bool IsMoving);

public static class NpcReactionSystem
{
    public const float PlayerAttackAwarenessRange = 5.2f;
    public const float PlayerAvoidRange = 2.8f;
    public const float EnemyAwarenessRange = 17f;
    public const float CivilianFleeDistance = 3.4f;
    public const float GuardStandOffDistance = 4.5f;

    public static NpcReactionSample Resolve(
        WorldState world,
        NpcDefinition npc,
        Vector2 basePosition,
        Vector2 baseForward,
        bool baseMoving,
        bool protectedByDialogue)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(npc);

        if (protectedByDialogue)
        {
            return new NpcReactionSample(
                NpcReactionKind.None,
                basePosition,
                baseForward,
                false);
        }

        var threat = NearestEngagedEnemy(world, basePosition);
        if (threat is not null)
        {
            var threat2 = new Vector2(
                threat.Position.X,
                threat.Position.Z);
            var delta = basePosition - threat2;
            var distance = delta.Length();

            if (distance <= EnemyAwarenessRange)
            {
                var away = SafeDirection(
                    delta,
                    npc.Id,
                    threat.Id);

                if (npc.Role == NpcRole.CommunityGuard)
                {
                    var desired = threat2 - away * GuardStandOffDistance;
                    var forward = SafeDirection(
                        threat2 - desired,
                        npc.Id,
                        threat.Id + "-guard");

                    return new NpcReactionSample(
                        NpcReactionKind.GuardThreat,
                        desired,
                        forward,
                        true);
                }

                return new NpcReactionSample(
                    NpcReactionKind.FleeThreat,
                    basePosition + away * CivilianFleeDistance,
                    away,
                    true);
            }
        }

        var player2 = new Vector2(
            world.PlayerPosition.X,
            world.PlayerPosition.Z);
        var playerDelta = basePosition - player2;
        var playerDistance = playerDelta.Length();
        var playerIsThreatening =
            world.Melee.Controller.State != MeleeAttackState.Free ||
            world.Magic.IsCasting;

        if (playerIsThreatening &&
            playerDistance <= PlayerAttackAwarenessRange)
        {
            var away = SafeDirection(
                playerDelta,
                npc.Id,
                "player");

            if (npc.Role == NpcRole.CommunityGuard)
            {
                return new NpcReactionSample(
                    NpcReactionKind.WatchPlayer,
                    basePosition,
                    -away,
                    false);
            }

            if (playerDistance < PlayerAvoidRange)
            {
                return new NpcReactionSample(
                    NpcReactionKind.AvoidPlayer,
                    basePosition + away * (PlayerAvoidRange - playerDistance + 0.9f),
                    away,
                    true);
            }

            return new NpcReactionSample(
                NpcReactionKind.WatchPlayer,
                basePosition,
                -away,
                false);
        }

        return new NpcReactionSample(
            NpcReactionKind.None,
            basePosition,
            baseForward,
            baseMoving);
    }

    private static EnemyAgent? NearestEngagedEnemy(
        WorldState world,
        Vector2 position)
    {
        EnemyAgent? best = null;
        var bestDistanceSquared =
            EnemyAwarenessRange * EnemyAwarenessRange;

        foreach (var enemy in world.Enemies)
        {
            if (!enemy.IsAlive ||
                enemy.State is not (
                    EnemyState.Alert or
                    EnemyState.Chase or
                    EnemyState.Attack))
            {
                continue;
            }

            var delta =
                new Vector2(
                    enemy.Position.X,
                    enemy.Position.Z) -
                position;
            var distanceSquared = delta.LengthSquared();
            if (distanceSquared > bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            best = enemy;
        }

        return best;
    }

    private static Vector2 SafeDirection(
        Vector2 value,
        string firstSeed,
        string secondSeed)
    {
        if (value.LengthSquared() > 0.000001f)
            return Vector2.Normalize(value);

        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in firstSeed)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            foreach (var ch in secondSeed)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            var angle =
                (hash % 4096u) /
                4096f *
                MathF.Tau;

            return new Vector2(
                MathF.Cos(angle),
                MathF.Sin(angle));
        }
    }
}
