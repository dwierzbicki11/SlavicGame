using System.Numerics;

namespace SlavicGame.Engine.World;

public enum WildlifeSpecies
{
    Deer,
    Boar,
    Wolf,
    Raven
}

public enum WildlifeBehavior
{
    Idle,
    Wander,
    Flee,
    Fly,
    Spooked
}

public sealed record WildlifeSpawnDefinition(
    string Id,
    WildlifeSpecies Species,
    Vector2 Home,
    float WanderRadius,
    float Phase);

public sealed record WildlifeWorldActor(
    string Id,
    WildlifeSpecies Species,
    Vector3 Position,
    float YawRadians,
    WildlifeBehavior Behavior,
    bool IsMoving);

public sealed record WildlifeVisualProfile(
    string ModelFile,
    Vector3 Scale,
    Vector3 Color,
    float WanderSpeed,
    float FleeSpeed,
    float AlertDistance,
    float BodyRadius);

public static class WildlifeCatalog
{
    private static readonly IReadOnlyDictionary<WildlifeSpecies, WildlifeVisualProfile> Profiles =
        new Dictionary<WildlifeSpecies, WildlifeVisualProfile>
        {
            [WildlifeSpecies.Deer] = new(
                "deer_animated.glb",
                new Vector3(1.05f),
                new Vector3(0.43f, 0.28f, 0.15f),
                WanderSpeed: 1.15f,
                FleeSpeed: 6.1f,
                AlertDistance: 18f,
                BodyRadius: 0.55f),

            [WildlifeSpecies.Boar] = new(
                "boar_animated.glb",
                new Vector3(1.02f),
                new Vector3(0.28f, 0.21f, 0.16f),
                WanderSpeed: 0.85f,
                FleeSpeed: 4.4f,
                AlertDistance: 12f,
                BodyRadius: 0.60f),

            [WildlifeSpecies.Wolf] = new(
                "wolf_animated.glb",
                new Vector3(1.00f),
                new Vector3(0.34f, 0.35f, 0.33f),
                WanderSpeed: 1.25f,
                FleeSpeed: 5.3f,
                AlertDistance: 15f,
                BodyRadius: 0.48f),

            [WildlifeSpecies.Raven] = new(
                "raven_animated.glb",
                new Vector3(1.00f),
                new Vector3(0.075f, 0.085f, 0.09f),
                WanderSpeed: 3.5f,
                FleeSpeed: 7.5f,
                AlertDistance: 13f,
                BodyRadius: 0.20f)
        };

    public static WildlifeVisualProfile For(WildlifeSpecies species) =>
        Profiles.TryGetValue(species, out var profile)
            ? profile
            : throw new ArgumentOutOfRangeException(nameof(species));

    public static IReadOnlyCollection<string> RequiredModelFiles { get; } =
        Profiles.Values
            .Select(profile => profile.ModelFile)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(file => file, StringComparer.Ordinal)
            .ToArray();
}

public static class WildlifeLayout
{
    public static IReadOnlyList<WildlifeSpawnDefinition> Spawns { get; } =
    [
        // Dębowa Knieja
        new("deer-oak-01", WildlifeSpecies.Deer, new Vector2(-500f, 405f), 42f, 0.12f),
        new("deer-oak-02", WildlifeSpecies.Deer, new Vector2(-575f, 455f), 34f, 0.47f),
        new("boar-oak-01", WildlifeSpecies.Boar, new Vector2(-465f, 495f), 28f, 0.71f),

        // Bór Perunowy
        new("wolf-pine-01", WildlifeSpecies.Wolf, new Vector2(-520f, -405f), 44f, 0.06f),
        new("wolf-pine-02", WildlifeSpecies.Wolf, new Vector2(-590f, -455f), 38f, 0.39f),
        new("wolf-pine-03", WildlifeSpecies.Wolf, new Vector2(-485f, -500f), 36f, 0.78f),
        new("raven-pine-01", WildlifeSpecies.Raven, new Vector2(-545f, -390f), 54f, 0.31f),

        // Brzozowe Łęgi
        new("deer-birch-01", WildlifeSpecies.Deer, new Vector2(475f, 415f), 38f, 0.22f),
        new("deer-birch-02", WildlifeSpecies.Deer, new Vector2(545f, 455f), 32f, 0.58f),
        new("deer-birch-03", WildlifeSpecies.Deer, new Vector2(510f, 505f), 36f, 0.83f),
        new("raven-birch-01", WildlifeSpecies.Raven, new Vector2(520f, 395f), 48f, 0.14f),

        // Mokry Bór
        new("boar-wet-01", WildlifeSpecies.Boar, new Vector2(500f, -345f), 30f, 0.17f),
        new("boar-wet-02", WildlifeSpecies.Boar, new Vector2(565f, -385f), 32f, 0.54f),
        new("boar-wet-03", WildlifeSpecies.Boar, new Vector2(530f, -430f), 26f, 0.88f),
        new("raven-wet-01", WildlifeSpecies.Raven, new Vector2(485f, -320f), 46f, 0.65f),

        // Central vertical-slice space
        new("raven-start-01", WildlifeSpecies.Raven, new Vector2(24f, 18f), 34f, 0.25f),
        new("raven-shrine-01", WildlifeSpecies.Raven, new Vector2(-92f, 64f), 28f, 0.73f)
    ];
}

public sealed class WildlifeWorldRuntime
{
    private sealed class State(WildlifeSpawnDefinition spawn)
    {
        public WildlifeSpawnDefinition Spawn { get; } = spawn;
        public Vector3 Position;
    }

    private readonly Dictionary<string, State> _states =
        new(StringComparer.Ordinal);
    private readonly List<WildlifeWorldActor> _actors = [];
    private double _elapsedSeconds;

    public IReadOnlyList<WildlifeWorldActor> Actors => _actors;

    public void Reset(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        _states.Clear();
        _actors.Clear();
        _elapsedSeconds = 0d;

        foreach (var spawn in WildlifeLayout.Spawns)
        {
            var position = new Vector3(spawn.Home.X, 0f, spawn.Home.Y);
            var profile = WildlifeCatalog.For(spawn.Species);

            if (spawn.Species == WildlifeSpecies.Raven)
            {
                position.Y =
                    world.Terrain.SampleHeight(position) +
                    7f +
                    spawn.Phase * 3f;
            }
            else
            {
                position.Y = world.Terrain.SampleHeight(position);
            }

            _states.Add(
                spawn.Id,
                new State(spawn)
                {
                    Position = position
                });
        }

        RebuildActors(world);
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        _elapsedSeconds += Math.Min(deltaSeconds, 0.25d);
        var dt = (float)Math.Min(deltaSeconds, 0.10d);

        foreach (var state in _states.Values)
        {
            UpdateState(world, state, dt);
        }

        RebuildActors(world);
    }

    private void UpdateState(
        WorldState world,
        State state,
        float deltaSeconds)
    {
        var spawn = state.Spawn;
        var profile = WildlifeCatalog.For(spawn.Species);

        var position2 = new Vector2(
            state.Position.X,
            state.Position.Z);
        var player2 = new Vector2(
            world.PlayerPosition.X,
            world.PlayerPosition.Z);
        var toPlayer = player2 - position2;
        var playerDistance = toPlayer.Length();

        if (spawn.Species == WildlifeSpecies.Raven)
        {
            UpdateRaven(
                world,
                state,
                profile,
                playerDistance,
                deltaSeconds);
            return;
        }

        var fleeing = playerDistance < profile.AlertDistance;
        Vector2 target;

        if (fleeing && playerDistance > 0.001f)
        {
            var away = -Vector2.Normalize(toPlayer);
            target =
                position2 +
                away * MathF.Max(8f, profile.FleeSpeed * 2.2f);
        }
        else
        {
            var phase =
                spawn.Phase * MathF.Tau +
                (float)_elapsedSeconds *
                (0.11f + spawn.Phase * 0.035f);
            target =
                spawn.Home +
                new Vector2(
                    MathF.Cos(phase),
                    MathF.Sin(phase * 0.83f)) *
                spawn.WanderRadius *
                0.72f;
        }

        var homeDelta = target - spawn.Home;
        if (homeDelta.Length() > spawn.WanderRadius)
        {
            target =
                spawn.Home +
                Vector2.Normalize(homeDelta) *
                spawn.WanderRadius;
        }

        var speed = fleeing
            ? profile.FleeSpeed
            : profile.WanderSpeed;
        MoveGround(
            world,
            state,
            target,
            speed,
            profile.BodyRadius,
            deltaSeconds);
    }

    private void UpdateRaven(
        WorldState world,
        State state,
        WildlifeVisualProfile profile,
        float playerDistance,
        float deltaSeconds)
    {
        var spawn = state.Spawn;
        var spooked = playerDistance < profile.AlertDistance;

        var angularSpeed = spooked ? 0.95f : 0.42f;
        var radiusMultiplier = spooked ? 1.05f : 0.72f;
        var angle =
            spawn.Phase * MathF.Tau +
            (float)_elapsedSeconds * angularSpeed;

        var target2 =
            spawn.Home +
            new Vector2(MathF.Cos(angle), MathF.Sin(angle)) *
            spawn.WanderRadius *
            radiusMultiplier;

        var target = new Vector3(target2.X, 0f, target2.Y);
        target.Y =
            world.Terrain.SampleHeight(target) +
            (spooked ? 13f : 7.5f) +
            MathF.Sin(angle * 1.7f) * 1.3f;

        var delta = target - state.Position;
        if (delta.LengthSquared() < 0.000001f)
            return;

        var maxStep =
            (spooked ? profile.FleeSpeed : profile.WanderSpeed) *
            deltaSeconds;
        var distance = delta.Length();
        state.Position +=
            delta / distance * MathF.Min(distance, maxStep);
    }

    private static void MoveGround(
        WorldState world,
        State state,
        Vector2 target,
        float speed,
        float bodyRadius,
        float deltaSeconds)
    {
        var position2 = new Vector2(
            state.Position.X,
            state.Position.Z);
        var delta = target - position2;
        if (delta.LengthSquared() < 0.000001f)
            return;

        var distance = delta.Length();
        var step =
            Vector2.Normalize(delta) *
            MathF.Min(distance, speed * deltaSeconds);

        var resolved =
            world.ResolveHorizontalPosition(
                position2 + step,
                bodyRadius);

        state.Position = new Vector3(
            resolved.X,
            0f,
            resolved.Y);
        state.Position = state.Position with
        {
            Y = world.Terrain.SampleHeight(state.Position)
        };
    }

    private void RebuildActors(WorldState world)
    {
        _actors.Clear();

        foreach (var state in _states.Values
                     .OrderBy(item => item.Spawn.Id, StringComparer.Ordinal))
        {
            var spawn = state.Spawn;
            var profile = WildlifeCatalog.For(spawn.Species);
            var position2 = new Vector2(
                state.Position.X,
                state.Position.Z);
            var player2 = new Vector2(
                world.PlayerPosition.X,
                world.PlayerPosition.Z);
            var playerDistance =
                Vector2.Distance(position2, player2);

            var behavior = spawn.Species switch
            {
                WildlifeSpecies.Raven when playerDistance < profile.AlertDistance =>
                    WildlifeBehavior.Spooked,
                WildlifeSpecies.Raven =>
                    WildlifeBehavior.Fly,
                _ when playerDistance < profile.AlertDistance =>
                    WildlifeBehavior.Flee,
                _ =>
                    WildlifeBehavior.Wander
            };

            var forward = spawn.Species == WildlifeSpecies.Raven
                ? new Vector2(
                    -MathF.Sin(
                        spawn.Phase * MathF.Tau +
                        (float)_elapsedSeconds * 0.42f),
                    MathF.Cos(
                        spawn.Phase * MathF.Tau +
                        (float)_elapsedSeconds * 0.42f))
                : DirectionForGround(state);

            var yaw =
                forward.LengthSquared() > 0.000001f
                    ? WorldPlacementOrientation.YawFacing(
                        position2,
                        position2 + Vector2.Normalize(forward))
                    : 0f;

            _actors.Add(new WildlifeWorldActor(
                spawn.Id,
                spawn.Species,
                state.Position,
                yaw,
                behavior,
                true));
        }
    }

    private static Vector2 DirectionForGround(State state)
    {
        var spawn = state.Spawn;
        var position =
            new Vector2(state.Position.X, state.Position.Z);
        var direction = spawn.Home - position;

        if (direction.LengthSquared() < 0.000001f)
            return Vector2.UnitY;

        return Vector2.Normalize(direction);
    }
}
