using System.Numerics;

namespace SlavicGame.Engine.AI;

/// <summary>
/// Tracks the last position at which an engaged target was actually visible.
/// This keeps chase decisions deterministic and prevents AI from receiving
/// perfect knowledge of a target while line of sight is broken.
/// </summary>
public sealed class EnemyChaseMemory
{
    public const float DefaultForgetSeconds = 2.0f;

    private readonly float _forgetSeconds;
    private float _unseenSeconds;

    public bool HasKnownPosition { get; private set; }
    public Vector3 LastKnownPosition { get; private set; }
    public float UnseenSeconds => _unseenSeconds;
    public bool IsExpired => HasKnownPosition && _unseenSeconds >= _forgetSeconds;

    public EnemyChaseMemory(float forgetSeconds = DefaultForgetSeconds)
    {
        if (!float.IsFinite(forgetSeconds) || forgetSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(forgetSeconds));

        _forgetSeconds = forgetSeconds;
    }

    public void Observe(Vector3 targetPosition)
    {
        if (!IsFinite(targetPosition))
            throw new ArgumentOutOfRangeException(nameof(targetPosition));

        LastKnownPosition = targetPosition;
        HasKnownPosition = true;
        _unseenSeconds = 0f;
    }

    public void AdvanceUnseen(double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d || !HasKnownPosition)
            return;

        _unseenSeconds = MathF.Min(_forgetSeconds, _unseenSeconds + (float)deltaSeconds);
    }

    public void Reset()
    {
        HasKnownPosition = false;
        LastKnownPosition = default;
        _unseenSeconds = 0f;
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
