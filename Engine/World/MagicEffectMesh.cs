using System.Numerics;
using SlavicGame.Engine.Magic;

namespace SlavicGame.Engine.World;

public static class MagicEffectMesh
{
    public static void Append(WorldState world, ref TerrainVertex[] vertices, ref uint[] indices)
    {
        if (world.Magic.FlashRemaining <= 0 && world.Magic.RevealRemaining <= 0) return;
        var output = new List<TerrainVertex>(vertices);
        var triangles = new List<uint>(indices);
        if (world.Magic.FlashRemaining > 0)
            AddDiamond(world.Magic.FlashPosition, world.Magic.FlashColor, 0.45f);
        if (world.Magic.RevealRemaining > 0)
        foreach (var track in world.Progress.Tracking.Tracks.Where(t => t.MagicSignature is not null).Take(128))
        {
            var point = new Vector3(track.X, track.Y, track.Z);
            if (Vector3.Distance(world.PlayerPosition, point) <= 22f &&
                SpellCasting.HasLineOfSight(world, world.PlayerPosition + Vector3.UnitY * 1.5f, point))
                AddDiamond(point, new Vector3(0.2f, 0.75f, 1f), 0.35f);
        }
        vertices = output.ToArray(); indices = triangles.ToArray();
        void AddDiamond(Vector3 p, Vector3 color, float size)
        {
            var start = (uint)output.Count;
            foreach (var offset in new[] { Vector3.UnitY, -Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ, -Vector3.UnitX, -Vector3.UnitZ })
                output.Add(new TerrainVertex(p + offset * size, color, offset));
            for (uint i = 0; i < 4; i++)
            {
                uint a = start + 2 + i, b = start + 2 + (i + 1) % 4;
                triangles.AddRange(new[] { start, a, b, start + 1, b, a });
            }
        }
    }
}
