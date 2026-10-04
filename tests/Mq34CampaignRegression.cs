using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq34CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(false);
        Check(!new Mq34Campaign(gated.Progress).Begin(), "MQ34 is gated by MQ33 completion");

        var world = NewWorld(true); var mq34 = new Mq34Campaign(world.Progress);
        Check(mq34.Begin(), "MQ34 begins after MQ33");
        Check(mq34.PresentModel(Mq34CrisisModel.Opening, true), "opening model can be presented first");
        Check(mq34.PresentModel(Mq34CrisisModel.HardClosure), "hard closure model has critical summary without optional notes");
        Check(!mq34.CompleteQuest(), "two models cannot complete MQ34");
        Check(mq34.PresentModel(Mq34CrisisModel.DistributedRebuild), "distributed rebuild model has critical summary without optional notes");
        Check(mq34.AllModelsKnown, "all three models are available for comparison");
        Check(!mq34.HasOptionalDetail(Mq34CrisisModel.HardClosure), "optional notes are not required for a model");
        Check(mq34.CompleteQuest(), "three models complete MQ34");
        Check(world.Progress.HasFlag(Mq34Campaign.Complete), "MQ34 completion persists in campaign state");
        Check(world.Progress.HasFlag(Mq34Campaign.Act4NaviaLeadAvailable), "Act IV Navia lead is established");
        Check(world.Progress.Quests.Get(Mq34Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ40 unlocks exactly on completion");
        Check(!mq34.CompleteQuest(), "MQ34 completion is idempotent");

        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, SaveGameService.Serialize(world));
        var resumed = new Mq34Campaign(restored.Progress);
        Check(restored.Progress.HasFlag(Mq34Campaign.Complete), "MQ34 completion survives save/load");
        Check(resumed.AllModelsKnown, "all three model summaries survive save/load");
        Check(resumed.HasOptionalDetail(Mq34CrisisModel.Opening), "optional detail survives save/load");
        Check(restored.Progress.HasFlag(Mq34Campaign.Act4NaviaLeadAvailable), "Navia lead survives save/load");
        Check(restored.Progress.Quests.Get(Mq34Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ40 handoff survives save/load");
        restored.Progress.Quests.Get(Mq34Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!resumed.CompleteQuest(), "restored completion remains idempotent");
        Check(restored.Progress.Quests.Get(Mq34Campaign.NextQuestId).Phase == QuestPhase.Investigation, "repeat cannot reset MQ40 progress");

        Console.WriteLine($"MQ34 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState(); world.Initialize();
        if (ready) world.Progress.SetFlag(Mq33Campaign.Complete);
        world.Progress.Quests.Get(Mq34Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
