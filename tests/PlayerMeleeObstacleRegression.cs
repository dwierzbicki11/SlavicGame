using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.World;

internal static class PlayerMeleeObstacleRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var obstacle = new WorldObstacle(
            "melee-wall",
            Vector3.Zero,
            new Vector2(0.5f, 0.5f),
            2f,
            Vector3.One);

        if (!obstacle.BlocksHorizontalSegment(
                new Vector3(-1f, 0f, 0f),
                new Vector3(1f, 0f, 0f)))
        {
            throw new InvalidOperationException(
                "Solid obstacle did not occlude a melee segment crossing its collider.");
        }

        if (obstacle.BlocksHorizontalSegment(
                new Vector3(-1f, 0f, 1f),
                new Vector3(1f, 0f, 1f)))
        {
            throw new InvalidOperationException(
                "Obstacle incorrectly occluded a melee segment that passes clear of its collider.");
        }

        if (!obstacle.BlocksHorizontalSegment(
                new Vector3(-1f, 0f, 0.5f),
                new Vector3(1f, 0f, 0.5f)))
        {
            throw new InvalidOperationException(
                "Melee segment touching a solid collider boundary must remain blocked.");
        }
    }
}
