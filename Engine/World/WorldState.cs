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
    private readonly List<WorldModelInstance> _models = [];

    public IReadOnlyList<WorldRegion> Regions => _regions;
    public IReadOnlyList<WorldObstacle> Obstacles => _obstacles;
    public IReadOnlyList<EnemyAgent> Enemies => _enemies;
    public IReadOnlyList<NpcDefinition> Npcs => _npcs;
    public IReadOnlyList<WorldModelInstance> Models => _models;
    public Vector3 PlayerPosition { get; private set; } = Vector3.Zero;
    public WorldTime Time { get; } = new();
    public WeatherSystem Weather { get; } = new();
    public PlayerVitals Player { get; } = new();
    public GameProgress Progress { get; } = new();
    public CosmologyState Cosmology { get; } = new();
    // ~1 km x 1 km terrain: 16x the area of the original R0 prototype.
    // The current R0 locations occupy only the central playable slice and can expand outward.
    public Terrain Terrain { get; } = new(257, 257, 4f);
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

        _models.Clear();
        AddModel("village-hut-a", "models/static/chata_r0_variant_01.glb", -10f, -88f, new Vector3(1.35f), 0.18f, new Vector3(0.42f, 0.27f, 0.12f));
        AddModel("village-hut-b", "models/static/chata_r0_variant_02.glb", 5f, -96f, new Vector3(1.35f), -0.35f, new Vector3(0.39f, 0.24f, 0.11f));
        AddModel("village-hut-c", "models/static/chata_r0_variant_03.glb", 13f, -78f, new Vector3(1.30f), 0.62f, new Vector3(0.44f, 0.29f, 0.13f));
        AddModel("village-granary", "models/static/spichlerz_r0_01.glb", -18f, -80f, new Vector3(1.15f), -0.15f, new Vector3(0.40f, 0.25f, 0.10f));
        AddModel("village-forge", "models/static/kuznia_r0_01.glb", 21f, -91f, new Vector3(1.15f), 0.40f, new Vector3(0.34f, 0.20f, 0.09f));
        AddModel("village-market", "models/static/stoisko_targowe_r0_01.glb", -1f, -76f, new Vector3(1.20f), 0f, new Vector3(0.48f, 0.31f, 0.15f));
        AddModel("village-gate", "models/static/brama_wioskowa_r0_01.glb", 0f, -108f, new Vector3(1.20f), 0f, new Vector3(0.36f, 0.22f, 0.09f));

        AddModel("forest-oak-a", "models/static/dab_stary_r0_01.glb", -14f, 11f, Vector3.One, 0.2f, new Vector3(0.20f, 0.35f, 0.13f));
        AddModel("forest-oak-b", "models/static/dab_stary_r0_02.glb", 12f, 7f, Vector3.One, -0.4f, new Vector3(0.18f, 0.32f, 0.12f));
        AddModel("forest-pine", "models/static/sosna_r0_02.glb", -21f, -4f, Vector3.One, 0.5f, new Vector3(0.16f, 0.30f, 0.11f));
        AddModel("forest-birch", "models/static/brzoza_r0_02.glb", 22f, -8f, Vector3.One, -0.3f, new Vector3(0.28f, 0.43f, 0.18f));
        AddModel("forest-ferns", "models/static/paproc_r0_01.glb", 7f, 13f, new Vector3(1.4f), 0f, new Vector3(0.20f, 0.40f, 0.14f));
        AddModel("forest-fallen-trunk", "models/static/powalone_drzewo_r0_01.glb", 18f, 15f, new Vector3(1.4f), 0.15f, new Vector3(0.28f, 0.17f, 0.07f));

        AddModel("shrine-circle", "models/static/kamienny_krag_fragment_r0_01.glb", -85f, 55f, new Vector3(2.4f), 0f, new Vector3(0.36f, 0.37f, 0.34f));
        AddModel("shrine-altar", "models/static/oltar_rytualny_r0_01.glb", -85f, 55f, new Vector3(1.25f), 0f, new Vector3(0.42f, 0.42f, 0.39f));
        AddModel("shrine-ruin", "models/static/kapliczka_ruina_r0_01.glb", -77f, 61f, new Vector3(1.15f), 0.35f, new Vector3(0.34f, 0.31f, 0.25f));

        AddModel("swamp-standing-stone", "models/static/znak_bagienny_r0_01.glb", 105f, 44f, new Vector3(1.3f), 0.25f, new Vector3(0.28f, 0.32f, 0.27f));
        AddModel("swamp-reeds-a", "models/static/trzciny_r0_01.glb", 88f, 30f, new Vector3(1.8f), 0f, new Vector3(0.32f, 0.38f, 0.17f));
        AddModel("swamp-reeds-b", "models/static/trzciny_r0_02.glb", 98f, 25f, new Vector3(1.5f), 0.6f, new Vector3(0.30f, 0.36f, 0.16f));
        AddModel("swamp-boardwalk", "models/static/kladka_bagienna_03.glb", 91f, 41f, new Vector3(1.4f), 0.3f, new Vector3(0.35f, 0.22f, 0.10f));
        AddModel("swamp-stump", "models/static/pien_bagienny_r0_01.glb", 108f, 31f, new Vector3(1.2f), 0f, new Vector3(0.22f, 0.28f, 0.12f));

        _models.AddRange(WorldDecorationGenerator.Generate(Terrain));

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

    private void AddModel(
        string id,
        string assetPath,
        float x,
        float z,
        Vector3 scale,
        float yawRadians,
        Vector3 color,
        float yOffset = 0f)
    {
        var position = new Vector3(x, 0f, z);
        position.Y = Terrain.SampleHeight(position) + yOffset;
        _models.Add(new WorldModelInstance(id, assetPath, position, scale, yawRadians, color));
    }

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
