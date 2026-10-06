using Veldrid.Sdl2;
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
