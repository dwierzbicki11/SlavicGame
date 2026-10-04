using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public enum MissingToolsOutcome
{
    None,
    MisplacedConfirmed,
    ReturnedUncertain,
    FalseAccusation
}

public static class MissingToolsSideQuest
{
    public const string QuestId = "side-r0-missing-tools";
    public const string GiverId = "settler-woodworker-01";
    public const string ToolItemId = "woodworker-missing-axe";

    public const string AcceptedFlag = "side-r0-missing-tools.accepted";
    public const string ToolCollectedFlag = "side-r0-missing-tools.tool-collected";
    public const string WorksiteInspectedFlag = "side-r0-missing-tools.worksite-inspected";
    public const string CompletedFlag = "side-r0-missing-tools.completed";

    public const string ToolEvidenceId = "side-r0-missing-tools.tool-found";
    public const string WorksiteEvidenceId = "side-r0-missing-tools.worksite-context";

    public static string OutcomeFlag(MissingToolsOutcome outcome) =>
        $"side-r0-missing-tools.outcome.{outcome.ToString().ToLowerInvariant()}";

    public static bool IsStarted(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var phase = world.Progress.Quests.Get(QuestId).Phase;
        return phase is not QuestPhase.Unavailable;
    }

    public static bool CanInvestigate(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var phase = world.Progress.Quests.Get(QuestId).Phase;
        return phase is QuestPhase.Active or QuestPhase.Investigation;
    }

    public static bool RecordToolFound(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInvestigate(world) ||
            world.Progress.HasFlag(ToolCollectedFlag))
        {
            return false;
        }

        var quest = world.Progress.Quests.Get(QuestId);
        world.Progress.Inventory.Add(ToolItemId);
        world.Progress.SetFlag(ToolCollectedFlag);

        quest.AddEvidence(new EvidenceEntry(
            ToolEvidenceId,
            QuestId,
            KnowledgeKind.Observation,
            "Przy niedokończonym miejscu pracy znaleziono brakującą siekierę cieśli.",
            "forest-worksite"));

        if (quest.Phase == QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        return true;
    }

    public static bool RecordWorksiteContext(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInvestigate(world) ||
            world.Progress.HasFlag(WorksiteInspectedFlag))
        {
            return false;
        }

        var quest = world.Progress.Quests.Get(QuestId);
        world.Progress.SetFlag(WorksiteInspectedFlag);

        quest.AddEvidence(new EvidenceEntry(
            WorksiteEvidenceId,
            QuestId,
            KnowledgeKind.ConfirmedFact,
            "Układ niedokończonej pracy i pozostawionych przedmiotów wskazuje, że narzędzie odłożono na miejscu; brak śladów kradzieży.",
            "forest-worksite"));

        if (quest.Phase == QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        return true;
    }

    public static MissingToolsOutcome Outcome(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        foreach (var outcome in Enum.GetValues<MissingToolsOutcome>())
        {
            if (outcome == MissingToolsOutcome.None)
                continue;
            if (world.Progress.HasFlag(OutcomeFlag(outcome)))
                return outcome;
        }

        return MissingToolsOutcome.None;
    }

    public static void Synchronize(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var quest = world.Progress.Quests.Get(QuestId);
        if (quest.Phase == QuestPhase.Unavailable ||
            quest.Phase == QuestPhase.TurnedIn ||
            world.Progress.HasFlag(CompletedFlag))
        {
            return;
        }

        var outcome = Outcome(world);
        if (outcome == MissingToolsOutcome.None)
            return;

        if (world.Progress.Inventory.Contains(ToolItemId))
            world.Progress.Inventory.Remove(ToolItemId);

        if (quest.Phase != QuestPhase.Resolved)
            quest.Resolve(QuestResolution.Other);

        var (money, reputation, trust) = outcome switch
        {
            MissingToolsOutcome.MisplacedConfirmed => (20, 4, 8),
            MissingToolsOutcome.ReturnedUncertain => (15, 2, 4),
            MissingToolsOutcome.FalseAccusation => (10, 0, -5),
            _ => (0, 0, 0)
        };

        if (money != 0)
            world.Progress.Profile.ChangeMoney(money);
        if (reputation != 0)
        {
            world.Progress.Reputation.Change(
                ReputationScope.Village,
                "old-village",
                reputation);
        }
        if (trust != 0)
        {
            world.Progress.Relationships.Change(
                GiverId,
                RelationshipKind.Trust,
                trust);
        }

        quest.ClaimReward();
        world.Progress.SetFlag(CompletedFlag);
    }
}
