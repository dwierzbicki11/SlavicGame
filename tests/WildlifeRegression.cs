using System.Numerics;
using SlavicGame.Engine.World;

internal static class WildlifeRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var actors = world.Wildlife.Actors;

        check(actors.Count == 11,
            "R0 wildlife population is bounded to 11 ambient actors");
        check(actors.Count(actor => actor.Species == WildlifeSpecies.Deer) == 3 &&
              actors.Count(actor => actor.Species == WildlifeSpecies.Boar) == 2 &&
              actors.Count(actor => actor.Species == WildlifeSpecies.Wolf) == 2 &&
              actors.Count(actor => actor.Species == WildlifeSpecies.Raven) == 4,
            "R0 wildlife roster contains deer boar wolf and raven groups");

        foreach (var actor in actors.Where(actor =>
                     !WildlifeVisualCatalog.IsFlying(actor.Species)))
        {
            var ground = world.Terrain.SampleHeight(actor.Position);
            check(MathF.Abs(actor.Position.Y - ground) < 0.001f,
                $"Ground wildlife {actor.Id} starts on terrain");
        }

        foreach (var raven in actors.Where(actor =>
                     actor.Species == WildlifeSpecies.Raven))
        {
            var ground = world.Terrain.SampleHeight(raven.Position);
            check(raven.Position.Y - ground >= 9f &&
                  raven.Motion == WildlifeMotion.Fly &&
                  raven.AnimationClip == "Fly",
                $"Raven {raven.Id} starts airborne with Fly animation");
        }

        var baseline = actors.ToDictionary(
            actor => actor.Id,
            actor => actor.Position,
            StringComparer.Ordinal);

        for (var i = 0; i < 30; i++)
            world.Wildlife.Update(world, 0.1);

        check(actors.Where(actor =>
                !WildlifeVisualCatalog.IsFlying(actor.Species))
            .Any(actor =>
                Vector3.DistanceSquared(
                    actor.Position,
                    baseline[actor.Id]) > 0.01f),
            "Ground wildlife wanders over time");

        check(actors.Where(actor => actor.Species == WildlifeSpecies.Raven)
            .All(actor =>
                Vector3.DistanceSquared(
                    actor.Position,
                    baseline[actor.Id]) > 0.01f),
            "All ravens visibly fly around their home ranges");

        var deer = actors.First(actor =>
            actor.Species == WildlifeSpecies.Deer);
        var deerBefore = deer.Position;
        var playerNearDeer = deer.Position + new Vector3(1.5f, 0f, 0f);
        world.SetPlayerPosition(playerNearDeer);

        var player2 = new Vector2(
            world.PlayerPosition.X,
            world.PlayerPosition.Z);
        var beforeDistance = Vector2.Distance(
            new Vector2(deer.Position.X, deer.Position.Z),
            player2);

        for (var i = 0; i < 10; i++)
            world.Wildlife.Update(world, 0.1);

        var afterDistance = Vector2.Distance(
            new Vector2(deer.Position.X, deer.Position.Z),
            player2);

        check(deer.Motion == WildlifeMotion.Flee &&
              deer.AnimationClip == "Run" &&
              afterDistance > beforeDistance &&
              Vector3.DistanceSquared(deer.Position, deerBefore) > 0.1f,
            "Deer flees from a nearby player using Run animation");

        var villageCenter = new Vector2(0f, -85f);
        check(actors.Where(actor =>
                !WildlifeVisualCatalog.IsFlying(actor.Species))
            .All(actor =>
                Vector2.Distance(
                    new Vector2(actor.Position.X, actor.Position.Z),
                    villageCenter) >= 37.5f),
            "Ambient ground wildlife stays outside the village core");

        var world2 = WorldGenerator.Generate();
        check(world2.Wildlife.Actors.Count == actors.Count &&
              world2.Wildlife.Actors
                  .OrderBy(actor => actor.Id, StringComparer.Ordinal)
                  .Zip(
                      WorldGenerator.Generate().Wildlife.Actors
                          .OrderBy(actor => actor.Id, StringComparer.Ordinal))
                  .All(pair =>
                      pair.First.Id == pair.Second.Id &&
                      Vector3.DistanceSquared(
                          pair.First.Position,
                          pair.Second.Position) < 0.000001f),
            "Wildlife initial population is deterministic across worlds");

        check(
            WildlifeVisualCatalog.ModelFile(WildlifeSpecies.Deer) ==
                "deer_animated.glb" &&
            WildlifeVisualCatalog.ModelFile(WildlifeSpecies.Boar) ==
                "boar_animated.glb" &&
            WildlifeVisualCatalog.ModelFile(WildlifeSpecies.Wolf) ==
                "wolf_animated.glb" &&
            WildlifeVisualCatalog.ModelFile(WildlifeSpecies.Raven) ==
                "raven_animated.glb",
            "Wildlife species resolve to their intended animated GLB families");
    }
}
