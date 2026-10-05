using System.Runtime.InteropServices;
using SlavicGame.Engine.Renderer.FidelityFx;
using SlavicGame.Engine.Renderer;
using Veldrid;

internal static class FidelityFxNativeRegression
{
    public static void Run(Action<bool, string> check)
    {
        if (Environment.GetEnvironmentVariable("SLAVICGAME_TEST_FSR3_NATIVE") != "1") return;
        check(OperatingSystem.IsLinux(), "Linux FSR native validation runs on Linux");
        check(FidelityFxNativeLibrary.TryLoad(out var library, out var diagnostic), diagnostic);
        using var provider = library!;
        check(provider.IsLoaded, "Source-built Linux FSR provider passes version/ABI validation");
        var create = Marshal.GetDelegateForFunctionPointer<FfxCreateContextDelegate>(provider.GetExport("ffxCreateContext"));
        var destroy = Marshal.GetDelegateForFunctionPointer<FfxDestroyContextDelegate>(provider.GetExport("ffxDestroyContext"));
        var dispatch = Marshal.GetDelegateForFunctionPointer<FfxDispatchDelegate>(provider.GetExport("ffxDispatch"));
        check(create(0, 0, 0) == 6, "Native FSR create rejects null arguments across the managed ABI");
        check(dispatch(0, 0) == 6, "Native FSR dispatch rejects null arguments across the managed ABI");
        var slot = Marshal.AllocHGlobal(nint.Size);
        var descriptor = Marshal.AllocHGlobal(Marshal.SizeOf<FfxCreateContextDescUpscale>());
        try
        {
            Marshal.WriteIntPtr(slot, 0);
            Marshal.StructureToPtr(new FfxCreateContextDescUpscale
            {
                Header = new FfxApiHeader { Type = FfxApi.CreateContextUpscale }
            }, descriptor, false);
            check(create(slot, descriptor, 0) == 6,
                "Native FSR reads marshalled context descriptor and rejects zero dimensions");
            check(destroy(slot, 0) == 0 && Marshal.ReadIntPtr(slot) == 0,
                "Native FSR safely destroys an empty context slot");
        }
        finally
        {
            Marshal.FreeHGlobal(descriptor);
            Marshal.FreeHGlobal(slot);
        }
        // Use the same Veldrid 4.9 device, marshalled descriptors, transitions
        // and command ring as the game, with an asymmetric image readback.
        VulkanRuntimeCompatibility.EnsureInitialized();
        using var device = GraphicsDevice.CreateVulkan(new GraphicsDeviceOptions());
        RunOnDevice(device, check);
    }

    internal static void RunOnDevice(GraphicsDevice device, Action<bool, string> check)
    {
        string diagnostic;
        check(FidelityFxUpscaler.TryCreate(device, 128, 128, out var upscaler, out diagnostic), diagnostic);
        using var fsr = upscaler!;
        RunDispatch(device, fsr, 64, 64, check);
        RunDispatch(device, fsr, 80, 48, check); // Grows width and recreates history.
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Pixel { public Half R, G, B, A; }

    private static void RunDispatch(GraphicsDevice device, FidelityFxUpscaler fsr,
        uint width, uint height, Action<bool, string> check)
    {
        var factory = device.ResourceFactory;
        using var color = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
        using var depth = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
        using var motion = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R16_G16_Float, TextureUsage.Sampled));
        using var reactive = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R8_UNorm, TextureUsage.Sampled));
        using var output = factory.CreateTexture(TextureDescription.Texture2D(width * 2, height * 2, 1, 1,
            PixelFormat.R16_G16_B16_A16_Float, TextureUsage.Storage | TextureUsage.Sampled));
        using var staging = factory.CreateTexture(TextureDescription.Texture2D(width * 2, height * 2, 1, 1,
            output.Format, TextureUsage.Staging));
        int pixels = checked((int)(width * height));
        var rgba = new byte[pixels * 4];
        for (uint y = 0; y < height; y++) for (uint x = 0; x < width; x++)
        {
            int i = checked((int)(y * width + x) * 4);
            rgba[i] = y < height / 2 ? (byte)204 : (byte)26;
            rgba[i + 1] = 64;
            rgba[i + 2] = x < width / 2 ? (byte)179 : (byte)38;
            rgba[i + 3] = 255;
        }
        device.UpdateTexture(color, rgba, 0, 0, 0, width, height, 1, 0, 0);
        device.UpdateTexture(depth, Enumerable.Repeat(0.5f, pixels).ToArray(), 0, 0, 0, width, height, 1, 0, 0);
        device.UpdateTexture(motion, new byte[pixels * 4], 0, 0, 0, width, height, 1, 0, 0);
        device.UpdateTexture(reactive, new byte[pixels], 0, 0, 0, width, height, 1, 0, 0);
        for (uint frame = 0; frame < 4; frame++)
        {
            var temporal = new TemporalFrameData { FrameIndex = frame, ResetHistory = frame == 0 || frame == 3 };
            fsr.Dispatch(color, depth, motion, reactive, output, temporal,
                1f / 60, 0.1f, 100, 1, frame == 0 ? 0 : 0.25f);
        }
        using var commands = factory.CreateCommandList();
        commands.Begin();
        commands.CopyTexture(output, staging);
        commands.End();
        device.SubmitCommands(commands);
        device.WaitForIdle();
        var map = device.Map<Pixel>(staging, MapMode.Read);
        try
        {
            bool valid = true;
            for (uint y = 0; y < output.Height; y++) for (uint x = 0; x < output.Width; x++)
            {
                var p = map[x, y];
                valid &= float.IsFinite((float)p.R) && float.IsFinite((float)p.G) && float.IsFinite((float)p.B);
            }
            check(valid, "Managed FSR dispatch produces finite pixels");
            foreach (uint y in new[] { output.Height / 4, output.Height * 3 / 4 })
                foreach (uint x in new[] { output.Width / 4, output.Width * 3 / 4 })
                {
                    var p = map[x, y];
                    float r = y < output.Height / 2 ? 204f / 255 : 26f / 255;
                    float b = x < output.Width / 2 ? 179f / 255 : 38f / 255;
                    check(Math.Abs((float)p.R - r) < 0.08f && Math.Abs((float)p.B - b) < 0.08f
                        && Math.Abs((float)p.G - 64f / 255) < 0.08f,
                        $"Veldrid FSR {width}x{height} readback preserves quadrant colors/orientation at {x},{y}");
                }
        }
        finally { device.Unmap(staging); }
    }
}
