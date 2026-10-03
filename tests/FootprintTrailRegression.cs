using System.Numerics;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class FootprintTrailRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check(
            FootprintTrailState.Trackability(
                new TerrainSurfaceWeights(0f, 0f, 0f, 1f, 0f, 0f),
                0f,
                0f) > 0.9f,
            "Pure mud strongly accepts physical footprints");

        check(
            FootprintTrailState.Trackability(
                new TerrainSurfaceWeights(0f, 0f, 0f, 0f, 0f, 1f),
                1f,
                1f) < 0.05f,
            "Rock rejects footprints even when player and weather are wet");

        var world = WorldGenerator.Generate();
        world.Weather.SetCondition(WeatherKind.Clear, true);

        var start = FindTrackableBank(world, 0f);
        world.SetPlayerPosition(start);
        world.WaterInteraction.Reset(world.PlayerPosition);
        world.Footprints.Reset(world.PlayerPosition);

        var durableTracksBefore = world.Progress.Tracking.Tracks.Count;

        for (var i = 1; i <= 7; i++)
        {
            var z = start.Z + i * 0.85f;
            var x =
                WaterLandscape.CenterX(z) +
                WaterLandscape.SurfaceHalfWidth(z) +
                2.4f;

            world.SetPlayerPosition(new Vector3(x, 0f, z));
            world.WaterInteraction.Update(world, 0.14);
            world.Footprints.Update(world, 0.14);
        }

        check(world.Footprints.Footprints.Count >= 5,
            "Walking along a muddy river bank creates a visible footprint trail");

        check(world.Footprints.Footprints.Count <= 96,
            "Transient footprint population stays inside its hard runtime budget");

        var alternating = world.Footprints.Footprints
            .Zip(world.Footprints.Footprints.Skip(1))
            .All(pair => pair.First.LeftFoot != pair.Second.LeftFoot);
        check(alternating,
            "Generated footprints alternate left and right foot");

        check(
            world.Progress.Tracking.Tracks.Count == durableTracksBefore,
            "Player footprints do not pollute durable quest/evidence tracking state");

        TerrainVertex[] footprintVertices = [];
        uint[] footprintIndices = [];
        FootprintEffectMesh.Append(
            world,
            ref footprintVertices,
            ref footprintIndices);

        check(
            footprintVertices.Length >= world.Footprints.Footprints.Count * 4 &&
            footprintIndices.Length > 0 &&
            footprintIndices.All(index => index < footprintVertices.Length),
            "Footprint renderer emits valid bounded ground geometry");

        var saved = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, saved);
        check(restored.Footprints.Footprints.Count == 0,
            "Transient player footprints intentionally do not persist through save/load");

        var waterWorld = WorldGenerator.Generate();
        var waterStart = new Vector3(
            WaterLandscape.CenterX(0f),
            0f,
            0f);
        waterWorld.SetPlayerPosition(waterStart);
        waterWorld.WaterInteraction.Reset(waterWorld.PlayerPosition);
        waterWorld.WaterInteraction.Update(waterWorld, 0.1);
        waterWorld.Footprints.Reset(waterWorld.PlayerPosition);

        for (var i = 1; i <= 4; i++)
        {
            var z = i * 0.9f;
            waterWorld.SetPlayerPosition(new Vector3(
                WaterLandscape.CenterX(z),
                0f,
                z));
            waterWorld.WaterInteraction.Update(waterWorld, 0.15);
            waterWorld.Footprints.Update(waterWorld, 0.15);
        }

        check(
            waterWorld.WaterInteraction.IsInWater &&
            waterWorld.Footprints.Footprints.Count == 0,
            "Walking inside the river does not stamp footprints on the submerged bed");

        var rainWorld = WorldGenerator.Generate();
        var rainStart = FindTrackableBank(rainWorld, 30f);
        rainWorld.SetPlayerPosition(rainStart);
        rainWorld.WaterInteraction.Reset(rainWorld.PlayerPosition);
        rainWorld.Footprints.Reset(rainWorld.PlayerPosition);

        for (var i = 1; i <= 5; i++)
        {
            var z = rainStart.Z + i * 0.85f;
            rainWorld.SetPlayerPosition(new Vector3(
                WaterLandscape.CenterX(z) +
                WaterLandscape.SurfaceHalfWidth(z) +
                2.4f,
                0f,
                z));
            rainWorld.Footprints.Update(rainWorld, 0.14);
        }

        check(rainWorld.Footprints.Footprints.Count > 0,
            "Rain-fade test starts with physical footprints");

        rainWorld.Weather.SetCondition(WeatherKind.Storm, true);
        rainWorld.Footprints.Update(rainWorld, 20.0);

        check(rainWorld.Footprints.Footprints.Count == 0,
            "Heavy rain rapidly erases old physical footprints");
    }

    private static Vector3 FindTrackableBank(
        WorldState world,
        float startZ)
    {
        for (var offsetZ = 0f; offsetZ <= 80f; offsetZ += 4f)
        {
            var z = startZ + offsetZ;
            var x =
                WaterLandscape.CenterX(z) +
                WaterLandscape.SurfaceHalfWidth(z) +
                2.4f;
            var point = new Vector3(x, 0f, z);
            point.Y = world.Terrain.SampleHeight(point);

            var trackability =
                FootprintTrailState.TrackabilityAt(
                    world,
                    point);

            if (trackability >= 0.30f)
                return point;
        }

        throw new InvalidOperationException(
            "Could not find a deterministic trackable river-bank sample.");
    }
}
