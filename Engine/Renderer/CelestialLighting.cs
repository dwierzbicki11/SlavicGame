using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public readonly record struct CelestialLightState(
    Vector3 SunDirection,
    float SunIntensity,
    Vector3 SunColor,
    float TwilightFactor,
    Vector3 MoonDirection,
    float MoonIntensity,
    float NightFactor,
    float LunarPhase);

public static class CelestialLighting
{
    private const double SunriseHour = 6.0;
    private const double SunsetHour = 20.0;
    private const float MaxSolarElevationRadians = 1.10f;
    private const float LunarPhase = 0.82f;

    public static CelestialLightState Evaluate(WorldTime time, WeatherSystem weather)
    {
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(weather);

        var hour = (float)time.TimeOfDayHours;
        var daylight = hour >= SunriseHour && hour < SunsetHour;
        var solarArc = CalculateSolarArc(hour);

        var elevation = MathF.Sin(solarArc) *
            (daylight ? MaxSolarElevationRadians : MaxSolarElevationRadians * 0.72f);

        // The azimuth keeps moving continuously through the night instead of
        // snapping the sun back to the sunrise side of the sky.
        var azimuth = MathF.PI * 0.15f + solarArc;
        var cosElevation = MathF.Cos(elevation);
        var sunDirection = Vector3.Normalize(new Vector3(
            MathF.Cos(azimuth) * cosElevation,
            MathF.Sin(elevation),
            MathF.Sin(azimuth) * cosElevation));

        var elevation01 = daylight
            ? Math.Clamp(
                sunDirection.Y / MathF.Sin(MaxSolarElevationRadians),
                0f,
                1f)
            : 0f;

        var twilight = daylight
            ? 1f - SmoothStep(0.03f, 0.42f, elevation01)
            : SmoothStep(-0.24f, 0.04f, sunDirection.Y);

        var clearIntensity = daylight
            ? 0.13f + 1.17f * MathF.Pow(elevation01, 0.70f)
            : 0.0f;

        var cloudAttenuation = 1f - weather.Cloudiness * 0.58f;
        var rainAttenuation = 1f - weather.RainIntensity * 0.14f;
        var sunIntensity = Math.Clamp(
            clearIntensity * cloudAttenuation * rainAttenuation,
            0f,
            1.30f);

        var noonColor = new Vector3(1.00f, 0.965f, 0.89f);
        var morningColor = new Vector3(1.00f, 0.58f, 0.29f);
        var eveningColor = new Vector3(1.00f, 0.39f, 0.14f);
        var warmColor = hour < 13f ? morningColor : eveningColor;
        var horizonWarmth = 1f - SmoothStep(0.02f, 0.58f, elevation01);
        var sunColor = daylight
            ? Vector3.Lerp(noonColor, warmColor, horizonWarmth * 0.94f)
            : new Vector3(0.10f, 0.12f, 0.18f);

        sunColor = Vector3.Lerp(
            sunColor,
            Vector3.One * Vector3.Dot(
                sunColor,
                new Vector3(0.2126f, 0.7152f, 0.0722f)),
            weather.Cloudiness * 0.26f);

        var moonDirection = Vector3.Normalize(-sunDirection);
        var nightFactor = 1f - SmoothStep(-0.08f, 0.12f, sunDirection.Y);
        var moonIntensity = Math.Clamp(
            nightFactor *
            (1f - weather.Cloudiness * 0.66f) *
            (1f - weather.RainIntensity * 0.22f) *
            0.36f,
            0f,
            0.36f);

        return new CelestialLightState(
            sunDirection,
            sunIntensity,
            sunColor,
            twilight,
            moonDirection,
            moonIntensity,
            nightFactor,
            LunarPhase);
    }

    private static float CalculateSolarArc(float hour)
    {
        if (hour >= SunriseHour && hour < SunsetHour)
        {
            var dayProgress =
                (hour - (float)SunriseHour) /
                (float)(SunsetHour - SunriseHour);
            return dayProgress * MathF.PI;
        }

        var nightElapsed = hour >= SunsetHour
            ? hour - (float)SunsetHour
            : hour + (24f - (float)SunsetHour);
        var nightLength = 24f - (float)SunsetHour + (float)SunriseHour;
        return MathF.PI + nightElapsed / nightLength * MathF.PI;
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var x = Math.Clamp(
            (value - edge0) / MathF.Max(edge1 - edge0, 0.0001f),
            0f,
            1f);
        return x * x * (3f - 2f * x);
    }
}
