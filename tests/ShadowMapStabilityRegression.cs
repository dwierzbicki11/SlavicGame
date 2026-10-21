using System.Numerics;
using SlavicGame.Engine.Renderer;

internal static class ShadowMapStabilityRegression
{
    public static void Run(Action<bool, string> check)
    {
        foreach (var direction in new[]
                 {
                     Vector3.Normalize(new Vector3(1f, 2f, 3f)),
                     Vector3.Normalize(new Vector3(-3f, 1f, 2f)),
                     Vector3.UnitY
                 })
        foreach (var mapSize in new uint[] { 512, 2048, 4096 })
        foreach (var span in new[] { 80f, 420f })
        {
            var texel = span / mapSize;
            var up = MathF.Abs(direction.Y) > 0.96f ? Vector3.UnitZ : Vector3.UnitY;
            var right = Vector3.Normalize(Vector3.Cross(up, direction));
            var lightUp = Vector3.Cross(direction, right);
            var receiver = new Vector3(13f, 7f, -21f);
            Vector2 Project(Vector3 focus)
            {
                var clip = Vector4.Transform(new Vector4(receiver, 1f),
                    ShadowMapRenderer.CalculateLightViewProjection(focus, direction, mapSize, span));
                return new Vector2(clip.X, clip.Y) / clip.W;
            }

            var initial = Project(Vector3.Zero);
            foreach (var axis in new[] { right, lightUp, Vector3.UnitY })
            {
                var moved = Project(axis * (texel * 0.2f));
                check(Vector2.Distance(initial, moved) < 0.00001f,
                    $"Shadow UV stays fixed for sub-texel motion ({direction}, {mapSize}, {span}, {axis})");
            }

            var pixelStep = 2f / mapSize;
            var horizontal = Project(right * (texel * 1.2f)) - initial;
            var vertical = Project(lightUp * (texel * 1.2f)) - initial;
            check(MathF.Abs(horizontal.X + pixelStep) < 0.00001f && MathF.Abs(horizontal.Y) < 0.00001f,
                "Shadow crossing a horizontal texel boundary moves exactly one texel");
            check(MathF.Abs(vertical.Y + pixelStep) < 0.00001f && MathF.Abs(vertical.X) < 0.00001f,
                "Shadow crossing a vertical texel boundary moves exactly one texel");
            check(Vector2.Distance(Project(direction * 10f), initial) < 0.00001f,
                "Motion along the sun direction preserves shadow UV");
        }
    }
}
