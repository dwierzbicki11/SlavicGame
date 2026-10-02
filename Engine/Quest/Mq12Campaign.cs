using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Quest;

/// <summary>
/// Durable MQ12 progression. Content owns concrete track, landmark, anomaly and encounter IDs;
/// this runtime only enforces the implementation-ready quest contract.
/// </summary>
public sealed class Mq12Campaign
{
    public const string QuestId = "MQ12";
    public const string NextQuestId = "MQ13";
    public const string RouteAnomaly = "MQ12_ROUTE_ANOMALY";
    public const string NodeConfirmed = "MQ12_NODE_CONFIRMED";
    public const string Complete = "MQ12_COMPLETE";
    public const string WitnessConfirmed = "MQ12_WITNESS_CONFIRMED";
    public const string EnvironmentalFallback = "MQ12_ENVIRONMENTAL_FALLBACK";

    private readonly GameProgress _progress;
    private readonly HashSet<string> _referenceTrackIds;
    private readonly string _anomalyId;

    public Mq12Campaign(GameProgress progress, string anomalyId, IEnumerable<string> referenceTrackIds)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentException.ThrowIfNullOrWhiteSpace(anomalyId);
        ArgumentNullException.ThrowIfNull(referenceTrackIds);
        _progress = progress;
        _anomalyId = anomalyId;
        _referenceTrackIds = new HashSet<string>(referenceTrackIds.Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.Ordinal);
        if (_referenceTrackIds.Count != 3) throw new ArgumentException("MQ12 requires exactly three distinct reference tracks.", nameof(referenceTrackIds));
    }

    public bool BeginRouteAnomaly()
    {
        if (!_progress.HasFlag(MainQuestCampaign.Mq11Complete)) return false;
        if (!_progress.Navigation.RouteAnomalyActive)
            _progress.Navigation.ActivateRouteAnomaly(_anomalyId);
        else if (_progress.Navigation.ActiveRouteAnomalyId != _anomalyId)
            return false;
        _progress.SetFlag(RouteAnomaly);
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered) quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool DiscoverReference(string trackId)
    {
        if (!_progress.HasFlag(RouteAnomaly) || !_referenceTrackIds.Contains(trackId)) return false;
        return _progress.Tracking.Discover(trackId);
    }

    public int DiscoveredReferenceCount => _referenceTrackIds.Count(_progress.Tracking.IsDiscovered);
    public bool HasRequiredReferences => DiscoveredReferenceCount >= 2;

    public bool ConfirmForestChange(bool witnessAvailable)
    {
        if (!HasRequiredReferences) return false;
        _progress.SetFlag(witnessAvailable ? WitnessConfirmed : EnvironmentalFallback);
        return true;
    }

    public bool ConfirmOldNode(string landmarkId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landmarkId);
        if (!HasRequiredReferences || !HasForestConfirmation()) return false;
        _progress.Navigation.DiscoverLandmark(landmarkId);
        _progress.SetFlag(NodeConfirmed);
        return true;
    }

    public bool CompleteQuest()
    {
        if (!_progress.HasFlag(NodeConfirmed) || _progress.HasFlag(Complete)) return false;
        // Recovery invariant: MQ12 never leaves its route anomaly active after completion.
        _progress.Navigation.ClearRouteAnomaly(_anomalyId);
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable) next.SetPhase(QuestPhase.Offered);
        return true;
    }

    public bool LeaveRegionSafely()
    {
        if (_progress.Navigation.ActiveRouteAnomalyId != _anomalyId) return false;
        return _progress.Navigation.ClearRouteAnomaly(_anomalyId);
    }

    private bool HasForestConfirmation() =>
        _progress.HasFlag(WitnessConfirmed) || _progress.HasFlag(EnvironmentalFallback);
}
