using System.Numerics;

namespace SlavicGame.Engine.Renderer;

public sealed class Camera3D
{
    public Vector3 Position { get; set; } = new(0f, 24f, 34f);
    public Vector3 Target { get; set; } = new(0f, 0f, 0f);
    public float FieldOfView { get; set; } = MathF.PI / 3f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 600f;

    public Matrix4x4 GetViewProjection(float aspectRatio)
    {
        var view = Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            FieldOfView,
            MathF.Max(0.1f, aspectRatio),
            NearPlane,
            FarPlane);

        return projection * view;
    }
}
