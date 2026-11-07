using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public static class ActThreeCampaignSystem
{
    // MQ30 — Pustkowie Pierwszego Progu
    public static readonly Vector3 ThresholdWasteEntranceSpot = new(-450f, 0f, 380f);
    public static readonly Vector3 ArchiveRuinsEntranceSpot = new(-480f, 0f, 410f);
    public static readonly Vector3 ArchiveRecordChamberSpot = new(-485f, 0f, 415f);

    // MQ31 — Archiwum Popiołu (Dokumenty i rejestry)
    public static readonly Vector3 ParentsArchiveSpot = new(-484f, 0f, 417f);
    public static readonly Vector3 WszeborArchiveSpot = new(-486f, 0f, 413f);
    public static readonly Vector3 ClosedThresholdNightRecordSpot = new(-488f, 0f, 416f);

    // MQ32 — Noc Zamkniętego Progu (Echo zdarzeń i relacje)
    public static readonly Vector3 NightOfThresholdEchoSpot = new(-475f, 0f, 405f);
    public static readonly Vector3 ClosureGuardRecordSpot = new(-478f, 0f, 402f);
    public static readonly Vector3 AlternativeFieldRecordSpot = new(-472f, 0f, 408f);

    // MQ33 — Rozwierający (Konfrontacja z Wszeborem)
    public static readonly Vector3 WszeborRetreatSpot = new(-495f, 0f, 425f);

    // MQ34 — Trzy Projekty (Wielka narada i synteza 75% kampanii)
    public static readonly Vector3 ThreeProjectsSynthesisSpot = new(-320f, 0f, 226f);

    public const string MilestoneActThree75Percent = "MILESTONE_ACT_THREE_75_PERCENT";
    public const string TitleArchiveMasterFlag = "TITLE_ARCHIWISTA_PROGOW";

    public const float InteractionDistance = 4.2f;

    // --- MQ30: PUSTKOWIE PIERWSZEGO PROGU ---
    public static bool CanBeginMq30(WorldState world) =>
        world.Progress.HasFlag("MQ25_COMPLETE") &&
        world.Progress.Quests.Get(Mq30Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq30(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq30(world)) return false;
        var mq30 = new Mq30Campaign(world.Progress);
        return mq30.Begin();
    }

    public static bool CanRecordArchiveTrace(WorldState world, Mq30ArchiveTrace trace)
    {
        var mq30 = new Mq30Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq30Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag("MQ30_ARCHIVE_TRACE_" + trace.ToString().ToUpperInvariant());
    }

    public static bool RecordArchiveTrace(WorldState world, Mq30ArchiveTrace trace)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanRecordArchiveTrace(world, trace)) return false;
        var mq30 = new Mq30Campaign(world.Progress);
        return mq30.RecordArchiveTrace(trace);
    }

    public static bool CanEstablishSafeRoute(WorldState world)
    {
        var mq30 = new Mq30Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq30Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq30.HasRequiredTraces &&
               !world.Progress.HasFlag(Mq30Campaign.RouteKnown);
    }

    public static bool EstablishSafeRoute(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanEstablishSafeRoute(world)) return false;
        var mq30 = new Mq30Campaign(world.Progress);
        var established = mq30.EstablishSafeRoute();
        if (!established) return false;

        var completed = mq30.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("forest-resin", 5);
            world.Progress.Inventory.Add("simple-bandage", 2);
            world.Progress.Inventory.Add("arrow-iron", 15);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 25);
            world.Progress.Reputation.Change(ReputationScope.Village, "osada-smolnikow", 20);
        }
        return completed;
    }

    // --- MQ31: ARCHIWUM POPIOŁU ---
    public static bool CanBeginMq31(WorldState world) =>
        world.Progress.HasFlag(Mq30Campaign.Complete) &&
        world.Progress.Quests.Get(Mq31Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq31(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq31(world)) return false;
        var mq31 = new Mq31Campaign(world.Progress);
        return mq31.Begin();
    }

    public static bool CanRecordArchiveEvidence(WorldState world, Mq31ArchiveFact fact)
    {
        var mq31 = new Mq31Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq31Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !mq31.HasFact(fact);
    }

    public static bool RecordArchiveEvidence(
        WorldState world,
        Mq31ArchiveFact fact,
        Mq31RecordChannel channel = Mq31RecordChannel.PrimaryDocument)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanRecordArchiveEvidence(world, fact)) return false;
        var mq31 = new Mq31Campaign(world.Progress);
        return mq31.RecordArchiveEvidence(fact, channel);
    }

    public static bool CanConcludeMq31(WorldState world)
    {
        var mq31 = new Mq31Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq31Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq31.HasAllCriticalFacts &&
               !world.Progress.HasFlag(Mq31Campaign.Complete);
    }

    public static bool ConcludeMq31(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanConcludeMq31(world)) return false;
        var mq31 = new Mq31Campaign(world.Progress);
        var completed = mq31.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("iron-chunk", 5);
            world.Progress.Inventory.Add("old-coin", 50);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 30);
            world.Progress.Reputation.Change(ReputationScope.Village, "old-village", 20);
        }
        return completed;
    }

    // --- MQ32: NOC ZAMKNIĘTEGO PROGU ---
    public static bool CanBeginMq32(WorldState world) =>
        world.Progress.HasFlag(Mq31Campaign.Complete) &&
        world.Progress.Quests.Get(Mq32Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq32(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq32(world)) return false;
        var mq32 = new Mq32Campaign(world.Progress);
        return mq32.Begin();
    }

    public static bool CanRecordCrisisEvidence(WorldState world, Mq32EvidenceCategory category)
    {
        var mq32 = new Mq32Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq32Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !mq32.HasEvidence(category);
    }

    public static bool RecordCrisisEvidence(WorldState world, Mq32EvidenceCategory category, bool fromInterestedFaction)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanRecordCrisisEvidence(world, category)) return false;
        var mq32 = new Mq32Campaign(world.Progress);
        return mq32.RecordEvidence(category, fromInterestedFaction);
    }

    public static bool CanMarkContradiction(WorldState world, Mq32EvidenceCategory first, Mq32EvidenceCategory second)
    {
        var mq32 = new Mq32Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq32Campaign.QuestId).Phase == QuestPhase.Investigation &&
               first != second &&
               mq32.HasEvidence(first) &&
               mq32.HasEvidence(second) &&
               !mq32.HasContradiction(first, second);
    }

    public static bool MarkContradiction(WorldState world, Mq32EvidenceCategory first, Mq32EvidenceCategory second)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanMarkContradiction(world, first, second)) return false;
        var mq32 = new Mq32Campaign(world.Progress);
        return mq32.MarkContradiction(first, second);
    }

    public static bool CanConcludeMq32(WorldState world)
    {
        var mq32 = new Mq32Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq32Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq32.IsCoreReconstructed &&
               !world.Progress.HasFlag(Mq32Campaign.Complete);
    }

    public static bool ConcludeMq32(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanConcludeMq32(world)) return false;
        var mq32 = new Mq32Campaign(world.Progress);
        var completed = mq32.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("forest-resin", 5);
            world.Progress.Inventory.Add("iron-chunk", 5);
            world.Progress.Inventory.Add("old-coin", 50);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 35);
            world.Progress.Reputation.Change(ReputationScope.Village, "przystan-rybacka", 20);
        }
        return completed;
    }

    // --- MQ33: ROZWIERAJĄCY ---
    public static bool CanBeginMq33(WorldState world) =>
        world.Progress.HasFlag(Mq32Campaign.Complete) &&
        world.Progress.Quests.Get(Mq33Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq33(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq33(world)) return false;
        var mq33 = new Mq33Campaign(world.Progress);
        return mq33.Begin();
    }

    public static bool CanRecordWszeborPosition(WorldState world)
    {
        var mq33 = new Mq33Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq33Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag(Mq33Campaign.PositionKnown);
    }

    public static bool RecordWszeborPosition(WorldState world, Mq33WszeborStance stance = Mq33WszeborStance.Guarded)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanRecordWszeborPosition(world)) return false;
        var mq33 = new Mq33Campaign(world.Progress);
        return mq33.RecordWszeborPosition(stance);
    }

    public static bool CanResolveWszeborConfrontation(WorldState world)
    {
        var mq33 = new Mq33Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq33Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag(Mq33Campaign.Complete);
    }

    public static bool ResolveWszeborConfrontation(
        WorldState world,
        Mq33ConfrontationOutcome outcome = Mq33ConfrontationOutcome.Conversation,
        Mq33WszeborStance stance = Mq33WszeborStance.Cooperative,
        bool directDataAvailable = true)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanResolveWszeborConfrontation(world)) return false;
        var mq33 = new Mq33Campaign(world.Progress);
        var resolved = mq33.ResolveConfrontation(outcome, stance, directDataAvailable);
        if (resolved)
        {
            world.Progress.Inventory.Add("arrow-iron", 30);
            world.Progress.Inventory.Add("simple-bandage", 4);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 40);
            world.Progress.Reputation.Change(ReputationScope.Village, "old-village", 25);
            world.Progress.Reputation.Change(ReputationScope.Village, "osada-smolnikow", 25);
        }
        return resolved;
    }

    // --- MQ34: TRZY PROJEKTY (75% KAMPANII) ---
    public static bool CanBeginMq34(WorldState world) =>
        world.Progress.HasFlag(Mq33Campaign.Complete) &&
        world.Progress.Quests.Get(Mq34Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq34(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq34(world)) return false;
        var mq34 = new Mq34Campaign(world.Progress);
        return mq34.Begin();
    }

    public static bool CanPresentModel(WorldState world, Mq34CrisisModel model)
    {
        var mq34 = new Mq34Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq34Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !mq34.HasModel(model);
    }

    public static bool PresentModel(WorldState world, Mq34CrisisModel model, bool optionalDetailAvailable = true)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanPresentModel(world, model)) return false;
        var mq34 = new Mq34Campaign(world.Progress);
        return mq34.PresentModel(model, optionalDetailAvailable);
    }

    public static bool CanConcludeActThree(WorldState world)
    {
        var mq34 = new Mq34Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq34Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq34.AllModelsKnown &&
               !world.Progress.HasFlag(Mq34Campaign.Complete);
    }

    public static bool ConcludeActThree(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanConcludeActThree(world)) return false;
        var mq34 = new Mq34Campaign(world.Progress);
        var completed = mq34.CompleteQuest();
        if (completed)
        {
            world.Progress.SetFlag(MilestoneActThree75Percent);
            world.Progress.SetFlag(TitleArchiveMasterFlag);
            world.Progress.Inventory.Add("armor-reinforced-leather", 1);
            world.Progress.Inventory.Add("iron-chunk", 10);
            world.Progress.Inventory.Add("forest-resin", 10);
            world.Progress.Inventory.Add("arrow-iron", 50);
            world.Progress.Inventory.Add("old-coin", 150);
            world.Progress.Profile.ChangeMoney(150);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 60);
            world.Progress.Reputation.Change(ReputationScope.Village, "old-village", 40);
            world.Progress.Reputation.Change(ReputationScope.Village, "przystan-rybacka", 40);
            world.Progress.Reputation.Change(ReputationScope.Village, "osada-smolnikow", 40);
        }
        return completed;
    }
}
