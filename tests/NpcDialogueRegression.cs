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

        check(world.NpcWorld.Actors.Count == 13,
            "Five authored NPCs plus eight ambient settlers exist in the daytime world");

        var ambientSettlers = world.NpcWorld.Actors
            .Where(actor => actor.Id.StartsWith("settler-", StringComparison.Ordinal))
            .ToArray();

        check(ambientSettlers.Length == 8,
            "R0 village population includes eight ambient settlers");
        check(ambientSettlers
                .Select(actor => new Vector2(actor.Position.X, actor.Position.Z))
                .Distinct()
                .Count() == ambientSettlers.Length,
            "Ambient settlers occupy distinct daytime poses instead of overlapping");

        var guardAt10 = world.NpcWorld.Find("community-guard")
            ?? throw new Exception("Community guard not present");
        var guardPositionAt10 = guardAt10.Position;

        world.Time.SetTimeOfDay(10.12);
        world.NpcWorld.Update(world);
        var guardAt1012 = world.NpcWorld.Find("community-guard")
            ?? throw new Exception("Community guard not present after route update");

        check(guardAt10.IsMoving &&
              guardAt1012.IsMoving &&
              Vector3.Distance(
                  guardPositionAt10,
                  guardAt1012.Position) > 0.25f,
            "Guard patrol advances through the village as world time changes");

        var travelerAt1012 = world.NpcWorld.Find("settler-traveler-01")
            ?? throw new Exception("Traveler not present");
        world.Time.SetTimeOfDay(10.22);
        world.NpcWorld.Update(world);
        var travelerAt1022 = world.NpcWorld.Find("settler-traveler-01")
            ?? throw new Exception("Traveler not present after route update");

        check(travelerAt1012.IsMoving &&
              travelerAt1022.IsMoving &&
              Vector3.Distance(
                  travelerAt1012.Position,
                  travelerAt1022.Position) > 0.25f,
            "Traveler visibly advances from the gate toward the market");

        world.Time.SetTimeOfDay(10);
        world.NpcWorld.Update(world);

        var shrineKeeperDay = world.NpcWorld.Find("shrine-keeper");
        check(shrineKeeperDay is not null &&
              shrineKeeperDay.LocationId == "old-shrine" &&
              shrineKeeperDay.IsMoving &&
              Vector2.Distance(
                  new Vector2(
                      shrineKeeperDay.Position.X,
                      shrineKeeperDay.Position.Z),
                  new Vector2(-85f, 55f)) < 10f,
            "Shrine keeper follows a daytime tending route around the shrine");

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

        var familyDialoguePosition =
            world.NpcWorld.Find("missing-family")!.Position;
        world.Time.SetTimeOfDay(10.10);
        world.NpcWorld.Update(world);
        var familyWhileTalking =
            world.NpcWorld.Find("missing-family")
            ?? throw new Exception("Missing family NPC disappeared during dialogue");

        check(!familyWhileTalking.IsMoving &&
              Vector3.Distance(
                  familyDialoguePosition,
                  familyWhileTalking.Position) < 0.001f,
            "Dialogue freezes the speaker in place while other routines continue");

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
        var familyAfterDialogue =
            world.NpcWorld.Find("missing-family")
            ?? throw new Exception("Missing family NPC disappeared after dialogue");

        check(Vector3.Distance(
                  familyDialoguePosition,
                  familyAfterDialogue.Position) < 0.05f,
            "Speaker resumes routine from the dialogue position without teleporting");

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
        var ambient = world.NpcWorld.Find("settler-farmer-02")
            ?? throw new Exception("Ambient settler not present");
        world.SetPlayerPosition(ambient.Position);
        world.NpcWorld.Update(world);

        check(!NpcPresentation.HasDialogue(ambient.Id) &&
              string.IsNullOrWhiteSpace(
                  world.NpcWorld.HudPrompt(world.PlayerPosition)) &&
              !world.Dialogue.TryStartNearest(world),
            "Ambient settlers do not expose fake dialogue prompts before authored graphs exist");

        world.SetPlayerPosition(Vector3.Zero);
        world.NpcWorld.Update(world);
        check(!world.Dialogue.TryStartNearest(world),
            "Dialogue cannot start with an NPC outside interaction range");
    }
}
