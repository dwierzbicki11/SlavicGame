using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Combat;

public sealed record BowProjectileView(
    long Id,
    Vector3 Position,
    Vector3 Velocity,
    float Damage);

public sealed record RecoverableArrowView(
    long Id,
    Vector3 Position,
    float YawRadians,
    float PitchRadians);

public sealed class BowCombatRuntime
{
    public const string BowItemId = "simple-bow";
    public const string ArrowItemId = "arrow-basic";

    private const float FullDrawSeconds = 1.25f;
    private const float MinimumDrawFraction = 0.18f;
    private const float MinimumVelocity = 22f;
    private const float MaximumVelocity = 52f;
    private const float MinimumDamage = 14f;
    private const float MaximumDamage = 38f;
    private const float Gravity = 9.81f;
    private const float MaxProjectileAgeSeconds = 8f;
    private const float SimulationStepSeconds = 1f / 90f;
    private const float RetrievalDistance = 2.6f;
    private const int MaxRecoverableArrows = 24;

    private sealed class Projectile
    {
        public required long Id;
        public Vector3 Position;
        public Vector3 Velocity;
        public float Damage;
        public float Age;
    }

    private sealed class Recoverable
    {
        public required long Id;
        public Vector3 Position;
        public float Yaw;
        public float Pitch;
    }

    private readonly List<Projectile> _projectiles = [];
    private readonly List<Recoverable> _recoverable = [];
    private readonly List<BowProjectileView> _projectileViews = [];
    private readonly List<RecoverableArrowView> _recoverableViews = [];

    private long _nextId = 1;
    private float _drawSeconds;

    public bool IsAiming { get; private set; }
    public bool IsDrawing { get; private set; }
    public float DrawFraction =>
        IsDrawing
            ? Math.Clamp(_drawSeconds / FullDrawSeconds, 0f, 1f)
            : 0f;

    public string Message { get; private set; } = "RMB CELUJ Z LUKU";

    public IReadOnlyList<BowProjectileView> Projectiles => _projectileViews;
    public IReadOnlyList<RecoverableArrowView> RecoverableArrows => _recoverableViews;

    public string HudText(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var arrows = world.Progress.Inventory.Count(ArrowItemId);
        if (!IsAiming)
            return "";

        return IsDrawing
            ? $"LUK / STRZALY {arrows} / NACIAG {DrawFraction * 100f:0}%"
            : $"LUK / STRZALY {arrows} / LPM NACIAG";
    }

    public void SetAiming(WorldState world, bool aiming)
    {
        ArgumentNullException.ThrowIfNull(world);

        var canAim =
            aiming &&
            world.Progress.Inventory.Contains(BowItemId) &&
            world.Player.IsAlive &&
            !world.Dodge.IsActive &&
            !world.Magic.IsCasting &&
            !world.Rituals.IsPerforming &&
            !world.Cinematics.IsPlaying &&
            !world.Dialogue.IsOpen &&
            !world.Vendors.IsOpen &&
            !world.Crafting.IsOpen &&
            !world.Loot.IsOpen;

        if (!canAim)
        {
            IsAiming = false;
            CancelDraw();
            return;
        }

        IsAiming = true;
        if (!IsDrawing)
            Message = "LMB PRZYTRZYMAJ / PUSC ABY STRZELIC";
    }

    public bool TryStartDraw(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (IsAiming) SetAiming(world, true);
        if (!IsAiming || IsDrawing)
            return false;

        if (!world.Progress.Inventory.Contains(BowItemId))
        {
            Message = "BRAK LUKU";
            return false;
        }

        if (!world.Progress.Inventory.Contains(ArrowItemId))
        {
            Message = "BRAK STRZAL";
            return false;
        }

        IsDrawing = true;
        _drawSeconds = 0f;
        Message = "NACIAGANIE";
        return true;
    }

    public bool TryRelease(
        WorldState world,
        Vector3 origin,
        Vector3 direction)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (IsAiming) SetAiming(world, true);
        if (!IsAiming || !IsDrawing)
            return false;

        if (!IsFinite(origin) ||
            !IsFinite(direction) ||
            direction.LengthSquared() < 0.0001f)
        {
            CancelDraw();
            return false;
        }

        var draw = MathF.Max(DrawFraction, MinimumDrawFraction);
        CancelDraw();

        if (!world.Progress.Inventory.Remove(ArrowItemId))
        {
            Message = "BRAK STRZAL";
            return false;
        }

        direction = Vector3.Normalize(direction);
        var velocity = Lerp(MinimumVelocity, MaximumVelocity, draw);
        var damage = Lerp(MinimumDamage, MaximumDamage, draw);

        _projectiles.Add(new Projectile
        {
            Id = _nextId++,
            Position = origin + direction * 0.42f - Vector3.UnitY * 0.08f,
            Velocity = direction * velocity,
            Damage = damage,
            Age = 0f
        });

        RefreshViews();
        Message = $"STRZAL / NACIAG {draw * 100f:0}%";
        return true;
    }

    public void CancelDraw()
    {
        IsDrawing = false;
        _drawSeconds = 0f;
    }

    public void Update(
        WorldState world,
        double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        if (IsAiming) SetAiming(world, true);

        if (IsDrawing)
        {
            _drawSeconds = MathF.Min(
                FullDrawSeconds,
                _drawSeconds + (float)deltaSeconds);
        }

        var remaining = Math.Min(deltaSeconds, MaxProjectileAgeSeconds);
        while (remaining > 0d && _projectiles.Count > 0)
        {
            var step = (float)Math.Min(
                remaining,
                SimulationStepSeconds);

            SimulateStep(world, step);
            remaining -= step;
        }

        RefreshViews();
    }

    public bool TryRetrieveNearest(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var bestIndex = -1;
        var bestDistanceSquared =
            RetrievalDistance * RetrievalDistance;

        for (var i = 0; i < _recoverable.Count; i++)
        {
            var delta =
                _recoverable[i].Position -
                world.PlayerPosition;
            var distanceSquared = delta.LengthSquared();

            if (distanceSquared <= bestDistanceSquared)
            {
                bestIndex = i;
                bestDistanceSquared = distanceSquared;
            }
        }

        if (bestIndex < 0)
            return false;

        world.Progress.Inventory.Add(ArrowItemId);
        _recoverable.RemoveAt(bestIndex);
        RefreshViews();
        Message = "ODZYSKANO STRZALE";
        return true;
    }

    public string RetrievalPrompt(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var threshold =
            RetrievalDistance * RetrievalDistance;

        return _recoverable.Any(arrow =>
                Vector3.DistanceSquared(
                    arrow.Position,
                    world.PlayerPosition) <= threshold)
            ? "E PODNIES STRZALE"
            : "";
    }

    private void SimulateStep(
        WorldState world,
        float deltaSeconds)
    {
        for (var index = _projectiles.Count - 1;
             index >= 0;
             index--)
        {
            var projectile = _projectiles[index];
            var from = projectile.Position;

            var acceleration = -Vector3.UnitY * Gravity;
            var to = from + projectile.Velocity * deltaSeconds +
                acceleration * (0.5f * deltaSeconds * deltaSeconds);
            projectile.Velocity += acceleration * deltaSeconds;

            projectile.Age += deltaSeconds;

            if (TryFindHit(
                    world,
                    from,
                    to,
                    out var hit))
            {
                ResolveHit(
                    world,
                    projectile,
                    from,
                    to,
                    hit);

                _projectiles.RemoveAt(index);
                continue;
            }

            projectile.Position = to;

            if (projectile.Age >= MaxProjectileAgeSeconds ||
                Vector3.DistanceSquared(
                    projectile.Position,
                    world.PlayerPosition) >
                350f * 350f)
            {
                _projectiles.RemoveAt(index);
            }
        }
    }

    private void ResolveHit(
        WorldState world,
        Projectile projectile,
        Vector3 from,
        Vector3 to,
        ProjectileHit hit)
    {
        var impact =
            Vector3.Lerp(
                from,
                to,
                hit.Fraction);

        switch (hit.Kind)
        {
            case ProjectileHitKind.Enemy:
            {
                var enemy =
                    world.Enemies.First(item =>
                        string.Equals(
                            item.Id,
                            hit.TargetId,
                            StringComparison.Ordinal));

                enemy.TakeDamage(projectile.Damage);

                if (!enemy.IsAlive &&
                    string.Equals(
                        enemy.Id,
                        SwampPredatorEncounter.Id,
                        StringComparison.Ordinal))
                {
                    SwampPredatorEncounter.MarkKilled(world);
                }

                Message = enemy.IsAlive
                    ? $"TRAFIENIE: {enemy.Id.ToUpperInvariant()} / {enemy.Health:0} HP"
                    : $"POKONANO: {enemy.Id.ToUpperInvariant()}";
                break;
            }

            case ProjectileHitKind.Wildlife:
            {
                if (hit.TargetId is null)
                    break;

                var killed = ApplyWildlifeDamage(
                    world,
                    hit.TargetId,
                    projectile.Damage);

                Message = killed
                    ? $"UPOLOWANO: {hit.TargetId.ToUpperInvariant()}"
                    : $"TRAFIENIE: {hit.TargetId.ToUpperInvariant()}";
                break;
            }

            case ProjectileHitKind.Terrain:
            {
                StickArrow(
                    impact,
                    projectile.Velocity);
                Message = "STRZALA WBILA SIE W ZIEMIE";
                break;
            }
        }
    }

    private void StickArrow(
        Vector3 position,
        Vector3 velocity)
    {
        if (_recoverable.Count >= MaxRecoverableArrows)
            _recoverable.RemoveAt(0);

        var direction =
            velocity.LengthSquared() > 0.0001f
                ? Vector3.Normalize(velocity)
                : -Vector3.UnitZ;

        var horizontal =
            new Vector2(direction.X, direction.Z);
        var yaw =
            horizontal.LengthSquared() > 0.0001f
                ? WorldPlacementOrientation.YawFacing(
                    Vector2.Zero,
                    Vector2.Normalize(horizontal))
                : 0f;
        var pitch =
            MathF.Asin(
                Math.Clamp(
                    direction.Y,
                    -1f,
                    1f));

        _recoverable.Add(new Recoverable
        {
            Id = _nextId++,
            Position = position + Vector3.UnitY * 0.025f,
            Yaw = yaw,
            Pitch = pitch
        });
    }

    private static bool TryFindHit(
        WorldState world,
        Vector3 from,
        Vector3 to,
        out ProjectileHit hit)
    {
        hit = default;
        var bestFraction = float.PositiveInfinity;

        foreach (var enemy in world.Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            var center =
                enemy.Position +
                Vector3.UnitY * 0.78f;

            if (SegmentSphere(
                    from,
                    to,
                    center,
                    enemy.Radius + 0.22f,
                    out var fraction) &&
                fraction < bestFraction)
            {
                bestFraction = fraction;
                hit = new ProjectileHit(
                    ProjectileHitKind.Enemy,
                    enemy.Id,
                    fraction);
            }
        }

        foreach (var animal in world.Wildlife.Actors)
        {
            if (animal.Health <= 0f || animal.Looted)
                continue;

            var profile =
                WildlifeCatalog.For(animal.Species);
            var center =
                animal.Position +
                Vector3.UnitY *
                (animal.Species == WildlifeSpecies.Raven
                    ? 0f
                    : profile.BodyRadius * 0.72f);

            if (SegmentSphere(
                    from,
                    to,
                    center,
                    profile.BodyRadius + 0.18f,
                    out var fraction) &&
                fraction < bestFraction)
            {
                bestFraction = fraction;
                hit = new ProjectileHit(
                    ProjectileHitKind.Wildlife,
                    animal.Id,
                    fraction);
            }
        }

        foreach (var obstacle in world.Obstacles)
        {
            var min = obstacle.Position - new Vector3(obstacle.HalfSize.X, 0f, obstacle.HalfSize.Y);
            var max = obstacle.Position + new Vector3(obstacle.HalfSize.X, obstacle.Height, obstacle.HalfSize.Y);
            if (SegmentBox(from, to, min, max, out var fraction) && fraction < bestFraction)
            {
                bestFraction = fraction;
                hit = new ProjectileHit(ProjectileHitKind.Terrain, null, fraction);
            }
        }

        var terrainFraction =
            world.Terrain.IntersectGroundSegment(
                from,
                to,
                0.015f);

        if (terrainFraction is { } terrainHit &&
            terrainHit < bestFraction)
        {
            hit = new ProjectileHit(
                ProjectileHitKind.Terrain,
                null,
                terrainHit);
            return true;
        }

        return bestFraction < float.PositiveInfinity;
    }

    private bool ApplyWildlifeDamage(
        WorldState world,
        string id,
        float damage)
    {
        var actor = world.Wildlife.Actors.FirstOrDefault(item =>
            string.Equals(item.Id, id, StringComparison.Ordinal));

        if (actor is null)
            return false;

        if (!world.Wildlife.TryDamage(world, id, damage)) return false;
        return world.Wildlife.Actors.First(item => item.Id == id).Health <= 0f;
    }

    public void Reset()
    {
        IsAiming = false;
        CancelDraw();
        _projectiles.Clear();
        _recoverable.Clear();
        RefreshViews();
        Message = "RMB CELUJ Z LUKU";
    }

    private static bool SegmentBox(Vector3 from, Vector3 to, Vector3 min, Vector3 max, out float fraction)
    {
        var delta = to - from;
        var enter = 0f;
        var exit = 1f;
        for (var axis = 0; axis < 3; axis++)
        {
            var origin = axis == 0 ? from.X : axis == 1 ? from.Y : from.Z;
            var step = axis == 0 ? delta.X : axis == 1 ? delta.Y : delta.Z;
            var low = axis == 0 ? min.X : axis == 1 ? min.Y : min.Z;
            var high = axis == 0 ? max.X : axis == 1 ? max.Y : max.Z;
            if (MathF.Abs(step) < 0.000001f)
            {
                if (origin < low || origin > high) { fraction = 0f; return false; }
                continue;
            }
            var first = (low - origin) / step;
            var second = (high - origin) / step;
            enter = MathF.Max(enter, MathF.Min(first, second));
            exit = MathF.Min(exit, MathF.Max(first, second));
            if (enter > exit) { fraction = 0f; return false; }
        }
        fraction = enter;
        return true;
    }

    private static bool SegmentSphere(
        Vector3 from,
        Vector3 to,
        Vector3 center,
        float radius,
        out float fraction)
    {
        fraction = 0f;

        var segment = to - from;
        var segmentLengthSquared =
            segment.LengthSquared();

        if (segmentLengthSquared < 0.0000001f)
            return false;

        var offset = from - center;
        var a = segmentLengthSquared;
        var b = 2f * Vector3.Dot(offset, segment);
        var c =
            offset.LengthSquared() -
            radius * radius;

        var discriminant =
            b * b -
            4f * a * c;

        if (discriminant < 0f)
            return false;

        var root =
            MathF.Sqrt(discriminant);
        var inverse =
            1f / (2f * a);

        var first =
            (-b - root) * inverse;
        var second =
            (-b + root) * inverse;

        var t =
            first >= 0f && first <= 1f
                ? first
                : second >= 0f && second <= 1f
                    ? second
                    : float.PositiveInfinity;

        if (!float.IsFinite(t))
            return false;

        fraction = t;
        return true;
    }

    private void RefreshViews()
    {
        _projectileViews.Clear();
        foreach (var projectile in _projectiles)
        {
            _projectileViews.Add(
                new BowProjectileView(
                    projectile.Id,
                    projectile.Position,
                    projectile.Velocity,
                    projectile.Damage));
        }

        _recoverableViews.Clear();
        foreach (var arrow in _recoverable)
        {
            _recoverableViews.Add(
                new RecoverableArrowView(
                    arrow.Id,
                    arrow.Position,
                    arrow.Yaw,
                    arrow.Pitch));
        }
    }

    private static float Lerp(
        float a,
        float b,
        float t) =>
        a + (b - a) * Math.Clamp(t, 0f, 1f);

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) &&
        float.IsFinite(value.Y) &&
        float.IsFinite(value.Z);

    private enum ProjectileHitKind
    {
        Terrain,
        Enemy,
        Wildlife
    }

    private readonly record struct ProjectileHit(
        ProjectileHitKind Kind,
        string? TargetId,
        float Fraction);
}
