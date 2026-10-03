namespace SlavicGame.Engine.Interaction;

public sealed record LootContainerView(string TargetId, IReadOnlyList<LootStack> Stacks, bool IsEmpty)
{
    public static LootContainerView From(LootContainerInteractionState state, LootContainerDefinition container)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(container);
        var stacks = state.Remaining(container).ToArray();
        return new LootContainerView(container.TargetId, stacks, stacks.Length == 0);
    }
}

/// <summary>UI adapter only; authoritative loot mutations remain in LootContainerInteractionState.</summary>
public sealed class LootContainerUiController
{
    private readonly LootContainerInteractionState _state;
    private readonly LootContainerDefinition _container;
    private readonly Inventory.InventoryState _inventory;
    private readonly Action<string>? _questEvent;

    public LootContainerUiController(LootContainerInteractionState state, LootContainerDefinition container,
        Inventory.InventoryState inventory, Action<string>? questEvent = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(inventory);
        _state = state; _container = container; _inventory = inventory; _questEvent = questEvent;
    }

    public LootContainerView View => LootContainerView.From(_state, _container);
    public LootContainerResult Take(InteractionTarget target, string itemId, int quantity) =>
        _state.Take(target, _container, _inventory, itemId, quantity, _questEvent);
    public LootContainerResult TakeAll(InteractionTarget target) =>
        _state.LootAll(target, _container, _inventory, _questEvent);
}
