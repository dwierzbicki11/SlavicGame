using System.Numerics;

namespace SlavicGame.Engine.World;

public enum BestiarySpecies
{
    Topielec,
    Poludnica,
    Upior,
    Boginka,
    ForestGuardian
}

public sealed record BestiaryActor(
    string Id,
    BestiarySpecies Species,
    string DisplayName,
    string ModelFile,
    Vector3 Position,
    float YawRadians,
    Vector3 FacingDirection,
    string AnimationClip,
    float ClipTime,
    float Health,
    float MaxHealth,
    bool IsAlive,
    bool IsAttackWindingUp,
    bool Looted,
    string TrophyItemId);

public sealed class BestiaryWorldRuntime
{
    private sealed class MonsterState
    {
        public required string Id { get; init; }
        public required BestiarySpecies Species { get; init; }
        public required string DisplayName { get; init; }
        public required string ModelFile { get; init; }
        public required string TrophyItemId { get; init; }
        public Vector3 HomePosition { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 FacingDirection { get; set; } = Vector3.UnitX;
        public float YawRadians { get; set; }
        public float MaxHealth { get; init; }
        public float Health { get; set; }
        public bool Looted { get; set; }
        public double AlertRemaining { get; set; }
        public double HitReactionRemaining { get; set; }
        public double AttackCooldownRemaining { get; set; }
        public double AttackWindupRemaining { get; set; }
        public string Clip { get; set; } = "Idle";
        public float ClipTime { get; set; }
        public float DetectionRange { get; init; } = 14f;
        public float AttackRange { get; init; } = 1.6f;
        public float MoveSpeed { get; init; } = 2.4f;
        public float Damage { get; init; } = 10f;
    }

    private readonly List<MonsterState> _monsters = [];
    private readonly List<BestiaryActor> _actors = [];

    public IReadOnlyList<BestiaryActor> Actors => _actors;

    public BestiaryWorldRuntime()
    {
        InitializeMonsters();
        RebuildActors();
    }

    public void Reset(WorldState? world = null)
    {
        _monsters.Clear();
        InitializeMonsters();
        if (world is not null)
        {
            foreach (var m in _monsters)
            {
                var p = m.Position;
                p.Y = world.Terrain.SampleHeight(p);
                m.Position = p;
                m.HomePosition = p;
            }
        }
        RebuildActors();
    }

    private void RebuildActors()
    {
        _actors.Clear();
        foreach (var m in _monsters)
        {
            _actors.Add(BuildActor(m));
        }
    }

    private void InitializeMonsters()
    {
        _monsters.Add(new MonsterState
        {
            Id = "topielec-swamp",
            Species = BestiarySpecies.Topielec,
            DisplayName = "Topielec z Bystrzycy",
            ModelFile = "topielec_animated.glb",
            TrophyItemId = "pazur-topielca",
            HomePosition = new Vector3(115f, 0f, 18f),
            Position = new Vector3(115f, 0f, 18f),
            MaxHealth = 55f,
            Health = 55f,
            Damage = 8f,
            MoveSpeed = 2.5f
        });

        _monsters.Add(new MonsterState
        {
            Id = "poludnica-fields",
            Species = BestiarySpecies.Poludnica,
            DisplayName = "Poludnica z Zytnich Lanow",
            ModelFile = "poludnica_animated.glb",
            TrophyItemId = "wieniec-poludnicy",
            HomePosition = new Vector3(-52f, 0f, -88f),
            Position = new Vector3(-52f, 0f, -88f),
            MaxHealth = 50f,
            Health = 50f,
            Damage = 11f,
            MoveSpeed = 2.7f
        });

        _monsters.Add(new MonsterState
        {
            Id = "upior-ruins",
            Species = BestiarySpecies.Upior,
            DisplayName = "Upior z Pradawnego Kurhanu",
            ModelFile = "upior_animated.glb",
            TrophyItemId = "pyl-upiora",
            HomePosition = new Vector3(-96f, 0f, 70f),
            Position = new Vector3(-96f, 0f, 70f),
            MaxHealth = 65f,
            Health = 65f,
            Damage = 12f,
            MoveSpeed = 2.3f
        });

        _monsters.Add(new MonsterState
        {
            Id = "boginka-waters",
            Species = BestiarySpecies.Boginka,
            DisplayName = "Boginka z Brzozowych Legow",
            ModelFile = "boginka_animated.glb",
            TrophyItemId = "lza-boginki",
            HomePosition = new Vector3(125f, 0f, -140f),
            Position = new Vector3(125f, 0f, -140f),
            MaxHealth = 50f,
            Health = 50f,
            Damage = 9f,
            MoveSpeed = 2.6f
        });

        _monsters.Add(new MonsterState
        {
            Id = "forest-guardian-perun",
            Species = BestiarySpecies.ForestGuardian,
            DisplayName = "Leszy z Boru Perunowego",
            ModelFile = "forest_guardian_animated.glb",
            TrophyItemId = "kora-leszego",
            HomePosition = new Vector3(-380f, 0f, -180f),
            Position = new Vector3(-380f, 0f, -180f),
            MaxHealth = 120f,
            Health = 120f,
            Damage = 16f,
            AttackRange = 2.2f,
            MoveSpeed = 2.0f
        });
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);

        var dt = (float)Math.Clamp(deltaSeconds, 0.0, 0.1);
        _actors.Clear();

        foreach (var m in _monsters)
        {
            if (m.Position.Y == 0f)
            {
                var groundPos = m.Position;
                groundPos.Y = world.Terrain.SampleHeight(groundPos);
                m.Position = groundPos;
            }

            if (m.Health <= 0f)
            {
                m.Clip = "Death";
                m.ClipTime = 1.0f;
                _actors.Add(BuildActor(m));
                continue;
            }

            if (m.HitReactionRemaining > 0.0)
            {
                m.HitReactionRemaining = Math.Max(0.0, m.HitReactionRemaining - deltaSeconds);
                m.Clip = "Hit";
                m.ClipTime = (float)(1.0 - m.HitReactionRemaining / 0.25);
                _actors.Add(BuildActor(m));
                continue;
            }

            if (m.AttackCooldownRemaining > 0.0)
            {
                m.AttackCooldownRemaining = Math.Max(0.0, m.AttackCooldownRemaining - deltaSeconds);
            }

            var deltaToPlayer = world.PlayerPosition - m.Position;
            deltaToPlayer.Y = 0f;
            var distToPlayer = deltaToPlayer.Length();

            if (distToPlayer > 0.01f)
            {
                m.FacingDirection = Vector3.Normalize(deltaToPlayer);
                m.YawRadians = MathF.Atan2(m.FacingDirection.X, m.FacingDirection.Z);
            }

            if (m.AttackWindupRemaining > 0.0)
            {
                m.AttackWindupRemaining = Math.Max(0.0, m.AttackWindupRemaining - deltaSeconds);
                m.Clip = "Attack";
                m.ClipTime = (float)(1.0 - m.AttackWindupRemaining / 0.45);

                if (m.AttackWindupRemaining <= 0.0)
                {
                    // Attack lands
                    if (distToPlayer <= m.AttackRange + 0.5f && world.Player.IsAlive)
                    {
                        world.Player.TakeDamage(m.Damage);
                    }
                    m.AttackCooldownRemaining = 1.5;
                }

                _actors.Add(BuildActor(m));
                continue;
            }

            if (distToPlayer <= m.AttackRange && m.AttackCooldownRemaining <= 0.0 && world.Player.IsAlive)
            {
                m.AttackWindupRemaining = 0.45;
                m.Clip = "Attack";
                m.ClipTime = 0f;
                _actors.Add(BuildActor(m));
                continue;
            }

            var distFromHome = Vector2.Distance(
                new Vector2(m.Position.X, m.Position.Z),
                new Vector2(m.HomePosition.X, m.HomePosition.Z));

            if (distToPlayer <= m.DetectionRange && distFromHome < 35f && world.Player.IsAlive)
            {
                // Chase player
                var moveStep = m.FacingDirection * (m.MoveSpeed * dt);
                var newPos = m.Position + moveStep;
                newPos.Y = world.Terrain.SampleHeight(newPos);
                m.Position = newPos;
                m.Clip = "Run";
                m.ClipTime = (m.ClipTime + dt * 1.8f) % 2.0f;
            }
            else if (distFromHome > 2.0f)
            {
                // Return home
                var toHome = m.HomePosition - m.Position;
                toHome.Y = 0f;
                var dir = Vector3.Normalize(toHome);
                m.FacingDirection = dir;
                m.YawRadians = MathF.Atan2(dir.X, dir.Z);
                var moveStep = dir * (m.MoveSpeed * 0.7f * dt);
                var newPos = m.Position + moveStep;
                newPos.Y = world.Terrain.SampleHeight(newPos);
                m.Position = newPos;
                m.Clip = "Walk";
                m.ClipTime = (m.ClipTime + dt * 1.2f) % 2.0f;
            }
            else
            {
                m.Clip = "Idle";
                m.ClipTime = (m.ClipTime + dt * 0.8f) % 3.0f;
            }

            _actors.Add(BuildActor(m));
        }
    }

    private static BestiaryActor BuildActor(MonsterState m) =>
        new(
            m.Id,
            m.Species,
            m.DisplayName,
            m.ModelFile,
            m.Position,
            m.YawRadians,
            m.FacingDirection,
            m.Clip,
            m.ClipTime,
            m.Health,
            m.MaxHealth,
            m.Health > 0f,
            m.AttackWindupRemaining > 0.0,
            m.Looted,
            m.TrophyItemId);

    public bool TryDamage(WorldState world, string id, float damage)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var m = _monsters.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
        if (m is null || m.Health <= 0f)
            return false;

        m.Health = MathF.Max(0f, m.Health - damage);
        if (m.Health <= 0f)
        {
            m.Clip = "Death";
            m.ClipTime = 1.0f;
        }
        else
        {
            m.HitReactionRemaining = 0.25;
            m.AttackWindupRemaining = 0.0;
            m.Clip = "Hit";
            m.ClipTime = 0f;
        }
        RebuildActors();
        return true;
    }

    public BestiaryActor? FindNearestHarvestable(Vector3 playerPosition, float maxDistance)
    {
        var maxDistSq = maxDistance * maxDistance;
        return _actors
            .Where(a => !a.IsAlive && !a.Looted)
            .Select(a => (Actor: a, DistSq: Vector3.DistanceSquared(playerPosition, a.Position)))
            .Where(pair => pair.DistSq <= maxDistSq)
            .OrderBy(pair => pair.DistSq)
            .Select(pair => pair.Actor)
            .FirstOrDefault();
    }

    public bool TryHarvest(WorldState world, string id)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var m = _monsters.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
        if (m is null || m.Health > 0f || m.Looted)
            return false;

        m.Looted = true;
        world.Progress.Inventory.Add(m.TrophyItemId, 1);

        var recipeFlag = m.Species switch
        {
            BestiarySpecies.Topielec => "recipe.drowned-tallow.learned",
            BestiarySpecies.Poludnica => "recipe.noonwraith-salve.learned",
            BestiarySpecies.Upior => "recipe.specter-oil.learned",
            BestiarySpecies.Boginka => "recipe.boginka-vitality-tonic.learned",
            BestiarySpecies.ForestGuardian => "recipe.leshy-protection-charm.learned",
            _ => null
        };

        if (recipeFlag is not null)
        {
            world.Progress.SetFlag(recipeFlag);
        }

        RebuildActors();
        return true;
    }
}
