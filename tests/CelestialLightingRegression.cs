using System.Numerics;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.World;

internal static class CelestialLightingRegression
{
    public static void Run(Action<bool, string> check)
    {
        var time = new WorldTime();
        var weather = new WeatherSystem(42);
        weather.SetCondition(WeatherKind.Clear, true);

        time.SetTimeOfDay(7.0);
        var sunrise = CelestialLighting.Evaluate(time, weather);

        time.SetTimeOfDay(13.0);
        var noon = CelestialLighting.Evaluate(time, weather);

        time.SetTimeOfDay(19.0);
        var sunset = CelestialLighting.Evaluate(time, weather);

        time.SetTimeOfDay(23.0);
        var night = CelestialLighting.Evaluate(time, weather);

        check(MathF.Abs(sunrise.SunDirection.Length() - 1f) < 0.001f,
            "Sunrise direction is normalized");
        check(MathF.Abs(noon.SunDirection.Length() - 1f) < 0.001f,
            "Noon direction is normalized");
        check(Vector3.DistanceSquared(sunrise.SunDirection, sunset.SunDirection) > 0.25f,
            "Sun moves across the sky between sunrise and sunset");
        check(noon.SunDirection.Y > sunrise.SunDirection.Y &&
              noon.SunDirection.Y > sunset.SunDirection.Y,
            "Sun reaches a higher elevation around midday");
        check(noon.SunIntensity > sunrise.SunIntensity &&
              noon.SunIntensity > sunset.SunIntensity,
            "Midday sunlight is stronger than twilight");
        check(night.SunIntensity < sunrise.SunIntensity,
            "Night sunlight is weaker than sunrise");
        check(sunrise.TwilightFactor > noon.TwilightFactor &&
              sunset.TwilightFactor > noon.TwilightFactor,
            "Twilight factor rises near the horizon");

        time.SetTimeOfDay(13.0);
        weather.SetCondition(WeatherKind.Clear, true);
        var clearNoon = CelestialLighting.Evaluate(time, weather);
        weather.SetCondition(WeatherKind.Overcast, true);
        var overcastNoon = CelestialLighting.Evaluate(time, weather);

        check(overcastNoon.SunIntensity < clearNoon.SunIntensity,
            "Cloud cover attenuates direct sunlight");
        check(clearNoon.SunColor.X >= clearNoon.SunColor.Z,
            "Daylight spectrum remains physically warmer than blue-biased night light");

        check(sunrise.SunColor.Z < noon.SunColor.Z &&
              sunset.SunColor.Z < noon.SunColor.Z,
            "Sunlight is visibly warmer near sunrise and sunset than at noon");
        check(sunset.SunColor.Y < sunrise.SunColor.Y,
            "Evening sunlight is redder than morning sunlight");
        check(MathF.Abs(night.MoonDirection.Length() - 1f) < 0.001f,
            "Moon direction is normalized");
        check(night.MoonDirection.Y > 0f,
            "Moon rises above the horizon while the sun is below it");
        check(night.NightFactor > noon.NightFactor,
            "Night factor rises after sunset");
        check(night.MoonIntensity > 0f,
            "Clear night produces visible moonlight");

        weather.SetCondition(WeatherKind.Storm, true);
        var stormNight = CelestialLighting.Evaluate(time, weather);
        check(stormNight.MoonIntensity < night.MoonIntensity,
            "Heavy cloud cover attenuates moon visibility");
    }
}
