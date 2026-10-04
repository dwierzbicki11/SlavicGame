using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

public enum Mq25DataPackage
{
    Nadborze,
    Przymorze,
    KamienneWyzyny,
    Arel
}

public enum Mq25SharedConnection
{
    CrossRegionalRoute,
    AnchorMaterialDistribution
}

public sealed class Mq25Campaign
{
    public const string QuestId = "MQ25";
    public const string NextQuestId = "MQ30";
    public const string MulticulturalOrigin = "NETWORK_MULTICULTURAL_ORIGIN";
    public const string Complete = "MQ25_COMPLETE";
    private const string PackagePrefix = "MQ25_PACKAGE_";
    private const string ConnectionPrefix = "MQ25_CONNECTION_";
    private readonly GameProgress _progress;

    public Mq25Campaign(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
    }

    public bool Begin()
    {
        if (!HasActTwoCompletion()) return false;
        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);
        return true;
    }

    public bool ValidateCriticalPackages()
    {
        if (!IsActive() || !HasCriticalEvidence()) return false;
        foreach (var package in Enum.GetValues<Mq25DataPackage>())
            _progress.SetFlag(PackageFlag(package));
        return true;
    }

    public bool HasAllCriticalPackages => Enum.GetValues<Mq25DataPackage>().All(package => _progress.HasFlag(PackageFlag(package)));

    public bool RecordSharedConnection(Mq25SharedConnection connection)
    {
        if (!IsActive() || !HasAllCriticalPackages) return false;
        _progress.SetFlag(ConnectionFlag(connection));
        return true;
    }

    public bool HasRequiredConnections => Enum.GetValues<Mq25SharedConnection>().All(connection => _progress.HasFlag(ConnectionFlag(connection)));

    public bool ConcludeSynthesis()
    {
        if (!IsActive() || !HasAllCriticalPackages || !HasRequiredConnections) return false;
        _progress.SetFlag(MulticulturalOrigin);
        return true;
    }

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) || !IsActive() || !_progress.HasFlag(MulticulturalOrigin)) return false;
        _progress.Quests.Get(QuestId).Resolve(QuestResolution.Other);
        _progress.SetFlag(Complete);
        _progress.Quests.Get(NextQuestId).SetPhase(QuestPhase.Offered);
        return true;
    }

    private bool HasActTwoCompletion() => new[] { "MQ20_COMPLETE", "MQ21_COMPLETE", "MQ22_COMPLETE", "MQ23_COMPLETE", "MQ24_COMPLETE" }.All(_progress.HasFlag);

    private bool HasCriticalEvidence() =>
        _progress.HasFlag("NETWORK_HYPOTHESIS") &&
        _progress.HasFlag("MQ20_REMOTE_NODE_EVIDENCE") &&
        _progress.HasFlag("MQ22_ANCHOR_MATERIAL") &&
        _progress.HasFlag("MQ24_ROUTE_RELATION");

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
    private static string PackageFlag(Mq25DataPackage package) => PackagePrefix + package.ToString().ToUpperInvariant();
    private static string ConnectionFlag(Mq25SharedConnection connection) => ConnectionPrefix + connection.ToString().ToUpperInvariant();
}
