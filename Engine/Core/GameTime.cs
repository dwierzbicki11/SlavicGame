namespace SlavicGame.Engine.Core;

public sealed class GameTime
{
    public double DeltaSeconds { get; private set; }
    public double TotalSeconds { get; private set; }
    public long FrameCount { get; private set; }

    internal void Advance(double deltaSeconds)
    {
        DeltaSeconds = Math.Clamp(deltaSeconds, 0.0, 0.25);
        TotalSeconds += DeltaSeconds;
        FrameCount++;
    }
}
