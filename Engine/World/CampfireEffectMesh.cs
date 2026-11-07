using System.Numerics;

namespace SlavicGame.Engine.World;

public static class CampfireEffectMesh
{
    public static void Append(
        WorldState world,
        float seconds,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        Append(world, seconds, output, triangles);
        vertices = output.ToArray();
        indices = triangles.ToArray();
    }

    public static void Append(
        WorldState world,
        float seconds,
        List<TerrainVertex> output,
        List<uint> triangles)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(triangles);

        foreach (var fire in CampfireSystem.Fires)
        {
            if (!world.Campfires.IsLit(world, fire.Id))
                continue;

            var center = new Vector3(
                fire.Position.X,
                0f,
                fire.Position.Y);
            center.Y = world.Terrain.SampleHeight(center) + 0.10f;

            var pulse =
                0.5f +
                0.5f * MathF.Sin(
                    seconds * 7.4f +
                    fire.Position.X * 0.11f +
                    fire.Position.Y * 0.07f);
            var swayX =
                MathF.Sin(
                    seconds * 5.2f +
                    fire.Position.Y * 0.09f) *
                0.09f *
                fire.VisualScale;
            var swayZ =
                MathF.Cos(
                    seconds * 4.4f +
                    fire.Position.X * 0.13f) *
                0.07f *
                fire.VisualScale;

            AddGroundGlow(
                center,
                0.78f * fire.VisualScale * (0.94f + pulse * 0.12f),
                new Vector3(1.85f, 0.44f, 0.035f) * (0.86f + pulse * 0.28f));

            AddFlame(
                center + new Vector3(swayX, 0f, swayZ),
                0.34f * fire.VisualScale * (0.92f + pulse * 0.16f),
                0.92f * fire.VisualScale * (0.90f + pulse * 0.20f),
                new Vector3(3.6f, 1.15f, 0.12f));

            AddFlame(
                center + new Vector3(-swayX * 0.60f, 0.05f, -swayZ * 0.60f),
                0.22f * fire.VisualScale,
                0.62f * fire.VisualScale * (0.92f + (1f - pulse) * 0.18f),
                new Vector3(4.2f, 2.05f, 0.24f));
        }

        void AddGroundGlow(
            Vector3 center,
            float radius,
            Vector3 color)
        {
            var start = checked((uint)output.Count);
            output.Add(new TerrainVertex(
                center + new Vector3(-radius, 0.01f, -radius),
                color,
                Vector3.UnitY));
            output.Add(new TerrainVertex(
                center + new Vector3(radius, 0.01f, -radius),
                color,
                Vector3.UnitY));
            output.Add(new TerrainVertex(
                center + new Vector3(radius, 0.01f, radius),
                color,
                Vector3.UnitY));
            output.Add(new TerrainVertex(
                center + new Vector3(-radius, 0.01f, radius),
                color,
                Vector3.UnitY));

            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 1);
            triangles.Add(start);
            triangles.Add(start + 3);
            triangles.Add(start + 2);
        }

        void AddFlame(
            Vector3 center,
            float radius,
            float height,
            Vector3 color)
        {
            var start = checked((uint)output.Count);
            var tip = center + new Vector3(0f, height, 0f);

            output.Add(new TerrainVertex(
                tip,
                color,
                Vector3.UnitY));

            for (var i = 0; i < 4; i++)
            {
                var angle = i * MathF.PI * 0.5f;
                var direction =
                    new Vector3(
                        MathF.Cos(angle),
                        0f,
                        MathF.Sin(angle));

                output.Add(new TerrainVertex(
                    center + direction * radius,
                    color * (0.72f + i * 0.05f),
                    Vector3.Normalize(
                        new Vector3(
                            direction.X,
                            0.65f,
                            direction.Z))));
            }

            for (uint i = 0; i < 4; i++)
            {
                var a = start + 1 + i;
                var b = start + 1 + (i + 1) % 4;
                triangles.Add(start);
                triangles.Add(a);
                triangles.Add(b);
            }
        }
    }
}
