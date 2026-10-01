namespace SlavicGame.Engine.Quest;

public enum KnowledgeKind
{
    Rumor,
    Observation,
    ConfirmedFact,
    Interpretation
}

public enum QuestPhase
{
    Unavailable,
    Offered,
    Active,
    Investigation,
    Preparation,
    Encounter,
    Resolved,
    TurnedIn,
    Failed
}

public enum QuestResolution
{
    None,
    RitualClosure,
    ConditionalPact,
    DestroyAnchor,
    Other
}

public sealed record EvidenceEntry(
    string Id,
    string QuestId,
    KnowledgeKind Kind,
    string Text,
    string? SourceId = null);

public sealed record QuestSnapshot(
    string Id,
    QuestPhase Phase,
    QuestResolution Resolution,
    bool RewardClaimed,
    EvidenceEntry[] Evidence);

public sealed class QuestRecord
{
    private readonly Dictionary<string, EvidenceEntry> _evidence = new(StringComparer.Ordinal);

    public string Id { get; }
    public QuestPhase Phase { get; private set; } = QuestPhase.Unavailable;
    public QuestResolution Resolution { get; private set; }
    public bool RewardClaimed { get; private set; }
    public IReadOnlyCollection<EvidenceEntry> Evidence => _evidence.Values;

    public QuestRecord(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public void SetPhase(QuestPhase phase)
    {
        if (Phase == QuestPhase.TurnedIn && phase != QuestPhase.TurnedIn)
        {
            throw new InvalidOperationException("A turned-in quest cannot return to an earlier phase.");
        }
        Phase = phase;
    }

    public void Resolve(QuestResolution resolution)
    {
        if (resolution == QuestResolution.None)
        {
            throw new ArgumentOutOfRangeException(nameof(resolution));
        }
        Resolution = resolution;
        Phase = QuestPhase.Resolved;
    }

    public bool ClaimReward()
    {
        if (Phase is not (QuestPhase.Resolved or QuestPhase.TurnedIn) || RewardClaimed)
        {
            return false;
        }

        RewardClaimed = true;
        Phase = QuestPhase.TurnedIn;
        return true;
    }

    public bool AddEvidence(EvidenceEntry evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (!string.Equals(evidence.QuestId, Id, StringComparison.Ordinal))
        {
            throw new ArgumentException("Evidence belongs to another quest.", nameof(evidence));
        }
        return _evidence.TryAdd(evidence.Id, evidence);
    }

    public QuestSnapshot Capture() =>
        new(Id, Phase, Resolution, RewardClaimed, _evidence.Values.OrderBy(e => e.Id).ToArray());

    public void Restore(QuestSnapshot snapshot)
    {
        if (!string.Equals(snapshot.Id, Id, StringComparison.Ordinal))
        {
            throw new ArgumentException("Snapshot quest does not match record.", nameof(snapshot));
        }

        Phase = snapshot.Phase;
        Resolution = snapshot.Resolution;
        RewardClaimed = snapshot.RewardClaimed;
        _evidence.Clear();
        foreach (var evidence in snapshot.Evidence ?? [])
        {
            if (!string.Equals(evidence.QuestId, Id, StringComparison.Ordinal))
            {
                throw new ArgumentException("Snapshot contains evidence for another quest.", nameof(snapshot));
            }
            _evidence.Add(evidence.Id, evidence);
        }
    }
}

public sealed class QuestJournal
{
    private readonly Dictionary<string, QuestRecord> _quests = new(StringComparer.Ordinal);

    public IReadOnlyCollection<QuestRecord> Quests => _quests.Values;

    public QuestRecord Get(string questId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questId);
        if (!_quests.TryGetValue(questId, out var quest))
        {
            quest = new QuestRecord(questId);
            _quests.Add(questId, quest);
        }
        return quest;
    }

    public QuestSnapshot[] Capture() =>
        _quests.Values.Select(q => q.Capture()).OrderBy(q => q.Id).ToArray();

    public void Restore(IEnumerable<QuestSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        _quests.Clear();
        foreach (var snapshot in snapshots)
        {
            var quest = new QuestRecord(snapshot.Id);
            quest.Restore(snapshot);
            _quests.Add(snapshot.Id, quest);
        }
    }
}
