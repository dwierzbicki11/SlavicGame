using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Quest;

/// <summary>
/// Durable MQ13 progression. Concrete dataset, evidence and predicted-point IDs are content data;
/// runtime only enforces the Act I synthesis contract.
/// </summary>
public sealed class Mq13Campaign
{
    public const string QuestId = "MQ13";
    public const string NextQuestId = "MQ20";
    public const string NetworkHypothesis = "NETWORK_HYPOTHESIS";
    public const string Complete = "MQ13_COMPLETE";

    private readonly GameProgress _progress;
    private readonly string[] _requiredDatasetIds;
    private readonly string[] _requiredEvidenceIds;

    public Mq13Campaign(GameProgress progress, IEnumerable<string> requiredDatasetIds, IEnumerable<string> requiredEvidenceIds)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(requiredDatasetIds);
        ArgumentNullException.ThrowIfNull(requiredEvidenceIds);
        _progress = progress;
        _requiredDatasetIds = requiredDatasetIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToArray();
        _requiredEvidenceIds = requiredEvidenceIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToArray();
        if (_requiredEvidenceIds.Length != 3)
            throw new ArgumentException("MQ13 requires exactly three distinct required evidence packages.", nameof(requiredEvidenceIds));
        if (_requiredDatasetIds.Length == 0)
            throw new ArgumentException("MQ13 requires at least one map dataset.", nameof(requiredDatasetIds));
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ10_COMPLETE") || !_progress.HasFlag("MQ11_COMPLETE") || !_progress.HasFlag("MQ12_COMPLETE")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered) quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool AddDataset(string datasetId)
    {
        if (!IsActive()) return false;
        return _progress.MapOverlay.AddDataset(datasetId);
    }

    public bool AddEvidence(string evidenceId)
    {
        if (!IsActive()) return false;
        return _progress.MapOverlay.AddEvidence(evidenceId);
    }

    public bool HasMinimumEvidence => _progress.MapOverlay.CanSynthesize(_requiredDatasetIds, _requiredEvidenceIds);

    public bool TestHypothesis(string predictedPointId)
    {
        if (!IsActive() || !HasMinimumEvidence) return false;
        if (!_progress.MapOverlay.Synthesize(_requiredDatasetIds, _requiredEvidenceIds, predictedPointId)) return false;
        _progress.SetFlag(NetworkHypothesis);
        return true;
    }

    public bool CompleteQuest()
    {
        if (!_progress.HasFlag(NetworkHypothesis) || !_progress.MapOverlay.HypothesisSynthesized || _progress.HasFlag(Complete)) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
