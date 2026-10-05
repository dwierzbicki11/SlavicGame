using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Converts one frame of storage UI input into at most one authoritative loot command.
/// Window/gamepad adapters stay dumb and the runtime remains the single owner of loot state.
/// </summary>
public static class LootInputRouter
{
    public static LootCommand? Resolve(
        bool close,
        bool containerPanel,
        bool inventoryPanel,
        bool previous,
        bool next,
        bool transfer,
        bool transferStack,
        bool takeAll)
    {
        if (close) return LootCommand.Close;
        if (takeAll) return LootCommand.TakeAll;
        if (containerPanel) return LootCommand.ContainerPanel;
        if (inventoryPanel) return LootCommand.InventoryPanel;
        if (previous) return LootCommand.Previous;
        if (next) return LootCommand.Next;
        if (transfer) return transferStack ? LootCommand.TransferStack : LootCommand.TransferOne;
        return null;
    }

    /// <summary>
    /// Resolves and executes one input frame through the authoritative loot runtime.
    /// This is the integration seam for window/gamepad adapters: callers cannot accidentally
    /// perform multiple inventory mutations from simultaneous keys in a single frame.
    /// </summary>
    public static LootContainerResult? Dispatch(
        WorldState world,
        bool close,
        bool containerPanel,
        bool inventoryPanel,
        bool previous,
        bool next,
        bool transfer,
        bool transferStack,
        bool takeAll)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!world.Loot.IsOpen) return null;

        var command = Resolve(
            close,
            containerPanel,
            inventoryPanel,
            previous,
            next,
            transfer,
            transferStack,
            takeAll);

        return command is null
            ? null
            : world.Loot.HandleCommand(world, command.Value);
    }
}
