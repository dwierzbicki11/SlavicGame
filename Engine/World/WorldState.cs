using System.Numerics;
using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.World;

public sealed class WorldState
{
    private readonly List<WorldRegion> _regions = [];

    public IReadOnlyList<WorldRegion> Regions => _regions;
    public Vector3 PlayerPosition { get; private set; } = Vector3.Zero;
    public WorldTime Time { get; } = new();
    public WeatherSystem Weather { get; } = new();
    public PlayerVitals Player { get; } = new();
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
        Weather.Update(deltaSeconds, GetCurrentRegion()?.Type);
    }

    public void SetPlayerPosition(Vector3 position)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Z))
            throw new ArgumentOutOfRangeException(nameof(position));
        var halfWidth = (Terrain.Width - 1) * Terrain.CellSize * 0.5f;
        var halfDepth = (Terrain.Depth - 1) * Terrain.CellSize * 0.5f;
        position.X = Math.Clamp(position.X, -halfWidth, halfWidth);
        position.Z = Math.Clamp(position.Z, -halfDepth, halfDepth);
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
