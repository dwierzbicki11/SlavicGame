using System.Numerics;

namespace SlavicGame.Engine.Input;

public static class MouseMotion
{
    // Events and the polled accumulator describe the same motion. Choose one;
    // adding them would double camera sensitivity. Preserve delivered events
    // even when another SDL consumer has already drained the accumulator.
    public static Vector2 Select(Vector2 eventDelta, Vector2 polledDelta)
        => eventDelta != Vector2.Zero ? eventDelta : polledDelta;
}
