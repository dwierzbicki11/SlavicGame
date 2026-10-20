namespace SlavicGame.Engine.NPC;

public enum NpcRole
{
    ContractGiver,
    CrossingKeeper,
    Herbalist,
    CommunityGuard,
    ShrineKeeper,
    Worker,
    Trader,
    Traveler,
    Other
}

public sealed record NpcScheduleSlot(
    double StartHour,
    double EndHour,
    string LocationId,
    string Activity)
{
    public bool Contains(double hour)
    {
        if (!double.IsFinite(hour) ||
            !double.IsFinite(StartHour) ||
            !double.IsFinite(EndHour))
        {
            return false;
        }

        // Preserve explicit full-day ranges before modulo turns 0–24 into 0–0.
        // Equal endpoints still describe an empty slot.
        if (EndHour - StartHour >= 24.0)
            return true;

        hour = ((hour % 24.0) + 24.0) % 24.0;
        var start = ((StartHour % 24.0) + 24.0) % 24.0;
        var end = ((EndHour % 24.0) + 24.0) % 24.0;
        return start <= end
            ? hour >= start && hour < end
            : hour >= start || hour < end;
    }
}

public sealed class NpcDefinition
{
    private readonly List<NpcScheduleSlot> _schedule = [];

    public string Id { get; }
    public NpcRole Role { get; }
    public IReadOnlyList<NpcScheduleSlot> Schedule => _schedule;

    public NpcDefinition(string id, NpcRole role, IEnumerable<NpcScheduleSlot>? schedule = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
        Role = role;
        if (schedule is not null) _schedule.AddRange(schedule);
    }

    public NpcScheduleSlot? GetSchedule(double hour) =>
        _schedule.FirstOrDefault(slot => slot.Contains(hour));
}
