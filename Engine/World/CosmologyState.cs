namespace SlavicGame.Engine.World;

public enum ExistenceSphere
{
    Jawia,
    Nawia,
    DivineRealm,
    FourthSphere
}

public enum BoundaryState
{
    Stable,
    Echo,
    Leaking,
    Passage
}

public sealed record BoundaryPhenomenonSnapshot(
    string Id,
    string RegionId,
    BoundaryState State,
    double ActiveFromHour,
    double ActiveUntilHour,
    bool Resolved);

public sealed class BoundaryPhenomenon
{
    public string Id { get; }
    public string RegionId { get; }
    public BoundaryState State { get; private set; }
    public double ActiveFromHour { get; }
    public double ActiveUntilHour { get; }
    public bool Resolved { get; private set; }

    public BoundaryPhenomenon(
        string id,
        string regionId,
        BoundaryState state,
        double activeFromHour,
        double activeUntilHour)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(regionId);
        Id = id;
        RegionId = regionId;
        State = state;
        ActiveFromHour = activeFromHour;
        ActiveUntilHour = activeUntilHour;
    }

    public bool IsActive(double hour)
    {
        if (Resolved || !double.IsFinite(hour) || State == BoundaryState.Stable)
        {
            return false;
        }

        hour = ((hour % 24.0) + 24.0) % 24.0;
        var start = ((ActiveFromHour % 24.0) + 24.0) % 24.0;
        var end = ((ActiveUntilHour % 24.0) + 24.0) % 24.0;
        return start <= end
            ? hour >= start && hour <= end
            : hour >= start || hour <= end;
    }

    public void Resolve() => Resolved = true;

    public BoundaryPhenomenonSnapshot Capture() =>
        new(Id, RegionId, State, ActiveFromHour, ActiveUntilHour, Resolved);

    public static BoundaryPhenomenon Restore(BoundaryPhenomenonSnapshot snapshot)
    {
        var phenomenon = new BoundaryPhenomenon(
            snapshot.Id,
            snapshot.RegionId,
            snapshot.State,
            snapshot.ActiveFromHour,
            snapshot.ActiveUntilHour);
        if (snapshot.Resolved) phenomenon.Resolve();
        return phenomenon;
    }
}

public sealed class CosmologyState
{
    private readonly Dictionary<string, BoundaryPhenomenon> _phenomena = new(StringComparer.Ordinal);

    public IReadOnlyCollection<BoundaryPhenomenon> Phenomena => _phenomena.Values;

    public void Add(BoundaryPhenomenon phenomenon)
    {
        ArgumentNullException.ThrowIfNull(phenomenon);
        if (!_phenomena.TryAdd(phenomenon.Id, phenomenon))
        {
            throw new InvalidOperationException($"Boundary phenomenon '{phenomenon.Id}' already exists.");
        }
    }

    public BoundaryPhenomenon? Find(string id) => _phenomena.GetValueOrDefault(id);

    public void Clear() => _phenomena.Clear();

    public BoundaryPhenomenonSnapshot[] Capture() =>
        _phenomena.Values.Select(p => p.Capture()).OrderBy(p => p.Id).ToArray();

    public void Restore(IEnumerable<BoundaryPhenomenonSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _phenomena.Clear();
        foreach (var snapshot in snapshots)
        {
            var phenomenon = BoundaryPhenomenon.Restore(snapshot);
            _phenomena.Add(phenomenon.Id, phenomenon);
        }
    }
}
