using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class VerticalSliceQuestInteractionRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        var interactions = world.QuestInteractions;

        check(quest.Phase == QuestPhase.Offered,
            "Vertical slice starts with Light Over Swamp offered");

        Move(world, -6, -84);
        interactions.Update(world);
        check(interactions.Current?.Id == "quest.family" &&
              interactions.HudText.Contains("PRZYJMIJ", StringComparison.Ordinal),
            "Family exposes contract acceptance in the village");
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              quest.Phase == QuestPhase.Active &&
              HasEvidence(quest, "light-over-swamp.witness-light") &&
              HasEvidence(quest, "light-over-swamp.last-route"),
            "Family conversation accepts contract and records starting knowledge");

        Move(world, 91, 41);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              quest.Phase == QuestPhase.Investigation &&
              HasEvidence(quest, "light-over-swamp.broken-planks"),
            "Crossing inspection starts investigation and records physical damage");

        Move(world, 98, 34);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              HasEvidence(quest, "light-over-swamp.predator-tracks"),
            "Predator tracks are a separate physical clue");

        Move(world, 82, 29);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              world.Progress.Inventory.Contains("missing-person-keepsake") &&
              HasEvidence(quest, "light-over-swamp.keepsake"),
            "Swamp keepsake becomes a durable quest item and observation");

        Move(world, 78, 24);
        world.Time.SetTimeOfDay(12);
        interactions.Update(world);
        check(interactions.Current?.Id == "quest.anomaly" &&
              interactions.HudText.Contains("PO ZMIERZCHU", StringComparison.Ordinal),
            "Anomaly interaction communicates its night requirement");
        check(interactions.TryInteract(world) == QuestInteractionResult.Blocked &&
              !HasEvidence(quest, "light-over-swamp.apparition-response"),
            "Daylight cannot fake the supernatural observation");

        world.Time.SetTimeOfDay(23);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              HasEvidence(quest, "light-over-swamp.apparition-response") &&
              quest.Phase == QuestPhase.Preparation,
            "Night anomaly completes the minimum investigation set and enters Preparation");

        Move(world, -6, -84);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              HasEvidence(quest, "light-over-swamp.keepsake-owner") &&
              world.Progress.HasFlag(VerticalSliceQuestInteractions.OwnerConfirmedFlag),
            "Returning the keepsake to the family confirms the missing person's identity");

        world.Time.SetTimeOfDay(10);
        Move(world, -82, 58);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              world.Progress.HasFlag(VerticalSliceRituals.AnchorKnowledgeFlag) &&
              world.Progress.HasFlag(VerticalSliceRituals.LearnedFlag) &&
              world.Progress.HasFlag(VerticalSliceRituals.IdentitySignFlag) &&
              world.Progress.HasFlag(VerticalSliceRituals.BoundarySignFlag),
            "Shrine keeper converts confirmed identity into anchor and ritual knowledge");

        if (world.Cinematics.IsPlaying)
            world.Cinematics.Finish(world);

        Move(world, 5, -92);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              world.Progress.Inventory.Contains("ritual-thread") &&
              world.Progress.HasFlag(VerticalSliceQuestInteractions.ThreadReceivedFlag),
            "Herbalist supplies the ritual thread after the ritual is known");

        world.Time.SetTimeOfDay(23);
        Move(world, -85, 55);
        var start = world.Rituals.TryStart(world);
        check(start.Started && quest.Phase == QuestPhase.Encounter,
            "Collected requirements make ritual executable and enter Encounter");
        world.Rituals.Update(world, 10);
        check(quest.Phase == QuestPhase.Resolved &&
              quest.Resolution == QuestResolution.RitualClosure &&
              world.Progress.HasFlag(VerticalSliceRituals.ReleasedFlag),
            "Playable quest loop reaches durable ritual closure");

        if (world.Cinematics.IsPlaying)
            world.Cinematics.Finish(world);

        var moneyBefore = world.Progress.Profile.Money;
        Move(world, -6, -84);
        check(interactions.TryInteract(world) == QuestInteractionResult.Completed &&
              quest.Phase == QuestPhase.TurnedIn &&
              quest.RewardClaimed &&
              world.Progress.Profile.Money == moneyBefore + 40 &&
              world.Progress.HasFlag(VerticalSliceQuestInteractions.TurnedInFlag),
            "Family turn-in pays the contract exactly once");

        check(interactions.TryInteract(world) != QuestInteractionResult.Completed &&
              world.Progress.Profile.Money == moneyBefore + 40,
            "Turned-in contract cannot duplicate its reward");

        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);
        var restoredQuest = restored.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);

        check(restoredQuest.Phase == QuestPhase.TurnedIn &&
              restoredQuest.RewardClaimed &&
              restored.Progress.Profile.Money == moneyBefore + 40 &&
              restored.Progress.HasFlag(VerticalSliceQuestInteractions.TurnedInFlag) &&
              restored.Progress.HasFlag(VerticalSliceRituals.ReleasedFlag),
            "Full interactive vertical-slice outcome survives save/load");
    }

    private static void Move(WorldState world, float x, float z)
    {
        var position = new Vector3(x, 0, z);
        world.SetPlayerPosition(position);
        world.QuestInteractions.Update(world);
    }

    private static bool HasEvidence(QuestRecord quest, string id) =>
        quest.Evidence.Any(entry => string.Equals(entry.Id, id, StringComparison.Ordinal));
}
