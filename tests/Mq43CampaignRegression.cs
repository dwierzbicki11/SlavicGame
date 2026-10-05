using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq43CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(mq42Complete: false, distinction: true);
        Check(!new Mq43Campaign(gated.Progress).Begin(), "MQ43 is gated by MQ42 completion");

        var world = NewWorld(mq42Complete: true, distinction: false);
        var mq43 = new Mq43Campaign(world.Progress);
        Check(mq43.Begin(), "MQ43 begins after MQ42");
        Check(!mq43.ObserveRuleBreak(), "rule break requires entering unstable pattern");
        Check(mq43.EnterUnstablePattern(), "unstable pattern can be entered");
        Check(mq43.ObserveRuleBreak(), "spatial rule break can be observed");
        Check(!mq43.RevealSplotTruth(), "full reveal requires MQ41 distinction");

        world.Progress.SetFlag(Mq41Campaign.NaviaSplotDistinction);
        var midSave = SaveGameService.Serialize(world);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, midSave);
        var resumed = new Mq43Campaign(restored.Progress);
        Check(restored.Progress.HasFlag(Mq43Campaign.UnstablePatternEntered) && restored.Progress.HasFlag(Mq43Campaign.RuleBreakObserved), "reveal observations survive save/load");
        Check(resumed.ResumeReveal(), "interrupted reveal resumes from durable checkpoint");
        Check(restored.Progress.HasFlag(Mq43Campaign.SplotTruthKnown), "Splot truth becomes durable only after gating");
        Check(resumed.CompleteQuest(), "MQ43 completes after reveal");
        Check(restored.Progress.Quests.Get(Mq43Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ44 unlocks exactly after MQ43 completion");
        Check(!resumed.CompleteQuest(), "MQ43 completion is idempotent");

        restored.Progress.Quests.Get(Mq43Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!resumed.CompleteQuest(), "repeat completion cannot reset MQ44 progress");
        Check(restored.Progress.Quests.Get(Mq43Campaign.NextQuestId).Phase == QuestPhase.Investigation, "MQ44 progress remains intact");

        Console.WriteLine($"MQ43 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool mq42Complete, bool distinction)
    {
        var world = new WorldState();
        world.Initialize();
        if (mq42Complete) world.Progress.SetFlag(Mq42Campaign.Complete);
        if (distinction) world.Progress.SetFlag(Mq41Campaign.NaviaSplotDistinction);
        world.Progress.Quests.Get(Mq43Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
