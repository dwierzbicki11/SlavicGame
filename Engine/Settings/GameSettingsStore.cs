using System.Text.Json;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Settings;

public sealed class GameSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

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
            var settings = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions)
                ?? new GameSettings();
            settings.Normalize();
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
