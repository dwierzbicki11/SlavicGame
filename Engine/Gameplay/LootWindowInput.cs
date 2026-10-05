using Veldrid;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Production keyboard adapter for the open loot/container UI.
/// It samples one frame and delegates arbitration plus mutation to LootInputRouter,
/// keeping GameEngine free of direct inventory/container mutations.
/// </summary>
public static class LootWindowInput
{
    public static LootContainerResult? Dispatch(GameWindow window, WorldState world)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(world);
        if (!world.Loot.IsOpen) return null;

        return LootInputRouter.Dispatch(
            world,
            close: window.ConsumeKeyPress(Key.Escape),
            containerPanel: window.ConsumeKeyPress(Key.A),
            inventoryPanel: window.ConsumeKeyPress(Key.D),
            previous: window.ConsumeKeyPress(Key.W),
            next: window.ConsumeKeyPress(Key.S),
            transfer: window.ConsumeKeyPress(Key.E),
            transferStack: window.IsKeyDown(Key.ShiftLeft),
            takeAll: window.ConsumeKeyPress(Key.F));
    }
}
