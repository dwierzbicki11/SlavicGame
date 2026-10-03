using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Dialogue;

public sealed class DialogueRuntime
{
    private DialogueGraph? _graph;
    private string? _currentNodeId;

    public bool IsOpen => _graph is not null && _currentNodeId is not null;
    public string? SpeakerId { get; private set; }
    public int SelectedChoiceIndex { get; private set; }
    public string Message { get; private set; } = "";

    public DialogueNode? CurrentNode =>
        IsOpen ? _graph!.GetNode(_currentNodeId!) : null;

    public IReadOnlyList<DialogueChoice> AvailableChoices(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var node = CurrentNode;
        if (node is null)
            return [];

        return node.Choices
            .Where(choice => choice.Requirements.All(req => RequirementMet(world, req)))
            .ToArray();
    }

    public static string MetFlag(string npcId) =>
        $"dialogue.npc.{npcId}.met";

    public static string TeacherReadyFlag(string npcId) =>
        $"dialogue.teacher.{npcId}.ready";

    public bool TryStartNearest(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var actor = world.NpcWorld.FindNearestInteractive(world.PlayerPosition);
        if (actor is null)
            return false;

        return Start(world, actor.Id);
    }

    public bool Start(WorldState world, string npcId)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId);

        if (!world.NpcWorld.IsNearby(npcId, world.PlayerPosition))
            return false;

        _graph = VerticalSliceDialogueCatalog.GetGraph(npcId);
        _currentNodeId = VerticalSliceDialogueCatalog.SelectStartNode(world, npcId);
        SpeakerId = npcId;
        SelectedChoiceIndex = 0;
        Message = "";
        world.Progress.SetFlag(MetFlag(npcId));
        ClampSelection(world);
        return true;
    }

    public void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!IsOpen || SpeakerId is null)
            return;

        if (!world.NpcWorld.IsNearby(SpeakerId, world.PlayerPosition, 5f))
            Close();
    }

    public void MoveSelection(WorldState world, int delta)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!IsOpen || delta == 0)
            return;

        var choices = AvailableChoices(world);
        if (choices.Count == 0)
        {
            SelectedChoiceIndex = 0;
            return;
        }

        SelectedChoiceIndex =
            (SelectedChoiceIndex + delta) % choices.Count;
        if (SelectedChoiceIndex < 0)
            SelectedChoiceIndex += choices.Count;
    }

    public bool Confirm(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!IsOpen)
            return false;

        var choices = AvailableChoices(world);
        if (choices.Count == 0)
        {
            Close();
            return true;
        }

        ClampSelection(world);
        return Choose(world, choices[SelectedChoiceIndex]);
    }

    public bool ChooseById(WorldState world, string choiceId)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(choiceId);

        var choice = AvailableChoices(world).FirstOrDefault(item =>
            string.Equals(item.Id, choiceId, StringComparison.Ordinal));
        return choice is not null && Choose(world, choice);
    }

    public void Close()
    {
        _graph = null;
        _currentNodeId = null;
        SpeakerId = null;
        SelectedChoiceIndex = 0;
        Message = "";
    }

    private bool Choose(WorldState world, DialogueChoice choice)
    {
        foreach (var effect in choice.Effects)
            ApplyEffect(world, effect);

        if (choice.NextNodeId is null)
        {
            Message = choice.Text;
            Close();
            return true;
        }

        _currentNodeId = choice.NextNodeId;
        SelectedChoiceIndex = 0;
        Message = "";
        ClampSelection(world);
        return true;
    }

    private void ClampSelection(WorldState world)
    {
        var count = AvailableChoices(world).Count;
        SelectedChoiceIndex = count == 0
            ? 0
            : Math.Clamp(SelectedChoiceIndex, 0, count - 1);
    }

    private static bool RequirementMet(
        WorldState world,
        DialogueRequirement requirement)
    {
        return requirement.Kind switch
        {
            DialogueRequirementKind.QuestPhase =>
                QuestPhaseAtLeast(
                    world.Progress.Quests.Get(requirement.TargetId).Phase,
                    ParseEnum<QuestPhase>(requirement.Value)),

            DialogueRequirementKind.Evidence =>
                HasEvidence(
                    world,
                    requirement.TargetId,
                    ParseEnumOrDefault(
                        requirement.Value,
                        KnowledgeKind.Rumor)),

            DialogueRequirementKind.Item =>
                world.Progress.Inventory.Contains(
                    requirement.TargetId,
                    ParsePositiveInt(requirement.Value, 1)),

            DialogueRequirementKind.Reputation =>
                world.Progress.Reputation.Get(
                    ReputationScope.Village,
                    requirement.TargetId) >=
                ParseInt(requirement.Value, 0),

            DialogueRequirementKind.DivineRelationship =>
                world.Progress.DivineRelationships
                    .Get(requirement.TargetId).Favor >=
                ParseInt(requirement.Value, 0),

            DialogueRequirementKind.WorldFlag =>
                world.Progress.HasFlag(requirement.TargetId) ==
                !string.Equals(
                    requirement.Value,
                    "false",
                    StringComparison.OrdinalIgnoreCase),

            _ => false
        };
    }

    private static void ApplyEffect(
        WorldState world,
        DialogueEffect effect)
    {
        switch (effect.Kind)
        {
            case DialogueEffectKind.AdvanceQuest:
                world.Progress.Quests.Get(effect.TargetId)
                    .SetPhase(ParseEnum<QuestPhase>(effect.Value));
                break;

            case DialogueEffectKind.AddEvidence:
                ApplyEvidence(world, effect);
                break;

            case DialogueEffectKind.GiveItem:
                world.Progress.Inventory.Add(
                    effect.TargetId,
                    Math.Max(1, effect.Amount));
                break;

            case DialogueEffectKind.TakeItem:
                world.Progress.Inventory.Remove(
                    effect.TargetId,
                    Math.Max(1, effect.Amount));
                break;

            case DialogueEffectKind.ChangeReputation:
            {
                var scope = ParseEnumOrDefault(
                    effect.Value,
                    ReputationScope.Village);
                world.Progress.Reputation.Change(
                    scope,
                    effect.TargetId,
                    effect.Amount);
                break;
            }

            case DialogueEffectKind.ChangeRelationship:
            {
                var kind = ParseEnumOrDefault(
                    effect.Value,
                    RelationshipKind.Trust);
                world.Progress.Relationships.Change(
                    effect.TargetId,
                    kind,
                    effect.Amount);
                break;
            }

            case DialogueEffectKind.ChangeDivineFavor:
                world.Progress.DivineRelationships
                    .Get(effect.TargetId)
                    .ChangeFavor(effect.Amount);
                break;

            case DialogueEffectKind.SetWorldFlag:
                world.Progress.SetFlag(
                    effect.TargetId,
                    !string.Equals(
                        effect.Value,
                        "false",
                        StringComparison.OrdinalIgnoreCase));
                break;
        }
    }

    private static void ApplyEvidence(
        WorldState world,
        DialogueEffect effect)
    {
        var parts = effect.Value.Split('|', 4);
        if (parts.Length < 3)
            throw new InvalidOperationException(
                $"Dialogue evidence effect '{effect.Value}' is malformed.");

        var evidenceId = parts[0];
        var kind = ParseEnum<KnowledgeKind>(parts[1]);
        var text = parts[2];
        var source = parts.Length >= 4 && !string.IsNullOrWhiteSpace(parts[3])
            ? parts[3]
            : effect.TargetId;

        world.Progress.Quests
            .Get(effect.TargetId)
            .AddEvidence(new EvidenceEntry(
                evidenceId,
                effect.TargetId,
                kind,
                text,
                source));
    }

    private static bool HasEvidence(
        WorldState world,
        string evidenceId,
        KnowledgeKind minimum)
    {
        var evidence = world.Progress.Quests.Quests
            .SelectMany(quest => quest.Evidence)
            .FirstOrDefault(entry =>
                string.Equals(
                    entry.Id,
                    evidenceId,
                    StringComparison.Ordinal));

        return evidence is not null &&
               EvidenceRank(evidence.Kind) >= EvidenceRank(minimum);
    }

    private static int EvidenceRank(KnowledgeKind kind) => kind switch
    {
        KnowledgeKind.Rumor => 0,
        KnowledgeKind.Observation => 1,
        KnowledgeKind.Interpretation => 2,
        KnowledgeKind.ConfirmedFact => 3,
        _ => 0
    };

    private static bool QuestPhaseAtLeast(
        QuestPhase actual,
        QuestPhase required)
    {
        if (actual == QuestPhase.Failed)
            return required == QuestPhase.Failed;
        if (required == QuestPhase.Failed)
            return false;
        return actual >= required;
    }

    private static T ParseEnum<T>(string value)
        where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException(
                $"Dialogue value '{value}' is not a valid {typeof(T).Name}.");

    private static T ParseEnumOrDefault<T>(
        string value,
        T fallback)
        where T : struct, Enum =>
        Enum.TryParse<T>(value, true, out var parsed)
            ? parsed
            : fallback;

    private static int ParsePositiveInt(
        string value,
        int fallback) =>
        Math.Max(1, ParseInt(value, fallback));

    private static int ParseInt(
        string value,
        int fallback) =>
        int.TryParse(value, out var parsed)
            ? parsed
            : fallback;
}
