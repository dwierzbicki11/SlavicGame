using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowProjectionRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var direction in new[] { new Vector3(1, 2, 3), Vector3.UnitY })
        foreach (var size in new uint[] { 512, 2048, 4096 })
        foreach (var span in new[] { 80f, 420f })
        {
            var sun = Vector3.Normalize(direction);
            var up = MathF.Abs(sun.Y) > 0.96f ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, sun));
            var lightUp = Vector3.Cross(sun, right);
            var texel = span / size;
            var receiver = new Vector3(13, 7, -19);
            Vector3 Project(Vector3 focus) => Vector3.Transform(receiver,
                ShadowMapRenderer.CalculateLightViewProjection(focus, sun, size, span));
            var initial = Project(Vector3.Zero);
            foreach (var axis in new[] { right, lightUp, Vector3.UnitY })
            {
                var moved = Project(axis * (texel * 0.2f));
                check(Vector2.Distance(new(initial.X, initial.Y), new(moved.X, moved.Y)) < 1e-6f,
                    "Sub-texel focus movement preserves shadow UV, including height and overhead sun");
            }

            var crossed = Project(right * (texel * 0.75f));
            check(MathF.Abs((crossed.X - initial.X) * size * 0.5f + 1f) < 0.001f &&
                  MathF.Abs(crossed.Y - initial.Y) < 1e-6f,
                "Crossing a light-space grid boundary moves the shadow by exactly one texel");
            var center = Vector3.Transform(Vector3.Zero,
                ShadowMapRenderer.CalculateLightViewProjection(Vector3.Zero, sun, size, span));
            check(center.Z > 0f && center.Z < 1f,
                "Shadow focus remains within the Vulkan zero-to-one depth range");
        }
    }
}
