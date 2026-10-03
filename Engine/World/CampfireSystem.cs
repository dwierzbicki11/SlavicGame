using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record CampfireDefinition(
    string Id,
    Vector2 Position,
    float HeatRadius,
    float VisualScale);

public static class CampfireSystem
{
    public static IReadOnlyList<CampfireDefinition> Fires { get; } =
    [
        new(
            "village-firepit",
            new Vector2(0f, -88f),
            HeatRadius: 7.5f,
            VisualScale: 1.05f),
        new(
            "forest-hunter-firepit",
            new Vector2(37f, 21f),
            HeatRadius: 6.5f,
            VisualScale: 0.92f)
    ];

    public static float HeatAt(Vector3 position)
    {
        var p = new Vector2(position.X, position.Z);
        var heat = 0f;

        foreach (var fire in Fires)
        {
            var distance = Vector2.Distance(p, fire.Position);
            if (distance >= fire.HeatRadius)
                continue;

            var t = 1f - distance / fire.HeatRadius;
            var smooth = t * t * (3f - 2f * t);
            heat = MathF.Max(heat, smooth);
        }

        return Math.Clamp(heat, 0f, 1f);
    }

    public static bool IsNearFire(Vector3 position) =>
        HeatAt(position) >= 0.08f;
}
