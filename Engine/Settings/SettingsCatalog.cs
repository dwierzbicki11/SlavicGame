namespace SlavicGame.Engine.Settings;

public enum SettingCategory
{
    Display,
    Controls,
    Graphics
}

public sealed record SettingDefinition(
    string Id,
    SettingCategory Category,
    string Label,
    Func<GameSettings, string> ValueText,
    Action<GameSettings, int> Change);

public static class SettingsCatalog
{
    public static IReadOnlyList<SettingDefinition> All { get; } =
    [
        Toggle("fullscreen", SettingCategory.Display, "PELNY EKRAN",
            s => s.Fullscreen, (s, v) => s.Fullscreen = v),
        new(
            "resolution",
            SettingCategory.Display,
            "ROZDZIELCZOSC",
            s => s.ResolutionSize.ToString(),
            (s, direction) =>
                s.Resolution = ResolutionCatalog.Cycle(s.Resolution, direction)),
        Toggle("vsync", SettingCategory.Display, "VSYNC",
            s => s.VSync, (s, v) => s.VSync = v),
        Toggle("fps", SettingCategory.Display, "LICZNIK FPS",
            s => s.ShowFps, (s, v) => s.ShowFps = v),

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
            }),
        new(
            "texture-quality",
            SettingCategory.Graphics,
            "JAKOSC TEKSTUR",
            s => TextureName(s.TextureQuality),
            (s, direction) =>
                s.TextureQuality = CycleEnum(s.TextureQuality, direction)),
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
            s => ShadowDistanceName(s.ShadowDistance),
            (s, direction) =>
                s.ShadowDistance = CycleEnum(s.ShadowDistance, direction)),
        new(
            "terrain-detail",
            SettingCategory.Graphics,
            "JAKOSC TERENU",
            s => TerrainDetailName(s.TerrainDetail),
            (s, direction) =>
                s.TerrainDetail = CycleEnum(s.TerrainDetail, direction)),
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
            s => QualityName(s.CloudQuality),
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
            s => ShadowName(s.ShadowQuality),
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
