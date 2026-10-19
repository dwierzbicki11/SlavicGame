using System.Numerics;
using SlavicGame.Engine.Renderer;

namespace SlavicGame.Engine.UI;

public static class SunShadowProjectionRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var size in new uint[] { 512, 2048, 4096 })
        foreach (var span in new float[] { 80f, 420f })
        foreach (var direction in new[] {
            new Vector3(0.4f, 0.7f, -0.5f), Vector3.UnitY,
            new Vector3(-0.8f, 0.1f, 0.3f) })
        {
            var sun = Vector3.Normalize(direction);
            var up = MathF.Abs(sun.Y) > 0.96f ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var lightUp = Vector3.Cross(sun, right);
            var texel = span / size;
            var origin = SunShadowProjection.Calculate(Vector3.Zero, sun, size, span);
            var receiver = new Vector3(17f, 3f, -21f);
            var initial = Vector3.Transform(receiver, origin);

            // Test both image axes, world-height motion, and light-depth motion.
            foreach (var axis in new[] { right, lightUp, Vector3.UnitY, sun })
            {
                var moved = SunShadowProjection.Calculate(axis * (texel * 0.2f), sun, size, span);
                var projected = Vector3.Transform(receiver, moved);
                check(MathF.Abs(projected.X - initial.X) < 1e-6f &&
                      MathF.Abs(projected.Y - initial.Y) < 1e-6f,
                    $"Shadow texels stay fixed for subtexel motion ({size}, {span}, {axis})");
            }

            foreach (var axis in new[] { right, lightUp })
            {
                var moved = SunShadowProjection.Calculate(axis * (texel * 0.8f), sun, size, span);
                var projected = Vector3.Transform(receiver, moved);
                var delta = new Vector2(projected.X - initial.X, projected.Y - initial.Y);
                check(MathF.Abs(delta.Length() - 2f / size) < 1e-6f,
                    $"Crossing shadow grid boundary moves exactly one texel ({size}, {span})");
            }

            var focus = new Vector3(150f, 12f, -90f);
            var centered = Vector3.Transform(focus,
                SunShadowProjection.Calculate(focus, sun, size, span));
            check(MathF.Abs(centered.X) <= 1f / size + 1e-6f &&
                  MathF.Abs(centered.Y) <= 1f / size + 1e-6f &&
                  centered.Z > 0f && centered.Z < 1f,
                "Shadow focus stays within half a texel of center and Vulkan depth range");
        }
    }
}
