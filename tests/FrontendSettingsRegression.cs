using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.UI;

public static class FrontendSettingsRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var definitions = SettingsCatalog.All;
        check(definitions.Count >= 31,
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
        check(GraphicsQualityCatalog.RenderDistance(RenderDistanceQuality.VeryLow) <
              GraphicsQualityCatalog.RenderDistance(RenderDistanceQuality.Ultra),
            "World render distance maps to increasing visibility range");
        check(GraphicsQualityCatalog.VegetationDistance(VegetationDistanceQuality.Short) <
              GraphicsQualityCatalog.VegetationDistance(VegetationDistanceQuality.Ultra),
            "Vegetation distance maps to increasing visibility range");
        check(GraphicsQualityCatalog.GroundClutterDistance(GroundClutterQuality.Off) == 0f &&
              GraphicsQualityCatalog.GroundClutterDistance(GroundClutterQuality.High) > 0f,
            "Ground clutter can be completely disabled");
        check(GraphicsQualityCatalog.ShadowDistance(ShadowDistanceQuality.Short) <
              GraphicsQualityCatalog.ShadowDistance(ShadowDistanceQuality.Ultra),
            "Shadow distance maps to increasing shadow coverage");
        check(GraphicsQualityCatalog.TerrainDetailLevel(TerrainDetailQuality.Low) <
              GraphicsQualityCatalog.TerrainDetailLevel(TerrainDetailQuality.Ultra),
            "Terrain detail maps to increasing shader work");
        check(GraphicsQualityCatalog.TerrainMeshStep(TerrainDetailQuality.Low) >
              GraphicsQualityCatalog.TerrainMeshStep(TerrainDetailQuality.High),
            "Low terrain detail uses a coarser geometry grid");
        var aggressiveLod = GraphicsQualityCatalog.ModelLodDistances(ModelLodQuality.Aggressive);
        var ultraLod = GraphicsQualityCatalog.ModelLodDistances(ModelLodQuality.Ultra);
        check(aggressiveLod.Lod1 < ultraLod.Lod1 &&
              aggressiveLod.Lod2 < ultraLod.Lod2,
            "Aggressive model LOD switches earlier than Ultra");

        var textureQuality = definitions.Single(item => item.Id == "texture-quality");
        settings.TextureQuality = TextureQuality.High;
        textureQuality.Change(settings, -1);
        check(settings.TextureQuality == TextureQuality.Medium,
            "Frontend texture quality setting changes runtime texture policy");
        check(GraphicsQualityCatalog.TextureMaximumDimension(TextureQuality.Low) <
              GraphicsQualityCatalog.TextureMaximumDimension(TextureQuality.High),
            "Lower texture quality reduces maximum uploaded texture dimension");

        var preset = definitions.Single(item => item.Id == "graphics-preset");
        GraphicsPresetCatalog.Apply(settings, GraphicsPreset.High);
        preset.Change(settings, -1);
        check(GraphicsPresetCatalog.DetectName(settings) == "BALANCED",
            "Frontend graphics preset can step down from High to Balanced");
        GraphicsPresetCatalog.Apply(settings, GraphicsPreset.LowEnd);
        check(!settings.VolumetricClouds &&
              !settings.CloudShadows &&
              !settings.SunShadows &&
              settings.TextureQuality == TextureQuality.Low &&
              settings.RenderDistance == RenderDistanceQuality.VeryLow &&
              settings.VegetationDistance == VegetationDistanceQuality.Short &&
              settings.GroundClutter == GroundClutterQuality.Off &&
              settings.ShadowDistance == ShadowDistanceQuality.Short &&
              settings.TerrainDetail == TerrainDetailQuality.Low &&
              settings.ModelLod == ModelLodQuality.Aggressive &&
              settings.Upscaler == UpscalerMode.Fsr1 &&
              !settings.NormalMapping &&
              !settings.SpecularHighlights,
            "Low-end preset disables the heaviest GPU effects and aggressively reduces shader work");

        var lod = definitions.Single(item => item.Id == "model-lod");
        settings.ModelLod = ModelLodQuality.Quality;
        lod.Change(settings, -1);
        check(settings.ModelLod == ModelLodQuality.Balanced,
            "Frontend model LOD setting changes distance thresholds");

        var upscaler = definitions.Single(item => item.Id == "upscaler");
        settings.Upscaler = UpscalerMode.Bilinear;
        upscaler.Change(settings, 1);
        check(settings.Upscaler == UpscalerMode.Fsr1,
            "Frontend can enable AMD FSR1");

        var sharpness = definitions.Single(item => item.Id == "fsr-sharpness");
        settings.FsrSharpness = 0.5f;
        sharpness.Change(settings, 1);
        check(settings.FsrSharpness > 0.5f && settings.FsrSharpness <= 1f,
            "Frontend FSR1 sharpness setting stays normalized");

        var lowResolution = ResolutionCatalog.Get(RenderResolution.Qhd540);
        check(lowResolution.Width == 960 && lowResolution.Height == 540,
            "Low internal resolution is available for FSR performance mode");

        var terrainDetail = definitions.Single(item => item.Id == "terrain-detail");
        settings.TerrainDetail = TerrainDetailQuality.High;
        terrainDetail.Change(settings, -1);
        check(settings.TerrainDetail == TerrainDetailQuality.Medium,
            "Frontend terrain detail setting changes terrain shader workload");

        var normalMapping = definitions.Single(item => item.Id == "normal-mapping");
        settings.NormalMapping = true;
        normalMapping.Change(settings, 1);
        check(!settings.NormalMapping,
            "Frontend can disable normal mapping");

        var specular = definitions.Single(item => item.Id == "specular");
        settings.SpecularHighlights = true;
        specular.Change(settings, 1);
        check(!settings.SpecularHighlights,
            "Frontend can disable specular highlights");

        settings.VolumetricClouds = true;
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
