using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.UI;

public static class FrontendSettingsRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var definitions = SettingsCatalog.All;
        check(definitions.Count >= 19,
            "Frontend exposes the current display, controls and graphics settings");
        check(definitions.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() == definitions.Count,
            "Frontend setting identifiers are unique");
        check(Enum.GetValues<SettingCategory>().All(category =>
                definitions.Any(item => item.Category == category)),
            "Frontend has at least one setting in every category");

        var settings = new GameSettings();
        var resolution = definitions.Single(item => item.Id == "resolution");
        var originalResolution = settings.Resolution;
        resolution.Change(settings, 1);
        check(settings.Resolution != originalResolution &&
              settings.ResolutionSize.Width > 0 &&
              settings.ResolutionSize.Height > 0,
            "Frontend resolution setting cycles valid render sizes");

        check(GraphicsQualityCatalog.CloudRaymarchSteps(CloudQuality.Low) <
              GraphicsQualityCatalog.CloudRaymarchSteps(CloudQuality.Ultra),
            "Cloud quality maps to increasing raymarch work");
        check(GraphicsQualityCatalog.ShadowMapSize(ShadowQuality.Low) <
              GraphicsQualityCatalog.ShadowMapSize(ShadowQuality.High),
            "Shadow quality maps to increasing shadow-map resolution");

        var clouds = definitions.Single(item => item.Id == "volumetric-clouds");
        clouds.Change(settings, 1);
        check(!settings.VolumetricClouds,
            "Frontend catalog changes volumetric cloud runtime setting");

        var fov = definitions.Single(item => item.Id == "fov");
        for (var i = 0; i < 20; i++)
            fov.Change(settings, 1);
        check(settings.FieldOfViewDegrees <= 100f,
            "Frontend FOV setting respects production clamp");

        var mouse = definitions.Single(item => item.Id == "mouse");
        for (var i = 0; i < 100; i++)
            mouse.Change(settings, -1);
        check(settings.MouseSensitivity >= 0.25f,
            "Frontend mouse sensitivity respects production clamp");
    }
}
