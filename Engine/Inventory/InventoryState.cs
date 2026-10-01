namespace SlavicGame.Engine.Inventory;

public enum ItemCategory
{
    Currency,
    Quest,
    Ingredient,
    Consumable,
    Weapon,
    Armor,
    Tool,
    Artifact,
    Miscellaneous
}

public sealed record ItemDefinition(
    string Id,
    string Name,
    ItemCategory Category,
    int MaxStack = 99,
    bool QuestItem = false);

public sealed record InventoryEntry(string ItemId, int Quantity);

public sealed class InventoryState
{
    private readonly Dictionary<string, int> _items = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, int> Items => _items;

    public int Count(string itemId) => _items.GetValueOrDefault(itemId);

    public void Add(string itemId, int quantity = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        _items[itemId] = checked(Count(itemId) + quantity);
    }

    public bool Remove(string itemId, int quantity = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        var current = Count(itemId);
        if (current < quantity)
        {
            return false;
        }

        var remaining = current - quantity;
        if (remaining == 0) _items.Remove(itemId);
        else _items[itemId] = remaining;
        return true;
    }

    public bool Contains(string itemId, int quantity = 1) =>
        quantity > 0 && Count(itemId) >= quantity;

    public InventoryEntry[] Capture() =>
        _items.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new InventoryEntry(pair.Key, pair.Value))
            .ToArray();

    public void Restore(IEnumerable<InventoryEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _items.Clear();
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.ItemId) || entry.Quantity <= 0)
            {
                throw new ArgumentException("Inventory snapshot contains an invalid entry.", nameof(entries));
            }
            _items.Add(entry.ItemId, entry.Quantity);
        }
    }
}
