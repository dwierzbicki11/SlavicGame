namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Keys understood by the loot UI. This gameplay boundary deliberately does not expose
/// the windowing backend's key type, so loot routing remains testable without SDL/Veldrid.
/// </summary>
public enum LootInputKey
{
    Escape,
    ContainerPanel,
    InventoryPanel,
    Previous,
    Next,
    Transfer,
    TransferStack,
    TakeAll
}

/// <summary>
/// Minimal per-frame input surface required by the loot UI adapter.
/// </summary>
public interface ILootInputSource
{
    bool ConsumeLootKeyPress(LootInputKey key);
    bool IsLootKeyDown(LootInputKey key);
}
