using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class VendorRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(10);
        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.NpcWorld.Update(world);

        var trader = world.NpcWorld.Find("settler-trader-01")
            ?? throw new Exception("R0 trader is missing");
        world.SetPlayerPosition(trader.Position);
        world.NpcWorld.Update(world);

        check(world.Vendors.TryOpenNearest(world) &&
              world.Vendors.IsOpen &&
              world.Vendors.VendorId == "vendor.r0.trader",
            "Nearby daytime trader opens the R0 vendor");

        var buyLines = world.Vendors.BuildLines(world);
        check(buyLines.Count == 3 &&
              buyLines[0].ItemId == "simple-bandage" &&
              buyLines[1].ItemId == "arrow-basic" &&
              buyLines[2].ItemId == "forest-resin",
            "Trader exposes deterministic first-pass stock");

        world.Progress.Profile.ChangeMoney(100);
        var moneyBefore = world.Progress.Profile.Money;
        var stockBefore =
            world.Vendors.StockOf(
                "vendor.r0.trader",
                "simple-bandage");
        var bandagesBefore =
            world.Progress.Inventory.Count("simple-bandage");

        var bandagePrice = buyLines[0].Price;
        check(world.Vendors.Confirm(world) ==
                  VendorTransactionResult.Completed &&
              world.Progress.Profile.Money ==
                  moneyBefore - bandagePrice &&
              world.Progress.Inventory.Count("simple-bandage") ==
                  bandagesBefore + 1 &&
              world.Vendors.StockOf(
                  "vendor.r0.trader",
                  "simple-bandage") ==
                  stockBefore - 1,
            "Vendor purchase atomically moves money item and stock");

        world.Progress.Inventory.Add("forest-resin", 2);
        world.Vendors.SetMode(world, VendorMode.Sell);
        var sellLines = world.Vendors.BuildLines(world);
        var resinIndex = sellLines
            .Select((line, index) => (line, index))
            .Single(pair => pair.line.ItemId == "forest-resin")
            .index;
        while (world.Vendors.SelectedIndex != resinIndex)
            world.Vendors.MoveSelection(world, 1);

        var moneyBeforeSale = world.Progress.Profile.Money;
        var resinBefore =
            world.Progress.Inventory.Count("forest-resin");
        var resinStockBefore =
            world.Vendors.StockOf(
                "vendor.r0.trader",
                "forest-resin");
        var resinSellPrice =
            world.Vendors.BuildLines(world)[resinIndex].Price;

        check(world.Vendors.Confirm(world) ==
                  VendorTransactionResult.Completed &&
              world.Progress.Profile.Money ==
                  moneyBeforeSale + resinSellPrice &&
              world.Progress.Inventory.Count("forest-resin") ==
                  resinBefore - 1 &&
              world.Vendors.StockOf(
                  "vendor.r0.trader",
                  "forest-resin") ==
                  resinStockBefore + 1,
            "Vendor sale atomically returns item to stock and pays the player");

        check(VendorRuntime.Catalog
                .SelectMany(vendor => vendor.Offers)
                .All(offer =>
                    offer.ItemId != "missing-person-keepsake" &&
                    offer.ItemId != "ritual-thread"),
            "Quest-protected items never enter vendor offers");

        var traderDefinition = VendorRuntime.Catalog.Single(vendor =>
            vendor.Id == "vendor.r0.trader");
        var arrowOffer = traderDefinition.Offers.Single(offer =>
            offer.ItemId == "arrow-basic");

        var neutralBuy =
            world.Vendors.BuyPrice(world, arrowOffer);
        world.Progress.Reputation.Change(
            ReputationScope.Village,
            "old-village",
            60);
        var trustedBuy =
            world.Vendors.BuyPrice(world, arrowOffer);
        var trustedSell =
            world.Vendors.SellPrice(world, arrowOffer);
        world.Progress.Reputation.Change(
            ReputationScope.Village,
            "old-village",
            -100);
        var dislikedBuy =
            world.Vendors.BuyPrice(world, arrowOffer);

        check(trustedBuy <= neutralBuy &&
              dislikedBuy >= neutralBuy &&
              trustedSell >= 1,
            "Village reputation applies a bounded favorable vendor price modifier");

        world.Vendors.Close();
        var herbalist = world.NpcWorld.Find("herbalist")
            ?? throw new Exception("Herbalist is missing");
        world.SetPlayerPosition(herbalist.Position);
        world.NpcWorld.Update(world);

        check(world.Vendors.TryOpenNearest(world) &&
              world.Vendors.VendorId == "vendor.r0.herbalist",
            "Nearby daytime herbalist opens her vendor");

        check(world.Vendors.BuildLines(world)
                .All(line => line.ItemId != "marsh-sight-tonic"),
            "Locked tonic is hidden before recipe knowledge");

        world.Progress.SetFlag("recipe.marsh-sight-tonic.learned");
        check(world.Vendors.BuildLines(world)
                .Any(line => line.ItemId == "marsh-sight-tonic"),
            "Recipe knowledge unlocks herbalist tonic stock");

        var traderStockAfterPurchase = stockBefore - 1;
        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);

        check(restored.Vendors.StockOf(
                  "vendor.r0.trader",
                  "simple-bandage") ==
                  traderStockAfterPurchase &&
              !restored.Vendors.IsOpen,
            "Vendor stock survives save/load while transient UI stays closed");

        restored.Time.SetTimeOfDay(23);
        restored.NpcWorld.Update(restored);
        var restingTrader = restored.NpcWorld.Find("settler-trader-01")
            ?? throw new Exception("Resting trader is missing");
        restored.SetPlayerPosition(restingTrader.Position);
        restored.NpcWorld.Update(restored);

        check(!restored.Vendors.TryOpenNearest(restored),
            "Vendor service is unavailable outside authored work hours");

        var poorWorld = WorldGenerator.Generate();
        poorWorld.Time.SetTimeOfDay(10);
        poorWorld.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        poorWorld.NpcWorld.Update(poorWorld);
        var poorTrader = poorWorld.NpcWorld.Find("settler-trader-01")
            ?? throw new Exception("Poor-world trader is missing");
        poorWorld.SetPlayerPosition(poorTrader.Position);
        poorWorld.NpcWorld.Update(poorWorld);
        check(poorWorld.Vendors.TryOpenNearest(poorWorld) &&
              poorWorld.Vendors.Confirm(poorWorld) ==
                  VendorTransactionResult.NotEnoughMoney,
            "Vendor refuses purchase without enough money");
    }
}
