using System.Numerics;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Characters;

public static class NpcAccessoryMeshBuilder
{
    public static void AppendAccessory(
        NpcAccessoryKind accessory,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var darkTone = color * 0.72f;
        var lightTone = Vector3.Lerp(color, Vector3.One, 0.18f);
        var leatherTone = new Vector3(0.36f, 0.24f, 0.15f);
        var brassTone = new Vector3(0.74f, 0.58f, 0.28f);
        var steelTone = new Vector3(0.62f, 0.65f, 0.69f);

        switch (accessory)
        {
            case NpcAccessoryKind.None:
                return;

            case NpcAccessoryKind.Shawl:
                AddShawl(transform, color, darkTone, brassTone, vertices, indices);
                return;

            case NpcAccessoryKind.Satchel:
                AddSatchel(transform, color, darkTone, brassTone, vertices, indices);
                return;

            case NpcAccessoryKind.Staff:
                AddStaff(transform, color, leatherTone, brassTone, vertices, indices);
                return;

            case NpcAccessoryKind.Spear:
                AddSpear(transform, color, leatherTone, steelTone, vertices, indices);
                return;

            case NpcAccessoryKind.Hood:
                AddOpenHood(transform, color, darkTone, vertices, indices);
                return;

            case NpcAccessoryKind.ToolBundle:
                AddToolBundle(transform, color, lightTone, leatherTone, steelTone, vertices, indices);
                return;

            case NpcAccessoryKind.Basket:
                AddWickerBasket(transform, color, darkTone, lightTone, vertices, indices);
                return;

            case NpcAccessoryKind.BeltPouch:
                AddBeltPouch(transform, color, darkTone, brassTone, vertices, indices);
                return;

            case NpcAccessoryKind.Apron:
                AddCurvedApron(transform, color, darkTone, leatherTone, vertices, indices);
                return;

            case NpcAccessoryKind.FishingPole:
                AddFishingPole(transform, color, lightTone, leatherTone, vertices, indices);
                return;

            case NpcAccessoryKind.ShoulderBundle:
                AddShoulderBundle(transform, color, lightTone, leatherTone, vertices, indices);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(accessory));
        }
    }

    private static void AddShawl(
        Matrix4x4 transform,
        Vector3 color,
        Vector3 trimColor,
        Vector3 claspColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Contoured pleated Slavic shoulder shawl (chusta / opończa) that drapes smoothly over shoulders and chest
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> rings =
        [
            (1.20f, 0.25f, 0.165f, 0f,  0.010f, 0.010f),
            (1.28f, 0.33f, 0.175f, 0f,  0.008f, 0.008f),
            (1.36f, 0.34f, 0.165f, 0f,  0.005f, 0.005f),
            (1.42f, 0.25f, 0.145f, 0f,  0.004f, 0.003f),
            (1.46f, 0.13f, 0.120f, 0f,  0.002f, 0.000f)
        ];
        AddYLoft(rings, 14, transform, color, vertices, indices, capBottom: false, capTop: false);

        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> borderRings =
        [
            (1.18f, 0.23f, 0.158f, 0f, 0.012f, 0.010f),
            (1.21f, 0.255f, 0.168f, 0f, 0.010f, 0.010f)
        ];
        AddYLoft(borderRings, 14, transform, trimColor, vertices, indices, capBottom: false, capTop: false);

        // Front brooch clasp (zapona) at chest collar
        AddEllipsoid(new Vector3(0f, 1.38f, -0.155f), new Vector3(0.026f, 0.026f, 0.012f), 5, 8, transform, claspColor, vertices, indices);
    }

    private static void AddOpenHood(
        Matrix4x4 transform,
        Vector3 color,
        Vector3 innerColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Lower cowl resting on upper shoulders/neck
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> cowlRings =
        [
            (1.36f, 0.26f, 0.165f, 0f, 0.012f, 0.006f),
            (1.43f, 0.17f, 0.142f, 0f, 0.010f, 0.004f),
            (1.48f, 0.135f, 0.132f, 0f, 0.012f, 0.002f)
        ];
        AddYLoft(cowlRings, 12, transform, color, vertices, indices, capBottom: false, capTop: false);

        // Open-front sculpted hood arch that frames the face (-Z is forward) without covering eyes, nose, or beard
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cz, float FrontOpenAngleRad)> hoodSlices =
        [
            (1.47f, 0.132f, 0.132f, 0.012f, 1.15f),
            (1.54f, 0.138f, 0.142f, 0.016f, 1.18f),
            (1.62f, 0.140f, 0.148f, 0.018f, 1.12f),
            (1.70f, 0.130f, 0.146f, 0.012f, 0.92f),
            (1.75f, 0.098f, 0.132f, 0.004f, 0.45f),
            (1.78f, 0.045f, 0.085f, 0.015f, 0.00f)
        ];

        const int arcSteps = 10;
        if (!Matrix4x4.Invert(transform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        var baseVertex = checked((uint)vertices.Count);
        for (var r = 0; r < hoodSlices.Length; r++)
        {
            var (y, rx, rz, cz, openHalfAngle) = hoodSlices[r];
            // Angle 0 is -Z (front). We sweep around the sides and back from +openHalfAngle to 2*PI - openHalfAngle
            var startAng = openHalfAngle;
            var endAng = MathF.PI * 2f - openHalfAngle;
            for (var s = 0; s <= arcSteps; s++)
            {
                var t = s / (float)arcSteps;
                var ang = startAng + (endAng - startAng) * t;
                var sinA = MathF.Sin(ang);
                var cosA = MathF.Cos(ang);
                var px = sinA * rx;
                var pz = cz - cosA * rz;
                var localNormal = Vector3.Normalize(new Vector3(sinA, 0.18f * (r / (float)hoodSlices.Length), -cosA));
                var worldNormal = TransformNormalSafe(localNormal, normalMatrix);
                var shade = (s == 0 || s == arcSteps) ? innerColor : color;
                vertices.Add(new TerrainVertex(
                    Vector3.Transform(new Vector3(px, y, pz), transform),
                    shade,
                    worldNormal));
            }
        }

        var stride = (uint)(arcSteps + 1);
        for (uint r = 0; r < (uint)(hoodSlices.Length - 1); r++)
        {
            for (uint s = 0; s < (uint)arcSteps; s++)
            {
                var i0 = baseVertex + r * stride + s;
                var i1 = i0 + 1;
                var i2 = i0 + stride;
                var i3 = i2 + 1;
                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        // Subtle draped hood tail at back of head (+Z)
        AddCylinderRod(
            new Vector3(0f, 1.72f, 0.135f),
            new Vector3(0f, 1.50f, 0.175f),
            0.048f,
            0.018f,
            8,
            transform,
            innerColor,
            vertices,
            indices);
    }

    private static void AddSatchel(
        Matrix4x4 transform,
        Vector3 color,
        Vector3 flapColor,
        Vector3 claspColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Rounded leather satchel resting against the right hip (X = +0.215, Y = 0.74..0.94, Z = +0.02)
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> bagRings =
        [
            (0.73f, 0.048f, 0.105f, 0.215f, 0.025f, 0.0f),
            (0.78f, 0.064f, 0.128f, 0.220f, 0.025f, 0.003f),
            (0.87f, 0.062f, 0.125f, 0.218f, 0.025f, 0.003f),
            (0.93f, 0.046f, 0.108f, 0.210f, 0.025f, 0.0f)
        ];
        AddYLoft(bagRings, 10, transform, color, vertices, indices, capBottom: true, capTop: true);

        // Top over-flap and brass buckle
        AddEllipsoid(new Vector3(0.222f, 0.90f, 0.025f), new Vector3(0.066f, 0.045f, 0.126f), 5, 10, transform, flapColor, vertices, indices);
        AddEllipsoid(new Vector3(0.278f, 0.85f, 0.025f), new Vector3(0.012f, 0.020f, 0.020f), 4, 8, transform, claspColor, vertices, indices);

        // Contoured diagonal shoulder strap wrapping front and back of torso from left shoulder to right hip
        var leftShoulder = new Vector3(-0.175f, 1.39f, 0.005f);
        var chestFront = new Vector3(0.03f, 1.13f, -0.142f);
        var backMid = new Vector3(0.03f, 1.13f, 0.132f);
        var bagAnchor = new Vector3(0.210f, 0.92f, 0.025f);

        AddCylinderRod(leftShoulder, chestFront, 0.018f, 0.018f, 6, transform, flapColor, vertices, indices);
        AddCylinderRod(chestFront, bagAnchor, 0.018f, 0.016f, 6, transform, flapColor, vertices, indices);
        AddCylinderRod(leftShoulder, backMid, 0.018f, 0.018f, 6, transform, flapColor, vertices, indices);
        AddCylinderRod(backMid, bagAnchor, 0.018f, 0.016f, 6, transform, flapColor, vertices, indices);
    }

    private static void AddWickerBasket(
        Matrix4x4 transform,
        Vector3 color,
        Vector3 rimColor,
        Vector3 clothColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Tapered round woven wicker basket held at the right side (X = 0.29, Y = 0.62..0.85, Z = -0.04)
        const float bx = 0.295f;
        const float bz = -0.040f;
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> basketRings =
        [
            (0.62f, 0.095f, 0.105f, bx, bz, 0.003f),
            (0.69f, 0.118f, 0.130f, bx, bz, 0.005f),
            (0.77f, 0.136f, 0.148f, bx, bz, 0.005f),
            (0.84f, 0.146f, 0.158f, bx, bz, 0.004f)
        ];
        AddYLoft(basketRings, 12, transform, color, vertices, indices, capBottom: true, capTop: false);

        // Thick braided wicker rim
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> rimRings =
        [
            (0.83f, 0.152f, 0.164f, bx, bz, 0.004f),
            (0.86f, 0.150f, 0.162f, bx, bz, 0.004f)
        ];
        AddYLoft(rimRings, 12, transform, rimColor, vertices, indices, capBottom: false, capTop: false);

        // Linen cloth / gathered contents inside the basket
        AddEllipsoid(new Vector3(bx, 0.82f, bz), new Vector3(0.132f, 0.045f, 0.144f), 5, 10, transform, clothColor, vertices, indices);

        // Curved arched wicker handle over the basket
        Span<Vector3> handlePts =
        [
            new Vector3(bx, 0.85f, bz - 0.148f),
            new Vector3(bx, 0.94f, bz - 0.105f),
            new Vector3(bx, 0.98f, bz),
            new Vector3(bx, 0.94f, bz + 0.105f),
            new Vector3(bx, 0.85f, bz + 0.148f)
        ];
        for (var i = 0; i < handlePts.Length - 1; i++)
        {
            AddCylinderRod(handlePts[i], handlePts[i + 1], 0.015f, 0.015f, 6, transform, rimColor, vertices, indices);
        }
    }

    private static void AddBeltPouch(
        Matrix4x4 transform,
        Vector3 color,
        Vector3 darkColor,
        Vector3 brassColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Rounded drawstring leather pouch hanging from the waist belt on the left hip
        const float px = -0.195f;
        const float pz = -0.055f;
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> pouchRings =
        [
            (0.76f, 0.038f, 0.044f, px, pz, 0.0f),
            (0.80f, 0.058f, 0.064f, px, pz, 0.004f),
            (0.85f, 0.052f, 0.058f, px, pz, 0.004f),
            (0.89f, 0.028f, 0.032f, px, pz, 0.006f),
            (0.91f, 0.036f, 0.038f, px, pz, 0.003f)
        ];
        AddYLoft(pouchRings, 10, transform, color, vertices, indices, capBottom: true, capTop: true);
        AddCylinderRod(
            new Vector3(px, 0.88f, pz),
            new Vector3(px + 0.02f, 0.93f, pz + 0.01f),
            0.012f,
            0.010f,
            6,
            transform,
            darkColor,
            vertices,
            indices);
        AddEllipsoid(new Vector3(px - 0.01f, 0.84f, pz - 0.052f), new Vector3(0.012f, 0.012f, 0.010f), 4, 6, transform, brassColor, vertices, indices);
    }

    private static void AddCurvedApron(
        Matrix4x4 transform,
        Vector3 color,
        Vector3 trimColor,
        Vector3 strapColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Curved front craftsman/villager apron wrapping smoothly around the front torso and thighs (-Z is front)
        ReadOnlySpan<(float Y, float HalfWidth, float FrontZ, float SideCurveZ)> slices =
        [
            (0.58f, 0.195f, -0.148f, 0.065f),
            (0.74f, 0.190f, -0.146f, 0.062f),
            (0.90f, 0.175f, -0.140f, 0.056f),
            (1.06f, 0.145f, -0.142f, 0.038f),
            (1.22f, 0.115f, -0.138f, 0.024f)
        ];

        const int segs = 10;
        if (!Matrix4x4.Invert(transform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        var baseVertex = checked((uint)vertices.Count);
        for (var r = 0; r < slices.Length; r++)
        {
            var (y, halfW, frontZ, curveZ) = slices[r];
            for (var s = 0; s <= segs; s++)
            {
                var u = (s / (float)segs) * 2f - 1f; // -1..+1
                var x = u * halfW;
                var z = frontZ + u * u * curveZ;
                var localNormal = Vector3.Normalize(new Vector3(u * 0.45f, 0.04f, -1f));
                var worldNormal = TransformNormalSafe(localNormal, normalMatrix);
                var vColor = (r == 0) ? trimColor : color;
                vertices.Add(new TerrainVertex(
                    Vector3.Transform(new Vector3(x, y, z), transform),
                    vColor,
                    worldNormal));
            }
        }

        var stride = (uint)(segs + 1);
        for (uint r = 0; r < (uint)(slices.Length - 1); r++)
        {
            for (uint s = 0; s < (uint)segs; s++)
            {
                var i0 = baseVertex + r * stride + s;
                var i1 = i0 + 1;
                var i2 = i0 + stride;
                var i3 = i2 + 1;
                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        // Neck loop strap & waist tie
        AddCylinderRod(new Vector3(-0.105f, 1.21f, -0.118f), new Vector3(-0.085f, 1.41f, -0.02f), 0.012f, 0.011f, 6, transform, strapColor, vertices, indices);
        AddCylinderRod(new Vector3(0.105f, 1.21f, -0.118f), new Vector3(0.085f, 1.41f, -0.02f), 0.012f, 0.011f, 6, transform, strapColor, vertices, indices);
    }

    private static void AddStaff(
        Matrix4x4 transform,
        Vector3 woodColor,
        Vector3 gripColor,
        Vector3 metalColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Sculpted wooden walking staff positioned naturally in the right hand (X = 0.33, Z = -0.06)
        const float sx = 0.33f;
        const float sz = -0.06f;
        AddCylinderRod(new Vector3(sx, 0.02f, sz), new Vector3(sx, 0.90f, sz - 0.01f), 0.020f, 0.023f, 8, transform, woodColor, vertices, indices);
        AddCylinderRod(new Vector3(sx, 0.90f, sz - 0.01f), new Vector3(sx + 0.01f, 1.66f, sz - 0.02f), 0.023f, 0.026f, 8, transform, woodColor, vertices, indices);

        // Leather hand-grip wrap & bronze ferrule
        AddCylinderRod(new Vector3(sx, 0.72f, sz - 0.008f), new Vector3(sx, 0.92f, sz - 0.011f), 0.026f, 0.026f, 8, transform, gripColor, vertices, indices);
        AddCylinderRod(new Vector3(sx, 0.02f, sz), new Vector3(sx, 0.10f, sz), 0.022f, 0.022f, 8, transform, metalColor, vertices, indices);

        // Carved top crook / solar finial
        AddCylinderRod(new Vector3(sx + 0.01f, 1.66f, sz - 0.02f), new Vector3(sx - 0.02f, 1.76f, sz - 0.06f), 0.025f, 0.018f, 8, transform, woodColor, vertices, indices);
        AddEllipsoid(new Vector3(sx - 0.01f, 1.76f, sz - 0.05f), new Vector3(0.034f, 0.038f, 0.034f), 5, 8, transform, metalColor, vertices, indices);
    }

    private static void AddSpear(
        Matrix4x4 transform,
        Vector3 shaftColor,
        Vector3 gripColor,
        Vector3 steelColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        const float sx = 0.34f;
        const float sz = -0.05f;
        AddCylinderRod(new Vector3(sx, 0.02f, sz), new Vector3(sx, 1.88f, sz), 0.019f, 0.021f, 8, transform, shaftColor, vertices, indices);
        AddCylinderRod(new Vector3(sx, 0.74f, sz), new Vector3(sx, 0.96f, sz), 0.024f, 0.024f, 8, transform, gripColor, vertices, indices);
        AddCylinderRod(new Vector3(sx, 1.84f, sz), new Vector3(sx, 1.92f, sz), 0.023f, 0.018f, 8, transform, steelColor * 0.82f, vertices, indices);

        // Forged leaf-shaped spearhead with central fuller ridge
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> bladeRings =
        [
            (1.91f, 0.018f, 0.016f, sx, sz, 0.0f),
            (1.97f, 0.042f, 0.014f, sx, sz, 0.0f),
            (2.05f, 0.032f, 0.011f, sx, sz, 0.0f),
            (2.14f, 0.014f, 0.006f, sx, sz, 0.0f),
            (2.20f, 0.002f, 0.002f, sx, sz, 0.0f)
        ];
        AddYLoft(bladeRings, 8, transform, steelColor, vertices, indices, capBottom: false, capTop: true);
    }

    private static void AddToolBundle(
        Matrix4x4 transform,
        Vector3 handleColor,
        Vector3 secondaryWoodColor,
        Vector3 strapColor,
        Vector3 ironColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Strapped bundle of Slavic woodworking tools (axe + mallet/adze) carried neatly at the left back/hip
        var h1Bot = new Vector3(-0.22f, 0.74f, 0.135f);
        var h1Top = new Vector3(-0.16f, 1.34f, 0.140f);
        var h2Bot = new Vector3(-0.15f, 0.76f, 0.145f);
        var h2Top = new Vector3(-0.08f, 1.28f, 0.148f);

        AddCylinderRod(h1Bot, h1Top, 0.018f, 0.020f, 8, transform, handleColor, vertices, indices);
        AddCylinderRod(h2Bot, h2Top, 0.016f, 0.018f, 8, transform, secondaryWoodColor, vertices, indices);

        // Forged iron axe blade at top of primary haft
        AddCylinderRod(
            new Vector3(-0.17f, 1.28f, 0.138f),
            new Vector3(-0.28f, 1.30f, 0.155f),
            0.026f,
            0.042f,
            6,
            transform,
            ironColor,
            vertices,
            indices);

        // Leather binding cinch around the tools
        AddEllipsoid(new Vector3(-0.15f, 1.02f, 0.142f), new Vector3(0.075f, 0.028f, 0.032f), 5, 8, transform, strapColor, vertices, indices);
    }

    private static void AddFishingPole(
        Matrix4x4 transform,
        Vector3 rodColor,
        Vector3 tipColor,
        Vector3 lineColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var p0 = new Vector3(0.32f, 0.68f, -0.05f);
        var p1 = new Vector3(0.37f, 1.45f, -0.16f);
        var p2 = new Vector3(0.43f, 2.08f, -0.30f);
        var p3 = new Vector3(0.49f, 2.42f, -0.44f);
        var lineEnd = new Vector3(0.49f, 1.68f, -0.44f);

        AddCylinderRod(p0, p1, 0.018f, 0.014f, 8, transform, rodColor, vertices, indices);
        AddCylinderRod(p1, p2, 0.014f, 0.009f, 8, transform, rodColor, vertices, indices);
        AddCylinderRod(p2, p3, 0.009f, 0.005f, 6, transform, tipColor, vertices, indices);
        AddCylinderRod(p3, lineEnd, 0.003f, 0.003f, 4, transform, lineColor, vertices, indices);
        AddEllipsoid(lineEnd, new Vector3(0.012f, 0.024f, 0.012f), 4, 6, transform, new Vector3(0.78f, 0.26f, 0.18f), vertices, indices);
    }

    private static void AddShoulderBundle(
        Matrix4x4 transform,
        Vector3 clothColor,
        Vector3 trimColor,
        Vector3 strapColor,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Rolled cloth travel bundle slung diagonally across the upper back
        var rollA = new Vector3(-0.22f, 1.32f, 0.155f);
        var rollB = new Vector3(0.16f, 0.98f, 0.155f);
        AddCylinderRod(rollA, rollB, 0.095f, 0.092f, 10, transform, clothColor, vertices, indices);
        AddEllipsoid(rollA, new Vector3(0.092f, 0.092f, 0.092f), 5, 8, transform, trimColor, vertices, indices);
        AddEllipsoid(rollB, new Vector3(0.090f, 0.090f, 0.090f), 5, 8, transform, trimColor, vertices, indices);

        // Two leather cinch straps around the rolled bundle + shoulder loop
        var s1 = Vector3.Lerp(rollA, rollB, 0.28f);
        var s2 = Vector3.Lerp(rollA, rollB, 0.72f);
        AddEllipsoid(s1, new Vector3(0.102f, 0.102f, 0.025f), 5, 8, transform, strapColor, vertices, indices);
        AddEllipsoid(s2, new Vector3(0.102f, 0.102f, 0.025f), 5, 8, transform, strapColor, vertices, indices);
        AddCylinderRod(
            new Vector3(-0.18f, 1.39f, 0.02f),
            new Vector3(0.12f, 1.04f, -0.14f),
            0.018f,
            0.018f,
            6,
            transform,
            strapColor,
            vertices,
            indices);
    }

    private static void AddYLoft(
        ReadOnlySpan<(float Y, float Rx, float Rz, float Cx, float Cz, float Pleat)> rings,
        int segments,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices,
        bool capBottom,
        bool capTop)
    {
        if (rings.Length < 2 || segments < 3)
            return;

        if (!Matrix4x4.Invert(transform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        var baseVertex = checked((uint)vertices.Count);
        for (var r = 0; r < rings.Length; r++)
        {
            var (y, rx, rz, cx, cz, pleat) = rings[r];
            var prev = rings[Math.Max(0, r - 1)];
            var next = rings[Math.Min(rings.Length - 1, r + 1)];
            var dy = MathF.Max(0.001f, next.Y - prev.Y);
            var dr = ((next.Rx - prev.Rx) + (next.Rz - prev.Rz)) * 0.5f;
            var nySlope = -dr / dy;

            for (var s = 0; s < segments; s++)
            {
                var ang = 2f * MathF.PI * s / segments;
                var cosA = MathF.Cos(ang);
                var sinA = MathF.Sin(ang);
                var mod = 1f + pleat * MathF.Cos(ang * 6f);
                var px = cx + cosA * rx * mod;
                var pz = cz + sinA * rz * mod;

                var localNormal = Vector3.Normalize(new Vector3(
                    cosA / MathF.Max(0.01f, rx),
                    nySlope,
                    sinA / MathF.Max(0.01f, rz)));
                var worldNormal = TransformNormalSafe(localNormal, normalMatrix);

                vertices.Add(new TerrainVertex(
                    Vector3.Transform(new Vector3(px, y, pz), transform),
                    color,
                    worldNormal));
            }
        }

        var segU = (uint)segments;
        for (uint r = 0; r < (uint)(rings.Length - 1); r++)
        {
            for (uint s = 0; s < segU; s++)
            {
                var sNext = (s + 1) % segU;
                var i0 = baseVertex + r * segU + s;
                var i1 = baseVertex + r * segU + sNext;
                var i2 = baseVertex + (r + 1) * segU + s;
                var i3 = baseVertex + (r + 1) * segU + sNext;
                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        if (capBottom)
        {
            var bot = rings[0];
            var centerIdx = checked((uint)vertices.Count);
            vertices.Add(new TerrainVertex(
                Vector3.Transform(new Vector3(bot.Cx, bot.Y, bot.Cz), transform),
                color * 0.85f,
                TransformNormalSafe(-Vector3.UnitY, normalMatrix)));
            for (uint s = 0; s < segU; s++)
            {
                var sNext = (s + 1) % segU;
                indices.Add(centerIdx);
                indices.Add(baseVertex + s);
                indices.Add(baseVertex + sNext);
            }
        }

        if (capTop)
        {
            var top = rings[^1];
            var topRingStart = baseVertex + (uint)(rings.Length - 1) * segU;
            var centerIdx = checked((uint)vertices.Count);
            vertices.Add(new TerrainVertex(
                Vector3.Transform(new Vector3(top.Cx, top.Y, top.Cz), transform),
                color,
                TransformNormalSafe(Vector3.UnitY, normalMatrix)));
            for (uint s = 0; s < segU; s++)
            {
                var sNext = (s + 1) % segU;
                indices.Add(centerIdx);
                indices.Add(topRingStart + sNext);
                indices.Add(topRingStart + s);
            }
        }
    }

    private static void AddCylinderRod(
        Vector3 from,
        Vector3 to,
        float radiusFrom,
        float radiusTo,
        int segments,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var axis = to - from;
        var len = axis.Length();
        if (len < 0.0001f)
            return;

        var tangent = axis / len;
        var refUp = MathF.Abs(Vector3.Dot(tangent, Vector3.UnitY)) > 0.92f
            ? Vector3.UnitX
            : Vector3.UnitY;
        var normal = Vector3.Normalize(Vector3.Cross(tangent, refUp));
        var binormal = Vector3.Normalize(Vector3.Cross(tangent, normal));

        if (!Matrix4x4.Invert(transform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        var baseVertex = checked((uint)vertices.Count);
        for (var ring = 0; ring <= 2; ring++)
        {
            var t = ring * 0.5f;
            var center = Vector3.Lerp(from, to, t);
            var r = radiusFrom + (radiusTo - radiusFrom) * t;
            if (ring == 1)
                r *= 1.03f;

            for (var s = 0; s < segments; s++)
            {
                var ang = 2f * MathF.PI * s / segments;
                var radial = MathF.Cos(ang) * normal + MathF.Sin(ang) * binormal;
                var pos = center + radial * r;
                var worldNormal = TransformNormalSafe(radial, normalMatrix);
                vertices.Add(new TerrainVertex(
                    Vector3.Transform(pos, transform),
                    color,
                    worldNormal));
            }
        }

        var segU = (uint)segments;
        for (uint ring = 0; ring < 2; ring++)
        {
            for (uint s = 0; s < segU; s++)
            {
                var sNext = (s + 1) % segU;
                var i0 = baseVertex + ring * segU + s;
                var i1 = baseVertex + ring * segU + sNext;
                var i2 = baseVertex + (ring + 1) * segU + s;
                var i3 = baseVertex + (ring + 1) * segU + sNext;
                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        var capStart = checked((uint)vertices.Count);
        vertices.Add(new TerrainVertex(
            Vector3.Transform(from, transform),
            color * 0.9f,
            TransformNormalSafe(-tangent, normalMatrix)));
        vertices.Add(new TerrainVertex(
            Vector3.Transform(to, transform),
            color,
            TransformNormalSafe(tangent, normalMatrix)));

        var topRingOffset = baseVertex + 2 * segU;
        for (uint s = 0; s < segU; s++)
        {
            var sNext = (s + 1) % segU;
            indices.Add(capStart);
            indices.Add(baseVertex + s);
            indices.Add(baseVertex + sNext);

            indices.Add(capStart + 1);
            indices.Add(topRingOffset + sNext);
            indices.Add(topRingOffset + s);
        }
    }

    private static void AddEllipsoid(
        Vector3 center,
        Vector3 radii,
        int latSegments,
        int lonSegments,
        Matrix4x4 transform,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        if (!Matrix4x4.Invert(transform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        var baseVertex = checked((uint)vertices.Count);
        for (var i = 0; i <= latSegments; i++)
        {
            var v = i / (float)latSegments;
            var phi = (v - 0.5f) * MathF.PI;
            var cosPhi = MathF.Cos(phi);
            var sinPhi = MathF.Sin(phi);
            for (var j = 0; j < lonSegments; j++)
            {
                var u = j / (float)lonSegments;
                var theta = u * 2f * MathF.PI;
                var ux = cosPhi * MathF.Cos(theta);
                var uy = sinPhi;
                var uz = cosPhi * MathF.Sin(theta);

                var localPos = center + new Vector3(ux * radii.X, uy * radii.Y, uz * radii.Z);
                var localNormal = Vector3.Normalize(new Vector3(
                    ux / MathF.Max(0.005f, radii.X),
                    uy / MathF.Max(0.005f, radii.Y),
                    uz / MathF.Max(0.005f, radii.Z)));
                var worldNormal = TransformNormalSafe(localNormal, normalMatrix);

                vertices.Add(new TerrainVertex(
                    Vector3.Transform(localPos, transform),
                    color,
                    worldNormal));
            }
        }

        var lonU = (uint)lonSegments;
        for (uint i = 0; i < (uint)latSegments; i++)
        {
            for (uint j = 0; j < lonU; j++)
            {
                var jNext = (j + 1) % lonU;
                var a = baseVertex + i * lonU + j;
                var b = baseVertex + i * lonU + jNext;
                var c = baseVertex + (i + 1) * lonU + j;
                var d = baseVertex + (i + 1) * lonU + jNext;
                indices.Add(a);
                indices.Add(b);
                indices.Add(d);
                indices.Add(a);
                indices.Add(d);
                indices.Add(c);
            }
        }
    }

    private static Vector3 TransformNormalSafe(Vector3 localNormal, Matrix4x4 normalMatrix)
    {
        var worldNormal = Vector3.TransformNormal(localNormal, normalMatrix);
        return worldNormal.LengthSquared() < 0.000001f
            ? Vector3.UnitY
            : Vector3.Normalize(worldNormal);
    }
}
