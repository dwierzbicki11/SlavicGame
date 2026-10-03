using SlavicGame.Engine.Inventory;

namespace SlavicGame.Engine.Interaction;

public sealed record PickupDefinition(
    string TargetId,
    string ItemId,
    int Quantity = 1,
    string? QuestEventId = null);

public enum PickupResult
{
    Collected,
    AlreadyCollected,
    InvalidTarget
}

/// <summary>
/// Owns the durable state of world pickups. A successful collection is atomic:
/// the item enters the inventory and the target is marked consumed in the same call.
/// </summary>
public sealed class PickupInteractionState
{
    private readonly HashSet<string> _collectedTargets = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> CollectedTargets => _collectedTargets;

    public bool IsCollected(string targetId) =>
        !string.IsNullOrWhiteSpace(targetId) && _collectedTargets.Contains(targetId);

    public PickupResult TryCollect(
        InteractionTarget target,
        PickupDefinition pickup,
        InventoryState inventory,
        Action<string>? questEvent = null)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(pickup);
        ArgumentNullException.ThrowIfNull(inventory);

        if (target.Kind != InteractionKind.Take ||
            !target.Enabled ||
            string.IsNullOrWhiteSpace(pickup.TargetId) ||
            string.IsNullOrWhiteSpace(pickup.ItemId) ||
            pickup.Quantity <= 0 ||
            !StringComparer.Ordinal.Equals(target.Id, pickup.TargetId))
        {
            return PickupResult.InvalidTarget;
        }

        if (_collectedTargets.Contains(target.Id))
            return PickupResult.AlreadyCollected;

        inventory.Add(pickup.ItemId, pickup.Quantity);
        _collectedTargets.Add(target.Id);

        if (!string.IsNullOrWhiteSpace(pickup.QuestEventId))
            questEvent?.Invoke(pickup.QuestEventId);

        return PickupResult.Collected;
    }

    public string[] Capture() =>
        _collectedTargets.OrderBy(id => id, StringComparer.Ordinal).ToArray();

    public void Restore(IEnumerable<string> collectedTargetIds)
    {
        ArgumentNullException.ThrowIfNull(collectedTargetIds);
        _collectedTargets.Clear();
        foreach (var id in collectedTargetIds)
        {
            if (string.IsNullOrWhiteSpace(id) || !_collectedTargets.Add(id))
                throw new ArgumentException("Pickup snapshot contains an invalid or duplicate target id.", nameof(collectedTargetIds));
        }
    }
}
