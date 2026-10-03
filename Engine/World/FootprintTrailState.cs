using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record TransientFootprint(
    Vector3 Position,
    Vector2 Forward,
    bool LeftFoot,
    float AgeSeconds,
    float LifetimeSeconds,
    float Strength);

public sealed class FootprintTrailState
{
    private const int MaxFootprints = 96;
    private const float StepDistance = 0.72f;
    private const float LateralOffset = 0.15f;

    private readonly List<TransientFootprint> _footprints = [];
    private Vector3 _previousPosition;
    private bool _hasPrevious;
    private float _distanceAccumulator;
    private bool _nextLeft = true;

    public IReadOnlyList<TransientFootprint> Footprints => _footprints;

    public void Reset(Vector3 position)
    {
        _footprints.Clear();
        _previousPosition = position;
        _hasPrevious = true;
        _distanceAccumulator = 0f;
        _nextLeft = true;
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        AgeExisting(world, (float)deltaSeconds);

        var position = world.PlayerPosition;
        if (!_hasPrevious)
        {
            Reset(position);
            return;
        }

        var delta3 = position - _previousPosition;
        var delta = new Vector2(delta3.X, delta3.Z);
        var distance = delta.Length();

        if (distance <= 0.0001f)
        {
            _previousPosition = position;
            return;
        }

        var direction = Vector2.Normalize(delta);
        _distanceAccumulator += distance;

        while (_distanceAccumulator >= StepDistance)
        {
            _distanceAccumulator -= StepDistance;

            var samplePosition = position;
            var groundPosition = new Vector3(
                samplePosition.X,
                world.Terrain.SampleHeight(samplePosition),
                samplePosition.Z);

            var weights = TerrainSurfaceClassifier.Classify(
                groundPosition,
                Vector3.UnitY);

            var trackability = Trackability(
                weights,
                world.WaterInteraction.Wetness,
                world.Weather.RainIntensity);

            if (!world.WaterInteraction.IsInWater &&
                trackability >= 0.26f)
            {
                AddFootprint(
                    world,
                    groundPosition,
                    direction,
                    trackability,
                    weights);
            }
        }

        _previousPosition = position;
    }

    public static float Trackability(
        TerrainSurfaceWeights weights,
        float wetness,
        float rainIntensity)
    {
        var material =
            weights.Mud * 1.00f +
            weights.Swamp * 0.72f +
            weights.Path * 0.18f +
            weights.ForestLitter * 0.10f;

        var moistureBoost =
            Math.Clamp(wetness, 0f, 1f) * 0.18f +
            Math.Clamp(rainIntensity, 0f, 1f) * 0.16f;

        var rockPenalty = weights.Rock * 0.92f;
        var grassPenalty = weights.Grass * 0.28f;

        return Math.Clamp(
            material + moistureBoost - rockPenalty - grassPenalty,
            0f,
            1f);
    }

    private void AddFootprint(
        WorldState world,
        Vector3 position,
        Vector2 forward,
        float trackability,
        TerrainSurfaceWeights weights)
    {
        var side = new Vector2(-forward.Y, forward.X);
        var lateral = (_nextLeft ? -1f : 1f) * LateralOffset;
        var footprintPosition = position;
        footprintPosition.X += side.X * lateral;
        footprintPosition.Z += side.Y * lateral;
        footprintPosition.Y =
            world.Terrain.SampleHeight(footprintPosition) + 0.022f;

        var mudFactor = Math.Clamp(
            weights.Mud + weights.Swamp * 0.75f,
            0f,
            1f);
        var lifetime =
            18f +
            mudFactor * 24f +
            trackability * 8f;

        _footprints.Add(new TransientFootprint(
            footprintPosition,
            forward,
            _nextLeft,
            0f,
            lifetime,
            trackability));

        _nextLeft = !_nextLeft;

        if (_footprints.Count > MaxFootprints)
            _footprints.RemoveRange(
                0,
                _footprints.Count - MaxFootprints);
    }

    private void AgeExisting(WorldState world, float deltaSeconds)
    {
        if (_footprints.Count == 0 || deltaSeconds <= 0f)
            return;

        var rainMultiplier =
            1f +
            Math.Clamp(world.Weather.RainIntensity, 0f, 1f) * 2.8f;

        for (var i = _footprints.Count - 1; i >= 0; i--)
        {
            var footprint = _footprints[i];
            var age =
                footprint.AgeSeconds +
                deltaSeconds * rainMultiplier;

            if (age >= footprint.LifetimeSeconds)
            {
                _footprints.RemoveAt(i);
                continue;
            }

            _footprints[i] = footprint with
            {
                AgeSeconds = age
            };
        }
    }
}
