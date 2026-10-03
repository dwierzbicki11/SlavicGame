using System.Numerics;

namespace SlavicGame.Engine.World;

public static class RiverbankPropGenerator
{
    private const int Seed = 0x52495645;
    private const float StartZ = -920f;
    private const float EndZ = 920f;
    private const float Step = 32f;

    public static IReadOnlyList<WorldModelInstance> Generate(Terrain terrain)
    {
        ArgumentNullException.ThrowIfNull(terrain);

        var random = new Random(Seed);
        var result = new List<WorldModelInstance>(128);
        var index = 0;

        for (var z = StartZ; z <= EndZ; z += Step)
        {
            var flow = WaterLandscape.FlowDirection(z);
            var yaw = MathF.Atan2(-flow.X, -flow.Y);

            for (var bank = 0; bank < 2; bank++)
            {
                // Keep the two sides slightly irregular so the channel does not
                // look like a perfectly mirrored trench.
                var side = bank == 0 ? -1f : 1f;
                var jitterZ = (random.NextSingle() - 0.5f) * 5f;
                var sampleZ = z + jitterZ;
                var centerX = WaterLandscape.CenterX(sampleZ);
                var offset =
                    WaterLandscape.SurfaceHalfWidth(sampleZ) +
                    1.25f +
                    random.NextSingle() * 2.75f;

                var x = centerX + side * offset;
                var position = new Vector3(x, 0f, sampleZ);
                position.Y = terrain.SampleHeight(position) - 0.05f;

                var scale = 0.62f + random.NextSingle() * 0.42f;
                result.Add(new WorldModelInstance(
                    $"riverbank-rock-{index++:000}",
                    "models/static/riverbank_rocky_r0_01.glb",
                    position,
                    new Vector3(scale, scale * 0.72f, scale),
                    yaw + (random.NextSingle() - 0.5f) * 0.35f,
                    Vector3.One));
            }
        }

        return result;
    }
}
