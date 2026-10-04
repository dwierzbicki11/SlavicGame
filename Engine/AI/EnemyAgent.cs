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
    private const float AttackIntervalSeconds = 1.2f;
    private const float AlertSeconds = 0.55f;
    private const float HitReactionSeconds = 0.18f;

    private float _patrolSign = 1f;
    private double _alertRemaining;
    private double _attackCooldown;
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
    public bool IsAlive => Health > 0f;
    public bool IsHitReacting => _hitReactionRemaining > 0.0;

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
            return;
        }

        if (_hitReactionRemaining > 0.0)
        {
            _hitReactionRemaining = Math.Max(0.0, _hitReactionRemaining - deltaSeconds);
            if (_hitReactionRemaining > 0.0)
            {
                return;
            }
        }

        _attackCooldown = Math.Max(0.0, _attackCooldown - deltaSeconds);

        var playerDistance = HorizontalDistance(Position, world.PlayerPosition);
        var homeDistance = HorizontalDistance(Position, HomePosition);
        var disengageRange = _provokedByDamage ? ProvokedDisengageRange : DisengageRange;

        switch (State)
        {
            case EnemyState.Patrol:
                if (world.Player.IsAlive && playerDistance <= DetectionRange)
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
                    _provokedByDamage = false;
                }
                else if (playerDistance > AttackExitRange)
                {
                    State = EnemyState.Chase;
                }
                else if (_attackCooldown <= 0.0)
                {
                    world.Player.TakeDamage(Damage);
                    _attackCooldown = AttackIntervalSeconds;
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
        _attackCooldown = State == EnemyState.Attack ? AttackIntervalSeconds : 0;
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
            _provokedByDamage = false;
            return;
        }

        _provokedByDamage = true;
        _hitReactionRemaining = HitReactionSeconds;

        // A hit can wake or re-alert an enemy, but it must not make an enemy that is
        // already pursuing or attacking forget its engagement and replay Alert.
        if (State is EnemyState.Chase or EnemyState.Attack)
        {
            return;
        }

        State = EnemyState.Alert;
        _alertRemaining = AlertSeconds;
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
