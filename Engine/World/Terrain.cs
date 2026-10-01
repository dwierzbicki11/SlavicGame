using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed class Terrain
{
    public int Width { get; }
    public int Depth { get; }
    public float CellSize { get; }

    private readonly float[,] _heights;

    public Terrain(int width = 128, int depth = 128, float cellSize = 2f)
    {
        if (width < 2) throw new ArgumentOutOfRangeException(nameof(width));
        if (depth < 2) throw new ArgumentOutOfRangeException(nameof(depth));
        if (!float.IsFinite(cellSize) || cellSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        Width = width;
        Depth = depth;
        CellSize = cellSize;
        _heights = new float[width, depth];
        Generate();
    }

    public float GetHeight(int x, int z) => _heights[x, z];

    public float SampleHeight(Vector3 worldPosition)
    {
        if (!float.IsFinite(worldPosition.X) || !float.IsFinite(worldPosition.Z))
            throw new ArgumentOutOfRangeException(nameof(worldPosition));
        var gx = Math.Clamp(worldPosition.X / CellSize + (Width - 1) * 0.5f, 0f, Width - 1);
        var gz = Math.Clamp(worldPosition.Z / CellSize + (Depth - 1) * 0.5f, 0f, Depth - 1);

        var x0 = Math.Clamp((int)MathF.Floor(gx), 0, Width - 1);
        var z0 = Math.Clamp((int)MathF.Floor(gz), 0, Depth - 1);
        var x1 = Math.Min(x0 + 1, Width - 1);
        var z1 = Math.Min(z0 + 1, Depth - 1);

        var tx = gx - x0;
        var tz = gz - z0;
        // Match the two planar triangles emitted by TerrainMesh (diagonal a-c).
        var a = _heights[x0, z0];
        var b = _heights[x1, z0];
        var c = _heights[x1, z1];
        var d = _heights[x0, z1];
        return tx >= tz
            ? a + tx * (b - a) + tz * (c - b)
            : a + tx * (c - d) + tz * (d - a);
    }

    // First contact with the rendered heightfield, offset vertically by clearance.
    // Splitting at grid edges and a-c diagonals makes each interval planar, so a
    // narrow ridge cannot be missed between fixed-distance ray samples.
    public float? IntersectGroundSegment(Vector3 start, Vector3 end, float clearance = 0f)
    {
        if (!IsFinite(start)) throw new ArgumentOutOfRangeException(nameof(start));
        if (!IsFinite(end)) throw new ArgumentOutOfRangeException(nameof(end));
        if (!float.IsFinite(clearance) || clearance < 0f)
            throw new ArgumentOutOfRangeException(nameof(clearance));

        var startGap = start.Y - SampleHeight(start) - clearance;
        if (startGap <= 0f) return 0f;

        var gridStart = ToGrid(start);
        var gridEnd = ToGrid(end);
        var crossings = new List<float> { 0f, 1f };
        AddGridCrossings(gridStart.X, gridEnd.X, Width, crossings);
        AddGridCrossings(gridStart.Y, gridEnd.Y, Depth, crossings);
        crossings.Sort();

        var previousT = 0f;
        var previousGap = startGap;
        for (var i = 1; i < crossings.Count; i++)
        {
            var from = crossings[i - 1];
            var to = crossings[i];
            if (to <= from) continue;

            var midpoint = ClampGrid(Vector2.Lerp(gridStart, gridEnd, (from + to) * 0.5f));
            var cellX = Math.Min((int)MathF.Floor(midpoint.X), Width - 2);
            var cellZ = Math.Min((int)MathF.Floor(midpoint.Y), Depth - 2);
            var a = ClampGrid(Vector2.Lerp(gridStart, gridEnd, from));
            var b = ClampGrid(Vector2.Lerp(gridStart, gridEnd, to));
            var diagonalA = (a.X - cellX) - (a.Y - cellZ);
            var diagonalB = (b.X - cellX) - (b.Y - cellZ);

            if ((diagonalA < 0f && diagonalB > 0f) || (diagonalA > 0f && diagonalB < 0f))
            {
                var diagonalT = from + (to - from) * diagonalA / (diagonalA - diagonalB);
                var diagonalHit = CheckInterval(diagonalT);
                if (diagonalHit is not null) return diagonalHit;
            }

            var hit = CheckInterval(to);
            if (hit is not null) return hit;
        }
        return null;

        float? CheckInterval(float t)
        {
            var position = Vector3.Lerp(start, end, t);
            var gap = position.Y - SampleHeight(position) - clearance;
            if (gap <= 0f)
                return previousT + (t - previousT) * previousGap / (previousGap - gap);
            previousT = t;
            previousGap = gap;
            return null;
        }
    }

    private Vector2 ToGrid(Vector3 position) => new(
        position.X / CellSize + (Width - 1) * 0.5f,
        position.Z / CellSize + (Depth - 1) * 0.5f);

    private Vector2 ClampGrid(Vector2 position) => new(
        Math.Clamp(position.X, 0f, Width - 1), Math.Clamp(position.Y, 0f, Depth - 1));

    private static bool IsFinite(Vector3 position) =>
        float.IsFinite(position.X) && float.IsFinite(position.Y) && float.IsFinite(position.Z);

    private static void AddGridCrossings(float from, float to, int size, List<float> crossings)
    {
        if (from == to) return;
        var first = (int)MathF.Ceiling(Math.Clamp(MathF.Min(from, to), 0f, size - 1));
        var last = (int)MathF.Floor(Math.Clamp(MathF.Max(from, to), 0f, size - 1));
        for (var gridLine = first; gridLine <= last; gridLine++)
        {
            var t = (gridLine - from) / (to - from);
            if (t > 0f && t < 1f) crossings.Add(t);
        }
    }

    private void Generate()
    {
        for (var x = 0; x < Width; x++)
        for (var z = 0; z < Depth; z++)
        {
            var nx = x / (float)(Width - 1);
            var nz = z / (float)(Depth - 1);
            var large = MathF.Sin(nx * MathF.PI * 2.2f) * 2.5f
                      + MathF.Cos(nz * MathF.PI * 1.7f) * 2f;
            var detail = MathF.Sin((nx + nz) * MathF.PI * 8f) * 0.45f;
            _heights[x, z] = large + detail;
        }
    }
}
