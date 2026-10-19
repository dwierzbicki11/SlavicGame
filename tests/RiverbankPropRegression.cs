using SlavicGame.Engine.World;

internal static class RiverbankPropRegression
{
    public static void Run(Action<bool, string> check)
    {
        var fullTerrain = new Terrain(513, 513, 4f);
        var full = RiverbankPropGenerator.Generate(fullTerrain);
        check(full.Count == 116, "Full R0 terrain retains all riverbank rocks");

        var small = RiverbankPropGenerator.Generate(new Terrain(128, 128, 2f));
        check(small.Count == 0,
            "Terrain that does not reach the river has no riverbank rocks");

        var clipped = RiverbankPropGenerator.Generate(new Terrain(241, 81, 2f));
        check(clipped.Count > 0 && clipped.Count < full.Count,
            "Partial river terrain retains its in-bounds riverbank rocks");
        check(clipped.All(rock => MathF.Abs(rock.Position.X) <= 240f &&
                                  MathF.Abs(rock.Position.Z) <= 80f),
            "Riverbank rock positions respect both terrain dimensions");
        check(clipped.All(rock => full.Any(original =>
                original.Id == rock.Id &&
                original.Position.X == rock.Position.X &&
                original.Position.Z == rock.Position.Z &&
                original.Scale == rock.Scale &&
                original.YawRadians == rock.YawRadians)),
            "Clipping preserves riverbank rock identities and seeded placement");
        check(clipped.SequenceEqual(RiverbankPropGenerator.Generate(new Terrain(241, 81, 2f))),
            "Riverbank generation remains deterministic on partial terrain");
    }
}
