using System.Numerics;

namespace SlavicGame.Engine.Input;

public readonly record struct PlayerInput(
    bool Forward,
    bool Backward,
    bool Right,
    bool Left,
    bool Running,
    Vector2 LookDelta,
    bool DodgePressed = false);
