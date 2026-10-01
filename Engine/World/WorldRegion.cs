namespace SlavicGame.Engine.World;

public enum WorldRegionType
{
    Forest,
    Village,
    Swamp,
    Shrine
}

public sealed class WorldRegion
{
    public string Id { get; }
    public string Name { get; }
    public WorldRegionType Type { get; }
    public float CenterX { get; }
    public float CenterZ { get; }
    public float Radius { get; }

    public WorldRegion(string id, string name, WorldRegionType type, float centerX, float centerZ, float radius)
    {
        Id = id;
        Name = name;
        Type = type;
        CenterX = centerX;
        CenterZ = centerZ;
        Radius = radius;
    }

    public bool Contains(float x, float z)
    {
        var dx = x - CenterX;
        var dz = z - CenterZ;
        return dx * dx + dz * dz <= Radius * Radius;
    }
}
