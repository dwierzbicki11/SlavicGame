using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq13CampaignRegression
{
    [ModuleInitializer]
    internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception(name);
            checks++;
        }

        var world = new WorldState();
        world.Initialize();
        var mq13 = NewCampaign(world.Progress);
        Check(!mq13.Begin(), "MQ13 is gated by prior completions");
        world.Progress.SetFlag("MQ10_COMPLETE");
        world.Progress.SetFlag("MQ11_COMPLETE");
        world.Progress.SetFlag("MQ12_COMPLETE");
        world.Progress.Quests.Get(Mq13Campaign.QuestId).SetPhase(QuestPhase.Offered);
        Check(mq13.Begin(), "MQ13 begins after Act I prerequisites");
        Check(mq13.AddDataset("nadborze-routes"), "first dataset accepted");
        Check(mq13.AddDataset("bor-nodes"), "second dataset accepted");
        Check(mq13.AddEvidence("mq10-pattern"), "MQ10 evidence accepted");
        Check(mq13.AddEvidence("mq11-context"), "MQ11 evidence accepted");
        Check(!mq13.HasMinimumEvidence, "two evidence packages do not satisfy MQ13");
        Check(mq13.AddEvidence("mq12-node"), "MQ12 evidence accepted");
        Check(mq13.HasMinimumEvidence, "three required evidence packages satisfy MQ13");
        Check(mq13.TestHypothesis("predicted-node-r2"), "predicted point synthesizes hypothesis");
        Check(world.Progress.HasFlag(Mq13Campaign.NetworkHypothesis), "network hypothesis flag is durable progress");

        var json = SaveGameService.Serialize(world);
        var restored = new WorldState();
        restored.Initialize();
        SaveGameService.Restore(restored, json);
        var restoredMq13 = NewCampaign(restored.Progress);
        Check(restored.Progress.MapOverlay.HypothesisSynthesized, "overlay survives save/load before completion");
        Check(restoredMq13.CompleteQuest(), "restored MQ13 can complete");
        Check(restored.Progress.HasFlag(Mq13Campaign.Complete), "MQ13 completion flag written");
        Check(restored.Progress.Quests.Get(Mq13Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ13 completion unlocks MQ20 and Act II");
        Check(!restoredMq13.CompleteQuest(), "MQ13 completion is idempotent");
        Check(restored.Progress.Quests.Get(Mq13Campaign.NextQuestId).Phase == QuestPhase.Offered, "idempotent completion preserves MQ20 offer");

        var completedJson = SaveGameService.Serialize(restored);
        var completedRestored = new WorldState();
        completedRestored.Initialize();
        SaveGameService.Restore(completedRestored, completedJson);
        Check(completedRestored.Progress.Quests.Get(Mq13Campaign.NextQuestId).Phase == QuestPhase.Offered, "Act II handoff survives save/load");

        var extended = new WorldState();
        extended.Initialize();
        extended.Progress.SetFlag("MQ10_COMPLETE");
        extended.Progress.SetFlag("MQ11_COMPLETE");
        extended.Progress.SetFlag("MQ12_COMPLETE");
        extended.Progress.Quests.Get(Mq13Campaign.QuestId).SetPhase(QuestPhase.Offered);
        var extendedMq13 = NewCampaign(extended.Progress);
        extendedMq13.Begin();
        extendedMq13.AddDataset("nadborze-routes");
        extendedMq13.AddDataset("bor-nodes");
        extendedMq13.AddEvidence("optional-context");
        extendedMq13.AddEvidence("mq10-pattern");
        extendedMq13.AddEvidence("mq11-context");
        extendedMq13.AddEvidence("mq12-node");
        Check(extendedMq13.HasMinimumEvidence, "optional evidence does not change minimum gating");
        Check(extendedMq13.TestHypothesis("predicted-node-r2"), "extended evidence set also synthesizes");

        return checks;
    }

    private static Mq13Campaign NewCampaign(GameProgress progress) => new(
        progress,
        new[] { "nadborze-routes", "bor-nodes" },
        new[] { "mq10-pattern", "mq11-context", "mq12-node" });
}
