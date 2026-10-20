using System.Numerics;
using SlavicGame.Engine.World;

internal static class NpcRoutineMotionRegression
{
    public static void Run(Action<bool, string> check)
    {
        // Include a dialogue time offset: midnight must remain continuous even
        // when the speaker's routine clock was paused earlier in the day.
        foreach (var offset in new[] { 0d, 0.17d, 1.3d })
        {
            var before = Sample(24d - 0.00001d - offset);
            var after = Sample(0.00001d - offset);
            check(before.IsMoving && after.IsMoving &&
                  Vector2.Distance(before.Position, after.Position) < 0.01f,
                $"Patrol route stays continuous across midnight (offset {offset})");

            var nextDay = Sample(8d + 24d - offset);
            var today = Sample(8d - offset);
            check(Vector2.Distance(today.Position, nextDay.Position) < 0.001f,
                $"NPC route repeats at the same time on consecutive days (offset {offset})");
        }

        var moving = Sample(23.9d);
        var later = Sample(23.91d);
        check(Vector2.Distance(moving.Position, later.Position) > 0.1f,
            "Patrol route continues moving within the day");
    }

    private static NpcRoutineSample Sample(double hours) =>
        NpcRoutineMotion.Sample(
            "community-guard", "patrol", Vector2.Zero, hours);
}
