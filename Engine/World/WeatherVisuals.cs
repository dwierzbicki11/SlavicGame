namespace SlavicGame.Engine.World;

public static class WeatherVisuals
{
    private const double LightningCycleSeconds = 13.7;

    public static float LightningFlash(
        WeatherSystem weather,
        double seconds)
    {
        ArgumentNullException.ThrowIfNull(weather);

        if (weather.Condition != WeatherKind.Storm ||
            weather.RainIntensity < 0.80f ||
            !double.IsFinite(seconds))
        {
            return 0f;
        }

        var phase = (float)(seconds % LightningCycleSeconds);
        if (phase < 0f)
            phase += (float)LightningCycleSeconds;

        var first = Pulse(phase, 0.045f, 0.050f);
        var second = Pulse(phase, 0.145f, 0.065f) * 0.72f;
        return Math.Clamp(MathF.Max(first, second), 0f, 1f);
    }

    private static float Pulse(
        float time,
        float center,
        float halfWidth)
    {
        var distance = MathF.Abs(time - center);
        if (distance >= halfWidth)
            return 0f;

        var normalized = 1f - distance / halfWidth;
        return normalized * normalized;
    }
}
