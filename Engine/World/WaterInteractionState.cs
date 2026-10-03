using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed class WaterInteractionState
{
    private const float SplashStrideMeters = 1.15f;
    private const float WettingRatePerSecond = 1.65f;
    private const float DryingRatePerSecond = 1f / 95f;
    private const float RainWettingRatePerSecond = 0.035f;

    private Vector3 _previousPosition;
    private bool _hasPrevious;
    private float _distanceSinceSplash;

    public bool IsInWater { get; private set; }
    public float WaterDepth { get; private set; }
    public float MovementIntensity { get; private set; }
    public float Wetness { get; private set; }
    public float Chill { get; private set; }
    public float HeatExposure { get; private set; }
    public float RainExposure { get; private set; }
    public float SplashPulse { get; private set; }
    public Vector3 SurfacePosition { get; private set; }

    public float MovementSpeedMultiplier =>
        MovementSpeedMultiplierForDepth(WaterDepth);

    public float SprintStaminaMultiplier =>
        StaminaDrainMultiplierForDepth(WaterDepth);

    public string HudStatus
    {
        get
        {
            var chillText = Chill >= 0.08f
                ? $" / WYCHLODZENIE {Chill * 100f:0}%"
                : "";

            if (IsInWater)
            {
                var currentSpeed = CurrentSpeedForDepth(WaterDepth);
                var currentText = currentSpeed >= 0.12f
                    ? $" / NURT {currentSpeed:0.0} M/S"
                    : "";
                return $"WODA {WaterDepth * 100f:0} CM / MOKRY {Wetness * 100f:0}%{currentText}{chillText}";
            }

            if (HeatExposure >= 0.08f && (Wetness >= 0.03f || Chill >= 0.03f))
            {
                return $"PRZY OGNISKU / MOKRY {Wetness * 100f:0}%{chillText}";
            }

            if (RainExposure >= 0.10f)
            {
                return $"DESZCZ / MOKRY {Wetness * 100f:0}%{chillText}";
            }

            return Wetness >= 0.08f || Chill >= 0.08f
                ? $"MOKRY {Wetness * 100f:0}%{chillText}"
                : "";
        }
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        SplashPulse = MathF.Max(
            0f,
            SplashPulse - 3.6f * (float)deltaSeconds);

        var position = world.PlayerPosition;
        HeatExposure = world.Campfires.HeatAt(world, position);
        RainExposure = Math.Clamp(world.Weather.RainIntensity, 0f, 1f);

        WaterDepth = DepthAt(world, position);
        IsInWater = WaterDepth > 0.03f;

        var waterLevel = WaterLandscape.WaterLevel(position.Z);
        SurfacePosition = new Vector3(
            position.X,
            waterLevel + 0.035f,
            position.Z);

        var delta = _hasPrevious
            ? new Vector2(
                position.X - _previousPosition.X,
                position.Z - _previousPosition.Z)
            : Vector2.Zero;

        var dt = Math.Max(deltaSeconds, 0.0001d);
        var speed = delta.Length() / (float)dt;
        var targetMovement = IsInWater
            ? Math.Clamp(speed / 5.0f, 0f, 1f)
            : 0f;

        var response = IsInWater ? 10f : 4f;
        var blend = 1f - MathF.Exp(
            -response * (float)Math.Max(0d, deltaSeconds));
        MovementIntensity +=
            (targetMovement - MovementIntensity) * blend;

        if (IsInWater)
        {
            var depthWetness =
                Math.Clamp(WaterDepth / 0.75f, 0.18f, 1f);
            Wetness = Math.Clamp(
                Wetness +
                WettingRatePerSecond *
                depthWetness *
                (float)deltaSeconds,
                0f,
                1f);

            _distanceSinceSplash += delta.Length();
            if (speed > 0.25f &&
                _distanceSinceSplash >= SplashStrideMeters)
            {
                _distanceSinceSplash %= SplashStrideMeters;
                SplashPulse = Math.Clamp(
                    0.45f +
                    MovementIntensity * 0.55f +
                    Math.Clamp(WaterDepth / 0.8f, 0f, 1f) * 0.20f,
                    0f,
                    1f);
            }
        }
        else
        {
            _distanceSinceSplash = 0f;

            var rainWetting =
                RainWettingRatePerSecond *
                RainExposure *
                (1f - HeatExposure * 0.75f);

            var drying =
                DryingRatePerSecond *
                (1f + HeatExposure * 9f) *
                (1f - RainExposure * 0.88f);

            Wetness = Math.Clamp(
                Wetness +
                (rainWetting - drying) * (float)deltaSeconds,
                0f,
                1f);
        }

        UpdateChill(world, deltaSeconds);

        if (!IsInWater && MovementIntensity < 0.01f)
            MovementIntensity = 0f;

        _previousPosition = position;
        _hasPrevious = true;
    }

    public void Reset(Vector3 position)
    {
        _previousPosition = position;
        _hasPrevious = true;
        _distanceSinceSplash = 0f;
        WaterDepth = 0f;
        MovementIntensity = 0f;
        Wetness = 0f;
        Chill = 0f;
        HeatExposure = 0f;
        RainExposure = 0f;
        SplashPulse = 0f;
        IsInWater = false;
        SurfacePosition = position;
    }

    public static float DepthAt(
        WorldState world,
        Vector3 position)
    {
        ArgumentNullException.ThrowIfNull(world);

        var horizontalDistance =
            MathF.Abs(
                position.X -
                WaterLandscape.CenterX(position.Z));
        if (horizontalDistance >
            WaterLandscape.SurfaceHalfWidth(position.Z))
        {
            return 0f;
        }

        var waterLevel =
            WaterLandscape.WaterLevel(position.Z);
        var ground =
            world.Terrain.SampleHeight(position);

        return MathF.Max(0f, waterLevel - ground);
    }

    public static float MovementSpeedMultiplierForDepth(
        float depth)
    {
        depth = MathF.Max(0f, depth);

        if (depth <= 0.08f)
            return 1f;
        if (depth <= 0.35f)
            return Lerp(1f, 0.86f, (depth - 0.08f) / 0.27f);
        if (depth <= 0.75f)
            return Lerp(0.86f, 0.68f, (depth - 0.35f) / 0.40f);

        return Lerp(
            0.68f,
            0.55f,
            Math.Clamp((depth - 0.75f) / 0.60f, 0f, 1f));
    }

    public static bool CanSprintAtDepth(float depth) =>
        depth < 0.58f;

    public static float StaminaDrainMultiplierForDepth(
        float depth)
    {
        depth = MathF.Max(0f, depth);
        return 1f +
               Math.Clamp(depth / 0.75f, 0f, 1f) * 0.85f;
    }

    public static float StaminaRecoveryMultiplier(
        float depth,
        float wetness,
        float chill = 0f)
    {
        var depthPenalty =
            Math.Clamp(depth / 0.75f, 0f, 1f) * 0.45f;
        var wetPenalty =
            Math.Clamp(wetness, 0f, 1f) * 0.15f;
        var chillPenalty =
            Math.Clamp(chill, 0f, 1f) * 0.22f;

        return Math.Clamp(
            1f - depthPenalty - wetPenalty - chillPenalty,
            0.25f,
            1f);
    }

    public static float CurrentSpeedForDepth(float depth)
    {
        depth = MathF.Max(0f, depth);
        if (depth <= 0.28f)
            return 0f;

        var normalized =
            Math.Clamp((depth - 0.28f) / 1.02f, 0f, 1f);
        return normalized * normalized * 1.65f;
    }

    public static Vector3 CurrentVelocityAt(
        WorldState world,
        Vector3 position)
    {
        ArgumentNullException.ThrowIfNull(world);

        var depth = DepthAt(world, position);
        var speed = CurrentSpeedForDepth(depth);
        if (speed <= 0f)
            return Vector3.Zero;

        var flow = WaterLandscape.FlowDirection(position.Z);
        return new Vector3(
            flow.X * speed,
            0f,
            flow.Y * speed);
    }

    private void UpdateChill(
        WorldState world,
        double deltaSeconds)
    {
        var depthFactor =
            Math.Clamp(WaterDepth / 0.75f, 0f, 1f);
        var weatherPressure =
            RainExposure * 0.42f +
            Math.Clamp(world.Weather.WindIntensity, 0f, 1f) * 0.16f;

        var target =
            Wetness * (0.30f + weatherPressure) +
            depthFactor * 0.24f -
            HeatExposure * 1.15f;

        target = Math.Clamp(target, 0f, 1f);

        var response =
            target > Chill
                ? 0.018f
                : HeatExposure > 0.05f
                    ? 0.20f
                    : 0.035f;

        var blend =
            1f - MathF.Exp(
                -response *
                (float)Math.Max(0d, deltaSeconds));

        Chill += (target - Chill) * blend;

        if (Chill < 0.002f)
            Chill = 0f;
    }

    private static float Lerp(float a, float b, float t) =>
        a + (b - a) * Math.Clamp(t, 0f, 1f);
}
