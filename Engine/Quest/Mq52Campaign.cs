using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq52Resolution
{
    Battle,
    Negotiation,
    Alliance,
    MultiConflict
}

public enum Mq52WszeborState
{
    AbsentOrDefeated,
    Hostile,
    Guarded,
    Cooperative
}

public enum Mq52GuardState
{
    HostileBlockade,
    ConditionalWatch,
    CooperativeCordon
}

public enum Mq52FinalSupportState
{
    Minimal,
    Contested,
    Coordinated
}

/// <summary>
/// Durable MQ52 progression ("Wszebor / Straż").
/// Computes and locks initial faction/character configuration from earlier campaign flags,
/// then resolves any of the four legal confrontation paths into node access for MQ53.
/// </summary>
public sealed class Mq52Campaign
{
    public const string QuestId = "MQ52";
    public const string NextQuestId = "MQ53";
    public const string ConfigurationLocked = "MQ52_CONFIGURATION_LOCKED";
    public const string Complete = "MQ52_COMPLETE";
    public const string ResolutionPrefix = "MQ52_RESOLUTION_";
    public const string WszeborStatePrefix = "MQ52_WSZEBOR_STATE_";
    public const string GuardStatePrefix = "MQ52_GUARD_STATE_";
    public const string FinalSupportStatePrefix = "MQ52_FINAL_SUPPORT_STATE_";

    private readonly GameProgress _progress;

    public Mq52Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq51Campaign.Complete))
            return false;

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);

        LockConfiguration();
        return true;
    }

    public bool LockConfiguration()
    {
        if (!IsActive())
            return false;

        if (_progress.HasFlag(ConfigurationLocked))
            return true;

        var wszeborState = EvaluateWszeborState();
        var guardState = EvaluateGuardState();

        foreach (var state in Enum.GetValues<Mq52WszeborState>())
            _progress.SetFlag(WszeborFlag(state), state == wszeborState);

        foreach (var state in Enum.GetValues<Mq52GuardState>())
            _progress.SetFlag(GuardFlag(state), state == guardState);

        _progress.SetFlag(ConfigurationLocked);
        return true;
    }

    public Mq52WszeborState? WszeborState =>
        Enum.GetValues<Mq52WszeborState>()
            .Cast<Mq52WszeborState?>()
            .FirstOrDefault(state => _progress.HasFlag(WszeborFlag(state!.Value)));

    public Mq52GuardState? GuardState =>
        Enum.GetValues<Mq52GuardState>()
            .Cast<Mq52GuardState?>()
            .FirstOrDefault(state => _progress.HasFlag(GuardFlag(state!.Value)));

    public Mq52Resolution? Resolution =>
        Enum.GetValues<Mq52Resolution>()
            .Cast<Mq52Resolution?>()
            .FirstOrDefault(value => _progress.HasFlag(ResolutionFlag(value!.Value)));

    public Mq52FinalSupportState? FinalSupportState =>
        Enum.GetValues<Mq52FinalSupportState>()
            .Cast<Mq52FinalSupportState?>()
            .FirstOrDefault(value => _progress.HasFlag(SupportFlag(value!.Value)));

    public bool ResolveConfrontation(Mq52Resolution resolution)
    {
        if (!IsActive() || !_progress.HasFlag(ConfigurationLocked) || Resolution is not null)
            return false;

        foreach (var value in Enum.GetValues<Mq52Resolution>())
            _progress.SetFlag(ResolutionFlag(value), value == resolution);

        var supportState = ComputeSupportState(resolution);
        foreach (var value in Enum.GetValues<Mq52FinalSupportState>())
            _progress.SetFlag(SupportFlag(value), value == supportState);

        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) ||
            !IsActive() ||
            !_progress.HasFlag(ConfigurationLocked) ||
            Resolution is null ||
            FinalSupportState is null)
        {
            return false;
        }

        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);

        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable)
            next.SetPhase(QuestPhase.Offered);

        return true;
    }

    private Mq52WszeborState EvaluateWszeborState()
    {
        if (_progress.HasFlag("WSZEBOR_CONFRONTATION_OUTCOME_COMBAT") &&
            _progress.HasFlag("MQ33_WSZEBOR_STANCE_HOSTILE"))
        {
            return Mq52WszeborState.AbsentOrDefeated;
        }

        if (_progress.HasFlag("MQ33_WSZEBOR_STANCE_COOPERATIVE") ||
            _progress.HasFlag("WSZEBOR_CONFRONTATION_OUTCOME_TEMPORARYACCORD"))
        {
            return Mq52WszeborState.Cooperative;
        }

        if (_progress.HasFlag("MQ33_WSZEBOR_STANCE_HOSTILE"))
            return Mq52WszeborState.Hostile;

        if (_progress.HasFlag("MQ33_WSZEBOR_STANCE_GUARDED") ||
            _progress.HasFlag("WSZEBOR_CONFRONTATION_OUTCOME_CONVERSATION"))
        {
            return Mq52WszeborState.Guarded;
        }

        return Mq52WszeborState.AbsentOrDefeated;
    }

    private Mq52GuardState EvaluateGuardState()
    {
        if (_progress.HasFlag(MainQuestCampaign.Mq11DecisionOpposition))
            return Mq52GuardState.HostileBlockade;

        if (_progress.HasFlag(MainQuestCampaign.Mq11DecisionConditional))
            return Mq52GuardState.CooperativeCordon;

        return Mq52GuardState.ConditionalWatch;
    }

    private Mq52FinalSupportState ComputeSupportState(Mq52Resolution resolution) => resolution switch
    {
        Mq52Resolution.Alliance => Mq52FinalSupportState.Coordinated,
        Mq52Resolution.Negotiation => GuardState == Mq52GuardState.HostileBlockade
            ? Mq52FinalSupportState.Contested
            : Mq52FinalSupportState.Coordinated,
        Mq52Resolution.Battle => WszeborState == Mq52WszeborState.Cooperative ||
                                 GuardState == Mq52GuardState.CooperativeCordon
            ? Mq52FinalSupportState.Contested
            : Mq52FinalSupportState.Minimal,
        Mq52Resolution.MultiConflict => Mq52FinalSupportState.Minimal,
        _ => throw new ArgumentOutOfRangeException(nameof(resolution))
    };

    private static string ResolutionFlag(Mq52Resolution resolution) =>
        ResolutionPrefix + resolution.ToString().ToUpperInvariant();

    private static string WszeborFlag(Mq52WszeborState state) =>
        WszeborStatePrefix + state.ToString().ToUpperInvariant();

    private static string GuardFlag(Mq52GuardState state) =>
        GuardStatePrefix + state.ToString().ToUpperInvariant();

    private static string SupportFlag(Mq52FinalSupportState state) =>
        FinalSupportStatePrefix + state.ToString().ToUpperInvariant();

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
