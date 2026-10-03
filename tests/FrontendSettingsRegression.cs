using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.UI;

public static class FrontendSettingsRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var definitions = SettingsCatalog.All;
        check(definitions.Count >= 40,
            "Frontend exposes the current display, controls, graphics and post-processing settings");
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

        var windowResolution = definitions.Single(item => item.Id == "window-resolution");
        var originalWindowResolution = settings.WindowResolution;
        windowResolution.Change(settings, 1);
        check(settings.WindowResolution != originalWindowResolution &&
              settings.WindowResolutionSize.Width > 0 &&
              settings.WindowResolutionSize.Height > 0,
            "Frontend window resolution is independent from internal render resolution");

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
        check(GraphicsQualityCatalog.VegetationImpostorStart(ModelLodQuality.Aggressive) <
              GraphicsQualityCatalog.VegetationImpostorStart(ModelLodQuality.Ultra),
            "Aggressive LOD switches trees to impostors earlier");

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
              settings.FarVegetation == FarVegetationMode.Impostors &&
              settings.Resolution == RenderResolution.Qhd540 &&
              settings.Upscaler == UpscalerMode.Fsr1 &&
              settings.AntiAliasing == AntiAliasingMode.Fxaa &&
              settings.Msaa == MsaaQuality.Off &&
              settings.Bloom == BloomQuality.Off &&
              !settings.NormalMapping &&
              !settings.SpecularHighlights,
            "Low-end preset disables the heaviest GPU effects and aggressively reduces shader work");

        var lod = definitions.Single(item => item.Id == "model-lod");
        settings.ModelLod = ModelLodQuality.Quality;
        lod.Change(settings, -1);
        check(settings.ModelLod == ModelLodQuality.Balanced,
            "Frontend model LOD setting changes distance thresholds");

        var farVegetation = definitions.Single(item => item.Id == "far-vegetation");
        settings.FarVegetation = FarVegetationMode.FullMeshes;
        farVegetation.Change(settings, -1);
        check(settings.FarVegetation == FarVegetationMode.Impostors,
            "Frontend can switch far trees from full meshes to impostors");

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

        var antiAliasing = definitions.Single(item => item.Id == "anti-aliasing");
        settings.AntiAliasing = AntiAliasingMode.Off;
        antiAliasing.Change(settings, 1);
        check(settings.AntiAliasing == AntiAliasingMode.Fxaa,
            "Frontend can enable FXAA");

        var msaa = definitions.Single(item => item.Id == "msaa");
        settings.Msaa = MsaaQuality.Off;
        msaa.Change(settings, 1);
        check(settings.Msaa == MsaaQuality.X2 &&
              GraphicsQualityCatalog.MsaaSamples(settings.Msaa) == 2,
            "Frontend can select 2x MSAA");

        var bloom = definitions.Single(item => item.Id == "bloom");
        settings.Bloom = BloomQuality.Off;
        bloom.Change(settings, 1);
        check(settings.Bloom == BloomQuality.Low &&
              GraphicsQualityCatalog.BloomTapCount(settings.Bloom) > 0,
            "Frontend can enable bloom quality");

        var brightness = definitions.Single(item => item.Id == "brightness");
        settings.Brightness = 1f;
        brightness.Change(settings, 1);
        check(settings.Brightness > 1f,
            "Frontend brightness adjustment changes post-process exposure");

        var gamma = definitions.Single(item => item.Id == "gamma");
        settings.Gamma = 2.2f;
        gamma.Change(settings, -1);
        check(settings.Gamma < 2.2f,
            "Frontend gamma adjustment changes display gamma");

        var fpsLimit = definitions.Single(item => item.Id == "fps-limit");
        settings.FpsLimit = FrameRateLimit.Unlimited;
        fpsLimit.Change(settings, 1);
        check(settings.FpsLimit == FrameRateLimit.Fps30 &&
              GraphicsQualityCatalog.FrameRate(settings.FpsLimit) == 30,
            "Frontend can select an FPS limit");

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
