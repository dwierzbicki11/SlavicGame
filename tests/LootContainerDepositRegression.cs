using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Inventory;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class LootContainerDepositRegression
{
    public static void Run(Action<bool, string> check)
    {
        var state = new LootContainerInteractionState();
        var definition = new LootContainerDefinition("test-chest", []);
        var target = new InteractionTarget(definition.TargetId, Vector3.Zero, InteractionKind.Use, "KUFER");
        var inventory = new InventoryState();
        inventory.Add("arrow-basic", 10);
        var controller = new LootContainerUiController(state, definition, inventory);
        check(controller.Store(target, "arrow-basic", 4) == LootContainerResult.Stored && inventory.Count("arrow-basic") == 6 && controller.View.Stacks.Single().Quantity == 4, "Store transfers exact quantity from inventory to authoritative container state");
        check(controller.Store(target, "arrow-basic", 7) == LootContainerResult.InvalidQuantity && inventory.Count("arrow-basic") == 6 && controller.View.Stacks.Single().Quantity == 4, "Insufficient inventory leaves both sides unchanged");
        check(controller.Store(target with { Enabled = false }, "arrow-basic", 1) == LootContainerResult.InvalidTarget, "Disabled containers reject deposits");
        check(controller.Store(target with { Id = "other" }, "arrow-basic", 1) == LootContainerResult.InvalidTarget, "Mismatched targets reject deposits");
        var restored = new LootContainerInteractionState(); restored.Restore(state.Capture());
        var resumed = new LootContainerUiController(restored, definition, inventory);
        check(resumed.View.Stacks.Single().Quantity == 4 && resumed.TakeAll(target) == LootContainerResult.Looted && inventory.Count("arrow-basic") == 10 && resumed.TakeAll(target) == LootContainerResult.Empty, "Store Capture Restore retrieves a deposit once without duplication");

        var saturated = new LootContainerDefinition("full", [new("arrow-basic", int.MaxValue)]);
        var saturatedTarget = target with { Id = saturated.TargetId };
        var overflow = false;
        try { state.Store(saturatedTarget, saturated, inventory, "arrow-basic", 1); } catch (OverflowException) { overflow = true; }
        check(overflow && inventory.Count("arrow-basic") == 10 && state.Remaining(saturated).Single().Quantity == int.MaxValue, "Deposit overflow rolls back inventory without changing the container");

        var bulk = new LootContainerDefinition("bulk", [new("a", 1), new("z", 1)]);
        var bulkInventory = new InventoryState(); bulkInventory.Add("z", int.MaxValue); overflow = false;
        try { state.LootAll(target with { Id = "bulk" }, bulk, bulkInventory); } catch (OverflowException) { overflow = true; }
        check(overflow && bulkInventory.Count("a") == 0 && bulkInventory.Count("z") == int.MaxValue && state.Remaining(bulk).Count == 2, "TakeAll overflow preflight prevents partial duplication");

        var world = WorldGenerator.Generate();
        var chest = LootContainerRuntime.Position(world);
        check(chest is not null && !world.Loot.TryOpenNearest(world), "Storage is tied to an existing model and cannot open at a distance");
        world.SetPlayerPosition(chest!.Value);
        check(world.Loot.TryOpenNearest(world), "E can open nearby market chest");
        world.Progress.Inventory.Restore([new("arrow-basic", 8)]);
        world.Loot.SelectPanel(LootPanel.Inventory);
        check(world.Loot.TransferSelected(world) == LootContainerResult.Stored && world.Progress.Inventory.Count("arrow-basic") == 7, "Inventory panel deposits one item");
        check(world.Loot.TransferSelected(world, wholeStack: true) == LootContainerResult.Stored && !world.Progress.Inventory.Contains("arrow-basic"), "Inventory panel deposits the selected whole stack");
        var loaded = WorldGenerator.Generate();
        SaveGameService.Restore(loaded, SaveGameService.Serialize(world));
        check(!loaded.Loot.IsOpen && loaded.Loot.Lines(loaded, LootPanel.Container).Single().Quantity == 8, "Full game save restores deposited items and closes transient storage UI");
        check(loaded.Loot.TryOpenNearest(loaded) && loaded.Loot.TransferSelected(loaded, true) == LootContainerResult.Looted && loaded.Progress.Inventory.Count("arrow-basic") == 8 && loaded.Loot.Lines(loaded, LootPanel.Container).Count == 0, "Restored deposits return to inventory through the container panel");
        check(loaded.Loot.TransferSelected(loaded, true) == LootContainerResult.Empty && loaded.Progress.Inventory.Count("arrow-basic") == 8, "Repeated retrieval does not duplicate stored items");

        loaded.Progress.Inventory.Add("simple-bandage", 2);
        loaded.Loot.SelectPanel(LootPanel.Inventory);
        check(loaded.Loot.TransferSelected(loaded, true) == LootContainerResult.Stored && loaded.Loot.Lines(loaded, LootPanel.Container).Single().ItemId == "arrow-basic", "Inventory selection remains deterministic while preparing bulk retrieval");
        loaded.Loot.SelectPanel(LootPanel.Inventory); loaded.Loot.MoveSelection(loaded, 1);
        check(loaded.Loot.TransferSelected(loaded, true) == LootContainerResult.Stored && loaded.Loot.Lines(loaded, LootPanel.Container).Count == 2, "Runtime container can hold multiple deposited stacks");
        check(loaded.Loot.TakeAll(loaded) == LootContainerResult.Looted && loaded.Progress.Inventory.Count("arrow-basic") == 8 && loaded.Progress.Inventory.Count("simple-bandage") == 2 && loaded.Loot.Lines(loaded, LootPanel.Container).Count == 0 && loaded.Loot.Message.StartsWith("ZABRANO WSZYSTKO", StringComparison.Ordinal), "Runtime TakeAll atomically retrieves every container stack and reports completion");
        check(loaded.Loot.TakeAll(loaded) == LootContainerResult.Empty && loaded.Progress.Inventory.Count("simple-bandage") == 2, "Runtime TakeAll on an empty container is idempotent");

        loaded.Progress.Inventory.Add("simple-bow"); loaded.Bow.SetAiming(loaded, true);
        check(!loaded.Bow.IsAiming, "Storage UI blocks bow aiming");
        loaded.SetPlayerPosition(Vector3.Zero); loaded.Loot.Update(loaded);
        check(!loaded.Loot.IsOpen, "Moving out of reach closes storage UI");
        var oldSave = SaveGameService.Capture(loaded) with { LootContainers = null };
        SaveGameService.Restore(loaded, oldSave);
        check(loaded.Loot.Lines(loaded, LootPanel.Container).Count == 0, "Existing saves without storage state load safely");
    }
}
