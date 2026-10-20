using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowMapRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var size in new uint[] { 512, 2048, 4096 })
        foreach (var span in new float[] { 80, 420 })
        foreach (var sun in new[] {
            Vector3.Normalize(new Vector3(1, 2, 3)),
            Vector3.Normalize(new Vector3(-3, 1, 2)),
            Vector3.UnitY })
        {
            var texel = span / size;
            var baseline = ShadowMapRenderer.CalculateLightViewProjection(
                Vector3.Zero, sun, size, span);
            var point = new Vector3(17, 4, -23);
            var initial = Vector3.Transform(point, baseline);
            foreach (var axis in new[] { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ })
            {
                var moved = ShadowMapRenderer.CalculateLightViewProjection(
                    axis * (texel * 0.2f), sun, size, span);
                var projected = Vector3.Transform(point, moved);
                check(float.IsFinite(projected.Z) &&
                    MathF.Abs(initial.X - projected.X) < 1e-7f &&
                    MathF.Abs(initial.Y - projected.Y) < 1e-7f,
                    $"Shadow texels remain fixed for sub-texel {axis} motion ({size}, {span}, {sun})");
            }

            var up = MathF.Abs(Vector3.Dot(sun, Vector3.UnitY)) > 0.96f
                ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var next = ShadowMapRenderer.CalculateLightViewProjection(
                right * (texel * 1.2f), sun, size, span);
            var stepped = Vector3.Transform(point, next);
            check(MathF.Abs((stepped.X - initial.X) * size * 0.5f + 1f) < 0.001f &&
                MathF.Abs(stepped.Y - initial.Y) < 1e-7f,
                "Shadow projection advances by exactly one texel across a grid boundary");

            var depthMove = ShadowMapRenderer.CalculateLightViewProjection(
                sun * 10f, sun, size, span);
            var depthProjected = Vector3.Transform(point, depthMove);
            check(MathF.Abs(depthProjected.X - initial.X) < 1e-7f &&
                MathF.Abs(depthProjected.Y - initial.Y) < 1e-7f &&
                MathF.Abs(depthProjected.Z - initial.Z) > 0.001f,
                "Movement along the light preserves lateral texels and updates depth");
        }
    }
}
