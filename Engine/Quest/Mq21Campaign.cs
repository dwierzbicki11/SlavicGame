using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq21AccessDecision
{
    LeaguePreference,
    NadborPreference,
    LimitedBoth
}

public enum Mq21ClaimSide
{
    League,
    Nadbor
}

public sealed class Mq21Campaign
{
    public const string QuestId = "MQ21";
    public const string NextQuestId = "MQ22";
    public const string Complete = "MQ21_COMPLETE";
    public const string DecisionPrefix = "MQ21_D01_";
    private const string VerifiedPrefix = "MQ21_CLAIM_VERIFIED_";
    private readonly GameProgress _progress;

    public Mq21Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ20_COMPLETE")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool VerifyClaim(Mq21ClaimSide side)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(VerifiedFlag(side));
        return true;
    }

    public bool HasVerifiedBothClaims => Enum.GetValues<Mq21ClaimSide>().All(side => _progress.HasFlag(VerifiedFlag(side)));

    public bool ChooseAccess(Mq21AccessDecision decision)
    {
        if (!IsActive() || !HasVerifiedBothClaims || HasDecision) return false;
        foreach (var value in Enum.GetValues<Mq21AccessDecision>())
            _progress.SetFlag(DecisionFlag(value), value == decision);
        return true;
    }

    public bool HasDecision => Enum.GetValues<Mq21AccessDecision>().Any(value => _progress.HasFlag(DecisionFlag(value)));

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !HasDecision) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string VerifiedFlag(Mq21ClaimSide side) => VerifiedPrefix + side.ToString().ToUpperInvariant();
    private static string DecisionFlag(Mq21AccessDecision decision) => DecisionPrefix + decision.ToString().ToUpperInvariant();
}
