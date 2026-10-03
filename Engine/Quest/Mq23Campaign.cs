using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq23Impact
{
    Extraction,
    Safety,
    Stabilization
}

public enum Mq23ResourceDecision
{
    ClaimantAControl,
    ClaimantBControl,
    SupervisedSplit,
    ExtractionLimitReserve
}

public sealed class Mq23Campaign
{
    public const string QuestId = "MQ23";
    public const string NextQuestId = "MQ24";
    public const string Complete = "MQ23_COMPLETE";
    public const string DecisionPrefix = "MQ23_D01_";
    private const string ImpactPrefix = "MQ23_IMPACT_REVIEWED_";
    private readonly GameProgress _progress;

    public Mq23Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ22_COMPLETE")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool ReviewImpact(Mq23Impact impact)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(ImpactFlag(impact));
        return true;
    }

    public bool HasReviewedAllImpacts => Enum.GetValues<Mq23Impact>().All(impact => _progress.HasFlag(ImpactFlag(impact)));

    public bool ChooseResourceControl(Mq23ResourceDecision decision)
    {
        if (!IsActive() || !HasReviewedAllImpacts || HasDecision) return false;
        foreach (var value in Enum.GetValues<Mq23ResourceDecision>())
            _progress.SetFlag(DecisionFlag(value), value == decision);
        return true;
    }

    public bool HasDecision => Enum.GetValues<Mq23ResourceDecision>().Any(value => _progress.HasFlag(DecisionFlag(value)));

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !HasDecision) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string ImpactFlag(Mq23Impact impact) => ImpactPrefix + impact.ToString().ToUpperInvariant();
    private static string DecisionFlag(Mq23ResourceDecision decision) => DecisionPrefix + decision.ToString().ToUpperInvariant();
}
