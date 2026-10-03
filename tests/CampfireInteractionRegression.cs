using System.Numerics;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class CampfireInteractionRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();

        check(world.Campfires.IsLit(world, "village-firepit"),
            "Village fire starts lit as a reliable heat source");
        check(!world.Campfires.IsLit(world, "forest-hunter-firepit"),
            "Hunter camp fire starts unlit");

        check(world.Campfires.HeatAt(
                world,
                new Vector3(0f, 0f, -88f)) > 0.95f &&
              world.Campfires.HeatAt(
                world,
                new Vector3(37f, 0f, 21f)) == 0f,
            "Only lit campfires emit gameplay heat");

        TerrainVertex[] defaultFireVertices = [];
        uint[] defaultFireIndices = [];
        CampfireEffectMesh.Append(
            world,
            0f,
            ref defaultFireVertices,
            ref defaultFireIndices);

        check(defaultFireVertices.Length == 14 &&
              defaultFireIndices.Length == 30,
            "Only the default-lit village fire renders flames");

        world.SetPlayerPosition(new Vector3(31f, 0f, 15f));
        world.EnvironmentInteractions.Update(world);

        check(world.EnvironmentInteractions.Current?.Id ==
              "resource.forest-resin",
            "Forest resin exposes a contextual pickup");

        check(world.EnvironmentInteractions.TryInteract(world) &&
              world.Progress.Inventory.Count("forest-resin") == 3 &&
              world.Progress.HasFlag(
                  EnvironmentInteractionSystem.ResinCollectedFlag),
            "Resin pickup grants three persistent portions");

        world.SetPlayerPosition(new Vector3(37f, 0f, 21f));
        world.EnvironmentInteractions.Update(world);

        check(world.EnvironmentInteractions.Current?.Id ==
              "campfire.forest-hunter-firepit",
            "Hunter fire exposes a contextual campfire interaction");

        check(world.EnvironmentInteractions.TryInteract(world) &&
              world.Campfires.IsLit(world, "forest-hunter-firepit") &&
              world.Progress.Inventory.Count("forest-resin") == 2,
            "Lighting the hunter fire consumes one resin");

        TerrainVertex[] bothFireVertices = [];
        uint[] bothFireIndices = [];
        CampfireEffectMesh.Append(
            world,
            0.25f,
            ref bothFireVertices,
            ref bothFireIndices);

        check(bothFireVertices.Length == defaultFireVertices.Length * 2 &&
              bothFireIndices.Length == defaultFireIndices.Length * 2,
            "Lighting the hunter fire adds its flame geometry");

        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);

        check(restored.Campfires.IsLit(
                restored,
                "forest-hunter-firepit") &&
              restored.Progress.Inventory.Count("forest-resin") == 2,
            "Lit campfire state and remaining resin survive save/load");

        restored.Weather.SetCondition(WeatherKind.Storm, true);
        restored.Campfires.Update(restored, 11.0);

        check(!restored.Campfires.IsLit(
                  restored,
                  "forest-hunter-firepit") &&
              restored.Campfires.IsLit(
                  restored,
                  "village-firepit") &&
              restored.Progress.HasFlag(
                  CampfireSystem.ExtinguishedFlag(
                      "forest-hunter-firepit")),
            "Heavy rain extinguishes exposed forest fire but not maintained village fire");

        check(restored.Campfires.HeatAt(
                restored,
                new Vector3(37f, 0f, 21f)) == 0f,
            "Rain-extinguished fire stops providing warmth");

        restored.Weather.SetCondition(WeatherKind.Clear, true);
        restored.SetPlayerPosition(new Vector3(37f, 0f, 21f));
        restored.EnvironmentInteractions.Update(restored);

        check(restored.EnvironmentInteractions.TryInteract(restored) &&
              restored.Campfires.IsLit(
                  restored,
                  "forest-hunter-firepit") &&
              restored.Progress.Inventory.Count("forest-resin") == 1,
            "Extinguished hunter fire can be relit after weather clears");

        restored.EnvironmentInteractions.Update(restored);
        check(restored.EnvironmentInteractions.TryInteract(restored) &&
              !restored.Campfires.IsLit(
                  restored,
                  "forest-hunter-firepit"),
            "Player can manually extinguish a lit campfire");
    }
}
