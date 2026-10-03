using System.Numerics;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.World;

internal static class WeatherVisualRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check(
            GraphicsQualityCatalog.RainStreakCount(CloudQuality.Low) <
            GraphicsQualityCatalog.RainStreakCount(CloudQuality.Medium) &&
            GraphicsQualityCatalog.RainStreakCount(CloudQuality.Medium) <
            GraphicsQualityCatalog.RainStreakCount(CloudQuality.High) &&
            GraphicsQualityCatalog.RainStreakCount(CloudQuality.High) <
            GraphicsQualityCatalog.RainStreakCount(CloudQuality.Ultra),
            "Rain streak budget scales monotonically with cloud quality");

        var world = WorldGenerator.Generate();
        var camera = new Vector3(0f, 4f, 0f);

        world.Weather.SetCondition(WeatherKind.Clear, true);
        TerrainVertex[] clearVertices = [];
        uint[] clearIndices = [];
        RainEffectMesh.Append(
            world,
            camera,
            1f,
            CloudQuality.Ultra,
            ref clearVertices,
            ref clearIndices);

        check(clearVertices.Length == 0 && clearIndices.Length == 0,
            "Clear weather emits no rain geometry");

        world.Weather.SetCondition(WeatherKind.Rain, true);
        TerrainVertex[] rainVerticesA = [];
        uint[] rainIndicesA = [];
        RainEffectMesh.Append(
            world,
            camera,
            1f,
            CloudQuality.High,
            ref rainVerticesA,
            ref rainIndicesA);

        check(
            rainVerticesA.Length > 0 &&
            rainIndicesA.Length > 0 &&
            rainVerticesA.Length <=
                GraphicsQualityCatalog.RainStreakCount(CloudQuality.High) * 4 &&
            rainIndicesA.All(index => index < rainVerticesA.Length),
            "Rain generates a bounded valid camera-local streak mesh");

        TerrainVertex[] rainVerticesB = [];
        uint[] rainIndicesB = [];
        RainEffectMesh.Append(
            world,
            camera,
            1.35f,
            CloudQuality.High,
            ref rainVerticesB,
            ref rainIndicesB);

        check(
            rainVerticesA.Length == rainVerticesB.Length &&
            rainVerticesA.Zip(rainVerticesB).Any(pair =>
                Vector3.DistanceSquared(
                    pair.First.Position,
                    pair.Second.Position) > 0.000001f),
            "Rain streak positions animate over time");

        check(
            WeatherVisuals.LightningFlash(world.Weather, 0.045) == 0f,
            "Rain without Storm never produces lightning");

        world.Weather.SetCondition(WeatherKind.Storm, true);
        var firstFlash = WeatherVisuals.LightningFlash(world.Weather, 0.045);
        var betweenFlashes = WeatherVisuals.LightningFlash(world.Weather, 3.0);

        check(firstFlash > 0.9f && betweenFlashes == 0f,
            "Storm produces a short deterministic lightning pulse instead of constant flashing");

        TerrainVertex[] stormVertices = [];
        uint[] stormIndices = [];
        RainEffectMesh.Append(
            world,
            camera,
            0.5f,
            CloudQuality.Low,
            ref stormVertices,
            ref stormIndices);

        check(
            stormVertices.Length <=
                GraphicsQualityCatalog.RainStreakCount(CloudQuality.Low) * 4 &&
            stormVertices.Length > 0,
            "Low quality keeps storm precipitation inside the low-end streak budget");
    }
}
