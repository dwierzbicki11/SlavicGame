using SlavicGame.Engine.Inventory;

namespace SlavicGame.Engine.Interaction;

public sealed record LootStack(string ItemId, int Quantity);

public sealed record LootContainerDefinition(
    string TargetId,
    IReadOnlyList<LootStack> Contents,
    string? QuestEventId = null);

public sealed record LootContainerSnapshot(
    string TargetId,
    LootStack[] RemainingContents,
    bool CompletionEventEmitted = false);

public enum LootContainerResult
{
    Looted,
    Stored,
    Empty,
    InvalidTarget,
    ItemNotFound,
    InvalidQuantity
}

/// <summary>
/// Owns durable per-container loot state. The definition describes initial loot;
/// after first access the remaining contents are owned by this state and can be persisted.
/// </summary>
public sealed class LootContainerInteractionState
{
    private readonly Dictionary<string, Dictionary<string, int>> _remaining = new(StringComparer.Ordinal);
    private readonly HashSet<string> _completedQuestEvents = new(StringComparer.Ordinal);

    public LootContainerResult LootAll(
        InteractionTarget target,
        LootContainerDefinition container,
        InventoryState inventory,
        Action<string>? questEvent = null)
    {
        if (!TryValidate(target, container, inventory))
            return LootContainerResult.InvalidTarget;

        var remaining = GetOrCreate(container);
        if (remaining.Count == 0)
            return LootContainerResult.Empty;

        // Preflight every destination before mutating either side.
        foreach (var pair in remaining)
            _ = checked(inventory.Count(pair.Key) + pair.Value);
        foreach (var pair in remaining)
            inventory.Add(pair.Key, pair.Value);

        remaining.Clear();
        EmitCompletedQuestEventOnce(container, questEvent);
        return LootContainerResult.Looted;
    }

    public LootContainerResult Take(
        InteractionTarget target,
        LootContainerDefinition container,
        InventoryState inventory,
        string itemId,
        int quantity,
        Action<string>? questEvent = null)
    {
        if (!TryValidate(target, container, inventory))
            return LootContainerResult.InvalidTarget;
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
            return LootContainerResult.InvalidQuantity;

        var remaining = GetOrCreate(container);
        if (remaining.Count == 0)
            return LootContainerResult.Empty;
        if (!remaining.TryGetValue(itemId, out var available))
            return LootContainerResult.ItemNotFound;
        if (quantity > available)
            return LootContainerResult.InvalidQuantity;

        inventory.Add(itemId, quantity);
        var left = available - quantity;
        if (left == 0)
            remaining.Remove(itemId);
        else
            remaining[itemId] = left;

        if (remaining.Count == 0)
            EmitCompletedQuestEventOnce(container, questEvent);

        return LootContainerResult.Looted;
    }

    public LootContainerResult Store(
        InteractionTarget target,
        LootContainerDefinition container,
        InventoryState inventory,
        string itemId,
        int quantity)
    {
        if (!TryValidate(target, container, inventory))
            return LootContainerResult.InvalidTarget;
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0 || inventory.Count(itemId) < quantity)
            return LootContainerResult.InvalidQuantity;

        var remaining = GetOrCreate(container);
        if (!inventory.Remove(itemId, quantity))
            return LootContainerResult.InvalidQuantity;

        try
        {
            remaining[itemId] = checked(remaining.GetValueOrDefault(itemId) + quantity);
        }
        catch
        {
            inventory.Add(itemId, quantity);
            throw;
        }

        return LootContainerResult.Stored;
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
                    .ToArray(),
                _completedQuestEvents.Contains(pair.Key)))
            .ToArray();

    public void Restore(IEnumerable<LootContainerSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _remaining.Clear();
        _completedQuestEvents.Clear();

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
            if (snapshot.CompletionEventEmitted)
                _completedQuestEvents.Add(snapshot.TargetId);
        }
    }

    private static bool TryValidate(
        InteractionTarget target,
        LootContainerDefinition container,
        InventoryState inventory)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(inventory);

        return target.Enabled &&
               !string.IsNullOrWhiteSpace(container.TargetId) &&
               StringComparer.Ordinal.Equals(target.Id, container.TargetId) &&
               container.Contents is not null;
    }

    private void EmitCompletedQuestEventOnce(
        LootContainerDefinition container,
        Action<string>? questEvent)
    {
        if (questEvent is null || string.IsNullOrWhiteSpace(container.QuestEventId) ||
            !_completedQuestEvents.Add(container.TargetId))
            return;

        questEvent(container.QuestEventId);
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
