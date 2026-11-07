using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Reputation;

namespace SlavicGame.Engine.Quest;

public enum Mq50PreparationCategory
{
    People,
    Materials,
    Knowledge,
    DivineTerms
}

public enum Mq50PreparationTier
{
    None = 0,
    Base = 1,
    Reinforced = 2,
    Full = 3
}

/// <summary>
/// Durable MQ50 progression ("Stare zobowiązania").
/// Evaluates prior campaign and regional state into an idempotent preparation snapshot
/// without turning optional side content into a mandatory gate.
/// </summary>
public sealed class Mq50Campaign
{
    public const string QuestId = "MQ50";
    public const string NextQuestId = "MQ51";
    public const string Started = "MQ50_STARTED";
    public const string PreparationSnapshotCreated = "MQ50_PREPARATION_SNAPSHOT_CREATED";
    public const string PeopleTierPrefix = "MQ50_PEOPLE_TIER_";
    public const string MaterialsTierPrefix = "MQ50_MATERIALS_TIER_";
    public const string KnowledgeTierPrefix = "MQ50_KNOWLEDGE_TIER_";
    public const string DivineTermsTierPrefix = "MQ50_DIVINE_TERMS_TIER_";
    public const string Complete = "MQ50_COMPLETE";

    private readonly GameProgress _progress;

    public Mq50Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq44Campaign.Complete))
            return false;

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);

        _progress.SetFlag(Started);
        return true;
    }

    public bool CreatePreparationSnapshot()
    {
        if (!IsActive() || !_progress.HasFlag(Started))
            return false;

        // Idempotent snapshot: reload or repeat call never re-evaluates or duplicates support.
        if (_progress.HasFlag(PreparationSnapshotCreated))
            return true;

        SetCategoryTier(Mq50PreparationCategory.People, ComputePeopleTier());
        SetCategoryTier(Mq50PreparationCategory.Materials, ComputeMaterialsTier());
        SetCategoryTier(Mq50PreparationCategory.Knowledge, ComputeKnowledgeTier());
        SetCategoryTier(Mq50PreparationCategory.DivineTerms, ComputeDivineTermsTier());

        _progress.SetFlag(PreparationSnapshotCreated);
        return true;
    }

    public Mq50PreparationTier GetTier(Mq50PreparationCategory category)
    {
        var prefix = CategoryPrefix(category);
        foreach (var tier in new[] { Mq50PreparationTier.Full, Mq50PreparationTier.Reinforced, Mq50PreparationTier.Base })
        {
            if (_progress.HasFlag(prefix + tier.ToString().ToUpperInvariant()))
                return tier;
        }

        return Mq50PreparationTier.None;
    }

    public bool AllCategoriesPrepared =>
        _progress.HasFlag(PreparationSnapshotCreated) &&
        Enum.GetValues<Mq50PreparationCategory>()
            .All(category => GetTier(category) >= Mq50PreparationTier.Base);

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !AllCategoriesPrepared)
            return false;

        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);

        var next = _progress.Quests.Get(NextQuestId);
        if (next.Phase == QuestPhase.Unavailable)
            next.SetPhase(QuestPhase.Offered);

        return true;
    }

    private Mq50PreparationTier ComputePeopleTier()
    {
        var reinforcements = 0;
        if (_progress.HasFlag(MainQuestCampaign.Mq11DecisionConditional) ||
            _progress.HasFlag(MainQuestCampaign.Mq11DecisionLocal) ||
            _progress.HasFlag("MQ21_COMPLETE"))
        {
            reinforcements++;
        }

        if (_progress.HasFlag(R0FordSideQuest.AccessFlag) ||
            _progress.Reputation.Get(ReputationScope.Village, "old-village") > 0 ||
            _progress.HasFlag("MQ33_WSZEBOR_STANCE_COOPERATIVE"))
        {
            reinforcements++;
        }

        return reinforcements switch
        {
            >= 2 => Mq50PreparationTier.Full,
            1 => Mq50PreparationTier.Reinforced,
            _ => Mq50PreparationTier.Base
        };
    }

    private Mq50PreparationTier ComputeMaterialsTier()
    {
        var reinforcements = 0;
        if (_progress.HasFlag("MQ22_ANCHOR_MATERIAL") || _progress.HasFlag(Mq23Campaign.Complete))
            reinforcements++;

        if (_progress.HasFlag(Mq23Campaign.DecisionPrefix + "EXTRACTIONLIMITRESERVE") ||
            _progress.HasFlag(Mq23Campaign.DecisionPrefix + "SUPERVISEDSPLIT") ||
            _progress.HasFlag("sq_r0_02.completed"))
        {
            reinforcements++;
        }

        return reinforcements switch
        {
            >= 2 => Mq50PreparationTier.Full,
            1 => Mq50PreparationTier.Reinforced,
            _ => Mq50PreparationTier.Base
        };
    }

    private Mq50PreparationTier ComputeKnowledgeTier()
    {
        var reinforcements = 0;
        if (_progress.HasFlag(Mq25Campaign.MulticulturalOrigin) &&
            _progress.HasFlag(Mq32Campaign.CoreReconstructed))
        {
            reinforcements++;
        }

        if (_progress.HasFlag(Mq43Campaign.SplotTruthKnown) &&
            (_progress.HasFlag(Mq33Campaign.DataPackageAvailable) ||
             _progress.HasFlag("r0_old_mound_known")))
        {
            reinforcements++;
        }

        return reinforcements switch
        {
            >= 2 => Mq50PreparationTier.Full,
            1 => Mq50PreparationTier.Reinforced,
            _ => Mq50PreparationTier.Base
        };
    }

    private Mq50PreparationTier ComputeDivineTermsTier()
    {
        var relationships = _progress.DivineRelationships.Relationships;
        if (relationships.Any(r => r.IsPatron && !r.Obligations.Any(o => o.Broken)))
            return Mq50PreparationTier.Full;

        if (relationships.Any(r => r.Favor > 0 || r.Obligations.Any(o => o.Fulfilled && !o.Broken)))
            return Mq50PreparationTier.Reinforced;

        return Mq50PreparationTier.Base;
    }

    private void SetCategoryTier(Mq50PreparationCategory category, Mq50PreparationTier selected)
    {
        var prefix = CategoryPrefix(category);
        foreach (var tier in new[] { Mq50PreparationTier.Base, Mq50PreparationTier.Reinforced, Mq50PreparationTier.Full })
        {
            _progress.SetFlag(prefix + tier.ToString().ToUpperInvariant(), tier == selected);
        }
    }

    private static string CategoryPrefix(Mq50PreparationCategory category) => category switch
    {
        Mq50PreparationCategory.People => PeopleTierPrefix,
        Mq50PreparationCategory.Materials => MaterialsTierPrefix,
        Mq50PreparationCategory.Knowledge => KnowledgeTierPrefix,
        Mq50PreparationCategory.DivineTerms => DivineTermsTierPrefix,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
