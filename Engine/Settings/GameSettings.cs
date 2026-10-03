namespace SlavicGame.Engine.Settings;

public enum CameraPreference
{
    FirstPerson,
    ThirdPerson
}

public enum RenderResolution
{
    Hd720,
    Hd768,
    Hd900,
    FullHd1080,
    Qhd1440,
    Uhd2160
}

public enum CloudQuality
{
    Low,
    Medium,
    High,
    Ultra
}

public enum ShadowQuality
{
    Low,
    Medium,
    High
}

public enum TextureQuality
{
    Low,
    Medium,
    High,
    Ultra
}

public enum GraphicsPreset
{
    LowEnd,
    Balanced,
    High,
    Ultra
}

public readonly record struct ResolutionSize(int Width, int Height)
{
    public override string ToString() => $"{Width}X{Height}";
}

public static class ResolutionCatalog
{
    public static ResolutionSize Get(RenderResolution resolution) =>
        resolution switch
        {
            RenderResolution.Hd720 => new(1280, 720),
            RenderResolution.Hd768 => new(1366, 768),
            RenderResolution.Hd900 => new(1600, 900),
            RenderResolution.FullHd1080 => new(1920, 1080),
            RenderResolution.Qhd1440 => new(2560, 1440),
            RenderResolution.Uhd2160 => new(3840, 2160),
            _ => new(1280, 720)
        };

    public static RenderResolution Cycle(
        RenderResolution current,
        int direction)
    {
        var values = Enum.GetValues<RenderResolution>();
        var index = Array.IndexOf(values, current);
        if (index < 0) index = 0;

        var next = (index + Math.Sign(direction)) % values.Length;
        if (next < 0) next += values.Length;
        return values[next];
    }
}

public static class GraphicsQualityCatalog
{
    public static int CloudRaymarchSteps(CloudQuality quality) =>
        quality switch
        {
            CloudQuality.Low => 6,
            CloudQuality.Medium => 10,
            CloudQuality.High => 14,
            CloudQuality.Ultra => 20,
            _ => 14
        };

    public static uint ShadowMapSize(ShadowQuality quality) =>
        quality switch
        {
            ShadowQuality.Low => 1024,
            ShadowQuality.Medium => 2048,
            ShadowQuality.High => 4096,
            _ => 2048
        };

    public static int TextureMaximumDimension(TextureQuality quality) =>
        quality switch
        {
            TextureQuality.Low => 512,
            TextureQuality.Medium => 1024,
            TextureQuality.High => 2048,
            TextureQuality.Ultra => int.MaxValue,
            _ => 2048
        };
}

public static class GraphicsPresetCatalog
{
    public static void Apply(GameSettings settings, GraphicsPreset preset)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.Sky = true;
        settings.Sun = true;
        settings.Moon = true;
        settings.Stars = true;
        settings.Fog = true;
        settings.TerrainPbr = true;
        settings.ModelPbr = true;

        switch (preset)
        {
            case GraphicsPreset.LowEnd:
                settings.VolumetricClouds = false;
                settings.CloudQuality = CloudQuality.Low;
                settings.CloudShadows = false;
                settings.SunShadows = false;
                settings.ShadowQuality = ShadowQuality.Low;
                settings.TextureQuality = TextureQuality.Low;
                break;

            case GraphicsPreset.Balanced:
                settings.VolumetricClouds = true;
                settings.CloudQuality = CloudQuality.Medium;
                settings.CloudShadows = false;
                settings.SunShadows = true;
                settings.ShadowQuality = ShadowQuality.Low;
                settings.TextureQuality = TextureQuality.Medium;
                break;

            case GraphicsPreset.High:
                settings.VolumetricClouds = true;
                settings.CloudQuality = CloudQuality.High;
                settings.CloudShadows = true;
                settings.SunShadows = true;
                settings.ShadowQuality = ShadowQuality.Medium;
                settings.TextureQuality = TextureQuality.High;
                break;

            case GraphicsPreset.Ultra:
                settings.VolumetricClouds = true;
                settings.CloudQuality = CloudQuality.Ultra;
                settings.CloudShadows = true;
                settings.SunShadows = true;
                settings.ShadowQuality = ShadowQuality.High;
                settings.TextureQuality = TextureQuality.Ultra;
                break;
        }
    }

    public static string DetectName(GameSettings settings)
    {
        foreach (var preset in Enum.GetValues<GraphicsPreset>())
        {
            var candidate = CloneQualitySettings(settings);
            Apply(candidate, preset);
            if (MatchesQualitySettings(settings, candidate))
                return preset switch
                {
                    GraphicsPreset.LowEnd => "LOW-END",
                    GraphicsPreset.Balanced => "BALANCED",
                    GraphicsPreset.High => "HIGH",
                    GraphicsPreset.Ultra => "ULTRA",
                    _ => "CUSTOM"
                };
        }

        return "CUSTOM";
    }

    private static GameSettings CloneQualitySettings(GameSettings source) =>
        new()
        {
            Sky = source.Sky,
            Sun = source.Sun,
            Moon = source.Moon,
            Stars = source.Stars,
            VolumetricClouds = source.VolumetricClouds,
            CloudQuality = source.CloudQuality,
            CloudShadows = source.CloudShadows,
            SunShadows = source.SunShadows,
            ShadowQuality = source.ShadowQuality,
            TextureQuality = source.TextureQuality,
            Fog = source.Fog,
            TerrainPbr = source.TerrainPbr,
            ModelPbr = source.ModelPbr
        };

    private static bool MatchesQualitySettings(GameSettings left, GameSettings right) =>
        left.Sky == right.Sky &&
        left.Sun == right.Sun &&
        left.Moon == right.Moon &&
        left.Stars == right.Stars &&
        left.VolumetricClouds == right.VolumetricClouds &&
        left.CloudQuality == right.CloudQuality &&
        left.CloudShadows == right.CloudShadows &&
        left.SunShadows == right.SunShadows &&
        left.ShadowQuality == right.ShadowQuality &&
        left.TextureQuality == right.TextureQuality &&
        left.Fog == right.Fog &&
        left.TerrainPbr == right.TerrainPbr &&
        left.ModelPbr == right.ModelPbr;
}

public sealed class GameSettings
{
    public bool Fullscreen { get; set; } = true;
    public bool VSync { get; set; }
    public bool ShowFps { get; set; } = true;
    public RenderResolution Resolution { get; set; } = RenderResolution.Hd720;

    public float FieldOfViewDegrees { get; set; } = 60f;
    public float MouseSensitivity { get; set; } = 1.0f;
    public CameraPreference Camera { get; set; } = CameraPreference.FirstPerson;

    public bool Sky { get; set; } = true;
    public bool Sun { get; set; } = true;
    public bool Moon { get; set; } = true;
    public bool Stars { get; set; } = true;
    public bool VolumetricClouds { get; set; } = true;
    public CloudQuality CloudQuality { get; set; } = CloudQuality.High;
    public bool CloudShadows { get; set; } = true;
    public bool SunShadows { get; set; } = true;
    public ShadowQuality ShadowQuality { get; set; } = ShadowQuality.Medium;
    public TextureQuality TextureQuality { get; set; } = TextureQuality.High;
    public bool Fog { get; set; } = true;
    public bool TerrainPbr { get; set; } = true;
    public bool ModelPbr { get; set; } = true;

    public ResolutionSize ResolutionSize => ResolutionCatalog.Get(Resolution);

    public void Normalize()
    {
        FieldOfViewDegrees = Math.Clamp(FieldOfViewDegrees, 50f, 100f);
        MouseSensitivity = Math.Clamp(MouseSensitivity, 0.25f, 3.0f);

        if (!Enum.IsDefined(Resolution))
            Resolution = RenderResolution.Hd720;
        if (!Enum.IsDefined(CloudQuality))
            CloudQuality = CloudQuality.High;
        if (!Enum.IsDefined(ShadowQuality))
            ShadowQuality = ShadowQuality.Medium;
        if (!Enum.IsDefined(TextureQuality))
            TextureQuality = TextureQuality.High;
    }
}
