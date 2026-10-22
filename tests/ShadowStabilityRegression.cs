using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowStabilityRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var direction in new[] { new Vector3(0.6f, 0.7f, 0.3f), Vector3.UnitY })
        foreach (var size in new uint[] { 512, 2048, 4096 })
        foreach (var span in new[] { 80f, 420f })
        {
            var sun = Vector3.Normalize(direction);
            var up = MathF.Abs(Vector3.Dot(sun, Vector3.UnitY)) > 0.96f
                ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var lightUp = Vector3.Cross(sun, right);
            var texel = span / size;
            var point = new Vector3(12, 8, -17);
            Vector3 Project(Vector3 focus) => Vector3.Transform(point,
                ShadowMapRenderer.CalculateLightViewProjection(focus, sun, size, span));
            var baseline = Project(Vector3.Zero);
            foreach (var axis in new[] { right, lightUp, Vector3.UnitY })
            {
                var small = Project(axis * texel * 0.2f);
                check(Vector2.Distance(new(baseline.X, baseline.Y), new(small.X, small.Y)) < 1e-6f,
                    $"Shadow grid stable below one texel: {sun}, {size}, {span}, {axis}");
            }
            foreach (var axis in new[] { right, lightUp })
            {
                var moved = Project(axis * texel * 1.2f);
                var pixelShift = new Vector2(moved.X - baseline.X, moved.Y - baseline.Y) * (size / 2f);
                check(MathF.Abs(pixelShift.Length() - 1f) < 0.002f,
                    $"Shadow grid moves by one whole texel: {sun}, {size}, {span}, {axis}");
            }
        }
    }
}
