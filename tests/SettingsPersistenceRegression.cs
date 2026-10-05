using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.UI;

public static class SettingsPersistenceRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var root = Path.Combine(
            Path.GetTempPath(),
            "SlavicGame.SettingsRegression",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var path = Path.Combine(root, "settings.json");
            var store = new GameSettingsStore(path);

            var original = new GameSettings
            {
                Fullscreen = false,
                Resolution = RenderResolution.Hd720,
                WindowResolution = RenderResolution.FullHd1080,
                Upscaler = UpscalerMode.Fsr3,
                FrameGeneration = true,
                FrameGenerationRuntime = FrameGenerationRuntimeState.Failed,
                FrameGenerationRuntimeReason = "transient test reason",
                FsrQuality = FsrQualityMode.Quality,
                Bloom = BloomQuality.Medium,
                TextureQuality = TextureQuality.Low
            };

            store.Save(original);
            var savedJson = File.ReadAllText(path);

            check(savedJson.Contains(
                    "\"Resolution\": \"Hd720\"",
                    StringComparison.Ordinal),
                "Settings persist resolution by stable enum name");
            check(savedJson.Contains(
                    "\"Upscaler\": \"Fsr3\"",
                    StringComparison.Ordinal),
                "Settings persist upscaler by stable enum name");
            check(savedJson.Contains(
                    "\"Bloom\": \"Medium\"",
                    StringComparison.Ordinal),
                "Settings persist post-processing enums by stable names");
            check(savedJson.Contains(
                    "\"FrameGeneration\": true",
                    StringComparison.Ordinal),
                "Settings persist Frame Generation by stable boolean property");
            check(!savedJson.Contains(
                    "FrameGenerationRuntime",
                    StringComparison.Ordinal) &&
                  !savedJson.Contains(
                    "transient test reason",
                    StringComparison.Ordinal),
                "Settings never persist transient Frame Generation runtime status");

            var roundTrip = store.Load();
            check(roundTrip.Resolution == RenderResolution.Hd720 &&
                  roundTrip.WindowResolution == RenderResolution.FullHd1080 &&
                  roundTrip.Upscaler == UpscalerMode.Fsr3 &&
                  roundTrip.FrameGeneration &&
                  roundTrip.FsrQuality == FsrQualityMode.Quality &&
                  roundTrip.Bloom == BloomQuality.Medium,
                "Named settings survive save/load round trip");

            File.WriteAllText(
                path,
                """
                {
                  "Fullscreen": false,
                  "Resolution": 0,
                  "VSync": false,
                  "ShowFps": true
                }
                """);

            var migrated = store.Load();
            check(migrated.Resolution == RenderResolution.Hd720 &&
                  migrated.WindowResolution == RenderResolution.Hd720 &&
                  !migrated.FrameGeneration,
                "Legacy settings migrate with Frame Generation safely defaulted OFF");

            var migratedJson = File.ReadAllText(path);
            check(migratedJson.Contains(
                    "\"Resolution\": \"Hd720\"",
                    StringComparison.Ordinal),
                "Legacy numeric settings are rewritten using stable enum names");

            File.WriteAllText(
                path,
                """
                {
                  "Resolution": 0,
                  "WindowResolution": 3,
                  "Upscaler": 1,
                  "FsrQuality": 3
                }
                """);

            var modernNumeric = store.Load();
            check(modernNumeric.Resolution == RenderResolution.Qhd540,
                "Modern numeric settings with upscaler metadata keep current resolution mapping");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); }
            catch { }
        }
    }
}
