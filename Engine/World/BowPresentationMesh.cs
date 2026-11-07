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

        if (!world.Bow.IsAiming &&
            world.Bow.Projectiles.Count == 0 &&
            world.Bow.RecoverableArrows.Count == 0)
        {
            return;
        }

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        Append(
            world,
            bowModel,
            arrowModel,
            cameraPosition,
            lookDirection,
            cameraRight,
            firstPerson,
            output,
            triangles);
        vertices = output.ToArray();
        indices = triangles.ToArray();
    }

    public static void Append(
        WorldState world,
        GlbModel bowModel,
        GlbModel arrowModel,
        Vector3 cameraPosition,
        Vector3 lookDirection,
        Vector3 cameraRight,
        bool firstPerson,
        List<TerrainVertex> output,
        List<uint> triangles)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(bowModel);
        ArgumentNullException.ThrowIfNull(arrowModel);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(triangles);

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
                  Vector3.UnitY * 1.25f +
                  right * 0.28f +
                  forward * 0.38f;

            var transform =
                Matrix4x4.CreateScale(0.92f) *
                Matrix4x4.CreateWorld(
                    position,
                    forward,
                    Vector3.UnitY);

            bowModel.AppendTerrainMesh(
                transform,
                new Vector3(0.38f, 0.24f, 0.11f),
                output,
                triangles,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: true);

            if (world.Bow.IsDrawing)
            {
                var drawFraction = world.Bow.DrawFraction;
                var drawOffset = drawFraction * 0.32f;
                var nocked = position +
                    forward * (0.24f - drawOffset) -
                    right * 0.04f +
                    Vector3.UnitY * 0.02f;

                var isIron = world.Progress.Inventory.Contains(BowCombatRuntime.IronArrowItemId);
                var arrowColor = isIron
                    ? new Vector3(0.58f, 0.62f, 0.68f)
                    : new Vector3(0.46f, 0.31f, 0.14f);

                AppendArrow(
                    nocked,
                    forward,
                    arrowColor);
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
                    MathF.Abs(Vector3.Dot(direction, Vector3.UnitY)) > 0.99f
                        ? Vector3.UnitX : Vector3.UnitY);

            arrowModel.AppendTerrainMesh(
                transform,
                color,
                output,
                triangles,
                animationName: null,
                animationTimeSeconds: 0f,
                sourceIsZUp: true);
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
