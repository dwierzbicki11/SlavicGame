namespace SlavicGame.Engine.World;

public static class WorldGenerator
{
    public static WorldState Generate()
    {
        var world = new WorldState();
        world.Initialize();
        VerticalSliceBootstrap.Apply(world);
        return world;
    }
}
