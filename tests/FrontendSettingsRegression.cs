using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.UI;

public static class FrontendSettingsRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var definitions = SettingsCatalog.All;
        check(definitions.Count >= 41,
            "Frontend exposes the current display, controls, graphics and post-processing settings");
        check(definitions.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() == definitions.Count,
            "Frontend setting identifiers are unique");
        check(Enum.GetValues<SettingCategory>().All(category =>
                definitions.Any(item => item.Category == category)),
            "Frontend has at least one setting in every category");

        foreach (var definition in definitions)
        {
            var sample = new GameSettings();
            sample.Normalize();

            var before = definition.ValueText(sample);
            check(!string.IsNullOrWhiteSpace(before),
                $"Setting '{definition.Id}' always exposes a readable value");

            try
            {
                definition.Change(sample, 1);
                sample.Normalize();
                var after = definition.ValueText(sample);
                check(!string.IsNullOrWhiteSpace(after),
                    $"Setting '{definition.Id}' remains readable after a change");
            }
            catch (Exception exception)
            {
                check(false,
                    $"Setting '{definition.Id}' can be changed without exception: {exception.Message}");
            }
        }

        var settings = new GameSettings();
        var resolution = definitions.Single(item => item.Id == "resolution");
        settings.Upscaler = UpscalerMode.Fsr1;
        settings.FsrQuality = FsrQualityMode.Performance;
        var originalResolution = settings.Resolution;
        resolution.Change(settings, 1);
        check(settings.Resolution != originalResolution &&
              settings.ResolutionSize.Width > 0 &&
              settings.ResolutionSize.Height > 0,
            "Frontend resolution setting cycles valid render sizes");
        check(settings.FsrQuality == FsrQualityMode.Custom,
            "Manual render resolution overrides automatic FSR quality sizing");
        check(resolution.ValueText(settings) == settings.ResolutionSize.ToString(),
            "Manual render resolution displays the effective custom size");

        settings.FsrQuality = FsrQualityMode.Quality;
        check(resolution.ValueText(settings).StartsWith("AUTO ", StringComparison.Ordinal),
            "Automatic FSR mode does not pretend the manual render resolution is active");

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
        check(GraphicsQualityCatalog.RainStreakCount(CloudQuality.Low) <
              GraphicsQualityCatalog.RainStreakCount(CloudQuality.Ultra),
            "Cloud quality also scales the visible precipitation budget");
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
        check(textureQuality.RequiresRestart,
            "Texture quality warns that full GPU texture reload requires restart");
        settings.TextureQuality = TextureQuality.High;
        textureQuality.Change(settings, -1);
        check(settings.TextureQuality == TextureQuality.Medium,
            "Frontend texture quality setting changes runtime texture policy");
        check(GraphicsQualityCatalog.TextureMaximumDimension(TextureQuality.Low) <
              GraphicsQualityCatalog.TextureMaximumDimension(TextureQuality.High),
            "Lower texture quality reduces maximum uploaded texture dimension");

        var preset = definitions.Single(item => item.Id == "graphics-preset");
        check(preset.RequiresRestart,
            "Graphics preset reports restart because it can change MSAA and texture upload quality");
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
              settings.FsrQuality == FsrQualityMode.Performance &&
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
        check(upscaler.RequiresRestart,
            "Upscaler warns that switching temporal FSR requires renderer restart");
        settings.Upscaler = UpscalerMode.Bilinear;
        upscaler.Change(settings, 1);
        check(settings.Upscaler == UpscalerMode.Fsr1 &&
              upscaler.ValueText(settings) == "FSR1",
            "Frontend can enable AMD FSR1");
        upscaler.Change(settings, 1);
        check(settings.Upscaler == UpscalerMode.Fsr3 &&
              upscaler.ValueText(settings) == "FSR3",
            "Frontend exposes AMD FSR3 temporal upscaling");

        var frameGeneration =
            definitions.Single(item => item.Id == "frame-generation");
        check(frameGeneration.RequiresRestart,
            "Frame Generation warns that Vulkan presentation resources require restart");
        settings.FrameGeneration = false;
        frameGeneration.Change(settings, 1);
        check(settings.FrameGeneration &&
              frameGeneration.ValueText(settings) == "ON",
            "Frontend can enable FSR3 Frame Generation");
        frameGeneration.Change(settings, -1);
        check(!settings.FrameGeneration &&
              frameGeneration.ValueText(settings) == "OFF",
            "Frontend can disable FSR3 Frame Generation");

        var fsrQuality = definitions.Single(item => item.Id == "fsr-quality");
        settings.Upscaler = UpscalerMode.Bilinear;
        check(fsrQuality.ValueText(settings) == "NIEAKTYWNE",
            "FSR quality reports inactive while bilinear scaling is selected");
        settings.Upscaler = UpscalerMode.Fsr3;
        settings.FsrQuality = FsrQualityMode.Quality;
        fsrQuality.Change(settings, 1);
        check(settings.FsrQuality == FsrQualityMode.UltraQuality &&
              fsrQuality.ValueText(settings) == "ULTRA QUALITY",
            "Frontend can change FSR3 quality mode");

        var custom = new ResolutionSize(1152, 648);
        var fsrPerformance = GraphicsQualityCatalog.FsrRenderResolution(
            1920,
            1080,
            FsrQualityMode.Performance,
            custom);
        var fsrQuality1080p = GraphicsQualityCatalog.FsrRenderResolution(
            1920,
            1080,
            FsrQualityMode.Quality,
            custom);
        var fsrCustom = GraphicsQualityCatalog.FsrRenderResolution(
            1920,
            1080,
            FsrQualityMode.Custom,
            custom);

        check(fsrPerformance.Width == 960 && fsrPerformance.Height == 540,
            "FSR1 Performance maps 1080p output to 960x540 internal rendering");
        check(fsrQuality1080p.Width == 1280 && fsrQuality1080p.Height == 720,
            "FSR1 Quality maps 1080p output to 1280x720 internal rendering");
        check(fsrCustom == custom,
            "FSR1 Custom preserves manually selected internal resolution");

        foreach (var presetValue in new[]
                 {
                     GraphicsPreset.LowEnd,
                     GraphicsPreset.Balanced,
                     GraphicsPreset.High
                 })
        {
            var presetSettings = new GameSettings();
            GraphicsPresetCatalog.Apply(presetSettings, presetValue);
            var internalSize = GraphicsQualityCatalog.FsrRenderResolution(
                1920,
                1080,
                presetSettings.FsrQuality,
                presetSettings.ResolutionSize);

            check(FsrPresentationPolicy.UsesUpscalePass(
                    presetSettings.Upscaler,
                    (uint)internalSize.Width,
                    (uint)internalSize.Height,
                    1920,
                    1080) &&
                  !FsrPresentationPolicy.RequiresFinalRcasYFlip(
                    presetSettings.Upscaler,
                    (uint)internalSize.Width,
                    (uint)internalSize.Height,
                    1920,
                    1080),
                $"{presetValue} uses EASU/RCAS without a manual Vulkan Y inversion");
        }

        check(!FsrPresentationPolicy.UsesSinglePassEasuCompatibility(true),
            "Vulkan FSR1 retains the full EASU plus RCAS path");
        check(!FsrPresentationPolicy.UsesSinglePassEasuCompatibility(false),
            "Non-Vulkan backends retain the full EASU plus RCAS path");

        var ultraSettings = new GameSettings();
        GraphicsPresetCatalog.Apply(ultraSettings, GraphicsPreset.Ultra);
        var ultraInternal = GraphicsQualityCatalog.FsrRenderResolution(
            1920,
            1080,
            ultraSettings.FsrQuality,
            ultraSettings.ResolutionSize);
        check(ultraSettings.FsrQuality == FsrQualityMode.Native &&
              ultraInternal.Width == 1920 &&
              ultraInternal.Height == 1080 &&
              !FsrPresentationPolicy.UsesUpscalePass(
                  ultraSettings.Upscaler,
                  (uint)ultraInternal.Width,
                  (uint)ultraInternal.Height,
                  1920,
                  1080),
            "Ultra stays on the already-correct native presentation path");

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
        check(msaa.RequiresRestart,
            "MSAA warns that framebuffer recreation requires restart");
        settings.Msaa = MsaaQuality.Off;
        msaa.Change(settings, 1);
        check(settings.Msaa == MsaaQuality.X2 &&
              GraphicsQualityCatalog.MsaaSamples(settings.Msaa) == 2,
            "Frontend can select 2x MSAA");

        var bloomStrength = definitions.Single(item => item.Id == "bloom-strength");
        settings.Bloom = BloomQuality.Off;
        check(bloomStrength.ValueText(settings) == "NIEAKTYWNE",
            "Bloom strength reports inactive while bloom is disabled");

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
