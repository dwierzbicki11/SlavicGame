using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq42CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();
    public static int Run()
    {
        var checks = 0; void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }
        var gated = NewWorld(false); Check(!new Mq42Campaign(gated.Progress).Begin(), "MQ42 is gated by MQ41 completion");
        var meeting = NewWorld(true); var mq42 = new Mq42Campaign(meeting.Progress); Check(mq42.Begin(), "MQ42 begins after MQ41"); Check(!mq42.VerifyIdentity(), "identity requires finding Parent B"); Check(mq42.FindParentB(), "Parent B pattern can be found"); Check(mq42.VerifyIdentity(), "identity can be verified from prior details"); Check(!mq42.CompleteQuest(), "meeting path requires a decision");
        var midSave = SaveGameService.Serialize(meeting); var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, midSave); var resumed = new Mq42Campaign(restored.Progress); Check(restored.Progress.HasFlag(Mq42Campaign.ParentBFound) && restored.Progress.HasFlag(Mq42Campaign.IdentityVerified), "meeting evidence survives save/load"); Check(resumed.RecordParentBDecision(), "relationship/promise decision is durable"); Check(!resumed.Skip(), "skip cannot contradict a Parent B decision"); Check(resumed.CompleteQuest(), "meeting path completes MQ42"); Check(restored.Progress.Quests.Get(Mq42Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ43 unlocks after meeting path"); Check(!resumed.CompleteQuest(), "meeting completion is idempotent");
        var skipped = NewWorld(true); var skipMq42 = new Mq42Campaign(skipped.Progress); Check(skipMq42.Begin(), "skip path begins normally"); Check(skipMq42.Skip(), "player may explicitly skip Parent B"); Check(!skipMq42.FindParentB(), "skip prevents contradictory meeting state"); Check(!skipMq42.RecordParentBDecision(), "skip and decision are mutually exclusive"); Check(skipMq42.CompleteQuest(), "skip path completes without softlock"); Check(skipped.Progress.HasFlag(Mq42Campaign.Skipped), "skip state is durable"); Check(skipped.Progress.Quests.Get(Mq42Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ43 unlocks after skip");
        var skipSave = SaveGameService.Serialize(skipped); var skipRestored = new WorldState(); skipRestored.Initialize(); SaveGameService.Restore(skipRestored, skipSave); Check(skipRestored.Progress.HasFlag(Mq42Campaign.Skipped) && skipRestored.Progress.HasFlag(Mq42Campaign.Complete), "skip completion survives save/load"); skipRestored.Progress.Quests.Get(Mq42Campaign.NextQuestId).SetPhase(QuestPhase.Investigation); Check(!new Mq42Campaign(skipRestored.Progress).CompleteQuest(), "repeat completion cannot reset MQ43 progress"); Check(skipRestored.Progress.Quests.Get(Mq42Campaign.NextQuestId).Phase == QuestPhase.Investigation, "MQ43 progress remains intact");
        Console.WriteLine($"MQ42 campaign regression checks passed: {checks}"); return checks;
    }
    private static WorldState NewWorld(bool ready) { var world = new WorldState(); world.Initialize(); if (ready) world.Progress.SetFlag(Mq41Campaign.Complete); world.Progress.Quests.Get(Mq42Campaign.QuestId).SetPhase(QuestPhase.Offered); return world; }
}
