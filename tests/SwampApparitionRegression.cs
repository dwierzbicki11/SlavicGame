using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

internal static class SwampApparitionRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        var quest = world.Progress.Quests.Get(
            VerticalSliceBootstrap.ContractQuestId);
        quest.SetPhase(QuestPhase.Active);

        var nearAnchor = new Vector3(70f, 0f, 24f);
        world.SetPlayerPosition(nearAnchor);

        world.Time.SetTimeOfDay(12);
        world.Apparition.Update(world, 2.0);

        check(!world.Apparition.IsVisible &&
              world.Apparition.Materialization < 0.01f,
            "Swamp apparition does not materialize in daylight");

        world.Time.SetTimeOfDay(23);
        world.Apparition.Update(world, 1.5);

        check(world.Apparition.IsVisible &&
              world.Apparition.Materialization > 0.85f,
            "Active quest and nearby player materialize the apparition at night");

        TerrainVertex[] apparitionVertices = [];
        uint[] apparitionIndices = [];
        ApparitionEffectMesh.Append(
            world,
            0.75f,
            ref apparitionVertices,
            ref apparitionIndices);

        check(apparitionVertices.Length > 0 &&
              apparitionVertices.Length < 256 &&
              apparitionIndices.Length > 0 &&
              apparitionIndices.All(index => index < apparitionVertices.Length),
            "Night apparition renders a bounded valid low-poly spectral mesh");

        check(!quest.Evidence.Any(entry =>
                entry.Id == "light-over-swamp.apparition-response"),
            "Seeing the apparition does not automatically award quest evidence");

        var anchorBeforeKeepsake = world.Apparition.Position;
        world.Progress.Inventory.Add("missing-person-keepsake");
        world.Apparition.Update(world, 1.5);

        check(world.Apparition.IsReactingToKeepsake &&
              world.Apparition.KeepsakeResponse > 0.85f &&
              Vector2.Distance(
                  new Vector2(
                      world.Apparition.Position.X,
                      world.Apparition.Position.Z),
                  new Vector2(
                      world.PlayerPosition.X,
                      world.PlayerPosition.Z)) <
              Vector2.Distance(
                  new Vector2(
                      anchorBeforeKeepsake.X,
                      anchorBeforeKeepsake.Z),
                  new Vector2(
                      world.PlayerPosition.X,
                      world.PlayerPosition.Z)),
            "Keepsake makes the apparition respond and approach without becoming an HP target");

        var ritualWorld = WorldGenerator.Generate();
        var ritualQuest = ritualWorld.Progress.Quests.Get(
            VerticalSliceBootstrap.ContractQuestId);
        ritualQuest.SetPhase(QuestPhase.Preparation);
        ritualQuest.AddEvidence(new EvidenceEntry(
            "light-over-swamp.keepsake-owner",
            ritualQuest.Id,
            KnowledgeKind.ConfirmedFact,
            "Pamiatka nalezala do zaginionego.",
            "missing-family"));

        ritualWorld.Progress.SetFlag(VerticalSliceRituals.LearnedFlag);
        ritualWorld.Progress.SetFlag(VerticalSliceRituals.IdentitySignFlag);
        ritualWorld.Progress.SetFlag(VerticalSliceRituals.BoundarySignFlag);
        ritualWorld.Progress.SetFlag(VerticalSliceRituals.AnchorKnowledgeFlag);
        ritualWorld.Progress.Inventory.Add("missing-person-keepsake");
        ritualWorld.Progress.Inventory.Add("ritual-thread");
        ritualWorld.SetPlayerPosition(new Vector3(-85f, 0f, 55f));
        ritualWorld.Time.SetTimeOfDay(23);

        var ritualStart = ritualWorld.Rituals.TryStart(ritualWorld);
        ritualWorld.Apparition.Update(ritualWorld, 1.0);

        check(ritualStart.Started &&
              ritualWorld.Apparition.IsVisible &&
              Vector3.Distance(
                  ritualWorld.Apparition.Position,
                  ritualWorld.Rituals.VisualOrigin) < 0.5f,
            "Ritual manifests the same apparition at the ritual origin");

        ritualWorld.Rituals.Update(ritualWorld, 10.0);
        check(ritualWorld.Progress.HasFlag(
                VerticalSliceRituals.ReleasedFlag),
            "Release ritual resolves the apparition before fade test");

        ritualWorld.Apparition.Update(ritualWorld, 6.0);

        apparitionVertices = [];
        apparitionIndices = [];
        ApparitionEffectMesh.Append(
            ritualWorld,
            8.0f,
            ref apparitionVertices,
            ref apparitionIndices);

        check(!ritualWorld.Apparition.IsVisible &&
              ritualWorld.Apparition.Materialization < 0.025f &&
              apparitionVertices.Length == 0 &&
              apparitionIndices.Length == 0,
            "Released apparition fully dissolves instead of remaining in the world");
    }
}
