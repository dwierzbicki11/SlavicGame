using System.Runtime.CompilerServices;
using SlavicGame.Engine.World;

internal static class TimedEventRegression
{
    [ModuleInitializer]
    internal static void Run()
    {
        var scheduler = new TimedEventSystem();
        scheduler.Register(new TimedEventDefinition(
            "ambient-fog", 20, 6, TimedEventPriority.Ambient, "swamp-slot", TimedEventRepeatPolicy.RepeatNextCycle));
        scheduler.Register(new TimedEventDefinition(
            "quest-light", 20, 6, TimedEventPriority.CriticalQuest, "swamp-slot"));

        var firstNight = scheduler.Evaluate(23, 0);
        Require(firstNight.Count == 1 && firstNight[0].Id == "quest-light",
            "Higher priority event must win a conflict group.");
        Require(scheduler.Evaluate(23, 0).Count == 1 && scheduler.Evaluate(23, 0).Count == 0,
            "Repeat event may fire at most once per day.");

        var snapshot = scheduler.Capture();
        var restored = new TimedEventSystem();
        restored.Register(new TimedEventDefinition(
            "ambient-fog", 20, 6, TimedEventPriority.Ambient, "swamp-slot", TimedEventRepeatPolicy.RepeatNextCycle));
        restored.Register(new TimedEventDefinition(
            "quest-light", 20, 6, TimedEventPriority.CriticalQuest, "swamp-slot"));
        restored.Restore(snapshot);
        Require(restored.Evaluate(23, 0).Count == 0,
            "Loading persisted state must not duplicate an event on the same day.");
        var nextNight = restored.Evaluate(2, 1);
        Require(nextNight.Count == 1 && nextNight[0].Id == "ambient-fog",
            "RepeatNextCycle event must become eligible on a later day while once-only event stays consumed.");

        var overnight = new TimedEventDefinition("overnight", 20, 6, TimedEventPriority.Ambient);
        Require(overnight.IsInWindow(23) && overnight.IsInWindow(2) && !overnight.IsInWindow(12),
            "Timed event windows must support crossing midnight.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
