using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Inventory;

static class LootContainerUiRegression
{
    public static void Run(Action<bool, string> check)
    {
        var state = new LootContainerInteractionState();
        var inventory = new InventoryState();
        var definition = new LootContainerDefinition(
            "ui-chest-01",
            [new LootStack("coin", 3), new LootStack("herb", 2)],
            "quest.ui-chest.emptied");
        var target = new InteractionTarget(
            definition.TargetId,
            Vector3.Zero,
            InteractionKind.Inspect,
            "Przeszukaj");
        var events = new List<string>();
        var ui = new LootContainerUiController(state, definition, inventory, events.Add);

        check(ui.View.Stacks.Count == 2 && !ui.View.IsEmpty,
            "loot UI exposes current container stacks");
        check(ui.Take(target, "coin", 1) == LootContainerResult.Looted,
            "loot UI delegates partial take to gameplay state");
        check(inventory.Count("coin") == 1 && ui.View.Stacks.Single(s => s.ItemId == "coin").Quantity == 2,
            "loot UI refreshes from authoritative state after partial take");
        check(events.Count == 0,
            "partial UI take does not complete container quest event");
        check(ui.TakeAll(target) == LootContainerResult.Looted && ui.View.IsEmpty,
            "loot UI take all empties container");
        check(inventory.Count("coin") == 3 && inventory.Count("herb") == 2,
            "loot UI transfers remaining stacks exactly once");
        check(events.SequenceEqual([definition.QuestEventId!]),
            "loot UI emits completion event only when container becomes empty");
        check(ui.TakeAll(target) == LootContainerResult.Empty,
            "empty loot UI cannot duplicate consumed contents");
    }
}
