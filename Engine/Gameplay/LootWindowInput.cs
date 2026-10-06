using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Production keyboard adapter for the open loot/container UI.
/// It samples one frame and delegates arbitration plus mutation to LootInputRouter,
/// keeping GameEngine free of direct inventory/container mutations.
/// </summary>
public static class LootWindowInput
{
    public static LootContainerResult? Dispatch(ILootInputSource input, WorldState world)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(world);
        if (!world.Loot.IsOpen) return null;

        return LootInputRouter.Dispatch(
            world,
            close: input.ConsumeLootKeyPress(LootInputKey.Escape),
            containerPanel: input.ConsumeLootKeyPress(LootInputKey.ContainerPanel),
            inventoryPanel: input.ConsumeLootKeyPress(LootInputKey.InventoryPanel),
            previous: input.ConsumeLootKeyPress(LootInputKey.Previous),
            next: input.ConsumeLootKeyPress(LootInputKey.Next),
            transfer: input.ConsumeLootKeyPress(LootInputKey.Transfer),
            transferStack: input.IsLootKeyDown(LootInputKey.TransferStack),
            takeAll: input.ConsumeLootKeyPress(LootInputKey.TakeAll));
    }
}
