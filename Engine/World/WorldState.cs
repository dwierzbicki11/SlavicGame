using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed class WorldState
{
    private readonly List<WorldRegion> _regions = [];

    public IReadOnlyList<WorldRegion> Regions => _regions;
    public Vector3 PlayerPosition { get; private set; } = Vector3.Zero;
    public WorldTime Time { get; } = new();
    public Terrain Terrain { get; } = new();
    public string CurrentRegion { get; private set; } = "starting-forest";

    public void Initialize()
    {
        _regions.Clear();
        _regions.Add(new WorldRegion("starting-forest", "Puszcza Żywia", WorldRegionType.Forest, 0f, 0f, 55f));
        _regions.Add(new WorldRegion("old-village", "Żarnowiec", WorldRegionType.Village, 0f, -85f, 32f));
        _regions.Add(new WorldRegion("black-swamp", "Czarne Mokradła", WorldRegionType.Swamp, 95f, 35f, 48f));
        _regions.Add(new WorldRegion("old-shrine", "Kamienny Krąg", WorldRegionType.Shrine, -85f, 55f, 22f));
        SetPlayerPosition(Vector3.Zero);
    }

    public void Update(double deltaSeconds)
    {
        Time.Update(deltaSeconds);
        SetPlayerPosition(PlayerPosition);
    }

    public void SetPlayerPosition(Vector3 position)
    {
        PlayerPosition = new Vector3(position.X, Terrain.SampleHeight(position), position.Z);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        foreach (var region in _regions)
            if (region.Contains(PlayerPosition.X, PlayerPosition.Z))
            {
                CurrentRegion = region.Id;
                return;
            }

        CurrentRegion = "wilderness";
    }

    public WorldRegion? GetCurrentRegion() => _regions.FirstOrDefault(r => r.Id == CurrentRegion);
}
