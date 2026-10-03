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

        var gated = new WorldState();
        gated.Initialize();
        gated.Progress.Quests.Get(Mq24Campaign.QuestId).SetPhase(QuestPhase.Offered);
        Check(!new Mq24Campaign(gated.Progress).Begin(), "MQ24 is gated by MQ23 completion");

        foreach (var first in Enum.GetValues<Mq24RouteClue>())
        {
            var second = first == Mq24RouteClue.StoryOrSign ? Mq24RouteClue.TerrainObservation : Mq24RouteClue.StoryOrSign;
            var world = NewWorld();
            var mq24 = new Mq24Campaign(world.Progress);
            Check(mq24.Begin(), "MQ24 begins after MQ23");
            Check(world.Progress.HasFlag(Mq24Campaign.MapMismatch), "map mismatch is recorded");
            Check(mq24.RecordRouteClue(first), "first route clue can be recorded");
            Check(mq24.RecordRouteClue(second), "second route clue can be recorded");
            Check(mq24.HasBothRouteClues, "route clues work in either order");
            Check(!mq24.AttemptTrialPassage(false), "wrong route attempt does not block recovery");
            Check(mq24.AttemptTrialPassage(true), "correct trial passage records relation without guide NPC");
            Check(world.Progress.HasFlag(Mq24Campaign.RouteRelation), "route relation evidence is recorded");
            Check(mq24.CompleteQuest(), "MQ24 completes after route relation");
            Check(world.Progress.Quests.Get(Mq24Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ24 unlocks MQ25");
            Check(!mq24.CompleteQuest(), "MQ24 completion is idempotent");
        }

        var persistent = NewWorld();
        var campaign = new Mq24Campaign(persistent.Progress);
        campaign.Begin();
        campaign.RecordRouteClue(Mq24RouteClue.TerrainObservation);
        var json = SaveGameService.Serialize(persistent);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var resumed = new Mq24Campaign(restored.Progress);
        Check(resumed.Begin(), "MQ24 resumes after leaving and returning to region");
        Check(resumed.RecordRouteClue(Mq24RouteClue.StoryOrSign), "second clue can be collected after save/load");
        Check(resumed.HasBothRouteClues, "route-memory clues survive save/load");
        Check(resumed.AttemptTrialPassage(true), "trial passage can finish after save/load");
        Check(resumed.CompleteQuest(), "restored MQ24 completes");

        var completedJson = SaveGameService.Serialize(restored);
        var completedRestored = new WorldState(); completedRestored.Initialize(); SaveGameService.Restore(completedRestored, completedJson);
        Check(completedRestored.Progress.HasFlag(Mq24Campaign.Complete), "MQ24 completion survives save/load");
        Check(completedRestored.Progress.HasFlag(Mq24Campaign.RouteRelation), "route relation survives save/load");
        Check(completedRestored.Progress.Quests.Get(Mq24Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ25 handoff survives save/load");
        return checks;
    }

    private static WorldState NewWorld()
    {
        var world = new WorldState();
        world.Initialize();
        world.Progress.SetFlag("MQ23_COMPLETE");
        world.Progress.Quests.Get(Mq24Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
