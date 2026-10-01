namespace SlavicGame.Engine.Relationships;

public enum RelationshipKind
{
    Companion,
    Friendship,
    Trust,
    Romance,
    Rivalry
}

public sealed record RelationshipSnapshot(
    string CharacterId,
    RelationshipKind Kind,
    int Value);

public sealed class RelationshipSystem
{
    private readonly Dictionary<(string CharacterId, RelationshipKind Kind), int> _values = [];

    public int Get(string characterId, RelationshipKind kind) =>
        _values.GetValueOrDefault((characterId, kind));

    public int Change(string characterId, RelationshipKind kind, int delta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        var key = (characterId, kind);
        var value = Math.Clamp(Get(characterId, kind) + delta, -100, 100);
        if (value == 0) _values.Remove(key);
        else _values[key] = value;
        return value;
    }

    public RelationshipSnapshot[] Capture() =>
        _values.Select(pair => new RelationshipSnapshot(
            pair.Key.CharacterId,
            pair.Key.Kind,
            pair.Value)).ToArray();

    public void Restore(IEnumerable<RelationshipSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _values.Clear();
        foreach (var snapshot in snapshots)
        {
            if (string.IsNullOrWhiteSpace(snapshot.CharacterId) ||
                snapshot.Value is < -100 or > 100)
            {
                throw new ArgumentException("Relationship snapshot contains invalid data.", nameof(snapshots));
            }

            if (snapshot.Value != 0)
            {
                _values.Add((snapshot.CharacterId, snapshot.Kind), snapshot.Value);
            }
        }
    }
}
