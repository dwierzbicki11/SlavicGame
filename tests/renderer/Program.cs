using System.Numerics;
using System.Runtime.InteropServices;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Renderer.FidelityFx;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.UI;
using SlavicGame.Engine.World;
using Veldrid;

// This executable references the game assembly: no copied renderer or native
// fixture renders these scenes. Its output framebuffer replaces only the OS
// swapchain endpoint, so terrain, actors, depth, velocity, filters and UI run
// through VeldridRenderer's shipping path.
if (!OperatingSystem.IsLinux())
{
    Console.WriteLine("Scene FG GPU proof requires the Linux provider.");
    return;
}
if (Environment.GetEnvironmentVariable("SLAVICGAME_TEST_FSR3_NATIVE") != "1")
    throw new InvalidOperationException("Set SLAVICGAME_TEST_FSR3_NATIVE=1 for the required scene GPU proof.");
Environment.SetEnvironmentVariable(FidelityFxSceneFrameGeneration.SceneValidationVariable, "1");
Environment.SetEnvironmentVariable(FidelityFxUpscalerPolicy.EnvironmentVariable, null);
VulkanRuntimeCompatibility.EnsureInitialized();
using var device = VulkanDeviceFactory.Create(new GraphicsDeviceOptions {
    PreferStandardClipSpaceYDirection = true, PreferDepthRangeZeroToOne = true });
var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
    Console.WriteLine("PASS: " + message);
}

var world = new WorldState();
world.Initialize();
var settings = new GameSettings();
GraphicsPresetCatalog.Apply(settings, GraphicsPreset.LowEnd);
settings.Upscaler = UpscalerMode.Bilinear;
settings.AntiAliasing = AntiAliasingMode.Off;
settings.ShowFps = true;
var camera = new Camera3D();
var ground = world.Terrain.SampleHeight(new Vector3(0, 0, -85));
void Pose(float x) => camera.SetCinematicPose(new Vector3(x, ground + 7, -66),
    new Vector3(x, ground + 2, -87));
Pose(0);
using var target = new Target(device, 640, 360);
using (var renderer = new VeldridRenderer())
{
    renderer.InitializeOffscreen(device, target.Framebuffer, world, TextureQuality.Low,
        MsaaQuality.X4, UpscalerMode.Bilinear);
    var fg = renderer.FrameGenerationScene ?? throw new Exception(renderer.FrameGenerationDiagnostic);
    Check(!renderer.NativeUpscalerReady && renderer.SceneDepth?.SampleCount == TextureSampleCount.Count1,
        "FG creates render-size single-sample depth independently of the FSR3 upscaler");
    void Render(int frame, MenuView? menu = null) => renderer.Render(world, camera, 60,
        frame / 60.0, 1.0 / 60, settings, menu);
    Render(0);
    Check(!fg.HasGeneratedFrame && fg.LastFrameId == 0, "First actual scene frame initializes AMD history");
    Rgba16[] previous = Read<Rgba16>(device, fg.HudlessColor);
    Rgba16[] current = previous;
    // AMD's first ten frames favor game motion during optical-flow warmup.
    // Cross that boundary with moving geometry and validate every result.
    const int movingFrames = 12;
    for (int frame = 1; frame <= movingFrames; frame++)
    {
        Pose(frame * 0.3f);
        Render(frame);
        Check(fg.HasGeneratedFrame && fg.LastFrameId == (ulong)frame,
            "Actual moving scene generates consecutive frame " + frame);
        Check(Read<Rgba16>(device, fg.GeneratedColor).All(p => p.Finite),
            "Actual moving scene interpolation has finite RGB on frame " + frame);
        previous = current;
        current = Read<Rgba16>(device, fg.HudlessColor);
    }
    var generated = Read<Rgba16>(device, fg.GeneratedColor);
    var depths = Read<float>(device, renderer.SceneDepth!);
    var motions = Read<Rg16>(device, renderer.SceneMotion!);
    Console.WriteLine($"Depth: min={depths.Min()}, max={depths.Max()}, nonfinite={depths.Count(z => !float.IsFinite(z))}; motion nonfinite={motions.Count(v => !float.IsFinite((float)v.X) || !float.IsFinite((float)v.Y))}");
    Console.WriteLine($"Nonfinite scene pixels: {current.Count(p => !p.Finite)}, generated: {generated.Count(p => !p.Finite)}");
    for (var i = 0; i < current.Length; i++)
        if (!current[i].Finite || !generated[i].Finite)
        {
            Console.WriteLine($"First nonfinite pixel ({i % 640},{i / 640}): scene={current[i].R},{current[i].G},{current[i].B}; FG={generated[i].R},{generated[i].G},{generated[i].B}");
            break;
        }
    Check(current.All(p => p.Finite) && generated.All(p => p.Finite),
        "Actual scene and AMD generated GPU images contain finite RGB values");
    Check(current.Max(p => (float)p.R) - current.Min(p => (float)p.R) > 0.02f,
        "FG input contains visible nonconstant scene geometry");
    var inputMotion = Difference(previous, current);
    var fromCurrent = Difference(generated, current);
    var fromPrevious = Difference(generated, previous);
    Console.WriteLine($"Scene RGB differences: motion={inputMotion:G6}, generated/current={fromCurrent:G6}, generated/previous={fromPrevious:G6}");
    Check(inputMotion > 0.0001 && fromCurrent > 0.00001 && fromPrevious > 0.00001,
        "AMD interpolation responds to scene movement and differs from both adjacent rendered images");
    Check(Read<float>(device, renderer.SceneDepth!).Any(z => z > 0 && z < 0.9999f),
        "FG consumes geometry depth from the real scene pass");
    Check(Read<Rg16>(device, renderer.SceneMotion!).Any(v => Math.Abs((float)v.X) > 0.0001f),
        "FG consumes camera motion reconstructed by the real velocity shader");
    var displayed = Read<Rgba16>(device, target.Color);
    Check(Difference(displayed, current) > 0.0001,
        "Rendered output has HUD pixels which the FG input excludes");

    // Freeze camera/animation and change only the separately drawn menu.
    var menu = new MenuView("FG UI EXCLUSION", "SCENE INPUT TEST", [],
        [new MenuPanelView("PANEL", [new MenuItemView("VISIBLE UI", null, true)])], "FOOTER");
    Render(movingFrames, menu);
    var withMenuScene = Read<Rgba16>(device, fg.HudlessColor);
    var withMenuDisplay = Read<Rgba16>(device, target.Color);
    Check(Difference(withMenuScene, current) < 0.000001 && Difference(withMenuDisplay, displayed) > 0.001,
        "Menu changes the displayed frame while leaving HUD-free FG scene pixels unchanged");
    renderer.ResetTemporalHistory();
    Render(movingFrames + 1);
    Check(!fg.HasGeneratedFrame && fg.LastFrameId == movingFrames + 2, "Camera/history reset suppresses interpolation without rewinding frame IDs");
    Render(movingFrames + 2);
    Check(fg.HasGeneratedFrame && fg.LastFrameId == movingFrames + 3, "Real scene FG recovers after camera/history reset");

    using var resized = new Target(device, 800, 450);
    renderer.SetOffscreenTarget(resized.Framebuffer);
    settings.Upscaler = UpscalerMode.Fsr1;
    Render(movingFrames + 3);
    Check(!fg.HasGeneratedFrame && fg.HudlessColor.Width == 800 && fg.HudlessColor.Height == 450 &&
        renderer.SceneDepth!.Width == 640 && renderer.SceneMotion!.Height == 360,
        "Display resize captures FSR1 EASU/RCAS at display size with separate render-size depth/motion");
    Render(movingFrames + 4);
    Check(fg.HasGeneratedFrame && Read<Rgba16>(device, fg.GeneratedColor).All(p => p.Finite),
        "FG recovers with spatial FSR1 instead of depending on a temporal upscaler");
    renderer.SetRenderResolution(720, 405);
    Render(movingFrames + 5);
    Check(!fg.HasGeneratedFrame && renderer.SceneDepth!.Width == 720 && renderer.SceneMotion!.Height == 405,
        "Render-resolution change recreates temporal inputs and suppresses stale interpolation");
    Render(movingFrames + 6);
    Check(fg.HasGeneratedFrame, "Changed render-size scene restores FG on the next actual frame");
    renderer.SetOffscreenTarget(target.Framebuffer);
    renderer.SetRenderResolution(640, 360);
    Render(movingFrames + 7);
    Check(!fg.HasGeneratedFrame && fg.HudlessColor.Width == 640, "Shrinking output and render size safely rebuilds FG history");
    Render(movingFrames + 8);
    Check(fg.HasGeneratedFrame, "Shrunk scene resumes real interpolation");
    var ownedColor = fg.GeneratedColor;
    // Leave real submitted GPU work pending at cleanup; production Dispose
    // must wait before releasing the images consumed by Veldrid/AMD.
    renderer.Dispose();
    renderer.Dispose();
    Check(ownedColor.IsDisposed, "Renderer cleanup safely releases generated scene output and is idempotent");
}

Pose(0);
settings.Upscaler = UpscalerMode.Fsr3;
using (var renderer = new VeldridRenderer())
{
    renderer.InitializeOffscreen(device, target.Framebuffer, world, TextureQuality.Low,
        MsaaQuality.Off, UpscalerMode.Fsr3);
    var fg = renderer.FrameGenerationScene ?? throw new Exception(renderer.FrameGenerationDiagnostic);
    renderer.Render(world, camera, 60, 0, 1.0 / 60, settings, null);
    Pose(0.3f);
    renderer.Render(world, camera, 60, 1.0 / 60, 1.0 / 60, settings, null);
    // Cross both native command/fence and SDK view rings without readback
    // fences between frames. This exposes retirement hidden by serial reads.
    for (int frame = 2; frame < 8; frame++)
    {
        Pose(frame * 0.3f);
        renderer.Render(world, camera, 60, frame / 60.0, 1.0 / 60, settings, null);
    }
    Check(renderer.NativeUpscalerReady && fg.HasGeneratedFrame,
        "Actual renderer orders native FSR3 upscale then HUD-free capture then AMD FG");
    Check(Read<Rgba16>(device, fg.HudlessColor).All(p => p.Finite), "FSR3 HUD-free scene readback is valid");
    Check(Read<Rgba16>(device, fg.GeneratedColor).All(p => p.Finite), "FSR3 plus FG scene readback is valid");
    // Expire the real context to exercise error isolation, without a fake
    // generator or synthetic successful dispatch.
    fg.Dispose();
    renderer.Render(world, camera, 60, 8.0 / 60, 1.0 / 60, settings, null);
    Check(renderer.FrameGenerationScene is null && renderer.NativeUpscalerReady &&
        renderer.FrameGenerationDiagnostic.Contains("disabled", StringComparison.OrdinalIgnoreCase),
        "Expired FG context disables only FG while native FSR3 keeps rendering");
    Check(Read<Rgba16>(device, target.Color).All(p => p.Finite), "FG failure preserves real output and HUD");
}
Console.WriteLine($"Scene FG runtime proof: {checks} checks passed; real renderer GPU frames, movement, separate UI inputs, reset/resize, FSR1/FSR3 and failure isolation. Extra swapchain presentation is not part of this stage.");

static double Difference(Rgba16[] a, Rgba16[] b) => a.Zip(b,
    (x, y) => Math.Abs((float)x.R - (float)y.R) + Math.Abs((float)x.G - (float)y.G) + Math.Abs((float)x.B - (float)y.B)).Average() / 3;

static T[] Read<T>(GraphicsDevice device, Texture texture) where T : unmanaged
{
    using var staging = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
        texture.Width, texture.Height, 1, 1, texture.Format, TextureUsage.Staging));
    using var commands = device.ResourceFactory.CreateCommandList();
    using var fence = device.ResourceFactory.CreateFence(false);
    commands.Begin();
    commands.CopyTexture(texture, staging);
    commands.End();
    device.SubmitCommands(commands, fence);
    device.WaitForFence(fence);
    var map = device.Map<T>(staging, MapMode.Read);
    try
    {
        var pixels = new T[texture.Width * texture.Height];
        for (uint y = 0; y < texture.Height; y++) for (uint x = 0; x < texture.Width; x++)
            pixels[y * texture.Width + x] = map[x, y];
        return pixels;
    }
    finally { device.Unmap(staging); }
}

[StructLayout(LayoutKind.Sequential)]
struct Rgba16
{
    public Half R, G, B, A;
    public readonly bool Finite => float.IsFinite((float)R) && float.IsFinite((float)G) && float.IsFinite((float)B);
}
[StructLayout(LayoutKind.Sequential)]
struct Rg16 { public Half X, Y; }

sealed class Target : IDisposable
{
    public Texture Color { get; }
    private Texture Depth { get; }
    public Framebuffer Framebuffer { get; }
    private readonly GraphicsDevice _device;
    public Target(GraphicsDevice device, uint width, uint height)
    {
        _device = device;
        Color = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R16_G16_B16_A16_Float, TextureUsage.RenderTarget | TextureUsage.Sampled));
        Depth = device.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R32_Float, TextureUsage.DepthStencil));
        Framebuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Depth, Color));
    }
    public void Dispose()
    {
        _device.WaitForIdle();
        Framebuffer.Dispose();
        Depth.Dispose();
        Color.Dispose();
    }
}
