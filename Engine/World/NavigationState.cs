namespace SlavicGame.Engine.World;

public sealed record NavigationSnapshot(
    string? ActiveRouteAnomalyId,
    string[] DiscoveredLandmarkIds);

public sealed class NavigationState
{
    private readonly HashSet<string> _landmarks = new(StringComparer.Ordinal);

    public string? ActiveRouteAnomalyId { get; private set; }
    public IReadOnlyCollection<string> DiscoveredLandmarkIds => _landmarks;
    public bool RouteAnomalyActive => ActiveRouteAnomalyId is not null;

    public bool ActivateRouteAnomaly(string anomalyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anomalyId);
        if (ActiveRouteAnomalyId == anomalyId) return false;
        if (ActiveRouteAnomalyId is not null)
            throw new InvalidOperationException($"Route anomaly '{ActiveRouteAnomalyId}' is already active.");
        ActiveRouteAnomalyId = anomalyId;
        return true;
    }

    public bool ClearRouteAnomaly(string anomalyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anomalyId);
        if (ActiveRouteAnomalyId != anomalyId) return false;
        ActiveRouteAnomalyId = null;
        return true;
    }

    public bool DiscoverLandmark(string landmarkId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(landmarkId);
        return _landmarks.Add(landmarkId);
    }

    public bool HasLandmark(string landmarkId) =>
        !string.IsNullOrWhiteSpace(landmarkId) && _landmarks.Contains(landmarkId);

    public NavigationSnapshot Capture() =>
        new(ActiveRouteAnomalyId, _landmarks.OrderBy(id => id).ToArray());

    public void Restore(NavigationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.ActiveRouteAnomalyId is { Length: 0 })
            throw new InvalidOperationException("Saved route anomaly ID cannot be empty.");

        ActiveRouteAnomalyId = snapshot.ActiveRouteAnomalyId;
        _landmarks.Clear();
        foreach (var id in snapshot.DiscoveredLandmarkIds ?? [])
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException("Saved landmark ID cannot be empty.");
            _landmarks.Add(id);
        }
    }
}
