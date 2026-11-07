using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

/// <summary>
/// Durable MQ56 progression ("Epilog").
/// Freezes an immutable EpilogueSnapshot in deterministic narrative order from recorded
/// campaign flags and marks CAMPAIGN_COMPLETE without re-running world mutations on reload.
/// </summary>
public sealed class Mq56Campaign
{
    public const string QuestId = "MQ56";
    public const string EpilogueSnapshotCreated = "MQ56_EPILOGUE_SNAPSHOT_CREATED";
    public const string ModulesLocked = "MQ56_MODULES_LOCKED";
    public const string CampaignComplete = "CAMPAIGN_COMPLETE";
    public const string Complete = "MQ56_COMPLETE";
    public const string ModulePrefix = "MQ56_MODULE_";

    private static readonly string[] NarrativeSlotOrder =
    [
        "01_GLOBAL_",
        "02_REGION_R0_",
        "03_REGION_R1_",
        "04_REGION_R2_",
        "05_REGION_R3_",
        "06_REGION_R4_",
        "07_REGION_R5_",
        "08_POLITICAL_",
        "09_DIVINE_",
        "10_FAMILY_",
        "11_HUNTER_"
    ];

    private readonly GameProgress _progress;

    public Mq56Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        var mq54 = new Mq54Campaign(_progress);
        var mq55 = new Mq55Campaign(_progress);
        if (!_progress.HasFlag(Mq55Campaign.Complete) ||
            !_progress.HasFlag(Mq55Campaign.FinalCostLocked) ||
            mq54.SelectedArchitecture is null ||
            mq55.ParentBResolution is null ||
            mq55.DivineContractResolution is null)
        {
            return false;
        }

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);

        return true;
    }

    public bool CreateEpilogueSnapshot()
    {
        if (!IsActive())
            return false;

        // Immutable once created: reload never recalculates modules from later runtime changes.
        if (_progress.HasFlag(EpilogueSnapshotCreated) && _progress.HasFlag(ModulesLocked))
            return true;

        var mq54 = new Mq54Campaign(_progress);
        var mq55 = new Mq55Campaign(_progress);
        if (mq54.SelectedArchitecture is not { } architecture ||
            mq55.ParentBResolution is not { } parentB ||
            mq55.DivineContractResolution is not { } divineContract)
        {
            return false;
        }

        var modules = new[]
        {
            $"{ModulePrefix}01_GLOBAL_{architecture.ToString().ToUpperInvariant()}",
            $"{ModulePrefix}02_REGION_R0_{EvaluateR0Outcome()}",
            $"{ModulePrefix}03_REGION_R1_{EvaluateR1Outcome()}",
            $"{ModulePrefix}04_REGION_R2_{EvaluateR2Outcome()}",
            $"{ModulePrefix}05_REGION_R3_{EvaluateR3Outcome()}",
            $"{ModulePrefix}06_REGION_R4_{EvaluateR4Outcome()}",
            $"{ModulePrefix}07_REGION_R5_{EvaluateR5Outcome()}",
            $"{ModulePrefix}08_POLITICAL_{EvaluatePoliticalOutcome(architecture)}",
            $"{ModulePrefix}09_DIVINE_{divineContract.ToString().ToUpperInvariant()}",
            $"{ModulePrefix}10_FAMILY_{EvaluateFamilyOutcome(parentB)}",
            $"{ModulePrefix}11_HUNTER_{EvaluateHunterOutcome(architecture)}"
        };

        foreach (var module in modules)
            _progress.SetFlag(module);

        _progress.SetFlag(EpilogueSnapshotCreated);
        _progress.SetFlag(ModulesLocked);
        return true;
    }

    public IReadOnlyList<string> GetLockedModules()
    {
        if (!_progress.HasFlag(EpilogueSnapshotCreated) || !_progress.HasFlag(ModulesLocked))
            return [];

        var allFlags = _progress.CaptureFlags();
        var ordered = new List<string>(NarrativeSlotOrder.Length);
        foreach (var slot in NarrativeSlotOrder)
        {
            var prefix = ModulePrefix + slot;
            var match = allFlags.FirstOrDefault(flag =>
                flag.StartsWith(prefix, StringComparison.Ordinal));
            if (match is not null)
                ordered.Add(match);
        }

        return ordered;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) ||
            !IsActive() ||
            !_progress.HasFlag(EpilogueSnapshotCreated) ||
            !_progress.HasFlag(ModulesLocked) ||
            GetLockedModules().Count != NarrativeSlotOrder.Length)
        {
            return false;
        }

        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(CampaignComplete);
        _progress.SetFlag(Complete);
        return true;
    }

    private string EvaluateR0Outcome()
    {
        if (_progress.HasFlag("swamp.apparition-released") || _progress.HasFlag(R0FordSideQuest.AccessFlag))
            return "CROSSING_STABILIZED";
        if (_progress.HasFlag("swamp.apparition-bound"))
            return "NIGHT_PACT_CUSTOM";
        if (_progress.HasFlag("swamp.anchor-destroyed"))
            return "ANCHOR_DESTROYED_UNCERTAINTY";
        return "CAUTIOUS_BORDERLAND";
    }

    private string EvaluateR1Outcome()
    {
        if (_progress.HasFlag(MainQuestCampaign.Mq11DecisionConditional))
            return "CENTRALIZED_OVERSIGHT";
        if (_progress.HasFlag(MainQuestCampaign.Mq11DecisionOpposition))
            return "CONTESTED_AUTONOMY";
        if (_progress.HasFlag(MainQuestCampaign.Mq11DecisionLocal))
            return "LOCAL_SETTLEMENT_ACCORD";
        return "BALANCED_ADMINISTRATION";
    }

    private string EvaluateR2Outcome()
    {
        if (_progress.HasFlag(Mq12Campaign.WitnessConfirmed))
            return "LOCAL_CUSTODIAN_COMPACT";
        if (_progress.HasFlag(Mq12Campaign.EnvironmentalFallback))
            return "GUARDED_FOREST_BORDERS";
        return "CAUTious_WOODLAND".ToUpperInvariant();
    }

    private string EvaluateR3Outcome()
    {
        if (_progress.HasFlag("MQ21_D01_LEAGUEPREFERENCE"))
            return "OPEN_LEAGUE_TRADE";
        if (_progress.HasFlag("MQ21_D01_NADBORPREFERENCE"))
            return "REGULATED_ESTUARY";
        if (_progress.HasFlag("MQ21_D01_LIMITEDBOTH"))
            return "SHARED_TRANSIT_COMPACT";
        return "STANDARD_PORT_TRAFFIC";
    }

    private string EvaluateR4Outcome()
    {
        if (_progress.HasFlag("MQ23_D01_EXTRACTIONLIMITRESERVE"))
            return "ANCHOR_RESERVE_PROTECTED";
        if (_progress.HasFlag("MQ23_D01_SUPERVISEDSPLIT"))
            return "SUPERVISED_MINE_SPLIT";
        if (_progress.HasFlag("MQ23_D01_CLAIMANTACONTROL") || _progress.HasFlag("MQ23_D01_CLAIMANTBCONTROL"))
            return "SINGLE_PRINCIPALITY_MINING";
        return "CONTESTED_HIGHLAND_PASSES";
    }

    private string EvaluateR5Outcome() =>
        _progress.HasFlag(Mq24Campaign.RouteRelation)
            ? "SEASONAL_ROUTE_MEMORY_PRESERVED"
            : "SHIFTING_STEPPE_BORDERS";

    private string EvaluatePoliticalOutcome(EndingArchitecture architecture) => architecture switch
    {
        EndingArchitecture.E1 => "RENEWED_CLOSURE_AUTHORITY",
        EndingArchitecture.E2 => "FEDERATED_NODE_COMMUNITIES",
        EndingArchitecture.E3 => "THRESHOLD_COVENANT_COUNCIL",
        EndingArchitecture.E4 => "CULT_INSTITUTION_ASCENDANCE",
        EndingArchitecture.E5 => "GREAT_OPENING_REALIGNMENT",
        _ => "FRAGMENTED_COMPROMISE"
    };

    private string EvaluateFamilyOutcome(Mq55ParentBResolution parentB)
    {
        if (parentB != Mq55ParentBResolution.NotPresent)
            return $"FULL_TRUTH_{parentB.ToString().ToUpperInvariant()}";

        if (_progress.HasFlag(Mq32Campaign.CoreReconstructed))
            return "PARTIAL_TRUTH_NO_PARENT_B";

        return "OPEN_FAMILY_MYSTERY";
    }

    private string EvaluateHunterOutcome(EndingArchitecture architecture)
    {
        if (architecture == EndingArchitecture.E2)
            return "NODE_CUSTODIAN";
        if (architecture == EndingArchitecture.E3)
            return "INDEPENDENT_RESEARCHER";
        if (_progress.HasFlag("MQ52_RESOLUTION_NEGOTIATION") || _progress.HasFlag("MQ52_RESOLUTION_ALLIANCE"))
            return "POLITICAL_MEDIATOR";
        if (architecture == EndingArchitecture.E5)
            return "WAYFARER";
        return "ACTIVE_HUNTER";
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
