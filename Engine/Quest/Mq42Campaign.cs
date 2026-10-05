using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public sealed class Mq42Campaign
{
    public const string QuestId = "MQ42";
    public const string NextQuestId = "MQ43";
    public const string ParentBFound = "MQ42_PARENT_B_FOUND";
    public const string IdentityVerified = "MQ42_IDENTITY_VERIFIED";
    public const string ParentBDecision = "MQ42_PARENT_B_DECISION";
    public const string Skipped = "MQ42_SKIPPED";
    public const string Complete = "MQ42_COMPLETE";

    private readonly GameProgress _progress;

    public Mq42Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq41Campaign.Complete)) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool FindParentB() => RecordWhileActive(ParentBFound);

    public bool VerifyIdentity()
    {
        if (!IsActive() || !_progress.HasFlag(ParentBFound)) return false;
        _progress.SetFlag(IdentityVerified);
        return true;
    }

    public bool RecordParentBDecision()
    {
        if (!IsActive() || _progress.HasFlag(Skipped) || !_progress.HasFlag(IdentityVerified)) return false;
        _progress.SetFlag(ParentBDecision);
        return true;
    }

    public bool Skip()
    {
        if (!IsActive() || _progress.HasFlag(ParentBDecision)) return false;
        _progress.SetFlag(Skipped);
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive()) return false;
        var metPath = _progress.HasFlag(ParentBDecision) && _progress.HasFlag(IdentityVerified);
        var skipPath = _progress.HasFlag(Skipped);
        if (!metPath && !skipPath) return false;
        if (metPath && skipPath) return false;

        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable)
            next.SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool RecordWhileActive(string flag)
    {
        if (!IsActive() || _progress.HasFlag(Skipped)) return false;
        _progress.SetFlag(flag);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
