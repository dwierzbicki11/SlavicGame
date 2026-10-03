using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq24RouteClue
{
    StoryOrSign,
    TerrainObservation
}

public sealed class Mq24Campaign
{
    public const string QuestId = "MQ24";
    public const string NextQuestId = "MQ25";
    public const string MapMismatch = "MQ24_MAP_MISMATCH";
    public const string RouteRelation = "MQ24_ROUTE_RELATION";
    public const string Complete = "MQ24_COMPLETE";
    private const string CluePrefix = "MQ24_ROUTE_CLUE_";
    private readonly GameProgress _progress;

    public Mq24Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!_progress.HasFlag("MQ23_COMPLETE")) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        _progress.SetFlag(MapMismatch);
        return true;
    }

    public bool RecordRouteClue(Mq24RouteClue clue)
    {
        if (!IsActive()) return false;
        _progress.SetFlag(ClueFlag(clue));
        return true;
    }

    public bool HasBothRouteClues => Enum.GetValues<Mq24RouteClue>().All(clue => _progress.HasFlag(ClueFlag(clue)));

    public bool AttemptTrialPassage(bool routeAppliedCorrectly)
    {
        if (!IsActive() || !HasBothRouteClues || _progress.HasFlag(RouteRelation)) return false;
        if (!routeAppliedCorrectly) return false;
        _progress.SetFlag(RouteRelation);
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !_progress.HasFlag(RouteRelation)) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string ClueFlag(Mq24RouteClue clue) => CluePrefix + clue.ToString().ToUpperInvariant();
}
