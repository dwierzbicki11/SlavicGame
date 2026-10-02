namespace SlavicGame.Engine.Core;

public sealed class EngineConfig
{
    public string WindowTitle { get; init; } = "SlavicGame";
    public int Width { get; init; } = 1280;
    public int Height { get; init; } = 720;
    public bool VSync { get; init; }
    public bool InputDiagnostics { get; init; }
    public bool UseMouseWarp { get; init; }
    public bool Fullscreen { get; init; } = true;
}
