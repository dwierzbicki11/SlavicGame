using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public sealed class Mq40Campaign
{
    public const string QuestId = "MQ40";
    public const string NextQuestId = "MQ41";
    public const string EnteredNavia = "MQ40_ENTERED_NAVIA";
    public const string ReturnAnchorSet = "MQ40_RETURN_ANCHOR_SET";
    public const string Complete = "MQ40_COMPLETE";
    public const string NaviaRulesObserved = "MQ40_NAVIA_RULES_OBSERVED";
    public const string EchoTrailFound = "MQ40_ECHO_TRAIL_FOUND";
    public const string ThresholdInstructionsRecovered = "MQ40_THRESHOLD_INSTRUCTIONS_RECOVERED";

    private readonly GameProgress _progress;

    public Mq40Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq34Campaign.Complete) ||
            !_progress.HasFlag(Mq34Campaign.Act4NaviaLeadAvailable)) return false;

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool RecoverThresholdInstructions()
    {
        if (!IsActive()) return false;
        _progress.SetFlag(ThresholdInstructionsRecovered);
        return true;
    }

    public bool EnterNavia()
    {
        if (!IsActive()) return false;
        _progress.SetFlag(EnteredNavia);
        return true;
    }

    public bool ObserveNaviaRules()
    {
        if (!IsActive() || !_progress.HasFlag(EnteredNavia)) return false;
        _progress.SetFlag(NaviaRulesObserved);
        return true;
    }

    public bool SetReturnAnchor()
    {
        if (!IsActive() || !_progress.HasFlag(EnteredNavia)) return false;
        _progress.SetFlag(ReturnAnchorSet);
        return true;
    }

    public bool FindEchoTrail()
    {
        if (!IsActive() || !_progress.HasFlag(NaviaRulesObserved)) return false;
        _progress.SetFlag(EchoTrailFound);
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() ||
            !_progress.HasFlag(EnteredNavia) ||
            !_progress.HasFlag(NaviaRulesObserved) ||
            !_progress.HasFlag(ReturnAnchorSet) ||
            !_progress.HasFlag(EchoTrailFound)) return false;

        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
