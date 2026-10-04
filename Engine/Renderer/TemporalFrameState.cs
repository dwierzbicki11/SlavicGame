using System.Numerics;

namespace SlavicGame.Engine.Renderer;

public readonly record struct TemporalFrameData(
    Matrix4x4 Projection,
    Matrix4x4 View,
    Matrix4x4 CurrentViewProjection,
    Matrix4x4 PreviousViewProjection,
    Matrix4x4 InverseCurrentViewProjection,
    Vector2 JitterPixels,
    Vector2 PreviousJitterPixels,
    uint FrameIndex,
    bool ResetHistory);

/// <summary>
/// Owns the camera history required by temporal anti-aliasing and temporal
/// upscalers such as FSR 2/3. The current renderer keeps jitter disabled until
/// a temporal upscaler consumes the history, but motion/depth plumbing can be
/// built and tested independently.
/// </summary>
public sealed class TemporalFrameState
{
    private Matrix4x4 _previousViewProjection = Matrix4x4.Identity;
    private Vector2 _previousJitterPixels;
    private uint _frameIndex;
    private bool _hasHistory;
    private bool _resetRequested = true;

    public uint FrameIndex => _frameIndex;
    public bool HasHistory => _hasHistory;

    public TemporalFrameData BeginFrame(
        Matrix4x4 projection,
        Matrix4x4 view,
        uint renderWidth,
        uint renderHeight,
        bool enableJitter)
    {
        renderWidth = Math.Max(1u, renderWidth);
        renderHeight = Math.Max(1u, renderHeight);

        var jitterPixels = enableJitter
            ? JitterForFrame(_frameIndex)
            : Vector2.Zero;
        var jitterNdc = new Vector2(
            jitterPixels.X * 2f / renderWidth,
            jitterPixels.Y * 2f / renderHeight);
        var jitteredProjection = enableJitter
            ? ApplyJitter(projection, jitterNdc)
            : projection;

        // System.Numerics uses row-vector composition. Uploaded matrices are
        // consumed by GLSL as column-major data, so View * Projection here is
        // seen by the shader as Projection * View.
        var currentViewProjection = view * jitteredProjection;
        if (!Matrix4x4.Invert(
                currentViewProjection,
                out var inverseCurrentViewProjection))
        {
            throw new InvalidOperationException(
                "Current camera view-projection matrix is not invertible.");
        }

        var resetHistory = !_hasHistory || _resetRequested;
        var previousViewProjection = resetHistory
            ? currentViewProjection
            : _previousViewProjection;
        var previousJitter = resetHistory
            ? jitterPixels
            : _previousJitterPixels;

        var frame = new TemporalFrameData(
            jitteredProjection,
            view,
            currentViewProjection,
            previousViewProjection,
            inverseCurrentViewProjection,
            jitterPixels,
            previousJitter,
            _frameIndex,
            resetHistory);

        _previousViewProjection = currentViewProjection;
        _previousJitterPixels = jitterPixels;
        _hasHistory = true;
        _resetRequested = false;
        _frameIndex++;

        return frame;
    }

    public void Reset()
    {
        _hasHistory = false;
        _resetRequested = true;
        _previousViewProjection = Matrix4x4.Identity;
        _previousJitterPixels = Vector2.Zero;
    }

    public static Vector2 JitterForFrame(uint frameIndex)
    {
        // Eight samples are enough for the first temporal implementation and
        // keep the pattern deterministic across platforms and save/load.
        var sample = frameIndex % 8u + 1u;
        return new Vector2(
            Halton(sample, 2u) - 0.5f,
            Halton(sample, 3u) - 0.5f);
    }

    public static Matrix4x4 ApplyJitter(
        Matrix4x4 projection,
        Vector2 jitterNdc)
    {
        // Perspective matrices created by System.Numerics have M34 = -1.
        // Adding a multiple of clip W through M31/M32 shifts projected NDC
        // without changing depth.
        projection.M31 += jitterNdc.X * projection.M34;
        projection.M32 += jitterNdc.Y * projection.M34;
        return projection;
    }

    private static float Halton(uint index, uint basis)
    {
        var result = 0f;
        var fraction = 1f;
        while (index > 0u)
        {
            fraction /= basis;
            result += fraction * (index % basis);
            index /= basis;
        }

        return result;
    }
}
