using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq24CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(mq23Complete: false);
        Check(!new Mq24Campaign(gated.Progress).Begin(), "MQ24 is gated by MQ23 completion");

        var world = NewWorld(mq23Complete: true);
        var mq24 = new Mq24Campaign(world.Progress);
        Check(mq24.Begin(), "MQ24 begins after MQ23 completion");
        Check(world.Progress.HasFlag(Mq24Campaign.MapMismatch), "MQ24 records map mismatch on start");
        Check(!mq24.AttemptTrialPassage(true), "trial passage requires both route clues");

        Check(mq24.RecordRouteClue(Mq24RouteClue.StoryOrSign), "story/sign route clue recorded");
        Check(!mq24.HasBothRouteClues, "single clue does not satisfy route clue requirement");
        Check(!mq24.AttemptTrialPassage(true), "trial passage remains locked with one clue");

        var midSave = SaveGameService.Serialize(world);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, midSave);
        var resumed = new Mq24Campaign(restored.Progress);
        Check(restored.Progress.HasFlag(Mq24Campaign.MapMismatch), "map mismatch survives save/load");
        Check(resumed.RecordRouteClue(Mq24RouteClue.TerrainObservation), "terrain observation clue recorded after save/load");
        Check(resumed.HasBothRouteClues, "both route clues are present across save/load");
        Check(!resumed.AttemptTrialPassage(false), "incorrect route application does not unlock route relation");
        Check(!restored.Progress.HasFlag(Mq24Campaign.RouteRelation), "failed trial passage leaves route relation unset");
        Check(resumed.AttemptTrialPassage(true), "correct route application confirms Arel route relation");
        Check(restored.Progress.HasFlag(Mq24Campaign.RouteRelation), "route relation flag is durable");
        Check(!resumed.AttemptTrialPassage(true), "trial passage is idempotent once route relation is known");

        Check(resumed.CompleteQuest(), "MQ24 completes after route relation is confirmed");
        Check(restored.Progress.HasFlag(Mq24Campaign.Complete), "MQ24 completion flag is durable");
        Check(restored.Progress.Quests.Get(Mq24Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ24 completion unlocks MQ25");
        Check(!resumed.CompleteQuest(), "MQ24 completion is idempotent");

        var completedRestored = new WorldState(); completedRestored.Initialize();
        SaveGameService.Restore(completedRestored, SaveGameService.Serialize(restored));
        Check(completedRestored.Progress.HasFlag(Mq24Campaign.Complete), "MQ24 completion survives save/load");
        Check(completedRestored.Progress.HasFlag(Mq24Campaign.RouteRelation), "MQ24 route relation survives save/load");
        Check(completedRestored.Progress.Quests.Get(Mq24Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ25 handoff survives save/load");

        Console.WriteLine($"MQ24 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool mq23Complete)
    {
        var world = new WorldState();
        world.Initialize();
        if (mq23Complete) world.Progress.SetFlag("MQ23_COMPLETE");
        world.Progress.Quests.Get(Mq24Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
