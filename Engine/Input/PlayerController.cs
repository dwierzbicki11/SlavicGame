using System.Numerics;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Input;

public static class PlayerController
{
    public static void Update(WorldState world, Camera3D camera, PlayerInput input, double deltaSeconds)
    {
        // Look and movement belong to the same frame, even while a movement key is held.
        camera.Rotate(input.LookDelta.X, input.LookDelta.Y);

        var move = Vector3.Zero;
        if (input.Forward) move += camera.GetMoveForward();
        if (input.Backward) move -= camera.GetMoveForward();
        if (input.Right) move += camera.GetMoveRight();
        if (input.Left) move -= camera.GetMoveRight();

        if (move.LengthSquared() > 0.001f)
        {
            move = Vector3.Normalize(move);
            var speed = input.Running ? 9f : 5f;
            world.SetPlayerPosition(world.PlayerPosition + move * speed * (float)deltaSeconds);
        }

        camera.Follow(world.PlayerPosition, (float)deltaSeconds);
    }
}
