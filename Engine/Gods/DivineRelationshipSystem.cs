namespace SlavicGame.Engine.Gods;

public sealed record DivineObligation(
    string Id,
    string Description,
    string BreachCondition,
    bool Fulfilled = false,
    bool Broken = false);

public sealed record DivineRelationshipSnapshot(
    string DeityId,
    int Favor,
    bool Patron,
    DivineObligation[] Obligations);

public sealed class DivineRelationship
{
    private readonly Dictionary<string, DivineObligation> _obligations = new(StringComparer.Ordinal);

    public string DeityId { get; }
    public int Favor { get; private set; }
    public bool IsPatron { get; private set; }
    public IReadOnlyCollection<DivineObligation> Obligations => _obligations.Values;

    public DivineRelationship(string deityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deityId);
        DeityId = deityId;
    }

    public void ChangeFavor(int delta) => Favor = Math.Clamp(Favor + delta, -100, 100);

    public void SetPatron(bool value) => IsPatron = value;

    public void AddObligation(DivineObligation obligation)
    {
        ArgumentNullException.ThrowIfNull(obligation);
        ArgumentException.ThrowIfNullOrWhiteSpace(obligation.Id);
        if (!_obligations.TryAdd(obligation.Id, obligation))
        {
            throw new InvalidOperationException($"Obligation '{obligation.Id}' already exists for '{DeityId}'.");
        }
    }

    public DivineRelationshipSnapshot Capture() =>
        new(DeityId, Favor, IsPatron, _obligations.Values.OrderBy(o => o.Id).ToArray());

    public void Restore(DivineRelationshipSnapshot snapshot)
    {
        if (!string.Equals(snapshot.DeityId, DeityId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Snapshot deity does not match relationship.", nameof(snapshot));
        }

        Favor = Math.Clamp(snapshot.Favor, -100, 100);
        IsPatron = snapshot.Patron;
        _obligations.Clear();
        foreach (var obligation in snapshot.Obligations ?? [])
        {
            _obligations.Add(obligation.Id, obligation);
        }
    }
}

public sealed class DivineRelationshipSystem
{
    private readonly Dictionary<string, DivineRelationship> _relationships = new(StringComparer.Ordinal);

    public IReadOnlyCollection<DivineRelationship> Relationships => _relationships.Values;

    public DivineRelationship Get(string deityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deityId);
        if (!_relationships.TryGetValue(deityId, out var relationship))
        {
            relationship = new DivineRelationship(deityId);
            _relationships.Add(deityId, relationship);
        }

        return relationship;
    }

    public DivineRelationshipSnapshot[] Capture() =>
        _relationships.Values.Select(r => r.Capture()).OrderBy(r => r.DeityId).ToArray();

    public void Restore(IEnumerable<DivineRelationshipSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _relationships.Clear();
        foreach (var snapshot in snapshots)
        {
            var relationship = new DivineRelationship(snapshot.DeityId);
            relationship.Restore(snapshot);
            _relationships.Add(snapshot.DeityId, relationship);
        }
    }
}
