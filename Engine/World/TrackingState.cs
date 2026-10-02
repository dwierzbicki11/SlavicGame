using System.Numerics;

namespace SlavicGame.Engine.World;

public enum TrackCategory
{
    Footprint,
    BrokenVegetation,
    BloodOrBodyTrace,
    ObjectTrace,
    SoundClue,
    EnvironmentalAnomaly,
    SupernaturalTrace
}

public enum TrackFreshness
{
    Fresh,
    Recent,
    Old,
    Faded
}

public sealed record TrackSnapshot(
    string Id,
    TrackCategory Category,
    float X,
    float Y,
    float Z,
    TrackFreshness Freshness,
    string? SourceId,
    string EvidenceId,
    string? MagicSignature,
    bool Discovered);

public sealed class TrackingState
{
    private readonly Dictionary<string, TrackSnapshot> _tracks = new(StringComparer.Ordinal);

    public IReadOnlyCollection<TrackSnapshot> Tracks => _tracks.Values;

    public void Register(
        string id,
        TrackCategory category,
        Vector3 position,
        TrackFreshness freshness,
        string evidenceId,
        string? sourceId = null,
        string? magicSignature = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceId);
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(position.Z))
            throw new ArgumentOutOfRangeException(nameof(position));
        if (_tracks.ContainsKey(id)) throw new InvalidOperationException($"Track '{id}' is already registered.");

        _tracks[id] = new TrackSnapshot(id, category, position.X, position.Y, position.Z, freshness,
            sourceId, evidenceId, magicSignature, false);
    }

    public bool Discover(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!_tracks.TryGetValue(id, out var track)) return false;
        if (track.Discovered) return false;
        _tracks[id] = track with { Discovered = true };
        return true;
    }

    public bool IsDiscovered(string id) =>
        !string.IsNullOrWhiteSpace(id) && _tracks.TryGetValue(id, out var track) && track.Discovered;

    public TrackSnapshot[] Capture() => _tracks.Values.OrderBy(track => track.Id).ToArray();

    public void Restore(IEnumerable<TrackSnapshot> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        _tracks.Clear();
        foreach (var track in tracks)
        {
            if (string.IsNullOrWhiteSpace(track.Id) || string.IsNullOrWhiteSpace(track.EvidenceId))
                throw new InvalidOperationException("Saved track has an invalid durable ID or evidence ID.");
            if (_tracks.ContainsKey(track.Id)) throw new InvalidOperationException($"Duplicate saved track '{track.Id}'.");
            _tracks.Add(track.Id, track);
        }
    }
}
