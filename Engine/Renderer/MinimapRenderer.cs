using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Renderer;

public readonly record struct MinimapPoi(
    string Name,
    string ShortTag,
    Vector2 Position,
    Vector4 Color);

public static class MinimapRenderer
{
    private static readonly MinimapPoi[] Landmarks =
    [
        new("ŻARNOWIEC", "Ż", ForestLayout.VillageCenter, new Vector4(0.96f, 0.82f, 0.30f, 1.0f)),
        new("GRÓD BORZYWOJ", "G", ForestLayout.StrongholdCenter, new Vector4(0.92f, 0.32f, 0.22f, 1.0f)),
        new("PRZYSTAŃ RYBACKA", "R", ForestLayout.FishingHamletCenter, new Vector4(0.28f, 0.76f, 0.88f, 1.0f)),
        new("MOST RZECZNY", "=", ForestLayout.RiverBridgeCenter, new Vector4(0.75f, 0.60f, 0.40f, 1.0f)),
        new("OSADA SMOLNIKÓW", "S", ForestLayout.WoodcutterCampCenter, new Vector4(0.38f, 0.82f, 0.38f, 1.0f)),
        new("OBÓZ KARAWANY", "O", ForestLayout.CaravanCampCenter, new Vector4(0.94f, 0.62f, 0.22f, 1.0f)),
        new("KAMIENNY KRĄG", "K", ForestLayout.ShrineCenter, new Vector4(0.82f, 0.52f, 0.92f, 1.0f)),
        new("CZARNE MOKRADŁA", "M", ForestLayout.SwampCenter, new Vector4(0.52f, 0.68f, 0.32f, 1.0f)),
        new("DĘBOWA KNIEJA", "D", new Vector2(-520f, 430f), new Vector4(0.45f, 0.78f, 0.42f, 1.0f)),
        new("BÓR PERUNOWY", "P", new Vector2(-535f, -430f), new Vector4(0.32f, 0.65f, 0.45f, 1.0f)),
        new("BRZOZOWE ŁĘGI", "B", new Vector2(505f, 435f), new Vector4(0.82f, 0.88f, 0.72f, 1.0f)),
        new("MOKRY BÓR", "W", new Vector2(530f, -360f), new Vector4(0.35f, 0.58f, 0.55f, 1.0f)),
        new("CHRAM PERUNA", "P", new Vector2(-248f, 160f), new Vector4(0.95f, 0.85f, 0.20f, 1.0f)),
        new("UROCZYSKO WELESA", "W", new Vector2(100f, -145f), new Vector4(0.35f, 0.65f, 0.85f, 1.0f)),
        new("ŚWIĘTY GAJ MOKOSZY", "M", new Vector2(-78.5f, -170f), new Vector4(0.85f, 0.55f, 0.75f, 1.0f)),
        new("STARA SZTOLNIA", "S", new Vector2(-410f, 310f), new Vector4(0.70f, 0.65f, 0.55f, 1.0f)),
        new("KUŹNIA SWAROGA", "S", new Vector2(-295f, 218f), new Vector4(0.98f, 0.45f, 0.15f, 1.0f)),
        new("PUSTKOWIE PROGU", "P", new Vector2(-460f, 400f), new Vector4(0.75f, 0.45f, 0.95f, 1.0f)),
        new("ARCHIWUM DAWNYCH PROGÓW", "A", new Vector2(-480f, 410f), new Vector4(0.55f, 0.70f, 0.95f, 1.0f))
    ];

    public static IReadOnlyList<MinimapPoi> Pois => Landmarks;

    public static void Render(
        WorldState world,
        Camera3D camera,
        float displayWidth,
        float displayHeight,
        Action<float, float, float, float, Vector4> addQuad,
        Action<Vector2, Vector2, Vector2, Vector4> addTriangle,
        Action<string, float, float, float, Vector4, bool, Vector4?> addText)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(addQuad);
        ArgumentNullException.ThrowIfNull(addTriangle);
        ArgumentNullException.ThrowIfNull(addText);

        const float mapSize = 148f;
        const float margin = 18f;
        var mapX = displayWidth - mapSize - margin;
        const float mapY = margin;

        var center = new Vector2(mapX + mapSize * 0.5f, mapY + mapSize * 0.5f);
        const float radius = mapSize * 0.44f; // ~65 pixels
        const float worldRange = 155f; // ~310m diameter

        var playerX = world.PlayerPosition.X;
        var playerZ = world.PlayerPosition.Z;

        // 1. Outer wooden frame and shadow
        addQuad(mapX - 3f, mapY - 3f, mapSize + 6f, mapSize + 6f, new Vector4(0.01f, 0.01f, 0.01f, 0.85f));
        addQuad(mapX, mapY, mapSize, mapSize, new Vector4(0.18f, 0.13f, 0.08f, 0.95f));

        // 2. Inner dark woodland terrain disc / background
        addQuad(mapX + 3f, mapY + 3f, mapSize - 6f, mapSize - 6f, new Vector4(0.06f, 0.08f, 0.05f, 0.92f));

        // 3. Octagonal corner bevels (carved Slavic wood border)
        const float cutSize = 18f;
        addTriangle(
            new Vector2(mapX + 3f, mapY + 3f),
            new Vector2(mapX + 3f + cutSize, mapY + 3f),
            new Vector2(mapX + 3f, mapY + 3f + cutSize),
            new Vector4(0.18f, 0.13f, 0.08f, 1f));
        addTriangle(
            new Vector2(mapX + mapSize - 3f, mapY + 3f),
            new Vector2(mapX + mapSize - 3f - cutSize, mapY + 3f),
            new Vector2(mapX + mapSize - 3f, mapY + 3f + cutSize),
            new Vector4(0.18f, 0.13f, 0.08f, 1f));
        addTriangle(
            new Vector2(mapX + 3f, mapY + mapSize - 3f),
            new Vector2(mapX + 3f + cutSize, mapY + mapSize - 3f),
            new Vector2(mapX + 3f, mapY + mapSize - 3f - cutSize),
            new Vector4(0.18f, 0.13f, 0.08f, 1f));
        addTriangle(
            new Vector2(mapX + mapSize - 3f, mapY + mapSize - 3f),
            new Vector2(mapX + mapSize - 3f - cutSize, mapY + mapSize - 3f),
            new Vector2(mapX + mapSize - 3f, mapY + mapSize - 3f - cutSize),
            new Vector4(0.18f, 0.13f, 0.08f, 1f));

        // Corner bronze rivets
        addQuad(mapX + 2f, mapY + 2f, 4f, 4f, new Vector4(0.65f, 0.52f, 0.28f, 1f));
        addQuad(mapX + mapSize - 6f, mapY + 2f, 4f, 4f, new Vector4(0.65f, 0.52f, 0.28f, 1f));
        addQuad(mapX + 2f, mapY + mapSize - 6f, 4f, 4f, new Vector4(0.65f, 0.52f, 0.28f, 1f));
        addQuad(mapX + mapSize - 6f, mapY + mapSize - 6f, 4f, 4f, new Vector4(0.65f, 0.52f, 0.28f, 1f));

        // 4. Subtle internal compass crosshairs
        addQuad(center.X - radius + 8f, center.Y - 0.5f, (radius - 8f) * 2f, 1f, new Vector4(0.35f, 0.42f, 0.32f, 0.35f));
        addQuad(center.X - 0.5f, center.Y - radius + 8f, 1f, (radius - 8f) * 2f, new Vector4(0.35f, 0.42f, 0.32f, 0.35f));

        // 5. Cardinal direction letters
        addText("N", center.X - 3f, mapY + 4f, 1.0f, new Vector4(0.98f, 0.86f, 0.35f, 1.0f), false, null);
        addText("S", center.X - 3f, mapY + mapSize - 12f, 1.0f, new Vector4(0.72f, 0.68f, 0.60f, 0.85f), false, null);
        addText("W", mapX + mapSize - 10f, center.Y - 4f, 1.0f, new Vector4(0.72f, 0.68f, 0.60f, 0.85f), false, null);
        addText("Z", mapX + 3f, center.Y - 4f, 1.0f, new Vector4(0.72f, 0.68f, 0.60f, 0.85f), false, null);

        // 6. River water ribbon
        const float zStep = 4.0f;
        for (var z = playerZ - worldRange; z <= playerZ + worldRange + 0.01f; z += zStep)
        {
            var cx = WaterLandscape.CenterX(z);
            var halfW = WaterLandscape.SurfaceHalfWidth(z);
            var dx = (cx - playerX) / worldRange * radius;
            var dy = -(z - playerZ) / worldRange * radius; // +Z is North (-screen Y)

            if (dx * dx + dy * dy <= (radius - 2f) * (radius - 2f))
            {
                var riverWidth = Math.Clamp(halfW / worldRange * radius * 1.8f, 3.5f, 12f);
                addQuad(
                    center.X + dx - riverWidth * 0.5f,
                    center.Y + dy - 2f,
                    riverWidth,
                    4.5f,
                    new Vector4(0.20f, 0.44f, 0.70f, 0.85f));
            }
        }

        // 7. Wooden bridge crossing
        var bdx = (ForestLayout.RiverBridgeCenter.X - playerX) / worldRange * radius;
        var bdy = -(ForestLayout.RiverBridgeCenter.Y - playerZ) / worldRange * radius;
        if (bdx * bdx + bdy * bdy <= (radius - 2f) * (radius - 2f))
        {
            addQuad(center.X + bdx - 5.5f, center.Y + bdy - 2.5f, 11f, 5f, new Vector4(0.75f, 0.58f, 0.35f, 0.98f));
        }

        // 8. Trails and roads
        var trails = ForestLayout.Trails;
        for (var i = 0; i < trails.Count; i++)
        {
            var trail = trails[i];
            const int trailSamples = 16;
            for (var s = 0; s <= trailSamples; s++)
            {
                var t = s / (float)trailSamples;
                var pt = ForestLayout.SampleMeanderingTrailPoint(trail, t);
                var tdx = (pt.X - playerX) / worldRange * radius;
                var tdy = -(pt.Y - playerZ) / worldRange * radius;

                if (tdx * tdx + tdy * tdy <= (radius - 2f) * (radius - 2f))
                {
                    addQuad(
                        center.X + tdx - 1.25f,
                        center.Y + tdy - 1.25f,
                        2.5f,
                        2.5f,
                        new Vector4(0.55f, 0.45f, 0.28f, 0.75f));
                }
            }
        }

        // 9. Points of Interest & settlements
        for (var i = 0; i < Landmarks.Length; i++)
        {
            var poi = Landmarks[i];
            var pdx = (poi.Position.X - playerX) / worldRange * radius;
            var pdy = -(poi.Position.Y - playerZ) / worldRange * radius;
            var pdist = MathF.Sqrt(pdx * pdx + pdy * pdy);

            if (pdist <= radius - 6f)
            {
                // In range: draw landmark diamond beacon
                addQuad(center.X + pdx - 3f, center.Y + pdy - 3f, 6f, 6f, poi.Color);
                addQuad(center.X + pdx - 1.5f, center.Y + pdy - 1.5f, 3f, 3f, new Vector4(1f, 1f, 1f, 0.9f));
                addText(poi.ShortTag, center.X + pdx - 2.5f, center.Y + pdy - 9f, 0.85f, poi.Color, false, null);
            }
            else
            {
                // Edge radar: clamp pointer to rim
                var angle = MathF.Atan2(pdy, pdx);
                var rimDist = radius - 5f;
                var tip = center + new Vector2(MathF.Cos(angle) * rimDist, MathF.Sin(angle) * rimDist);
                var back = tip - new Vector2(MathF.Cos(angle) * 5f, MathF.Sin(angle) * 5f);
                var perp = new Vector2(-MathF.Sin(angle) * 3.5f, MathF.Cos(angle) * 3.5f);
                addTriangle(tip, back + perp, back - perp, poi.Color);
            }
        }

        // 10. Wildlife actors in local sensory range
        var actors = world.Wildlife.Actors;
        for (var i = 0; i < actors.Count; i++)
        {
            var actor = actors[i];
            var adx = (actor.Position.X - playerX) / worldRange * radius;
            var ady = -(actor.Position.Z - playerZ) / worldRange * radius;

            if (adx * adx + ady * ady <= (radius - 3f) * (radius - 3f))
            {
                if (actor.Species == WildlifeSpecies.Wolf)
                {
                    addQuad(center.X + adx - 1.5f, center.Y + ady - 1.5f, 3f, 3f, new Vector4(0.92f, 0.24f, 0.18f, 0.95f));
                }
                else if (actor.Species == WildlifeSpecies.Raven)
                {
                    addQuad(center.X + adx - 1f, center.Y + ady - 1f, 2f, 2f, new Vector4(0.70f, 0.75f, 0.90f, 0.75f));
                }
                else
                {
                    addQuad(center.X + adx - 1.5f, center.Y + ady - 1.5f, 3f, 3f, new Vector4(0.45f, 0.85f, 0.38f, 0.85f));
                }
            }
        }

        // 10b. Active campfires in range
        foreach (var fire in CampfireSystem.Fires)
        {
            var fdx = (fire.Position.X - playerX) / worldRange * radius;
            var fdy = -(fire.Position.Y - playerZ) / worldRange * radius;
            if (fdx * fdx + fdy * fdy <= (radius - 3f) * (radius - 3f))
            {
                var isLit = world.Campfires.IsLit(world, fire.Id);
                var fireColor = isLit
                    ? new Vector4(1.0f, 0.55f, 0.12f, 0.95f)
                    : new Vector4(0.40f, 0.35f, 0.30f, 0.70f);
                addQuad(center.X + fdx - 2f, center.Y + fdy - 2f, 4f, 4f, fireColor);
            }
        }

        // 10c. Slavic sacred springs and village wells in range
        foreach (var spring in SlavicSpringSystem.Springs)
        {
            var sdx = (spring.Position.X - playerX) / worldRange * radius;
            var sdy = -(spring.Position.Z - playerZ) / worldRange * radius;
            if (sdx * sdx + sdy * sdy <= (radius - 3f) * (radius - 3f))
            {
                addQuad(center.X + sdx - 2f, center.Y + sdy - 2f, 4f, 4f, new Vector4(0.20f, 0.75f, 1.0f, 0.95f));
            }
        }

        // 11. Enemies in combat range
        for (var i = 0; i < world.Enemies.Count; i++)
        {
            var enemy = world.Enemies[i];
            if (!enemy.IsAlive)
                continue;

            var edx = (enemy.Position.X - playerX) / worldRange * radius;
            var edy = -(enemy.Position.Z - playerZ) / worldRange * radius;

            if (edx * edx + edy * edy <= (radius - 3f) * (radius - 3f))
            {
                addQuad(center.X + edx - 2.5f, center.Y + edy - 2.5f, 5f, 5f, new Vector4(1.0f, 0.15f, 0.15f, 1f));
                addQuad(center.X + edx - 1f, center.Y + edy - 1f, 2f, 2f, new Vector4(1f, 1f, 1f, 0.9f));
            }
        }

        // 11b. Slavic Bestiary mythical monsters in range
        var bestiaryActors = world.Bestiary.Actors;
        for (var i = 0; i < bestiaryActors.Count; i++)
        {
            var monster = bestiaryActors[i];
            if (!monster.IsAlive)
                continue;

            var mdx = (monster.Position.X - playerX) / worldRange * radius;
            var mdy = -(monster.Position.Z - playerZ) / worldRange * radius;

            if (mdx * mdx + mdy * mdy <= (radius - 3f) * (radius - 3f))
            {
                addQuad(center.X + mdx - 2.5f, center.Y + mdy - 2.5f, 5f, 5f, new Vector4(0.95f, 0.20f, 0.50f, 1f));
                addQuad(center.X + mdx - 1f, center.Y + mdy - 1f, 2f, 2f, new Vector4(1f, 1f, 1f, 0.9f));
            }
        }

        // 12. Player position and camera heading arrow / view cone
        var fwd = new Vector2(MathF.Sin(camera.Yaw), -MathF.Cos(camera.Yaw));
        var right = new Vector2(-fwd.Y, fwd.X);

        // View cone
        var coneLeft = center + fwd * 20f + right * 10f;
        var coneRight = center + fwd * 20f - right * 10f;
        addTriangle(center, coneLeft, coneRight, new Vector4(1.0f, 0.92f, 0.40f, 0.18f));

        // Player heading triangle
        var arrowTip = center + fwd * 9f;
        var arrowLeft = center - fwd * 3.5f + right * 4.5f;
        var arrowRight = center - fwd * 3.5f - right * 4.5f;
        addTriangle(arrowTip, arrowLeft, arrowRight, new Vector4(1.0f, 0.95f, 0.45f, 1.0f));

        // Player central dot
        addQuad(center.X - 1.5f, center.Y - 1.5f, 3f, 3f, new Vector4(1f, 1f, 1f, 1f));

        // 13. Region title banner beneath minimap
        var currentRegion = world.GetCurrentRegion();
        var regionName = BitmapFont.Prepare(currentRegion?.Name ?? "DZICZ");
        var regWidth = regionName.Length * BitmapFont.Advance * 1.0f;
        var regX = center.X - regWidth * 0.5f;
        addText(regionName, regX, mapY + mapSize + 4f, 1.0f, new Vector4(0.96f, 0.92f, 0.82f, 1.0f), true, null);
    }
}
