using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class SlavicHerbalismAndActTwoQuestsRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(12);
        world.Initialize();

        // 1. Slavic Herbalism Catalog & Harvesting
        check(SlavicHerbalismSystem.HerbCatalog.Count == 4, "Herbal catalog defines 4 authentic Slavic wild herbs");
        check(SlavicHerbalismSystem.HerbCatalog.ContainsKey("ziolo-dziurawiec"), "Catalog contains Dziurawiec Zwyczajny");
        check(SlavicHerbalismSystem.HerbCatalog.ContainsKey("ziolo-piolun"), "Catalog contains Piołun Polny");
        check(SlavicHerbalismSystem.HerbCatalog.ContainsKey("ziolo-krwawnik"), "Catalog contains Krwawnik Pospolity");
        check(SlavicHerbalismSystem.HerbCatalog.ContainsKey("ziolo-zywokost"), "Catalog contains Korzeń Żywokostu");
        check(SlavicHerbalismSystem.Spawns.Count >= 12, $"World contains {SlavicHerbalismSystem.Spawns.Count} distributed herb spawns");

        // Harvest Dziurawiec at start clearing
        var dziurawiecSpawn = SlavicHerbalismSystem.Spawns.First(s => s.ItemId == "ziolo-dziurawiec");
        check(SlavicHerbalismSystem.CanHarvest(world, dziurawiecSpawn.Id), "Dziurawiec spawn is initially ready to harvest");

        world.SetPlayerPosition(new Vector3(dziurawiecSpawn.Position.X, 0f, dziurawiecSpawn.Position.Y));
        world.EnvironmentInteractions.Update(world);
        var herbTarget = world.EnvironmentInteractions.Current;
        check(herbTarget is not null && herbTarget.Id == $"resource.herb.{dziurawiecSpawn.Id}",
            "Contextual interaction target surfaces for Dziurawiec");

        var harvested = world.EnvironmentInteractions.TryInteract(world);
        check(harvested, "Harvesting Dziurawiec succeeded");
        check(world.Progress.Inventory.Count("ziolo-dziurawiec") == 2, "Inventory received 2x ziolo-dziurawiec");
        check(!SlavicHerbalismSystem.CanHarvest(world, dziurawiecSpawn.Id), "Harvested herb spawn is no longer harvestable");

        // Harvest Żywokost near river
        var zywokostSpawn = SlavicHerbalismSystem.Spawns.First(s => s.ItemId == "ziolo-zywokost");
        world.SetPlayerPosition(new Vector3(zywokostSpawn.Position.X, 0f, zywokostSpawn.Position.Y));
        world.EnvironmentInteractions.Update(world);
        var zywokostHarvested = world.EnvironmentInteractions.TryInteract(world);
        check(zywokostHarvested, "Harvesting Żywokost succeeded");
        check(world.Progress.Inventory.Count("ziolo-zywokost") == 2, "Inventory received 2x ziolo-zywokost");

        // 2. Slavic Waymarker Signposts
        check(SlavicWaymarkerSystem.Waymarkers.Count >= 5, "Waymarker system provides crossroads signposts across regions");
        var zarnowiecMarker = SlavicWaymarkerSystem.Waymarkers.First(w => w.Id == "waymarker-zarnowiec");
        world.SetPlayerPosition(new Vector3(zarnowiecMarker.Position.X, 0f, zarnowiecMarker.Position.Y));
        world.EnvironmentInteractions.Update(world);
        var waymarkerTarget = world.EnvironmentInteractions.Current;
        check(waymarkerTarget is not null && waymarkerTarget.Id == zarnowiecMarker.Id,
            "Contextual interaction target surfaces for Żarnowiec crossroads signpost");

        var inspected = world.EnvironmentInteractions.TryInteract(world);
        check(inspected, "Reading signpost succeeds");
        check(world.Progress.HasFlag(SlavicWaymarkerSystem.DiscoveredFlag(zarnowiecMarker.Id)),
            "Signpost discovery flag recorded in progress");

        // 3. Campfire Rest & Vitals Restoration
        check(CampfireSystem.Fires.Count >= 7, $"World features {CampfireSystem.Fires.Count} campfires across regions and outposts");
        var villageFire = CampfireSystem.Get("village-firepit");
        check(world.Campfires.IsLit(world, villageFire.Id), "Village firepit is lit by default");

        world.Player.TakeDamage(50f);
        check(world.Player.Health == 50f, "Player took 50 damage (Health is 50)");

        var restPos = new Vector3(villageFire.Position.X + 0.75f, 0f, villageFire.Position.Y + 0.75f);
        world.SetPlayerPosition(restPos);
        world.EnvironmentInteractions.Update(world);
        var restTarget = world.EnvironmentInteractions.Current;
        check(restTarget is not null && restTarget.Id == $"campfire.{villageFire.Id}.rest",
            "Contextual resting interaction target surfaces near lit campfire");

        var rested = world.EnvironmentInteractions.TryInteract(world);
        check(rested, "Interacting with lit campfire rests the player");
        check(world.Player.Health == 100f, "Resting at campfire fully restored player vitals (100 HP)");

        // 4. Herbal Alchemy & Potion Crafting
        world.Time.SetTimeOfDay(10);
        world.Weather.SetCondition(WeatherKind.Clear, immediate: true);
        world.NpcWorld.Update(world);

        var herbalist = world.NpcWorld.Find("herbalist") ?? throw new Exception("Herbalist missing");
        world.SetPlayerPosition(herbalist.Position);
        world.NpcWorld.Update(world);

        check(world.Crafting.CanOpenNearest(world), "Can open crafting UI near herbalist Mirosława");
        world.Crafting.TryOpenNearest(world);
        check(world.Crafting.IsOpen, "Crafting UI opened");

        var recipes = world.Crafting.BuildLines(world);
        check(recipes.Any(r => r.RecipeId == "recipe.healing-herbal-tincture"),
            "Crafting menu includes 'Napar z Dziurawca i Miodu'");
        check(recipes.Any(r => r.RecipeId == "recipe.comfrey-salve"),
            "Crafting menu includes 'Maść Żywokostowa'");
        check(recipes.Any(r => r.RecipeId == "recipe.wormwood-extract"),
            "Crafting menu includes 'Odwar z Piołunu'");

        // Craft Napar z Dziurawca
        world.Progress.Inventory.Add("forest-resin", 5);
        var tinctureIndex = recipes.Select((r, i) => (r, i)).First(x => x.r.RecipeId == "recipe.healing-herbal-tincture").i;
        while (world.Crafting.SelectedIndex < tinctureIndex) world.Crafting.MoveSelection(world, 1);
        while (world.Crafting.SelectedIndex > tinctureIndex) world.Crafting.MoveSelection(world, -1);

        check(world.Crafting.Confirm(world) == CraftingResult.Completed, "Crafting Napar z Dziurawca succeeded");
        check(world.Progress.Inventory.Contains("napar-leczniczy"), "Player inventory received 'napar-leczniczy'");
        check(world.Progress.Inventory.Count("ziolo-dziurawiec") == 0, "Dziurawiec herbs consumed in crafting");

        // Craft Maść Żywokostowa
        recipes = world.Crafting.BuildLines(world);
        var salveIndex = recipes.Select((r, i) => (r, i)).First(x => x.r.RecipeId == "recipe.comfrey-salve").i;
        while (world.Crafting.SelectedIndex < salveIndex) world.Crafting.MoveSelection(world, 1);
        while (world.Crafting.SelectedIndex > salveIndex) world.Crafting.MoveSelection(world, -1);

        check(world.Crafting.Confirm(world) == CraftingResult.Completed, "Crafting Maść Żywokostowa succeeded");
        check(world.Progress.Inventory.Contains("masc-zywokostowa"), "Player inventory received 'masc-zywokostowa'");

        world.Crafting.Close();

        // 5. Act II: MQ21 ("Spór o Trakt Rzeczny")
        // Prepare Act 1 & MQ20 completion
        world.Progress.SetFlag(VerticalSliceQuestInteractions.TurnedInFlag);
        ActOneCampaignSystem.TakeLead(world);
        ActOneCampaignSystem.EnterStronghold(world);
        ActOneCampaignSystem.InspectKurhan(world);
        ActOneCampaignSystem.HoldCouncil(world);
        ActOneCampaignSystem.InvestigatePerunBór(world);
        ActOneCampaignSystem.SynthesizeMapTable(world);

        ActTwoCampaignSystem.BeginAct2(world);
        ActTwoCampaignSystem.NegotiateEstuary(world);
        ActTwoCampaignSystem.InspectRemoteBeacon(world);
        ActTwoCampaignSystem.ResolveEstuary(world);
        check(world.Progress.HasFlag(Mq20Campaign.Complete), "MQ20 complete");
        check(ActTwoCampaignSystem.CanBeginMq21(world), "MQ21 is ready to begin");

        // Begin MQ21
        world.SetPlayerPosition(ActTwoCampaignSystem.RiverCrossingBridgeSpot);
        world.EnvironmentInteractions.Update(world);
        var mq21BeginTarget = world.EnvironmentInteractions.Current;
        check(mq21BeginTarget is not null && mq21BeginTarget.Id == "act2.mq21.begin",
            "MQ21 begin interaction surfaces at Great River Bridge");

        var mq21Begun = world.EnvironmentInteractions.TryInteract(world);
        check(mq21Begun, "Beginning MQ21 succeeded");
        check(world.Progress.Quests.Get(Mq21Campaign.QuestId).Phase == QuestPhase.Investigation,
            "MQ21 entered investigation phase");

        // Inspect League Claim
        world.SetPlayerPosition(ActTwoCampaignSystem.LeagueClaimSpot);
        world.EnvironmentInteractions.Update(world);
        var leagueTarget = world.EnvironmentInteractions.Current;
        check(leagueTarget is not null && leagueTarget.Id == "act2.mq21.league",
            "League claim prompt surfaces at merchant bridge post");
        check(world.EnvironmentInteractions.TryInteract(world), "Inspecting League claim succeeded");
        check(world.Progress.HasFlag("MQ21_CLAIM_VERIFIED_LEAGUE"), "League claim verified flag set");

        // Inspect Nadbor Claim
        world.SetPlayerPosition(ActTwoCampaignSystem.NadborClaimSpot);
        world.EnvironmentInteractions.Update(world);
        var nadborTarget = world.EnvironmentInteractions.Current;
        check(nadborTarget is not null && nadborTarget.Id == "act2.mq21.nadbor",
            "Nadbor claim prompt surfaces at northern shore");
        check(world.EnvironmentInteractions.TryInteract(world), "Inspecting Nadbor claim succeeded");
        check(world.Progress.HasFlag("MQ21_CLAIM_VERIFIED_NADBOR"), "Nadbor claim verified flag set");

        // Resolve River Treaty & complete MQ21
        world.SetPlayerPosition(ActTwoCampaignSystem.TradeTreatySpot);
        world.EnvironmentInteractions.Update(world);
        var resolveTreatyTarget = world.EnvironmentInteractions.Current;
        check(resolveTreatyTarget is not null && resolveTreatyTarget.Id == "act2.mq21.resolve",
            "Trade treaty resolution prompt surfaces at river boundary stone");
        check(world.EnvironmentInteractions.TryInteract(world), "Signing river trade treaty succeeded");
        check(world.Progress.HasFlag(Mq21Campaign.Complete), "MQ21 is marked complete");
        check(world.Progress.Quests.Get(Mq21Campaign.NextQuestId).Phase == QuestPhase.Offered,
            "MQ22 is unlocked and offered in Quest Journal!");

        // 6. Act II: MQ22 ("Wyłom w Starej Sztolni")
        check(ActTwoCampaignSystem.CanBeginMq22(world), "MQ22 is ready to begin");

        // Begin MQ22 at Stara Sztolnia
        world.SetPlayerPosition(ActTwoCampaignSystem.MineBreachSpot);
        world.EnvironmentInteractions.Update(world);
        var mq22BeginTarget = world.EnvironmentInteractions.Current;
        check(mq22BeginTarget is not null && mq22BeginTarget.Id == "act2.mq22.begin",
            "MQ22 begin prompt surfaces at Stara Sztolnia entrance");
        check(world.EnvironmentInteractions.TryInteract(world), "Beginning MQ22 succeeded");

        // Enter mine breach
        world.EnvironmentInteractions.Update(world);
        var breachTarget = world.EnvironmentInteractions.Current;
        check(breachTarget is not null && breachTarget.Id == "act2.mq22.breach",
            "Mine breach entry prompt surfaces");
        check(world.EnvironmentInteractions.TryInteract(world), "Entering mine breach succeeded");
        check(world.Progress.HasFlag(Mq22Campaign.MineBreach), "Mine breach flag confirmed");

        // Inspect Timbering
        world.SetPlayerPosition(ActTwoCampaignSystem.AncientTimberingSpot);
        world.EnvironmentInteractions.Update(world);
        var timberTarget = world.EnvironmentInteractions.Current;
        check(timberTarget is not null && timberTarget.Id == "act2.mq22.timber",
            "Ancient timbering clue prompt surfaces");
        check(world.EnvironmentInteractions.TryInteract(world), "Inspecting timbering succeeded");
        check(world.Progress.HasFlag("MQ22_CONSTRUCTION_CLUE_MATERIALLAYER"), "Timbering clue recorded");

        // Inspect Tools
        world.SetPlayerPosition(ActTwoCampaignSystem.RunicToolsSpot);
        world.EnvironmentInteractions.Update(world);
        var toolsTarget = world.EnvironmentInteractions.Current;
        check(toolsTarget is not null && toolsTarget.Id == "act2.mq22.tools",
            "Runic tools clue prompt surfaces");
        check(world.EnvironmentInteractions.TryInteract(world), "Inspecting runic tools succeeded");
        check(world.Progress.HasFlag("MQ22_CONSTRUCTION_CLUE_TOOLINGPATTERN"), "Tooling pattern clue recorded");

        // Analyze Anchor Ore
        world.SetPlayerPosition(ActTwoCampaignSystem.AnchorOreSpot);
        world.EnvironmentInteractions.Update(world);
        var oreTarget = world.EnvironmentInteractions.Current;
        check(oreTarget is not null && oreTarget.Id == "act2.mq22.anchor",
            "Magnetic anchor ore analysis prompt surfaces");
        check(world.EnvironmentInteractions.TryInteract(world), "Analyzing anchor ore succeeded");
        check(world.Progress.HasFlag(Mq22Campaign.AnchorMaterial), "Anchor material flag confirmed");

        // Stabilize Mine Entrance & complete MQ22
        world.SetPlayerPosition(ActTwoCampaignSystem.StabilizationSpot);
        world.EnvironmentInteractions.Update(world);
        var stabilizeTarget = world.EnvironmentInteractions.Current;
        check(stabilizeTarget is not null && stabilizeTarget.Id == "act2.mq22.stabilize",
            "Mine stabilization prompt surfaces");
        check(world.EnvironmentInteractions.TryInteract(world), "Stabilizing mine entrance succeeded");
        check(world.Progress.HasFlag(Mq22Campaign.Complete), "MQ22 marked complete");
        check(world.Progress.Quests.Get(Mq22Campaign.NextQuestId).Phase == QuestPhase.Offered,
            "MQ23 is unlocked and offered in Quest Journal!");

        // 7. Durability across Save/Load
        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        restored.Initialize();
        SaveGameService.Restore(restored, json);

        check(restored.Progress.HasFlag(Mq21Campaign.Complete), "MQ21 completion survives save/load");
        check(restored.Progress.HasFlag(Mq22Campaign.Complete), "MQ22 completion survives save/load");
        check(restored.Progress.Quests.Get(Mq22Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ23 offer survives save/load");
        check(restored.Progress.Inventory.Contains("napar-leczniczy"), "Crafted Napar survives save/load");
        check(restored.Progress.Inventory.Contains("masc-zywokostowa"), "Crafted Maść survives save/load");
        check(!SlavicHerbalismSystem.CanHarvest(restored, dziurawiecSpawn.Id), "Herb harvested state survives save/load");
    }
}
