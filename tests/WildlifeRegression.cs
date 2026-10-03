using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.World;

internal static class WildlifeRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var actors = world.Wildlife.Actors;

        check(actors.Count == 17,
            "R0 wildlife runtime registers seventeen ambient animals");
        check(actors.Select(actor => actor.Id).Distinct(StringComparer.Ordinal).Count() == actors.Count,
            "Wildlife spawn IDs are unique");

        check(actors.Count(actor => actor.Species == WildlifeSpecies.Deer) == 5,
            "R0 wildlife includes five deer");
        check(actors.Count(actor => actor.Species == WildlifeSpecies.Boar) == 4,
            "R0 wildlife includes four boars");
        check(actors.Count(actor => actor.Species == WildlifeSpecies.Wolf) == 3,
            "R0 wildlife includes three wolves");
        check(actors.Count(actor => actor.Species == WildlifeSpecies.Raven) == 5,
            "R0 wildlife includes five ravens");

        foreach (var actor in actors)
        {
            var ground = world.Terrain.SampleHeight(actor.Position);
            if (actor.Species == WildlifeSpecies.Raven)
            {
                check(actor.Position.Y > ground + 5f,
                    $"{actor.Id} starts above terrain as a flying bird");
            }
            else
            {
                check(MathF.Abs(actor.Position.Y - ground) < 0.01f,
                    $"{actor.Id} starts grounded on terrain");
            }
        }

        var deerBefore = world.Wildlife.Actors.Single(actor => actor.Id == "deer-oak-01").Position;
        for (var i = 0; i < 20; i++)
            world.Wildlife.Update(world, 0.1);

        var deerAfterWander = world.Wildlife.Actors.Single(actor => actor.Id == "deer-oak-01");
        check(Vector3.Distance(deerBefore, deerAfterWander.Position) > 0.5f &&
              deerAfterWander.Behavior == WildlifeBehavior.Wander,
            "Deer wanders through its local forest territory when undisturbed");

        world.SetPlayerPosition(deerAfterWander.Position);
        world.Wildlife.Update(world, 0.1);
        var deerFleeStart = world.Wildlife.Actors.Single(actor => actor.Id == "deer-oak-01");
        check(deerFleeStart.Behavior == WildlifeBehavior.Flee,
            "Deer enters flee behavior when the player approaches");

        var playerAtScare = world.PlayerPosition;
        var distanceBeforeFlee = Vector2.Distance(
            new Vector2(deerFleeStart.Position.X, deerFleeStart.Position.Z),
            new Vector2(playerAtScare.X, playerAtScare.Z));

        for (var i = 0; i < 12; i++)
            world.Wildlife.Update(world, 0.1);

        var deerFleeAfter = world.Wildlife.Actors.Single(actor => actor.Id == "deer-oak-01");
        var distanceAfterFlee = Vector2.Distance(
            new Vector2(deerFleeAfter.Position.X, deerFleeAfter.Position.Z),
            new Vector2(playerAtScare.X, playerAtScare.Z));

        check(distanceAfterFlee > distanceBeforeFlee + 1f,
            "Fleeing deer increases distance from the player");

        var raven = world.Wildlife.Actors.Single(actor => actor.Id == "raven-start-01");
        world.SetPlayerPosition(raven.Position);
        var ravenAltitudeBefore = raven.Position.Y;
        for (var i = 0; i < 8; i++)
            world.Wildlife.Update(world, 0.1);

        var ravenAfter = world.Wildlife.Actors.Single(actor => actor.Id == "raven-start-01");
        check(ravenAfter.Behavior == WildlifeBehavior.Spooked &&
              ravenAfter.Position.Y > ravenAltitudeBefore,
            "Raven is spooked by a nearby player and gains altitude");

        var assetsRoot = Path.Combine(AppContext.BaseDirectory, "assets");
        var models = WildlifeCatalog.RequiredModelFiles.ToDictionary(
            file => file,
            file =>
            {
                var path = Path.Combine(assetsRoot, "models", "animated", file);
                check(File.Exists(path), $"Wildlife model exists: {file}");
                var model = GlbModel.Load(path);
                check(model.AnimationNames.Count > 0,
                    $"Wildlife model exposes animation clips: {file}");
                return model;
            },
            StringComparer.Ordinal);

        check(models.Count == 4,
            "Wildlife uses four dedicated animated model families");

        TerrainVertex[] nearVertices = [];
        uint[] nearIndices = [];
        var oakDeer = world.Wildlife.Actors.Single(actor => actor.Id == "deer-oak-02");
        WildlifeModelMesh.Append(
            world,
            models,
            0.75,
            oakDeer.Position,
            120f,
            ref nearVertices,
            ref nearIndices);

        check(nearVertices.Length > 0 &&
              nearIndices.Length > 0 &&
              nearIndices.All(index => index < nearVertices.Length),
            "Nearby wildlife produces valid animated actor geometry");

        TerrainVertex[] farVertices = [];
        uint[] farIndices = [];
        WildlifeModelMesh.Append(
            world,
            models,
            0.75,
            new Vector3(950f, 0f, 950f),
            80f,
            ref farVertices,
            ref farIndices);

        check(farVertices.Length == 0 && farIndices.Length == 0,
            "Wildlife outside the camera culling radius builds no geometry");
    }
}
