using System.Numerics;
using SlavicGame.Engine.Settings;

namespace SlavicGame.Engine.World;

public static class RainEffectMesh
{
    private const float Radius = 12f;
    private const float VerticalSpan = 15f;

    public static void Append(
        WorldState world,
        Vector3 cameraPosition,
        float seconds,
        CloudQuality quality,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);

        var rain = Math.Clamp(world.Weather.RainIntensity, 0f, 1f);
        if (rain <= 0.025f)
            return;

        var maxCount = GraphicsQualityCatalog.RainStreakCount(quality);
        var count = Math.Max(1, (int)MathF.Ceiling(maxCount * rain));

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);

        var wind = world.Weather.WindIntensity;
        var streakLength = 0.65f + rain * 0.75f;
        var fallSpeed = 10f + rain * 8f;
        var streakDirection = Vector3.Normalize(new Vector3(
            wind * 0.58f,
            -1f,
            wind * 0.18f));

        for (var i = 0; i < count; i++)
        {
            var h0 = Hash01((uint)(i * 3 + 1));
            var h1 = Hash01((uint)(i * 3 + 2));
            var h2 = Hash01((uint)(i * 3 + 3));

            var angle = h0 * MathF.Tau;
            var radius = MathF.Sqrt(h1) * Radius;
            var phase = Fract(h2 + seconds * fallSpeed / VerticalSpan);

            var position = cameraPosition + new Vector3(
                MathF.Cos(angle) * radius,
                7.5f - phase * VerticalSpan,
                MathF.Sin(angle) * radius);

            var toCamera = cameraPosition - position;
            toCamera.Y = 0f;
            if (toCamera.LengthSquared() < 0.0001f)
                toCamera = Vector3.UnitZ;

            var side = Vector3.Normalize(Vector3.Cross(
                Vector3.UnitY,
                Vector3.Normalize(toCamera))) *
                (0.010f + rain * 0.012f);

            var half = streakDirection * streakLength * 0.5f;
            var start = checked((uint)output.Count);
            var color = new Vector3(
                1.06f + rain * 0.05f,
                1.10f + rain * 0.06f,
                1.16f + rain * 0.08f);

            output.Add(new TerrainVertex(position - half - side, color, Vector3.UnitY));
            output.Add(new TerrainVertex(position - half + side, color, Vector3.UnitY));
            output.Add(new TerrainVertex(position + half + side, color, Vector3.UnitY));
            output.Add(new TerrainVertex(position + half - side, color, Vector3.UnitY));

            triangles.AddRange(
                [start, start + 2, start + 1,
                 start, start + 3, start + 2]);
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();
    }

    private static float Hash01(uint value)
    {
        value ^= value >> 16;
        value *= 0x7feb352du;
        value ^= value >> 15;
        value *= 0x846ca68bu;
        value ^= value >> 16;
        return (value & 0x00ffffffu) / 16777215f;
    }

    private static float Fract(float value) =>
        value - MathF.Floor(value);
}
