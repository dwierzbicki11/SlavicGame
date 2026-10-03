using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class SwampPredatorEncounterRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var encounter = world.Progress.Encounters.Get(SwampPredatorEncounter.Id);
        var predator = world.Enemies.Single(enemy =>
            string.Equals(enemy.Id, SwampPredatorEncounter.Id, StringComparison.Ordinal));

        check(encounter.Awareness == EncounterAwareness.Unseen &&
              encounter.Resolution == EncounterResolution.None,
            "Swamp predator starts as an unresolved unseen encounter");

        world.Progress.SetFlag(VerticalSliceQuestInteractions.PredatorTracksFlag);
        SwampPredatorEncounter.Update(world);
        check(encounter.Awareness == EncounterAwareness.Observed,
            "Predator tracks advance encounter to Observed");

        world.SetPlayerPosition(predator.Position + new Vector3(0f, 0f, -1.7f));
        SwampPredatorEncounter.Update(world);
        check(encounter.Awareness == EncounterAwareness.Identified &&
              world.Progress.HasFlag(SwampPredatorEncounter.IdentifiedFlag),
            "Close visual encounter identifies the physical predator");

        var staminaBefore = world.Player.Stamina;
        for (var swing = 0; swing < 3; swing++)
        {
            check(world.Melee.TryStart(world),
                $"Melee swing {swing + 1} starts");
            world.Melee.Update(world, Vector3.UnitZ, 0.25);
            world.Melee.Update(world, Vector3.UnitZ, 0.40);
        }

        check(world.Player.Stamina == staminaBefore - PlayerMeleeCombat.LightAttack.StaminaCost * 3f,
            "Each melee attack spends stamina exactly once");
        check(!predator.IsAlive && predator.Health == 0f,
            "Three light attacks kill the prototype swamp predator");
        check(encounter.Awareness == EncounterAwareness.Resolved &&
              encounter.Resolution == EncounterResolution.Killed &&
              world.Progress.HasFlag(SwampPredatorEncounter.KilledFlag),
            "Predator death resolves encounter as Killed");

        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        check(quest.Evidence.Any(entry =>
                entry.Id == "light-over-swamp.predator-killed" &&
                entry.Kind == KnowledgeKind.ConfirmedFact),
            "Predator kill records a confirmed physical-threat fact");

        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);

        var restoredPredator = restored.Enemies.Single(enemy =>
            string.Equals(enemy.Id, SwampPredatorEncounter.Id, StringComparison.Ordinal));
        var restoredEncounter = restored.Progress.Encounters.Get(SwampPredatorEncounter.Id);

        check(!restoredPredator.IsAlive &&
              restoredEncounter.Awareness == EncounterAwareness.Resolved &&
              restoredEncounter.Resolution == EncounterResolution.Killed,
            "Save/load preserves both dead enemy state and encounter resolution");

        var restoredQuest = restored.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        restoredQuest.Resolve(QuestResolution.RitualClosure);
        var moneyBefore = restored.Progress.Profile.Money;

        restored.SetPlayerPosition(new Vector3(-6f, 0f, -84f));
        restored.QuestInteractions.Update(restored);
        check(restored.QuestInteractions.TryInteract(restored) == QuestInteractionResult.Completed &&
              restored.Progress.Profile.Money == moneyBefore + 60,
            "Resolved apparition plus killed predator grants the enhanced contract reward");
    }
}
