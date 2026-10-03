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
        Toggle("cloud-shadows", SettingCategory.Graphics, "CIENIE CHMUR",
            s => s.CloudShadows, (s, v) => s.CloudShadows = v),
        Toggle("sun-shadows", SettingCategory.Graphics, "CIENIE SLONCA",
            s => s.SunShadows, (s, v) => s.SunShadows = v),
        Toggle("fog", SettingCategory.Graphics, "MGLA",
            s => s.Fog, (s, v) => s.Fog = v),
        Toggle("terrain-pbr", SettingCategory.Graphics, "PBR PODLOZA",
            s => s.TerrainPbr, (s, v) => s.TerrainPbr = v),
        Toggle("model-pbr", SettingCategory.Graphics, "PBR OBIEKTOW",
            s => s.ModelPbr, (s, v) => s.ModelPbr = v)
    ];

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
