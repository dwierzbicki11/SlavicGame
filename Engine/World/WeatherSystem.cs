namespace SlavicGame.Engine.World;

public enum WeatherKind
{
    Clear,
    Overcast,
    Fog,
    Rain,
    Storm
}

public sealed class WeatherSystem
{
    private readonly Random _random;
    private double _secondsUntilChange = 45.0;
    private float _targetFogDensity = 0.002f;
    private float _targetCloudiness = 0.1f;
    private float _targetRainIntensity;
    private float _targetWindIntensity = 0.15f;

    public WeatherKind Condition { get; private set; } = WeatherKind.Clear;
    public float FogDensity { get; private set; } = 0.002f;
    public float Cloudiness { get; private set; } = 0.1f;
    public float RainIntensity { get; private set; }
    public float WindIntensity { get; private set; } = 0.15f;
    public double SecondsUntilChange => _secondsUntilChange;

    public WeatherSystem(int seed = 20261001)
    {
        _random = new Random(seed);
    }

    public void Update(double deltaSeconds, WorldRegionType? regionType)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
        {
            return;
        }

        _secondsUntilChange -= deltaSeconds;
        while (_secondsUntilChange <= 0.0)
        {
            SetTarget(ChooseNext(regionType));
            _secondsUntilChange += 75.0 + _random.NextDouble() * 105.0;
        }

        var response = 0.18f * (float)deltaSeconds;
        FogDensity = MoveTowards(FogDensity, _targetFogDensity, response * 0.03f);
        Cloudiness = MoveTowards(Cloudiness, _targetCloudiness, response);
        RainIntensity = MoveTowards(RainIntensity, _targetRainIntensity, response);
        WindIntensity = MoveTowards(WindIntensity, _targetWindIntensity, response);
    }

    public void SetCondition(WeatherKind kind, bool immediate = false)
    {
        SetTarget(kind);
        if (!immediate)
        {
            return;
        }

        FogDensity = _targetFogDensity;
        Cloudiness = _targetCloudiness;
        RainIntensity = _targetRainIntensity;
        WindIntensity = _targetWindIntensity;
    }

    private void SetTarget(WeatherKind kind)
    {
        Condition = kind;
        (_targetFogDensity, _targetCloudiness, _targetRainIntensity, _targetWindIntensity) = kind switch
        {
            WeatherKind.Clear => (0.002f, 0.10f, 0.00f, 0.15f),
            WeatherKind.Overcast => (0.006f, 0.72f, 0.00f, 0.35f),
            WeatherKind.Fog => (0.032f, 0.62f, 0.00f, 0.12f),
            WeatherKind.Rain => (0.011f, 0.82f, 0.65f, 0.52f),
            WeatherKind.Storm => (0.017f, 1.00f, 1.00f, 0.92f),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    private WeatherKind ChooseNext(WorldRegionType? regionType)
    {
        var roll = _random.Next(100);
        return regionType switch
        {
            WorldRegionType.Swamp => roll switch
            {
                < 10 => WeatherKind.Clear,
                < 30 => WeatherKind.Overcast,
                < 65 => WeatherKind.Fog,
                < 95 => WeatherKind.Rain,
                _ => WeatherKind.Storm
            },
            WorldRegionType.Shrine => roll switch
            {
                < 25 => WeatherKind.Clear,
                < 50 => WeatherKind.Overcast,
                < 80 => WeatherKind.Fog,
                < 95 => WeatherKind.Rain,
                _ => WeatherKind.Storm
            },
            WorldRegionType.Village => roll switch
            {
                < 45 => WeatherKind.Clear,
                < 75 => WeatherKind.Overcast,
                < 83 => WeatherKind.Fog,
                < 98 => WeatherKind.Rain,
                _ => WeatherKind.Storm
            },
            _ => roll switch
            {
                < 30 => WeatherKind.Clear,
                < 55 => WeatherKind.Overcast,
                < 75 => WeatherKind.Fog,
                < 95 => WeatherKind.Rain,
                _ => WeatherKind.Storm
            }
        };
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
        {
            return target;
        }

        return current + MathF.CopySign(maxDelta, target - current);
    }
}
