using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowProjectionRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var direction in new[] { new Vector3(1, 2, 3), Vector3.UnitY })
        {
            var sun = Vector3.Normalize(direction);
            var up = MathF.Abs(Vector3.Dot(sun, Vector3.UnitY)) > 0.96f
                ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var lightUp = Vector3.Cross(sun, right);
            const float span = 128f;
            const uint size = 1024;
            const float texel = span / size;
            var origin = ShadowProjection.Create(Vector3.Zero, sun, size, span);
            var point = new Vector3(12, 7, -3);
            var baseline = Vector3.Transform(point, origin);
            foreach (var axis in new[] { right, lightUp })
            {
                var smallMove = ShadowProjection.Create(axis * (texel * 0.2f), sun, size, span);
                check(Vector2.Distance(
                    new Vector2(baseline.X, baseline.Y),
                    new Vector2(Vector3.Transform(point, smallMove).X,
                        Vector3.Transform(point, smallMove).Y)) < 0.000001f,
                    "Shadow XY stays stable for subtexel motion in light space");
                var step = ShadowProjection.Create(axis * texel, sun, size, span);
                var moved = Vector3.Transform(point, step);
                var delta = new Vector2(moved.X - baseline.X, moved.Y - baseline.Y);
                check(MathF.Abs(delta.Length() - 2f / size) < 0.000001f,
                    "Shadow projection advances by exactly one texel");
            }
            var depthMove = ShadowProjection.Create(sun * 2f, sun, size, span);
            var depthPoint = Vector3.Transform(point, depthMove);
            check(MathF.Abs(depthPoint.X - baseline.X) < 0.000001f &&
                  MathF.Abs(depthPoint.Y - baseline.Y) < 0.000001f,
                "Motion along sunlight does not move shadow UVs");
            check(float.IsFinite(origin.M11) && float.IsFinite(origin.M33),
                "Shadow projection remains finite for overhead sunlight");
        }
    }
}
