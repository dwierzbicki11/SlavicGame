using SlavicGame.Engine.World;

internal static class RiverbankPropRegression
{
    public static void Run(Action<bool, string> check)
    {
        check(RiverbankPropGenerator.Generate(new Terrain()).Count == 0,
            "Riverbank models are absent when the river lies outside the terrain");

        var fullTerrain = new Terrain(513, 513, 4f);
        var full = RiverbankPropGenerator.Generate(fullTerrain);
        check(full.Count == 116, "Full map retains both banks of the river");

        var shortTerrain = new Terrain(513, 65, 4f);
        var shortMap = RiverbankPropGenerator.Generate(shortTerrain);
        check(shortMap.Count > 0 && shortMap.Count < full.Count,
            "Short terrain retains only its visible riverbank models");
        check(shortMap.All(model =>
                MathF.Abs(model.Position.X) <= 1024f &&
                MathF.Abs(model.Position.Z) <= 128f),
            "Riverbank model origins remain inside rectangular terrain bounds");
        check(shortMap.All(model => full.Any(original =>
                original.Position.X == model.Position.X &&
                original.Position.Z == model.Position.Z &&
                original.Scale == model.Scale &&
                original.YawRadians == model.YawRadians)),
            "Clipping preserves deterministic riverbank placement and appearance");
    }
}
