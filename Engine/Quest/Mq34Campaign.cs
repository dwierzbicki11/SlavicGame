using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq34CrisisModel
{
    HardClosure,
    DistributedRebuild,
    Opening
}

public sealed class Mq34Campaign
{
    public const string QuestId = "MQ34";
    public const string NextQuestId = "MQ40";
    public const string Complete = "MQ34_COMPLETE";
    public const string HardClosureKnown = "HARD_CLOSURE_MODEL_KNOWN";
    public const string DistributedRebuildKnown = "DISTRIBUTED_REBUILD_MODEL_KNOWN";
    public const string OpeningKnown = "OPENING_MODEL_KNOWN";
    public const string Act4NaviaLeadAvailable = "ACT4_NAVIA_LEAD_AVAILABLE";
    private const string DetailPrefix = "MQ34_OPTIONAL_DETAIL_";
    private readonly GameProgress _progress;

    public Mq34Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq33Campaign.Complete)) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    // Critical summaries are campaign-state writes. Optional notes may enrich a model,
    // but can never be required to make that model available for comparison.
    public bool PresentModel(Mq34CrisisModel model, bool optionalDetailAvailable = false)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(ModelFlag(model));
        if (optionalDetailAvailable)
            _progress.SetFlag(DetailFlag(model));
        return true;
    }

    public bool HasModel(Mq34CrisisModel model) => _progress.HasFlag(ModelFlag(model));
    public bool HasOptionalDetail(Mq34CrisisModel model) => _progress.HasFlag(DetailFlag(model));
    public bool AllModelsKnown => Enum.GetValues<Mq34CrisisModel>().All(HasModel);

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !AllModelsKnown) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.SetFlag(Act4NaviaLeadAvailable);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;

    private static string ModelFlag(Mq34CrisisModel model) => model switch
    {
        Mq34CrisisModel.HardClosure => HardClosureKnown,
        Mq34CrisisModel.DistributedRebuild => DistributedRebuildKnown,
        Mq34CrisisModel.Opening => OpeningKnown,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    private static string DetailFlag(Mq34CrisisModel model) => $"{DetailPrefix}{model.ToString().ToUpperInvariant()}";
}
