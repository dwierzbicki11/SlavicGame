using System.Numerics;
using SlavicGame.Engine.World;

internal static class WildlifeTrackingRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var durableBefore = world.Progress.Tracking.Tracks.Count;

        check(world.WildlifeTracks.Marks.Count == 0,
            "Wildlife trail starts empty");

        for (var i = 0; i < 180; i++)
        {
            world.Wildlife.Update(world, 0.1);
            world.WildlifeTracks.Update(world, 0.1);
        }

        var marks = world.WildlifeTracks.Marks;
        check(marks.Count > 0 && marks.Count <= 180,
            "Moving ground wildlife leaves a bounded transient track trail");
        check(marks.All(mark => mark.Species != WildlifeSpecies.Raven),
            "Flying ravens never stamp ground tracks");
        check(marks.Any(mark => mark.Species == WildlifeSpecies.Deer) &&
              marks.Any(mark => mark.Species == WildlifeSpecies.Boar) &&
              marks.Any(mark => mark.Species == WildlifeSpecies.Wolf),
            "Deer boar and wolf can all leave physical tracks");
        check(marks.All(mark =>
                mark.Strength >= 0.20f &&
                WaterInteractionState.DepthAt(world, mark.Position) <= 0.035f),
            "Wildlife tracks only stamp on readable non-submerged ground");

        var fresh = marks.OrderBy(mark => mark.AgeSeconds).First();
        check(fresh.Freshness is TrackFreshness.Fresh or TrackFreshness.Recent,
            "Newest wildlife mark reports a useful freshness class");

        world.SetPlayerPosition(fresh.Position);
        var nearest = world.WildlifeTracks.FindNearest(world.PlayerPosition);
        check(nearest is not null &&
              nearest.SourceId == fresh.SourceId &&
              !string.IsNullOrWhiteSpace(world.WildlifeTracks.HudStatus(world)),
            "Approaching a wildlife track exposes species and freshness HUD feedback");

        TerrainVertex[] nearVertices = [];
        uint[] nearIndices = [];
        WildlifeTrackEffectMesh.Append(
            world,
            ref nearVertices,
            ref nearIndices);

        check(nearVertices.Length > 0 &&
              nearIndices.Length > 0 &&
              nearIndices.All(index => index < nearVertices.Length),
            "Nearby wildlife marks produce valid low-cost track geometry");

        world.SetPlayerPosition(new Vector3(900f, 0f, 900f));
        TerrainVertex[] farVertices = [];
        uint[] farIndices = [];
        WildlifeTrackEffectMesh.Append(
            world,
            ref farVertices,
            ref farIndices);

        check(farVertices.Length == 0 && farIndices.Length == 0,
            "Distant wildlife tracks are culled before mesh generation");

        var countBeforeRain = world.WildlifeTracks.Marks.Count;
        var ageBeforeRain = world.WildlifeTracks.Marks.Average(mark => mark.AgeSeconds);
        world.Weather.SetCondition(WeatherKind.Storm, immediate: true);
        world.WildlifeTracks.Update(world, 5.0);
        var marksAfterRain = world.WildlifeTracks.Marks;
        var ageAfterRain = marksAfterRain.Count == 0
            ? float.PositiveInfinity
            : marksAfterRain.Average(mark => mark.AgeSeconds);

        check(ageAfterRain > ageBeforeRain + 5f ||
              marksAfterRain.Count < countBeforeRain,
            "Heavy rain erodes wildlife tracks faster than wall-clock age");

        check(
            WildlifeTrackTrailState.Trackability(
                new TerrainSurfaceWeights(0f, 1f, 0f, 0f, 0f, 0f),
                0f) >= 0.5f &&
            WildlifeTrackTrailState.Trackability(
                new TerrainSurfaceWeights(0f, 0f, 0f, 0f, 0f, 1f),
                0f) == 0f,
            "Forest litter accepts animal tracks while bare rock rejects them");

        check(world.Progress.Tracking.Tracks.Count == durableBefore,
            "Ambient hunting tracks never create durable quest evidence");
    }
}
