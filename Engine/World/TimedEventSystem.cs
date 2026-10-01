namespace SlavicGame.Engine.World;

public enum TimedEventPriority
{
    Ambient = 0,
    RegionalEncounter = 1,
    NpcScheduleOverride = 2,
    UniqueStory = 3,
    CriticalQuest = 4
}

public enum TimedEventRepeatPolicy
{
    Once,
    RepeatNextCycle
}

public sealed record TimedEventDefinition(
    string Id,
    double StartHour,
    double EndHour,
    TimedEventPriority Priority,
    string? ConflictGroup = null,
    TimedEventRepeatPolicy RepeatPolicy = TimedEventRepeatPolicy.Once)
{
    public bool IsInWindow(double hour)
    {
        if (!double.IsFinite(hour)) return false;
        hour = ((hour % 24.0) + 24.0) % 24.0;
        var start = ((StartHour % 24.0) + 24.0) % 24.0;
        var end = ((EndHour % 24.0) + 24.0) % 24.0;
        return start <= end ? hour >= start && hour < end : hour >= start || hour < end;
    }

    public TimedEventDefinition Validate()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentException("Event id is required.", nameof(Id));
        if (!double.IsFinite(StartHour) || !double.IsFinite(EndHour))
            throw new ArgumentOutOfRangeException(nameof(StartHour));
        return this;
    }
}

public sealed record TimedEventSnapshot(IReadOnlyCollection<string> FiredOnce, IReadOnlyDictionary<string, int> LastFiredDay);

public sealed class TimedEventSystem
{
    private readonly Dictionary<string, TimedEventDefinition> _definitions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _firedOnce = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _lastFiredDay = new(StringComparer.Ordinal);

    public void Register(TimedEventDefinition definition)
    {
        definition.Validate();
        if (!_definitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Timed event '{definition.Id}' is already registered.");
    }

    public IReadOnlyList<TimedEventDefinition> Evaluate(double hour, int day)
    {
        if (!double.IsFinite(hour)) throw new ArgumentOutOfRangeException(nameof(hour));
        if (day < 0) throw new ArgumentOutOfRangeException(nameof(day));

        var eligible = _definitions.Values
            .Where(definition => definition.IsInWindow(hour) && CanFire(definition, day))
            .OrderByDescending(definition => definition.Priority)
            .ThenBy(definition => definition.Id, StringComparer.Ordinal)
            .ToArray();

        var claimedGroups = new HashSet<string>(StringComparer.Ordinal);
        var fired = new List<TimedEventDefinition>();
        foreach (var definition in eligible)
        {
            if (!string.IsNullOrWhiteSpace(definition.ConflictGroup) && !claimedGroups.Add(definition.ConflictGroup))
                continue;

            fired.Add(definition);
            _lastFiredDay[definition.Id] = day;
            if (definition.RepeatPolicy == TimedEventRepeatPolicy.Once)
                _firedOnce.Add(definition.Id);
        }
        return fired;
    }

    public TimedEventSnapshot Capture() => new(_firedOnce.ToArray(), new Dictionary<string, int>(_lastFiredDay));

    public void Restore(TimedEventSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _firedOnce.Clear();
        _lastFiredDay.Clear();
        foreach (var id in snapshot.FiredOnce) _firedOnce.Add(id);
        foreach (var pair in snapshot.LastFiredDay) _lastFiredDay[pair.Key] = pair.Value;
    }

    private bool CanFire(TimedEventDefinition definition, int day)
    {
        if (definition.RepeatPolicy == TimedEventRepeatPolicy.Once)
            return !_firedOnce.Contains(definition.Id);
        return !_lastFiredDay.TryGetValue(definition.Id, out var lastDay) || lastDay < day;
    }
}
