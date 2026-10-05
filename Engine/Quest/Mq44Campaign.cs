using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public sealed class Mq44Campaign
{
    public const string QuestId = "MQ44";
    public const string NextQuestId = "MQ50";
    public const string ReturnedToJawia = "MQ44_RETURNED_TO_JAWIA";
    public const string CrisisEscalated = "MQ44_CRISIS_ESCALATED";
    public const string FinalNeedsKnown = "MQ44_FINAL_NEEDS_KNOWN";
    public const string Complete = "MQ44_COMPLETE";

    private readonly GameProgress _progress;

    public Mq44Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq43Campaign.Complete) ||
            !_progress.HasFlag(Mq43Campaign.SplotTruthKnown) ||
            !_progress.HasFlag(Mq40Campaign.ReturnAnchorSet)) return false;

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool ReturnToJawia()
    {
        if (!IsActive() || !_progress.HasFlag(Mq40Campaign.ReturnAnchorSet)) return false;
        _progress.SetFlag(ReturnedToJawia);
        return true;
    }

    public bool EscalateCrisis()
    {
        if (!IsActive() || !_progress.HasFlag(ReturnedToJawia)) return false;
        _progress.SetFlag(CrisisEscalated);
        return true;
    }

    // Regional messenger/content variants are presentation. Critical information has a durable fallback.
    public bool SynthesizeFinalNeeds()
    {
        if (!IsActive() ||
            !_progress.HasFlag(ReturnedToJawia) ||
            !_progress.HasFlag(CrisisEscalated)) return false;
        _progress.SetFlag(FinalNeedsKnown);
        return true;
    }

    public bool RecoverCriticalMessages() => SynthesizeFinalNeeds();

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !_progress.HasFlag(FinalNeedsKnown)) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable)
            next.SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
