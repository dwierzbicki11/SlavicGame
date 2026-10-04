using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq33ConfrontationOutcome
{
    Conversation,
    Break,
    TemporaryAccord,
    Combat,
    Escape
}

public enum Mq33WszeborStance
{
    Unknown,
    Cooperative,
    Guarded,
    Hostile
}

public sealed class Mq33Campaign
{
    public const string QuestId = "MQ33";
    public const string NextQuestId = "MQ34";
    public const string Complete = "MQ33_COMPLETE";
    public const string PositionKnown = "WSZEBOR_POSITION_KNOWN";
    public const string DataPackageAvailable = "MQ33_DATA_PACKAGE_AVAILABLE";
    public const string RecoveryDataAvailable = "MQ33_RECOVERY_DATA_AVAILABLE";
    private const string OutcomePrefix = "WSZEBOR_CONFRONTATION_OUTCOME_";
    private const string StancePrefix = "MQ33_WSZEBOR_STANCE_";
    private readonly GameProgress _progress;

    public Mq33Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq32Campaign.Complete)) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool RecordWszeborPosition(Mq33WszeborStance stance)
    {
        if (!IsActive() || stance == Mq33WszeborStance.Unknown) return false;
        _progress.SetFlag(PositionKnown);
        _progress.SetFlag(StanceFlag(stance));
        return true;
    }

    public bool ResolveConfrontation(Mq33ConfrontationOutcome outcome, Mq33WszeborStance stance, bool directDataAvailable)
    {
        if (_progress.HasFlag(Complete) || !IsActive() || stance == Mq33WszeborStance.Unknown) return false;
        RecordWszeborPosition(stance);
        _progress.SetFlag(OutcomeFlag(outcome));
        if (directDataAvailable) _progress.SetFlag(DataPackageAvailable);
        else _progress.SetFlag(RecoveryDataAvailable);
        _progress.SetFlag(DataPackageAvailable);
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    public bool HasOutcome(Mq33ConfrontationOutcome outcome) => _progress.HasFlag(OutcomeFlag(outcome));
    public bool HasStance(Mq33WszeborStance stance) => stance != Mq33WszeborStance.Unknown && _progress.HasFlag(StanceFlag(stance));
    public bool HasDataForMq34 => _progress.HasFlag(DataPackageAvailable);

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string OutcomeFlag(Mq33ConfrontationOutcome outcome) => $"{OutcomePrefix}{outcome.ToString().ToUpperInvariant()}";
    private static string StanceFlag(Mq33WszeborStance stance) => $"{StancePrefix}{stance.ToString().ToUpperInvariant()}";
}
