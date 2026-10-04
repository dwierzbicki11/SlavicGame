using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class CraftingRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10);
        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.NpcWorld.Update(world);

        var herbalist = world.NpcWorld.Find("herbalist")
            ?? throw new Exception("Herbalist is missing");
        world.SetPlayerPosition(herbalist.Position);
        world.NpcWorld.Update(world);

        check(world.Crafting.TryOpenNearest(world) &&
              world.Crafting.IsOpen,
            "Nearby daytime herbalist exposes alchemy station");

        var locked = world.Crafting.BuildLines(world);
        check(locked.Count == 1 &&
              locked[0].RecipeId == "recipe.marsh-sight-tonic" &&
              !locked[0].Unlocked &&
              !locked[0].CanCraft,
            "Tonic recipe starts hidden behind knowledge gate");

        check(world.Crafting.Confirm(world) ==
                  CraftingResult.Locked &&
              world.Progress.Inventory.Count("marsh-sight-tonic") == 0,
            "Unknown recipe cannot be crafted");

        world.Progress.SetFlag("recipe.marsh-sight-tonic.learned");
        check(world.Crafting.Confirm(world) ==
                  CraftingResult.MissingIngredients,
            "Known recipe reports missing ingredients without partial craft");

        var herbBefore =
            world.Progress.Inventory.Count("marsh-herb");
        var resinBefore =
            world.Progress.Inventory.Count("forest-resin");
        check(herbBefore == 0 && resinBefore == 0,
            "Missing-ingredient validation consumes nothing");

        world.Progress.Inventory.Add("marsh-herb", 2);
        world.Progress.Inventory.Add("forest-resin", 2);

        var ready = world.Crafting.BuildLines(world);
        check(ready[0].Unlocked &&
              ready[0].CanCraft &&
              ready[0].RequirementText.Contains("2/1", StringComparison.Ordinal),
            "Alchemy HUD data exposes owned versus required ingredients");

        check(world.Crafting.Confirm(world) ==
                  CraftingResult.Completed &&
              world.Progress.Inventory.Count("marsh-herb") == 1 &&
              world.Progress.Inventory.Count("forest-resin") == 1 &&
              world.Progress.Inventory.Count("marsh-sight-tonic") == 1,
            "Tonic craft atomically consumes one herb and resin and creates output");

        check(world.Crafting.Confirm(world) ==
                  CraftingResult.Completed &&
              world.Progress.Inventory.Count("marsh-herb") == 0 &&
              world.Progress.Inventory.Count("forest-resin") == 0 &&
              world.Progress.Inventory.Count("marsh-sight-tonic") == 2,
            "Repeated craft consumes a fresh ingredient set each time");

        check(world.Crafting.Confirm(world) ==
                  CraftingResult.MissingIngredients &&
              world.Progress.Inventory.Count("marsh-sight-tonic") == 2,
            "Crafting cannot duplicate output after ingredients are exhausted");

        var herbalistVendor = VendorRuntime.Catalog.Single(vendor =>
            vendor.Id == "vendor.r0.herbalist");
        var tonicOffer = herbalistVendor.Offers.Single(offer =>
            offer.ItemId == "marsh-sight-tonic");
        check(!tonicOffer.PlayerMaySell,
            "Crafted tonic cannot create a craft-sell money loop in R0 first pass");

        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);

        check(restored.Progress.Inventory.Count("marsh-sight-tonic") == 2 &&
              restored.Progress.HasFlag("recipe.marsh-sight-tonic.learned") &&
              !restored.Crafting.IsOpen,
            "Craft result and recipe knowledge survive save/load while crafting UI stays transient");

        restored.Time.SetTimeOfDay(23);
        restored.NpcWorld.Update(restored);
        var restingHerbalist = restored.NpcWorld.Find("herbalist")
            ?? throw new Exception("Resting herbalist missing");
        restored.SetPlayerPosition(restingHerbalist.Position);
        restored.NpcWorld.Update(restored);

        check(!restored.Crafting.TryOpenNearest(restored),
            "Alchemy station is unavailable when the herbalist is off duty");
    }
}
