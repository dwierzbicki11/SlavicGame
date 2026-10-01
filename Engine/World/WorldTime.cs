namespace SlavicGame.Engine.World;

public sealed class WorldTime
{
    public double TimeOfDayHours { get; private set; } = 8.0;
    public double DayLengthSeconds { get; set; } = 900.0;

    public bool IsNight => TimeOfDayHours < 6.0 || TimeOfDayHours >= 20.0;
    public bool IsDay => !IsNight;

    public void Update(double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0 ||
            !double.IsFinite(DayLengthSeconds) || DayLengthSeconds <= 0)
        {
            return;
        }

        TimeOfDayHours = (TimeOfDayHours + (deltaSeconds % DayLengthSeconds) / DayLengthSeconds * 24.0) % 24.0;
    }
}
