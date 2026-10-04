using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq30ArchiveTrace
{
    StructuralRecord,
    FieldRecord
}

public sealed class Mq30Campaign
{
    public const string QuestId = "MQ30";
    public const string NextQuestId = "MQ31";
    public const string Complete = "MQ30_COMPLETE";
    public const string RegionEntered = "R6_ENTERED";
    public const string RouteKnown = "FIRST_THRESHOLD_ROUTE_KNOWN";
    public const string ArchivePresenceConfirmed = "ARCHIVE_PRESENCE_CONFIRMED";
    private const string TracePrefix = "MQ30_ARCHIVE_TRACE_";
    private readonly GameProgress _progress;

    public Mq30Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ25_COMPLETE") || !_progress.HasFlag("NETWORK_MULTICULTURAL_ORIGIN")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        _progress.SetFlag(RegionEntered);
        return true;
    }

    public bool RecordArchiveTrace(Mq30ArchiveTrace trace)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(TraceFlag(trace));
        if (HasRequiredTraces) _progress.SetFlag(ArchivePresenceConfirmed);
        return true;
    }

    public bool HasRequiredTraces => Enum.GetValues<Mq30ArchiveTrace>().All(trace => _progress.HasFlag(TraceFlag(trace)));

    public bool EstablishSafeRoute()
    {
        if (!IsActive() || !HasRequiredTraces || !_progress.HasFlag(ArchivePresenceConfirmed)) return false;
        _progress.SetFlag(RouteKnown);
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !_progress.HasFlag(RouteKnown)) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string TraceFlag(Mq30ArchiveTrace trace) => TracePrefix + trace.ToString().ToUpperInvariant();
}
