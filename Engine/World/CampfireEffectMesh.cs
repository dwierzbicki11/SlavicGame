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

        foreach (var fire in CampfireSystem.Fires)
        {
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
            var sway =
                MathF.Sin(
                    seconds * 4.9f +
                    fire.Position.Y * 0.09f) *
                0.10f *
                fire.VisualScale;

            AddGroundGlow(
                center,
                0.72f * fire.VisualScale,
                new Vector3(1.65f, 0.46f, 0.05f));

            AddFlame(
                center + new Vector3(sway, 0f, 0f),
                0.34f * fire.VisualScale * (0.92f + pulse * 0.16f),
                0.90f * fire.VisualScale * (0.90f + pulse * 0.20f),
                new Vector3(3.2f, 0.78f, 0.08f));

            AddFlame(
                center + new Vector3(-sway * 0.55f, 0.05f, sway * 0.35f),
                0.20f * fire.VisualScale,
                0.58f * fire.VisualScale * (0.92f + (1f - pulse) * 0.18f),
                new Vector3(3.8f, 1.52f, 0.16f));
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();

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

            triangles.AddRange(
                [start, start + 2, start + 1,
                 start, start + 3, start + 2]);
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
                triangles.AddRange([start, a, b]);
            }
        }
    }
}
