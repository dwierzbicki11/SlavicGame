using System.Numerics;

namespace SlavicGame.Engine.World;

public static class WildlifeTrackEffectMesh
{
    private const int MaxVisibleMarks = 96;
    private const float MaxRenderDistance = 38f;

    public static void Append(
        WorldState world,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        var player2 = new Vector2(
            world.PlayerPosition.X,
            world.PlayerPosition.Z);

        foreach (var mark in world.WildlifeTracks.Marks
                     .OrderBy(mark => Vector2.DistanceSquared(
                         new Vector2(mark.Position.X, mark.Position.Z),
                         player2))
                     .Take(MaxVisibleMarks))
        {
            var mark2 = new Vector2(mark.Position.X, mark.Position.Z);
            if (Vector2.DistanceSquared(mark2, player2) >
                MaxRenderDistance * MaxRenderDistance)
            {
                continue;
            }

            var fade = mark.Freshness switch
            {
                TrackFreshness.Fresh => 1.00f,
                TrackFreshness.Recent => 0.78f,
                TrackFreshness.Old => 0.53f,
                TrackFreshness.Faded => 0.30f,
                _ => 0.30f
            };

            var baseColor = mark.Species switch
            {
                WildlifeSpecies.Deer =>
                    new Vector3(0.17f, 0.115f, 0.065f),
                WildlifeSpecies.Boar =>
                    new Vector3(0.13f, 0.085f, 0.050f),
                WildlifeSpecies.Wolf =>
                    new Vector3(0.11f, 0.095f, 0.075f),
                _ =>
                    new Vector3(0.14f, 0.10f, 0.07f)
            };

            var color =
                baseColor * (0.52f + fade * 0.48f);
            var forward =
                mark.Forward.LengthSquared() > 0.000001f
                    ? Vector2.Normalize(mark.Forward)
                    : Vector2.UnitY;
            var side =
                new Vector2(-forward.Y, forward.X);

            if (mark.Species == WildlifeSpecies.Wolf)
            {
                AddPaw(
                    mark.Position,
                    forward,
                    side,
                    0.21f,
                    0.16f,
                    color);
            }
            else
            {
                AddHoofPair(
                    mark.Position,
                    forward,
                    side,
                    mark.Species == WildlifeSpecies.Boar
                        ? 0.24f
                        : 0.20f,
                    mark.Species == WildlifeSpecies.Boar
                        ? 0.11f
                        : 0.09f,
                    color);
            }
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();

        void AddHoofPair(
            Vector3 center,
            Vector2 forward,
            Vector2 side,
            float length,
            float separation,
            Vector3 color)
        {
            AddImprint(
                center,
                forward,
                side,
                length,
                0.055f,
                -separation,
                color);
            AddImprint(
                center,
                forward,
                side,
                length,
                0.055f,
                separation,
                color);
        }

        void AddPaw(
            Vector3 center,
            Vector2 forward,
            Vector2 side,
            float length,
            float width,
            Vector3 color)
        {
            AddImprint(
                center,
                forward,
                side,
                length,
                width,
                0f,
                color);

            var toeForward =
                new Vector3(
                    forward.X * length * 0.55f,
                    0f,
                    forward.Y * length * 0.55f);

            for (var toe = -1; toe <= 1; toe++)
            {
                var toeCenter =
                    center +
                    toeForward +
                    new Vector3(
                        side.X * toe * width * 0.43f,
                        0f,
                        side.Y * toe * width * 0.43f);

                AddImprint(
                    toeCenter,
                    forward,
                    side,
                    length * 0.28f,
                    width * 0.20f,
                    0f,
                    color * 0.94f);
            }
        }

        void AddImprint(
            Vector3 center,
            Vector2 forward,
            Vector2 side,
            float length,
            float halfWidth,
            float lateralOffset,
            Vector3 color)
        {
            var lateral =
                side * lateralOffset;
            var c =
                new Vector3(
                    center.X + lateral.X,
                    center.Y,
                    center.Z + lateral.Y);
            var f =
                new Vector3(
                    forward.X * length * 0.5f,
                    0f,
                    forward.Y * length * 0.5f);
            var s =
                new Vector3(
                    side.X * halfWidth,
                    0f,
                    side.Y * halfWidth);

            var start =
                checked((uint)output.Count);

            output.Add(
                new TerrainVertex(
                    c - f - s,
                    color,
                    Vector3.UnitY));
            output.Add(
                new TerrainVertex(
                    c - f + s,
                    color,
                    Vector3.UnitY));
            output.Add(
                new TerrainVertex(
                    c + f + s,
                    color,
                    Vector3.UnitY));
            output.Add(
                new TerrainVertex(
                    c + f - s,
                    color,
                    Vector3.UnitY));

            triangles.AddRange(
                [start, start + 2, start + 1,
                 start, start + 3, start + 2]);
        }
    }
}
