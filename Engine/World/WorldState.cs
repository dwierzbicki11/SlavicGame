using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.NPC;

namespace SlavicGame.Engine.World;

public sealed class WorldState
{
    private readonly List<WorldRegion> _regions = [];
    private readonly List<WorldObstacle> _obstacles = [];
    private readonly List<EnemyAgent> _enemies = [];
    private readonly List<NpcDefinition> _npcs = [];

    public IReadOnlyList<WorldRegion> Regions => _regions;
    public IReadOnlyList<WorldObstacle> Obstacles => _obstacles;
    public IReadOnlyList<EnemyAgent> Enemies => _enemies;
    public IReadOnlyList<NpcDefinition> Npcs => _npcs;
    public Vector3 PlayerPosition { get; private set; } = Vector3.Zero;
    public WorldTime Time { get; } = new();
    public WeatherSystem Weather { get; } = new();
    public PlayerVitals Player { get; } = new();
    public GameProgress Progress { get; } = new();
    public CosmologyState Cosmology { get; } = new();
    public Terrain Terrain { get; } = new();
    public float PlayerRadius { get; } = 0.55f;
    public string CurrentRegion { get; private set; } = "starting-forest";

    public void Initialize()
    {
        _regions.Clear();
        _regions.Add(new WorldRegion("starting-forest", "Puszcza Żywia", WorldRegionType.Forest, 0f, 0f, 55f));
        _regions.Add(new WorldRegion("old-village", "Żarnowiec", WorldRegionType.Village, 0f, -85f, 32f));
        _regions.Add(new WorldRegion("black-swamp", "Czarne Mokradła", WorldRegionType.Swamp, 95f, 35f, 48f));
        _regions.Add(new WorldRegion("old-shrine", "Kamienny Krąg", WorldRegionType.Shrine, -85f, 55f, 22f));

        _obstacles.Clear();
        AddObstacle("village-hut-a", -10f, -88f, 8f, 7f, 5f, new Vector3(0.32f, 0.20f, 0.10f));
        AddObstacle("village-hut-b", 5f, -96f, 9f, 6f, 5.5f, new Vector3(0.29f, 0.18f, 0.09f));
        AddObstacle("village-hut-c", 13f, -78f, 7f, 8f, 4.8f, new Vector3(0.35f, 0.22f, 0.11f));

        var stone = new Vector3(0.30f, 0.31f, 0.28f);
        AddObstacle("shrine-stone-west", -91f, 55f, 2.2f, 2.2f, 4.2f, stone);
        AddObstacle("shrine-stone-east", -79f, 55f, 2.2f, 2.2f, 4.2f, stone);
        AddObstacle("shrine-stone-north", -85f, 49f, 2.2f, 2.2f, 4.2f, stone);
        AddObstacle("shrine-stone-south", -85f, 61f, 2.2f, 2.2f, 4.2f, stone);

        AddObstacle("forest-fallen-trunk", 18f, 15f, 8f, 2.2f, 1.2f, new Vector3(0.24f, 0.15f, 0.07f));
        AddObstacle("swamp-standing-stone", 105f, 44f, 3f, 3f, 2.2f, new Vector3(0.20f, 0.23f, 0.20f));

        _enemies.Clear();
        AddEnemy("swamp-predator", 92f, 35f);

        _npcs.Clear();
        AddNpc("missing-family", NpcRole.ContractGiver,
            new NpcScheduleSlot(6, 20, "old-village", "home-and-search"),
            new NpcScheduleSlot(20, 6, "old-village", "home"));
        AddNpc("crossing-keeper", NpcRole.CrossingKeeper,
            new NpcScheduleSlot(6, 19, "black-swamp", "maintain-crossing"),
            new NpcScheduleSlot(19, 6, "old-village", "rest"));
        AddNpc("herbalist", NpcRole.Herbalist,
            new NpcScheduleSlot(7, 18, "old-village", "trade-and-prepare"),
            new NpcScheduleSlot(18, 7, "old-village", "rest"));
        AddNpc("community-guard", NpcRole.CommunityGuard,
            new NpcScheduleSlot(6, 18, "old-village", "patrol"),
            new NpcScheduleSlot(18, 6, "old-village", "night-watch"));
        AddNpc("shrine-keeper", NpcRole.ShrineKeeper,
            new NpcScheduleSlot(7, 19, "old-shrine", "tend-shrine"),
            new NpcScheduleSlot(19, 7, "old-village", "rest"));

        SetPlayerPosition(Vector3.Zero);
    }

    public void Update(double deltaSeconds)
    {
        Time.Update(deltaSeconds);
        SetPlayerPosition(PlayerPosition);
        Weather.Update(deltaSeconds, GetCurrentRegion()?.Type);
        foreach (var enemy in _enemies)
        {
            enemy.Update(this, deltaSeconds);
        }
    }

    public void SetPlayerPosition(Vector3 position)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Z))
            throw new ArgumentOutOfRangeException(nameof(position));
        var horizontal = ResolveHorizontalPosition(new Vector2(position.X, position.Z), PlayerRadius);
        position.X = horizontal.X;
        position.Z = horizontal.Y;
        PlayerPosition = new Vector3(position.X, Terrain.SampleHeight(position), position.Z);
        UpdateRegion();
    }

    public Vector2 ResolveHorizontalPosition(Vector2 position, float radius)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) ||
            !float.IsFinite(radius) || radius < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        var halfWidth = (Terrain.Width - 1) * Terrain.CellSize * 0.5f;
        var halfDepth = (Terrain.Depth - 1) * Terrain.CellSize * 0.5f;
        position.X = Math.Clamp(position.X, -halfWidth, halfWidth);
        position.Y = Math.Clamp(position.Y, -halfDepth, halfDepth);

        for (var pass = 0; pass < 2; pass++)
        {
            foreach (var obstacle in _obstacles)
            {
                position = obstacle.ResolvePoint(position, radius);
            }
        }

        position.X = Math.Clamp(position.X, -halfWidth, halfWidth);
        position.Y = Math.Clamp(position.Y, -halfDepth, halfDepth);
        return position;
    }

    private void AddNpc(string id, NpcRole role, params NpcScheduleSlot[] schedule) =>
        _npcs.Add(new NpcDefinition(id, role, schedule));

    private void AddEnemy(string id, float x, float z)
    {
        var home = new Vector3(x, 0f, z);
        home.Y = Terrain.SampleHeight(home);
        _enemies.Add(new EnemyAgent(id, home));
    }

    private void AddObstacle(
        string id,
        float x,
        float z,
        float width,
        float depth,
        float height,
        Vector3 color)
    {
        var center = new Vector3(x, 0f, z);
        center.Y = Terrain.SampleHeight(center) - 0.15f;
        _obstacles.Add(new WorldObstacle(
            id,
            center,
            new Vector2(width * 0.5f, depth * 0.5f),
            height,
            color));
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
