using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Combat;

public sealed record ArrowProjectileView(
    long Id,
    Vector3 Position,
    Vector3 Velocity,
    float DrawFraction);

public sealed record RecoverableArrow(
    long Id,
    Vector3 Position);

public sealed class BowCombat
{
    public const string BowItemId = "simple-bow";
    public const string ArrowItemId = "arrow-basic";

    public const double DrawTimeSeconds = 1.20;
    public const double MinimumReleaseSeconds = 0.10;
    public const float MinVelocity = 24f;
    public const float MaxVelocity = 55f;
    public const float MinDamage = 10f;
    public const float MaxDamage = 40f;
    public const float Gravity = 9.81f;

    private const double MaxProjectileStepSeconds = 0.02;
    private const double MaxProjectileLifetimeSeconds = 8.0;
    private const float ArrowRadius = 0.08f;

    private sealed class Projectile(
        long id,
        Vector3 position,
        Vector3 velocity,
        float drawFraction)
    {
        public long Id = id;
        public Vector3 Position = position;
        public Vector3 Velocity = velocity;
        public float DrawFraction = drawFraction;
        public double LifetimeSeconds;
    }

    private readonly List<Projectile> _projectiles = [];
    private readonly List<RecoverableArrow> _recoverable = [];

    private long _nextArrowId = 1;
    private bool _isDrawing;
    private double _drawSeconds;
    private Vector3 _drawOrigin;
    private Vector3 _drawDirection = Vector3.UnitZ;

    public bool IsAiming { get; private set; }
    public bool IsDrawing => _isDrawing;
    public float DrawFraction =>
        _isDrawing
            ? Math.Clamp(
                (float)(_drawSeconds / DrawTimeSeconds),
                0f,
                1f)
            : 0f;

    public string Message { get; private set; } =
        "RMB CELUJ LUKIEM";

    public IReadOnlyList<ArrowProjectileView> Projectiles =>
        _projectiles
            .Select(item => new ArrowProjectileView(
                item.Id,
                item.Position,
                item.Velocity,
                item.DrawFraction))
            .ToArray();

    public IReadOnlyList<RecoverableArrow> RecoverableArrows =>
        _recoverable;

    public void UpdateControl(
        WorldState world,
        bool aimHeld,
        bool leftPressed,
        bool leftHeld,
        Vector3 origin,
        Vector3 direction,
        double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        var canUseBow =
            world.Progress.Inventory.Contains(BowItemId) &&
            world.Player.IsAlive &&
            !world.Cinematics.IsPlaying &&
            !world.Dialogue.IsOpen &&
            !world.Vendors.IsOpen &&
            !world.Crafting.IsOpen &&
            !world.Rituals.IsPerforming &&
            !world.Magic.IsCasting &&
            world.Melee.Controller.State == MeleeAttackState.Free;

        IsAiming = aimHeld && canUseBow;

        if (!IsAiming)
        {
            if (_isDrawing)
                CancelDraw();

            Message = world.Progress.Inventory.Contains(BowItemId)
                ? "RMB CELUJ LUKIEM"
                : "BRAK LUKU";
            return;
        }

        direction = SafeDirection(direction);
        _drawOrigin = origin;
        _drawDirection = direction;

        if (leftPressed && !_isDrawing)
        {
            if (!world.Progress.Inventory.Contains(ArrowItemId))
            {
                Message = "BRAK STRZAL";
                return;
            }

            _isDrawing = true;
            _drawSeconds = 0d;
            Message = "NACIAGANIE LUKU";
        }

        if (!_isDrawing)
        {
            Message =
                $"CELUJ / STRZALY {world.Progress.Inventory.Count(ArrowItemId)}";
            return;
        }

        if (leftHeld)
        {
            _drawSeconds =
                Math.Min(
                    DrawTimeSeconds,
                    _drawSeconds + deltaSeconds);

            Message =
                $"NACIAG {DrawFraction * 100f:0}% / STRZALY {world.Progress.Inventory.Count(ArrowItemId)}";
            return;
        }

        Release(world);
    }

    public void Update(
        WorldState world,
        double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds <= 0d)
            return;

        var remaining = deltaSeconds;
        while (remaining > 0d && _projectiles.Count > 0)
        {
            var step =
                Math.Min(
                    MaxProjectileStepSeconds,
                    remaining);
            SimulateStep(
                world,
                (float)step);
            remaining -= step;
        }
    }

    public RecoverableArrow? FindNearestRecoverable(
        Vector3 position,
        float maxDistance = 3.2f)
    {
        var maxDistanceSquared =
            maxDistance * maxDistance;
        RecoverableArrow? best = null;

        foreach (var arrow in _recoverable)
        {
            var delta =
                arrow.Position - position;
            var distanceSquared =
                delta.LengthSquared();
            if (distanceSquared >
                maxDistanceSquared)
            {
                continue;
            }

            maxDistanceSquared =
                distanceSquared;
            best = arrow;
        }

        return best;
    }

    public bool TryRecover(
        WorldState world,
        long id)
    {
        ArgumentNullException.ThrowIfNull(world);

        var index =
            _recoverable.FindIndex(item =>
                item.Id == id);
        if (index < 0)
            return false;

        _recoverable.RemoveAt(index);
        world.Progress.Inventory.Add(ArrowItemId);
        Message =
            $"ODZYSKANO STRZALE / STRZALY {world.Progress.Inventory.Count(ArrowItemId)}";
        return true;
    }

    public void ResetTransient()
    {
        _projectiles.Clear();
        _recoverable.Clear();
        IsAiming = false;
        _isDrawing = false;
        _drawSeconds = 0d;
        Message = "RMB CELUJ LUKIEM";
    }

    private void CancelDraw()
    {
        _isDrawing = false;
        _drawSeconds = 0d;
    }

    private void Release(
        WorldState world)
    {
        if (!_isDrawing)
            return;

        var heldSeconds = _drawSeconds;
        var fraction =
            Math.Clamp(
                (float)(heldSeconds / DrawTimeSeconds),
                0f,
                1f);

        _isDrawing = false;
        _drawSeconds = 0d;

        if (heldSeconds < MinimumReleaseSeconds)
        {
            Message = "NACIAG ZA KROTKI / STRZALA NIE ZUZYTA";
            return;
        }

        if (!world.Progress.Inventory.Remove(ArrowItemId))
        {
            Message = "BRAK STRZAL";
            return;
        }

        var velocity =
            MathF.Lerp(
                MinVelocity,
                MaxVelocity,
                fraction);

        _projectiles.Add(
            new Projectile(
                _nextArrowId++,
                _drawOrigin,
                _drawDirection * velocity,
                fraction));

        Message =
            $"STRZAL / NACIAG {fraction * 100f:0}% / STRZALY {world.Progress.Inventory.Count(ArrowItemId)}";
    }

    private void SimulateStep(
        WorldState world,
        float deltaSeconds)
    {
        for (var i = _projectiles.Count - 1;
             i >= 0;
             i--)
        {
            var projectile =
                _projectiles[i];

            projectile.LifetimeSeconds +=
                deltaSeconds;
            if (projectile.LifetimeSeconds >
                MaxProjectileLifetimeSeconds)
            {
                _projectiles.RemoveAt(i);
                continue;
            }

            var from =
                projectile.Position;
            projectile.Velocity +=
                new Vector3(
                    0f,
                    -Gravity * deltaSeconds,
                    0f);
            var to =
                from +
                projectile.Velocity *
                deltaSeconds;

            var hit =
                FindFirstHit(
                    world,
                    from,
                    to);

            if (hit is null)
            {
                projectile.Position = to;
                continue;
            }

            ResolveHit(
                world,
                projectile,
                hit.Value);

            _projectiles.RemoveAt(i);
        }
    }

    private ArrowHit? FindFirstHit(
        WorldState world,
        Vector3 from,
        Vector3 to)
    {
        ArrowHit? best = null;

        var terrainFraction =
            world.Terrain.IntersectGroundSegment(
                from,
                to,
                0f);
        if (terrainFraction is { } terrainT)
        {
            Consider(
                new ArrowHit(
                    ArrowHitKind.World,
                    Math.Clamp(
                        terrainT,
                        0f,
                        1f),
                    Vector3.Lerp(
                        from,
                        to,
                        Math.Clamp(
                            terrainT,
                            0f,
                            1f)),
                    null));
        }

        foreach (var obstacle in world.Obstacles)
        {
            if (SegmentAabb(
                    from,
                    to,
                    obstacle,
                    out var t))
            {
                Consider(
                    new ArrowHit(
                        ArrowHitKind.World,
                        t,
                        Vector3.Lerp(
                            from,
                            to,
                            t),
                        null));
            }
        }

        foreach (var enemy in world.Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            var center =
                enemy.Position +
                Vector3.UnitY *
                MathF.Max(
                    0.55f,
                    enemy.Radius);

            if (SegmentSphere(
                    from,
                    to,
                    center,
                    enemy.Radius +
                    ArrowRadius,
                    out var t))
            {
                Consider(
                    new ArrowHit(
                        ArrowHitKind.Enemy,
                        t,
                        Vector3.Lerp(
                            from,
                            to,
                            t),
                        enemy.Id));
            }
        }

        foreach (var wildlife in world.Wildlife.Actors)
        {
            if (wildlife.Behavior ==
                    WildlifeBehavior.Dead)
            {
                continue;
            }

            var profile =
                WildlifeCatalog.For(
                    wildlife.Species);
            var center =
                wildlife.Position +
                Vector3.UnitY *
                (wildlife.Species ==
                    WildlifeSpecies.Raven
                    ? 0f
                    : profile.BodyRadius *
                      0.70f);

            if (SegmentSphere(
                    from,
                    to,
                    center,
                    profile.BodyRadius +
                    ArrowRadius,
                    out var t))
            {
                Consider(
                    new ArrowHit(
                        ArrowHitKind.Wildlife,
                        t,
                        Vector3.Lerp(
                            from,
                            to,
                            t),
                        wildlife.Id));
            }
        }

        return best;

        void Consider(
            ArrowHit candidate)
        {
            if (best is null ||
                candidate.Fraction <
                best.Value.Fraction)
            {
                best = candidate;
            }
        }
    }

    private void ResolveHit(
        WorldState world,
        Projectile projectile,
        ArrowHit hit)
    {
        var impact =
            hit.Position;

        var damage =
            MathF.Lerp(
                MinDamage,
                MaxDamage,
                projectile.DrawFraction);

        if (hit.Kind == ArrowHitKind.Enemy &&
            hit.TargetId is not null)
        {
            var enemy =
                world.Enemies.FirstOrDefault(item =>
                    item.Id == hit.TargetId);

            if (enemy is not null &&
                enemy.IsAlive)
            {
                enemy.TakeDamage(damage);
                if (!enemy.IsAlive &&
                    enemy.Id ==
                    SwampPredatorEncounter.Id)
                {
                    SwampPredatorEncounter.MarkKilled(
                        world);
                }

                Message =
                    enemy.IsAlive
                        ? $"STRZALA TRAFIA {enemy.Id.ToUpperInvariant()} / HP {enemy.Health:0}/{enemy.MaxHealth:0}"
                        : $"STRZALA POKONALA {enemy.Id.ToUpperInvariant()}";
            }

            return;
        }

        if (hit.Kind ==
                ArrowHitKind.Wildlife &&
            hit.TargetId is not null)
        {
            var before =
                world.Wildlife.Actors
                    .FirstOrDefault(item =>
                        item.Id == hit.TargetId);

            if (before is not null &&
                world.Wildlife.TryDamage(
                    world,
                    hit.TargetId,
                    damage))
            {
                var after =
                    world.Wildlife.Actors
                        .First(item =>
                            item.Id ==
                            hit.TargetId);

                Message =
                    after.Behavior ==
                    WildlifeBehavior.Dead
                        ? $"UPOLWIONO Z LUKU: {WildlifeCatalog.DisplayName(after.Species)}"
                        : $"TRAFIONO: {WildlifeCatalog.DisplayName(after.Species)} / HP {after.Health:0}/{after.MaxHealth:0}";
            }

            return;
        }

        _recoverable.Add(
            new RecoverableArrow(
                projectile.Id,
                impact));

        Message =
            "STRZALA WBILA SIE W PODLOZE / E ABY ODZYSKAC";
    }

    private static Vector3 SafeDirection(
        Vector3 value)
    {
        if (!float.IsFinite(value.X) ||
            !float.IsFinite(value.Y) ||
            !float.IsFinite(value.Z) ||
            value.LengthSquared() <
            0.000001f)
        {
            return Vector3.UnitZ;
        }

        return Vector3.Normalize(value);
    }

    private static bool SegmentSphere(
        Vector3 from,
        Vector3 to,
        Vector3 center,
        float radius,
        out float t)
    {
        var segment = to - from;
        var lengthSquared =
            segment.LengthSquared();

        if (lengthSquared <
            0.000001f)
        {
            t = 0f;
            return Vector3.DistanceSquared(
                from,
                center) <=
                radius * radius;
        }

        t = Math.Clamp(
            Vector3.Dot(
                center - from,
                segment) /
            lengthSquared,
            0f,
            1f);

        var closest =
            from + segment * t;
        return Vector3.DistanceSquared(
            closest,
            center) <=
            radius * radius;
    }

    private static bool SegmentAabb(
        Vector3 from,
        Vector3 to,
        WorldObstacle obstacle,
        out float hitT)
    {
        var min =
            new Vector3(
                obstacle.Position.X -
                    obstacle.HalfSize.X,
                obstacle.Position.Y,
                obstacle.Position.Z -
                    obstacle.HalfSize.Y);
        var max =
            new Vector3(
                obstacle.Position.X +
                    obstacle.HalfSize.X,
                obstacle.Position.Y +
                    obstacle.Height,
                obstacle.Position.Z +
                    obstacle.HalfSize.Y);

        var direction = to - from;
        var tMin = 0f;
        var tMax = 1f;

        if (!Clip(
                from.X,
                direction.X,
                min.X,
                max.X,
                ref tMin,
                ref tMax) ||
            !Clip(
                from.Y,
                direction.Y,
                min.Y,
                max.Y,
                ref tMin,
                ref tMax) ||
            !Clip(
                from.Z,
                direction.Z,
                min.Z,
                max.Z,
                ref tMin,
                ref tMax))
        {
            hitT = 0f;
            return false;
        }

        hitT = tMin;
        return true;
    }

    private static bool Clip(
        float origin,
        float direction,
        float min,
        float max,
        ref float tMin,
        ref float tMax)
    {
        if (MathF.Abs(direction) <
            0.000001f)
        {
            return origin >= min &&
                   origin <= max;
        }

        var inverse =
            1f / direction;
        var a =
            (min - origin) * inverse;
        var b =
            (max - origin) * inverse;

        if (a > b)
            (a, b) = (b, a);

        tMin = MathF.Max(
            tMin,
            a);
        tMax = MathF.Min(
            tMax,
            b);

        return tMin <= tMax;
    }

    private readonly record struct ArrowHit(
        ArrowHitKind Kind,
        float Fraction,
        Vector3 Position,
        string? TargetId);

    private enum ArrowHitKind
    {
        World,
        Enemy,
        Wildlife
    }
}
