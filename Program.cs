using SlavicGame.Engine.Core;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Settings;

try
{
    var settingsStore = new GameSettingsStore();
    var settings = settingsStore.Load();

    if (args.Contains("--windowed", StringComparer.Ordinal))
        settings.Fullscreen = false;
    if (args.Contains("--fullscreen", StringComparer.Ordinal))
        settings.Fullscreen = true;
    if (args.Contains("--vsync", StringComparer.Ordinal))
        settings.VSync = true;

    using var game = new GameEngine(
        new EngineConfig
        {
            WindowTitle = "SlavicGame",
            Width = 1280,
            Height = 720,
            VSync = settings.VSync,
            Fullscreen = settings.Fullscreen,
            UseMouseWarp = args.Contains("--mouse-warp", StringComparer.Ordinal),
            InputDiagnostics = args.Contains("--input-debug", StringComparer.Ordinal)
        },
        settings,
        settingsStore);

    game.Initialize();
    game.Run();
}
catch (Exception exception)
{
    EngineLog.Error(exception.ToString());
    Environment.ExitCode = 1;
}
