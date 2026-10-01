namespace SlavicGame.Engine.Core;

public sealed class GameTime
{
    public double DeltaSeconds { get; private set; }
    public double TotalSeconds { get; private set; }
    public long FrameCount { get; private set; }

    internal void Advance(double deltaSeconds)
    {
        DeltaSeconds = double.IsFinite(deltaSeconds) ? Math.Clamp(deltaSeconds, 0.0, 0.25) : 0.0;
        TotalSeconds += DeltaSeconds;
        FrameCount++;
    }
}
