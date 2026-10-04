using System.Numerics;
using SlavicGame.Engine.Combat;

namespace SlavicGame.Engine.World;

public static class BowProjectileMesh
{
    public static void Append(
        WorldState world,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);

        var output =
            new List<TerrainVertex>(vertices);
        var triangles =
            new List<uint>(indices);

        foreach (var projectile in world.Bow.Projectiles)
        {
            var direction =
                projectile.Velocity.LengthSquared() > 0.000001f
                    ? Vector3.Normalize(projectile.Velocity)
                    : Vector3.UnitZ;

            AddArrow(
                projectile.Position,
                direction,
                new Vector3(0.42f, 0.27f, 0.11f));
        }

        foreach (var arrow in world.Bow.RecoverableArrows)
        {
            AddArrow(
                arrow.Position + Vector3.UnitY * 0.04f,
                Vector3.Normalize(new Vector3(0.18f, -0.58f, 0.79f)),
                new Vector3(0.36f, 0.23f, 0.10f));
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();

        void AddArrow(
            Vector3 center,
            Vector3 forward,
            Vector3 color)
        {
            forward =
                forward.LengthSquared() > 0.000001f
                    ? Vector3.Normalize(forward)
                    : Vector3.UnitZ;

            var reference =
                MathF.Abs(Vector3.Dot(
                    forward,
                    Vector3.UnitY)) > 0.92f
                    ? Vector3.UnitX
                    : Vector3.UnitY;

            var side =
                Vector3.Normalize(
                    Vector3.Cross(
                        forward,
                        reference));
            var up =
                Vector3.Normalize(
                    Vector3.Cross(
                        side,
                        forward));

            const float halfLength = 0.42f;
            const float radius = 0.025f;

            var tip =
                center + forward * halfLength;
            var tail =
                center - forward * halfLength;
            var mid =
                center;

            var start =
                checked((uint)output.Count);

            output.Add(new TerrainVertex(
                tip,
                new Vector3(0.58f, 0.60f, 0.57f),
                forward));
            output.Add(new TerrainVertex(
                tail,
                color,
                -forward));
            output.Add(new TerrainVertex(
                mid + side * radius,
                color,
                side));
            output.Add(new TerrainVertex(
                mid - side * radius,
                color,
                -side));
            output.Add(new TerrainVertex(
                mid + up * radius,
                color,
                up));
            output.Add(new TerrainVertex(
                mid - up * radius,
                color,
                -up));

            triangles.AddRange(
            [
                start, start + 2, start + 4,
                start, start + 4, start + 3,
                start, start + 3, start + 5,
                start, start + 5, start + 2,

                start + 1, start + 4, start + 2,
                start + 1, start + 3, start + 4,
                start + 1, start + 5, start + 3,
                start + 1, start + 2, start + 5
            ]);
        }
    }
}
