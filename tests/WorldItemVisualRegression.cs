using System.Numerics;
using SlavicGame.Engine.Assets;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

internal static class WorldItemVisualRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var assetsRoot = Path.Combine(AppContext.BaseDirectory, "assets");
        var models = WorldItemVisualCatalog.Definitions
            .Select(definition => definition.AssetPath)
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(
                assetPath => assetPath,
                assetPath =>
                {
                    var path = Path.Combine(
                        assetsRoot,
                        assetPath.Replace('/', Path.DirectorySeparatorChar));
                    check(File.Exists(path),
                        $"Dynamic world item GLB exists: {assetPath}");
                    return GlbModel.Load(path);
                },
                StringComparer.Ordinal);

        check(models.Count == 4,
            "P0 world item pass uses four distinct tracked GLB assets");

        var world = WorldGenerator.Generate();
        var quest = world.Progress.Quests.Get(
            VerticalSliceBootstrap.ContractQuestId);

        var keepsake = Find("visual.quest.keepsake");
        var resin = Find("visual.resource.resin");
        var herb = Find("visual.resource.marsh-herb");
        var thread = Find("visual.quest.ritual-thread");

        check(!WorldItemVisualCatalog.IsVisible(world, keepsake) &&
              WorldItemVisualCatalog.IsVisible(world, resin) &&
              WorldItemVisualCatalog.IsVisible(world, herb) &&
              !WorldItemVisualCatalog.IsVisible(world, thread),
            "New game shows gatherable resources but hides gated quest items");

        TerrainVertex[] initialVertices = [];
        uint[] initialIndices = [];
        WorldItemModelMesh.Append(
            world,
            models,
            ref initialVertices,
            ref initialIndices);

        check(initialVertices.Length > 0 &&
              initialIndices.Length > 0 &&
              initialIndices.All(index => index < initialVertices.Length),
            "Visible resource GLBs append valid dynamic geometry");

        quest.SetPhase(QuestPhase.Active);
        check(WorldItemVisualCatalog.IsVisible(world, keepsake),
            "Accepting the contract reveals the physical keepsake model");

        TerrainVertex[] activeVertices = [];
        uint[] activeIndices = [];
        WorldItemModelMesh.Append(
            world,
            models,
            ref activeVertices,
            ref activeIndices);

        check(activeVertices.Length > initialVertices.Length,
            "Quest activation adds keepsake geometry to the actor buffer");

        Move(world, 82f, 29f);
        world.QuestInteractions.Update(world);
        check(world.QuestInteractions.TryInteract(world) ==
                  QuestInteractionResult.Completed &&
              world.Progress.Inventory.Contains("missing-person-keepsake") &&
              !WorldItemVisualCatalog.IsVisible(world, keepsake),
            "Taking the keepsake removes its GLB visual immediately");

        Move(world, 31f, 15f);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.TryInteract(world) &&
              world.Progress.Inventory.Count("forest-resin") == 3 &&
              !WorldItemVisualCatalog.IsVisible(world, resin),
            "Gathering resin removes the visible resin chunk");

        Move(world, 107f, 20f);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.TryInteract(world) &&
              world.Progress.Inventory.Count("marsh-herb") == 2 &&
              world.Progress.HasFlag(
                  EnvironmentInteractionSystem.MarshHerbCollectedFlag) &&
              !WorldItemVisualCatalog.IsVisible(world, herb),
            "Gathering marsh herb transfers two herbs and removes the bundle model");

        quest.SetPhase(QuestPhase.Investigation);
        quest.SetPhase(QuestPhase.Preparation);
        world.Progress.SetFlag(VerticalSliceRituals.LearnedFlag);

        check(WorldItemVisualCatalog.IsVisible(world, thread),
            "Learned ritual in Preparation reveals the ritual-thread model beside the herbalist");

        Move(world, 5f, -92f);
        world.QuestInteractions.Update(world);
        check(world.QuestInteractions.TryInteract(world) ==
                  QuestInteractionResult.Completed &&
              world.Progress.Inventory.Contains("ritual-thread") &&
              !WorldItemVisualCatalog.IsVisible(world, thread),
            "Receiving ritual thread removes its world visual");

        TerrainVertex[] consumedVertices = [];
        uint[] consumedIndices = [];
        WorldItemModelMesh.Append(
            world,
            models,
            ref consumedVertices,
            ref consumedIndices);

        check(consumedVertices.Length == 0 &&
              consumedIndices.Length == 0,
            "Collected P0 item visuals leave no stale dynamic geometry");

        WorldItemVisualDefinition Find(string id) =>
            WorldItemVisualCatalog.Definitions.Single(
                definition => string.Equals(
                    definition.Id,
                    id,
                    StringComparison.Ordinal));
    }

    private static void Move(
        WorldState world,
        float x,
        float z)
    {
        world.SetPlayerPosition(
            new Vector3(x, 0f, z));
    }
}
