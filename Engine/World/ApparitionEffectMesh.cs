using System.Numerics;
using SlavicGame.Engine.Magic;

namespace SlavicGame.Engine.World;

public static class ApparitionEffectMesh
{
    private const int Segments = 10;
    private const int RingCount = 6;

    public static void Append(
        WorldState world,
        float seconds,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);

        var apparition = world.Apparition;
        if (!apparition.IsVisible)
            return;

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);

        var materialization =
            Math.Clamp(
                apparition.Materialization,
                0f,
                1f);
        var response =
            Math.Clamp(
                apparition.KeepsakeResponse,
                0f,
                1f);
        var ritualProgress =
            world.Rituals.IsPerforming ||
            world.Rituals.CompletionGlowRemaining > 0d
                ? (float)world.Rituals.OverallProgress
                : 0f;

        var bob =
            MathF.Sin(seconds * 1.45f) *
            0.08f;
        var dissolveLift =
            (1f - materialization) *
            0.95f;
        var center =
            apparition.Position +
            new Vector3(
                0f,
                0.22f + bob + dissolveLift,
                0f);

        var baseColor =
            Vector3.Lerp(
                new Vector3(
                    0.24f,
                    1.25f,
                    1.75f),
                new Vector3(
                    0.72f,
                    1.85f,
                    2.65f),
                response * 0.72f +
                ritualProgress * 0.28f);

        Span<float> heights =
            [0.00f, 0.38f, 0.82f, 1.18f, 1.52f, 1.78f];
        Span<float> radii =
            [0.22f, 0.38f, 0.43f, 0.35f, 0.27f, 0.18f];

        var ringStarts =
            new uint[RingCount];

        for (var ring = 0;
             ring < RingCount;
             ring++)
        {
            ringStarts[ring] =
                checked((uint)output.Count);

            var ringHeight =
                heights[ring] +
                ritualProgress *
                ring *
                0.045f;
            var radius =
                radii[ring] *
                (0.84f +
                 materialization * 0.16f);

            for (var segment = 0;
                 segment < Segments;
                 segment++)
            {
                var angle =
                    segment *
                    MathF.Tau /
                    Segments;

                var phase =
                    seconds * 1.9f +
                    ring * 0.63f +
                    segment * 0.47f;

                var wobble =
                    MathF.Sin(phase) *
                    0.035f *
                    (1f + response * 0.8f);

                var ritualSpread =
                    ritualProgress *
                    (ring / (float)(RingCount - 1)) *
                    0.08f *
                    MathF.Sin(
                        angle * 3f +
                        seconds * 2.2f);

                var localRadius =
                    radius +
                    wobble +
                    ritualSpread;

                var local =
                    new Vector3(
                        MathF.Cos(angle) *
                            localRadius,
                        ringHeight,
                        MathF.Sin(angle) *
                            localRadius);

                var colorScale =
                    0.70f +
                    ring /
                    (float)(RingCount - 1) *
                    0.30f;

                output.Add(
                    new TerrainVertex(
                        center + local,
                        baseColor *
                            colorScale *
                            materialization,
                        Vector3.Normalize(
                            new Vector3(
                                local.X,
                                0.18f,
                                local.Z))));
            }
        }

        for (var ring = 0;
             ring < RingCount - 1;
             ring++)
        {
            for (uint segment = 0;
                 segment < Segments;
                 segment++)
            {
                var next =
                    (segment + 1) %
                    Segments;

                var a =
                    ringStarts[ring] +
                    segment;
                var b =
                    ringStarts[ring] +
                    next;
                var c =
                    ringStarts[ring + 1] +
                    next;
                var d =
                    ringStarts[ring + 1] +
                    segment;

                triangles.AddRange(
                    [a, c, b, a, d, c]);
            }
        }

        var headCenter =
            center +
            new Vector3(
                0f,
                2.02f +
                ritualProgress * 0.18f,
                0f);

        AddDiamond(
            headCenter,
            0.22f +
            response * 0.035f,
            baseColor *
                (1.08f +
                 response * 0.32f));

        AddTrailingVeil(
            center,
            seconds,
            baseColor,
            materialization,
            ritualProgress);

        for (var i = 0;
             i < 5;
             i++)
        {
            var angle =
                seconds *
                (0.55f + i * 0.07f) +
                i *
                MathF.Tau /
                5f;
            var radius =
                0.62f +
                0.10f *
                MathF.Sin(
                    seconds * 1.2f +
                    i);
            var height =
                0.55f +
                i * 0.24f +
                MathF.Sin(
                    seconds * 1.8f +
                    i * 0.9f) *
                0.12f +
                ritualProgress *
                i *
                0.05f;

            AddDiamond(
                center +
                new Vector3(
                    MathF.Cos(angle) *
                        radius,
                    height,
                    MathF.Sin(angle) *
                        radius),
                0.055f +
                response * 0.025f,
                baseColor *
                    (0.82f +
                     response * 0.34f));
        }

        if (response > 0.05f)
        {
            var pulse =
                0.5f +
                0.5f *
                MathF.Sin(
                    seconds * 4.6f);

            AddDiamond(
                center +
                new Vector3(
                    0f,
                    1.28f,
                    0f),
                0.10f +
                pulse *
                0.08f *
                response,
                new Vector3(
                    1.45f,
                    2.25f,
                    2.85f) *
                response);
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();

        void AddTrailingVeil(
            Vector3 origin,
            float time,
            Vector3 color,
            float visibility,
            float ritual)
        {
            for (var strip = 0;
                 strip < 4;
                 strip++)
            {
                var angle =
                    strip *
                    MathF.Tau /
                    4f +
                    MathF.Sin(
                        time * 0.8f +
                        strip) *
                    0.18f;

                var side =
                    new Vector3(
                        MathF.Cos(angle),
                        0f,
                        MathF.Sin(angle));

                var start =
                    checked(
                        (uint)output.Count);

                var top =
                    origin +
                    side * 0.24f +
                    new Vector3(
                        0f,
                        0.78f,
                        0f);
                var mid =
                    origin +
                    side *
                    (0.32f +
                     0.06f *
                     MathF.Sin(
                         time * 1.7f +
                         strip)) +
                    new Vector3(
                        0f,
                        0.30f,
                        0f);
                var tail =
                    origin +
                    side *
                    (0.20f +
                     ritual * 0.22f) +
                    new Vector3(
                        0f,
                        -0.16f -
                        ritual * 0.28f,
                        0f);

                output.Add(
                    new TerrainVertex(
                        top,
                        color *
                            visibility,
                        Vector3.UnitY));
                output.Add(
                    new TerrainVertex(
                        mid +
                        Vector3.Cross(
                            side,
                            Vector3.UnitY) *
                        0.08f,
                        color *
                            0.78f *
                            visibility,
                        Vector3.UnitY));
                output.Add(
                    new TerrainVertex(
                        tail,
                        color *
                            0.42f *
                            visibility,
                        Vector3.UnitY));

                triangles.AddRange(
                    [start,
                     start + 2,
                     start + 1]);
            }
        }

        void AddDiamond(
            Vector3 position,
            float size,
            Vector3 color)
        {
            var start =
                checked(
                    (uint)output.Count);

            var offsets =
                new[]
                {
                    Vector3.UnitY,
                    -Vector3.UnitY,
                    Vector3.UnitX,
                    Vector3.UnitZ,
                    -Vector3.UnitX,
                    -Vector3.UnitZ
                };

            foreach (var offset
                     in offsets)
            {
                output.Add(
                    new TerrainVertex(
                        position +
                        offset * size,
                        color,
                        offset));
            }

            for (uint i = 0;
                 i < 4;
                 i++)
            {
                var a =
                    start + 2 + i;
                var b =
                    start +
                    2 +
                    (i + 1) % 4;

                triangles.AddRange(
                    [start,
                     a,
                     b,
                     start + 1,
                     b,
                     a]);
            }
        }
    }
}
