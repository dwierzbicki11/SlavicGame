using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;
using SlavicGame.Engine.Dialogue;

namespace SlavicGame.Engine.Magic;

public enum SpellLessonRequirementKind
{
    QuestPhaseAtLeast,
    WorldFlag,
    Item,
    Evidence,
    LearnedSpell,
    WoundedExperience
}

public sealed record SpellLessonRequirement(
    SpellLessonRequirementKind Kind,
    string TargetId,
    int Amount = 0,
    QuestPhase MinimumQuestPhase = QuestPhase.Unavailable,
    KnowledgeKind MinimumEvidenceKind = KnowledgeKind.Rumor);

public sealed record SpellLessonDefinition(
    string SpellId,
    string TeacherId,
    string RequiredRegionId,
    IReadOnlyList<SpellLessonRequirement> Requirements,
    IReadOnlyList<ItemCost> PracticeCosts);

public sealed record SpellLessonEvaluation(
    bool CanLearn,
    SpellLessonDefinition Lesson,
    string Message,
    IReadOnlyList<string> Missing);

public static class SpellLessons
{
    public const string WoundExperienceFlag = "magic.lesson.mend.wound-experienced";

    public static IReadOnlyList<SpellLessonDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        new SpellLessonDefinition(
            "spell.spark",
            "shrine-keeper",
            "old-shrine",
            [
                new SpellLessonRequirement(
                    SpellLessonRequirementKind.QuestPhaseAtLeast,
                    SlavicGame.Engine.Gameplay.VerticalSliceBootstrap.ContractQuestId,
                    MinimumQuestPhase: QuestPhase.Offered)
            ],
            []),

        new SpellLessonDefinition(
            "spell.mend",
            "herbalist",
            "old-village",
            [
                new SpellLessonRequirement(
                    SpellLessonRequirementKind.LearnedSpell,
                    "spell.spark"),
                new SpellLessonRequirement(
                    SpellLessonRequirementKind.WoundedExperience,
                    WoundExperienceFlag),
                new SpellLessonRequirement(
                    SpellLessonRequirementKind.Item,
                    "simple-bandage",
                    Amount: 1)
            ],
            [new ItemCost("simple-bandage", 1)]),

        new SpellLessonDefinition(
            "spell.reveal-trace",
            "shrine-keeper",
            "old-shrine",
            [
                new SpellLessonRequirement(
                    SpellLessonRequirementKind.LearnedSpell,
                    "spell.spark"),
                new SpellLessonRequirement(
                    SpellLessonRequirementKind.Evidence,
                    "light-over-swamp.keepsake-owner",
                    MinimumEvidenceKind: KnowledgeKind.ConfirmedFact),
                new SpellLessonRequirement(
                    SpellLessonRequirementKind.WorldFlag,
                    VerticalSliceRituals.AnchorKnowledgeFlag)
            ],
            [])
    });

    private static readonly IReadOnlyDictionary<string, SpellLessonDefinition> BySpellId =
        All.ToDictionary(lesson => lesson.SpellId, StringComparer.Ordinal);

    public static SpellLessonDefinition Get(string spellId) =>
        BySpellId.TryGetValue(spellId, out var lesson)
            ? lesson
            : throw new KeyNotFoundException($"No lesson registered for spell '{spellId}'.");

    public static string LearnedFlag(string spellId) => $"magic.{spellId}.learned";

    public static bool IsLearned(WorldState world, string spellId)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentException.ThrowIfNullOrWhiteSpace(spellId);
        return world.Progress.HasFlag(LearnedFlag(spellId));
    }
}

public sealed class SpellLearningSystem
{
    public string Message { get; private set; } = "L NAUKA CZARU";

    public void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Player.IsAlive &&
            world.Player.Health <= world.Player.MaxHealth * 0.70f)
        {
            world.Progress.SetFlag(SpellLessons.WoundExperienceFlag);
        }
    }

    public SpellLessonDefinition? LessonAtCurrentRegion(WorldState world) =>
        SpellLessons.All.FirstOrDefault(lesson =>
            string.Equals(lesson.RequiredRegionId, world.CurrentRegion, StringComparison.Ordinal) &&
            !SpellLessons.IsLearned(world, lesson.SpellId));

    public SpellLessonEvaluation Evaluate(WorldState world, SpellLessonDefinition lesson)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(lesson);

        var missing = new List<string>();

        if (!string.Equals(world.CurrentRegion, lesson.RequiredRegionId, StringComparison.Ordinal))
            missing.Add($"MIEJSCE: {lesson.RequiredRegionId.ToUpperInvariant()}");

        if (!string.IsNullOrWhiteSpace(lesson.TeacherId))
        {
            if (!world.Progress.HasFlag(DialogueRuntime.TeacherReadyFlag(lesson.TeacherId)))
            {
                missing.Add($"POROZMAWIAJ Z: {NpcPresentation.DisplayName(lesson.TeacherId)}");
            }
            else if (!world.NpcWorld.IsNearby(
                         lesson.TeacherId,
                         world.PlayerPosition,
                         4.25f))
            {
                missing.Add($"NAUCZYCIEL NIE JEST W POBLIZU: {NpcPresentation.DisplayName(lesson.TeacherId)}");
            }
        }

        foreach (var requirement in lesson.Requirements)
        {
            switch (requirement.Kind)
            {
                case SpellLessonRequirementKind.QuestPhaseAtLeast:
                {
                    var phase = world.Progress.Quests.Get(requirement.TargetId).Phase;
                    if (!PhaseAtLeast(phase, requirement.MinimumQuestPhase))
                        missing.Add($"POSTEP QUESTA: {requirement.MinimumQuestPhase.ToString().ToUpperInvariant()}");
                    break;
                }

                case SpellLessonRequirementKind.WorldFlag:
                    if (!world.Progress.HasFlag(requirement.TargetId))
                        missing.Add("BRAK WYMAGANEJ WIEDZY");
                    break;

                case SpellLessonRequirementKind.Item:
                    if (!world.Progress.Inventory.Contains(requirement.TargetId, Math.Max(1, requirement.Amount)))
                        missing.Add($"PRZEDMIOT: {requirement.TargetId.ToUpperInvariant()}");
                    break;

                case SpellLessonRequirementKind.Evidence:
                {
                    var evidence = world.Progress.Quests.Quests
                        .SelectMany(quest => quest.Evidence)
                        .FirstOrDefault(entry =>
                            string.Equals(entry.Id, requirement.TargetId, StringComparison.Ordinal));
                    if (evidence is null || !EvidenceAtLeast(evidence.Kind, requirement.MinimumEvidenceKind))
                        missing.Add($"DOWOD: {requirement.TargetId.ToUpperInvariant()}");
                    break;
                }

                case SpellLessonRequirementKind.LearnedSpell:
                    if (!SpellLessons.IsLearned(world, requirement.TargetId))
                        missing.Add($"CZAR: {SpellName(requirement.TargetId)}");
                    break;

                case SpellLessonRequirementKind.WoundedExperience:
                    if (!world.Progress.HasFlag(requirement.TargetId))
                        missing.Add("DOSWIADCZENIE PRAWDZIWYCH OBRAZEN");
                    break;
            }
        }

        foreach (var cost in lesson.PracticeCosts)
            if (!world.Progress.Inventory.Contains(cost.ItemId, cost.Quantity))
                missing.Add($"MATERIAL CWICZENIA: {cost.ItemId.ToUpperInvariant()}");

        var canLearn = missing.Count == 0;
        var message = canLearn
            ? $"L NAUCZ SIE: {SpellName(lesson.SpellId)}"
            : $"NAUKA {SpellName(lesson.SpellId)}: {missing[0]}";

        return new SpellLessonEvaluation(canLearn, lesson, message, missing);
    }

    public SpellLessonEvaluation EvaluateCurrent(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var lesson = LessonAtCurrentRegion(world);
        if (lesson is null)
        {
            var fallback = SpellLessons.All.FirstOrDefault(item => !SpellLessons.IsLearned(world, item.SpellId))
                ?? SpellLessons.All[0];
            return new SpellLessonEvaluation(
                false,
                fallback,
                SpellLessons.All.All(item => SpellLessons.IsLearned(world, item.SpellId))
                    ? "WSZYSTKIE DOSTEPNE CZARY SA POZNANE"
                    : "W TYM MIEJSCU NIE MA DOSTEPNEJ NAUKI CZARU",
                ["BRAK LEKCJI W TYM MIEJSCU"]);
        }

        return Evaluate(world, lesson);
    }

    public bool TryLearnCurrent(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var evaluation = EvaluateCurrent(world);
        Message = evaluation.Message;
        if (!evaluation.CanLearn)
            return false;

        if (SpellLessons.IsLearned(world, evaluation.Lesson.SpellId))
        {
            Message = $"JUZ ZNASZ: {SpellName(evaluation.Lesson.SpellId)}";
            return false;
        }

        foreach (var cost in evaluation.Lesson.PracticeCosts)
        {
            if (!world.Progress.Inventory.Contains(cost.ItemId, cost.Quantity))
            {
                Message = $"BRAK MATERIALU: {cost.ItemId.ToUpperInvariant()}";
                return false;
            }
        }

        foreach (var cost in evaluation.Lesson.PracticeCosts)
            world.Progress.Inventory.Remove(cost.ItemId, cost.Quantity);

        world.Progress.SetFlag(SpellLessons.LearnedFlag(evaluation.Lesson.SpellId));
        world.Progress.SetFlag($"magic.lesson.{evaluation.Lesson.SpellId}.completed");
        world.Magic.SelectSpell(evaluation.Lesson.SpellId);
        Message = $"NAUCZONO: {SpellName(evaluation.Lesson.SpellId)} / {world.Magic.Current.Incantation}";
        return true;
    }

    public void RefreshMessage(WorldState world)
    {
        var evaluation = EvaluateCurrent(world);
        Message = evaluation.Message;
    }

    private static bool PhaseAtLeast(QuestPhase actual, QuestPhase required)
    {
        if (actual == QuestPhase.Failed)
            return required == QuestPhase.Failed;
        if (required == QuestPhase.Failed)
            return false;
        return actual >= required;
    }

    private static bool EvidenceAtLeast(KnowledgeKind actual, KnowledgeKind required) =>
        Rank(actual) >= Rank(required);

    private static int Rank(KnowledgeKind kind) => kind switch
    {
        KnowledgeKind.Rumor => 0,
        KnowledgeKind.Observation => 1,
        KnowledgeKind.Interpretation => 2,
        KnowledgeKind.ConfirmedFact => 3,
        _ => 0
    };

    private static string SpellName(string spellId) =>
        SpellCasting.Spells.FirstOrDefault(spell => string.Equals(spell.Id, spellId, StringComparison.Ordinal))?.Name
        ?? spellId.ToUpperInvariant();
}
