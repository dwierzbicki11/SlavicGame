using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

/// <summary>
/// Deterministic campaign progression for the production-ready Act 0 contract.
/// Presentation systems decide how the player sees a lead; this class owns only
/// durable quest/flag transitions so no optional NPC can soft-lock the campaign.
/// </summary>
public sealed class MainQuestCampaign
{
    public const string Mq00Id = "MQ00";
    public const string Mq01Id = "MQ01";
    public const string Mq10Id = "MQ10";

    public const string Mq00Complete = "MQ00_COMPLETE";
    public const string Mq01ConsequenceSeen = "MQ01_CONSEQUENCE_SEEN";
    public const string Mq01NadborzeLead = "MQ01_NADBORZE_LEAD";
    public const string Mq01Complete = "MQ01_COMPLETE";

    private readonly GameProgress _progress;

    public MainQuestCampaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public void SynchronizeAct0()
    {
        var mq00 = _progress.Quests.Get(Mq00Id);
        if (mq00.Phase == QuestPhase.TurnedIn)
        {
            _progress.SetFlag(Mq00Complete);
        }

        if (_progress.HasFlag(Mq00Complete))
        {
            var mq01 = _progress.Quests.Get(Mq01Id);
            if (mq01.Phase == QuestPhase.Unavailable)
            {
                mq01.SetPhase(QuestPhase.Offered);
            }
        }

        if (_progress.HasFlag(Mq01Complete))
        {
            var mq10 = _progress.Quests.Get(Mq10Id);
            if (mq10.Phase == QuestPhase.Unavailable)
            {
                mq10.SetPhase(QuestPhase.Offered);
            }
        }
    }

    public bool MarkMq01ConsequenceSeen()
    {
        if (!_progress.HasFlag(Mq00Complete)) return false;
        _progress.SetFlag(Mq01ConsequenceSeen);
        return true;
    }

    public bool GrantNadborzeLead()
    {
        if (!_progress.HasFlag(Mq00Complete)) return false;

        _progress.SetFlag(Mq01NadborzeLead);
        var mq01 = _progress.Quests.Get(Mq01Id);
        if (mq01.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
        {
            mq01.SetPhase(QuestPhase.Active);
        }
        return true;
    }

    /// <summary>
    /// Diegetic fallback for a missing/skipped primary lead giver. It deliberately
    /// shares the same idempotent state write as the normal dialogue path.
    /// </summary>
    public bool EnsureNadborzeLead() => GrantNadborzeLead();

    public bool DepartForNadborze()
    {
        if (!_progress.HasFlag(Mq01NadborzeLead)) return false;
        if (_progress.HasFlag(Mq01Complete)) return false;

        var mq01 = _progress.Quests.Get(Mq01Id);
        mq01.Resolve(QuestResolution.Other);
        _progress.SetFlag(Mq01Complete);
        SynchronizeAct0();
        return true;
    }
}
