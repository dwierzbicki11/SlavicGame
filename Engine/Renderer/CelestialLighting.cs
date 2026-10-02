using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public readonly record struct CelestialLightState(
    Vector3 SunDirection,
    float SunIntensity,
    Vector3 SunColor,
    float TwilightFactor);

public static class CelestialLighting
{
    private const double SunriseHour = 6.0;
    private const double SunsetHour = 20.0;
    private const float MaxSolarElevationRadians = 1.10f;

    public static CelestialLightState Evaluate(WorldTime time, WeatherSystem weather)
    {
        ArgumentNullException.ThrowIfNull(time);
        ArgumentNullException.ThrowIfNull(weather);

        var hour = (float)time.TimeOfDayHours;
        var daylightPhase = Math.Clamp(
            (hour - (float)SunriseHour) / (float)(SunsetHour - SunriseHour),
            0f,
            1f);

        var daylight = hour >= SunriseHour && hour < SunsetHour;
        var elevation = daylight
            ? MathF.Sin(daylightPhase * MathF.PI) * MaxSolarElevationRadians
            : -0.22f;

        var azimuth = MathF.Lerp(
            MathF.PI * 0.15f,
            MathF.PI * 1.15f,
            daylightPhase);

        var cosElevation = MathF.Cos(elevation);
        var direction = Vector3.Normalize(new Vector3(
            MathF.Cos(azimuth) * cosElevation,
            MathF.Sin(elevation),
            MathF.Sin(azimuth) * cosElevation));

        var elevation01 = daylight
            ? Math.Clamp(direction.Y / MathF.Sin(MaxSolarElevationRadians), 0f, 1f)
            : 0f;

        var twilight = daylight
            ? 1f - SmoothStep(0.04f, 0.34f, elevation01)
            : 0f;

        var clearIntensity = daylight
            ? 0.16f + 1.14f * MathF.Pow(elevation01, 0.72f)
            : 0.035f;

        var cloudAttenuation = 1f - weather.Cloudiness * 0.58f;
        var rainAttenuation = 1f - weather.RainIntensity * 0.12f;
        var intensity = Math.Clamp(
            clearIntensity * cloudAttenuation * rainAttenuation,
            daylight ? 0.08f : 0.018f,
            1.30f);

        var warm = new Vector3(1.00f, 0.49f, 0.24f);
        var daylightColor = new Vector3(1.00f, 0.94f, 0.84f);
        var nightColor = new Vector3(0.24f, 0.31f, 0.46f);
        var color = daylight
            ? Vector3.Lerp(daylightColor, warm, twilight * 0.82f)
            : nightColor;

        color = Vector3.Lerp(
            color,
            Vector3.One * Vector3.Dot(color, new Vector3(0.2126f, 0.7152f, 0.0722f)),
            weather.Cloudiness * 0.34f);

        return new CelestialLightState(direction, intensity, color, twilight);
    }

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var x = Math.Clamp((value - edge0) / MathF.Max(edge1 - edge0, 0.0001f), 0f, 1f);
        return x * x * (3f - 2f * x);
    }
}
