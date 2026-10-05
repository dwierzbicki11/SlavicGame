using System.Diagnostics;
using System.Numerics;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Renderer.FidelityFx;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.Windowing;
using SlavicGame.Engine.World;
using Veldrid;

internal static class PresentationProof
{
    internal static void Run()
    {
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            checks++;
            Console.WriteLine("PASS: " + message);
        }
        foreach (var vsync in new[] { false, true })
        {
            using var window = new GameWindow(new EngineConfig { Width = 640, Height = 360,
                Fullscreen = false, VSync = vsync, WindowTitle = "SlavicGame actual FG presentation proof" });
            var world = new WorldState();
            world.Initialize();
            var camera = new Camera3D();
            var ground = world.Terrain.SampleHeight(new Vector3(0, 0, -85));
            var settings = new GameSettings();
            GraphicsPresetCatalog.Apply(settings, GraphicsPreset.LowEnd);
            settings.Upscaler = UpscalerMode.Bilinear;
            settings.AntiAliasing = AntiAliasingMode.Off;
            settings.FpsLimit = FrameRateLimit.Fps60;
            settings.VSync = vsync;
            using var renderer = new VeldridRenderer();
            renderer.Initialize(window, world, vsync, TextureQuality.Low, MsaaQuality.Off, settings.Upscaler);
            var device = renderer.GraphicsDevice;
            var vk = device.GetVulkanInfo();
            var presenter = renderer.FramePresenter ?? throw new Exception(renderer.FrameGenerationDiagnostic);
            var fg = renderer.FrameGenerationScene!;
            Check(vk.CanReadSwapchainImages(device.MainSwapchain), "WSI supports readback of the actual presented image");
            var events = new List<FramePresentation>();
            var copies = new List<Texture>();
            presenter.Presented += events.Add;
            renderer.BeforeFramePresentation = (commands, target, kind, id) =>
            {
                var actualSwapchainImage = target.ColorTargets[0].Target;
                var copy = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    actualSwapchainImage.Width, actualSwapchainImage.Height, 1, 1,
                    actualSwapchainImage.Format, TextureUsage.Staging));
                commands.CopyTexture(actualSwapchainImage, copy);
                copies.Add(copy);
            };
            byte[]? lastGenerated = null;
            byte[]? lastReal = null;
            byte[]? previousReal = null;
            void RenderFrame(int frame, bool expectPair)
            {
                window.PumpEvents();
                Check(window.Exists, "Actual SDL/Vulkan window remains open");
                camera.SetCinematicPose(new Vector3(frame * 0.3f, ground + 7, -66),
                    new Vector3(frame * 0.3f, ground + 2, -87));
                world.Update(1.0 / 60);
                var simulationTime = world.Time.TimeOfDayHours;
                var before = events.Count;
                renderer.Render(world, camera, 60, frame / 60.0, 1.0 / 60, settings, null);
                Check(world.Time.TimeOfDayHours == simulationTime, "Extra presentation does not update simulation again");
                var output = events.Skip(before).ToArray();
                Check(output.Length == (expectPair ? 2 : 1) &&
                    output[^1].Kind == PresentedFrameKind.Rendered &&
                    (!expectPair || output[0].Kind == PresentedFrameKind.Generated) &&
                    output.All(p => p.SceneFrameId == (ulong)frame),
                    "vkQueuePresentKHR accepts generated then real, or only real during history reset");
                Check(output.All(p => vsync ? p.PresentMode == 2 : p.PresentMode is 0 or 2),
                    "Actual FG present mode preserves order rather than replacing queued frames in MAILBOX");
                if (expectPair)
                {
                    var gap = (output[1].Timestamp - output[0].Timestamp) / (double)Stopwatch.Frequency;
                    Check(gap + 0.000001 >= output[1].MinimumSpacingSeconds,
                        $"Actual generated/real present spacing {gap * 1000:F3} ms meets {output[1].MinimumSpacingSeconds * 1000:F3} ms deadline");
                }
                // The copies were recorded in the actual WSI draw command
                // buffers, before their final PRESENT layout transition. This
                // fence waits for those copies, not for an offscreen fixture.
                using var fence = device.ResourceFactory.CreateFence(false);
                using var finish = device.ResourceFactory.CreateCommandList();
                finish.Begin(); finish.End(); device.SubmitCommands(finish, fence); device.WaitForFence(fence);
                Check(copies.Count == output.Length, "Each accepted present uses the captured actual swapchain image");
                var pixels = copies.Select(copy => ReadBytes(device, copy)).ToArray();
                foreach (var copy in copies) copy.Dispose();
                copies.Clear();
                Check(pixels.All(p => RgbRange(p) > 16), "Presented RGB contains nonconstant rendered scene and UI");
                lastGenerated = expectPair ? pixels[0] : null;
                previousReal = lastReal;
                lastReal = pixels[^1];
            }
            RenderFrame(0, false);
            for (int frame = 1; frame <= 12; frame++) RenderFrame(frame, true);
            Check(Difference(previousReal!, lastReal!) > 0.0001 &&
                Difference(lastGenerated!, lastReal!) > 0.00001 && Difference(lastGenerated!, previousReal!) > 0.00001,
                "The actual generated swapchain RGB responds to scene movement and differs from both adjacent real images after AMD optical-flow warmup");
            Check(fg.DispatchCount == 13 && presenter.RenderedCount == 13 && presenter.GeneratedCount == 12 &&
                presenter.PresentedCount == 25, "13 rendered/AMD-dispatched scene frames produce 25 accepted WSI presentations");
            renderer.ResetTemporalHistory();
            RenderFrame(13, false);
            RenderFrame(14, true);
            Check(fg.DispatchCount == presenter.RenderedCount && presenter.GeneratedCount == fg.GeneratedCount,
                "History reset suppresses exactly the intermediate presentation, then real AMD output resumes");
            var generation = events[^1].SwapchainGeneration;
            window.SetWindowedSize(800, 450);
            window.PumpEvents();
            renderer.Resize(800, 450);
            RenderFrame(15, false);
            RenderFrame(16, true);
            Check(events[^1].SwapchainGeneration > generation && fg.HudlessColor.Width == 800,
                "Real swapchain recreation reacquires new images and safely resumes AMD presentation");
            Check(presenter.RenderedCount == 17 && presenter.GeneratedCount == 14 && presenter.PresentedCount == 31,
                "WSI counts include reset and resize without double-counting rendered frames");
            Console.WriteLine($"WSI proof VSync={vsync}: rendered={presenter.RenderedCount}, generated={presenter.GeneratedCount}, presented={presenter.PresentedCount}; physical scanout/FPS quality is not measured by this software-Vulkan test.");
            renderer.BeforeFramePresentation = null;
        }
        Console.WriteLine($"Presentation FG runtime proof: {checks} checks passed; real SDL swapchain acquire/submit/present, moving AMD output, ordered and paced pairs, reset and resize with VSync off/on.");
    }

    private static byte[] ReadBytes(GraphicsDevice device, Texture staging)
    {
        var map = device.Map<byte>(staging, MapMode.Read);
        try
        {
            var bytes = new byte[staging.Width * staging.Height * 4];
            for (uint y = 0; y < staging.Height; y++) for (uint x = 0; x < staging.Width * 4; x++)
                bytes[y * staging.Width * 4 + x] = map[x, y];
            return bytes;
        }
        finally { device.Unmap(staging); }
    }

    private static int RgbRange(byte[] pixels)
    {
        var rgb = pixels.Where((_, i) => i % 4 != 3);
        return rgb.Max() - rgb.Min();
    }

    private static double Difference(byte[] a, byte[] b) =>
        a.Zip(b, (x, y) => Math.Abs(x - y) / 255.0).Where((_, i) => i % 4 != 3).Average();
}
