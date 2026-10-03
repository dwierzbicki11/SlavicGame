using System.Numerics;

namespace SlavicGame.Engine.Renderer;

public readonly struct CameraFrustum
{
    private readonly Vector3 _position;
    private readonly Vector3 _forward;
    private readonly Vector3 _right;
    private readonly Vector3 _up;
    private readonly float _tanHalfVertical;
    private readonly float _tanHalfHorizontal;
    private readonly float _near;
    private readonly float _far;

    private CameraFrustum(
        Vector3 position,
        Vector3 forward,
        Vector3 right,
        Vector3 up,
        float tanHalfVertical,
        float tanHalfHorizontal,
        float nearPlane,
        float farPlane)
    {
        _position = position;
        _forward = forward;
        _right = right;
        _up = up;
        _tanHalfVertical = tanHalfVertical;
        _tanHalfHorizontal = tanHalfHorizontal;
        _near = nearPlane;
        _far = farPlane;
    }

    public static CameraFrustum Create(Camera3D camera, float aspectRatio)
    {
        ArgumentNullException.ThrowIfNull(camera);

        var forward = camera.GetLookDirection();
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        if (!IsFinite(right) || right.LengthSquared() < 0.000001f)
            right = Vector3.UnitX;

        var up = Vector3.Normalize(Vector3.Cross(right, forward));
        var halfVertical = Math.Clamp(
            camera.FieldOfView * 0.5f,
            0.05f,
            MathF.PI * 0.49f);
        var tanHalfVertical = MathF.Tan(halfVertical);
        var tanHalfHorizontal =
            tanHalfVertical * MathF.Max(0.1f, aspectRatio);

        return new CameraFrustum(
            camera.Position,
            forward,
            right,
            up,
            tanHalfVertical,
            tanHalfHorizontal,
            MathF.Max(0f, camera.NearPlane),
            MathF.Max(camera.NearPlane + 0.01f, camera.FarPlane));
    }

    public bool IntersectsSphere(Vector3 center, float radius)
    {
        if (!IsFinite(center) || !float.IsFinite(radius) || radius < 0f)
            return false;

        var toCenter = center - _position;
        var forwardDistance = Vector3.Dot(toCenter, _forward);

        if (forwardDistance + radius < _near)
            return false;
        if (forwardDistance - radius > _far)
            return false;

        // A sphere partly surrounding the camera is visible regardless of
        // angular side-plane checks.
        if (forwardDistance <= 0f && toCenter.LengthSquared() <= radius * radius)
            return true;
        if (forwardDistance + radius <= 0f)
            return false;

        var sideDepth = MathF.Max(forwardDistance, 0f);
        var horizontal = MathF.Abs(Vector3.Dot(toCenter, _right));
        var vertical = MathF.Abs(Vector3.Dot(toCenter, _up));

        var horizontalLimit = sideDepth * _tanHalfHorizontal + radius;
        var verticalLimit = sideDepth * _tanHalfVertical + radius;

        return horizontal <= horizontalLimit &&
               vertical <= verticalLimit;
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) &&
        float.IsFinite(value.Y) &&
        float.IsFinite(value.Z);
}
