using System.Numerics;
using SlavicGame.Engine.NPC;
using SlavicGame.Engine.World;

internal static class NpcCrowdSteeringRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.SetPlayerPosition(new Vector3(400f, 0f, 400f));

        var overlapA = CreateActor(world, "crowd-a", 45f, -45f, moving: true);
        var overlapB = CreateActor(world, "crowd-b", 45f, -45f, moving: true);
        var actors = new List<NpcWorldActor> { overlapA, overlapB };

        NpcCrowdSteering.Resolve(world, actors);

        check(
            HorizontalDistance(actors[0], actors[1]) >=
                NpcCrowdSteering.MinimumSpacing - 0.01f,
            "Crowd steering separates two NPCs spawned at the same point");

        var deterministicA = new List<NpcWorldActor>
        {
            overlapA,
            overlapB
        };
        var deterministicB = new List<NpcWorldActor>
        {
            overlapA,
            overlapB
        };

        NpcCrowdSteering.Resolve(world, deterministicA);
        NpcCrowdSteering.Resolve(world, deterministicB);

        check(
            deterministicA.Zip(deterministicB).All(pair =>
                Vector3.DistanceSquared(
                    pair.First.Position,
                    pair.Second.Position) < 0.000001f),
            "Crowd steering is deterministic and cannot jitter from random pushes");

        var speaker = CreateActor(
            world,
            "speaker",
            60f,
            -40f,
            moving: false);
        var passer = CreateActor(
            world,
            "passer",
            60f,
            -40f,
            moving: true);
        var protectedActors = new List<NpcWorldActor>
        {
            speaker,
            passer
        };

        var speakerBefore = protectedActors[0].Position;
        NpcCrowdSteering.Resolve(
            world,
            protectedActors,
            protectedActorId: "speaker");

        check(
            Vector3.DistanceSquared(
                protectedActors[0].Position,
                speakerBefore) < 0.000001f,
            "NPC currently protected by dialogue remains fixed");

        check(
            HorizontalDistance(
                protectedActors[0],
                protectedActors[1]) >=
                NpcCrowdSteering.MinimumSpacing - 0.01f,
            "Other NPC yields around the protected dialogue speaker");

        world.SetPlayerPosition(new Vector3(72f, 0f, -40f));
        var movingThroughPlayer = new List<NpcWorldActor>
        {
            CreateActor(
                world,
                "walker",
                world.PlayerPosition.X,
                world.PlayerPosition.Z,
                moving: true)
        };

        NpcCrowdSteering.Resolve(
            world,
            movingThroughPlayer);

        var playerDistance = Vector2.Distance(
            new Vector2(
                movingThroughPlayer[0].Position.X,
                movingThroughPlayer[0].Position.Z),
            new Vector2(
                world.PlayerPosition.X,
                world.PlayerPosition.Z));

        check(
            playerDistance >=
                NpcCrowdSteering.PlayerSpacing - 0.01f,
            "Moving NPC keeps personal space from the player");

        world.Time.SetTimeOfDay(10.0);
        world.Weather.SetCondition(
            WeatherKind.Clear,
            immediate: true);
        world.NpcWorld.Update(world);

        check(
            world.NpcWorld.Actors.Count >= 15,
            "Crowd steering preserves the active R0 NPC population");

        check(
            world.NpcWorld.Actors
                .Select(actor => actor.Id)
                .Distinct(StringComparer.Ordinal)
                .Count() ==
            world.NpcWorld.Actors.Count,
            "Crowd steering preserves one runtime actor per NPC");
    }

    private static NpcWorldActor CreateActor(
        WorldState world,
        string id,
        float x,
        float z,
        bool moving)
    {
        var position = new Vector3(x, 0f, z);
        position.Y = world.Terrain.SampleHeight(position);

        return new NpcWorldActor(
            id,
            NpcRole.Worker,
            "test",
            "old-village",
            position,
            0f,
            moving);
    }

    private static float HorizontalDistance(
        NpcWorldActor a,
        NpcWorldActor b) =>
        Vector2.Distance(
            new Vector2(a.Position.X, a.Position.Z),
            new Vector2(b.Position.X, b.Position.Z));
}
