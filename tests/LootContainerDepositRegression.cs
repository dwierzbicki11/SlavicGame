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

        var questState = new LootContainerInteractionState();
        var questDefinition = new LootContainerDefinition("quest-chest", [new("marsh-herb", 1)], "quest:chest-cleared");
        var questTarget = target with { Id = questDefinition.TargetId };
        var questInventory = new InventoryState();
        var questEvents = 0;
        var questController = new LootContainerUiController(questState, questDefinition, questInventory, _ => questEvents++);
        check(questController.TakeAll(questTarget) == LootContainerResult.Looted && questEvents == 1, "Emptying a quest loot container emits its completion event once");
        check(questController.Store(questTarget, "marsh-herb", 1) == LootContainerResult.Stored && questController.TakeAll(questTarget) == LootContainerResult.Looted && questEvents == 1, "Re-storing and re-looting a completed quest container does not duplicate its quest event");
        var questRestoredState = new LootContainerInteractionState(); questRestoredState.Restore(questState.Capture());
        var questRestored = new LootContainerUiController(questRestoredState, questDefinition, questInventory, _ => questEvents++);
        check(questRestored.Store(questTarget, "marsh-herb", 1) == LootContainerResult.Stored && questRestored.TakeAll(questTarget) == LootContainerResult.Looted && questEvents == 1, "Save restore preserves one-shot loot quest completion");
        var legacyQuestState = new LootContainerInteractionState();
        legacyQuestState.Restore([new LootContainerSnapshot("legacy-quest-chest", [new("marsh-herb", 1)])]);
        var legacyQuestDefinition = new LootContainerDefinition("legacy-quest-chest", [new("marsh-herb", 1)], "quest:legacy-chest-cleared");
        var legacyQuestTarget = target with { Id = legacyQuestDefinition.TargetId };
        var legacyEvents = 0;
        check(new LootContainerUiController(legacyQuestState, legacyQuestDefinition, new InventoryState(), _ => legacyEvents++).TakeAll(legacyQuestTarget) == LootContainerResult.Looted && legacyEvents == 1, "Legacy snapshots without completion metadata remain compatible and can emit their first quest event");

        var saturated = new LootContainerDefinition("full", [new("arrow-basic", int.MaxValue)]);
        var saturatedTarget = target with { Id = saturated.TargetId };
        var overflow = false;
        try { state.Store(saturatedTarget, saturated, inventory, "arrow-basic", 1); } catch (OverflowException) { overflow = true; }
        check(overflow && inventory.Count("arrow-basic") == 10 && state.Remaining(saturated).Single().Quantity == int.MaxValue, "Deposit overflow rolls back inventory without changing the container");

        var bulk = new LootContainerDefinition("bulk", [new("a", 1), new("z", 1)]);
        var bulkInventory = new InventoryState(); bulkInventory.Add("z", int.MaxValue); overflow = false;
        try { state.LootAll(target with { Id = "bulk" }, bulk, bulkInventory); } catch (OverflowException) { overflow = true; }
        check(overflow && bulkInventory.Count("a") == 0 && bulkInventory.Count("z") == int.MaxValue && state.Remaining(bulk).Count == 2, "TakeAll overflow preflight prevents partial duplication");

        check(LootInputRouter.Resolve(false, false, false, false, false, true, false, false) == LootCommand.TransferOne, "Loot input routes E-style transfer to one item");
        check(LootInputRouter.Resolve(false, false, false, false, false, true, true, false) == LootCommand.TransferStack, "Loot input routes modified transfer to the whole stack");
        check(LootInputRouter.Resolve(false, false, false, false, false, true, true, true) == LootCommand.TakeAll, "Take All wins over transfer so one key frame cannot perform two mutations");
        check(LootInputRouter.Resolve(true, false, false, false, false, true, true, true) == LootCommand.Close, "Close has highest loot input priority and cannot mutate inventory while dismissing UI");
        check(LootInputRouter.Resolve(false, false, false, false, false, false, false, false) is null, "Idle loot input emits no command");

        var world = WorldGenerator.Generate();
        var chest = LootContainerRuntime.Position(world);
        check(chest is not null && !world.Loot.TryOpenNearest(world), "Storage is tied to an existing model and cannot open at a distance");
        world.SetPlayerPosition(chest!.Value);
        check(world.Loot.TryOpenNearest(world), "E can open nearby market chest");
        world.Progress.Inventory.Restore([new("arrow-basic", 8)]);
        check(world.Loot.HandleCommand(world, LootCommand.InventoryPanel) is null && world.Loot.Panel == LootPanel.Inventory, "Loot command surface switches to inventory panel");
        check(world.Loot.HandleCommand(world, LootCommand.TransferOne) == LootContainerResult.Stored && world.Progress.Inventory.Count("arrow-basic") == 7, "Loot command surface deposits one selected item");
        check(world.Loot.HandleCommand(world, LootCommand.TransferStack) == LootContainerResult.Stored && !world.Progress.Inventory.Contains("arrow-basic"), "Loot command surface deposits the selected whole stack");
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
        check(loaded.Loot.HandleCommand(loaded, LootCommand.TakeAll) == LootContainerResult.Looted && loaded.Progress.Inventory.Count("arrow-basic") == 8 && loaded.Progress.Inventory.Count("simple-bandage") == 2 && loaded.Loot.Lines(loaded, LootPanel.Container).Count == 0 && loaded.Loot.Message.StartsWith("ZABRANO WSZYSTKO", StringComparison.Ordinal), "Loot command surface atomically retrieves every container stack and reports completion");
        check(loaded.Loot.HandleCommand(loaded, LootCommand.TakeAll) == LootContainerResult.Empty && loaded.Progress.Inventory.Count("simple-bandage") == 2, "Loot command TakeAll on an empty container is idempotent");

        loaded.Progress.Inventory.Add("simple-bow"); loaded.Bow.SetAiming(loaded, true);
        check(!loaded.Bow.IsAiming, "Storage UI blocks bow aiming");
        check(loaded.Loot.HandleCommand(loaded, LootCommand.Close) is null && !loaded.Loot.IsOpen, "Loot command surface closes the storage UI");
        loaded.SetPlayerPosition(Vector3.Zero); loaded.Loot.Update(loaded);
        check(!loaded.Loot.IsOpen, "Moving out of reach closes storage UI");
        var oldSave = SaveGameService.Capture(loaded) with { LootContainers = null };
        SaveGameService.Restore(loaded, oldSave);
        check(loaded.Loot.Lines(loaded, LootPanel.Container).Count == 0, "Existing saves without storage state load safely");
    }
}
