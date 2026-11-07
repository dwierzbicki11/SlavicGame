using System.Diagnostics;
using System.Numerics;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.World;
using Veldrid;

/// <summary>
/// Repeatable offscreen benchmark of the shipping renderer. It renders the
/// same moving camera path with each configuration and reports the CPU time
/// spent recording a frame and the total time until the GPU is idle. Run:
/// SLAVICGAME_TEST_FSR3_NATIVE=1 dotnet run -c Release -- --benchmark [frames]
/// </summary>
static class RendererBenchmark
{
    internal static void Run(string[] args)
    {
        var frames = 90;
        var index = Array.IndexOf(args, "--benchmark");
        if (index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out var parsed))
            frames = Math.Max(10, parsed);
        var filter = args.SkipWhile(a => a != "--only").Skip(1).FirstOrDefault();

        using var device = VulkanDeviceFactory.Create(new GraphicsDeviceOptions
        {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true
        });
        Console.WriteLine($"Device: {device.DeviceName} ({device.BackendType})");

        var scenarios = new (string Name, Action<GameSettings> Configure, bool Fg)[]
        {
            ("user-balanced-fsr3perf", s => UserSettings(s), false),
            ("user-balanced-fsr3perf+fg", s => UserSettings(s), true),
            ("user-balanced-fsr1perf", s => { UserSettings(s); s.Upscaler = UpscalerMode.Fsr1; }, false),
            ("lowend-fsr1perf", s =>
            {
                GraphicsPresetCatalog.Apply(s, GraphicsPreset.LowEnd);
                s.FsrQuality = FsrQualityMode.Performance;
            }, false),
        };

        foreach (var scenario in scenarios)
        {
            if (filter is not null && !scenario.Name.Contains(filter, StringComparison.Ordinal))
                continue;
            var settings = new GameSettings();
            scenario.Configure(settings);
            RunScenario(device, scenario.Name, settings, scenario.Fg, frames);
        }
    }

    private static void UserSettings(GameSettings s)
    {
        GraphicsPresetCatalog.Apply(s, GraphicsPreset.Balanced);
        s.Upscaler = UpscalerMode.Fsr3;
        s.FsrQuality = FsrQualityMode.Performance;
        s.FsrSharpness = 1f;
        s.Bloom = BloomQuality.Low;
        s.NormalMapping = true;
        s.SpecularHighlights = false;
        s.AntiAliasing = AntiAliasingMode.Fxaa;
        s.Msaa = MsaaQuality.Off;
        s.ShowFps = true;
    }

    private static void RunScenario(GraphicsDevice device, string name, GameSettings settings, bool fg, int frames)
    {
        const uint displayWidth = 1280, displayHeight = 720;
        var world = new WorldState();
        world.Initialize();
        var camera = new Camera3D();
        var factor = settings.Upscaler is UpscalerMode.Fsr3 or UpscalerMode.Fsr1
            ? settings.FsrQuality switch
            {
                FsrQualityMode.Performance => 2.0f,
                FsrQualityMode.Balanced => 1.7f,
                FsrQualityMode.Quality => 1.5f,
                _ => 1.0f
            }
            : 1.0f;
        var renderWidth = (int)MathF.Round(displayWidth / factor);
        var renderHeight = (int)MathF.Round(displayHeight / factor);

        using var color = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            displayWidth, displayHeight, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm,
            TextureUsage.RenderTarget | TextureUsage.Sampled));
        using var framebuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, color));
        var renderer = new VeldridRenderer();
        try
        {
            renderer.InitializeOffscreen(device, framebuffer, world, settings.TextureQuality,
                settings.Msaa, settings.Upscaler, frameGenerationEnabled: fg);
            renderer.SetRenderResolution(renderWidth, renderHeight);
            renderer.SetShadowResolution(GraphicsQualityCatalog.ShadowMapSize(settings.ShadowQuality));
            renderer.SetShadowDistance(GraphicsQualityCatalog.ShadowDistance(settings.ShadowDistance));

            var spawn = world.PlayerPosition;
            void Pose(int frame)
            {
                // Walk forward while sweeping the view: mixes sky-heavy,
                // horizon and ground-heavy framing like normal play.
                var t = frame / 60f;
                var position = spawn + new Vector3(MathF.Sin(t * 0.4f) * 6f, 0, -t * 3f);
                position.Y = world.Terrain.SampleHeight(position) + 1.7f;
                var yaw = t * 0.9f;
                var pitch = MathF.Sin(t * 0.7f) * 0.25f - 0.05f;
                var forward = new Vector3(MathF.Sin(yaw) * MathF.Cos(pitch), MathF.Sin(pitch),
                    -MathF.Cos(yaw) * MathF.Cos(pitch));
                camera.SetCinematicPose(position, position + forward * 10f);
            }

            const int warmup = 20;
            var cpu = new List<double>(frames);
            var total = new List<double>(frames);
            for (var frame = 0; frame < warmup + frames; frame++)
            {
                Pose(frame);
                var start = Stopwatch.GetTimestamp();
                renderer.Render(world, camera, 30, frame / 60.0, 1.0 / 60, settings, null);
                var recorded = Stopwatch.GetTimestamp();
                device.WaitForIdle();
                var finished = Stopwatch.GetTimestamp();
                if (frame < warmup) continue;
                cpu.Add((recorded - start) * 1000.0 / Stopwatch.Frequency);
                total.Add((finished - start) * 1000.0 / Stopwatch.Frequency);
            }

            // Pipelined throughput: no per-frame idle wait, like the game loop.
            var pipelinedStart = Stopwatch.GetTimestamp();
            for (var frame = 0; frame < frames; frame++)
            {
                Pose(warmup + frames + frame);
                renderer.Render(world, camera, 30, (warmup + frames + frame) / 60.0, 1.0 / 60, settings, null);
            }
            device.WaitForIdle();
            var pipelinedMs = (Stopwatch.GetTimestamp() - pipelinedStart) * 1000.0 / Stopwatch.Frequency / frames;

            cpu.Sort();
            total.Sort();
            Console.WriteLine(
                $"BENCH {name,-28} render={renderWidth}x{renderHeight} fg={(renderer.FrameGenerationScene is not null ? "on" : "off")} " +
                $"cpu avg={cpu.Average():0.00}ms  serial avg={total.Average():0.00}ms p50={total[total.Count / 2]:0.00} " +
                $"p95={total[(int)(total.Count * 0.95)]:0.00}  pipelined={pipelinedMs:0.00}ms ({1000.0 / pipelinedMs:0.0} rendered fps)");
        }
        finally
        {
            renderer.Dispose();
        }
    }
}
