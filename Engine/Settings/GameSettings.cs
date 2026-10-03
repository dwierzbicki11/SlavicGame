namespace SlavicGame.Engine.Settings;

public enum CameraPreference
{
    FirstPerson,
    ThirdPerson
}

public enum RenderResolution
{
    Qhd540,
    Hd576,
    Hd648,
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

public enum RenderDistanceQuality
{
    VeryLow,
    Low,
    Medium,
    High,
    Ultra
}

public enum VegetationDistanceQuality
{
    Off,
    Short,
    Medium,
    Far,
    Ultra
}

public enum GroundClutterQuality
{
    Off,
    Low,
    Medium,
    High
}

public enum ShadowDistanceQuality
{
    Short,
    Medium,
    Far,
    Ultra
}

public enum TerrainDetailQuality
{
    Low,
    Medium,
    High,
    Ultra
}

public enum ModelLodQuality
{
    Aggressive,
    Balanced,
    Quality,
    Ultra
}

public enum UpscalerMode
{
    Bilinear,
    Fsr1
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
            RenderResolution.Qhd540 => new(960, 540),
            RenderResolution.Hd576 => new(1024, 576),
            RenderResolution.Hd648 => new(1152, 648),
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

    public static float RenderDistance(RenderDistanceQuality quality) =>
        quality switch
        {
            RenderDistanceQuality.VeryLow => 180f,
            RenderDistanceQuality.Low => 300f,
            RenderDistanceQuality.Medium => 450f,
            RenderDistanceQuality.High => 650f,
            RenderDistanceQuality.Ultra => 900f,
            _ => 650f
        };

    public static float VegetationDistance(VegetationDistanceQuality quality) =>
        quality switch
        {
            VegetationDistanceQuality.Off => 0f,
            VegetationDistanceQuality.Short => 90f,
            VegetationDistanceQuality.Medium => 180f,
            VegetationDistanceQuality.Far => 320f,
            VegetationDistanceQuality.Ultra => 520f,
            _ => 320f
        };

    public static float GroundClutterDistance(GroundClutterQuality quality) =>
        quality switch
        {
            GroundClutterQuality.Off => 0f,
            GroundClutterQuality.Low => 30f,
            GroundClutterQuality.Medium => 70f,
            GroundClutterQuality.High => 140f,
            _ => 70f
        };

    public static float ShadowDistance(ShadowDistanceQuality quality) =>
        quality switch
        {
            ShadowDistanceQuality.Short => 70f,
            ShadowDistanceQuality.Medium => 120f,
            ShadowDistanceQuality.Far => 180f,
            ShadowDistanceQuality.Ultra => 210f,
            _ => 120f
        };

    public static float TerrainDetailLevel(TerrainDetailQuality quality) =>
        quality switch
        {
            TerrainDetailQuality.Low => 0f,
            TerrainDetailQuality.Medium => 1f,
            TerrainDetailQuality.High => 2f,
            TerrainDetailQuality.Ultra => 3f,
            _ => 2f
        };

    public static int TerrainMeshStep(TerrainDetailQuality quality) =>
        quality switch
        {
            TerrainDetailQuality.Low => 4,
            TerrainDetailQuality.Medium => 2,
            TerrainDetailQuality.High => 1,
            TerrainDetailQuality.Ultra => 1,
            _ => 1
        };

    public static (float Lod1, float Lod2) ModelLodDistances(ModelLodQuality quality) =>
        quality switch
        {
            ModelLodQuality.Aggressive => (18f, 45f),
            ModelLodQuality.Balanced => (30f, 90f),
            ModelLodQuality.Quality => (55f, 160f),
            ModelLodQuality.Ultra => (90f, 280f),
            _ => (55f, 160f)
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
                settings.RenderDistance = RenderDistanceQuality.VeryLow;
                settings.VegetationDistance = VegetationDistanceQuality.Short;
                settings.GroundClutter = GroundClutterQuality.Off;
                settings.ShadowDistance = ShadowDistanceQuality.Short;
                settings.TerrainDetail = TerrainDetailQuality.Low;
                settings.ModelLod = ModelLodQuality.Aggressive;
                settings.Upscaler = UpscalerMode.Fsr1;
                settings.FsrSharpness = 0.55f;
                settings.NormalMapping = false;
                settings.SpecularHighlights = false;
                break;

            case GraphicsPreset.Balanced:
                settings.VolumetricClouds = true;
                settings.CloudQuality = CloudQuality.Medium;
                settings.CloudShadows = false;
                settings.SunShadows = true;
                settings.ShadowQuality = ShadowQuality.Low;
                settings.TextureQuality = TextureQuality.Medium;
                settings.RenderDistance = RenderDistanceQuality.Medium;
                settings.VegetationDistance = VegetationDistanceQuality.Medium;
                settings.GroundClutter = GroundClutterQuality.Low;
                settings.ShadowDistance = ShadowDistanceQuality.Medium;
                settings.TerrainDetail = TerrainDetailQuality.Medium;
                settings.ModelLod = ModelLodQuality.Balanced;
                settings.Upscaler = UpscalerMode.Fsr1;
                settings.FsrSharpness = 0.45f;
                settings.NormalMapping = true;
                settings.SpecularHighlights = false;
                break;

            case GraphicsPreset.High:
                settings.VolumetricClouds = true;
                settings.CloudQuality = CloudQuality.High;
                settings.CloudShadows = true;
                settings.SunShadows = true;
                settings.ShadowQuality = ShadowQuality.Medium;
                settings.TextureQuality = TextureQuality.High;
                settings.RenderDistance = RenderDistanceQuality.High;
                settings.VegetationDistance = VegetationDistanceQuality.Far;
                settings.GroundClutter = GroundClutterQuality.Medium;
                settings.ShadowDistance = ShadowDistanceQuality.Far;
                settings.TerrainDetail = TerrainDetailQuality.High;
                settings.ModelLod = ModelLodQuality.Quality;
                settings.Upscaler = UpscalerMode.Fsr1;
                settings.FsrSharpness = 0.35f;
                settings.NormalMapping = true;
                settings.SpecularHighlights = true;
                break;

            case GraphicsPreset.Ultra:
                settings.VolumetricClouds = true;
                settings.CloudQuality = CloudQuality.Ultra;
                settings.CloudShadows = true;
                settings.SunShadows = true;
                settings.ShadowQuality = ShadowQuality.High;
                settings.TextureQuality = TextureQuality.Ultra;
                settings.RenderDistance = RenderDistanceQuality.Ultra;
                settings.VegetationDistance = VegetationDistanceQuality.Ultra;
                settings.GroundClutter = GroundClutterQuality.High;
                settings.ShadowDistance = ShadowDistanceQuality.Ultra;
                settings.TerrainDetail = TerrainDetailQuality.Ultra;
                settings.ModelLod = ModelLodQuality.Ultra;
                settings.Upscaler = UpscalerMode.Fsr1;
                settings.FsrSharpness = 0.25f;
                settings.NormalMapping = true;
                settings.SpecularHighlights = true;
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
            RenderDistance = source.RenderDistance,
            VegetationDistance = source.VegetationDistance,
            GroundClutter = source.GroundClutter,
            ShadowDistance = source.ShadowDistance,
            TerrainDetail = source.TerrainDetail,
            ModelLod = source.ModelLod,
            Upscaler = source.Upscaler,
            FsrSharpness = source.FsrSharpness,
            NormalMapping = source.NormalMapping,
            SpecularHighlights = source.SpecularHighlights,
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
        left.RenderDistance == right.RenderDistance &&
        left.VegetationDistance == right.VegetationDistance &&
        left.GroundClutter == right.GroundClutter &&
        left.ShadowDistance == right.ShadowDistance &&
        left.TerrainDetail == right.TerrainDetail &&
        left.ModelLod == right.ModelLod &&
        left.Upscaler == right.Upscaler &&
        MathF.Abs(left.FsrSharpness - right.FsrSharpness) < 0.001f &&
        left.NormalMapping == right.NormalMapping &&
        left.SpecularHighlights == right.SpecularHighlights &&
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
    public RenderResolution WindowResolution { get; set; } = RenderResolution.Hd720;

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
    public RenderDistanceQuality RenderDistance { get; set; } = RenderDistanceQuality.High;
    public VegetationDistanceQuality VegetationDistance { get; set; } = VegetationDistanceQuality.Far;
    public GroundClutterQuality GroundClutter { get; set; } = GroundClutterQuality.Medium;
    public ShadowDistanceQuality ShadowDistance { get; set; } = ShadowDistanceQuality.Far;
    public TerrainDetailQuality TerrainDetail { get; set; } = TerrainDetailQuality.High;
    public ModelLodQuality ModelLod { get; set; } = ModelLodQuality.Quality;
    public UpscalerMode Upscaler { get; set; } = UpscalerMode.Fsr1;
    public float FsrSharpness { get; set; } = 0.35f;
    public bool NormalMapping { get; set; } = true;
    public bool SpecularHighlights { get; set; } = true;
    public bool Fog { get; set; } = true;
    public bool TerrainPbr { get; set; } = true;
    public bool ModelPbr { get; set; } = true;

    public ResolutionSize ResolutionSize => ResolutionCatalog.Get(Resolution);
    public ResolutionSize WindowResolutionSize => ResolutionCatalog.Get(WindowResolution);

    public void Normalize()
    {
        FieldOfViewDegrees = Math.Clamp(FieldOfViewDegrees, 50f, 100f);
        MouseSensitivity = Math.Clamp(MouseSensitivity, 0.25f, 3.0f);

        if (!Enum.IsDefined(Resolution))
            Resolution = RenderResolution.Hd720;
        if (!Enum.IsDefined(WindowResolution))
            WindowResolution = RenderResolution.Hd720;
        if (!Enum.IsDefined(CloudQuality))
            CloudQuality = CloudQuality.High;
        if (!Enum.IsDefined(ShadowQuality))
            ShadowQuality = ShadowQuality.Medium;
        if (!Enum.IsDefined(TextureQuality))
            TextureQuality = TextureQuality.High;
        if (!Enum.IsDefined(RenderDistance))
            RenderDistance = RenderDistanceQuality.High;
        if (!Enum.IsDefined(VegetationDistance))
            VegetationDistance = VegetationDistanceQuality.Far;
        if (!Enum.IsDefined(GroundClutter))
            GroundClutter = GroundClutterQuality.Medium;
        if (!Enum.IsDefined(ShadowDistance))
            ShadowDistance = ShadowDistanceQuality.Far;
        if (!Enum.IsDefined(TerrainDetail))
            TerrainDetail = TerrainDetailQuality.High;
        if (!Enum.IsDefined(ModelLod))
            ModelLod = ModelLodQuality.Quality;
        if (!Enum.IsDefined(Upscaler))
            Upscaler = UpscalerMode.Fsr1;
        FsrSharpness = Math.Clamp(FsrSharpness, 0f, 1f);
    }
}
