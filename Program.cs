using SlavicGame.Engine.Core;
using SlavicGame.Engine.Diagnostics;

try
{
    using var game = new GameEngine(new EngineConfig
    {
        WindowTitle = "SlavicGame",
        Width = 1280,
        Height = 720,
        VSync = true,
        Fullscreen = true
    });

    game.Initialize();
    game.Run();
}
catch (Exception exception)
{
    EngineLog.Error(exception.ToString());
    Environment.ExitCode = 1;
}
