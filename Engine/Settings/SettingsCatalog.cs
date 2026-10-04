namespace SlavicGame.Engine.Settings;

public enum SettingCategory
{
    Display,
    Controls,
    Graphics,
    PostProcessing,
    Audio
}

public sealed record SettingDefinition(
    string Id,
    SettingCategory Category,
    string Label,
    Func<GameSettings, string> ValueText,
    Action<GameSettings, int> Change,
    bool RequiresRestart = false);

public static class SettingsCatalog
{
    public static IReadOnlyList<SettingDefinition> All { get; } =
    [
        Toggle("fullscreen", SettingCategory.Display, "PELNY EKRAN",
            s => s.Fullscreen, (s, v) => s.Fullscreen = v),
        new(
            "resolution",
            SettingCategory.Display,
            "ROZDZIELCZOSC RENDERU",
            s => s.Upscaler is UpscalerMode.Fsr1 or UpscalerMode.Fsr3 &&
                 s.FsrQuality != FsrQualityMode.Custom
                ? $"AUTO {FsrQualityName(s.FsrQuality)}"
                : s.ResolutionSize.ToString(),
            (s, direction) =>
            {
                s.Resolution = ResolutionCatalog.Cycle(s.Resolution, direction);
                if (s.Upscaler is UpscalerMode.Fsr1 or UpscalerMode.Fsr3)
                    s.FsrQuality = FsrQualityMode.Custom;
            }),
        new(
            "window-resolution",
            SettingCategory.Display,
            "ROZDZIELCZOSC OKNA",
            s => s.WindowResolutionSize.ToString(),
            (s, direction) =>
                s.WindowResolution = ResolutionCatalog.Cycle(
                    s.WindowResolution,
                    direction)),
        Toggle("vsync", SettingCategory.Display, "VSYNC",
            s => s.VSync, (s, v) => s.VSync = v),
        Toggle("fps", SettingCategory.Display, "LICZNIK FPS",
            s => s.ShowFps, (s, v) => s.ShowFps = v),
        new(
            "fps-limit",
            SettingCategory.Display,
            "LIMIT FPS",
            s => FrameLimitName(s.FpsLimit),
            (s, direction) =>
                s.FpsLimit = CycleEnum(s.FpsLimit, direction)),

        Volume("master-volume", "GLOSNOSC GLOWNA",
            s => s.MasterVolume, (s, v) => s.MasterVolume = v),
        Volume("music-volume", "MUZYKA",
            s => s.MusicVolume, (s, v) => s.MusicVolume = v),
        Volume("ambience-volume", "AMBIENT",
            s => s.AmbienceVolume, (s, v) => s.AmbienceVolume = v),
        Volume("effects-volume", "EFEKTY",
            s => s.EffectsVolume, (s, v) => s.EffectsVolume = v),
        Volume("voice-volume", "GLOSY",
            s => s.VoiceVolume, (s, v) => s.VoiceVolume = v),
        Volume("ui-volume", "INTERFEJS",
            s => s.UiVolume, (s, v) => s.UiVolume = v),

        new(
            "fov",
            SettingCategory.Controls,
            "POLE WIDZENIA",
            s => $"{s.FieldOfViewDegrees:0} DEG",
            (s, direction) =>
            {
                s.FieldOfViewDegrees = Math.Clamp(
                    s.FieldOfViewDegrees + direction * 5f,
                    50f,
                    100f);
            }),
        new(
            "mouse",
            SettingCategory.Controls,
            "CZULOSC MYSZY",
            s => $"{s.MouseSensitivity:0.00}X",
            (s, direction) =>
            {
                s.MouseSensitivity = Math.Clamp(
                    s.MouseSensitivity + direction * 0.10f,
                    0.25f,
                    3.0f);
            }),
        new(
            "camera",
            SettingCategory.Controls,
            "KAMERA",
            s => s.Camera == CameraPreference.FirstPerson ? "FPP" : "TPP",
            (s, _) =>
            {
                s.Camera = s.Camera == CameraPreference.FirstPerson
                    ? CameraPreference.ThirdPerson
                    : CameraPreference.FirstPerson;
            }),

        new(
            "graphics-preset",
            SettingCategory.Graphics,
            "PRESET GRAFIKI",
            GraphicsPresetCatalog.DetectName,
            (s, direction) =>
            {
                var presets = Enum.GetValues<GraphicsPreset>();
                var currentName = GraphicsPresetCatalog.DetectName(s);
                var currentIndex = currentName switch
                {
                    "LOW-END" => Array.IndexOf(presets, GraphicsPreset.LowEnd),
                    "BALANCED" => Array.IndexOf(presets, GraphicsPreset.Balanced),
                    "HIGH" => Array.IndexOf(presets, GraphicsPreset.High),
                    "ULTRA" => Array.IndexOf(presets, GraphicsPreset.Ultra),
                    _ => Array.IndexOf(presets, GraphicsPreset.High)
                };
                var next = (currentIndex + Math.Sign(direction)) % presets.Length;
                if (next < 0) next += presets.Length;
                GraphicsPresetCatalog.Apply(s, presets[next]);
            },
            RequiresRestart: true),
        new(
            "texture-quality",
            SettingCategory.Graphics,
            "JAKOSC TEKSTUR",
            s => TextureName(s.TextureQuality),
            (s, direction) =>
                s.TextureQuality = CycleEnum(s.TextureQuality, direction),
            RequiresRestart: true),
        new(
            "render-distance",
            SettingCategory.Graphics,
            "ZASIEG SWIATA",
            s => DistanceName(s.RenderDistance),
            (s, direction) =>
                s.RenderDistance = CycleEnum(s.RenderDistance, direction)),
        new(
            "vegetation-distance",
            SettingCategory.Graphics,
            "ZASIEG ROSLINNOSCI",
            s => VegetationDistanceName(s.VegetationDistance),
            (s, direction) =>
                s.VegetationDistance = CycleEnum(s.VegetationDistance, direction)),
        new(
            "ground-clutter",
            SettingCategory.Graphics,
            "DROBNA ROSLINNOSC",
            s => GroundClutterName(s.GroundClutter),
            (s, direction) =>
                s.GroundClutter = CycleEnum(s.GroundClutter, direction)),
        new(
            "shadow-distance",
            SettingCategory.Graphics,
            "ZASIEG CIENI",
            s => s.SunShadows
                ? ShadowDistanceName(s.ShadowDistance)
                : "NIEAKTYWNE",
            (s, direction) =>
                s.ShadowDistance = CycleEnum(s.ShadowDistance, direction)),
        new(
            "terrain-detail",
            SettingCategory.Graphics,
            "JAKOSC TERENU",
            s => TerrainDetailName(s.TerrainDetail),
            (s, direction) =>
                s.TerrainDetail = CycleEnum(s.TerrainDetail, direction)),
        new(
            "model-lod",
            SettingCategory.Graphics,
            "LOD MODELI",
            s => ModelLodName(s.ModelLod),
            (s, direction) =>
                s.ModelLod = CycleEnum(s.ModelLod, direction)),
        new(
            "far-vegetation",
            SettingCategory.Graphics,
            "DALEKIE DRZEWA",
            s => FarVegetationName(s.FarVegetation),
            (s, direction) =>
                s.FarVegetation = CycleEnum(s.FarVegetation, direction)),

        new(
            "upscaler",
            SettingCategory.PostProcessing,
            "UPSCALER",
            s => s.Upscaler switch
            {
                UpscalerMode.Fsr3 => "FSR3",
                UpscalerMode.Fsr1 => "FSR1",
                _ => "BILINEAR"
            },
            (s, direction) =>
                s.Upscaler = CycleEnum(s.Upscaler, direction),
            RequiresRestart: true),
        new(
            "fsr-quality",
            SettingCategory.PostProcessing,
            "FSR TRYB",
            s => s.Upscaler is UpscalerMode.Fsr1 or UpscalerMode.Fsr3
                ? FsrQualityName(s.FsrQuality)
                : "NIEAKTYWNE",
            (s, direction) =>
                s.FsrQuality = CycleEnum(s.FsrQuality, direction)),
        new(
            "fsr-sharpness",
            SettingCategory.PostProcessing,
            "FSR OSTROSC",
            s => s.Upscaler is UpscalerMode.Fsr1 or UpscalerMode.Fsr3 &&
                 s.FsrQuality != FsrQualityMode.Native
                ? $"{s.FsrSharpness:0.00}"
                : "NIEAKTYWNE",
            (s, direction) =>
                s.FsrSharpness = Math.Clamp(
                    s.FsrSharpness + direction * 0.05f,
                    0f,
                    1f)),
        new(
            "anti-aliasing",
            SettingCategory.PostProcessing,
            "ANTYALIASING",
            s => s.AntiAliasing == AntiAliasingMode.Fxaa ? "FXAA" : "WYL",
            (s, direction) =>
                s.AntiAliasing = CycleEnum(s.AntiAliasing, direction)),
        new(
            "msaa",
            SettingCategory.PostProcessing,
            "MSAA",
            s => MsaaName(s.Msaa),
            (s, direction) =>
                s.Msaa = CycleEnum(s.Msaa, direction),
            RequiresRestart: true),
        new(
            "bloom",
            SettingCategory.PostProcessing,
            "BLOOM",
            s => BloomName(s.Bloom),
            (s, direction) =>
                s.Bloom = CycleEnum(s.Bloom, direction)),
        new(
            "bloom-strength",
            SettingCategory.PostProcessing,
            "SILA BLOOM",
            s => s.Bloom == BloomQuality.Off
                ? "NIEAKTYWNE"
                : $"{s.BloomStrength:0.00}",
            (s, direction) =>
                s.BloomStrength = Math.Clamp(
                    s.BloomStrength + direction * 0.05f,
                    0f,
                    1.5f)),
        new(
            "brightness",
            SettingCategory.PostProcessing,
            "JASNOSC",
            s => $"{s.Brightness:0.00}",
            (s, direction) =>
                s.Brightness = Math.Clamp(
                    s.Brightness + direction * 0.05f,
                    0.5f,
                    1.5f)),
        new(
            "gamma",
            SettingCategory.PostProcessing,
            "GAMMA",
            s => $"{s.Gamma:0.00}",
            (s, direction) =>
                s.Gamma = Math.Clamp(
                    s.Gamma + direction * 0.05f,
                    1.6f,
                    2.8f)),

        Toggle("normal-mapping", SettingCategory.Graphics, "NORMAL MAPPING",
            s => s.NormalMapping, (s, v) => s.NormalMapping = v),
        Toggle("specular", SettingCategory.Graphics, "ODBICIA SPECULAR",
            s => s.SpecularHighlights, (s, v) => s.SpecularHighlights = v),
        Toggle("sky", SettingCategory.Graphics, "NIEBO",
            s => s.Sky, (s, v) => s.Sky = v),
        Toggle("sun", SettingCategory.Graphics, "SLONCE",
            s => s.Sun, (s, v) => s.Sun = v),
        Toggle("moon", SettingCategory.Graphics, "KSIEZYC",
            s => s.Moon, (s, v) => s.Moon = v),
        Toggle("stars", SettingCategory.Graphics, "GWIAZDY",
            s => s.Stars, (s, v) => s.Stars = v),
        Toggle("volumetric-clouds", SettingCategory.Graphics, "CHMURY VOLUMETRYCZNE",
            s => s.VolumetricClouds, (s, v) => s.VolumetricClouds = v),
        new(
            "cloud-quality",
            SettingCategory.Graphics,
            "JAKOSC CHMUR",
            s => s.VolumetricClouds
                ? QualityName(s.CloudQuality)
                : "NIEAKTYWNE",
            (s, direction) =>
                s.CloudQuality = CycleEnum(s.CloudQuality, direction)),
        Toggle("cloud-shadows", SettingCategory.Graphics, "CIENIE CHMUR",
            s => s.CloudShadows, (s, v) => s.CloudShadows = v),
        Toggle("sun-shadows", SettingCategory.Graphics, "CIENIE SLONCA",
            s => s.SunShadows, (s, v) => s.SunShadows = v),
        new(
            "shadow-quality",
            SettingCategory.Graphics,
            "JAKOSC CIENI",
            s => s.SunShadows
                ? ShadowName(s.ShadowQuality)
                : "NIEAKTYWNE",
            (s, direction) =>
                s.ShadowQuality = CycleEnum(s.ShadowQuality, direction)),
        Toggle("fog", SettingCategory.Graphics, "MGLA",
            s => s.Fog, (s, v) => s.Fog = v),
        Toggle("terrain-pbr", SettingCategory.Graphics, "PBR PODLOZA",
            s => s.TerrainPbr, (s, v) => s.TerrainPbr = v),
        Toggle("model-pbr", SettingCategory.Graphics, "PBR OBIEKTOW",
            s => s.ModelPbr, (s, v) => s.ModelPbr = v)
    ];

    private static T CycleEnum<T>(T current, int direction)
        where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        var index = Array.IndexOf(values, current);
        if (index < 0) index = 0;

        var next = (index + Math.Sign(direction)) % values.Length;
        if (next < 0) next += values.Length;
        return values[next];
    }

    private static string FsrQualityName(FsrQualityMode quality) =>
        quality switch
        {
            FsrQualityMode.Quality => "QUALITY",
            FsrQualityMode.UltraQuality => "ULTRA QUALITY",
            FsrQualityMode.Balanced => "BALANCED",
            FsrQualityMode.Performance => "PERFORMANCE",
            FsrQualityMode.Native => "NATIVE",
            FsrQualityMode.Custom => "CUSTOM",
            _ => "QUALITY"
        };

    private static string FrameLimitName(FrameRateLimit limit) =>
        limit switch
        {
            FrameRateLimit.Unlimited => "BEZ LIMITU",
            FrameRateLimit.Fps30 => "30",
            FrameRateLimit.Fps45 => "45",
            FrameRateLimit.Fps60 => "60",
            FrameRateLimit.Fps90 => "90",
            FrameRateLimit.Fps120 => "120",
            _ => "BEZ LIMITU"
        };

    private static string MsaaName(MsaaQuality quality) =>
        quality switch
        {
            MsaaQuality.Off => "WYL",
            MsaaQuality.X2 => "2X*",
            MsaaQuality.X4 => "4X*",
            _ => "WYL"
        };

    private static string BloomName(BloomQuality quality) =>
        quality switch
        {
            BloomQuality.Off => "WYL",
            BloomQuality.Low => "NISKI",
            BloomQuality.Medium => "SREDNI",
            BloomQuality.High => "WYSOKI",
            _ => "WYL"
        };

    private static string QualityName(CloudQuality quality) =>
        quality switch
        {
            CloudQuality.Low => "NISKA",
            CloudQuality.Medium => "SREDNIA",
            CloudQuality.High => "WYSOKA",
            CloudQuality.Ultra => "ULTRA",
            _ => "WYSOKA"
        };

    private static string DistanceName(RenderDistanceQuality quality) =>
        quality switch
        {
            RenderDistanceQuality.VeryLow => "180M",
            RenderDistanceQuality.Low => "300M",
            RenderDistanceQuality.Medium => "450M",
            RenderDistanceQuality.High => "650M",
            RenderDistanceQuality.Ultra => "900M",
            _ => "650M"
        };

    private static string VegetationDistanceName(VegetationDistanceQuality quality) =>
        quality switch
        {
            VegetationDistanceQuality.Off => "WYL",
            VegetationDistanceQuality.Short => "90M",
            VegetationDistanceQuality.Medium => "180M",
            VegetationDistanceQuality.Far => "320M",
            VegetationDistanceQuality.Ultra => "520M",
            _ => "320M"
        };

    private static string GroundClutterName(GroundClutterQuality quality) =>
        quality switch
        {
            GroundClutterQuality.Off => "WYL",
            GroundClutterQuality.Low => "30M",
            GroundClutterQuality.Medium => "70M",
            GroundClutterQuality.High => "140M",
            _ => "70M"
        };

    private static string ShadowDistanceName(ShadowDistanceQuality quality) =>
        quality switch
        {
            ShadowDistanceQuality.Short => "70M",
            ShadowDistanceQuality.Medium => "120M",
            ShadowDistanceQuality.Far => "180M",
            ShadowDistanceQuality.Ultra => "210M",
            _ => "180M"
        };

    private static string FarVegetationName(FarVegetationMode mode) =>
        mode switch
        {
            FarVegetationMode.Off => "WYL",
            FarVegetationMode.Impostors => "IMPOSTORY",
            FarVegetationMode.FullMeshes => "PELNE",
            _ => "IMPOSTORY"
        };

    private static string ModelLodName(ModelLodQuality quality) =>
        quality switch
        {
            ModelLodQuality.Aggressive => "AGRESYWNY",
            ModelLodQuality.Balanced => "BALANCED",
            ModelLodQuality.Quality => "QUALITY",
            ModelLodQuality.Ultra => "ULTRA",
            _ => "QUALITY"
        };

    private static string TerrainDetailName(TerrainDetailQuality quality) =>
        quality switch
        {
            TerrainDetailQuality.Low => "NISKA",
            TerrainDetailQuality.Medium => "SREDNIA",
            TerrainDetailQuality.High => "WYSOKA",
            TerrainDetailQuality.Ultra => "ULTRA",
            _ => "WYSOKA"
        };

    private static string TextureName(TextureQuality quality) =>
        quality switch
        {
            TextureQuality.Low => "NISKA",
            TextureQuality.Medium => "SREDNIA",
            TextureQuality.High => "WYSOKA",
            TextureQuality.Ultra => "ULTRA",
            _ => "WYSOKA"
        };

    private static string ShadowName(ShadowQuality quality) =>
        quality switch
        {
            ShadowQuality.Low => "1024",
            ShadowQuality.Medium => "2048",
            ShadowQuality.High => "4096",
            _ => "2048"
        };

    private static SettingDefinition Volume(
        string id,
        string label,
        Func<GameSettings, float> getter,
        Action<GameSettings, float> setter) =>
        new(
            id,
            SettingCategory.Audio,
            label,
            s => $"{getter(s) * 100f:0}%",
            (s, direction) =>
                setter(
                    s,
                    Math.Clamp(
                        getter(s) + Math.Sign(direction) * 0.05f,
                        0f,
                        1f)));

    private static SettingDefinition Toggle(
        string id,
        SettingCategory category,
        string label,
        Func<GameSettings, bool> getter,
        Action<GameSettings, bool> setter) =>
        new(
            id,
            category,
            label,
            s => getter(s) ? "WL" : "WYL",
            (s, _) => setter(s, !getter(s)));
}
