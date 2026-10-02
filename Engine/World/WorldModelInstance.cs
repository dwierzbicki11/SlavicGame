using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record WorldModelInstance(
    string Id,
    string AssetPath,
    Vector3 Position,
    Vector3 Scale,
    float YawRadians,
    Vector3 Color,
    bool SourceIsZUp = true);
