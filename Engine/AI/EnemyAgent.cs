using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.AI;

public enum EnemyState
{
    Patrol,
    Alert,
    Chase,
    Attack,
    Return,
    Dead
}

public sealed record EnemySnapshot(
    string Id,
    Vector3 Position,
    float Health,
    EnemyState State);

public sealed class EnemyAgent : IDamageReceiver
{
    private const float PatrolDistance = 4f;
    private const float DetectionRange = 14f;
    private const float DisengageRange = 22f;
    private const float ProvokedDisengageRange = 30f;
    private const float MaxLeashDistance = 24f;
    private const float AttackRange = 1.45f;
    private const float AttackExitRange = 1.9f;
    private const float MoveSpeed = 2.6f;
    private const float ReturnSpeed = 3.0f;
    private const float Damage = 8f;
    private const float AlertSeconds = 0.55f;
    private const float HitReactionSeconds = 0.18f;

    private float _patrolSign = 1f;
    private double _alertRemaining;
    private double _hitReactionRemaining;
    private bool _provokedByDamage;

    public string Id { get; }
    public Vector3 HomePosition { get; }
    public Vector3 Position { get; private set; }
    public float Radius { get; } = 0.45f;
    public float Height { get; } = 1.7f;
    public float MaxHealth { get; } = 60f;
    public float Health { get; private set; } = 60f;
    public EnemyState State { get; private set; } = EnemyState.Patrol;
    public EnemyAttackController Attack { get; } = new();
    public Vector3 FacingDirection { get; private set; } = Vector3.UnitX;
    public bool IsAttackWindingUp => Attack.Phase == EnemyAttackPhase.Windup && !IsHitReacting;
    public bool IsAlive => Health > 0f;
    public bool IsHitReacting => _hitReactionRemaining > 0.0;
    public float HitReactionProgress => IsHitReacting
        ? Math.Clamp(1f - (float)(_hitReactionRemaining / HitReactionSeconds), 0f, 1f)
        : 0f;

    public EnemyAgent(string id, Vector3 homePosition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (!IsFinite(homePosition))
        {
            throw new ArgumentOutOfRangeException(nameof(homePosition));
        }

        Id = id;
        HomePosition = homePosition;
        Position = homePosition;
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
        {
            return;
        }

        if (!IsAlive)
        {
            State = EnemyState.Dead;
            Attack.Reset();
            return;
        }

        if (_hitReactionRemaining > 0.0)
        {
            var reactionSeconds = Math.Min(_hitReactionRemaining, deltaSeconds);
            _hitReactionRemaining -= reactionSeconds;
            deltaSeconds -= reactionSeconds;
            if (_hitReactionRemaining > 0.0 || deltaSeconds <= 0.0)
            {
                return;
            }
        }

        if (State != EnemyState.Attack && Attack.Phase == EnemyAttackPhase.Recovery)
            Attack.Advance(deltaSeconds, out _);

        var playerDistance = HorizontalDistance(Position, world.PlayerPosition);
        var homeDistance = HorizontalDistance(Position, HomePosition);
        var disengageRange = _provokedByDamage ? ProvokedDisengageRange : DisengageRange;

        switch (State)
        {
            case EnemyState.Patrol:
                if (world.Player.IsAlive && playerDistance <= DetectionRange && HasLineOfSight(world, world.PlayerPosition))
                {
                    State = EnemyState.Alert;
                    _alertRemaining = AlertSeconds;
                }
                else
                {
                    Patrol(world, deltaSeconds);
                }
                break;

            case EnemyState.Alert:
                if (!world.Player.IsAlive || playerDistance > disengageRange)
                {
                    State = EnemyState.Return;
                    _provokedByDamage = false;
                    break;
                }

                _alertRemaining -= deltaSeconds;
                if (_alertRemaining <= 0.0)
                {
                    State = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                if (!world.Player.IsAlive ||
                    playerDistance > disengageRange ||
                    homeDistance > MaxLeashDistance)
                {
                    State = EnemyState.Return;
                    _provokedByDamage = false;
                }
                else if (playerDistance <= AttackRange)
                {
                    State = EnemyState.Attack;
                    LockFacingTowards(world.PlayerPosition);
                }
                else
                {
                    MoveTowards(world, world.PlayerPosition, MoveSpeed, deltaSeconds);
                }
                break;

            case EnemyState.Attack:
                if (!world.Player.IsAlive ||
                    playerDistance > disengageRange ||
                    homeDistance > MaxLeashDistance)
                {
                    State = EnemyState.Return;
                    Attack.Reset();
                    _provokedByDamage = false;
                }
                else if (playerDistance > AttackExitRange && Attack.Phase != EnemyAttackPhase.Windup)
                {
                    State = EnemyState.Chase;
                    Attack.Advance(deltaSeconds, out _);
                }
                else
                {
                    UpdateAttack(world, deltaSeconds, playerDistance);
                }
                break;

            case EnemyState.Return:
                if (homeDistance <= 0.2f)
                {
                    Position = HomePosition;
                    State = EnemyState.Patrol;
                    _provokedByDamage = false;
                }
                else
                {
                    MoveTowards(world, HomePosition, ReturnSpeed, deltaSeconds);
                }
                break;

            case EnemyState.Dead:
                break;
        }
    }

    public EnemySnapshot Capture() => new(Id, Position, Health, State);

    public void Restore(EnemySnapshot snapshot)
    {
        if (!string.Equals(snapshot.Id, Id, StringComparison.Ordinal) ||
            !IsFinite(snapshot.Position) ||
            !float.IsFinite(snapshot.Health) ||
            snapshot.Health < 0f || snapshot.Health > MaxHealth)
        {
            throw new ArgumentException("Enemy snapshot is invalid.", nameof(snapshot));
        }

        Position = snapshot.Position;
        Health = snapshot.Health;
        State = Health <= 0f ? EnemyState.Dead : snapshot.State;
        _alertRemaining = 0;
        Attack.Reset();
        if (State == EnemyState.Attack) Attack.Interrupt();
        _hitReactionRemaining = 0;
        _provokedByDamage = false;
    }

    public void ApplyDamage(float amount, DamageType damageType)
    {
        if (!Enum.IsDefined(damageType))
        {
            throw new ArgumentOutOfRangeException(nameof(damageType));
        }

        TakeDamage(amount);
    }

    public void TakeDamage(float amount)
    {
        if (!float.IsFinite(amount) || amount < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        Health = MathF.Max(0f, Health - amount);
        if (Health <= 0f)
        {
            State = EnemyState.Dead;
            _alertRemaining = 0;
            _hitReactionRemaining = 0;
            Attack.Reset();
            _provokedByDamage = false;
            return;
        }

        _provokedByDamage = true;
        _hitReactionRemaining = HitReactionSeconds;
        if (Attack.Phase == EnemyAttackPhase.Windup) Attack.Interrupt();

        if (State is EnemyState.Chase or EnemyState.Attack)
        {
            return;
        }

        State = EnemyState.Alert;
        _alertRemaining = AlertSeconds;
    }

    private void UpdateAttack(WorldState world, double deltaSeconds, float playerDistance)
    {
        var remaining = deltaSeconds;
        while (remaining > 0d && world.Player.IsAlive)
        {
            if (Attack.Phase == EnemyAttackPhase.Ready)
            {
                if (playerDistance > AttackRange) break;
                LockFacingTowards(world.PlayerPosition);
                Attack.TryStart();
            }

            remaining = Attack.Advance(remaining, out var impact);
            if (impact && MathF.Abs(world.PlayerPosition.Y - Position.Y) <= Height &&
                MeleeHitDetection.FindTargets(Position, FacingDirection, AttackExitRange, 55f,
                    [new MeleeHitCandidate("player", world.PlayerPosition)]).Count != 0)
            {
                world.Player.TakeDamage(Damage);
            }
        }
    }

    private bool HasLineOfSight(WorldState world, Vector3 target)
    {
        var start = new Vector2(Position.X, Position.Z);
        var end = new Vector2(target.X, target.Z);
        foreach (var obstacle in world.Obstacles)
        {
            var min = new Vector2(obstacle.Position.X, obstacle.Position.Z) - obstacle.HalfSize;
            var max = new Vector2(obstacle.Position.X, obstacle.Position.Z) + obstacle.HalfSize;
            if (SegmentIntersectsBox(start, end, min, max))
                return false;
        }

        return true;
    }

    private static bool SegmentIntersectsBox(Vector2 start, Vector2 end, Vector2 min, Vector2 max)
    {
        var direction = end - start;
        var tMin = 0f;
        var tMax = 1f;

        if (!ClipAxis(start.X, direction.X, min.X, max.X, ref tMin, ref tMax)) return false;
        if (!ClipAxis(start.Y, direction.Y, min.Y, max.Y, ref tMin, ref tMax)) return false;
        return tMax >= tMin;
    }

    private static bool ClipAxis(float origin, float direction, float min, float max, ref float tMin, ref float tMax)
    {
        if (MathF.Abs(direction) <= 0.000001f)
            return origin >= min && origin <= max;

        var inverse = 1f / direction;
        var near = (min - origin) * inverse;
        var far = (max - origin) * inverse;
        if (near > far) (near, far) = (far, near);
        tMin = MathF.Max(tMin, near);
        tMax = MathF.Min(tMax, far);
        return tMin <= tMax;
    }

    private void LockFacingTowards(Vector3 target)
    {
        var direction = target - Position;
        direction.Y = 0f;
        if (!IsFinite(direction) || direction.LengthSquared() <= 0.000001f)
            return;

        FacingDirection = Vector3.Normalize(direction);
    }

    private void Patrol(WorldState world, double deltaSeconds)
    {
        var target = HomePosition + new Vector3(PatrolDistance * _patrolSign, 0f, 0f);
        if (HorizontalDistance(Position, target) <= 0.25f)
        {
            _patrolSign *= -1f;
            target = HomePosition + new Vector3(PatrolDistance * _patrolSign, 0f, 0f);
        }

        MoveTowards(world, target, MoveSpeed * 0.55f, deltaSeconds);
    }

    private void MoveTowards(WorldState world, Vector3 target, float speed, double deltaSeconds)
    {
        var current = new Vector2(Position.X, Position.Z);
        var destination = new Vector2(target.X, target.Z);
        var difference = destination - current;
        var distance = difference.Length();
        if (distance <= 0.0001f)
        {
            return;
        }

        FacingDirection = new Vector3(difference.X, 0f, difference.Y) / distance;
        var step = MathF.Min(distance, speed * (float)deltaSeconds);
        var desired = current + difference / distance * step;
        var resolved = world.ResolveHorizontalPosition(desired, Radius);
        var grounded = new Vector3(resolved.X, 0f, resolved.Y);
        grounded.Y = world.Terrain.SampleHeight(grounded);
        Position = grounded;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b) =>
        Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
