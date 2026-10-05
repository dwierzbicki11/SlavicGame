using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq40CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(false);
        Check(!new Mq40Campaign(gated.Progress).Begin(), "MQ40 is gated by MQ34 completion and lead");

        var world = NewWorld(true); var mq40 = new Mq40Campaign(world.Progress);
        Check(mq40.Begin(), "MQ40 begins after MQ34");
        Check(mq40.RecoverThresholdInstructions(), "critical threshold instructions have a guide-independent recovery path");
        Check(!mq40.SetReturnAnchor(), "return anchor cannot precede entry into Navia");
        Check(mq40.EnterNavia(), "stable threshold enters Navia");
        Check(mq40.SetReturnAnchor(), "return anchor can be established after entry");
        Check(!mq40.CompleteQuest(), "MQ40 cannot complete before rules and echo trail");
        Check(mq40.ObserveNaviaRules(), "Navia rules observation persists");
        Check(mq40.FindEchoTrail(), "trail to two similar echoes can be established");
        Check(mq40.CompleteQuest(), "critical MQ40 beats complete quest");
        Check(world.Progress.HasFlag(Mq40Campaign.EnteredNavia), "Navia entry persists");
        Check(world.Progress.HasFlag(Mq40Campaign.ReturnAnchorSet), "return anchor persists");
        Check(world.Progress.HasFlag(Mq40Campaign.Complete), "MQ40 completion persists");
        Check(world.Progress.Quests.Get(Mq40Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ41 unlocks exactly on completion");
        Check(!mq40.CompleteQuest(), "MQ40 completion is idempotent");

        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, SaveGameService.Serialize(world));
        var resumed = new Mq40Campaign(restored.Progress);
        Check(restored.Progress.HasFlag(Mq40Campaign.EnteredNavia), "entry survives save/load");
        Check(restored.Progress.HasFlag(Mq40Campaign.ReturnAnchorSet), "anchor survives save/load");
        Check(restored.Progress.HasFlag(Mq40Campaign.NaviaRulesObserved), "rules observation survives save/load");
        Check(restored.Progress.HasFlag(Mq40Campaign.EchoTrailFound), "echo trail survives save/load");
        Check(restored.Progress.HasFlag(Mq40Campaign.ThresholdInstructionsRecovered), "recovery instructions survive save/load");
        restored.Progress.Quests.Get(Mq40Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!resumed.CompleteQuest(), "restored completion remains idempotent");
        Check(restored.Progress.Quests.Get(Mq40Campaign.NextQuestId).Phase == QuestPhase.Investigation, "repeat cannot reset MQ41 progress");

        Console.WriteLine($"MQ40 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState(); world.Initialize();
        if (ready)
        {
            world.Progress.SetFlag(Mq34Campaign.Complete);
            world.Progress.SetFlag(Mq34Campaign.Act4NaviaLeadAvailable);
        }
        world.Progress.Quests.Get(Mq40Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
