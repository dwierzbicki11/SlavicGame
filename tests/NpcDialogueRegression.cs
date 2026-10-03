using System.Numerics;
using SlavicGame.Engine.Dialogue;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Relationships;
using SlavicGame.Engine.World;

internal static class NpcDialogueRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10);
        world.NpcWorld.Update(world);

        check(world.NpcWorld.Actors.Count == 5,
            "All five vertical-slice NPC roles exist in the daytime world");

        var shrineKeeperDay = world.NpcWorld.Find("shrine-keeper");
        check(shrineKeeperDay is not null &&
              shrineKeeperDay.LocationId == "old-shrine" &&
              Vector3.Distance(
                  shrineKeeperDay.Position,
                  new Vector3(-82f, shrineKeeperDay.Position.Y, 58f)) < 0.1f,
            "Shrine keeper follows daytime shrine schedule");

        world.Time.SetTimeOfDay(22);
        world.NpcWorld.Update(world);
        var shrineKeeperNight = world.NpcWorld.Find("shrine-keeper");
        check(shrineKeeperNight is not null &&
              shrineKeeperNight.LocationId == "old-village",
            "Shrine keeper moves back to the village at night");

        world.Time.SetTimeOfDay(10);
        world.NpcWorld.Update(world);

        var family = world.NpcWorld.Find("missing-family")
            ?? throw new Exception("Missing family NPC not present");
        world.SetPlayerPosition(family.Position);
        world.NpcWorld.Update(world);

        check(world.Dialogue.TryStartNearest(world) &&
              world.Dialogue.IsOpen &&
              world.Dialogue.SpeakerId == "missing-family",
            "E-range NPC can start an executable dialogue session");

        check(world.Dialogue.AvailableChoices(world).Any(choice =>
                choice.Id == "mf.accept"),
            "Offered contract exposes the accept dialogue choice");

        check(world.Dialogue.ChooseById(world, "mf.accept"),
            "Dialogue choice can be selected by stable choice ID");

        var quest = world.Progress.Quests.Get(
            VerticalSliceBootstrap.ContractQuestId);

        check(!world.Dialogue.IsOpen &&
              quest.Phase == QuestPhase.Active &&
              world.Progress.HasFlag(
                  VerticalSliceQuestInteractions.ContractAcceptedFlag) &&
              quest.Evidence.Any(entry =>
                  entry.Id == "light-over-swamp.last-route"),
            "Family dialogue accepts quest and records starting evidence");

        world.Progress.Inventory.Add("missing-person-keepsake");
        quest.SetPhase(QuestPhase.Investigation);

        world.NpcWorld.Update(world);
        check(world.Dialogue.Start(world, "missing-family") &&
              world.Dialogue.CurrentNode?.Id == "mf.keepsake",
            "Keepsake changes the family's dialogue start state");

        check(world.Dialogue.ChooseById(world, "mf.show-keepsake") &&
              quest.Evidence.Any(entry =>
                  entry.Id == "light-over-swamp.keepsake-owner" &&
                  entry.Kind == KnowledgeKind.ConfirmedFact) &&
              world.Progress.Relationships.Get(
                  "missing-family",
                  RelationshipKind.Trust) >= 4,
            "Showing keepsake confirms owner and changes NPC trust");

        world.Dialogue.Close();

        var guard = world.NpcWorld.Find("community-guard")
            ?? throw new Exception("Community guard not present");
        world.SetPlayerPosition(guard.Position);
        world.NpcWorld.Update(world);
        check(world.Dialogue.Start(world, "community-guard"),
            "Guard dialogue starts from scheduled NPC position");

        check(!world.Dialogue.AvailableChoices(world).Any(choice =>
                choice.Id == "cg.predator"),
            "Predator dialogue option stays hidden before identification");

        world.Dialogue.Close();
        world.Progress.SetFlag(
            SwampPredatorEncounter.IdentifiedFlag);
        check(world.Dialogue.Start(world, "community-guard") &&
              world.Dialogue.AvailableChoices(world).Any(choice =>
                  choice.Id == "cg.predator"),
            "Known predator unlocks knowledge-gated guard dialogue option");
        world.Dialogue.Close();

        var shrineKeeper = world.NpcWorld.Find("shrine-keeper")
            ?? throw new Exception("Shrine keeper not present");
        world.SetPlayerPosition(shrineKeeper.Position);
        world.NpcWorld.Update(world);

        var spark = SpellLessons.Get("spell.spark");
        var beforeTeacherTalk =
            world.SpellLearning.Evaluate(world, spark);

        check(!beforeTeacherTalk.CanLearn &&
              beforeTeacherTalk.Missing.Any(item =>
                  item.Contains("POROZMAWIAJ", StringComparison.Ordinal)),
            "Spell lesson is blocked until the player speaks with its teacher");

        check(world.Dialogue.Start(world, "shrine-keeper") &&
              world.Dialogue.ChooseById(world, "sk.teacher"),
            "Shrine keeper exposes teacher dialogue");

        check(world.Progress.HasFlag(
                DialogueRuntime.TeacherReadyFlag("shrine-keeper")),
            "Teacher conversation persists as a world flag");

        world.Dialogue.Close();
        var withTeacherNearby =
            world.SpellLearning.Evaluate(world, spark);

        check(withTeacherNearby.CanLearn,
            "Spark lesson becomes available only beside the prepared teacher");

        world.SetPlayerPosition(new Vector3(-96f, 0f, 55f));
        world.NpcWorld.Update(world);

        var teacherTooFar =
            world.SpellLearning.Evaluate(world, spark);

        check(!teacherTooFar.CanLearn &&
              teacherTooFar.Missing.Any(item =>
                  item.Contains(
                      "NAUCZYCIEL NIE JEST W POBLIZU",
                      StringComparison.Ordinal)),
            "Leaving the teacher blocks the actual spell lesson");

        world.SetPlayerPosition(shrineKeeper.Position);
        world.NpcWorld.Update(world);
        check(world.SpellLearning.TryLearnCurrent(world) &&
              SpellLessons.IsLearned(world, "spell.spark"),
            "Contextual L lesson succeeds when teacher and requirements are real");

        world.Dialogue.Close();
        world.SetPlayerPosition(Vector3.Zero);
        world.NpcWorld.Update(world);
        check(!world.Dialogue.TryStartNearest(world),
            "Dialogue cannot start with an NPC outside interaction range");
    }
}
