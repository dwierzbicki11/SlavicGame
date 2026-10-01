using System.Numerics;
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

public sealed class EnemyAgent
{
    private const float PatrolDistance = 4f;
    private const float DetectionRange = 14f;
    private const float DisengageRange = 22f;
    private const float MaxLeashDistance = 24f;
    private const float AttackRange = 1.45f;
    private const float AttackExitRange = 1.9f;
    private const float MoveSpeed = 2.6f;
    private const float ReturnSpeed = 3.0f;
    private const float Damage = 8f;
    private const float AttackIntervalSeconds = 1.2f;
    private const float AlertSeconds = 0.55f;

    private float _patrolSign = 1f;
    private double _alertRemaining;
    private double _attackCooldown;

    public string Id { get; }
    public Vector3 HomePosition { get; }
    public Vector3 Position { get; private set; }
    public float Radius { get; } = 0.45f;
    public float Height { get; } = 1.7f;
    public float MaxHealth { get; } = 60f;
    public float Health { get; private set; } = 60f;
    public EnemyState State { get; private set; } = EnemyState.Patrol;
    public bool IsAlive => Health > 0f;

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

        _attackCooldown = Math.Max(0.0, _attackCooldown - deltaSeconds);

        var playerDistance = HorizontalDistance(Position, world.PlayerPosition);
        var homeDistance = HorizontalDistance(Position, HomePosition);

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
                if (!world.Player.IsAlive || playerDistance > DisengageRange)
                {
                    State = EnemyState.Return;
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
                    playerDistance > DisengageRange ||
                    homeDistance > MaxLeashDistance)
                {
                    State = EnemyState.Return;
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
                    playerDistance > DisengageRange ||
                    homeDistance > MaxLeashDistance)
                {
                    State = EnemyState.Return;
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

    public void TakeDamage(float amount)
    {
        if (!float.IsFinite(amount) || amount < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Health = MathF.Max(0f, Health - amount);
        if (Health <= 0f)
        {
            State = EnemyState.Dead;
        }
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
