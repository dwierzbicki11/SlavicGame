using System.Numerics;

namespace SlavicGame.Engine.Physics;

[Flags]
public enum CollisionLayer
{
    None = 0,
    World = 1 << 0,
    Player = 1 << 1,
    Npc = 1 << 2,
    Enemy = 1 << 3,
    Projectile = 1 << 4,
    Trigger = 1 << 5,
    All = int.MaxValue
}

public readonly record struct SphereCollider(
    Vector3 Center,
    float Radius,
    CollisionLayer Layer,
    CollisionLayer Mask)
{
    public bool IsValid =>
        float.IsFinite(Center.X) &&
        float.IsFinite(Center.Y) &&
        float.IsFinite(Center.Z) &&
        float.IsFinite(Radius) &&
        Radius > 0f;
}

public readonly record struct BoxCollider(
    Vector3 Center,
    Vector3 HalfExtents,
    CollisionLayer Layer,
    CollisionLayer Mask)
{
    public bool IsValid =>
        float.IsFinite(Center.X) &&
        float.IsFinite(Center.Y) &&
        float.IsFinite(Center.Z) &&
        float.IsFinite(HalfExtents.X) &&
        float.IsFinite(HalfExtents.Y) &&
        float.IsFinite(HalfExtents.Z) &&
        HalfExtents.X > 0f &&
        HalfExtents.Y > 0f &&
        HalfExtents.Z > 0f;
}
