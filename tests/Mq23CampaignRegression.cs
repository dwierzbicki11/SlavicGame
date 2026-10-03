using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq23CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = new WorldState();
        gated.Initialize();
        gated.Progress.Quests.Get(Mq23Campaign.QuestId).SetPhase(QuestPhase.Offered);
        Check(!new Mq23Campaign(gated.Progress).Begin(), "MQ23 is gated by MQ22 completion");

        foreach (var decision in Enum.GetValues<Mq23ResourceDecision>())
        {
            var world = NewWorld();
            var mq23 = new Mq23Campaign(world.Progress);
            Check(mq23.Begin(), $"MQ23 begins for {decision}");
            Check(!mq23.ChooseResourceControl(decision), "decision requires consequence review");
            Check(mq23.ReviewImpact(Mq23Impact.Safety), "safety impact can be reviewed first");
            Check(mq23.ReviewImpact(Mq23Impact.Extraction), "extraction impact can be reviewed second");
            Check(mq23.ReviewImpact(Mq23Impact.Stabilization), "stabilization impact can be reviewed last");
            Check(mq23.HasReviewedAllImpacts, "all three consequences are reviewed");
            Check(mq23.ChooseResourceControl(decision), $"{decision} records MQ23-D01");
            Check(!mq23.ChooseResourceControl(decision), "MQ23-D01 is single-shot");
            Check(mq23.CompleteQuest(), $"{decision} completes MQ23");
            Check(world.Progress.HasFlag(Mq23Campaign.Complete), "MQ23 completion flag written");
            Check(world.Progress.Quests.Get(Mq23Campaign.NextQuestId).Phase == QuestPhase.Offered, "every MQ23 decision unlocks MQ24");
            Check(!mq23.CompleteQuest(), "MQ23 completion is idempotent");
        }

        var persistent = NewWorld();
        var campaign = new Mq23Campaign(persistent.Progress);
        campaign.Begin();
        campaign.ReviewImpact(Mq23Impact.Extraction);
        campaign.ReviewImpact(Mq23Impact.Safety);
        var json = SaveGameService.Serialize(persistent);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var restoredCampaign = new Mq23Campaign(restored.Progress);
        Check(restoredCampaign.Begin(), "MQ23 resumes after return to region");
        Check(restoredCampaign.ReviewImpact(Mq23Impact.Stabilization), "impact review resumes after save/load");
        Check(restoredCampaign.HasReviewedAllImpacts, "reviewed impacts survive save/load");
        Check(restoredCampaign.ChooseResourceControl(Mq23ResourceDecision.ExtractionLimitReserve), "restored MQ23 accepts a decision");
        Check(restoredCampaign.CompleteQuest(), "restored MQ23 completes");

        var completedJson = SaveGameService.Serialize(restored);
        var completedRestored = new WorldState(); completedRestored.Initialize(); SaveGameService.Restore(completedRestored, completedJson);
        Check(completedRestored.Progress.HasFlag(Mq23Campaign.Complete), "MQ23 completion survives save/load");
        Check(completedRestored.Progress.HasFlag(Mq23Campaign.DecisionPrefix + "EXTRACTIONLIMITRESERVE"), "MQ23 region-state decision survives save/load");
        Check(completedRestored.Progress.Quests.Get(Mq23Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ24 handoff survives save/load");
        return checks;
    }

    private static WorldState NewWorld()
    {
        var world = new WorldState();
        world.Initialize();
        world.Progress.SetFlag("MQ22_COMPLETE");
        world.Progress.Quests.Get(Mq23Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
