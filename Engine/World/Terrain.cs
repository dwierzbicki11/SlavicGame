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
