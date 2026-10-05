using System.Numerics;
using System.Runtime.InteropServices;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Renderer.FidelityFx;
using Veldrid;

internal static class VulkanDeviceRegression
{
    public static void Run(Action<bool, string> check)
    {
        check(VulkanDeviceFactory.SelectInstanceVersion(VulkanDeviceFactory.Vulkan10, true)
            == VulkanDeviceFactory.Vulkan10, "Vulkan 1.0 loaders retain a usable renderer");
        check(VulkanDeviceFactory.SelectInstanceVersion(VulkanDeviceFactory.Vulkan11, true)
            == VulkanDeviceFactory.Vulkan11, "Linux requests Vulkan 1.1 when the loader supports it");
        check(VulkanDeviceFactory.SelectInstanceVersion((1u << 22) | (4u << 12), false)
            == VulkanDeviceFactory.Vulkan10, "Windows keeps its existing Vulkan 1.0 instance contract");
        if (!OperatingSystem.IsLinux() ||
            Environment.GetEnvironmentVariable("SLAVICGAME_TEST_FSR3_NATIVE") != "1") return;

        VulkanRuntimeCompatibility.EnsureInitialized();
        using (var legacy = GraphicsDevice.CreateVulkan(new GraphicsDeviceOptions(),
                   new VulkanDeviceOptions { InstanceApiVersion = VulkanDeviceFactory.Vulkan10 }))
        {
            check(legacy.GetVulkanInfo().InstanceApiVersion == VulkanDeviceFactory.Vulkan10,
                "Creation metadata records the actual Vulkan 1.0 instance on a newer driver");
            check(!FidelityFxFrameGeneration.TryCreate(legacy, out _, out var diagnostic)
                && diagnostic.Contains("instance", StringComparison.Ordinal),
                "Advertised driver API cannot enable FG on a Vulkan 1.0 instance");
        }
        using (var disabled = GraphicsDevice.CreateVulkan(new GraphicsDeviceOptions(),
                   new VulkanDeviceOptions {
                       InstanceApiVersion = VulkanDeviceFactory.Vulkan11,
                       EnableShaderStorageImageExtendedFormats = false }))
        {
            check(!disabled.GetVulkanInfo().ShaderStorageImageExtendedFormatsEnabled,
                "Logical device metadata distinguishes supported and enabled storage features");
            check(!FidelityFxFrameGeneration.TryCreate(disabled, out _, out var diagnostic)
                && diagnostic.Contains("enabled", StringComparison.Ordinal),
                "FG stays disabled when a required device feature was deliberately not enabled");
            using var buffer = disabled.ResourceFactory.CreateBuffer(
                new BufferDescription(16, BufferUsage.UniformBuffer));
            disabled.UpdateBuffer(buffer, 0, new uint[] { 1, 2, 3, 4 });
            disabled.WaitForIdle();
            check(!buffer.IsDisposed, "Missing FG capability leaves ordinary rendering resources usable");
        }

        using var device = VulkanDeviceFactory.Create(new GraphicsDeviceOptions());
        var info = device.GetVulkanInfo();
        check(info.InstanceApiVersion == VulkanDeviceFactory.Vulkan11,
            "Production device factory creates a real Vulkan 1.1 instance");
        check(info.ShaderStorageImageExtendedFormatsEnabled,
            "Production device explicitly enables the supported FG storage feature");
        FidelityFxNativeRegression.RunOnDevice(device, check);
        check(FidelityFxFrameGeneration.TryCreate(device, out var generator, out var diagnostic2), diagnostic2);
        using var fg = generator!;
        var factory = device.ResourceFactory;
        const uint width = 128, height = 128, render = 64;
        using var color = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R16_G16_B16_A16_Float, TextureUsage.Sampled));
        using var depth = factory.CreateTexture(TextureDescription.Texture2D(render, render, 1, 1,
            PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
        using var motion = factory.CreateTexture(TextureDescription.Texture2D(render, render, 1, 1,
            PixelFormat.R16_G16_Float, TextureUsage.Sampled));
        using var output = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            PixelFormat.R16_G16_B16_A16_Float, TextureUsage.Storage | TextureUsage.Sampled));
        using var staging = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1,
            output.Format, TextureUsage.Staging));
        device.UpdateTexture(depth, Enumerable.Repeat(0.5f, (int)(render * render)).ToArray(),
            0, 0, 0, render, render, 1, 0, 0);
        var temporal = new TemporalFrameState();
        var velocities = new Half[render * render * 2];
        for (int i = 0; i < velocities.Length; i += 2) velocities[i] = (Half)(-2f / width);
        device.UpdateTexture(motion, velocities, 0, 0, 0, render, render, 1, 0, 0);
        double[] sums = new double[2];
        for (ulong id = 0; id < 8; id++)
        {
            var pixels = new Pixel[width * height];
            for (uint y = 0; y < height; y++) for (uint x = 0; x < width; x++)
            {
                // Asymmetric scene with a moving high-contrast rectangle.
                bool rectangle = x >= 24 + id * 2 && x < 56 + id * 2 && y >= 32 && y < 96;
                pixels[y * width + x] = new Pixel {
                    R = (Half)(rectangle ? 0.8f : 0.1f),
                    G = (Half)(y < height / 2 ? 0.15f : 0.45f), B = (Half)0.2f, A = (Half)1 };
            }
            device.UpdateTexture(color, pixels, 0, 0, 0, width, height, 1, 0, 0);
            info.TransitionImageLayout(color, 5);
            info.TransitionImageLayout(depth, 5);
            info.TransitionImageLayout(motion, 5);
            info.TransitionImageLayout(output, 1);
            var frame = temporal.BeginFrame(
                Matrix4x4.CreatePerspectiveFieldOfView(1, 1, 0.1f, 100),
                Matrix4x4.Identity, render, render, enableJitter: false);
            check(fg.Dispatch(Wrap(device, color, FfxApi.FormatR16G16B16A16Float, FfxApi.ResourceUsageReadOnly),
                Wrap(device, depth, FfxApi.FormatR32Float, FfxApi.ResourceUsageDepthTarget),
                Wrap(device, motion, FfxApi.FormatR16G16Float, FfxApi.ResourceUsageReadOnly),
                Wrap(device, output, FfxApi.FormatR16G16B16A16Float, FfxApi.ResourceUsageUav),
                frame, id, 1f / 60, 0.1f, 100, 1) == (id != 0),
                "Production Vulkan FG dispatch warms history then generates frame " + id);
            info.OverrideImageLayout(output, 1);
            if (id != 3 && id != 7) continue;
            using var commands = factory.CreateCommandList();
            commands.Begin();
            commands.CopyTexture(output, staging);
            commands.End();
            device.SubmitCommands(commands);
            device.WaitForIdle();
            var map = device.Map<Pixel>(staging, MapMode.Read);
            try
            {
                bool finite = true;
                double weighted = 0;
                for (uint y = 0; y < height; y++) for (uint x = 0; x < width; x++)
                {
                    var pixel = map[x, y];
                    finite &= float.IsFinite((float)pixel.R) && float.IsFinite((float)pixel.G)
                        && float.IsFinite((float)pixel.B);
                    weighted += (x + 1) * (float)pixel.R;
                }
                check(finite, "Production FG readback has finite pixels at frame " + id);
                check(Math.Abs((float)map[8, 16].G - 0.15f) < 0.04f
                    && Math.Abs((float)map[8, 112].G - 0.45f) < 0.04f,
                    "Production FG retains asymmetric top/bottom image orientation");
                sums[id == 3 ? 0 : 1] = weighted;
            }
            finally { device.Unmap(staging); }
        }
        check(sums[1] > sums[0] + 100,
            "Real FG GPU output responds to the moving scene instead of retaining a constant image");
        fg.WaitForIdle();
        device.WaitForIdle();
        Console.WriteLine("Vulkan production-device FG passed: API 1.1, enabled features, " +
            "moving-image prepare/optical-flow/interpolation/readback and API/feature fallback.");
    }

    private static FfxApiResource Wrap(GraphicsDevice device, Texture texture, uint format, uint usage) => new()
    {
        Resource = unchecked((nint)(long)device.GetVulkanInfo().GetVkImage(texture)),
        Description = new FfxApiResourceDescription {
            Type = FfxApi.ResourceTypeTexture2D, Format = format,
            Width = texture.Width, Height = texture.Height, Depth = 1, MipCount = 1, Usage = usage },
        State = usage == FfxApi.ResourceUsageUav
            ? FfxApi.ResourceStateUnorderedAccess : FfxApi.ResourceStateComputeRead
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct Pixel { public Half R, G, B, A; }
}
