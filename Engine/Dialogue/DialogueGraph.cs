namespace SlavicGame.Engine.Dialogue;

public enum DialogueRequirementKind
{
    QuestPhase,
    Evidence,
    Item,
    Reputation,
    DivineRelationship,
    WorldFlag
}

public enum DialogueEffectKind
{
    AdvanceQuest,
    AddEvidence,
    GiveItem,
    TakeItem,
    ChangeReputation,
    ChangeDivineFavor,
    SetWorldFlag
}

public sealed record DialogueRequirement(
    DialogueRequirementKind Kind,
    string TargetId,
    string Value);

public sealed record DialogueEffect(
    DialogueEffectKind Kind,
    string TargetId,
    string Value,
    int Amount = 0);

public sealed record DialogueChoice(
    string Id,
    string Text,
    string? NextNodeId,
    DialogueRequirement[] Requirements,
    DialogueEffect[] Effects);

public sealed record DialogueNode(
    string Id,
    string SpeakerId,
    string Text,
    DialogueChoice[] Choices);

public sealed class DialogueGraph
{
    private readonly Dictionary<string, DialogueNode> _nodes = new(StringComparer.Ordinal);

    public string Id { get; }
    public string StartNodeId { get; }

    public DialogueGraph(string id, string startNodeId, IEnumerable<DialogueNode> nodes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(startNodeId);
        ArgumentNullException.ThrowIfNull(nodes);
        Id = id;
        StartNodeId = startNodeId;

        foreach (var node in nodes)
        {
            if (!_nodes.TryAdd(node.Id, node))
            {
                throw new ArgumentException($"Dialogue contains duplicate node '{node.Id}'.", nameof(nodes));
            }
        }

        if (!_nodes.ContainsKey(StartNodeId))
        {
            throw new ArgumentException("Dialogue start node does not exist.", nameof(startNodeId));
        }
    }

    public DialogueNode GetNode(string nodeId) =>
        _nodes.TryGetValue(nodeId, out var node)
            ? node
            : throw new KeyNotFoundException($"Dialogue node '{nodeId}' does not exist.");
}
