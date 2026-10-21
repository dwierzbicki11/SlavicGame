using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowProjectionRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var direction in new[] { new Vector3(0.4f, 0.7f, 0.6f), Vector3.UnitY })
        foreach (var mapSize in new uint[] { 512, 2048, 4096 })
        {
            var sun = Vector3.Normalize(direction);
            var up = MathF.Abs(sun.Y) > 0.96f ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var lightUp = Vector3.Cross(sun, right);
            const float span = 420f;
            var texel = span / mapSize;
            var origin = ShadowProjection.Calculate(Vector3.Zero, sun, mapSize, span);
            var receiver = new Vector3(12f, 3f, -7f);
            var baseline = Vector3.Transform(receiver, origin);

            foreach (var axis in new[] { right, lightUp, Vector3.UnitY })
            {
                var moved = ShadowProjection.Calculate(axis * (texel * 0.2f), sun, mapSize, span);
                var projected = Vector3.Transform(receiver, moved);
                check(Vector2.Distance(new(baseline.X, baseline.Y), new(projected.X, projected.Y)) < 1e-6f,
                    $"Subtexel motion preserves shadow UVs ({sun}, {mapSize}, {axis})");
            }

            foreach (var sign in new[] { -1f, 1f })
            {
                var moved = ShadowProjection.Calculate(right * (texel * 1.2f * sign), sun, mapSize, span);
                var projected = Vector3.Transform(receiver, moved);
                check(MathF.Abs((projected.X - baseline.X) * mapSize * 0.5f + sign) < 0.001f,
                    "Shadow translation advances by exactly one texel in either direction");
                check(MathF.Abs(projected.Y - baseline.Y) < 1e-6f,
                    "Horizontal shadow translation preserves vertical UV");
            }

            var centered = Vector3.Transform(Vector3.Zero, origin);
            check(centered.Z > 0f && centered.Z < 1f,
                "Shadow focus remains inside Vulkan depth range");
        }
    }
}
