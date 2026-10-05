using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq41CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(false);
        Check(!new Mq41Campaign(gated.Progress).Begin(), "MQ41 is gated by MQ40 completion");

        var world = NewWorld(true); var mq41 = new Mq41Campaign(world.Progress);
        Check(mq41.Begin(), "MQ41 begins after MQ40");
        Check(mq41.FindEchoB(), "echo B may be found first");
        Check(!mq41.RecordDifference01(), "differences require both echoes");
        Check(mq41.FindEchoA(), "echo A may be found second");
        Check(!mq41.CompareInJournal(), "one or zero differences cannot establish distinction");
        Check(mq41.RecordDifference02(), "second-labelled independent difference can be recorded first");
        Check(!mq41.CompareInJournal(), "one difference remains insufficient");

        var midSave = SaveGameService.Serialize(world);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, midSave);
        var resumed = new Mq41Campaign(restored.Progress);
        Check(restored.Progress.HasFlag(Mq41Campaign.EchoAFound) && restored.Progress.HasFlag(Mq41Campaign.EchoBFound), "both echoes survive save/load");
        Check(restored.Progress.HasFlag(Mq41Campaign.Difference02), "partial evidence survives save/load");
        Check(resumed.RecoverDifference01(), "missed observation has durable recovery evidence path");
        Check(resumed.CompareInJournal(), "two differences and both echoes establish operational distinction");
        Check(restored.Progress.HasFlag(Mq41Campaign.NaviaSplotDistinction), "Navia/Splot distinction persists");
        Check(resumed.CompleteQuest(), "MQ41 completes after journal comparison");
        Check(restored.Progress.HasFlag(Mq41Campaign.Complete), "MQ41 completion persists");
        Check(restored.Progress.Quests.Get(Mq41Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ42 unlocks on completion");
        Check(!resumed.CompleteQuest(), "MQ41 completion is idempotent");
        restored.Progress.Quests.Get(Mq41Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!resumed.CompleteQuest(), "repeat completion remains rejected");
        Check(restored.Progress.Quests.Get(Mq41Campaign.NextQuestId).Phase == QuestPhase.Investigation, "repeat cannot reset MQ42 progress");

        var direct = NewWorld(true); var directMq41 = new Mq41Campaign(direct.Progress);
        Check(directMq41.Begin() && directMq41.FindEchoA() && directMq41.FindEchoB(), "A/B order is also valid");
        Check(directMq41.RecordDifference01() && directMq41.RecordDifference02() && directMq41.CompareInJournal() && directMq41.CompleteQuest(), "direct evidence path completes MQ41");

        Console.WriteLine($"MQ41 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState(); world.Initialize();
        if (ready) world.Progress.SetFlag(Mq40Campaign.Complete);
        world.Progress.Quests.Get(Mq41Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
