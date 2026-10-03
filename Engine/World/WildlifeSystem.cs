using System.Numerics;

namespace SlavicGame.Engine.World;

public enum WildlifeSpecies
{
    Deer,
    Boar,
    Wolf,
    Raven
}

public enum WildlifeMotion
{
    Idle,
    Wander,
    Flee,
    Fly
}

public sealed record WildlifeDefinition(
    string Id,
    WildlifeSpecies Species,
    Vector2 Home,
    float WanderRadius,
    float WalkSpeed,
    float FleeSpeed,
    float FleeDistance,
    float Scale,
    Vector3 Color);

public sealed class WildlifeActor
{
    internal WildlifeActor(WildlifeDefinition definition, Vector3 position)
    {
        Definition = definition;
        Position = position;
    }

    public WildlifeDefinition Definition { get; }
    public string Id => Definition.Id;
    public WildlifeSpecies Species => Definition.Species;
    public Vector3 Position { get; internal set; }
    public float YawRadians { get; internal set; }
    public WildlifeMotion Motion { get; internal set; }
    public string AnimationClip { get; internal set; } = "Idle";
}

public static class WildlifeVisualCatalog
{
    public static string ModelFile(WildlifeSpecies species) => species switch
    {
        WildlifeSpecies.Deer => "deer_animated.glb",
        WildlifeSpecies.Boar => "boar_animated.glb",
        WildlifeSpecies.Wolf => "wolf_animated.glb",
        WildlifeSpecies.Raven => "raven_animated.glb",
        _ => throw new ArgumentOutOfRangeException(nameof(species))
    };

    public static bool IsFlying(WildlifeSpecies species) =>
        species == WildlifeSpecies.Raven;
}

public sealed class WildlifeSystem
{
    private static readonly WildlifeDefinition[] Definitions =
    [
        new("deer-01", WildlifeSpecies.Deer, new Vector2(-42f, 34f), 18f, 1.25f, 5.4f, 18f, 1.00f, new Vector3(0.47f, 0.33f, 0.20f)),
        new("deer-02", WildlifeSpecies.Deer, new Vector2(48f, -18f), 22f, 1.20f, 5.2f, 18f, 0.94f, new Vector3(0.52f, 0.38f, 0.23f)),
        new("deer-03", WildlifeSpecies.Deer, new Vector2(-28f, 58f), 16f, 1.15f, 5.0f, 18f, 0.90f, new Vector3(0.44f, 0.31f, 0.19f)),

        new("boar-01", WildlifeSpecies.Boar, new Vector2(-68f, -22f), 16f, 1.05f, 4.2f, 12f, 1.02f, new Vector3(0.28f, 0.22f, 0.17f)),
        new("boar-02", WildlifeSpecies.Boar, new Vector2(67f, 68f), 17f, 1.00f, 4.0f, 12f, 0.96f, new Vector3(0.31f, 0.24f, 0.18f)),

        new("wolf-01", WildlifeSpecies.Wolf, new Vector2(-116f, 14f), 24f, 1.45f, 5.8f, 16f, 0.96f, new Vector3(0.34f, 0.34f, 0.32f)),
        new("wolf-02", WildlifeSpecies.Wolf, new Vector2(112f, -48f), 26f, 1.50f, 5.9f, 16f, 0.92f, new Vector3(0.31f, 0.31f, 0.30f)),

        new("raven-01", WildlifeSpecies.Raven, new Vector2(-21f, 18f), 28f, 4.0f, 0f, 0f, 0.90f, new Vector3(0.08f, 0.09f, 0.10f)),
        new("raven-02", WildlifeSpecies.Raven, new Vector2(32f, 27f), 24f, 4.3f, 0f, 0f, 0.84f, new Vector3(0.09f, 0.09f, 0.11f)),
        new("raven-03", WildlifeSpecies.Raven, new Vector2(-54f, -12f), 30f, 4.1f, 0f, 0f, 0.88f, new Vector3(0.07f, 0.08f, 0.09f)),
        new("raven-04", WildlifeSpecies.Raven, new Vector2(49f, -43f), 26f, 4.2f, 0f, 0f, 0.82f, new Vector3(0.08f, 0.08f, 0.10f))
    ];

    private readonly List<WildlifeActor> _actors = [];
    private double _elapsedSeconds;

    public IReadOnlyList<WildlifeActor> Actors => _actors;

    public void Initialize(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        _actors.Clear();
        _elapsedSeconds = 0d;

        foreach (var definition in Definitions)
        {
            var position = GroundedPosition(world, definition.Home);
            if (WildlifeVisualCatalog.IsFlying(definition.Species))
            {
                position.Y += 10f + StableUnit(definition.Id) * 5f;
            }

            _actors.Add(new WildlifeActor(definition, position)
            {
                Motion = WildlifeVisualCatalog.IsFlying(definition.Species)
                    ? WildlifeMotion.Fly
                    : WildlifeMotion.Idle,
                AnimationClip = WildlifeVisualCatalog.IsFlying(definition.Species)
                    ? "Fly"
                    : "Idle"
            });
        }
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        var dt = (float)Math.Min(deltaSeconds, 0.10d);
        _elapsedSeconds += deltaSeconds;

        foreach (var actor in _actors)
        {
            if (WildlifeVisualCatalog.IsFlying(actor.Species))
            {
                UpdateRaven(world, actor);
                continue;
            }

            UpdateGroundAnimal(world, actor, dt);
        }
    }

    private void UpdateGroundAnimal(
        WorldState world,
        WildlifeActor actor,
        float deltaSeconds)
    {
        var position2 = new Vector2(actor.Position.X, actor.Position.Z);
        var player2 = new Vector2(world.PlayerPosition.X, world.PlayerPosition.Z);
        var fromPlayer = position2 - player2;
        var distanceToPlayer = fromPlayer.Length();

        Vector2 desiredDirection;
        float speed;
        WildlifeMotion motion;
        string clip;

        if (distanceToPlayer > 0.001f &&
            distanceToPlayer < actor.Definition.FleeDistance)
        {
            desiredDirection = Vector2.Normalize(fromPlayer);
            speed = actor.Definition.FleeSpeed;
            motion = WildlifeMotion.Flee;
            clip = "Run";
        }
        else
        {
            var phase = StablePhase(actor.Id);
            var orbit = new Vector2(
                MathF.Sin((float)_elapsedSeconds * 0.11f + phase),
                MathF.Cos((float)_elapsedSeconds * 0.083f + phase * 1.37f));
            var secondary = new Vector2(
                MathF.Sin((float)_elapsedSeconds * 0.047f + phase * 2.1f),
                MathF.Cos((float)_elapsedSeconds * 0.061f + phase * 0.73f));
            var target = actor.Definition.Home +
                         (orbit * 0.72f + secondary * 0.28f) *
                         actor.Definition.WanderRadius;

            var toTarget = target - position2;
            if (toTarget.LengthSquared() < 1.4f * 1.4f)
            {
                desiredDirection = Vector2.Zero;
                speed = 0f;
                motion = WildlifeMotion.Idle;
                clip = "Idle";
            }
            else
            {
                desiredDirection = Vector2.Normalize(toTarget);
                speed = actor.Definition.WalkSpeed;
                motion = WildlifeMotion.Wander;
                clip = "Walk";
            }
        }

        if (desiredDirection.LengthSquared() > 0.000001f)
        {
            var next = position2 + desiredDirection * speed * deltaSeconds;
            next = world.ResolveHorizontalPosition(next, 0.38f);

            // Ambient fauna should not wander through the village core.
            var villageCenter = new Vector2(0f, -85f);
            if (Vector2.Distance(next, villageCenter) < 38f)
            {
                var away = next - villageCenter;
                if (away.LengthSquared() > 0.000001f)
                    next = villageCenter + Vector2.Normalize(away) * 38f;
            }

            actor.Position = GroundedPosition(world, next);
            actor.YawRadians = WorldPlacementOrientation.YawFacing(
                position2,
                next);
        }

        actor.Motion = motion;
        actor.AnimationClip = clip;
    }

    private void UpdateRaven(
        WorldState world,
        WildlifeActor actor)
    {
        var phase = StablePhase(actor.Id);
        var speed = actor.Definition.WalkSpeed;
        var t = (float)_elapsedSeconds * speed * 0.055f + phase;

        var radius = actor.Definition.WanderRadius *
                     (0.82f + 0.12f * MathF.Sin(t * 0.41f + phase));
        var horizontal = actor.Definition.Home +
                         new Vector2(
                             MathF.Cos(t) * radius,
                             MathF.Sin(t * 0.83f) * radius * 0.72f);

        var ground = world.Terrain.SampleHeight(
            new Vector3(horizontal.X, 0f, horizontal.Y));
        var altitude =
            10f +
            StableUnit(actor.Id) * 5f +
            MathF.Sin(t * 1.7f) * 1.4f;

        var previous = new Vector2(actor.Position.X, actor.Position.Z);
        actor.Position = new Vector3(
            horizontal.X,
            ground + altitude,
            horizontal.Y);

        var forward = horizontal - previous;
        if (forward.LengthSquared() > 0.000001f)
        {
            actor.YawRadians = WorldPlacementOrientation.YawFacing(
                previous,
                horizontal);
        }

        actor.Motion = WildlifeMotion.Fly;
        actor.AnimationClip = "Fly";
    }

    private static Vector3 GroundedPosition(
        WorldState world,
        Vector2 horizontal)
    {
        var resolved = world.ResolveHorizontalPosition(horizontal, 0.38f);
        var position = new Vector3(resolved.X, 0f, resolved.Y);
        position.Y = world.Terrain.SampleHeight(position);
        return position;
    }

    private static float StablePhase(string id) =>
        StableUnit(id) * MathF.Tau;

    private static float StableUnit(string id)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in id)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            return (hash % 10000u) / 9999f;
        }
    }
}
