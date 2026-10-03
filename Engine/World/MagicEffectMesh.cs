using System.Numerics;
using SlavicGame.Engine.Magic;

namespace SlavicGame.Engine.World;

public static class MagicEffectMesh
{
    public static void Append(WorldState world, ref TerrainVertex[] vertices, ref uint[] indices)
    {
        var ritualVisible = world.Rituals.IsPerforming || world.Rituals.CompletionGlowRemaining > 0;
        if (world.Magic.FlashRemaining <= 0 &&
            world.Magic.RevealRemaining <= 0 &&
            !ritualVisible)
            return;

        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);

        if (world.Magic.FlashRemaining > 0)
            AddDiamond(world.Magic.FlashPosition, world.Magic.FlashColor, 0.45f);

        if (world.Magic.RevealRemaining > 0)
        {
            foreach (var track in world.Progress.Tracking.Tracks
                         .Where(t => t.MagicSignature is not null)
                         .Take(128))
            {
                var point = new Vector3(track.X, track.Y, track.Z);
                if (Vector3.Distance(world.PlayerPosition, point) <= 22f &&
                    SpellCasting.HasLineOfSight(
                        world,
                        world.PlayerPosition + Vector3.UnitY * 1.5f,
                        point))
                {
                    AddDiamond(point, new Vector3(0.2f, 0.75f, 1f), 0.35f);
                }
            }
        }

        if (ritualVisible)
            AddRitualVisuals();

        vertices = output.ToArray();
        indices = triangles.ToArray();

        void AddRitualVisuals()
        {
            var ritual = world.Rituals;
            var center = ritual.VisualOrigin + Vector3.UnitY * 0.08f;
            var completion = ritual.CompletionGlowRemaining > 0;
            var color = completion
                ? new Vector3(0.55f, 0.92f, 1f)
                : RitualColor(ritual.CurrentStep);

            const int ringSegments = 24;
            var visibleFraction = completion
                ? 1f
                : Math.Clamp((float)ritual.OverallProgress + 0.08f, 0.08f, 1f);
            var visibleSegments = Math.Clamp(
                (int)MathF.Ceiling(ringSegments * visibleFraction),
                2,
                ringSegments);

            AddRing(center, 2.8f, 3.05f, ringSegments, visibleSegments, color);

            if (completion || ritual.CurrentStep >= RitualStep.PlaceAnchor)
                AddDiamond(center + Vector3.UnitY * 0.55f, color, completion ? 0.65f : 0.38f);

            if (completion || ritual.CurrentStep >= RitualStep.RecognitionSign)
            {
                AddGroundStrip(
                    center + new Vector3(-1.65f, 0, 0),
                    center + new Vector3(1.65f, 0, 0),
                    0.09f,
                    color);
                AddGroundStrip(
                    center + new Vector3(0, 0, -1.65f),
                    center + new Vector3(0, 0, 1.65f),
                    0.09f,
                    color);
            }

            if (completion || ritual.CurrentStep >= RitualStep.AwaitReaction)
            {
                for (var i = 0; i < 4; i++)
                {
                    var angle = i * MathF.PI * 0.5f;
                    var point = center + new Vector3(MathF.Cos(angle), 0.35f, MathF.Sin(angle)) * 2.15f;
                    AddDiamond(point, color, 0.22f);
                }
            }

            if (completion || ritual.CurrentStep >= RitualStep.CloseBond)
            {
                AddGroundStrip(
                    center + new Vector3(-1.15f, 0, -1.15f),
                    center + new Vector3(1.15f, 0, 1.15f),
                    0.075f,
                    color);
                AddGroundStrip(
                    center + new Vector3(-1.15f, 0, 1.15f),
                    center + new Vector3(1.15f, 0, -1.15f),
                    0.075f,
                    color);
            }

            if (completion)
            {
                var strength = (float)Math.Clamp(ritual.CompletionGlowRemaining / 2.0, 0.0, 1.0);
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * MathF.Tau / 8f;
                    var radius = 3.35f + (1f - strength) * 0.35f;
                    var point = center + new Vector3(MathF.Cos(angle) * radius, 0.55f, MathF.Sin(angle) * radius);
                    AddDiamond(point, color, 0.18f + 0.12f * strength);
                }
            }
        }

        void AddRing(
            Vector3 center,
            float innerRadius,
            float outerRadius,
            int segments,
            int visibleSegments,
            Vector3 color)
        {
            for (var i = 0; i < visibleSegments; i++)
            {
                var a0 = i * MathF.Tau / segments;
                var a1 = (i + 1) * MathF.Tau / segments;
                var start = (uint)output.Count;

                output.Add(new TerrainVertex(
                    Ground(center, MathF.Cos(a0) * innerRadius, MathF.Sin(a0) * innerRadius),
                    color,
                    Vector3.UnitY));
                output.Add(new TerrainVertex(
                    Ground(center, MathF.Cos(a0) * outerRadius, MathF.Sin(a0) * outerRadius),
                    color,
                    Vector3.UnitY));
                output.Add(new TerrainVertex(
                    Ground(center, MathF.Cos(a1) * outerRadius, MathF.Sin(a1) * outerRadius),
                    color,
                    Vector3.UnitY));
                output.Add(new TerrainVertex(
                    Ground(center, MathF.Cos(a1) * innerRadius, MathF.Sin(a1) * innerRadius),
                    color,
                    Vector3.UnitY));

                triangles.AddRange(
                    [start, start + 1, start + 2, start, start + 2, start + 3]);
            }
        }

        void AddGroundStrip(Vector3 from, Vector3 to, float halfWidth, Vector3 color)
        {
            var direction = to - from;
            direction.Y = 0;
            if (direction.LengthSquared() < 0.0001f) return;
            direction = Vector3.Normalize(direction);
            var side = new Vector3(-direction.Z, 0, direction.X) * halfWidth;

            var a = OnGround(from + side);
            var b = OnGround(from - side);
            var c = OnGround(to - side);
            var d = OnGround(to + side);
            var start = (uint)output.Count;

            output.Add(new TerrainVertex(a, color, Vector3.UnitY));
            output.Add(new TerrainVertex(b, color, Vector3.UnitY));
            output.Add(new TerrainVertex(c, color, Vector3.UnitY));
            output.Add(new TerrainVertex(d, color, Vector3.UnitY));
            triangles.AddRange(
                [start, start + 1, start + 2, start, start + 2, start + 3]);
        }

        Vector3 Ground(Vector3 center, float xOffset, float zOffset) =>
            OnGround(center + new Vector3(xOffset, 0, zOffset));

        Vector3 OnGround(Vector3 point)
        {
            point.Y = world.Terrain.SampleHeight(point) + 0.075f;
            return point;
        }

        void AddDiamond(Vector3 p, Vector3 color, float size)
        {
            var start = (uint)output.Count;
            foreach (var offset in new[]
                     {
                         Vector3.UnitY,
                         -Vector3.UnitY,
                         Vector3.UnitX,
                         Vector3.UnitZ,
                         -Vector3.UnitX,
                         -Vector3.UnitZ
                     })
            {
                output.Add(new TerrainVertex(p + offset * size, color, offset));
            }

            for (uint i = 0; i < 4; i++)
            {
                uint a = start + 2 + i;
                uint b = start + 2 + (i + 1) % 4;
                triangles.AddRange([start, a, b, start + 1, b, a]);
            }
        }
    }

    private static Vector3 RitualColor(RitualStep step) => step switch
    {
        RitualStep.DefineArea => new Vector3(0.45f, 0.60f, 0.78f),
        RitualStep.PlaceAnchor => new Vector3(0.50f, 0.68f, 0.88f),
        RitualStep.RecognitionSign => new Vector3(0.55f, 0.76f, 0.95f),
        RitualStep.AwaitReaction => new Vector3(0.48f, 0.70f, 1.00f),
        RitualStep.ConfirmIdentity => new Vector3(0.62f, 0.78f, 1.00f),
        RitualStep.CloseBond => new Vector3(0.72f, 0.82f, 1.00f),
        RitualStep.ObserveResult => new Vector3(0.78f, 0.90f, 1.00f),
        _ => new Vector3(0.55f, 0.75f, 1.00f)
    };
}
