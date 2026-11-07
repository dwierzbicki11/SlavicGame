using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum EndingArchitecture
{
    E1,
    E2,
    E3,
    E4,
    E5
}

/// <summary>
/// Durable MQ54 progression ("Architektura").
/// Evaluates legal availability of ending architectures E1–E5 according to owner specs
/// and persists the chosen architecture before personal costs are resolved in MQ55.
/// </summary>
public sealed class Mq54Campaign
{
    public const string QuestId = "MQ54";
    public const string NextQuestId = "MQ55";
    public const string OptionsEvaluated = "MQ54_OPTIONS_EVALUATED";
    public const string ArchitecturePrefix = "ENDING_ARCHITECTURE_";
    public const string Complete = "MQ54_COMPLETE";

    private readonly GameProgress _progress;

    public Mq54Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq53Campaign.Complete) ||
            !_progress.HasFlag(Mq43Campaign.SplotTruthKnown) ||
            !_progress.HasFlag(Mq34Campaign.Complete))
        {
            return false;
        }

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);

        return true;
    }

    public bool EvaluateOptions()
    {
        if (!IsActive())
            return false;

        _progress.SetFlag(OptionsEvaluated);
        return true;
    }

    public bool IsArchitectureAvailable(EndingArchitecture architecture)
    {
        if (!_progress.HasFlag(Mq53Campaign.Complete) ||
            !_progress.HasFlag(Mq43Campaign.SplotTruthKnown) ||
            !_progress.HasFlag(Mq34Campaign.Complete))
        {
            return false;
        }

        return architecture switch
        {
            EndingArchitecture.E1 => _progress.HasFlag(Mq34Campaign.HardClosureKnown),
            EndingArchitecture.E2 => _progress.HasFlag(Mq34Campaign.DistributedRebuildKnown),
            EndingArchitecture.E3 =>
                _progress.HasFlag(Mq41Campaign.NaviaSplotDistinction) &&
                _progress.HasFlag(Mq43Campaign.SplotTruthKnown) &&
                (_progress.HasFlag(Mq25Campaign.MulticulturalOrigin) ||
                 _progress.HasFlag(Mq32Campaign.CoreReconstructed) ||
                 _progress.HasFlag("MQ52_WSZEBOR_STATE_COOPERATIVE") ||
                 _progress.HasFlag("MQ50_KNOWLEDGE_TIER_FULL") ||
                 _progress.HasFlag("MQ50_KNOWLEDGE_TIER_REINFORCED")),
            EndingArchitecture.E4 =>
                _progress.DivineRelationships.Relationships.Any(r =>
                    (r.IsPatron || r.Favor >= 25) &&
                    !r.Obligations.Any(o => o.Broken)),
            EndingArchitecture.E5 => _progress.HasFlag(Mq34Campaign.OpeningKnown),
            _ => false
        };
    }

    public IReadOnlyList<EndingArchitecture> AvailableArchitectures =>
        Enum.GetValues<EndingArchitecture>()
            .Where(IsArchitectureAvailable)
            .ToArray();

    public EndingArchitecture? SelectedArchitecture
    {
        get
        {
            var matches = Enum.GetValues<EndingArchitecture>()
                .Where(arch => _progress.HasFlag(ArchitectureFlag(arch)))
                .ToArray();

            return matches.Length == 1 && IsArchitectureAvailable(matches[0])
                ? matches[0]
                : null;
        }
    }

    public bool ChooseArchitecture(EndingArchitecture architecture)
    {
        if (!IsActive() ||
            !_progress.HasFlag(OptionsEvaluated) ||
            HasAnyArchitectureFlag() ||
            !IsArchitectureAvailable(architecture))
        {
            return false;
        }

        _progress.SetFlag(ArchitectureFlag(architecture));
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || SelectedArchitecture is null)
            return false;

        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);

        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable)
            next.SetPhase(QuestPhase.Offered);

        return true;
    }

    public static string ArchitectureFlag(EndingArchitecture architecture) =>
        ArchitecturePrefix + architecture.ToString().ToUpperInvariant();

    private bool HasAnyArchitectureFlag() =>
        Enum.GetValues<EndingArchitecture>()
            .Any(arch => _progress.HasFlag(ArchitectureFlag(arch)));

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
