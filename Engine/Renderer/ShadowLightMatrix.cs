using System.Numerics;

namespace SlavicGame.Engine.Renderer;

// CPU-only shadow projection math, shared by the renderer and regression tests.
internal static class ShadowLightMatrix
{
    public static Matrix4x4 Calculate(
        Vector3 focusPosition,
        Vector3 sunDirection,
        uint mapSize,
        float worldSpan)
    {
        if (!float.IsFinite(focusPosition.X) ||
            !float.IsFinite(focusPosition.Y) ||
            !float.IsFinite(focusPosition.Z))
        {
            throw new ArgumentOutOfRangeException(nameof(focusPosition));
        }

        if (!float.IsFinite(sunDirection.X) ||
            !float.IsFinite(sunDirection.Y) ||
            !float.IsFinite(sunDirection.Z) ||
            sunDirection.LengthSquared() < 0.000001f)
        {
            throw new ArgumentOutOfRangeException(nameof(sunDirection));
        }

        sunDirection = Vector3.Normalize(sunDirection);

        mapSize = Math.Clamp(mapSize, 512u, 4096u);
        worldSpan = Math.Clamp(worldSpan, 80f, 420f);
        var texelWorldSize = worldSpan / mapSize;
        var up = MathF.Abs(Vector3.Dot(sunDirection, Vector3.UnitY)) > 0.96f
            ? Vector3.UnitZ
            : Vector3.UnitY;

        // Texels lie in the light's XY plane, not the world's XZ plane.
        // Build the rotation at the origin so camera movement cannot change
        // its basis through floating-point cancellation in eye - target.
        var lightView = Matrix4x4.CreateLookAt(Vector3.Zero, -sunDirection, up);
        var lightFocus = Vector3.Transform(focusPosition, lightView);
        lightView.M41 = -MathF.Round(lightFocus.X / texelWorldSize) * texelWorldSize;
        lightView.M42 = -MathF.Round(lightFocus.Y / texelWorldSize) * texelWorldSize;
        lightView.M43 = -lightFocus.Z - 520f;

        var lightProjection = Matrix4x4.CreateOrthographic(
            worldSpan,
            worldSpan,
            2f,
            1050f);

        return lightView * lightProjection;
    }
}
