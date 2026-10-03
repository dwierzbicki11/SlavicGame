using System.Numerics;

namespace SlavicGame.Engine.World;

public static class FootprintEffectMesh
{
    public static void Append(
        WorldState world,
        ref TerrainVertex[] vertices,
        ref uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Footprints.Footprints.Count == 0)
            return;

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);

        foreach (var footprint in world.Footprints.Footprints)
        {
            var life =
                1f -
                Math.Clamp(
                    footprint.AgeSeconds /
                    MathF.Max(footprint.LifetimeSeconds, 0.001f),
                    0f,
                    1f);

            if (life <= 0.02f)
                continue;

            var forward = footprint.Forward;
            if (forward.LengthSquared() < 0.0001f)
                forward = new Vector2(0f, 1f);
            else
                forward = Vector2.Normalize(forward);

            var side = new Vector2(-forward.Y, forward.X);
            var length = 0.30f;
            var width = 0.095f;
            var center = footprint.Position;

            var forward3 =
                new Vector3(forward.X, 0f, forward.Y);
            var side3 =
                new Vector3(side.X, 0f, side.Y);

            var heel =
                center -
                forward3 * length * 0.34f;
            var toe =
                center +
                forward3 * length * 0.42f;

            var visibility =
                Math.Clamp(
                    footprint.Strength * life,
                    0f,
                    1f);

            var faded =
                new Vector3(0.20f, 0.14f, 0.09f);
            var fresh =
                new Vector3(0.055f, 0.025f, 0.012f);
            var color =
                Vector3.Lerp(
                    faded,
                    fresh,
                    visibility);

            var start = checked((uint)output.Count);

            output.Add(new TerrainVertex(
                heel - side3 * width,
                color,
                Vector3.UnitY));
            output.Add(new TerrainVertex(
                heel + side3 * width,
                color,
                Vector3.UnitY));
            output.Add(new TerrainVertex(
                toe + side3 * width * 0.78f,
                color,
                Vector3.UnitY));
            output.Add(new TerrainVertex(
                toe - side3 * width * 0.78f,
                color,
                Vector3.UnitY));

            triangles.AddRange(
                [start, start + 2, start + 1,
                 start, start + 3, start + 2]);
        }

        vertices = output.ToArray();
        indices = triangles.ToArray();
    }
}
