using Veldrid;
using SlavicGame.Engine.Windowing;

namespace SlavicGame.Engine.Gameplay;

/// <summary>
/// Thin production bridge from GameWindow to the loot input boundary.
/// It does not own input state and never mutates inventory or container state.
/// </summary>
public sealed class GameWindowLootInputSource(GameWindow window) : ILootInputSource
{
    private readonly GameWindow _window = window ?? throw new ArgumentNullException(nameof(window));

    public bool ConsumeKeyPress(Key key) => _window.ConsumeKeyPress(key);
    public bool IsKeyDown(Key key) => _window.IsKeyDown(key);
}
