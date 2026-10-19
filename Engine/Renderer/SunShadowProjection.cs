using System.Numerics;

namespace SlavicGame.Engine.Renderer;

// CPU-only projection math, shared by the renderer and regression tests.
public static class SunShadowProjection
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

        // Anchor the texel grid to the light axes, not world X/Z. Snapping
        // world coordinates leaves fractional texel motion as the light rotates
        // and does not stabilize camera height changes.
        var lightRight = Vector3.Normalize(Vector3.Cross(up, sunDirection));
        var lightUp = Vector3.Cross(sunDirection, lightRight);
        var focusX = Vector3.Dot(focusPosition, lightRight);
        var focusY = Vector3.Dot(focusPosition, lightUp);
        var snappedX = MathF.Round(focusX / texelWorldSize) * texelWorldSize;
        var snappedY = MathF.Round(focusY / texelWorldSize) * texelWorldSize;

        // Build the view directly so CreateLookAt subtraction cannot introduce
        // camera-dependent rotations through floating-point cancellation.
        var lightView = new Matrix4x4(
            lightRight.X, lightUp.X, sunDirection.X, 0f,
            lightRight.Y, lightUp.Y, sunDirection.Y, 0f,
            lightRight.Z, lightUp.Z, sunDirection.Z, 0f,
            -snappedX, -snappedY,
            -Vector3.Dot(focusPosition, sunDirection) - 520f, 1f);
        var lightProjection = Matrix4x4.CreateOrthographic(
            worldSpan,
            worldSpan,
            2f,
            1050f);

        return lightView * lightProjection;
    }
}
