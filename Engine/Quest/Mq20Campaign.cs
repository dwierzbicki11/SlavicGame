using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq20MapAccessRoute
{
    Favor,
    Negotiation,
    Reputation,
    EnvironmentalEvidence
}

public enum Mq20EstuaryOutcome
{
    Resolved,
    Bypassed
}

public sealed class Mq20Campaign
{
    public const string QuestId = "MQ20";
    public const string NextQuestId = "MQ21";
    public const string EstuaryBlocked = "MQ20_ESTUARY_BLOCKED";
    public const string RemoteNodeEvidence = "MQ20_REMOTE_NODE_EVIDENCE";
    public const string Complete = "MQ20_COMPLETE";
    private const string MapAccessPrefix = "MQ20_MAP_ACCESS_";
    private const string EstuaryOutcomePrefix = "MQ20_ESTUARY_";
    private readonly GameProgress _progress;

    public Mq20Campaign(GameProgress progress) { ArgumentNullException.ThrowIfNull(progress); _progress = progress; }
    public bool Begin() { if (!_progress.HasFlag("MQ13_COMPLETE")) return false; var quest = _progress.Quests.Get(QuestId); if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered) quest.SetPhase(QuestPhase.Investigation); _progress.SetFlag(EstuaryBlocked); return true; }
    public bool GainMapAccess(Mq20MapAccessRoute route) { if (!IsActive()) return false; _progress.SetFlag(MapAccessFlag(route)); return true; }
    public bool HasMapAccess => Enum.GetValues<Mq20MapAccessRoute>().Any(route => _progress.HasFlag(MapAccessFlag(route)));
    public bool CompareRemoteNode() { if (!IsActive() || !HasMapAccess || !_progress.HasFlag("NETWORK_HYPOTHESIS")) return false; _progress.SetFlag(RemoteNodeEvidence); return true; }
    public bool SetEstuaryOutcome(Mq20EstuaryOutcome outcome) { if (!IsActive()) return false; foreach (var value in Enum.GetValues<Mq20EstuaryOutcome>()) _progress.SetFlag(EstuaryOutcomeFlag(value), value == outcome); return true; }
    public bool CompleteQuest() { if (_progress.HasFlag(Complete) || !_progress.HasFlag(RemoteNodeEvidence) || !HasEstuaryOutcome) return false; _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other); _progress.SetFlag(Complete); _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered); return true; }
    public bool HasEstuaryOutcome => Enum.GetValues<Mq20EstuaryOutcome>().Any(value => _progress.HasFlag(EstuaryOutcomeFlag(value)));
    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string MapAccessFlag(Mq20MapAccessRoute route) => MapAccessPrefix + route.ToString().ToUpperInvariant();
    private static string EstuaryOutcomeFlag(Mq20EstuaryOutcome outcome) => EstuaryOutcomePrefix + outcome.ToString().ToUpperInvariant();
}
