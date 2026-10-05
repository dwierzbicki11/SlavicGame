using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Settings;
using SlavicGame.Engine.Renderer.FidelityFx;
using Veldrid;

internal static class UpscalerOrientationProof
{
    public static void Run()
    {
        // Exercise the actual EASU -> RCAS pipelines, not shader source patterns.
        // Both Vulkan viewport conventions must agree with direct presentation.
        foreach (bool standardY in new[] { false, true })
        {
            using var device = VulkanDeviceFactory.Create(new GraphicsDeviceOptions {
                PreferStandardClipSpaceYDirection = standardY,
                PreferDepthRangeZeroToOne = true });
            foreach (var size in new[] { (1366u, 768u), (1280u, 720u), (960u, 540u) })
                RunSize(device, size.Item1, size.Item2);
        }
        Console.WriteLine("Upscaler orientation runtime proof: 12 checks passed;");
    }

    private static void RunSize(GraphicsDevice device, uint width, uint height)
    {
        var factory = device.ResourceFactory;
        using var source = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
        using var view = factory.CreateTextureView(source);
        using var output = factory.CreateTexture(TextureDescription.Texture2D(1366, 768, 1, 1,
            PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget));
        using var target = factory.CreateFramebuffer(new FramebufferDescription(null, output));
        using var staging = factory.CreateTexture(TextureDescription.Texture2D(1366, 768, 1, 1,
            output.Format, TextureUsage.Staging));
        using var scaler = new ResolutionScalerRenderer();
        scaler.Initialize(device, target.OutputDescription, width, height, MsaaQuality.Off);
        var pixels = new byte[checked((int)(width * height * 4))];
        for (uint y = 0; y < height; y++) for (uint x = 0; x < width; x++)
        {
            int i = checked((int)((y * width + x) * 4));
            pixels[i] = y < height / 2 ? (byte)204 : (byte)26;
            pixels[i + 1] = 64;
            pixels[i + 2] = x < width / 2 ? (byte)179 : (byte)38;
            pixels[i + 3] = 255;
        }
        device.UpdateTexture(source, pixels, 0, 0, 0, width, height, 1, 0, 0);
        var baseline = Read(UpscalerMode.Bilinear, view);
        if (Math.Abs(baseline[0] - baseline[6]) < 150 ||
            Math.Abs(baseline[2] - baseline[5]) < 100 ||
            Enumerable.Range(0, 4).Any(i => Math.Abs(baseline[i * 3 + 1] - 64) > 2))
            throw new Exception("BILINEAR readback did not preserve the asymmetric test image");
        var fsr = Read(UpscalerMode.Fsr1, view);
        for (int i = 0; i < baseline.Length; i++)
            if (Math.Abs(baseline[i] - fsr[i]) > 8)
                throw new Exception($"FSR1 differs from BILINEAR at {width}x{height}, " +
                    $"clipYInverted={device.IsClipSpaceYInverted}, sample={i}: {baseline[i]} vs {fsr[i]}");
        Console.WriteLine($"PASS: FSR1 {width}x{height} -> 1366x768 matches BILINEAR orientation " +
            $"(clipYInverted={device.IsClipSpaceYInverted})");

        // Native temporal output preserves texture row order; compose it with
        // the same final pass as the game and compare against spatial baseline.
        if (!FidelityFxUpscaler.TryCreate(device, 1366, 768, out var upscaler, out var diagnostic))
            throw new Exception(diagnostic);
        using var fsr3 = upscaler!;
        using var depth = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
        using var motion = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R16_G16_Float, TextureUsage.Sampled));
        using var reactive = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R8_UNorm, TextureUsage.Sampled));
        scaler.EnsureFsr3Output(1366, 768);
        int count = checked((int)(width * height));
        device.UpdateTexture(depth, Enumerable.Repeat(0.5f, count).ToArray(), 0, 0, 0, width, height, 1, 0, 0);
        device.UpdateTexture(motion, new byte[count * 4], 0, 0, 0, width, height, 1, 0, 0);
        device.UpdateTexture(reactive, new byte[count], 0, 0, 0, width, height, 1, 0, 0);
        for (uint frame = 0; frame < 2; frame++)
            fsr3.Dispatch(source, depth, motion, reactive, scaler.Fsr3OutputTexture,
                new TemporalFrameData { FrameIndex = frame, ResetHistory = frame == 0 },
                1f / 60, 0.1f, 100, 1, 0);
        var temporal = Read(UpscalerMode.Bilinear, scaler.Fsr3OutputView);
        for (int i = 0; i < baseline.Length; i++)
            if (Math.Abs(baseline[i] - temporal[i]) > 8)
                throw new Exception($"Native FSR3 differs from BILINEAR at {width}x{height}, " +
                    $"clipYInverted={device.IsClipSpaceYInverted}, sample={i}");
        Console.WriteLine($"PASS: Native FSR3 {width}x{height} -> 1366x768 matches BILINEAR orientation " +
            $"(clipYInverted={device.IsClipSpaceYInverted})");

        byte[] Read(UpscalerMode mode, TextureView input)
        {
            using var commands = factory.CreateCommandList();
            commands.Begin();
            scaler.Present(commands, target, mode, 0.2f, input);
            commands.CopyTexture(output, staging);
            commands.End();
            device.SubmitCommands(commands);
            device.WaitForIdle();
            var map = device.Map<byte>(staging, MapMode.Read);
            try
            {
                var samples = new List<byte>();
                foreach (uint y in new[] { 192u, 576u })
                    foreach (uint x in new[] { 341u, 1024u })
                        for (uint channel = 0; channel < 3; channel++)
                            samples.Add(map[x * 4 + channel, y]);
                return samples.ToArray();
            }
            finally { device.Unmap(staging); }
        }
    }
}
