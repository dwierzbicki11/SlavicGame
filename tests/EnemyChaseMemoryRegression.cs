using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.AI;

internal static class EnemyChaseMemoryRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var memory = new EnemyChaseMemory();
        var firstSight = new Vector3(4f, 0f, -3f);
        memory.Observe(firstSight);

        if (!memory.HasKnownPosition || memory.LastKnownPosition != firstSight || memory.IsExpired)
            throw new InvalidOperationException("Chase memory did not retain the visible target position.");

        memory.AdvanceUnseen(1.5);
        if (memory.IsExpired || MathF.Abs(memory.UnseenSeconds - 1.5f) > 0.001f)
            throw new InvalidOperationException("Chase memory expired before the lost-sight grace period.");

        var reacquired = new Vector3(7f, 0f, 2f);
        memory.Observe(reacquired);
        if (memory.LastKnownPosition != reacquired || memory.UnseenSeconds != 0f)
            throw new InvalidOperationException("Reacquiring the target did not refresh chase memory.");

        memory.AdvanceUnseen(2.1);
        if (!memory.IsExpired)
            throw new InvalidOperationException("Chase memory did not expire after sustained loss of sight.");

        memory.Reset();
        if (memory.HasKnownPosition || memory.IsExpired || memory.UnseenSeconds != 0f)
            throw new InvalidOperationException("Chase memory reset left stale engagement state.");
    }
}
