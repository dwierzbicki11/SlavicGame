using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Quest;

/// <summary>
/// Durable MQ20 "Sól i milczenie" progression. Concrete actors, dialogue and presentation
/// remain content-owned; runtime only enforces the implementation-ready campaign contract.
/// </summary>
public sealed class Mq20Campaign
{
    public const string QuestId = "MQ20";
    public const string EstuaryBlocked = "MQ20_ESTUARY_BLOCKED";
    public const string RemoteNodeEvidence = "MQ20_REMOTE_NODE_EVIDENCE";
    public const string Complete = "MQ20_COMPLETE";

    private readonly GameProgress _progress;
    private readonly string _remoteDatasetId;
    private readonly string _predictedPointId;

    public Mq20Campaign(GameProgress progress, string remoteDatasetId, string predictedPointId)
    {
        ArgumentNullException.ThrowIfNull(progress);
        if (string.IsNullOrWhiteSpace(remoteDatasetId)) throw new ArgumentException("Remote map dataset ID is required.", nameof(remoteDatasetId));
        if (string.IsNullOrWhiteSpace(predictedPointId)) throw new ArgumentException("Predicted point ID is required.", nameof(predictedPointId));
        _progress = progress;
        _remoteDatasetId = remoteDatasetId;
        _predictedPointId = predictedPointId;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ13_COMPLETE")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool EnterBlockedEstuary()
    {
        if (!IsActive()) return false;
        _progress.SetFlag(EstuaryBlocked);
        return true;
    }

    public bool AcquireMapAccess(bool accessGranted)
    {
        if (!IsActive() || !accessGranted) return false;
        _progress.MapOverlay.AddDataset(_remoteDatasetId);
        return true;
    }

    public bool CompareRemoteNode()
    {
        if (!IsActive() || !_progress.MapOverlay.DatasetIds.Contains(_remoteDatasetId, StringComparer.Ordinal)) return false;
        if (!_progress.MapOverlay.HypothesisSynthesized ||
            !string.Equals(_progress.MapOverlay.PredictedPointId, _predictedPointId, StringComparison.Ordinal)) return false;
        _progress.SetFlag(RemoteNodeEvidence);
        return true;
    }

    public bool CompleteQuest(bool estuaryResolvedOrBypassed)
    {
        if (!IsActive() || !estuaryResolvedOrBypassed || !_progress.HasFlag(RemoteNodeEvidence) || _progress.HasFlag(Complete)) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
