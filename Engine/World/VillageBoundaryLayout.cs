using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record VillageWallPlacement(
    string Id,
    string AssetPath,
    float X,
    float Z,
    Vector3 Scale,
    float YawRadians,
    bool BlocksMovement,
    float CollisionWidth,
    float CollisionDepth,
    float CollisionHeight);

public static class VillageBoundaryLayout
{
    public static IReadOnlyList<VillageWallPlacement> Placements { get; } = Build();

    private static IReadOnlyList<VillageWallPlacement> Build()
    {
        var result = new List<VillageWallPlacement>();

        // Northern defensive line. A central opening remains as the main approach
        // from the starting forest until an interactive gate is implemented.
        AddHorizontalLine(
            result,
            "village-north",
            z: -58f,
            minX: -28f,
            maxX: 28f,
            spacing: 5.2f,
            leaveCenterGap: true,
            asset: "models/static/village_walls/palisade_straight.glb");

        // Eastern and western sides close most of the settlement while leaving
        // the southern side more open toward fields and future farmland.
        AddVerticalLine(
            result,
            "village-west",
            x: -30f,
            minZ: -108f,
            maxZ: -64f,
            spacing: 5.3f,
            asset: "models/static/village_walls/palisade_straight.glb");

        AddVerticalLine(
            result,
            "village-east",
            x: 30f,
            minZ: -108f,
            maxZ: -64f,
            spacing: 5.3f,
            asset: "models/static/village_walls/palisade_straight.glb");

        // Internal low wattle fencing makes the settlement read as inhabited
        // rather than a set of isolated buildings.
        result.Add(new VillageWallPlacement(
            "yard-wattle-a",
            "models/static/village_walls/wall_wattle_straight.glb",
            -17f, -92f,
            Vector3.One,
            0f,
            true,
            4.2f, 0.45f, 1.9f));
        result.Add(new VillageWallPlacement(
            "yard-wattle-b",
            "models/static/village_walls/wall_wattle_corner.glb",
            -15f, -88f,
            Vector3.One,
            MathF.PI * 0.5f,
            true,
            0.5f, 3.8f, 1.9f));
        result.Add(new VillageWallPlacement(
            "yard-wattle-broken",
            "models/static/village_walls/wall_wattle_broken.glb",
            18f, -72f,
            Vector3.One,
            0.22f,
            false,
            0f, 0f, 0f));
        result.Add(new VillageWallPlacement(
            "village-stone-low-wall",
            "models/static/village_walls/stone_foundation_wall.glb",
            19f, -100f,
            new Vector3(1.15f),
            0f,
            true,
            5.0f, 0.8f, 1.2f));

        return result;
    }

    private static void AddHorizontalLine(
        List<VillageWallPlacement> output,
        string prefix,
        float z,
        float minX,
        float maxX,
        float spacing,
        bool leaveCenterGap,
        string asset)
    {
        var index = 0;
        for (var x = minX; x <= maxX + 0.01f; x += spacing)
        {
            if (leaveCenterGap && MathF.Abs(x) < spacing * 1.15f)
                continue;

            output.Add(new VillageWallPlacement(
                $"{prefix}-{index++:00}",
                asset,
                x,
                z,
                Vector3.One,
                0f,
                true,
                5.0f,
                0.55f,
                2.8f));
        }
    }

    private static void AddVerticalLine(
        List<VillageWallPlacement> output,
        string prefix,
        float x,
        float minZ,
        float maxZ,
        float spacing,
        string asset)
    {
        var index = 0;
        for (var z = minZ; z <= maxZ + 0.01f; z += spacing)
        {
            output.Add(new VillageWallPlacement(
                $"{prefix}-{index++:00}",
                asset,
                x,
                z,
                Vector3.One,
                MathF.PI * 0.5f,
                true,
                0.55f,
                5.0f,
                2.8f));
        }
    }
}
