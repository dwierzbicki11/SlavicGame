using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq22ConstructionClue
{
    MaterialLayer,
    ToolingPattern
}

public enum Mq22Stabilization
{
    SecureSite,
    BypassHazard,
    LimitExtraction
}

public sealed class Mq22Campaign
{
    public const string QuestId = "MQ22";
    public const string NextQuestId = "MQ23";
    public const string MineBreach = "MQ22_MINE_BREACH";
    public const string AnchorMaterial = "MQ22_ANCHOR_MATERIAL";
    public const string Complete = "MQ22_COMPLETE";
    private const string CluePrefix = "MQ22_CONSTRUCTION_CLUE_";
    private const string StabilizationPrefix = "MQ22_STABILIZATION_";
    private readonly GameProgress _progress;

    public Mq22Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ21_COMPLETE")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool EnterMineBreach()
    {
        if (!IsActive()) return false;
        _progress.SetFlag(MineBreach);
        return true;
    }

    public bool RecordConstructionClue(Mq22ConstructionClue clue)
    {
        if (!IsActive() || !_progress.HasFlag(MineBreach)) return false;
        _progress.SetFlag(ClueFlag(clue));
        return true;
    }

    public bool HasBothConstructionClues => Enum.GetValues<Mq22ConstructionClue>().All(clue => _progress.HasFlag(ClueFlag(clue)));

    public bool AnalyzeAnchorMaterial(bool optionalNpcAvailable = true)
    {
        if (!IsActive() || !HasBothConstructionClues) return false;
        // The campaign contract guarantees a player/environment fallback when an optional analyst is unavailable.
        _progress.SetFlag(AnchorMaterial);
        return true;
    }

    public bool Stabilize(Mq22Stabilization stabilization)
    {
        if (!IsActive() || !_progress.HasFlag(AnchorMaterial) || HasStabilization) return false;
        foreach (var value in Enum.GetValues<Mq22Stabilization>())
            _progress.SetFlag(StabilizationFlag(value), value == stabilization);
        return true;
    }

    public bool HasStabilization => Enum.GetValues<Mq22Stabilization>().Any(value => _progress.HasFlag(StabilizationFlag(value)));

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !_progress.HasFlag(AnchorMaterial) || !HasStabilization) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string ClueFlag(Mq22ConstructionClue clue) => CluePrefix + clue.ToString().ToUpperInvariant();
    private static string StabilizationFlag(Mq22Stabilization stabilization) => StabilizationPrefix + stabilization.ToString().ToUpperInvariant();
}
