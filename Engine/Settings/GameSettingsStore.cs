using System.Text.Json;
using System.Text.Json.Serialization;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Settings;

public sealed class GameSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public string Path { get; }

    public GameSettingsStore(string? path = null)
    {
        Path = path ?? GetDefaultPath();
    }

    public GameSettings Load()
    {
        try
        {
            if (!File.Exists(Path))
                return new GameSettings();

            var json = File.ReadAllText(Path);
            var legacyResolution = TryReadLegacyResolution(json);

            var settings = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions)
                ?? new GameSettings();

            if (legacyResolution is not null)
            {
                settings.Resolution = legacyResolution.Value;
                settings.WindowResolution = legacyResolution.Value;
                EngineLog.Info(
                    $"Migrated legacy numeric render resolution to {settings.ResolutionSize}.");
            }

            settings.Normalize();

            // Rewrite legacy numeric enum files immediately. Future enum additions
            // are then safe because values are persisted by stable names.
            if (ContainsNumericEnumStorage(json))
                Save(settings);

            return settings;
        }
        catch (Exception exception)
        {
            EngineLog.Warn($"Could not load settings '{Path}': {exception.Message}");
            return new GameSettings();
        }
    }

    public void Save(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Normalize();

        try
        {
            var directory = System.IO.Path.GetDirectoryName(Path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(
                Path,
                JsonSerializer.Serialize(settings, JsonOptions));
        }
        catch (Exception exception)
        {
            EngineLog.Warn($"Could not save settings '{Path}': {exception.Message}");
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(
            namingPolicy: null,
            allowIntegerValues: true));
        return options;
    }

    private static bool ContainsNumericEnumStorage(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        foreach (var propertyName in EnumPropertyNames)
        {
            if (root.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.Number)
            {
                return true;
            }
        }

        return false;
    }

    private static RenderResolution? TryReadLegacyResolution(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // The first persisted settings schema used:
        // 0=720p, 1=768p, 2=900p, 3=1080p, 4=1440p, 5=2160p.
        // Later 540p/576p/648p were inserted before those values. Numeric JSON
        // therefore changed meaning. Newer files already contain the display /
        // upscaler fields and must keep the current numeric mapping.
        if (root.TryGetProperty("WindowResolution", out _) ||
            root.TryGetProperty("Upscaler", out _) ||
            !root.TryGetProperty("Resolution", out var resolution) ||
            resolution.ValueKind != JsonValueKind.Number ||
            !resolution.TryGetInt32(out var legacyValue))
        {
            return null;
        }

        return legacyValue switch
        {
            0 => RenderResolution.Hd720,
            1 => RenderResolution.Hd768,
            2 => RenderResolution.Hd900,
            3 => RenderResolution.FullHd1080,
            4 => RenderResolution.Qhd1440,
            5 => RenderResolution.Uhd2160,
            _ => null
        };
    }

    private static readonly string[] EnumPropertyNames =
    [
        "Camera",
        "Resolution",
        "WindowResolution",
        "CloudQuality",
        "ShadowQuality",
        "TextureQuality",
        "RenderDistance",
        "VegetationDistance",
        "GroundClutter",
        "ShadowDistance",
        "TerrainDetail",
        "ModelLod",
        "FarVegetation",
        "Upscaler",
        "FsrQuality",
        "AntiAliasing",
        "Msaa",
        "Bloom",
        "FpsLimit"
    ];

    private static string GetDefaultPath()
    {
        var root = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(root))
            root = AppContext.BaseDirectory;

        return System.IO.Path.Combine(
            root,
            "SlavicGame",
            "settings.json");
    }
}
