using System.Numerics;
using SlavicGame.Engine.Renderer;

namespace SlavicGame.Engine.UI;

public static class CameraFrustumRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var camera = new Camera3D
        {
            FieldOfView = MathF.PI / 3f,
            NearPlane = 0.1f,
            FarPlane = 250f
        };

        var frustum = CameraFrustum.Create(camera, 16f / 9f);
        var forward = camera.GetLookDirection();

        check(
            frustum.IntersectsSphere(
                camera.Position + forward * 50f,
                2f),
            "Frustum keeps a visible sphere in front of camera");

        check(
            !frustum.IntersectsSphere(
                camera.Position - forward * 50f,
                2f),
            "Frustum rejects sphere behind camera");

        check(
            !frustum.IntersectsSphere(
                camera.Position + forward * 400f,
                2f),
            "Frustum rejects sphere beyond far plane");

        var right = Vector3.Normalize(
            Vector3.Cross(forward, Vector3.UnitY));
        check(
            !frustum.IntersectsSphere(
                camera.Position + forward * 30f + right * 120f,
                2f),
            "Frustum rejects sphere outside horizontal field of view");
    }
}
