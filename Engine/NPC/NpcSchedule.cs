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
        if (!double.IsFinite(hour))
        {
            return false;
        }

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
