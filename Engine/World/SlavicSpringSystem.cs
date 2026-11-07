using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record SlavicSpring(
    string Id,
    string Name,
    string RegionId,
    Vector3 Position,
    float InteractionRadius = 3.8f,
    string ModelPath = "models/static/wiadro_r0_01.glb")
{
    public string Prompt => $"E NAPIJ SIE ZE ZRODLA: {Name.ToUpperInvariant()}";
}

public sealed class SlavicSpringSystem
{
    public const float StaminaRecoveryMultiplier = 1.5f; // +50% recovery
    public const double BuffDurationSeconds = 120.0;     // 2 minutes of refreshed vigor

    public double BlessedWaterBuffRemainingSeconds { get; private set; }
    public bool HasBlessedWater => BlessedWaterBuffRemainingSeconds > 0.0;
    public string? LastSpringDrunkId { get; private set; }

    public static readonly IReadOnlyList<SlavicSpring> Springs =
    [
        new("well-zarnowiec", "Studnia Wiejska w Zarnowcu", "old-village", new Vector3(-4.0f, 0f, -86.0f), 3.8f, "models/static/wiadro_r0_01.glb"),
        new("spring-swiety-gaj", "Swiete Zrodlo w Swietym Gaju", "old-shrine", new Vector3(-166.0f, 0f, -76.0f), 4.2f, "models/static/misa_ofiarna_r0_01.glb"),
        new("spring-perun", "Zrodlo Zywej Wody Peruna", "grod-borzywoj", new Vector3(-246.0f, 0f, 156.0f), 4.2f, "models/static/kamien_kultowy_r0_01.glb"),
        new("spring-rybackie", "Zrodlisko Przy Mostku", "przystan-rybacka", new Vector3(149.0f, 0f, -166.0f), 4.0f, "models/static/garnek_gliniany_r0_01.glb")
    ];

    public void Update(double deltaSeconds)
    {
        if (BlessedWaterBuffRemainingSeconds > 0.0)
        {
            BlessedWaterBuffRemainingSeconds = Math.Max(0.0, BlessedWaterBuffRemainingSeconds - deltaSeconds);
        }
    }

    public bool TryDrink(WorldState world, string springId)
    {
        ArgumentNullException.ThrowIfNull(world);
        var spring = Springs.FirstOrDefault(s => string.Equals(s.Id, springId, StringComparison.Ordinal));
        if (spring is null) return false;

        var distSq = Vector2.DistanceSquared(
            new Vector2(world.PlayerPosition.X, world.PlayerPosition.Z),
            new Vector2(spring.Position.X, spring.Position.Z));
        if (distSq > spring.InteractionRadius * spring.InteractionRadius * 1.5f)
            return false;

        BlessedWaterBuffRemainingSeconds = BuffDurationSeconds;
        LastSpringDrunkId = spring.Id;

        // Quench fatigue, heal vitality, and fill stamina to 100%
        world.Player.Heal(20f);
        world.Player.RestoreStamina();

        return true;
    }

    public SlavicSpring? FindNearest(Vector3 playerPosition, float maxDistance = 4.2f)
    {
        SlavicSpring? nearest = null;
        var nearestDistSq = maxDistance * maxDistance;
        var playerH = new Vector2(playerPosition.X, playerPosition.Z);

        foreach (var spring in Springs)
        {
            var springH = new Vector2(spring.Position.X, spring.Position.Z);
            var d2 = Vector2.DistanceSquared(playerH, springH);
            if (d2 <= nearestDistSq)
            {
                nearestDistSq = d2;
                nearest = spring;
            }
        }

        return nearest;
    }

    public void Reset()
    {
        BlessedWaterBuffRemainingSeconds = 0.0;
        LastSpringDrunkId = null;
    }
}
