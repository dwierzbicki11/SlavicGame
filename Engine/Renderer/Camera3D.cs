using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public enum CameraMode
{
    FirstPerson,
    ThirdPerson
}

public sealed class Camera3D
{
    private bool _hasFollowed;
    private float _terrainClearance = 0.5f;

    public Vector3 Position { get; private set; } = new(0f, 8f, 12f);
    public Vector3 Target { get; private set; } = Vector3.Zero;
    public float FieldOfView { get; set; } = MathF.PI / 3f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 600f;
    public float Yaw { get; private set; } = MathF.PI;
    public float Pitch { get; private set; } = -0.25f;
    public float MouseSensitivity { get; set; } = 0.0035f;
    public float VerticalSensitivity { get; set; } = 0.0025f;
    public float Distance { get; set; } = 3.6f;
    public float AimDistance { get; set; } = 1.85f;
    public float ShoulderOffset { get; set; } = 0.35f;
    public float AimShoulderOffset { get; set; } = 0.55f;
    public float TargetHeight { get; set; } = 1.5f;
    public float HeightOffset { get; set; } = 0.5f;
    public float PositionSmoothing { get; set; } = 14f;
    public CameraMode Mode { get; set; } = CameraMode.ThirdPerson;
    public float EyeHeight { get; set; } = 1.72f;
    public bool IsAiming { get; set; }

    private float _currentDistance = 3.6f;
    private float _currentShoulder = 0.35f;
    private float _currentHeightOffset = 0.5f;
    public float TerrainClearance
    {
        get => _terrainClearance;
        set
        {
            if (!float.IsFinite(value) || value < 0f)
                throw new ArgumentOutOfRangeException(nameof(value));
            _terrainClearance = value;
        }
    }

    public void Update(Vector3 playerPosition, float mouseDeltaX, float mouseDeltaY, float deltaSeconds,
        Terrain? terrain = null)
    {
        Rotate(mouseDeltaX, mouseDeltaY);
        Follow(playerPosition, deltaSeconds, terrain);
    }

    public void SetCinematicPose(Vector3 position, Vector3 target)
    {
        Position = position;
        Target = target;
    }

    public void ResumeFollow(Vector3 playerPosition, Terrain terrain)
    {
        _hasFollowed = false;
        _currentDistance = IsAiming ? AimDistance : Distance;
        _currentShoulder = IsAiming ? AimShoulderOffset : ShoulderOffset;
        _currentHeightOffset = IsAiming ? 0.18f : HeightOffset;
        Follow(playerPosition, 0f, terrain);
    }

    public void Rotate(float mouseDeltaX, float mouseDeltaY)
    {
        var maxMouseDelta = 150f;
        mouseDeltaX = Math.Clamp(mouseDeltaX, -maxMouseDelta, maxMouseDelta);
        mouseDeltaY = Math.Clamp(mouseDeltaY, -maxMouseDelta, maxMouseDelta);

        Yaw -= mouseDeltaX * MouseSensitivity;
        Pitch -= mouseDeltaY * VerticalSensitivity;
        Pitch = Math.Clamp(Pitch, -1.15f, 0.85f);
    }

    public void Follow(Vector3 playerPosition, float deltaSeconds, Terrain? terrain = null)
    {
        if (Mode == CameraMode.FirstPerson)
        {
            var eye = playerPosition + Vector3.UnitY * EyeHeight;
            if (terrain is not null)
                eye = KeepAboveGround(terrain, eye, MathF.Max(EyeHeight, TerrainClearance + 0.1f));

            Position = eye;
            Target = eye + GetLookDirection() * 10f;
            _hasFollowed = true;
            return;
        }

        var targetDist = IsAiming ? AimDistance : Distance;
        var targetShoulder = IsAiming ? AimShoulderOffset : ShoulderOffset;
        var targetHeight = IsAiming ? 0.18f : HeightOffset;
        var smoothing = 1f - MathF.Exp(-PositionSmoothing * MathF.Max(0f, deltaSeconds));

        _currentDistance = _hasFollowed ? float.Lerp(_currentDistance, targetDist, smoothing) : targetDist;
        _currentShoulder = _hasFollowed ? float.Lerp(_currentShoulder, targetShoulder, smoothing) : targetShoulder;
        _currentHeightOffset = _hasFollowed ? float.Lerp(_currentHeightOffset, targetHeight, smoothing) : targetHeight;

        var target = playerPosition + new Vector3(0f, TargetHeight, 0f);
        var cameraForward = GetLookDirection();
        var cameraRight = GetMoveRight();
        var desiredPosition = target - cameraForward * _currentDistance + cameraRight * _currentShoulder + Vector3.UnitY * _currentHeightOffset;

        if (terrain is not null)
            desiredPosition = KeepAboveGround(terrain, desiredPosition, TerrainClearance + 0.01f);

        Position = _hasFollowed ? Vector3.Lerp(Position, desiredPosition, smoothing) : desiredPosition;
        Target = _hasFollowed ? Vector3.Lerp(Target, target, smoothing) : target;
        _hasFollowed = true;

        if (terrain is null) return;

        Target = KeepAboveGround(terrain, Target, MathF.Max(TargetHeight, TerrainClearance + 0.1f));
        Position = KeepAboveGround(terrain, Position, TerrainClearance + 0.01f);

        var hit = terrain.IntersectGroundSegment(Target, Position, TerrainClearance);
        if (hit is not null)
        {
            var length = Vector3.Distance(Target, Position);
            var backoff = MathF.Min(hit.Value * 0.1f, 0.02f / MathF.Max(0.02f, length));
            var safeFraction = hit.Value - backoff;
            Position = Vector3.Lerp(Target, Position, safeFraction);
            Position = KeepAboveGround(terrain, Position, TerrainClearance + 0.01f);
        }
    }

    private static Vector3 KeepAboveGround(Terrain terrain, Vector3 position, float clearance)
    {
        position.Y = MathF.Max(position.Y, terrain.SampleHeight(position) + clearance);
        return position;
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
        return view * projection;
    }
}
