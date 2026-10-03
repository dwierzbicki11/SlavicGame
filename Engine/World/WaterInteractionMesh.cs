using System.Numerics;

namespace SlavicGame.Engine.World;

public static class WaterInteractionMesh
{
    private const int Segments = 24;
    private const int RingCount = 3;

    public static void Append(
        WorldState world,
        float seconds,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);

        var state = world.WaterInteraction;
        if (!state.IsInWater || state.MovementIntensity <= 0.02f)
            return;

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        var center = state.SurfacePosition;
        var strength = Math.Clamp(state.MovementIntensity, 0f, 1f);

        for (var ring = 0; ring < RingCount; ring++)
        {
            var phase = Fract(seconds * 0.72f + ring / (float)RingCount);
            var radius = 0.30f + phase * 2.35f;
            var width = 0.035f + 0.025f * (1f - phase);
            var brightness = (1f - phase) * strength;
            var color = Vector3.Lerp(
                new Vector3(0.09f, 0.22f, 0.24f),
                new Vector3(0.45f, 0.70f, 0.69f),
                brightness);

            AppendRing(output, triangles, center, radius, width, color);
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();
    }

    private static void AppendRing(
        List<TerrainVertex> vertices,
        List<uint> indices,
        Vector3 center,
        float radius,
        float width,
        Vector3 color)
    {
        var start = checked((uint)vertices.Count);
        for (var i = 0; i <= Segments; i++)
        {
            var angle = MathF.Tau * i / Segments;
            var direction = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle));
            vertices.Add(new TerrainVertex(
                center + direction * MathF.Max(0f, radius - width),
                color,
                Vector3.UnitY));
            vertices.Add(new TerrainVertex(
                center + direction * (radius + width),
                color,
                Vector3.UnitY));
        }

        for (uint i = 0; i < Segments; i++)
        {
            var a = start + i * 2;
            var b = a + 1;
            var c = a + 2;
            var d = a + 3;

            indices.Add(a);
            indices.Add(d);
            indices.Add(b);
            indices.Add(a);
            indices.Add(c);
            indices.Add(d);
        }
    }

    private static float Fract(float value) =>
        value - MathF.Floor(value);
}
