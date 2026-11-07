using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq55ParentBResolution
{
    NotPresent,
    Depart,
    RemainInNavia,
    RiskyMaterialization
}

public enum Mq55DivineContractResolution
{
    NoneActive,
    Fulfilled,
    Renegotiated,
    BrokenOrRejected
}

/// <summary>
/// Durable MQ55 progression ("To, co zostaje").
/// Resolves personal costs (Parent B and divine contracts) after E1–E5 is chosen in MQ54,
/// without ever mutating the selected world architecture.
/// </summary>
public sealed class Mq55Campaign
{
    public const string QuestId = "MQ55";
    public const string NextQuestId = "MQ56";
    public const string ParentBResolutionPrefix = "MQ55_PARENT_B_RESOLUTION_";
    public const string DivineContractResolutionPrefix = "MQ55_DIVINE_CONTRACT_RESOLUTION_";
    public const string FinalCostLocked = "MQ55_FINAL_COST_LOCKED";
    public const string Complete = "MQ55_COMPLETE";

    private readonly GameProgress _progress;

    public Mq55Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        var mq54 = new Mq54Campaign(_progress);
        if (!_progress.HasFlag(Mq54Campaign.Complete) || mq54.SelectedArchitecture is null)
            return false;

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);

        return true;
    }

    public bool IsParentBPresent =>
        _progress.HasFlag(Mq42Campaign.ParentBDecision) &&
        !_progress.HasFlag(Mq42Campaign.Skipped);

    public bool HasActiveDivineContracts =>
        _progress.DivineRelationships.Relationships.Any(r =>
            r.IsPatron || r.Obligations.Count > 0);

    public Mq55ParentBResolution? ParentBResolution
    {
        get
        {
            var matches = Enum.GetValues<Mq55ParentBResolution>()
                .Where(res => _progress.HasFlag(ParentBFlag(res)))
                .ToArray();

            return matches.Length == 1 ? matches[0] : null;
        }
    }

    public Mq55DivineContractResolution? DivineContractResolution
    {
        get
        {
            var matches = Enum.GetValues<Mq55DivineContractResolution>()
                .Where(res => _progress.HasFlag(DivineContractFlag(res)))
                .ToArray();

            return matches.Length == 1 ? matches[0] : null;
        }
    }

    public bool ResolveParentB(Mq55ParentBResolution resolution)
    {
        if (!IsActive() || ParentBResolution is not null)
            return false;

        if (IsParentBPresent)
        {
            if (resolution == Mq55ParentBResolution.NotPresent)
                return false;
        }
        else
        {
            if (resolution != Mq55ParentBResolution.NotPresent)
                return false;
        }

        _progress.SetFlag(ParentBFlag(resolution));
        return true;
    }

    public bool ResolveDivineContracts(Mq55DivineContractResolution resolution)
    {
        if (!IsActive() || DivineContractResolution is not null)
            return false;

        if (HasActiveDivineContracts)
        {
            if (resolution == Mq55DivineContractResolution.NoneActive)
                return false;
        }
        else
        {
            if (resolution is not (Mq55DivineContractResolution.NoneActive or Mq55DivineContractResolution.BrokenOrRejected))
                return false;
        }

        _progress.SetFlag(DivineContractFlag(resolution));
        return true;
    }

    public bool LockFinalCost()
    {
        var mq54 = new Mq54Campaign(_progress);
        if (!IsActive() ||
            mq54.SelectedArchitecture is null ||
            ParentBResolution is null ||
            DivineContractResolution is null)
        {
            return false;
        }

        _progress.SetFlag(FinalCostLocked);
        return true;
    }

    public bool CompleteQuest()
    {
        var mq54 = new Mq54Campaign(_progress);
        if (_progress.HasFlag(Complete) ||
            !IsActive() ||
            !_progress.HasFlag(FinalCostLocked) ||
            mq54.SelectedArchitecture is null)
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

    public static string ParentBFlag(Mq55ParentBResolution resolution) =>
        ParentBResolutionPrefix + resolution.ToString().ToUpperInvariant();

    public static string DivineContractFlag(Mq55DivineContractResolution resolution) =>
        DivineContractResolutionPrefix + resolution.ToString().ToUpperInvariant();

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
