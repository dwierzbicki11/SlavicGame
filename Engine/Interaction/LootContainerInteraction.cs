using SlavicGame.Engine.Inventory;

namespace SlavicGame.Engine.Interaction;

public sealed record LootStack(string ItemId, int Quantity);

public sealed record LootContainerDefinition(
    string TargetId,
    IReadOnlyList<LootStack> Contents,
    string? QuestEventId = null);

public sealed record LootContainerSnapshot(
    string TargetId,
    LootStack[] RemainingContents);

public enum LootContainerResult
{
    Looted,
    Empty,
    InvalidTarget
}

/// <summary>
/// Owns durable per-container loot state. The definition describes initial loot;
/// after first access the remaining contents are owned by this state and can be persisted.
/// </summary>
public sealed class LootContainerInteractionState
{
    private readonly Dictionary<string, Dictionary<string, int>> _remaining = new(StringComparer.Ordinal);

    public LootContainerResult LootAll(
        InteractionTarget target,
        LootContainerDefinition container,
        InventoryState inventory,
        Action<string>? questEvent = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(inventory);

        if (!target.Enabled ||
            string.IsNullOrWhiteSpace(container.TargetId) ||
            !StringComparer.Ordinal.Equals(target.Id, container.TargetId) ||
            container.Contents is null)
        {
            return LootContainerResult.InvalidTarget;
        }

        var remaining = GetOrCreate(container);
        if (remaining.Count == 0)
            return LootContainerResult.Empty;

        foreach (var pair in remaining)
            inventory.Add(pair.Key, pair.Value);

        remaining.Clear();

        if (!string.IsNullOrWhiteSpace(container.QuestEventId))
            questEvent?.Invoke(container.QuestEventId);

        return LootContainerResult.Looted;
    }

    public IReadOnlyList<LootStack> Remaining(LootContainerDefinition container)
    {
        ArgumentNullException.ThrowIfNull(container);
        return GetOrCreate(container)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new LootStack(pair.Key, pair.Value))
            .ToArray();
    }

    public LootContainerSnapshot[] Capture() =>
        _remaining
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new LootContainerSnapshot(
                pair.Key,
                pair.Value.OrderBy(item => item.Key, StringComparer.Ordinal)
                    .Select(item => new LootStack(item.Key, item.Value))
                    .ToArray()))
            .ToArray();

    public void Restore(IEnumerable<LootContainerSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _remaining.Clear();

        foreach (var snapshot in snapshots)
        {
            if (string.IsNullOrWhiteSpace(snapshot.TargetId) || snapshot.RemainingContents is null ||
                _remaining.ContainsKey(snapshot.TargetId))
                throw new ArgumentException("Loot container snapshot is invalid or duplicated.", nameof(snapshots));

            var contents = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var stack in snapshot.RemainingContents)
            {
                if (string.IsNullOrWhiteSpace(stack.ItemId) || stack.Quantity <= 0 ||
                    !contents.TryAdd(stack.ItemId, stack.Quantity))
                    throw new ArgumentException("Loot container snapshot contains an invalid stack.", nameof(snapshots));
            }

            _remaining.Add(snapshot.TargetId, contents);
        }
    }

    private Dictionary<string, int> GetOrCreate(LootContainerDefinition container)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(container.TargetId);
        if (_remaining.TryGetValue(container.TargetId, out var existing))
            return existing;

        var contents = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var stack in container.Contents)
        {
            if (string.IsNullOrWhiteSpace(stack.ItemId) || stack.Quantity <= 0)
                throw new ArgumentException("Loot container definition contains an invalid stack.", nameof(container));

            contents[stack.ItemId] = checked(contents.GetValueOrDefault(stack.ItemId) + stack.Quantity);
        }

        _remaining.Add(container.TargetId, contents);
        return contents;
    }
}
