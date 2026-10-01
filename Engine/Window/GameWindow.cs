using Silk.NET.Maths;
using Silk.NET.Windowing;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.Diagnostics;

namespace SlavicGame.Engine.Windowing;

public sealed class GameWindow : IDisposable
{
    private readonly IWindow _window;

    public IWindow NativeWindow => _window;
    public Vector2D<int> FramebufferSize => _window.FramebufferSize;
    public bool IsClosing => _window.IsClosing;

    public event Action? Load;
    public event Action<double>? Update;
    public event Action<double>? Render;
    public event Action? Closing;

    public GameWindow(EngineConfig config)
    {
        var options = WindowOptions.DefaultVulkan with
        {
            Size = new Vector2D<int>(config.Width, config.Height),
            Title = config.WindowTitle,
            VSync = config.VSync
        };

        _window = Window.Create(options);

        _window.Load += () => Load?.Invoke();
        _window.Update += delta => Update?.Invoke(delta);
        _window.Render += delta => Render?.Invoke(delta);
        _window.Closing += () => Closing?.Invoke();
    }

    public void Initialize()
    {
        EngineLog.Info($"Creating window {FramebufferSize.X}x{FramebufferSize.Y}.");
        _window.Initialize();
    }

    public void Run() => _window.Run();

    public void Dispose() => _window.Dispose();
}
