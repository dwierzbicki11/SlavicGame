using Veldrid;

namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Minimal per-frame input surface required by the loot UI adapter.
/// Keeps live keyboard routing testable without constructing an SDL window.
/// </summary>
public interface ILootInputSource
{
    bool ConsumeKeyPress(Key key);
    bool IsKeyDown(Key key);
}
