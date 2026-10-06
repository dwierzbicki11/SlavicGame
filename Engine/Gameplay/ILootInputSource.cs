using Veldrid;

namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Minimal per-frame input surface required by the loot UI adapter.
/// Keeping the adapter on this boundary makes live keyboard routing regression-testable
/// without constructing an SDL window.
/// </summary>
public interface ILootInputSource
{
    bool ConsumeKeyPress(Key key);
    bool IsKeyDown(Key key);
}
