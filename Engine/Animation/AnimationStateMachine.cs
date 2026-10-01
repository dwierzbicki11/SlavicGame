namespace SlavicGame.Engine.Animation;

public sealed class AnimationStateMachine
{
    private readonly HashSet<string> _states = new(StringComparer.Ordinal);

    public string? CurrentState { get; private set; }
    public float NormalizedTime { get; private set; }

    public void Register(string stateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stateId);
        _states.Add(stateId);
    }

    public void Play(string stateId, bool restart = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stateId);
        if (!_states.Contains(stateId))
        {
            throw new KeyNotFoundException($"Animation state '{stateId}' is not registered.");
        }

        if (!restart && CurrentState == stateId)
        {
            return;
        }

        CurrentState = stateId;
        NormalizedTime = 0f;
    }

    public void Advance(double deltaSeconds, double durationSeconds)
    {
        if (CurrentState is null ||
            !double.IsFinite(deltaSeconds) || deltaSeconds < 0 ||
            !double.IsFinite(durationSeconds) || durationSeconds <= 0)
        {
            return;
        }

        NormalizedTime = Math.Clamp(
            NormalizedTime + (float)(deltaSeconds / durationSeconds),
            0f,
            1f);
    }
}
