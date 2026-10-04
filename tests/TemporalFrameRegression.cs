using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class TemporalFrameRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var state = new TemporalFrameState();
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 3f,
            16f / 9f,
            0.1f,
            1000f);
        var view = Matrix4x4.CreateLookAt(
            new Vector3(0f, 2f, -5f),
            new Vector3(0f, 2f, 0f),
            Vector3.UnitY);

        var first = state.BeginFrame(
            projection,
            view,
            1280,
            720,
            enableJitter: true);
        check(first.ResetHistory,
            "Temporal history resets on the first frame");
        check(first.CurrentViewProjection == first.PreviousViewProjection,
            "First temporal frame cannot reference stale camera history");
        check(first.JitterPixels.X >= -0.5f &&
              first.JitterPixels.X <= 0.5f &&
              first.JitterPixels.Y >= -0.5f &&
              first.JitterPixels.Y <= 0.5f,
            "Temporal jitter stays within one source pixel footprint");

        var second = state.BeginFrame(
            projection,
            view,
            1280,
            720,
            enableJitter: true);
        check(!second.ResetHistory &&
              second.PreviousViewProjection == first.CurrentViewProjection,
            "Temporal frame carries the exact previous view-projection");
        check(second.JitterPixels != first.JitterPixels,
            "Temporal jitter sequence advances every frame");

        var unjittered = new TemporalFrameState().BeginFrame(
            projection,
            view,
            1920,
            1080,
            enableJitter: false);
        check(unjittered.JitterPixels == Vector2.Zero &&
              unjittered.Projection == projection,
            "Spatial render path remains bit-for-bit unjittered");

        state.Reset();
        var afterReset = state.BeginFrame(
            projection,
            view,
            1280,
            720,
            enableJitter: true);
        check(afterReset.ResetHistory &&
              afterReset.CurrentViewProjection ==
              afterReset.PreviousViewProjection,
            "Resize/camera cuts can invalidate temporal history safely");

        var samples = Enumerable.Range(0, 8)
            .Select(i => TemporalFrameState.JitterForFrame((uint)i))
            .ToArray();
        check(samples.Distinct().Count() == 8,
            "Eight-sample temporal jitter pattern has no duplicates");
    }
}
