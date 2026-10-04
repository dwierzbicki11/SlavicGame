using System.Numerics;

namespace SlavicGame.Engine.World;

public static class NpcCrowdSteering
{
    public const float BodyRadius = 0.38f;
    public const float MinimumSpacing = BodyRadius * 2f;
    public const float PlayerSpacing = 0.72f;
    public const int SolverPasses = 2;
    public const float MaxPushPerPass = 0.40f;

    public static void Resolve(
        WorldState world,
        List<NpcWorldActor> actors,
        string? protectedActorId = null)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actors);

        if (actors.Count == 0)
            return;

        for (var pass = 0; pass < SolverPasses; pass++)
        {
            ResolveNpcPairs(world, actors, protectedActorId);
            ResolvePlayerSpacing(world, actors, protectedActorId);
        }
    }

    private static void ResolveNpcPairs(
        WorldState world,
        List<NpcWorldActor> actors,
        string? protectedActorId)
    {
        for (var i = 0; i < actors.Count; i++)
        {
            for (var j = i + 1; j < actors.Count; j++)
            {
                var a = actors[i];
                var b = actors[j];

                var a2 = new Vector2(a.Position.X, a.Position.Z);
                var b2 = new Vector2(b.Position.X, b.Position.Z);
                var delta = b2 - a2;
                var distanceSquared = delta.LengthSquared();

                if (distanceSquared >= MinimumSpacing * MinimumSpacing)
                    continue;

                var direction = distanceSquared > 0.000001f
                    ? Vector2.Normalize(delta)
                    : StableFallbackDirection(a.Id, b.Id);

                var distance = MathF.Sqrt(MathF.Max(distanceSquared, 0f));
                var penetration = MinimumSpacing - distance;
                if (penetration <= 0f)
                    continue;

                var aProtected = string.Equals(
                    a.Id,
                    protectedActorId,
                    StringComparison.Ordinal);
                var bProtected = string.Equals(
                    b.Id,
                    protectedActorId,
                    StringComparison.Ordinal);

                if (aProtected && bProtected)
                    continue;

                var push = MathF.Min(
                    MaxPushPerPass,
                    penetration * (aProtected || bProtected ? 1f : 0.5f));

                if (!aProtected)
                {
                    a = WithResolvedPosition(
                        world,
                        a,
                        a2 - direction * push);
                    actors[i] = a;
                }

                if (!bProtected)
                {
                    b = WithResolvedPosition(
                        world,
                        b,
                        b2 + direction * push);
                    actors[j] = b;
                }
            }
        }
    }

    private static void ResolvePlayerSpacing(
        WorldState world,
        List<NpcWorldActor> actors,
        string? protectedActorId)
    {
        var player = new Vector2(
            world.PlayerPosition.X,
            world.PlayerPosition.Z);

        for (var i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];

            if (!actor.IsMoving ||
                string.Equals(
                    actor.Id,
                    protectedActorId,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var position = new Vector2(
                actor.Position.X,
                actor.Position.Z);
            var delta = position - player;
            var distanceSquared = delta.LengthSquared();

            if (distanceSquared >= PlayerSpacing * PlayerSpacing)
                continue;

            var direction = distanceSquared > 0.000001f
                ? Vector2.Normalize(delta)
                : StableFallbackDirection(actor.Id, "player");

            var distance = MathF.Sqrt(MathF.Max(distanceSquared, 0f));
            var penetration = PlayerSpacing - distance;
            var push = MathF.Min(MaxPushPerPass, penetration);

            actors[i] = WithResolvedPosition(
                world,
                actor,
                position + direction * push);
        }
    }

    private static NpcWorldActor WithResolvedPosition(
        WorldState world,
        NpcWorldActor actor,
        Vector2 requested)
    {
        var resolved = world.ResolveHorizontalPosition(
            requested,
            BodyRadius);

        var position = new Vector3(
            resolved.X,
            0f,
            resolved.Y);
        position.Y = world.Terrain.SampleHeight(position);

        return actor with
        {
            Position = position
        };
    }

    private static Vector2 StableFallbackDirection(
        string first,
        string second)
    {
        unchecked
        {
            uint hash = 2166136261;

            foreach (var ch in first)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            foreach (var ch in second)
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
