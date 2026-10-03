using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed class WaterInteractionState
{
    private Vector3 _previousPosition;
    private bool _hasPrevious;

    public bool IsInWater { get; private set; }
    public float MovementIntensity { get; private set; }
    public Vector3 SurfacePosition { get; private set; }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);

        var position = world.PlayerPosition;
        var waterLevel = WaterLandscape.WaterLevel(position.Z);
        var insideRibbon =
            MathF.Abs(position.X - WaterLandscape.CenterX(position.Z)) <=
            WaterLandscape.SurfaceHalfWidth(position.Z);
        var groundBelowWater = world.Terrain.SampleHeight(position) < waterLevel - 0.03f;

        IsInWater = insideRibbon && groundBelowWater;
        SurfacePosition = new Vector3(position.X, waterLevel + 0.035f, position.Z);

        var delta = _hasPrevious
            ? new Vector2(position.X - _previousPosition.X, position.Z - _previousPosition.Z)
            : Vector2.Zero;

        var dt = Math.Max(deltaSeconds, 0.0001);
        var speed = delta.Length() / (float)dt;
        var target = IsInWater
            ? Math.Clamp(speed / 5.0f, 0f, 1f)
            : 0f;

        var response = IsInWater ? 10f : 4f;
        var blend = 1f - MathF.Exp(-response * (float)Math.Max(0d, deltaSeconds));
        MovementIntensity += (target - MovementIntensity) * blend;

        if (!IsInWater && MovementIntensity < 0.01f)
            MovementIntensity = 0f;

        _previousPosition = position;
        _hasPrevious = true;
    }

    public void Reset(Vector3 position)
    {
        _previousPosition = position;
        _hasPrevious = true;
        MovementIntensity = 0f;
        IsInWater = false;
        SurfacePosition = position;
    }
}
