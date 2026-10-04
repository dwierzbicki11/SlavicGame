using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq31ArchiveFact
{
    ParentsArchiveRole,
    WszeborArchiveLink,
    ClosedThresholdNightReference
}

public enum Mq31RecordChannel
{
    PrimaryDocument,
    SecondaryRegister
}

public sealed class Mq31Campaign
{
    public const string QuestId = "MQ31";
    public const string NextQuestId = "MQ32";
    public const string Complete = "MQ31_COMPLETE";
    public const string ArchiveRecordsFound = "ARCHIVE_RECORDS_FOUND";
    public const string ParentsArchiveRoleKnown = "PARENTS_ARCHIVE_ROLE_KNOWN";
    public const string WszeborArchiveLinkKnown = "WSZEBOR_ARCHIVE_LINK_KNOWN";
    public const string ClosedThresholdNightReferenceKnown = "CLOSED_THRESHOLD_NIGHT_REFERENCE_KNOWN";
    private const string RecordPrefix = "MQ31_RECORD_";
    private readonly GameProgress _progress;

    public Mq31Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ30_COMPLETE")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool RecordArchiveEvidence(Mq31ArchiveFact fact, Mq31RecordChannel channel)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(RecordFlag(fact, channel));
        _progress.SetFlag(ArchiveRecordsFound);
        _progress.SetFlag(FactFlag(fact));
        return true;
    }

    public bool HasFact(Mq31ArchiveFact fact) => _progress.HasFlag(FactFlag(fact));
    public bool HasAllCriticalFacts => Enum.GetValues<Mq31ArchiveFact>().All(HasFact);

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !HasAllCriticalFacts) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;

    private static string FactFlag(Mq31ArchiveFact fact) => fact switch
    {
        Mq31ArchiveFact.ParentsArchiveRole => ParentsArchiveRoleKnown,
        Mq31ArchiveFact.WszeborArchiveLink => WszeborArchiveLinkKnown,
        Mq31ArchiveFact.ClosedThresholdNightReference => ClosedThresholdNightReferenceKnown,
        _ => throw new ArgumentOutOfRangeException(nameof(fact))
    };

    private static string RecordFlag(Mq31ArchiveFact fact, Mq31RecordChannel channel) =>
        $"{RecordPrefix}{fact.ToString().ToUpperInvariant()}_{channel.ToString().ToUpperInvariant()}";
}