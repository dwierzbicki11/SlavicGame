using System.Numerics;

namespace SlavicGame.Engine.World;

public static class WorldPlacementOrientation
{
    private static readonly Vector2 VillageCenter = new(0f, -85f);
    private static readonly Vector2 VillageMarket = new(-1f, -76f);
    private static readonly Vector2 VillageForge = new(21f, -91f);
    private static readonly Vector2 ForestHunterCamp = new(37f, 21f);
    private static readonly Vector2 ShrineCenter = new(-85f, 55f);
    private static readonly Vector2 SwampCrossing = new(95f, 35f);
    private static readonly Vector2 PredatorHome = new(92f, 35f);
    private static readonly Vector2 KeepsakeArea = new(82f, 29f);

    // Source static assets are authored Z-up with their semantic front along +Y.
    // GlbModel maps source +Y to engine -Z, so yaw=0 means "face -Z".
    public static float YawFacing(Vector2 from, Vector2 target)
    {
        var delta = target - from;
        if (delta.LengthSquared() < 0.000001f)
            return 0f;

        delta = Vector2.Normalize(delta);
        return Normalize(MathF.Atan2(-delta.X, -delta.Y));
    }

    public static Vector2 ForwardFromYaw(float yaw)
    {
        var forward = new Vector2(-MathF.Sin(yaw), -MathF.Cos(yaw));
        return forward.LengthSquared() > 0f
            ? Vector2.Normalize(forward)
            : new Vector2(0f, -1f);
    }

    public static float ResolveYaw(
        string id,
        string assetPath,
        float x,
        float z,
        float authoredYaw)
    {
        var position = new Vector2(x, z);

        return id switch
        {
            "village-hut-a" or
            "village-hut-b" or
            "village-hut-c" or
            "village-hut-d" or
            "village-granary" or
            "village-barn" or
            "village-stable" or
            "village-watchtower" or
            "village-gate" =>
                YawFacing(position, VillageCenter),

            "village-forge" or
            "village-forge-anvil" or
            "village-forge-tools" or
            "village-wood-forge-a" =>
                YawFacing(position, VillageForge),

            "village-market" or
            "village-cart-market" or
            "village-market-basket-a" or
            "village-market-basket-b" or
            "village-market-sack-a" or
            "village-market-sack-b" or
            "village-market-pot-a" or
            "village-market-pot-b" =>
                YawFacing(position, VillageMarket),

            "village-cart-south" or
            "village-road-sign" =>
                YawFacing(position, VillageCenter),

            "forest-hunter-basket" or
            "forest-hunter-sack" or
            "forest-hunter-trap-a" or
            "forest-hunter-trap-b" =>
                YawFacing(position, ForestHunterCamp),

            "forest-crossroad-sign" =>
                YawFacing(position, Vector2.Zero),

            "forest-cave-entrance" =>
                YawFacing(position, new Vector2(-45f, 20f)),

            "swamp-broken-bridge" or
            "swamp-damaged-boardwalk" or
            "swamp-boardwalk" =>
                YawFacing(position, SwampCrossing),

            "swamp-claw-tracks-a" or
            "swamp-claw-tracks-b" =>
                YawFacing(position, PredatorHome),

            "swamp-blood-trace" =>
                YawFacing(position, KeepsakeArea),

            _ when id.StartsWith("shrine-", StringComparison.Ordinal) &&
                   !id.Equals("shrine-circle", StringComparison.Ordinal) &&
                   !id.Equals("shrine-altar", StringComparison.Ordinal) =>
                YawFacing(position, ShrineCenter),

            _ => Normalize(authoredYaw)
        };
    }

    private static float Normalize(float yaw)
    {
        while (yaw > MathF.PI) yaw -= MathF.Tau;
        while (yaw < -MathF.PI) yaw += MathF.Tau;
        return yaw;
    }
}
