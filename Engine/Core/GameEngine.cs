using System.Diagnostics;
using SlavicGame.Engine.Diagnostics;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Core;

public sealed class GameEngine : IDisposable
{
    private readonly GameTime _time = new();
    private readonly GameWindow _window;
    private readonly VeldridRenderer _renderer = new();
    private readonly WorldState _world = WorldGenerator.Generate();

    private bool _initialized;

    public GameTime Time => _time;
    public GameWindow Window => _window;
    public VeldridRenderer Renderer => _renderer;
    public WorldState World => _world;

    public GameEngine(EngineConfig config)
    {
        _window = new GameWindow(config);
        _window.Resized += OnWindowResized;
        _window.Closing += () => EngineLog.Info("Closing SlavicGame.");
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        EngineLog.Info("Starting SlavicGame engine.");
        _renderer.Initialize(_window, true);
        _initialized = true;
        EngineLog.Info("Engine initialization complete.");
    }

    public void Run()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Call Initialize() before Run().");
        }

        var stopwatch = Stopwatch.StartNew();
        var previousSeconds = stopwatch.Elapsed.TotalSeconds;
        var loggedFirstFrame = false;

        while (_window.Exists)
        {
            _window.PumpEvents();

            var currentSeconds = stopwatch.Elapsed.TotalSeconds;
            var deltaSeconds = currentSeconds - previousSeconds;
            previousSeconds = currentSeconds;

            _time.Advance(deltaSeconds);
            _world.Update(deltaSeconds);

            if (!loggedFirstFrame)
            {
                loggedFirstFrame = true;
                EngineLog.Info("Main loop is running.");
            }

            _renderer.Render();
        }
    }

    private void OnWindowResized()
    {
        if (!_initialized)
        {
            return;
        }

        _renderer.Resize((uint)Math.Max(1, _window.Width), (uint)Math.Max(1, _window.Height));
    }

    public void Dispose()
    {
        _renderer.Dispose();
        _window.Dispose();
    }
}
