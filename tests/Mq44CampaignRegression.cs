using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq44CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(mq43Complete: false, truthKnown: true, anchor: true);
        Check(!new Mq44Campaign(gated.Progress).Begin(), "MQ44 requires MQ43 completion");
        var missingTruth = NewWorld(mq43Complete: true, truthKnown: false, anchor: true);
        Check(!new Mq44Campaign(missingTruth.Progress).Begin(), "MQ44 requires Splot truth");
        var missingAnchor = NewWorld(mq43Complete: true, truthKnown: true, anchor: false);
        Check(!new Mq44Campaign(missingAnchor.Progress).Begin(), "MQ44 requires MQ40 return anchor");

        foreach (var parentBMet in new[] { false, true })
        {
            var world = NewWorld(mq43Complete: true, truthKnown: true, anchor: true);
            if (parentBMet) world.Progress.SetFlag(Mq42Campaign.ParentBDecision);
            else world.Progress.SetFlag(Mq42Campaign.Skipped);
            var mq44 = new Mq44Campaign(world.Progress);
            Check(mq44.Begin(), "MQ44 begins for either MQ42 outcome");
            Check(mq44.ReturnToJawia(), "return through anchor succeeds");
            Check(mq44.EscalateCrisis(), "crisis escalation follows return");

            var midSave = SaveGameService.Serialize(world);
            var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, midSave);
            var resumed = new Mq44Campaign(restored.Progress);
            Check(restored.Progress.HasFlag(Mq44Campaign.ReturnedToJawia) && restored.Progress.HasFlag(Mq44Campaign.CrisisEscalated), "return and escalation survive save/load");
            Check(resumed.RecoverCriticalMessages(), "critical message fallback provides final needs");
            Check(restored.Progress.HasFlag(Mq44Campaign.FinalNeedsKnown), "final needs are durable");
            Check(resumed.CompleteQuest(), "MQ44 completes after final-needs synthesis");
            Check(restored.Progress.Quests.Get(Mq44Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ50 unlocks after MQ44");
            Check(!resumed.CompleteQuest(), "MQ44 completion is idempotent");
            restored.Progress.Quests.Get(Mq44Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
            Check(!resumed.CompleteQuest(), "repeat completion cannot reset MQ50 progress");
            Check(restored.Progress.Quests.Get(Mq44Campaign.NextQuestId).Phase == QuestPhase.Investigation, "MQ50 progress remains intact");
        }

        Console.WriteLine($"MQ44 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool mq43Complete, bool truthKnown, bool anchor)
    {
        var world = new WorldState(); world.Initialize();
        if (mq43Complete) world.Progress.SetFlag(Mq43Campaign.Complete);
        if (truthKnown) world.Progress.SetFlag(Mq43Campaign.SplotTruthKnown);
        if (anchor) world.Progress.SetFlag(Mq40Campaign.ReturnAnchorSet);
        world.Progress.Quests.Get(Mq44Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
