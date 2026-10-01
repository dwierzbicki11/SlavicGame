using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed class WorldState
{
    private readonly List<WorldRegion> _regions = [];

    public IReadOnlyList<WorldRegion> Regions => _regions;
    public Vector3 PlayerPosition { get; private set; } = new(0f, 0f, 0f);
    public WorldTime Time { get; } = new();

    public string CurrentRegion { get; private set; } = "starting-forest";

    public void Initialize()
    {
        _regions.Clear();

        _regions.Add(new WorldRegion(
            "starting-forest", "Puszcza Żywia", WorldRegionType.Forest, 0f, 0f, 55f));

        _regions.Add(new WorldRegion(
            "old-village", "Żarnowiec", WorldRegionType.Village, 0f, -85f, 32f));

        _regions.Add(new WorldRegion(
            "black-swamp", "Czarne Mokradła", WorldRegionType.Swamp, 95f, 35f, 48f));

        _regions.Add(new WorldRegion(
            "old-shrine", "Kamienny Krąg", WorldRegionType.Shrine, -85f, 55f, 22f));

        UpdateRegion();
    }

    public void Update(double deltaSeconds)
    {
        Time.Update(deltaSeconds);
        UpdateRegion();
    }

    public void SetPlayerPosition(Vector3 position)
    {
        PlayerPosition = position;
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        foreach (var region in _regions)
        {
            if (region.Contains(PlayerPosition.X, PlayerPosition.Z))
            {
                CurrentRegion = region.Id;
                return;
            }
        }

        CurrentRegion = "wilderness";
    }

    public WorldRegion? GetCurrentRegion()
    {
        return _regions.FirstOrDefault(r => r.Id == CurrentRegion);
    }
}
