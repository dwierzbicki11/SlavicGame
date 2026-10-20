using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowProjectionRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var sun in new[] { Vector3.Normalize(new Vector3(1, 2, 3)), Vector3.UnitY })
        foreach (var size in new uint[] { 512, 2048, 4096 })
        foreach (var span in new[] { 80f, 420f })
        {
            var texel = span / size;
            var origin = ShadowMapRenderer.CalculateLightViewProjection(Vector3.Zero, sun, size, span);
            // Exercise both horizontal movement and terrain-height changes.
            foreach (var axis in new[] { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ })
            {
                var small = ShadowMapRenderer.CalculateLightViewProjection(axis * (texel * 0.2f), sun, size, span);
                check(MathF.Abs(small.M41 - origin.M41) < 1e-6f &&
                      MathF.Abs(small.M42 - origin.M42) < 1e-6f,
                    "Sub-texel camera movement preserves shadow image-plane translation");

                var moved = ShadowMapRenderer.CalculateLightViewProjection(axis * 7.13f, sun, size, span);
                var deltaX = (moved.M41 - origin.M41) * size * 0.5f;
                var deltaY = (moved.M42 - origin.M42) * size * 0.5f;
                check(MathF.Abs(deltaX - MathF.Round(deltaX)) < 0.001f &&
                      MathF.Abs(deltaY - MathF.Round(deltaY)) < 0.001f,
                    "Camera movement translates the shadow projection by whole texels");
                var focusClip = Vector3.Transform(axis * 7.13f, moved);
                check(MathF.Abs(focusClip.X) <= 1.01f / size &&
                      MathF.Abs(focusClip.Y) <= 1.01f / size &&
                      focusClip.Z > 0f && focusClip.Z < 1f,
                    "Snapped shadow focus remains centered within half a texel and inside depth range");
            }
        }
    }
}
