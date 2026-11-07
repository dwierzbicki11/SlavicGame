using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.Quest;

/// <summary>
/// Durable MQ53 progression ("Węzły").
/// Tracks stabilization of each required threshold node independently in durable flags
/// so reload never resets a stabilized node or double-counts activation resources.
/// </summary>
public sealed class Mq53Campaign
{
    public const string QuestId = "MQ53";
    public const string NextQuestId = "MQ54";
    public const string Started = "MQ53_STARTED";
    public const string RequiredNodesStable = "MQ53_REQUIRED_NODES_STABLE";
    public const string Complete = "MQ53_COMPLETE";
    public const string NodePrefix = "MQ53_NODE_";
    public const string NodeSuffix = "_STABLE";

    public static readonly IReadOnlyList<string> DefaultRequiredNodes =
        ["ANCHOR_NORTH", "ANCHOR_EAST", "ANCHOR_CORE"];

    private readonly GameProgress _progress;
    private readonly string[] _requiredNodeIds;

    public Mq53Campaign(GameProgress progress, IEnumerable<string>? requiredNodeIds = null)
    {
        ArgumentNullException.ThrowIfNull(progress);
        _progress = progress;
        var nodes = (requiredNodeIds ?? DefaultRequiredNodes)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(NormalizeNodeId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (nodes.Length == 0)
            throw new ArgumentException("MQ53 requires at least one critical node ID.", nameof(requiredNodeIds));

        _requiredNodeIds = nodes;
    }

    public IReadOnlyList<string> RequiredNodeIds => _requiredNodeIds;

    public bool Begin()
    {
        if (!_progress.HasFlag(Mq52Campaign.Complete))
            return false;

        var quest = _progress.Quests.Get(QuestId);
        if (quest.Phase is QuestPhase.Unavailable or QuestPhase.Offered)
            quest.SetPhase(QuestPhase.Investigation);

        _progress.SetFlag(Started);
        if (AllRequiredNodesStable)
            _progress.SetFlag(RequiredNodesStable);

        return true;
    }

    public bool StabilizeNode(string nodeId)
    {
        if (!IsActive() || !_progress.HasFlag(Started) || string.IsNullOrWhiteSpace(nodeId))
            return false;

        var normalized = NormalizeNodeId(nodeId);
        if (!_requiredNodeIds.Contains(normalized, StringComparer.Ordinal))
            return false;

        var flag = NodeFlag(normalized);
        if (_progress.HasFlag(flag))
            return false;

        _progress.SetFlag(flag);
        if (AllRequiredNodesStable)
            _progress.SetFlag(RequiredNodesStable);

        return true;
    }

    public bool IsNodeStable(string nodeId) =>
        !string.IsNullOrWhiteSpace(nodeId) &&
        _progress.HasFlag(NodeFlag(NormalizeNodeId(nodeId)));

    public int StabilizedNodeCount => _requiredNodeIds.Count(IsNodeStable);

    public bool AllRequiredNodesStable => _requiredNodeIds.All(IsNodeStable);

    public bool CompleteQuest()
    {
        if (_progress.HasFlag(Complete) ||
            !IsActive() ||
            !_progress.HasFlag(RequiredNodesStable) ||
            !AllRequiredNodesStable)
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

    public static string NodeFlag(string nodeId) =>
        NodePrefix + NormalizeNodeId(nodeId) + NodeSuffix;

    private static string NormalizeNodeId(string nodeId) =>
        nodeId.Trim().Replace('-', '_').ToUpperInvariant();

    private bool IsActive() => _progress.Quests.Get(QuestId).Phase == QuestPhase.Investigation;
}
