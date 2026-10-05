using System.Numerics;
using System.Runtime.InteropServices;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Renderer.FidelityFx;

internal static class FidelityFxFrameGenerationRegression
{
    public static void Run(Action<bool, string> check)
    {
        check(!FidelityFxFrameGeneration.TryCreate(default, 1u << 22,
                out var unsupported, out var diagnostic) && unsupported is null &&
                diagnostic.Contains("Vulkan 1.1", StringComparison.Ordinal),
            "FG rejects a Vulkan 1.0 instance before accessing native device handles");
        check(!FidelityFxFrameGeneration.TryCreate(default,
                FidelityFxFrameGeneration.MinimumVulkanVersion, out unsupported, out diagnostic),
            "FG does not accept missing device/queue handles");

        if (!OperatingSystem.IsLinux() ||
            Environment.GetEnvironmentVariable("SLAVICGAME_TEST_FSR3_NATIVE") != "1")
            return;

        // Unlike the existing Veldrid 1.0 upscaler test, FG needs a real Vulkan
        // 1.1 instance. The fixture supplies only GPU images/device/readback;
        // the production C# class owns create/prepare/dispatch and submissions.
        var fixturePath = Path.Combine(AppContext.BaseDirectory,
            "native", "fidelityfx", "libslavic_fg_fixture.so");
        var fixtureLibrary = NativeLibrary.Load(fixturePath);
        nint fixture = 0;
        FidelityFxFrameGeneration? generator = null;
        try
        {
            var create = Bind<CreateFixture>(fixtureLibrary, "slavicFgTestCreate");
            var resize = Bind<ResizeFixture>(fixtureLibrary, "slavicFgTestResize");
            var verify = Bind<VerifyFixture>(fixtureLibrary, "slavicFgTestVerify");
            check(create(out fixture, out var instance, out var physical, out var device,
                    out var queue, out var family, out var apiVersion) == 0 && fixture != 0,
                "Managed FG fixture creates a real Vulkan 1.1 device");
            var handles = new FidelityFxVulkanDeviceHandles(instance, physical, device, queue, family);
            check(FidelityFxFrameGeneration.TryCreate(handles, apiVersion, out generator, out diagnostic),
                "Managed FG loads and validates native ABI: " + diagnostic);
            var fg = generator!;
            check(!fg.IsReady, "FG allocates its independent context lazily for actual image sizes");

            check(resize(fixture, 64, 64, 128, 128,
                    out var color, out var depth, out var motion, out var output) == 0,
                "Managed FG fixture initializes shader-readable color/depth/velocity and UAV output");
            var temporal = new TemporalFrameState();
            TemporalFrameData Frame(bool reset = false)
            {
                if (reset) temporal.Reset();
                return temporal.BeginFrame(
                    Matrix4x4.CreatePerspectiveFieldOfView(1, 1, 0.1f, 100),
                    Matrix4x4.CreateLookAt(new Vector3(2, 3, 4), Vector3.Zero, Vector3.UnitY),
                    depth.Description.Width, depth.Description.Height, enableJitter: true);
            }
            bool Dispatch(ulong id, bool reset = false) => fg.Dispatch(
                color, depth, motion, output, Frame(reset), id, 1f / 60, 0.1f, 100, 1);

            check(!Dispatch(0) && fg.IsReady, "First native FG frame warms up history without scheduling interpolation");
            // Fill every ring slot and reuse them while the GPU owns previous
            // submissions; this exercises the actual fence recycling path.
            for (ulong id = 1; id <= 7; id++)
                check(Dispatch(id), "Consecutive managed FG dispatch can generate frame " + id);
            fg.WaitForIdle();
            check(verify(fixture) == 0,
                "C# prepare + optical flow + interpolation reconstruct known GPU color at 128x128");

            check(!Dispatch(20), "A frame-ID gap resets FG instead of interpolating stale history");
            check(Dispatch(21), "FG resumes on the next consecutive frame after a gap");
            fg.RequestReset();
            check(!Dispatch(22), "Explicit FG reset suppresses the intermediate frame");
            check(Dispatch(23), "FG resumes after an explicit reset");
            check(!Dispatch(24, reset: true), "Camera temporal reset also resets FG history");
            check(Dispatch(25), "FG resumes after a camera history reset");

            var invalidOutput = output;
            invalidOutput.Resource = color.Resource;
            ExpectArgumentFailure(() => fg.Dispatch(color, depth, motion, invalidOutput,
                Frame(), 26, 1f / 60, 0.1f, 100, 1));
            check(Dispatch(26), "Invalid aliased output is rejected without advancing native frame history");
            fg.WaitForIdle();
            check(verify(fixture) == 0, "Managed FG reset/recovery output remains finite and reconstructs input");

            check(resize(fixture, 80, 48, 160, 96, out color, out depth, out motion, out output) == 0,
                "Managed FG fixture changes render and display extents");
            check(!Dispatch(27), "Size change waits for GPU work, recreates FG context and resets history");
            check(Dispatch(28), "Resized FG context generates the next consecutive frame");
            fg.WaitForIdle();
            check(verify(fixture) == 0, "Managed FG reads back reconstructed color at 160x96 after resize");

            // Shrinking also requires a new display-sized optical-flow context.
            check(resize(fixture, 64, 64, 128, 128, out color, out depth, out motion, out output) == 0,
                "Managed FG fixture can shrink the display");
            check(!Dispatch(29) && Dispatch(30), "Shrinking FG context resets then restores interpolation");
            fg.WaitForIdle();
            check(verify(fixture) == 0, "Shrunk FG context produces valid GPU output");
            fg.Dispose();
            fg.Dispose();
            check(!fg.IsReady, "FG cleanup is idempotent and waits for submitted GPU work");
            check(FidelityFxNativeLibrary.TryLoad(out var upscalerLibrary, out diagnostic),
                "Disposing FG leaves the temporal upscaler provider loadable");
            upscalerLibrary!.Dispose();
            Console.WriteLine("FSR3 managed FG passed: real prepare/dispatch/readback, " +
                "128x128 -> 160x96 -> 128x128, history resets and fence-ring reuse.");
        }
        finally
        {
            generator?.Dispose();
            if (fixture != 0) Bind<DestroyFixture>(fixtureLibrary, "slavicFgTestDestroy")(fixture);
            NativeLibrary.Free(fixtureLibrary);
        }
    }

    private static void ExpectArgumentFailure(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new Exception("FG accepted invalid resources");
    }

    private static T Bind<T>(nint library, string name) where T : Delegate =>
        Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint CreateFixture(out nint fixture, out nint instance, out nint physical,
        out nint device, out nint queue, out uint family, out uint apiVersion);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint ResizeFixture(nint fixture, uint renderW, uint renderH, uint displayW, uint displayH,
        out FfxApiResource color, out FfxApiResource depth, out FfxApiResource motion, out FfxApiResource output);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint VerifyFixture(nint fixture);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DestroyFixture(nint fixture);
}
