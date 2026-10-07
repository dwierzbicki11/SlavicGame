using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public readonly struct DynamicMotionVertex
{
    public const uint SizeInBytes = 24;

    public readonly Vector3 CurrentPosition;
    public readonly Vector3 PreviousPosition;

    public DynamicMotionVertex(
        Vector3 currentPosition,
        Vector3 previousPosition)
    {
        CurrentPosition = currentPosition;
        PreviousPosition = previousPosition;
    }
}

/// <summary>
/// Keeps one frame of CPU-generated dynamic geometry so the temporal pipeline
/// can provide object motion in addition to camera motion. Only the stable,
/// opaque actor/model prefix is fed here; water, particles and spectral VFX
/// continue to rely on the reactive mask.
/// </summary>
public sealed class DynamicMotionHistory
{
    private Vector3[] _previousPositions = [];
    private int _previousCount;

    public int PreviousVertexCount => _previousCount;

    public DynamicMotionVertex[] Build(
        ReadOnlySpan<TerrainVertex> currentVertices,
        bool resetHistory)
    {
        var result = new DynamicMotionVertex[currentVertices.Length];
        BuildInto(currentVertices, resetHistory, result);
        return result;
    }

    /// <summary>
    /// Fills caller-owned storage so the renderer can reuse its upload array
    /// instead of allocating two CPU arrays for every temporal frame.
    /// </summary>
    public void BuildInto(
        ReadOnlySpan<TerrainVertex> currentVertices,
        bool resetHistory,
        Span<DynamicMotionVertex> destination)
    {
        var count = currentVertices.Length;
        if (destination.Length < count)
            throw new ArgumentException("Destination is smaller than the current vertex span.", nameof(destination));

        var previousMatches = !resetHistory && _previousCount == count;
        EnsurePreviousCapacity(count);

        for (var i = 0; i < count; i++)
        {
            var current = currentVertices[i].Position;
            var previous = previousMatches
                ? _previousPositions[i]
                : current;

            destination[i] = new DynamicMotionVertex(current, previous);
            _previousPositions[i] = current;
        }

        _previousCount = count;
    }

    private void EnsurePreviousCapacity(int count)
    {
        if (_previousPositions.Length >= count)
            return;

        var capacity = Math.Max(count, Math.Max(64, _previousPositions.Length * 2));
        Array.Resize(ref _previousPositions, capacity);
    }

    public void Reset()
    {
        _previousCount = 0;
    }
}
