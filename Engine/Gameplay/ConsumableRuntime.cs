namespace SlavicGame.Engine.Gameplay;

public enum ConsumableUseResult
{
    Completed,
    MissingItem,
    AlreadyActive
}

public sealed record ConsumableSnapshot(
    double TrackingTonicRemaining);

public sealed class ConsumableRuntime
{
    public const string TrackingTonicItemId = "marsh-sight-tonic";
    public const double TrackingTonicDurationSeconds = 90.0;

    public double TrackingTonicRemaining { get; private set; }
    public bool TrackingTonicActive =>
        TrackingTonicRemaining > 0.0;

    public float TrackVisualMultiplier =>
        TrackingTonicActive ? 1.35f : 1f;

    public float TrackRenderDistance =>
        TrackingTonicActive ? 52f : 38f;

    public float TrackInspectDistance =>
        TrackingTonicActive ? 4.8f : 3.2f;

    public string HudStatus =>
        TrackingTonicActive
            ? $"NAPAR TROPICIELA {Math.Ceiling(TrackingTonicRemaining):0}s"
            : "";

    public string Message { get; private set; } = "";

    public ConsumableUseResult TryUseTrackingTonic(
        World.WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (TrackingTonicActive)
        {
            Message = "NAPAR JUZ DZIALA";
            return ConsumableUseResult.AlreadyActive;
        }

        if (!world.Progress.Inventory.Remove(
                TrackingTonicItemId,
                1))
        {
            Message = "BRAK NAPARU TROPICIELA";
            return ConsumableUseResult.MissingItem;
        }

        TrackingTonicRemaining =
            TrackingTonicDurationSeconds;
        Message = "NAPAR TROPICIELA / WYOSTRZONE TROPY";
        return ConsumableUseResult.Completed;
    }

    public void Update(double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) ||
            deltaSeconds < 0d)
        {
            return;
        }

        TrackingTonicRemaining =
            Math.Max(
                0d,
                TrackingTonicRemaining - deltaSeconds);

        if (!TrackingTonicActive &&
            Message.StartsWith(
                "NAPAR TROPICIELA /",
                StringComparison.Ordinal))
        {
            Message = "NAPAR TROPICIELA PRZESTAL DZIALAC";
        }
    }

    public ConsumableSnapshot Capture() =>
        new(TrackingTonicRemaining);

    public void Restore(ConsumableSnapshot? snapshot)
    {
        TrackingTonicRemaining =
            snapshot is null
                ? 0d
                : Math.Max(
                    0d,
                    Math.Min(
                        TrackingTonicDurationSeconds,
                        snapshot.TrackingTonicRemaining));
        Message = "";
    }
}
