using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record WildlifeTrailMark(
    string SourceId,
    WildlifeSpecies Species,
    Vector3 Position,
    Vector2 Forward,
    float AgeSeconds,
    float LifetimeSeconds,
    float Strength)
{
    public TrackFreshness Freshness
    {
        get
        {
            var ratio = LifetimeSeconds <= 0f
                ? 1f
                : Math.Clamp(AgeSeconds / LifetimeSeconds, 0f, 1f);

            return ratio switch
            {
                < 0.25f => TrackFreshness.Fresh,
                < 0.55f => TrackFreshness.Recent,
                < 0.82f => TrackFreshness.Old,
                _ => TrackFreshness.Faded
            };
        }
    }
}

public sealed class WildlifeTrackTrailState
{
    private const int MaxMarks = 180;
    private const float InspectDistance = 3.2f;

    private readonly List<WildlifeTrailMark> _marks = [];
    private readonly Dictionary<string, Vector3> _previous =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, float> _distance =
        new(StringComparer.Ordinal);

    public IReadOnlyList<WildlifeTrailMark> Marks => _marks;

    public string HudStatus(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var nearest = FindNearest(world.PlayerPosition);
        if (nearest is null)
            return "";

        var freshness = nearest.Freshness switch
        {
            TrackFreshness.Fresh => "SWIEZY",
            TrackFreshness.Recent => "NIEDAWNY",
            TrackFreshness.Old => "STARY",
            TrackFreshness.Faded => "ZATARTY",
            _ => "NIEZNANY"
        };

        var species = nearest.Species switch
        {
            WildlifeSpecies.Deer => "JELEN",
            WildlifeSpecies.Boar => "DZIK",
            WildlifeSpecies.Wolf => "WILK",
            _ => "ZWIERZE"
        };

        return $"TROP: {species} / {freshness}";
    }

    public void Reset(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        _marks.Clear();
        _previous.Clear();
        _distance.Clear();

        foreach (var actor in world.Wildlife.Actors)
        {
            _previous[actor.Id] = actor.Position;
            _distance[actor.Id] = 0f;
        }
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        Age(world, (float)deltaSeconds);

        foreach (var actor in world.Wildlife.Actors)
        {
            if (actor.Species == WildlifeSpecies.Raven)
            {
                _previous[actor.Id] = actor.Position;
                _distance[actor.Id] = 0f;
                continue;
            }

            if (!_previous.TryGetValue(actor.Id, out var previous))
            {
                _previous[actor.Id] = actor.Position;
                _distance[actor.Id] = 0f;
                continue;
            }

            var delta3 = actor.Position - previous;
            var delta = new Vector2(delta3.X, delta3.Z);
            var moved = delta.Length();

            if (moved <= 0.0001f)
            {
                _previous[actor.Id] = actor.Position;
                continue;
            }

            var forward = Vector2.Normalize(delta);
            var accumulated =
                (_distance.TryGetValue(actor.Id, out var value) ? value : 0f) +
                moved;
            var stride = Stride(actor.Species);

            while (accumulated >= stride)
            {
                accumulated -= stride;
                TryStamp(world, actor, forward);
            }

            _distance[actor.Id] = accumulated;
            _previous[actor.Id] = actor.Position;
        }
    }

    public WildlifeTrailMark? FindNearest(Vector3 position)
    {
        WildlifeTrailMark? nearest = null;
        var bestDistanceSquared = InspectDistance * InspectDistance;

        foreach (var mark in _marks)
        {
            var dx = mark.Position.X - position.X;
            var dz = mark.Position.Z - position.Z;
            var distanceSquared = dx * dx + dz * dz;

            if (distanceSquared > bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            nearest = mark;
        }

        return nearest;
    }

    public static float Trackability(
        TerrainSurfaceWeights weights,
        float rainIntensity)
    {
        var baseValue =
            weights.Mud * 1.00f +
            weights.Swamp * 0.86f +
            weights.ForestLitter * 0.58f +
            weights.Path * 0.32f +
            weights.Grass * 0.16f -
            weights.Rock * 0.82f;

        var moisture =
            Math.Clamp(rainIntensity, 0f, 1f) * 0.16f;

        return Math.Clamp(baseValue + moisture, 0f, 1f);
    }

    private void TryStamp(
        WorldState world,
        WildlifeWorldActor actor,
        Vector2 forward)
    {
        var position = actor.Position;

        if (WaterInteractionState.DepthAt(world, position) > 0.035f)
            return;

        var weights = TerrainSurfaceClassifier.Classify(
            position,
            Vector3.UnitY);
        var trackability = Trackability(
            weights,
            world.Weather.RainIntensity);

        if (trackability < 0.20f)
            return;

        position.Y =
            world.Terrain.SampleHeight(position) + 0.026f;

        var speciesFactor = actor.Species switch
        {
            WildlifeSpecies.Boar => 1.12f,
            WildlifeSpecies.Wolf => 0.92f,
            _ => 1f
        };

        var lifetime =
            (48f + trackability * 72f) * speciesFactor;

        _marks.Add(new WildlifeTrailMark(
            actor.Id,
            actor.Species,
            position,
            forward,
            0f,
            lifetime,
            trackability));

        if (_marks.Count > MaxMarks)
        {
            _marks.RemoveRange(
                0,
                _marks.Count - MaxMarks);
        }
    }

    private void Age(WorldState world, float deltaSeconds)
    {
        if (_marks.Count == 0 || deltaSeconds <= 0f)
            return;

        var rainMultiplier =
            1f +
            Math.Clamp(world.Weather.RainIntensity, 0f, 1f) * 3.6f;

        for (var i = _marks.Count - 1; i >= 0; i--)
        {
            var mark = _marks[i];
            var age =
                mark.AgeSeconds +
                deltaSeconds * rainMultiplier;

            if (age >= mark.LifetimeSeconds)
            {
                _marks.RemoveAt(i);
                continue;
            }

            _marks[i] = mark with { AgeSeconds = age };
        }
    }

    private static float Stride(WildlifeSpecies species) => species switch
    {
        WildlifeSpecies.Deer => 1.18f,
        WildlifeSpecies.Boar => 0.82f,
        WildlifeSpecies.Wolf => 0.95f,
        _ => float.MaxValue
    };
}
