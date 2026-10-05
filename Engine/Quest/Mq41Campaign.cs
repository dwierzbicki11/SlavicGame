using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public sealed class Mq41Campaign
{
    public const string QuestId = "MQ41";
    public const string NextQuestId = "MQ42";
    public const string EchoAFound = "MQ41_ECHO_A_FOUND";
    public const string EchoBFound = "MQ41_ECHO_B_FOUND";
    public const string Difference01 = "MQ41_DIFFERENCE_01";
    public const string Difference02 = "MQ41_DIFFERENCE_02";
    public const string NaviaSplotDistinction = "MQ41_NAVIA_SPLOT_DISTINCTION";
    public const string Complete = "MQ41_COMPLETE";
    public const string JournalCompared = "MQ41_JOURNAL_COMPARED";

    private readonly GameProgress _progress;

    public Mq41Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq40Campaign.Complete)) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool FindEchoA() => RecordWhileActive(EchoAFound);
    public bool FindEchoB() => RecordWhileActive(EchoBFound);

    public bool RecordDifference01()
    {
        if (!BothEchoesFound()) return false;
        _progress.SetFlag(Difference01);
        return true;
    }

    public bool RecordDifference02()
    {
        if (!BothEchoesFound()) return false;
        _progress.SetFlag(Difference02);
        return true;
    }

    // Recovery observations use the same durable evidence flags as direct observations.
    public bool RecoverDifference01() => RecordDifference01();
    public bool RecoverDifference02() => RecordDifference02();

    public bool CompareInJournal()
    {
        if (!HasRequiredEvidence()) return false;
        _progress.SetFlag(JournalCompared);
        _progress.SetFlag(NaviaSplotDistinction);
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() ||
            !_progress.HasFlag(NaviaSplotDistinction) ||
            !_progress.HasFlag(JournalCompared)) return false;

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

    private bool BothEchoesFound() => IsActive() &&
        _progress.HasFlag(EchoAFound) && _progress.HasFlag(EchoBFound);

    private bool HasRequiredEvidence() => BothEchoesFound() &&
        _progress.HasFlag(Difference01) && _progress.HasFlag(Difference02);

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
