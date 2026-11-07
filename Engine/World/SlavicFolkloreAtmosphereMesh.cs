using System.Numerics;

namespace SlavicGame.Engine.World;

public static class SlavicFolkloreAtmosphereMesh
{
    public const float MaxShrineRenderDistance = 125f;
    public const float MaxWispRenderDistance = 95f;

    private readonly record struct TotemPlacement(
        Vector2 Position,
        float YawRadians,
        float Scale,
        Vector3 RibbonColor);

    private readonly record struct WispCluster(
        Vector2 Center,
        float SpreadRadius,
        int Count,
        Vector3 DayGlowColor,
        Vector3 NightGlowColor);

    private static readonly TotemPlacement[] TotemShrines =
    [
        // Starting forest oak clearing shrine (immediately visible near spawn)
        new(new Vector2(-6.5f, 6.5f), 0.55f, 1.05f, new Vector3(0.76f, 0.16f, 0.12f)),
        // Żarnowiec village northern crossroads kapliczka
        new(new Vector2(-5.5f, -66.0f), 0.18f, 1.12f, new Vector3(0.80f, 0.22f, 0.14f)),
        // Forest trail toward Kamienny Krąg
        new(new Vector2(-36.0f, 24.0f), -0.65f, 1.00f, new Vector3(0.72f, 0.52f, 0.16f)),
        // Kamienny Krąg sacred idol of Światowid
        new(new Vector2(-85.0f, 64.5f), MathF.PI, 1.28f, new Vector3(0.82f, 0.18f, 0.14f)),
        // Czarne Mokradła warning totem of Weles
        new(new Vector2(68.0f, 22.0f), -1.20f, 1.08f, new Vector3(0.28f, 0.58f, 0.46f)),
        // Rozstaje pod Grodem Borzywoja
        new(new Vector2(-242.0f, 138.0f), 0.75f, 1.15f, new Vector3(0.85f, 0.45f, 0.15f)),
        // Most Rzeczny i Szlak ku Przystani
        new(new Vector2(172.0f, -175.0f), 1.45f, 1.10f, new Vector3(0.25f, 0.65f, 0.75f)),
        // Stara Sztolnia w Dębowej Kniei
        new(new Vector2(-405.0f, 308.0f), -0.35f, 1.25f, new Vector3(0.65f, 0.25f, 0.18f)),
        // Rozstaje Smolników w Borze Perunowym
        new(new Vector2(-155.0f, -265.0f), 0.88f, 1.05f, new Vector3(0.55f, 0.45f, 0.25f))
    ];

    private static readonly WispCluster[] FolkloreWisps =
    [
        // Błędne ogniki nad Czarnymi Mokradłami
        new(new Vector2(95f, 35f), 26f, 14, new Vector3(0.45f, 1.45f, 1.35f), new Vector3(0.65f, 2.55f, 2.35f)),
        // Duchy przodków w Kamiennym Kręgu
        new(new Vector2(-85f, 55f), 16f, 10, new Vector3(1.35f, 0.95f, 0.42f), new Vector3(2.45f, 1.65f, 0.58f)),
        // Świetliki w Puszczy Żywia (okolice polany startowej)
        new(new Vector2(0f, 4f), 22f, 12, new Vector3(1.25f, 1.05f, 0.28f), new Vector3(2.35f, 1.85f, 0.42f)),
        // Świetliki przy wiosce Żarnowiec
        new(new Vector2(0f, -82f), 20f, 8, new Vector3(1.30f, 0.88f, 0.26f), new Vector3(2.20f, 1.45f, 0.38f)),
        // Świetliki nad Mostem Rzecznym
        new(new Vector2(179f, -180f), 20f, 10, new Vector3(0.55f, 1.35f, 1.55f), new Vector3(0.75f, 2.25f, 2.65f)),
        // Ogniki wokół Starej Sztolni
        new(new Vector2(-410f, 310f), 22f, 8, new Vector3(1.10f, 0.75f, 0.35f), new Vector3(2.10f, 1.25f, 0.45f))
    ];

    public static void AppendCarvedTotems(
        WorldState world,
        Vector3 viewerPosition,
        float seconds,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indices);

        var maxDistSq = MaxShrineRenderDistance * MaxShrineRenderDistance;
        var wind = Math.Clamp(world.Weather.WindIntensity, 0.08f, 1f);

        for (var i = 0; i < TotemShrines.Length; i++)
        {
            var shrine = TotemShrines[i];
            var dx = shrine.Position.X - viewerPosition.X;
            var dz = shrine.Position.Y - viewerPosition.Z;
            if (dx * dx + dz * dz > maxDistSq)
                continue;

            var groundY = world.Terrain.SampleHeight(new Vector3(shrine.Position.X, 0f, shrine.Position.Y));
            var root =
                Matrix4x4.CreateScale(shrine.Scale) *
                Matrix4x4.CreateRotationY(shrine.YawRadians) *
                Matrix4x4.CreateTranslation(new Vector3(shrine.Position.X, groundY, shrine.Position.Y));

            var stoneColor = new Vector3(0.36f, 0.37f, 0.34f);
            var mossyStoneColor = new Vector3(0.25f, 0.33f, 0.22f);
            var agedOakColor = new Vector3(0.32f, 0.21f, 0.12f);
            var carvedFaceColor = new Vector3(0.42f, 0.29f, 0.17f);
            var shingleRoofColor = new Vector3(0.22f, 0.14f, 0.08f);
            var wreathHerbColor = new Vector3(0.22f, 0.38f, 0.16f);

            // 1. Kamienna omszała podstawa (Mossy stone plinth)
            AddBox(new Vector3(0f, 0.16f, 0f), new Vector3(0.42f, 0.16f, 0.42f), root, stoneColor, vertices, indices);
            AddBox(new Vector3(0f, 0.36f, 0f), new Vector3(0.32f, 0.08f, 0.32f), root, mossyStoneColor, vertices, indices);

            // 2. Rzeźbiony słup dębowy z rytymi symbolami (Carved oak totem shaft)
            AddBox(new Vector3(0f, 1.15f, 0f), new Vector3(0.18f, 0.74f, 0.18f), root, agedOakColor, vertices, indices);
            AddBox(new Vector3(0f, 1.38f, 0f), new Vector3(0.21f, 0.05f, 0.21f), root, shrine.RibbonColor, vertices, indices);

            // 3. Czterotwarzowa głowa Światowida (Four-faced Slavic idol head)
            AddBox(new Vector3(0f, 2.02f, 0f), new Vector3(0.22f, 0.24f, 0.22f), root, carvedFaceColor, vertices, indices);
            // Brow ridges & noses on all 4 cardinal sides
            AddBox(new Vector3(0f, 2.10f, 0.23f), new Vector3(0.16f, 0.035f, 0.03f), root, agedOakColor, vertices, indices);
            AddBox(new Vector3(0f, 2.10f, -0.23f), new Vector3(0.16f, 0.035f, 0.03f), root, agedOakColor, vertices, indices);
            AddBox(new Vector3(0.23f, 2.10f, 0f), new Vector3(0.03f, 0.035f, 0.16f), root, agedOakColor, vertices, indices);
            AddBox(new Vector3(-0.23f, 2.10f, 0f), new Vector3(0.03f, 0.035f, 0.16f), root, agedOakColor, vertices, indices);

            // 4. Gontowy spiczasty daszek kapliczki (Peaked wooden shingle cap)
            AddPyramid(new Vector3(0f, 2.26f, 0f), 0.38f, 0.34f, root, shingleRoofColor, vertices, indices);

            // 5. Wianek ofiarny i powiewające wstążki (Herbal wreath & swaying Slavic ribbons)
            AddBox(new Vector3(0f, 1.72f, 0f), new Vector3(0.25f, 0.045f, 0.25f), root, wreathHerbColor, vertices, indices);

            var sway = MathF.Sin(seconds * (2.2f + wind * 1.8f) + i * 1.7f) * (0.04f + wind * 0.08f);
            AddBox(
                new Vector3(0.24f + sway * 0.5f, 1.48f, 0.08f),
                new Vector3(0.025f, 0.22f, 0.045f),
                root,
                shrine.RibbonColor,
                vertices,
                indices);
            AddBox(
                new Vector3(-0.24f + sway * 0.5f, 1.50f, -0.08f),
                new Vector3(0.025f, 0.20f, 0.045f),
                root,
                Vector3.Lerp(shrine.RibbonColor, new Vector3(0.92f, 0.85f, 0.68f), 0.35f),
                vertices,
                indices);
        }
    }

    public static void AppendFolkloreGlows(
        WorldState world,
        Vector3 viewerPosition,
        float seconds,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indices);

        var maxShrineDistSq = MaxShrineRenderDistance * MaxShrineRenderDistance;
        var maxWispDistSq = MaxWispRenderDistance * MaxWispRenderDistance;

        var hour = (float)world.Time.TimeOfDayHours;
        var isEveningOrNight = hour < 7.0f || hour > 18.0f;
        var nightStrength = isEveningOrNight ? 1.0f : 0.42f;

        // 1. Znicze ofiarne u stóp kapliczek (Votive candles & warm ground glow at Slavic shrines)
        for (var i = 0; i < TotemShrines.Length; i++)
        {
            var shrine = TotemShrines[i];
            var dx = shrine.Position.X - viewerPosition.X;
            var dz = shrine.Position.Y - viewerPosition.Z;
            if (dx * dx + dz * dz > maxShrineDistSq)
                continue;

            var groundY = world.Terrain.SampleHeight(new Vector3(shrine.Position.X, 0f, shrine.Position.Y));
            var center = new Vector3(shrine.Position.X, groundY + 0.44f, shrine.Position.Y);
            var flicker = 0.86f + 0.14f * MathF.Sin(seconds * 9.5f + i * 2.3f);
            var flameColor = new Vector3(3.4f, 1.55f, 0.22f) * flicker * (0.75f + 0.25f * nightStrength);

            AddGlowingOrb(center + new Vector3(0.22f, 0.06f, 0.22f), 0.065f * shrine.Scale, flameColor, vertices, indices);
            AddGlowingOrb(center + new Vector3(-0.20f, 0.05f, 0.24f), 0.058f * shrine.Scale, flameColor * 0.92f, vertices, indices);
        }

        // 2. Błędne ogniki nad mokradłami i świetliki w puszczy (Marsh wisps & forest fireflies)
        for (var c = 0; c < FolkloreWisps.Length; c++)
        {
            var cluster = FolkloreWisps[c];
            var dx = cluster.Center.X - viewerPosition.X;
            var dz = cluster.Center.Y - viewerPosition.Z;
            if (dx * dx + dz * dz > maxWispDistSq)
                continue;

            var glowColor = Vector3.Lerp(cluster.DayGlowColor, cluster.NightGlowColor, nightStrength);
            for (var i = 0; i < cluster.Count; i++)
            {
                var seed = c * 37.0f + i * 11.3f;
                var angle = seed * 1.618f + seconds * (0.18f + (i % 3) * 0.05f);
                var radial = cluster.SpreadRadius * (0.22f + 0.78f * ((i * 7 + 3) % cluster.Count) / (float)cluster.Count);
                var wx = cluster.Center.X + MathF.Cos(angle) * radial + MathF.Sin(seconds * 0.9f + seed) * 0.65f;
                var wz = cluster.Center.Y + MathF.Sin(angle) * radial + MathF.Cos(seconds * 0.8f + seed) * 0.65f;
                var wy = world.Terrain.SampleHeight(new Vector3(wx, 0f, wz)) + 0.65f +
                         0.45f * MathF.Sin(seconds * 1.7f + seed * 2.1f) +
                         (i % 4) * 0.22f;

                var pulse = 0.55f + 0.45f * MathF.Sin(seconds * 3.4f + seed * 3.7f);
                var orbRadius = (0.045f + (i % 3) * 0.018f) * (0.85f + 0.25f * pulse);
                AddGlowingOrb(
                    new Vector3(wx, wy, wz),
                    orbRadius,
                    glowColor * (0.72f + 0.55f * pulse),
                    vertices,
                    indices);
            }
        }

        // 3. Unoszące się iskry nad płonącymi ogniskami (Rising campfire embers)
        foreach (var fire in CampfireSystem.Fires)
        {
            if (!world.Campfires.IsLit(world, fire.Id))
                continue;

            var dx = fire.Position.X - viewerPosition.X;
            var dz = fire.Position.Y - viewerPosition.Z;
            if (dx * dx + dz * dz > maxWispDistSq)
                continue;

            var center = new Vector3(
                fire.Position.X,
                world.Terrain.SampleHeight(new Vector3(fire.Position.X, 0f, fire.Position.Y)) + 0.20f,
                fire.Position.Y);

            for (var e = 0; e < 6; e++)
            {
                var emberPhase = (seconds * (0.75f + e * 0.12f) + e * 0.31f) % 1.0f;
                var emberAngle = seconds * 2.1f + e * 1.15f;
                var emberRadius = (0.08f + emberPhase * 0.32f) * fire.VisualScale;
                var emberPos = center + new Vector3(
                    MathF.Cos(emberAngle) * emberRadius,
                    0.35f + emberPhase * 1.85f * fire.VisualScale,
                    MathF.Sin(emberAngle) * emberRadius);
                var emberFade = 1.0f - emberPhase * 0.72f;
                AddGlowingOrb(
                    emberPos,
                    0.024f * fire.VisualScale * emberFade,
                    new Vector3(4.6f, 2.0f, 0.28f) * emberFade,
                    vertices,
                    indices);
            }
        }
    }

    private static void AddBox(
        Vector3 center,
        Vector3 halfExtent,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        Span<Vector3> signs =
        [
            new Vector3(-1f, -1f, -1f),
            new Vector3( 1f, -1f, -1f),
            new Vector3( 1f, -1f,  1f),
            new Vector3(-1f, -1f,  1f),
            new Vector3(-1f,  1f, -1f),
            new Vector3( 1f,  1f, -1f),
            new Vector3( 1f,  1f,  1f),
            new Vector3(-1f,  1f,  1f)
        ];

        if (!Matrix4x4.Invert(transform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        var start = checked((uint)vertices.Count);
        for (var i = 0; i < signs.Length; i++)
        {
            var s = signs[i];
            var point = center + s * halfExtent;
            var localNormal = Vector3.Normalize(new Vector3(
                s.X / MathF.Max(0.01f, halfExtent.X),
                s.Y / MathF.Max(0.01f, halfExtent.Y),
                s.Z / MathF.Max(0.01f, halfExtent.Z)));
            var worldNormal = Vector3.TransformNormal(localNormal, normalMatrix);
            worldNormal = worldNormal.LengthSquared() < 0.000001f
                ? Vector3.UnitY
                : Vector3.Normalize(worldNormal);

            vertices.Add(new TerrainVertex(
                Vector3.Transform(point, transform),
                color,
                worldNormal));
        }

        AddFace(0, 1, 2, 3);
        AddFace(4, 7, 6, 5);
        AddFace(0, 4, 5, 1);
        AddFace(1, 5, 6, 2);
        AddFace(2, 6, 7, 3);
        AddFace(3, 7, 4, 0);

        void AddFace(uint a, uint b, uint c, uint d)
        {
            indices.Add(start + a);
            indices.Add(start + b);
            indices.Add(start + c);
            indices.Add(start + a);
            indices.Add(start + c);
            indices.Add(start + d);
        }
    }

    private static void AddPyramid(
        Vector3 baseCenter,
        float halfWidth,
        float height,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        if (!Matrix4x4.Invert(transform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        var start = checked((uint)vertices.Count);
        Span<Vector3> pts =
        [
            baseCenter + new Vector3(0f, height, 0f),
            baseCenter + new Vector3(-halfWidth, 0f, -halfWidth),
            baseCenter + new Vector3( halfWidth, 0f, -halfWidth),
            baseCenter + new Vector3( halfWidth, 0f,  halfWidth),
            baseCenter + new Vector3(-halfWidth, 0f,  halfWidth)
        ];
        Span<Vector3> nrms =
        [
            Vector3.UnitY,
            Vector3.Normalize(new Vector3(-1f, 0.5f, -1f)),
            Vector3.Normalize(new Vector3( 1f, 0.5f, -1f)),
            Vector3.Normalize(new Vector3( 1f, 0.5f,  1f)),
            Vector3.Normalize(new Vector3(-1f, 0.5f,  1f))
        ];

        for (var i = 0; i < pts.Length; i++)
        {
            var worldNormal = Vector3.TransformNormal(nrms[i], normalMatrix);
            worldNormal = worldNormal.LengthSquared() < 0.000001f
                ? Vector3.UnitY
                : Vector3.Normalize(worldNormal);
            vertices.Add(new TerrainVertex(
                Vector3.Transform(pts[i], transform),
                color,
                worldNormal));
        }

        for (uint i = 0; i < 4; i++)
        {
            indices.Add(start);
            indices.Add(start + 1 + i);
            indices.Add(start + 1 + (i + 1) % 4);
        }
    }

    private static void AddGlowingOrb(
        Vector3 center,
        float radius,
        Vector3 emissiveColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var start = checked((uint)vertices.Count);
        Span<Vector3> offsets =
        [
            Vector3.UnitY,
            -Vector3.UnitY,
            Vector3.UnitX,
            Vector3.UnitZ,
            -Vector3.UnitX,
            -Vector3.UnitZ
        ];

        for (var i = 0; i < offsets.Length; i++)
        {
            var offset = offsets[i];
            vertices.Add(new TerrainVertex(
                center + offset * radius,
                emissiveColor,
                offset));
        }

        for (uint i = 0; i < 4; i++)
        {
            var a = start + 2 + i;
            var b = start + 2 + (i + 1) % 4;
            indices.Add(start);
            indices.Add(a);
            indices.Add(b);
            indices.Add(start + 1);
            indices.Add(b);
            indices.Add(a);
        }
    }
}
