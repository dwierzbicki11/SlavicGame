using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Combat;

namespace SlavicGame.Engine.World;

public static class BowPresentationMesh
{
    public static void Append(
        WorldState world,
        GlbModel bowModel,
        GlbModel arrowModel,
        Vector3 cameraPosition,
        Vector3 lookDirection,
        Vector3 cameraRight,
        bool firstPerson,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(bowModel);
        ArgumentNullException.ThrowIfNull(arrowModel);

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);

        if (world.Bow.IsAiming)
        {
            var forward = SafeDirection(lookDirection);
            var right = SafeDirection(cameraRight);
            var position = firstPerson
                ? cameraPosition +
                  forward * 0.62f +
                  right * 0.34f -
                  Vector3.UnitY * 0.27f
                : world.PlayerPosition +
                  Vector3.UnitY * 1.10f +
                  right * 0.30f;

            var transform =
                Matrix4x4.CreateScale(0.92f) *
                Matrix4x4.CreateWorld(
                    position,
                    forward,
                    Vector3.UnitY);

            AppendGeometry(
                bowModel.BuildMesh(
                    transform,
                    animationName: null,
                    animationTimeSeconds: 0f,
                    sourceIsZUp: true),
                new Vector3(0.38f, 0.24f, 0.11f));

            if (world.Bow.IsDrawing)
            {
                var nocked =
                    position +
                    forward * 0.22f -
                    right * 0.05f +
                    Vector3.UnitY * 0.03f;

                AppendArrow(
                    nocked,
                    forward,
                    new Vector3(0.46f, 0.31f, 0.14f));
            }
        }

        foreach (var projectile in world.Bow.Projectiles)
        {
            AppendArrow(
                projectile.Position,
                projectile.Velocity,
                new Vector3(0.52f, 0.35f, 0.16f));
        }

        foreach (var arrow in world.Bow.RecoverableArrows)
        {
            var horizontal =
                WorldPlacementOrientation.ForwardFromYaw(
                    arrow.YawRadians);
            var cosPitch =
                MathF.Cos(arrow.PitchRadians);
            var direction = new Vector3(
                horizontal.X * cosPitch,
                MathF.Sin(arrow.PitchRadians),
                horizontal.Y * cosPitch);

            AppendArrow(
                arrow.Position,
                direction,
                new Vector3(0.43f, 0.30f, 0.15f));
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();

        void AppendArrow(
            Vector3 position,
            Vector3 direction,
            Vector3 color)
        {
            direction = SafeDirection(direction);
            var transform =
                Matrix4x4.CreateScale(0.96f) *
                Matrix4x4.CreateWorld(
                    position,
                    direction,
                    Vector3.UnitY);

            AppendGeometry(
                arrowModel.BuildMesh(
                    transform,
                    animationName: null,
                    animationTimeSeconds: 0f,
                    sourceIsZUp: true),
                color);
        }

        void AppendGeometry(
            MeshGeometry geometry,
            Vector3 color)
        {
            var start =
                checked((uint)output.Count);

            foreach (var position in geometry.Positions)
            {
                output.Add(
                    new TerrainVertex(
                        position,
                        color));
            }

            foreach (var index in geometry.Indices)
                triangles.Add(start + index);
        }
    }

    private static Vector3 SafeDirection(Vector3 direction)
    {
        if (!float.IsFinite(direction.X) ||
            !float.IsFinite(direction.Y) ||
            !float.IsFinite(direction.Z) ||
            direction.LengthSquared() < 0.0001f)
        {
            return -Vector3.UnitZ;
        }

        return Vector3.Normalize(direction);
    }
}
