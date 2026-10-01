namespace SlavicGame.Engine.Reputation;

public enum ReputationScope
{
    Npc,
    Village,
    Region,
    Faction,
    Culture,
    ReligiousInstitution
}

public readonly record struct ReputationKey(ReputationScope Scope, string Id);

public sealed record ReputationEntry(ReputationScope Scope, string Id, int Value);

public sealed class ReputationSystem
{
    private readonly Dictionary<ReputationKey, int> _values = [];

    public int Get(ReputationScope scope, string id) =>
        _values.GetValueOrDefault(new ReputationKey(scope, id));

    public int Change(ReputationScope scope, string id, int delta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var key = new ReputationKey(scope, id);
        var value = Math.Clamp(Get(scope, id) + delta, -100, 100);
        if (value == 0) _values.Remove(key);
        else _values[key] = value;
        return value;
    }

    public ReputationEntry[] Capture() =>
        _values.Select(pair => new ReputationEntry(pair.Key.Scope, pair.Key.Id, pair.Value))
            .OrderBy(entry => entry.Scope)
            .ThenBy(entry => entry.Id, StringComparer.Ordinal)
            .ToArray();

    public void Restore(IEnumerable<ReputationEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _values.Clear();
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Value is < -100 or > 100)
            {
                throw new ArgumentException("Reputation snapshot contains an invalid entry.", nameof(entries));
            }
            if (entry.Value != 0)
            {
                _values.Add(new ReputationKey(entry.Scope, entry.Id), entry.Value);
            }
        }
    }
}
