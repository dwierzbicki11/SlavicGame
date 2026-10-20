using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowStabilityRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var direction in new[] { new Vector3(0.4f, 0.7f, 0.6f), Vector3.UnitY })
        foreach (var size in new uint[] { 512, 2048, 4096 })
        foreach (var span in new[] { 80f, 420f })
        {
            var sun = Vector3.Normalize(direction);
            var up = MathF.Abs(Vector3.Dot(sun, Vector3.UnitY)) > 0.96f
                ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var lightUp = Vector3.Cross(sun, right);
            var texel = span / size;
            var baseline = ShadowMapRenderer.CalculateLightViewProjection(Vector3.Zero, sun, size, span);
            var point = new Vector3(3, 7, -11);
            var expected = Vector3.Transform(point, baseline);
            foreach (var axis in new[] { right, lightUp, Vector3.UnitY })
            {
                var moved = ShadowMapRenderer.CalculateLightViewProjection(axis * texel * 0.2f, sun, size, span);
                var actual = Vector3.Transform(point, moved);
                check(Vector2.Distance(new(expected.X, expected.Y), new(actual.X, actual.Y)) < 1e-6f,
                    "Subtexel camera motion preserves shadow UV, including camera height");
            }
            foreach (var axis in new[] { right, lightUp })
            {
                var moved = ShadowMapRenderer.CalculateLightViewProjection(axis * texel, sun, size, span);
                var actual = Vector3.Transform(point, moved);
                var delta = new Vector2(actual.X - expected.X, actual.Y - expected.Y);
                check(MathF.Abs(delta.Length() - 2f / size) < 1e-6f,
                    "One light-space texel moves the shadow projection by exactly one texel");
            }
            check(expected.Z > 0 && expected.Z < 1,
                "Shadow projection retains Vulkan zero-to-one scene depth");
        }
    }
}
