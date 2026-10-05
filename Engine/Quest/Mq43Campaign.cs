using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public sealed class Mq43Campaign
{
    public const string QuestId = "MQ43";
    public const string NextQuestId = "MQ44";
    public const string UnstablePatternEntered = "MQ43_UNSTABLE_PATTERN_ENTERED";
    public const string RuleBreakObserved = "MQ43_RULE_BREAK_OBSERVED";
    public const string SplotTruthKnown = "MQ43_SPLOT_TRUTH_KNOWN";
    public const string Complete = "MQ43_COMPLETE";

    private readonly GameProgress _progress;

    public Mq43Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq42Campaign.Complete)) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool EnterUnstablePattern() => RecordWhileActive(UnstablePatternEntered);

    public bool ObserveRuleBreak()
    {
        if (!IsActive() || !_progress.HasFlag(UnstablePatternEntered)) return false;
        _progress.SetFlag(RuleBreakObserved);
        return true;
    }

    public bool RevealSplotTruth()
    {
        if (!IsActive() ||
            !_progress.HasFlag(UnstablePatternEntered) ||
            !_progress.HasFlag(RuleBreakObserved) ||
            !_progress.HasFlag(Mq41Campaign.NaviaSplotDistinction)) return false;

        _progress.SetFlag(SplotTruthKnown);
        return true;
    }

    // Interrupted presentation resumes from durable observations; consequence flags are never counted twice.
    public bool ResumeReveal() => RevealSplotTruth();

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !_progress.HasFlag(SplotTruthKnown)) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable)
            next.SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool RecordWhileActive(string flag)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(flag);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
