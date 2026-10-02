namespace SlavicGame.Engine.World;

public sealed record MapOverlaySnapshot(
    string[] DatasetIds,
    string[] EvidenceIds,
    string? PredictedPointId,
    bool HypothesisSynthesized);

/// <summary>
/// Persistent, content-ID-driven state for comparing map/evidence datasets.
/// It deliberately owns no rendering or quest-specific names.
/// </summary>
public sealed class MapOverlayState
{
    private readonly HashSet<string> _datasetIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _evidenceIds = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> DatasetIds => _datasetIds;
    public IReadOnlyCollection<string> EvidenceIds => _evidenceIds;
    public string? PredictedPointId { get; private set; }
    public bool HypothesisSynthesized { get; private set; }

    public bool AddDataset(string datasetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(datasetId);
        return _datasetIds.Add(datasetId);
    }

    public bool AddEvidence(string evidenceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);
        return _evidenceIds.Add(evidenceId);
    }

    public bool HasDataset(string datasetId) =>
        !string.IsNullOrWhiteSpace(datasetId) && _datasetIds.Contains(datasetId);

    public bool HasEvidence(string evidenceId) =>
        !string.IsNullOrWhiteSpace(evidenceId) && _evidenceIds.Contains(evidenceId);

    public bool CanSynthesize(IEnumerable<string> requiredDatasetIds, IEnumerable<string> requiredEvidenceIds)
    {
        ArgumentNullException.ThrowIfNull(requiredDatasetIds);
        ArgumentNullException.ThrowIfNull(requiredEvidenceIds);
        return requiredDatasetIds.All(HasDataset) && requiredEvidenceIds.All(HasEvidence);
    }

    public bool Synthesize(
        IEnumerable<string> requiredDatasetIds,
        IEnumerable<string> requiredEvidenceIds,
        string predictedPointId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(predictedPointId);
        if (!CanSynthesize(requiredDatasetIds, requiredEvidenceIds)) return false;

        if (HypothesisSynthesized)
            return StringComparer.Ordinal.Equals(PredictedPointId, predictedPointId);

        PredictedPointId = predictedPointId;
        HypothesisSynthesized = true;
        return true;
    }

    public MapOverlaySnapshot Capture() => new(
        _datasetIds.OrderBy(id => id).ToArray(),
        _evidenceIds.OrderBy(id => id).ToArray(),
        PredictedPointId,
        HypothesisSynthesized);

    public void Restore(MapOverlaySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _datasetIds.Clear();
        _evidenceIds.Clear();
        foreach (var id in snapshot.DatasetIds ?? [])
            if (!string.IsNullOrWhiteSpace(id)) _datasetIds.Add(id);
        foreach (var id in snapshot.EvidenceIds ?? [])
            if (!string.IsNullOrWhiteSpace(id)) _evidenceIds.Add(id);

        HypothesisSynthesized = snapshot.HypothesisSynthesized;
        PredictedPointId = HypothesisSynthesized && !string.IsNullOrWhiteSpace(snapshot.PredictedPointId)
            ? snapshot.PredictedPointId
            : null;
        if (HypothesisSynthesized && PredictedPointId is null)
            HypothesisSynthesized = false;
    }
}
