using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowLightMatrixRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var direction in new[]
                 { Vector3.Normalize(new Vector3(1, 2, 3)), Vector3.UnitY })
        foreach (uint size in new uint[] { 512, 2048, 4096 })
        foreach (float span in new[] { 80f, 420f })
        {
            var up = MathF.Abs(direction.Y) > 0.96f ? Vector3.UnitZ : Vector3.UnitY;
            var rotation = Matrix4x4.CreateLookAt(Vector3.Zero, -direction, up);
            var inverse = Matrix4x4.Transpose(rotation);
            var texel = span / size;
            var baseline = ShadowLightMatrix.Calculate(Vector3.Zero, direction, size, span);
            var probe = new Vector3(7, 11, -13);
            var initial = Vector3.Transform(probe, baseline);
            foreach (var axis in new[] { Vector3.UnitX, Vector3.UnitY })
            {
                var smallMove = Vector3.TransformNormal(axis * (texel * 0.2f), inverse);
                var moved = Vector3.Transform(probe,
                    ShadowLightMatrix.Calculate(smallMove, direction, size, span));
                check(Vector2.Distance(new(initial.X, initial.Y), new(moved.X, moved.Y)) < 1e-6f,
                    $"Shadow UV stays fixed for subtexel light-plane motion ({direction}, {size}, {span}, {axis})");

                var fullMove = Vector3.TransformNormal(axis * texel, inverse);
                moved = Vector3.Transform(probe,
                    ShadowLightMatrix.Calculate(fullMove, direction, size, span));
                var pixelShift = new Vector2(moved.X - initial.X, moved.Y - initial.Y) * (size / 2f);
                check(Vector2.Distance(pixelShift, new Vector2(-axis.X, -axis.Y)) < 0.001f,
                    "Crossing one shadow texel moves the projection by exactly one pixel");
            }

            // Motion along the light ray must affect depth without sliding UVs.
            var alongRay = Vector3.Transform(probe,
                ShadowLightMatrix.Calculate(direction * 3f, direction, size, span));
            check(Vector2.Distance(new(initial.X, initial.Y), new(alongRay.X, alongRay.Y)) < 1e-6f &&
                  MathF.Abs(alongRay.Z - initial.Z) > 0.001f,
                "Light-axis motion preserves shadow UV while updating depth");
            var focusClip = Vector3.Transform(Vector3.Zero, baseline);
            check(focusClip.Z > 0 && focusClip.Z < 1,
                "Shadow focus remains within Vulkan zero-to-one depth range");
        }
    }
}
