using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowStabilityRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var sun in new[] { Vector3.Normalize(new Vector3(1, 2, 3)), Vector3.UnitY })
        foreach (var size in new uint[] { 512, 2048, 4096 })
        {
            const float span = 420f;
            var texel = span / size;
            var up = MathF.Abs(Vector3.Dot(sun, Vector3.UnitY)) > 0.96f
                ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var lightUp = Vector3.Cross(sun, right);
            var reference = ShadowMapRenderer.CalculateLightViewProjection(Vector3.Zero, sun, size, span);
            var point = new Vector3(17, 31, -9);
            var initial = Vector3.Transform(point, reference);

            foreach (var axis in new[] { right, lightUp, Vector3.UnitY })
            {
                var small = ShadowMapRenderer.CalculateLightViewProjection(axis * texel * 0.2f, sun, size, span);
                var projected = Vector3.Transform(point, small);
                check(Vector2.Distance(new(initial.X, initial.Y), new(projected.X, projected.Y)) < 1e-6f,
                    $"Shadow XY stable for sub-texel movement: {sun}, {size}, {axis}");
            }

            foreach (var axis in new[] { right, lightUp })
            {
                var moved = ShadowMapRenderer.CalculateLightViewProjection(axis * texel * 1.2f, sun, size, span);
                var projected = Vector3.Transform(point, moved);
                var delta = new Vector2(projected.X - initial.X, projected.Y - initial.Y) * (size * 0.5f);
                check(MathF.Abs(delta.Length() - 1f) < 0.001f,
                    $"Shadow movement crosses exactly one texel: {sun}, {size}, {axis}");
            }
            check(initial.Z > 0f && initial.Z < 1f, "Shadow depth remains within Vulkan clip range");
        }
    }
}
