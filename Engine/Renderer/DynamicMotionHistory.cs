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

    public int PreviousVertexCount => _previousPositions.Length;

    public DynamicMotionVertex[] Build(
        ReadOnlySpan<TerrainVertex> currentVertices,
        bool resetHistory)
    {
        var count = currentVertices.Length;
        var previousMatches =
            !resetHistory &&
            _previousPositions.Length == count;

        var result = new DynamicMotionVertex[count];
        var nextPrevious = new Vector3[count];

        for (var i = 0; i < count; i++)
        {
            var current = currentVertices[i].Position;
            var previous = previousMatches
                ? _previousPositions[i]
                : current;

            result[i] = new DynamicMotionVertex(current, previous);
            nextPrevious[i] = current;
        }

        _previousPositions = nextPrevious;
        return result;
    }

    public void Reset() => _previousPositions = [];
}
