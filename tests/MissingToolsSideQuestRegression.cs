using System.Numerics;
using SlavicGame.Engine.Dialogue;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class MissingToolsSideQuestRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        RunCarefulOutcome(check);
        RunUncertainOutcome(check);
        RunFalseAccusationOutcome(check);
    }

    private static void RunCarefulOutcome(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10);
        world.NpcWorld.Update(world);

        var quest = world.Progress.Quests.Get(MissingToolsSideQuest.QuestId);
        check(quest.Phase == QuestPhase.Unavailable,
            "Missing-tools side quest starts unavailable and does not collide with production SQ_R0_01");

        StartAtWoodworker(world);
        check(world.Dialogue.CurrentNode?.Id == "mt.offer" &&
              world.Dialogue.ChooseById(world, "mt.accept") &&
              quest.Phase == QuestPhase.Active &&
              world.Progress.HasFlag(MissingToolsSideQuest.AcceptedFlag),
            "Woodworker dialogue offers and activates the missing-tools side quest");

        var axeVisual = WorldItemVisualCatalog.Definitions.Single(item =>
            item.Id == "visual.side.missing-tools.axe");
        check(WorldItemVisualCatalog.IsVisible(world, axeVisual),
            "Accepting the side quest reveals the physical axe in the forest");

        Move(world, 22f, 17f);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.TryInteract(world) &&
              world.Progress.Inventory.Contains(MissingToolsSideQuest.ToolItemId) &&
              world.Progress.HasFlag(MissingToolsSideQuest.ToolCollectedFlag) &&
              quest.Phase == QuestPhase.Investigation &&
              quest.Evidence.Any(entry =>
                  entry.Id == MissingToolsSideQuest.ToolEvidenceId),
            "Player can recover the missing axe as a durable quest item");
        check(!WorldItemVisualCatalog.IsVisible(world, axeVisual),
            "Recovered axe disappears from world presentation immediately");

        Move(world, 18.5f, 15.5f);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.TryInteract(world) &&
              world.Progress.HasFlag(MissingToolsSideQuest.WorksiteInspectedFlag) &&
              quest.Evidence.Any(entry =>
                  entry.Id == MissingToolsSideQuest.WorksiteEvidenceId &&
                  entry.Kind == KnowledgeKind.ConfirmedFact),
            "Inspecting the worksite records evidence that argues against theft");

        var moneyBefore = world.Progress.Profile.Money;
        StartAtWoodworker(world);
        check(world.Dialogue.CurrentNode?.Id == "mt.return-informed" &&
              world.Dialogue.ChooseById(world, "mt.return-misplaced"),
            "Evidence unlocks the careful no-theft conclusion");

        MissingToolsSideQuest.Synchronize(world);

        check(quest.Phase == QuestPhase.TurnedIn &&
              quest.RewardClaimed &&
              MissingToolsSideQuest.Outcome(world) ==
                  MissingToolsOutcome.MisplacedConfirmed &&
              !world.Progress.Inventory.Contains(MissingToolsSideQuest.ToolItemId) &&
              world.Progress.Profile.Money == moneyBefore + 20 &&
              world.Progress.Reputation.Get(
                  ReputationScope.Village,
                  "old-village") == 4 &&
              world.Progress.Relationships.Get(
                  MissingToolsSideQuest.GiverId,
                  RelationshipKind.Trust) == 8,
            "Careful investigation gives the best one-time side-quest outcome");

        var serialized = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, serialized);

        var restoredQuest =
            restored.Progress.Quests.Get(MissingToolsSideQuest.QuestId);
        var restoredMoney = restored.Progress.Profile.Money;
        MissingToolsSideQuest.Synchronize(restored);

        check(restoredQuest.Phase == QuestPhase.TurnedIn &&
              MissingToolsSideQuest.Outcome(restored) ==
                  MissingToolsOutcome.MisplacedConfirmed &&
              restored.Progress.Profile.Money == restoredMoney &&
              restored.Progress.Relationships.Get(
                  MissingToolsSideQuest.GiverId,
                  RelationshipKind.Trust) == 8,
            "Missing-tools outcome survives save/load without duplicating its reward");
    }

    private static void RunUncertainOutcome(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10);
        world.NpcWorld.Update(world);
        StartAtWoodworker(world);
        check(world.Dialogue.ChooseById(world, "mt.accept"),
            "Uncertain branch can accept the side quest");

        Move(world, 22f, 17f);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.TryInteract(world),
            "Uncertain branch can recover the axe without inspecting context");

        var moneyBefore = world.Progress.Profile.Money;
        StartAtWoodworker(world);
        check(world.Dialogue.CurrentNode?.Id == "mt.return-unverified" &&
              world.Dialogue.ChooseById(world, "mt.return-neutral"),
            "Player can return the axe without inventing an explanation");

        MissingToolsSideQuest.Synchronize(world);

        check(MissingToolsSideQuest.Outcome(world) ==
                  MissingToolsOutcome.ReturnedUncertain &&
              world.Progress.Profile.Money == moneyBefore + 15 &&
              world.Progress.Reputation.Get(
                  ReputationScope.Village,
                  "old-village") == 2 &&
              world.Progress.Relationships.Get(
                  MissingToolsSideQuest.GiverId,
                  RelationshipKind.Trust) == 4,
            "Uncertain return gives an intermediate reward without false certainty");
    }

    private static void RunFalseAccusationOutcome(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10);
        world.NpcWorld.Update(world);
        StartAtWoodworker(world);
        check(world.Dialogue.ChooseById(world, "mt.accept"),
            "False-accusation branch can accept the side quest");

        Move(world, 22f, 17f);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.TryInteract(world),
            "False-accusation branch can recover the axe");

        var moneyBefore = world.Progress.Profile.Money;
        StartAtWoodworker(world);
        check(world.Dialogue.ChooseById(world, "mt.return-accuse"),
            "Player can make an unsupported theft accusation");

        MissingToolsSideQuest.Synchronize(world);

        check(MissingToolsSideQuest.Outcome(world) ==
                  MissingToolsOutcome.FalseAccusation &&
              world.Progress.Profile.Money == moneyBefore + 10 &&
              world.Progress.Reputation.Get(
                  ReputationScope.Village,
                  "old-village") == 0 &&
              world.Progress.Relationships.Get(
                  MissingToolsSideQuest.GiverId,
                  RelationshipKind.Trust) == -5,
            "Unsupported accusation resolves fail-forward but damages trust");
    }

    private static void StartAtWoodworker(WorldState world)
    {
        world.Dialogue.Close();
        world.NpcWorld.Update(world);
        var woodworker = world.NpcWorld.Find(MissingToolsSideQuest.GiverId)
            ?? throw new Exception("Woodworker not present");
        world.SetPlayerPosition(woodworker.Position);
        world.NpcWorld.Update(world);

        if (!world.Dialogue.Start(world, MissingToolsSideQuest.GiverId))
            throw new Exception("Could not start woodworker dialogue");
    }

    private static void Move(WorldState world, float x, float z)
    {
        world.Dialogue.Close();
        world.SetPlayerPosition(new Vector3(x, 0f, z));
        world.EnvironmentInteractions.Update(world);
    }
}
