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
    }
}
