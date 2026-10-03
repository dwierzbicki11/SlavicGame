using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public sealed record QuestWorldInteraction(
    string Id,
    Vector3 Position,
    InteractionKind Kind,
    string Prompt);

public enum QuestInteractionResult
{
    None,
    Completed,
    Blocked
}

public sealed class VerticalSliceQuestInteractions
{
    public const string ContractAcceptedFlag = "light-over-swamp.contract-accepted";
    public const string CrossingInspectedFlag = "light-over-swamp.crossing-inspected";
    public const string PredatorTracksFlag = "light-over-swamp.predator-tracks-found";
    public const string KeepsakeCollectedFlag = "light-over-swamp.keepsake-collected";
    public const string AnomalyObservedFlag = "light-over-swamp.anomaly-observed";
    public const string OwnerConfirmedFlag = "light-over-swamp.owner-confirmed";
    public const string ShrineLessonFlag = "light-over-swamp.shrine-lesson";
    public const string ThreadReceivedFlag = "light-over-swamp.ritual-thread-received";
    public const string TurnedInFlag = "light-over-swamp.turned-in";

    private const float InteractionDistance = 4f;

    private static readonly Vector3 FamilySpot = new(-6f, 0f, -84f);
    private static readonly Vector3 CrossingSpot = new(91f, 0f, 41f);
    private static readonly Vector3 TracksSpot = new(98f, 0f, 34f);
    private static readonly Vector3 KeepsakeSpot = new(82f, 0f, 29f);
    private static readonly Vector3 AnomalySpot = new(78f, 0f, 24f);
    private static readonly Vector3 ShrineKeeperSpot = new(-82f, 0f, 58f);
    private static readonly Vector3 HerbalistSpot = new(5f, 0f, -92f);

    public QuestWorldInteraction? Current { get; private set; }
    public string Message { get; private set; } = "";
    public string HudText => Current?.Prompt ?? Message;

    public void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        Current = FindNearestAvailable(world);
    }

    public QuestInteractionResult TryInteract(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        Current = FindNearestAvailable(world);
        if (Current is null)
        {
            Message = "BRAK INTERAKCJI W ZASIEGU";
            return QuestInteractionResult.None;
        }

        var result = Current.Id switch
        {
            "quest.family" => InteractFamily(world),
            "quest.crossing" => InspectCrossing(world),
            "quest.predator-tracks" => InspectPredatorTracks(world),
            "quest.keepsake" => TakeKeepsake(world),
            "quest.anomaly" => ObserveAnomaly(world),
            "quest.shrine-keeper" => LearnAtShrine(world),
            "quest.herbalist-thread" => ReceiveRitualThread(world),
            _ => QuestInteractionResult.None
        };

        Current = FindNearestAvailable(world);
        return result;
    }

    private QuestWorldInteraction? FindNearestAvailable(WorldState world)
    {
        var candidates = BuildAvailable(world);
        return InteractionSystem.FindNearest(
            world.PlayerPosition,
            candidates.Select(item => new InteractionTarget(
                item.Id,
                Ground(world, item.Position),
                item.Kind,
                item.Prompt)),
            InteractionDistance) is { } nearest
                ? candidates.First(item => item.Id == nearest.Id)
                : null;
    }

    private static List<QuestWorldInteraction> BuildAvailable(WorldState world)
    {
        var result = new List<QuestWorldInteraction>();
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);

        if (quest.Phase == QuestPhase.Offered ||
            (quest.Phase == QuestPhase.Resolved && !quest.RewardClaimed) ||
            (world.Progress.Inventory.Contains("missing-person-keepsake") &&
             !HasEvidence(quest, "light-over-swamp.keepsake-owner")))
        {
            var familyPrompt = quest.Phase == QuestPhase.Resolved
                ? "E ZAKONCZ KONTRAKT Z RODZINA"
                : world.Progress.Inventory.Contains("missing-person-keepsake") &&
                  !HasEvidence(quest, "light-over-swamp.keepsake-owner")
                    ? "E POKAZ PAMIATKE RODZINIE"
                    : "E PRZYJMIJ KONTRAKT OD RODZINY";

            result.Add(new QuestWorldInteraction(
                "quest.family",
                FamilySpot,
                InteractionKind.Talk,
                familyPrompt));
        }

        if (quest.Phase is QuestPhase.Active or QuestPhase.Investigation)
        {
            if (!world.Progress.HasFlag(CrossingInspectedFlag))
                result.Add(new QuestWorldInteraction(
                    "quest.crossing",
                    CrossingSpot,
                    InteractionKind.Inspect,
                    "E OBEJRZYJ USZKODZONA KLADKE"));

            if (!world.Progress.HasFlag(PredatorTracksFlag))
                result.Add(new QuestWorldInteraction(
                    "quest.predator-tracks",
                    TracksSpot,
                    InteractionKind.Inspect,
                    "E ZBADAJ SLADY DRAPIEZNIKA"));

            if (!world.Progress.HasFlag(KeepsakeCollectedFlag))
                result.Add(new QuestWorldInteraction(
                    "quest.keepsake",
                    KeepsakeSpot,
                    InteractionKind.Take,
                    "E PODNIES PAMIATKE"));

            if (!world.Progress.HasFlag(AnomalyObservedFlag))
                result.Add(new QuestWorldInteraction(
                    "quest.anomaly",
                    AnomalySpot,
                    InteractionKind.Inspect,
                    IsNight(world.Time.TimeOfDayHours)
                        ? "E OBSERWUJ NOCNA ANOMALIE"
                        : "ANOMALIA UJAWNIA SIE PO ZMIERZCHU"));
        }

        if (quest.Phase == QuestPhase.Preparation &&
            HasEvidence(quest, "light-over-swamp.keepsake-owner") &&
            !world.Progress.HasFlag(VerticalSliceRituals.AnchorKnowledgeFlag) &&
            NpcIsAt(world, "shrine-keeper", "old-shrine"))
        {
            result.Add(new QuestWorldInteraction(
                "quest.shrine-keeper",
                ShrineKeeperSpot,
                InteractionKind.Talk,
                "E ZAPYTAJ O PAMIATKE I RYTUAL"));
        }

        if (quest.Phase == QuestPhase.Preparation &&
            world.Progress.HasFlag(VerticalSliceRituals.LearnedFlag) &&
            !world.Progress.Inventory.Contains("ritual-thread") &&
            !world.Progress.HasFlag(ThreadReceivedFlag))
        {
            result.Add(new QuestWorldInteraction(
                "quest.herbalist-thread",
                HerbalistSpot,
                InteractionKind.Talk,
                "E POPROS ZIELARKE O NIC OBRZEDOWA"));
        }

        return result;
    }

    private QuestInteractionResult InteractFamily(WorldState world)
    {
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);

        if (quest.Phase == QuestPhase.Resolved && !quest.RewardClaimed)
        {
            if (!quest.ClaimReward())
            {
                Message = "NAGRODA NIE JEST DOSTEPNA";
                return QuestInteractionResult.Blocked;
            }

            var predatorKilled = SwampPredatorEncounter.IsKilled(world);
            var moneyReward = predatorKilled ? 60 : 40;
            var reputationReward = predatorKilled ? 12 : 8;

            world.Progress.Profile.ChangeMoney(moneyReward);
            world.Progress.Reputation.Change(
                ReputationScope.Village,
                "old-village",
                reputationReward);
            world.Progress.Relationships.Change(
                "missing-family",
                RelationshipKind.Trust,
                10);
            world.Progress.SetFlag(TurnedInFlag);

            Message = predatorKilled
                ? "KONTRAKT ZAKONCZONY / ZJAWA UWOLNIONA / DRAPIEZNIK USUNIETY / NAGRODA 60 / REPUTACJA +12"
                : "KONTRAKT ZAKONCZONY / ZJAWA UWOLNIONA / DRAPIEZNIK NADAL ZAGRAZA / NAGRODA 40 / REPUTACJA +8";
            return QuestInteractionResult.Completed;
        }

        if (world.Progress.Inventory.Contains("missing-person-keepsake") &&
            !HasEvidence(quest, "light-over-swamp.keepsake-owner"))
        {
            quest.AddEvidence(new EvidenceEntry(
                "light-over-swamp.keepsake-owner",
                quest.Id,
                KnowledgeKind.ConfirmedFact,
                "Rodzina potwierdzila, ze pamiatka nalezala do zaginionego.",
                "missing-family"));
            world.Progress.SetFlag(OwnerConfirmedFlag);
            world.Progress.Relationships.Change("missing-family", RelationshipKind.Trust, 4);
            Message = "TO JEGO PAMIATKA / TOZSAMOSC POTWIERDZONA";
            return QuestInteractionResult.Completed;
        }

        if (quest.Phase != QuestPhase.Offered)
        {
            Message = "RODZINA NIE MA TERAZ NOWEJ INFORMACJI";
            return QuestInteractionResult.Blocked;
        }

        quest.SetPhase(QuestPhase.Active);
        quest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.witness-light",
            quest.Id,
            KnowledgeKind.Rumor,
            "Po zmierzchu widziano swiatlo przy przeprawie.",
            "missing-family"));
        quest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.last-route",
            quest.Id,
            KnowledgeKind.ConfirmedFact,
            "Zaginiony mial przejsc przez mokradla.",
            "missing-family"));
        world.Progress.SetFlag(ContractAcceptedFlag);
        Message = "KONTRAKT PRZYJETY / ZBADAJ PRZEPRAWE";
        return QuestInteractionResult.Completed;
    }

    private QuestInteractionResult InspectCrossing(WorldState world)
    {
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        if (quest.Phase == QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        quest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.broken-planks",
            quest.Id,
            KnowledgeKind.Observation,
            "Czesc przeprawy jest fizycznie uszkodzona.",
            "crossing"));
        world.Progress.SetFlag(CrossingInspectedFlag);
        Message = "KLADKA USZKODZONA / TO NIE TYLKO ZJAWA";
        AdvanceInvestigation(world);
        return QuestInteractionResult.Completed;
    }

    private QuestInteractionResult InspectPredatorTracks(WorldState world)
    {
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        if (quest.Phase == QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        quest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.predator-tracks",
            quest.Id,
            KnowledgeKind.Observation,
            "Tropy duzej istoty prowadza wzdluz mokradla.",
            "swamp-predator"));
        world.Progress.SetFlag(PredatorTracksFlag);
        world.Progress.Encounters.Get(SwampPredatorEncounter.Id).Observe();
        Message = "TROPY POTWIERDZAJA FIZYCZNE ZAGROZENIE";
        AdvanceInvestigation(world);
        return QuestInteractionResult.Completed;
    }

    private QuestInteractionResult TakeKeepsake(WorldState world)
    {
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        if (quest.Phase == QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        if (!world.Progress.Inventory.Contains("missing-person-keepsake"))
            world.Progress.Inventory.Add("missing-person-keepsake");

        quest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.keepsake",
            quest.Id,
            KnowledgeKind.Observation,
            "Przy miejscu zdarzenia znaleziono osobista pamiatke.",
            "black-swamp"));
        world.Progress.SetFlag(KeepsakeCollectedFlag);
        Message = "ZDOBYTO: PAMIATKA ZAGINIONEGO";
        AdvanceInvestigation(world);
        return QuestInteractionResult.Completed;
    }

    private QuestInteractionResult ObserveAnomaly(WorldState world)
    {
        if (!IsNight(world.Time.TimeOfDayHours))
        {
            Message = "WRÓC PO ZMIERZCHU";
            return QuestInteractionResult.Blocked;
        }

        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        if (quest.Phase == QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        quest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.apparition-response",
            quest.Id,
            KnowledgeKind.Observation,
            "Nocne swiatlo reaguje na obecnosc przy przeprawie.",
            "black-swamp-leak"));
        world.Progress.SetFlag(AnomalyObservedFlag);
        Message = "ANOMALIA POTWIERDZONA / TO NIE JEST ZWYKLE SWIATLO";
        AdvanceInvestigation(world);
        return QuestInteractionResult.Completed;
    }

    private QuestInteractionResult LearnAtShrine(WorldState world)
    {
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        if (quest.Phase != QuestPhase.Preparation ||
            !HasEvidence(quest, "light-over-swamp.keepsake-owner"))
        {
            Message = "BRAK WYSTARCZAJACEJ WIEDZY O PAMIATCE";
            return QuestInteractionResult.Blocked;
        }

        world.Progress.SetFlag(VerticalSliceRituals.AnchorKnowledgeFlag);
        world.Progress.SetFlag(VerticalSliceRituals.LearnedFlag);
        world.Progress.SetFlag(VerticalSliceRituals.IdentitySignFlag);
        world.Progress.SetFlag(VerticalSliceRituals.BoundarySignFlag);
        world.Progress.SetFlag(ShrineLessonFlag);
        world.Cinematics.TryStartById(world, "ritual-preparation");
        Message = "POZNANO RYTUAL / ZNAKI: TOZSAMOSC I GRANICA";
        return QuestInteractionResult.Completed;
    }

    private QuestInteractionResult ReceiveRitualThread(WorldState world)
    {
        if (!world.Progress.HasFlag(VerticalSliceRituals.LearnedFlag))
        {
            Message = "NAJPIERW MUSISZ POZNAC RYTUAL";
            return QuestInteractionResult.Blocked;
        }

        if (!world.Progress.Inventory.Contains("ritual-thread"))
            world.Progress.Inventory.Add("ritual-thread");

        world.Progress.SetFlag(ThreadReceivedFlag);
        Message = "ZDOBYTO: NIC OBRZEDOWA";
        return QuestInteractionResult.Completed;
    }

    private static void AdvanceInvestigation(WorldState world)
    {
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        if (quest.Phase != QuestPhase.Investigation)
            return;

        var hasRequiredEvidence =
            world.Progress.Inventory.Contains("missing-person-keepsake") &&
            HasEvidence(quest, "light-over-swamp.predator-tracks") &&
            HasEvidence(quest, "light-over-swamp.apparition-response");

        if (hasRequiredEvidence)
            quest.SetPhase(QuestPhase.Preparation);
    }

    private static bool NpcIsAt(WorldState world, string npcId, string locationId)
    {
        var npc = world.Npcs.FirstOrDefault(item => item.Id == npcId);
        return string.Equals(
            npc?.GetSchedule(world.Time.TimeOfDayHours)?.LocationId,
            locationId,
            StringComparison.Ordinal);
    }

    private static bool HasEvidence(QuestRecord quest, string evidenceId) =>
        quest.Evidence.Any(entry => string.Equals(entry.Id, evidenceId, StringComparison.Ordinal));

    private static bool IsNight(double hour) =>
        hour >= 20.0 || hour < 6.0;

    private static Vector3 Ground(WorldState world, Vector3 position)
    {
        position.Y = world.Terrain.SampleHeight(position);
        return position;
    }
}
