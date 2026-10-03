namespace SlavicGame.Engine.Gameplay;

public enum EncounterAwareness
{
    Unseen,
    Observed,
    Identified,
    Resolved
}

public enum EncounterResolution
{
    None,
    Killed,
    DrivenOff,
    Appeased,
    Contained,
    Helped,
    AvoidedPersistently
}

public sealed record EncounterSnapshot(
    string Id,
    EncounterAwareness Awareness,
    EncounterResolution Resolution);

public sealed class EncounterRecord
{
    public string Id { get; }
    public EncounterAwareness Awareness { get; private set; }
    public EncounterResolution Resolution { get; private set; }

    public EncounterRecord(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public bool Observe()
    {
        if (Awareness >= EncounterAwareness.Observed)
            return false;
        Awareness = EncounterAwareness.Observed;
        return true;
    }

    public bool Identify()
    {
        if (Awareness >= EncounterAwareness.Identified)
            return false;
        Awareness = EncounterAwareness.Identified;
        return true;
    }

    public bool Resolve(EncounterResolution resolution)
    {
        if (resolution == EncounterResolution.None)
            throw new ArgumentOutOfRangeException(nameof(resolution));
        if (Awareness == EncounterAwareness.Resolved)
            return false;

        Awareness = EncounterAwareness.Resolved;
        Resolution = resolution;
        return true;
    }

    internal EncounterSnapshot Capture() =>
        new(Id, Awareness, Resolution);

    internal void Restore(EncounterSnapshot snapshot)
    {
        if (!string.Equals(snapshot.Id, Id, StringComparison.Ordinal))
            throw new ArgumentException("Encounter snapshot ID mismatch.", nameof(snapshot));
        if (!Enum.IsDefined(snapshot.Awareness) || !Enum.IsDefined(snapshot.Resolution))
            throw new ArgumentException("Encounter snapshot contains an invalid enum value.", nameof(snapshot));
        if (snapshot.Awareness == EncounterAwareness.Resolved &&
            snapshot.Resolution == EncounterResolution.None)
            throw new ArgumentException("Resolved encounter must carry a resolution type.", nameof(snapshot));
        if (snapshot.Awareness != EncounterAwareness.Resolved &&
            snapshot.Resolution != EncounterResolution.None)
            throw new ArgumentException("Unresolved encounter cannot carry a resolution type.", nameof(snapshot));

        Awareness = snapshot.Awareness;
        Resolution = snapshot.Resolution;
    }
}

public sealed class EncounterJournal
{
    private readonly Dictionary<string, EncounterRecord> _encounters =
        new(StringComparer.Ordinal);

    public IReadOnlyCollection<EncounterRecord> Encounters => _encounters.Values;

    public EncounterRecord Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!_encounters.TryGetValue(id, out var record))
        {
            record = new EncounterRecord(id);
            _encounters.Add(id, record);
        }
        return record;
    }

    public EncounterSnapshot[] Capture() =>
        _encounters.Values
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .Select(item => item.Capture())
            .ToArray();

    public void Restore(IEnumerable<EncounterSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _encounters.Clear();

        foreach (var snapshot in snapshots)
        {
            if (string.IsNullOrWhiteSpace(snapshot.Id))
                continue;

            var record = new EncounterRecord(snapshot.Id);
            record.Restore(snapshot);
            _encounters.Add(record.Id, record);
        }
    }
}
