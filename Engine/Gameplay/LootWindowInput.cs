using Veldrid;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Production keyboard adapter for the open loot/container UI.
/// Samples one frame and delegates arbitration and mutation to LootInputRouter.
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
            close: input.ConsumeKeyPress(Key.Escape),
            containerPanel: input.ConsumeKeyPress(Key.A),
            inventoryPanel: input.ConsumeKeyPress(Key.D),
            previous: input.ConsumeKeyPress(Key.W),
            next: input.ConsumeKeyPress(Key.S),
            transfer: input.ConsumeKeyPress(Key.E),
            transferStack: input.IsKeyDown(Key.ShiftLeft),
            takeAll: input.ConsumeKeyPress(Key.F));
    }
}
