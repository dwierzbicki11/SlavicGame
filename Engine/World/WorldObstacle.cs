using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed class WorldObstacle
{
    private const float ResolveEpsilon = 0.001f;

    public string Id { get; }
    public Vector3 Position { get; }
    public Vector2 HalfSize { get; }
    public float Height { get; }
    public Vector3 Color { get; }

    public WorldObstacle(
        string id,
        Vector3 position,
        Vector2 halfSize,
        float height,
        Vector3 color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!IsFinite(position) ||
            !float.IsFinite(halfSize.X) || !float.IsFinite(halfSize.Y) ||
            halfSize.X <= 0f || halfSize.Y <= 0f ||
            !float.IsFinite(height) || height <= 0f ||
            !IsFinite(color))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Obstacle geometry must be finite and positive.");
        }

        Id = id;
        Position = position;
        HalfSize = halfSize;
        Height = height;
        Color = Vector3.Clamp(color, Vector3.Zero, Vector3.One);
    }

    public bool IntersectsCircle(Vector2 center, float radius)
    {
        if (!float.IsFinite(center.X) || !float.IsFinite(center.Y) ||
            !float.IsFinite(radius) || radius < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(radius));
        }

        var min = new Vector2(Position.X, Position.Z) - HalfSize;
        var max = new Vector2(Position.X, Position.Z) + HalfSize;
        var closest = Vector2.Clamp(center, min, max);
        return Vector2.DistanceSquared(center, closest) < radius * radius;
    }

    public bool BlocksHorizontalSegment(Vector3 start, Vector3 end)
    {
        if (!IsFinite(start) || !IsFinite(end))
            throw new ArgumentOutOfRangeException(nameof(start), "Segment endpoints must be finite.");

        var origin = new Vector2(start.X, start.Z);
        var direction = new Vector2(end.X - start.X, end.Z - start.Z);
        var min = new Vector2(Position.X, Position.Z) - HalfSize;
        var max = new Vector2(Position.X, Position.Z) + HalfSize;
        var tMin = 0f;
        var tMax = 1f;

        return ClipAxis(origin.X, direction.X, min.X, max.X, ref tMin, ref tMax) &&
               ClipAxis(origin.Y, direction.Y, min.Y, max.Y, ref tMin, ref tMax) &&
               tMax >= tMin;
    }

    public Vector2 ResolvePoint(Vector2 point, float radius)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) ||
            !float.IsFinite(radius) || radius < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(radius));
        }

        var center = new Vector2(Position.X, Position.Z);
        var expanded = HalfSize + new Vector2(radius);
        var min = center - expanded;
        var max = center + expanded;

        if (point.X <= min.X || point.X >= max.X ||
            point.Y <= min.Y || point.Y >= max.Y)
        {
            return point;
        }

        var left = point.X - min.X;
        var right = max.X - point.X;
        var top = point.Y - min.Y;
        var bottom = max.Y - point.Y;
        var nearest = MathF.Min(MathF.Min(left, right), MathF.Min(top, bottom));

        if (nearest == left) point.X = min.X - ResolveEpsilon;
        else if (nearest == right) point.X = max.X + ResolveEpsilon;
        else if (nearest == top) point.Y = min.Y - ResolveEpsilon;
        else point.Y = max.Y + ResolveEpsilon;

        return point;
    }

    private static bool ClipAxis(float origin, float direction, float min, float max, ref float tMin, ref float tMax)
    {
        if (MathF.Abs(direction) <= 0.000001f) return origin >= min && origin <= max;
        var inverse = 1f / direction;
        var near = (min - origin) * inverse;
        var far = (max - origin) * inverse;
        if (near > far) (near, far) = (far, near);
        tMin = MathF.Max(tMin, near);
        tMax = MathF.Min(tMax, far);
        return tMin <= tMax;
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
