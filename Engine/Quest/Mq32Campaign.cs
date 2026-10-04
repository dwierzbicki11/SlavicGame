using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq32EvidenceCategory
{
    ArchiveDocuments,
    EventEcho,
    LivingOrIndirectWitness,
    WszeborTestimony,
    ClosureGuardRecord,
    AlternativeFieldRecord
}

public sealed class Mq32Campaign
{
    public const string QuestId = "MQ32";
    public const string NextQuestId = "MQ33";
    public const string Complete = "MQ32_COMPLETE";
    public const string CoreReconstructed = "CLOSED_THRESHOLD_CORE_RECONSTRUCTED";
    private const string EvidencePrefix = "MQ32_EVIDENCE_";
    private const string IndependentPrefix = "MQ32_INDEPENDENT_";
    private const string ContradictionPrefix = "MQ32_CONTRADICTION_";
    private readonly GameProgress _progress;

    public Mq32Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq31Campaign.Complete)) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool RecordEvidence(Mq32EvidenceCategory category, bool fromInterestedFaction)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(EvidenceFlag(category));
        if (!fromInterestedFaction) _progress.SetFlag(IndependentFlag(category));
        TryReconstructCore();
        return true;
    }

    public bool MarkContradiction(Mq32EvidenceCategory first, Mq32EvidenceCategory second)
    {
        if (!IsActive() || first == second || !HasEvidence(first) || !HasEvidence(second)) return false;
        _progress.SetFlag(ContradictionFlag(first, second));
        return true;
    }

    public bool HasEvidence(Mq32EvidenceCategory category) => _progress.HasFlag(EvidenceFlag(category));
    public int EvidenceCategoryCount => Enum.GetValues<Mq32EvidenceCategory>().Count(HasEvidence);
    public bool HasIndependentEvidence => Enum.GetValues<Mq32EvidenceCategory>().Any(c => HasEvidence(c) && _progress.HasFlag(IndependentFlag(c)));
    public bool IsCoreReconstructed => _progress.HasFlag(CoreReconstructed);

    public bool HasContradiction(Mq32EvidenceCategory first, Mq32EvidenceCategory second) =>
        first != second && _progress.HasFlag(ContradictionFlag(first, second));

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !IsCoreReconstructed) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;

    private void TryReconstructCore()
    {
        if (EvidenceCategoryCount >= 3 && HasIndependentEvidence)
            _progress.SetFlag(CoreReconstructed);
    }

    private static string EvidenceFlag(Mq32EvidenceCategory category) => $"{EvidencePrefix}{category.ToString().ToUpperInvariant()}";
    private static string IndependentFlag(Mq32EvidenceCategory category) => $"{IndependentPrefix}{category.ToString().ToUpperInvariant()}";

    private static string ContradictionFlag(Mq32EvidenceCategory first, Mq32EvidenceCategory second)
    {
        var a = first.ToString().ToUpperInvariant();
        var b = second.ToString().ToUpperInvariant();
        return string.CompareOrdinal(a, b) < 0 ? $"{ContradictionPrefix}{a}_{b}" : $"{ContradictionPrefix}{b}_{a}";
    }
}
