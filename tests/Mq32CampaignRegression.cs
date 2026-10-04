using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq32CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();
    public static int Run()
    {
        var checks = 0; void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }
        var gated = NewWorld(false); Check(!new Mq32Campaign(gated.Progress).Begin(), "MQ32 is gated by MQ31 completion");
        var world = NewWorld(true); var mq32 = new Mq32Campaign(world.Progress);
        Check(mq32.Begin(), "MQ32 begins after MQ31");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.ArchiveDocuments, true), "archive documents recorded");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.WszeborTestimony, true), "Wszebor testimony recorded");
        Check(!mq32.IsCoreReconstructed, "two categories cannot reconstruct core");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.ClosureGuardRecord, true), "third interested source recorded");
        Check(!mq32.IsCoreReconstructed, "interested sources alone cannot reconstruct core");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.EventEcho, false), "event echo supplies independent evidence");
        Check(mq32.IsCoreReconstructed, "minimum categories plus independent source reconstruct core");
        Check(mq32.MarkContradiction(Mq32EvidenceCategory.ArchiveDocuments, Mq32EvidenceCategory.WszeborTestimony), "contradiction recorded");
        Check(mq32.HasContradiction(Mq32EvidenceCategory.WszeborTestimony, Mq32EvidenceCategory.ArchiveDocuments), "contradiction is order independent");
        Check(mq32.CompleteQuest(), "MQ32 completes");
        Check(world.Progress.Quests.Get(Mq32Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ32 unlocks MQ33");
        Check(!mq32.CompleteQuest(), "completion is idempotent");

        var recovery = NewWorld(true); var recoveryQuest = new Mq32Campaign(recovery.Progress); recoveryQuest.Begin();
        recoveryQuest.RecordEvidence(Mq32EvidenceCategory.ArchiveDocuments, true);
        recoveryQuest.RecordEvidence(Mq32EvidenceCategory.ClosureGuardRecord, true);
        Check(recoveryQuest.RecordEvidence(Mq32EvidenceCategory.AlternativeFieldRecord, false), "field record replaces omitted echo");
        Check(recoveryQuest.IsCoreReconstructed, "recovery combination reconstructs core");

        var partial = NewWorld(true); var partialQuest = new Mq32Campaign(partial.Progress); partialQuest.Begin();
        partialQuest.RecordEvidence(Mq32EvidenceCategory.LivingOrIndirectWitness, false);
        partialQuest.RecordEvidence(Mq32EvidenceCategory.ArchiveDocuments, true);
        partialQuest.MarkContradiction(Mq32EvidenceCategory.LivingOrIndirectWitness, Mq32EvidenceCategory.ArchiveDocuments);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, SaveGameService.Serialize(partial));
        var resumed = new Mq32Campaign(restored.Progress);
        Check(resumed.Begin(), "partial MQ32 resumes after save/load");
        Check(resumed.EvidenceCategoryCount == 2, "partial categories survive save/load");
        Check(resumed.HasIndependentEvidence, "provenance survives save/load");
        Check(resumed.HasContradiction(Mq32EvidenceCategory.ArchiveDocuments, Mq32EvidenceCategory.LivingOrIndirectWitness), "contradiction survives save/load");
        Check(resumed.RecordEvidence(Mq32EvidenceCategory.ClosureGuardRecord, true), "missing category recoverable after restore");
        Check(resumed.IsCoreReconstructed && resumed.CompleteQuest(), "restored MQ32 completes");
        var completed = new WorldState(); completed.Initialize(); SaveGameService.Restore(completed, SaveGameService.Serialize(restored));
        Check(completed.Progress.HasFlag(Mq32Campaign.Complete), "completion survives save/load");
        Check(completed.Progress.HasFlag(Mq32Campaign.CoreReconstructed), "core flag survives save/load");
        Check(completed.Progress.Quests.Get(Mq32Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ33 handoff survives save/load");
        completed.Progress.Quests.Get(Mq32Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!new Mq32Campaign(completed.Progress).CompleteQuest(), "restored completion remains idempotent");
        Check(completed.Progress.Quests.Get(Mq32Campaign.NextQuestId).Phase == QuestPhase.Investigation, "repeat cannot reset MQ33 progress");
        Console.WriteLine($"MQ32 campaign regression checks passed: {checks}"); return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState(); world.Initialize();
        if (ready) world.Progress.SetFlag(Mq31Campaign.Complete);
        world.Progress.Quests.Get(Mq32Campaign.QuestId).SetPhase(QuestPhase.Offered); return world;
    }
}
