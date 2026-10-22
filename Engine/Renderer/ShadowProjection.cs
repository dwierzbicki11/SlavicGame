using System.Numerics;

namespace SlavicGame.Engine.Renderer;

/// <summary>CPU-only directional shadow projection shared by renderer and regressions.</summary>
public static class ShadowProjection
{
    public static Matrix4x4 Create(
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

        // Snap the shadow focus to the shadow texel footprint. This greatly
        // reduces shimmering while the camera moves slowly.
        mapSize = Math.Clamp(mapSize, 512u, 4096u);
        worldSpan = Math.Clamp(worldSpan, 80f, 420f);
        var texelWorldSize = worldSpan / mapSize;
        var up = MathF.Abs(Vector3.Dot(sunDirection, Vector3.UnitY)) > 0.96f
            ? Vector3.UnitZ
            : Vector3.UnitY;
        var right = Vector3.Normalize(Vector3.Cross(up, sunDirection));
        var lightUp = Vector3.Cross(sunDirection, right);

        // Quantize in the shadow map's axes, including camera height. Keep
        // the component along the light direction continuous for depth range.
        var lightX = Vector3.Dot(focusPosition, right);
        var lightY = Vector3.Dot(focusPosition, lightUp);
        var focus = focusPosition +
            right * (MathF.Round(lightX / texelWorldSize) * texelWorldSize - lightX) +
            lightUp * (MathF.Round(lightY / texelWorldSize) * texelWorldSize - lightY);

        var eye = focus + sunDirection * 520f;
        var lightView = Matrix4x4.CreateLookAt(eye, focus, up);
        var lightProjection = Matrix4x4.CreateOrthographic(
            worldSpan,
            worldSpan,
            2f,
            1050f);

        return lightView * lightProjection;
    }
}
