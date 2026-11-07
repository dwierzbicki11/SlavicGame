using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Characters;

public static class PlayerHunterAccessoryBuilder
{
    private static readonly Vector3 CuredLeatherTone = new(0.28f, 0.16f, 0.08f);
    private static readonly Vector3 DarkLeatherTone = new(0.18f, 0.10f, 0.05f);
    private static readonly Vector3 SlavicBrassTone = new(0.72f, 0.58f, 0.26f);
    private static readonly Vector3 WoodAshTone = new(0.48f, 0.35f, 0.20f);
    private static readonly Vector3 GooseFeatherTone = new(0.85f, 0.82f, 0.76f);
    private static readonly Vector3 BlackFeatherTone = new(0.18f, 0.18f, 0.20f);
    private static readonly Vector3 ForgedIronTone = new(0.62f, 0.65f, 0.70f);
    private static readonly Vector3 BowstringTone = new(0.78f, 0.74f, 0.64f);

    public static void AppendHunterEquipment(
        WorldState world,
        Matrix4x4 playerTransform,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indices);

        var hasBow = world.Progress.Inventory.Contains(BowCombatRuntime.BowItemId);
        var hasBasicArrows = world.Progress.Inventory.Contains(BowCombatRuntime.ArrowItemId);
        var hasIronArrows = world.Progress.Inventory.Contains(BowCombatRuntime.IronArrowItemId);
        var hasAnyArrows = hasBasicArrows || hasIronArrows;

        if (!Matrix4x4.Invert(playerTransform, out var inverse))
            inverse = Matrix4x4.Identity;
        var normalMatrix = Matrix4x4.Transpose(inverse);

        // 1. Leather quiver on hunter's back (slung diagonally across right shoulder down to left hip)
        if (hasBow || hasAnyArrows)
        {
            AppendQuiver(
                playerTransform,
                normalMatrix,
                hasIronArrows,
                vertices,
                indices);
        }

        // 2. Slung recurve bow across the back (when bow is owned and character is NOT actively aiming)
        if (hasBow && !world.Bow.IsAiming)
        {
            AppendSlungBow(
                playerTransform,
                normalMatrix,
                vertices,
                indices);
        }
    }

    private static void AppendQuiver(
        Matrix4x4 transform,
        Matrix4x4 normalMatrix,
        bool isIronArrow,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Quiver tube on back (+Z is back, +Y is up, +X is right)
        var top = new Vector3(0.12f, 1.40f, 0.16f);
        var bottom = new Vector3(0.04f, 0.95f, 0.14f);

        // Leather cylinder with brass reinforced rim
        AddCylinderRod(bottom, top, 0.046f, 0.054f, 10, transform, normalMatrix, CuredLeatherTone, vertices, indices);
        AddCylinderRod(top, top + new Vector3(0.008f, 0.035f, 0.002f), 0.055f, 0.056f, 10, transform, normalMatrix, SlavicBrassTone, vertices, indices);

        // Lower decorative brass band
        var midBottom = Vector3.Lerp(bottom, top, 0.30f);
        AddCylinderRod(midBottom, midBottom + new Vector3(0.005f, 0.025f, 0.001f), 0.048f, 0.049f, 8, transform, normalMatrix, SlavicBrassTone, vertices, indices);

        // Diagonal chest baldric strap across chest (-Z is front)
        var strapStart = top + new Vector3(0.02f, 0.02f, -0.04f);
        var strapChest = new Vector3(0.0f, 1.25f, -0.155f);
        var strapHip = new Vector3(-0.16f, 0.98f, 0.08f);

        AddCylinderRod(strapStart, strapChest, 0.020f, 0.022f, 6, transform, normalMatrix, DarkLeatherTone, vertices, indices);
        AddCylinderRod(strapChest, strapHip, 0.022f, 0.020f, 6, transform, normalMatrix, DarkLeatherTone, vertices, indices);

        // Carved bronze Perun amulet buckle on chest baldric
        AddEllipsoid(strapChest + new Vector3(0f, 0f, -0.015f), new Vector3(0.024f, 0.024f, 0.008f), 4, 8, transform, normalMatrix, SlavicBrassTone, vertices, indices);

        // Arrows protruding from top of quiver
        var featherColor = isIronArrow ? BlackFeatherTone : GooseFeatherTone;
        var tipColor = isIronArrow ? ForgedIronTone : SlavicBrassTone;

        ReadOnlySpan<Vector3> arrowOffsets =
        [
            new Vector3(-0.022f, 0.16f, -0.012f),
            new Vector3(0.012f, 0.19f, -0.008f),
            new Vector3(-0.008f, 0.22f, 0.016f),
            new Vector3(0.024f, 0.17f, 0.018f)
        ];

        foreach (var offset in arrowOffsets)
        {
            var arrowBase = top + new Vector3(offset.X * 0.4f, 0.02f, offset.Z * 0.4f);
            var arrowTip = top + offset;

            // Wooden shaft
            AddCylinderRod(arrowBase, arrowTip, 0.006f, 0.006f, 5, transform, normalMatrix, WoodAshTone, vertices, indices);

            // Feather vanes near nock
            var fletchStart = Vector3.Lerp(arrowBase, arrowTip, 0.65f);
            AddEllipsoid(fletchStart, new Vector3(0.018f, 0.038f, 0.018f), 3, 6, transform, normalMatrix, featherColor, vertices, indices);

            // Nock / arrowhead tip
            AddEllipsoid(arrowTip, new Vector3(0.008f, 0.014f, 0.008f), 3, 6, transform, normalMatrix, tipColor, vertices, indices);
        }
    }

    private static void AppendSlungBow(
        Matrix4x4 transform,
        Matrix4x4 normalMatrix,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        // Slung recurve bow across the back (from left shoulder to right hip, creating an X with the quiver)
        var grip = new Vector3(-0.02f, 1.22f, 0.19f);
        var upperTip = new Vector3(-0.24f, 1.58f, 0.14f);
        var lowerTip = new Vector3(0.18f, 0.88f, 0.16f);

        var upperMid = Vector3.Lerp(grip, upperTip, 0.5f) + new Vector3(-0.04f, 0.0f, -0.03f);
        var lowerMid = Vector3.Lerp(grip, lowerTip, 0.5f) + new Vector3(0.04f, 0.0f, -0.03f);

        // Leather wrapped center grip
        AddCylinderRod(grip - new Vector3(0.02f, 0.04f, 0f), grip + new Vector3(0.02f, 0.04f, 0f), 0.016f, 0.016f, 6, transform, normalMatrix, DarkLeatherTone, vertices, indices);

        // Curved wooden limbs
        AddCylinderRod(grip, upperMid, 0.015f, 0.012f, 6, transform, normalMatrix, WoodAshTone, vertices, indices);
        AddCylinderRod(upperMid, upperTip, 0.012f, 0.008f, 6, transform, normalMatrix, WoodAshTone, vertices, indices);

        AddCylinderRod(grip, lowerMid, 0.015f, 0.012f, 6, transform, normalMatrix, WoodAshTone, vertices, indices);
        AddCylinderRod(lowerMid, lowerTip, 0.012f, 0.008f, 6, transform, normalMatrix, WoodAshTone, vertices, indices);

        // Bowstring between tips
        AddCylinderRod(upperTip, lowerTip, 0.003f, 0.003f, 4, transform, normalMatrix, BowstringTone, vertices, indices);
    }

    private static void AddCylinderRod(
        Vector3 from,
        Vector3 to,
        float radiusFrom,
        float radiusTo,
        int segments,
        Matrix4x4 transform,
        Matrix4x4 normalMatrix,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
        var axis = to - from;
        var len = axis.Length();
        if (len < 0.001f)
            return;

        var tangent = axis / len;
        var perp = MathF.Abs(tangent.Y) < 0.90f ? Vector3.UnitY : Vector3.UnitX;
        var normal = Vector3.Normalize(Vector3.Cross(tangent, perp));
        var binormal = Vector3.Cross(tangent, normal);

        var baseVertex = checked((uint)vertices.Count);
        var segU = (uint)segments;

        // Two rings: from and to
        for (var ring = 0; ring < 2; ring++)
        {
            var center = ring == 0 ? from : to;
            var r = ring == 0 ? radiusFrom : radiusTo;

            for (var s = 0; s < segments; s++)
            {
                var ang = s / (float)segments * MathF.PI * 2f;
                var radial = MathF.Cos(ang) * normal + MathF.Sin(ang) * binormal;
                var pos = center + radial * r;
                var worldNormal = TransformNormalSafe(radial, normalMatrix);

                vertices.Add(new TerrainVertex(
                    Vector3.Transform(pos, transform),
                    color,
                    worldNormal));
            }
        }

        // Connect rings with quads (2 triangles each)
        for (uint s = 0; s < segU; s++)
        {
            var sNext = (s + 1) % segU;
            var i0 = baseVertex + s;
            var i1 = baseVertex + sNext;
            var i2 = baseVertex + segU + s;
            var i3 = baseVertex + segU + sNext;

            indices.Add(i0);
            indices.Add(i2);
            indices.Add(i1);
            indices.Add(i1);
            indices.Add(i2);
            indices.Add(i3);
        }
    }

    private static void AddEllipsoid(
        Vector3 center,
        Vector3 radii,
        int latSegments,
        int lonSegments,
        Matrix4x4 transform,
        Matrix4x4 normalMatrix,
        Vector3 color,
        List<TerrainVertex> vertices,
        List<uint> indices)
    {
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

    private static Vector3 TransformNormalSafe(Vector3 normal, Matrix4x4 normalMatrix)
    {
        var transformed = Vector3.TransformNormal(normal, normalMatrix);
        return transformed.LengthSquared() < 0.000001f
            ? Vector3.UnitY
            : Vector3.Normalize(transformed);
    }
}
