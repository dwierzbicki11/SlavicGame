using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq31CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(ready: false);
        Check(!new Mq31Campaign(gated.Progress).Begin(), "MQ31 is gated by MQ30 completion");

        var world = NewWorld(ready: true);
        var mq31 = new Mq31Campaign(world.Progress);
        Check(mq31.Begin(), "MQ31 begins after MQ30");
        Check(mq31.RecordArchiveEvidence(Mq31ArchiveFact.WszeborArchiveLink, Mq31RecordChannel.SecondaryRegister), "secondary register can recover Wszebor fact");
        Check(mq31.RecordArchiveEvidence(Mq31ArchiveFact.ParentsArchiveRole, Mq31RecordChannel.PrimaryDocument), "primary parent record can be read independently");
        Check(!mq31.CompleteQuest(), "two facts cannot complete MQ31");
        Check(mq31.RecordArchiveEvidence(Mq31ArchiveFact.ClosedThresholdNightReference, Mq31RecordChannel.SecondaryRegister), "secondary register can recover night reference");
        Check(world.Progress.HasFlag(Mq31Campaign.ArchiveRecordsFound), "archive records discovery is durable");
        Check(mq31.HasAllCriticalFacts, "all three critical facts are independent of reading order");
        Check(mq31.CompleteQuest(), "MQ31 completes after all critical facts");
        Check(world.Progress.Quests.Get(Mq31Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ31 unlocks MQ32");
        Check(!mq31.CompleteQuest(), "MQ31 completion is idempotent");

        var persistent = NewWorld(ready: true);
        var campaign = new Mq31Campaign(persistent.Progress);
        campaign.Begin();
        campaign.RecordArchiveEvidence(Mq31ArchiveFact.ParentsArchiveRole, Mq31RecordChannel.SecondaryRegister);
        campaign.RecordArchiveEvidence(Mq31ArchiveFact.ClosedThresholdNightReference, Mq31RecordChannel.PrimaryDocument);
        var json = SaveGameService.Serialize(persistent);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var resumed = new Mq31Campaign(restored.Progress);
        Check(resumed.Begin(), "MQ31 resumes after save/load");
        Check(resumed.HasFact(Mq31ArchiveFact.ParentsArchiveRole), "parent fact survives restore");
        Check(resumed.HasFact(Mq31ArchiveFact.ClosedThresholdNightReference), "night reference survives restore");
        Check(resumed.RecordArchiveEvidence(Mq31ArchiveFact.WszeborArchiveLink, Mq31RecordChannel.PrimaryDocument), "missing fact remains recoverable after restore");
        Check(resumed.CompleteQuest(), "restored MQ31 completes");
        Check(restored.Progress.HasFlag(Mq31Campaign.ParentsArchiveRoleKnown), "parent role durable flag is set");
        Check(restored.Progress.HasFlag(Mq31Campaign.WszeborArchiveLinkKnown), "Wszebor link durable flag is set");
        Check(restored.Progress.HasFlag(Mq31Campaign.ClosedThresholdNightReferenceKnown), "night reference durable flag is set");

        var completedRestored = new WorldState(); completedRestored.Initialize();
        SaveGameService.Restore(completedRestored, SaveGameService.Serialize(restored));
        Check(completedRestored.Progress.HasFlag(Mq31Campaign.Complete), "MQ31 completion survives save/load");
        Check(completedRestored.Progress.Quests.Get(Mq31Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ32 handoff survives save/load");
        completedRestored.Progress.Quests.Get(Mq31Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!new Mq31Campaign(completedRestored.Progress).CompleteQuest(), "restored completion remains idempotent");
        Check(completedRestored.Progress.Quests.Get(Mq31Campaign.NextQuestId).Phase == QuestPhase.Investigation, "repeated completion cannot reset MQ32 progress");

        Console.WriteLine($"MQ31 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState();
        world.Initialize();
        if (ready) world.Progress.SetFlag("MQ30_COMPLETE");
        world.Progress.Quests.Get(Mq31Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
