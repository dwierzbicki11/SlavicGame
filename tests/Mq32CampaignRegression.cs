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
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(ready: false);
        Check(!new Mq32Campaign(gated.Progress).Begin(), "MQ32 is gated by MQ31 completion");

        var world = NewWorld(ready: true);
        var mq32 = new Mq32Campaign(world.Progress);
        Check(mq32.Begin(), "MQ32 begins after MQ31");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.ArchiveDocuments, true), "archive documents can be recorded");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.WszeborTestimony, true), "Wszebor testimony can be recorded independently");
        Check(!mq32.IsCoreReconstructed, "two categories cannot reconstruct the core");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.ClosureGuardRecord, true), "third interested source can be recorded");
        Check(!mq32.IsCoreReconstructed, "three interested sources do not satisfy independent-source rule");
        Check(mq32.RecordEvidence(Mq32EvidenceCategory.EventEcho, false), "event echo supplies independent evidence");
        Check(mq32.IsCoreReconstructed, "three categories plus independent evidence reconstruct the core");
        Check(mq32.MarkContradiction(Mq32EvidenceCategory.ArchiveDocuments, Mq32EvidenceCategory.WszeborTestimony), "contradiction can be recorded between collected sources");
        Check(mq32.HasContradiction(Mq32EvidenceCategory.WszeborTestimony, Mq32EvidenceCategory.ArchiveDocuments), "contradiction identity is order independent");
        Check(mq32.CompleteQuest(), "MQ32 completes after core reconstruction");
        Check(world.Progress.Quests.Get(Mq32Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ32 unlocks MQ33");
        Check(!mq32.CompleteQuest(), "MQ32 completion is idempotent");

        var recovery = NewWorld(ready: true);
        var recoveryQuest = new Mq32Campaign(recovery.Progress);
        recoveryQuest.Begin();
        Check(recoveryQuest.RecordEvidence(Mq32EvidenceCategory.ArchiveDocuments, true), "recovery path records archive documents");
        Check(recoveryQuest.RecordEvidence(Mq32EvidenceCategory.ClosureGuardRecord, true), "recovery path records guard record");
        Check(recoveryQuest.RecordEvidence(Mq32EvidenceCategory.AlternativeFieldRecord, false), "field record replaces omitted echo as independent third category");
        Check(recoveryQuest.IsCoreReconstructed, "documents plus guard plus field record recover critical core");

        var partial = NewWorld(ready: true);
        var partialQuest = new Mq32Campaign(partial.Progress);
        partialQuest.Begin();
        partialQuest.RecordEvidence(Mq32EvidenceCategory.LivingOrIndirectWitness, false);
        partialQuest.RecordEvidence(Mq32EvidenceCategory.ArchiveDocuments, true);
        partialQuest.MarkContradiction(Mq32EvidenceCategory.LivingOrIndirectWitness, Mq32EvidenceCategory.ArchiveDocuments);
        var json = SaveGameService.Serialize(partial);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var resumed = new Mq32Campaign(restored.Progress);
        Check(resumed.Begin(), "partial MQ32 resumes after save/load");
        Check(resumed.EvidenceCategoryCount == 2, "partial evidence categories survive save/load");
        Check(resumed.HasIndependentEvidence, "independent-source provenance survives save/load");
        Check(resumed.HasContradiction(Mq32EvidenceCategory.ArchiveDocuments, Mq32EvidenceCategory.LivingOrIndirectWitness), "contradiction survives save/load");
        Check(resumed.RecordEvidence(Mq32EvidenceCategory.ClosureGuardRecord, true), "missing category remains recoverable after restore");
        Check(resumed.IsCoreReconstructed, "restored partial reconstruction can finish");
        Check(resumed.CompleteQuest(), "restored MQ32 completes");

        var completedRestored = new WorldState(); completedRestored.Initialize();
        SaveGameService.Restore(completedRestored, SaveGameService.Serialize(restored));
        Check(completedRestored.Progress.HasFlag(Mq32Campaign.Complete), "MQ32 completion survives save/load");
        Check(completedRestored.Progress.HasFlag(Mq32Campaign.CoreReconstructed), "core reconstruction survives save/load");
        Check(completedRestored.Progress.Quests.Get(Mq32Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ33 handoff survives save/load");
        completedRestored.Progress.Quests.Get(Mq32Campaign.NextQuestId).SetPhase(QuestPhase.Investigation);
        Check(!new Mq32Campaign(completedRestored.Progress).CompleteQuest(), "restored completion remains idempotent");
        Check(completedRestored.Progress.Quests.Get(Mq32Campaign.NextQuestId).Phase == QuestPhase.Investigation, "repeated completion cannot reset MQ33 progress");

        Console.WriteLine($"MQ32 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState();
        world.Initialize();
        if (ready) world.Progress.SetFlag(Mq31Campaign.Complete);
        world.Progress.Quests.Get(Mq32Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
