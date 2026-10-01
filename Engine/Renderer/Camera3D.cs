using System.Numerics;

namespace SlavicGame.Engine.Renderer;

public sealed class Camera3D
{
    public Vector3 Position { get; private set; } = new(0f, 8f, 12f);
    public Vector3 Target { get; private set; } = Vector3.Zero;
    public float FieldOfView { get; set; } = MathF.PI / 3f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 600f;
    public float Yaw { get; private set; } = MathF.PI;
    public float Pitch { get; private set; } = -0.25f;

    public void Update(Vector3 playerPosition, float mouseDeltaX, float mouseDeltaY)
    {
        Yaw -= mouseDeltaX * 0.0035f;
        Pitch -= mouseDeltaY * 0.0025f;
        Pitch = Math.Clamp(Pitch, -1.25f, 1.0f);

        var target = playerPosition + new Vector3(0f, 1.5f, 0f);
        var cameraForward = GetLookDirection();
        Position = target - cameraForward * 9f + Vector3.UnitY * 5f;
        Target = target;
    }

    public Vector3 GetLookDirection()
    {
        var cp = MathF.Cos(Pitch);
        return Vector3.Normalize(new Vector3(
            MathF.Sin(Yaw) * cp,
            MathF.Sin(Pitch),
            MathF.Cos(Yaw) * cp));
    }

    public Vector3 GetMoveForward()
    {
        var f = GetLookDirection();
        f.Y = 0f;
        return f.LengthSquared() > 0.001f ? Vector3.Normalize(f) : -Vector3.UnitZ;
    }

    public Vector3 GetMoveRight()
        => Vector3.Normalize(Vector3.Cross(GetMoveForward(), Vector3.UnitY));

    public Matrix4x4 GetViewProjection(float aspectRatio)
    {
        var view = Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView, MathF.Max(0.1f, aspectRatio), NearPlane, FarPlane);
        return projection * view;
    }
}
