using System.Numerics;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Input;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class ActTwoCampaignFinaleRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        TestSacredSpringsAndVillageWells(check);
        TestWorldChestsAndHiddenCaches(check);
        TestActTwoCampaignProgressionThroughFinale(check);
        TestEnvironmentInteractionsIntegration(check);
        TestSaveLoadDurability(check);
    }

    private static void TestSacredSpringsAndVillageWells(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // 1. Catalog validation
        var springs = SlavicSpringSystem.Springs;
        check(springs.Count >= 4, "SlavicSpringSystem provides at least 4 authentic sacred springs and wells");
        check(springs.Any(s => s.Id == "well-zarnowiec" && s.RegionId == "old-village"), "Village well in Żarnowiec is registered");
        check(springs.Any(s => s.Id == "spring-swiety-gaj" && s.RegionId == "old-shrine"), "Sacred grove spring is registered");
        check(springs.Any(s => s.Id == "spring-perun" && s.RegionId == "grod-borzywoj"), "Perun's living water spring is registered");
        check(springs.Any(s => s.Id == "spring-rybackie" && s.RegionId == "przystan-rybacka"), "Harbor creek spring is registered");

        // 2. Drinking from well
        var well = springs.First(s => s.Id == "well-zarnowiec");
        world.SetPlayerPosition(well.Position);
        world.Player.TakeDamage(30f);
        world.Player.TrySpendStamina(50f);
        var hpBefore = world.Player.Health;

        check(world.Springs.TryDrink(world, well.Id), "Drinking from village well succeeds when in range");
        check(world.Springs.HasBlessedWater, "Drinking activates Blessed Water buff");
        check(world.Springs.BlessedWaterBuffRemainingSeconds > 100.0, "Blessed Water duration is initialized to 120 seconds");
        check(world.Player.Health > hpBefore, "Drinking from well restores player health");
        check(world.Player.Stamina == world.Player.MaxStamina, "Drinking from well restores full stamina");

        // 3. Distance check
        world.SetPlayerPosition(new Vector3(500f, 0f, 500f));
        check(!world.Springs.TryDrink(world, well.Id), "Drinking fails when player is far from spring");

        // 4. Recovery multiplier integration
        var camera = new Camera3D();
        world.SetPlayerPosition(well.Position);
        world.Player.TrySpendStamina(20f);
        var staminaPreUpdate = world.Player.Stamina;
        PlayerController.Update(world, camera, new PlayerInput(false, false, false, false, false, Vector2.Zero), 1.0);
        check(world.Player.Stamina > staminaPreUpdate, "Stamina recovers with active Blessed Water buff");

        // 5. Time update decrements buff
        var prevBuffTime = world.Springs.BlessedWaterBuffRemainingSeconds;
        world.Update(2.0);
        check(world.Springs.BlessedWaterBuffRemainingSeconds < prevBuffTime, "World update decrements blessed water buff timer");
    }

    private static void TestWorldChestsAndHiddenCaches(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // 1. Backwards compatibility for TargetId position
        var traderChestPos = LootContainerRuntime.Position(world);
        check(traderChestPos.HasValue, "LootContainerRuntime.Position(world) returns trader chest position");

        // 2. Catalog check
        check(LootContainerRuntime.WorldContainers.Count >= 6, "World defines multiple authentic chests across settlements");
        check(LootContainerRuntime.WorldContainers.ContainsKey("stronghold-chest-hall"), "Stronghold armory chest is defined");
        check(LootContainerRuntime.WorldContainers.ContainsKey("smolnik-chest"), "Smolnik forest cache is defined");
        check(LootContainerRuntime.WorldContainers.ContainsKey("mine-chest-gwarek"), "Ancient mine gwarek chest is defined");

        // 3. Opening stronghold armory chest
        var hallChestPos = LootContainerRuntime.WorldContainers["stronghold-chest-hall"].Position;
        world.SetPlayerPosition(hallChestPos);
        check(world.Loot.CanOpenNearest(world), "Player near stronghold armory chest can open it");
        check(world.Loot.TryOpenNearest(world), "TryOpenNearest successfully opens stronghold armory chest");
        check(world.Loot.ActiveContainerId == "stronghold-chest-hall", "ActiveContainerId reflects opened chest");
        check(world.Loot.CurrentContainerTitle.Contains("SALI GRODU"), "Chest title displays Great Hall container");

        var containerLines = world.Loot.Lines(world, LootPanel.Container);
        check(containerLines.Any(s => s.ItemId == "iron-chunk" && s.Quantity >= 5), "Armory chest contains iron chunks");
        check(containerLines.Any(s => s.ItemId == "arrow-iron" && s.Quantity >= 20), "Armory chest contains iron arrows");

        // Transfer one item from chest to inventory
        var invArrowsBefore = world.Progress.Inventory.Count("arrow-iron");
        var arrowIndex = containerLines.Select((l, i) => (l, i)).First(x => x.l.ItemId == "arrow-iron").i;
        for (var i = 0; i < arrowIndex; i++) world.Loot.MoveSelection(world, 1);
        check(world.Loot.TransferSelected(world) == LootContainerResult.Looted, "Transferring one item from world chest succeeds");
        check(world.Progress.Inventory.Count("arrow-iron") == invArrowsBefore + 1, "Inventory arrow count incremented by 1");

        world.Loot.Close();
        check(!world.Loot.IsOpen, "Chest closed cleanly");

        // 4. Opening Old Mine gwarek chest
        var mineChestPos = LootContainerRuntime.WorldContainers["mine-chest-gwarek"].Position;
        world.SetPlayerPosition(mineChestPos);
        check(world.Loot.TryOpenNearest(world), "Player can open ancient mine gwarek chest");
        check(world.Loot.ActiveContainerId == "mine-chest-gwarek", "Mine chest is active");
        var mineLines = world.Loot.Lines(world, LootPanel.Container);
        check(mineLines.Any(s => s.ItemId == "iron-chunk" && s.Quantity >= 8), "Mine chest contains raw iron chunks");
        world.Loot.Close();
    }

    private static void TestActTwoCampaignProgressionThroughFinale(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // Setup prerequisites: Act I complete, MQ20-MQ22 complete
        world.Progress.SetFlag(Mq13Campaign.Complete);
        world.Progress.SetFlag(Mq13Campaign.NetworkHypothesis);
        world.Progress.SetFlag(Mq20Campaign.Complete);
        world.Progress.SetFlag(Mq20Campaign.RemoteNodeEvidence);
        world.Progress.SetFlag(Mq21Campaign.Complete);
        world.Progress.SetFlag(Mq22Campaign.Complete);
        world.Progress.SetFlag(Mq22Campaign.AnchorMaterial);
        world.Progress.Quests.Get(Mq23Campaign.QuestId).SetPhase(QuestPhase.Offered);

        // --- MQ23: PODZIAŁ ZASOBÓW STAREJ SZTOLNI ---
        check(ActTwoCampaignSystem.CanBeginMq23(world), "MQ23 can be started after MQ22 complete");
        check(ActTwoCampaignSystem.BeginMq23(world), "MQ23 begins successfully");
        check(world.Progress.Quests.Get(Mq23Campaign.QuestId).Phase == QuestPhase.Investigation, "MQ23 in Investigation phase");

        check(ActTwoCampaignSystem.CanReviewImpact(world, Mq23Impact.Extraction), "Can review extraction impact");
        check(ActTwoCampaignSystem.ReviewImpact(world, Mq23Impact.Extraction), "Reviewed extraction impact");
        check(!ActTwoCampaignSystem.CanReviewImpact(world, Mq23Impact.Extraction), "Extraction review is idempotent");

        check(ActTwoCampaignSystem.CanReviewImpact(world, Mq23Impact.Safety), "Can review safety impact");
        check(ActTwoCampaignSystem.ReviewImpact(world, Mq23Impact.Safety), "Reviewed safety impact");

        check(!ActTwoCampaignSystem.CanChooseResourceControl(world), "Cannot choose resource control until all 3 impacts reviewed");
        check(ActTwoCampaignSystem.CanReviewImpact(world, Mq23Impact.Stabilization), "Can review stabilization impact");
        check(ActTwoCampaignSystem.ReviewImpact(world, Mq23Impact.Stabilization), "Reviewed stabilization impact");

        check(ActTwoCampaignSystem.CanChooseResourceControl(world), "Can choose resource control after all 3 impacts reviewed");
        var coinsBefore = world.Progress.Inventory.Count("old-coin");
        check(ActTwoCampaignSystem.ChooseResourceControl(world, Mq23ResourceDecision.SupervisedSplit), "Chose supervised resource division and completed MQ23");
        check(world.Progress.HasFlag(Mq23Campaign.Complete), "MQ23_COMPLETE flag is set");
        check(world.Progress.Inventory.Count("old-coin") > coinsBefore, "MQ23 granted monetary reward");
        check(world.Progress.Quests.Get(Mq24Campaign.QuestId).Phase == QuestPhase.Offered, "MQ24 is offered next");

        // --- MQ24: PRÓBA PRZEJŚCIA PRZEZ BÓR PERUNOWY ---
        check(ActTwoCampaignSystem.CanBeginMq24(world), "MQ24 can be started after MQ23 complete");
        check(ActTwoCampaignSystem.BeginMq24(world), "MQ24 begins successfully");
        check(world.Progress.Quests.Get(Mq24Campaign.QuestId).Phase == QuestPhase.Investigation, "MQ24 in Investigation phase");
        check(world.Progress.HasFlag(Mq24Campaign.MapMismatch), "MQ24 sets MapMismatch flag");

        check(ActTwoCampaignSystem.CanInspectRouteClue(world, Mq24RouteClue.StoryOrSign), "Can inspect lightning waypost clue");
        check(ActTwoCampaignSystem.InspectRouteClue(world, Mq24RouteClue.StoryOrSign), "Inspected lightning waypost clue");

        check(!ActTwoCampaignSystem.CanAttemptTrialPassage(world), "Cannot attempt passage before discovering all clues");
        check(ActTwoCampaignSystem.CanInspectRouteClue(world, Mq24RouteClue.TerrainObservation), "Can inspect terrain moss rock clue");
        check(ActTwoCampaignSystem.InspectRouteClue(world, Mq24RouteClue.TerrainObservation), "Inspected terrain moss rock clue");

        check(ActTwoCampaignSystem.CanAttemptTrialPassage(world), "Can attempt trial passage with both clues");
        check(ActTwoCampaignSystem.AttemptTrialPassage(world, true), "Trial passage through Bór Perunowy succeeded");
        check(world.Progress.HasFlag(Mq24Campaign.RouteRelation), "RouteRelation flag is set");
        check(world.Progress.HasFlag(Mq24Campaign.Complete), "MQ24_COMPLETE flag is set");
        check(world.Progress.Quests.Get(Mq25Campaign.QuestId).Phase == QuestPhase.Offered, "MQ25 is offered next");

        // --- MQ25: WIELKA SYNTEZA SZLAKÓW SŁOWIAŃSKICH (50% KAMPANII) ---
        check(ActTwoCampaignSystem.CanBeginMq25(world), "MQ25 can be started after MQ20-MQ24 all complete");
        check(ActTwoCampaignSystem.BeginMq25(world), "MQ25 begins successfully");
        check(world.Progress.Quests.Get(Mq25Campaign.QuestId).Phase == QuestPhase.Investigation, "MQ25 in Investigation phase");

        check(ActTwoCampaignSystem.CanValidateCriticalPackages(world), "Can validate critical regional data packages");
        check(ActTwoCampaignSystem.ValidateCriticalPackages(world), "Validated critical regional data packages");

        check(ActTwoCampaignSystem.CanRecordSharedConnection(world, Mq25SharedConnection.CrossRegionalRoute), "Can connect cross-regional route");
        check(ActTwoCampaignSystem.RecordSharedConnection(world, Mq25SharedConnection.CrossRegionalRoute), "Recorded cross-regional route connection");

        check(!ActTwoCampaignSystem.CanConcludeSynthesis(world), "Cannot conclude synthesis before all connections are made");
        check(ActTwoCampaignSystem.CanRecordSharedConnection(world, Mq25SharedConnection.AnchorMaterialDistribution), "Can connect anchor material distribution");
        check(ActTwoCampaignSystem.RecordSharedConnection(world, Mq25SharedConnection.AnchorMaterialDistribution), "Recorded anchor material distribution");

        check(ActTwoCampaignSystem.CanConcludeSynthesis(world), "Can conclude grand synthesis");
        check(ActTwoCampaignSystem.ConcludeSynthesis(world), "Concluded grand synthesis and completed MQ25 finale");

        // Verify Act II Finale Milestones & Rewards
        check(world.Progress.HasFlag(Mq25Campaign.Complete), "MQ25_COMPLETE flag is set");
        check(world.Progress.HasFlag(ActTwoCampaignSystem.MilestoneActTwo50Percent), "50% campaign milestone flag (MILESTONE_ACT_TWO_50_PERCENT) is set");
        check(world.Progress.HasFlag(ActTwoCampaignSystem.TitleMasterOfTrailsFlag), "Title 'TITLE_POWIERNIK_SZLAKOW' is granted to the player");
        check(world.Progress.Inventory.Count("sword-slavic-spatha") >= 1, "Legendary Slavic Spatha sword awarded");
        check(world.Progress.Inventory.Count("iron-chunk") >= 10, "Rewarded with rich iron chunks");
        check(world.Progress.Quests.Get(Mq25Campaign.NextQuestId).Phase == QuestPhase.Offered, "Act III (MQ30) is unlocked in Offered phase");
    }

    private static void TestEnvironmentInteractionsIntegration(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // 1. Spring interaction in village
        var well = SlavicSpringSystem.Springs.First(s => s.Id == "well-zarnowiec");
        world.SetPlayerPosition(well.Position);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current != null, "Interaction target appears near village well");
        check(world.EnvironmentInteractions.Current!.Id == "spring.well-zarnowiec", "Target ID matches spring");
        check(world.EnvironmentInteractions.TryInteract(world), "Drinking from well via environment interaction succeeds");
        check(world.Springs.HasBlessedWater, "Blessed water activated from interaction");

        // 2. Setup MQ23 in Gród Borzywoja
        world.Progress.SetFlag(Mq13Campaign.Complete);
        world.Progress.SetFlag(Mq13Campaign.NetworkHypothesis);
        world.Progress.SetFlag(Mq20Campaign.Complete);
        world.Progress.SetFlag(Mq20Campaign.RemoteNodeEvidence);
        world.Progress.SetFlag(Mq21Campaign.Complete);
        world.Progress.SetFlag(Mq22Campaign.Complete);
        world.Progress.SetFlag(Mq22Campaign.AnchorMaterial);
        world.Progress.Quests.Get(Mq23Campaign.QuestId).SetPhase(QuestPhase.Offered);

        world.SetPlayerPosition(ActTwoCampaignSystem.MineCouncilTableSpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act2.mq23.begin", "MQ23 begin prompt appears at council table");
        check(world.EnvironmentInteractions.TryInteract(world), "Executing MQ23 begin interaction succeeds");

        // Step through MQ23 impacts
        world.SetPlayerPosition(ActTwoCampaignSystem.MineImpactExtractionSpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act2.mq23.extraction", "Extraction review prompt appears at extraction spot");
        check(world.EnvironmentInteractions.TryInteract(world), "Executing extraction review succeeds");

        world.SetPlayerPosition(ActTwoCampaignSystem.MineImpactSafetySpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act2.mq23.safety", "Safety review prompt appears at safety spot");
        check(world.EnvironmentInteractions.TryInteract(world), "Executing safety review succeeds");

        world.SetPlayerPosition(ActTwoCampaignSystem.MineImpactStabilizationSpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act2.mq23.stabilization", "Stabilization prompt appears at stabilization spot");
        check(world.EnvironmentInteractions.TryInteract(world), "Executing stabilization review succeeds");

        world.SetPlayerPosition(ActTwoCampaignSystem.MineCouncilTableSpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act2.mq23.resolve", "Council resolution prompt appears at council table");
        check(world.EnvironmentInteractions.TryInteract(world), "Executing council resolution succeeds");
        check(world.Progress.HasFlag(Mq23Campaign.Complete), "MQ23 completed via world interactions");
    }

    private static void TestSaveLoadDurability(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // Drink from spring
        var well = SlavicSpringSystem.Springs.First(s => s.Id == "well-zarnowiec");
        world.SetPlayerPosition(well.Position);
        world.Springs.TryDrink(world, well.Id);

        // Complete Act II finale flags
        world.Progress.SetFlag(Mq25Campaign.Complete);
        world.Progress.SetFlag(ActTwoCampaignSystem.MilestoneActTwo50Percent);
        world.Progress.SetFlag(ActTwoCampaignSystem.TitleMasterOfTrailsFlag);
        world.Progress.Inventory.Add("sword-slavic-spatha", 1);

        // Open and modify stronghold armory chest
        var hallChestPos = LootContainerRuntime.WorldContainers["stronghold-chest-hall"].Position;
        world.SetPlayerPosition(hallChestPos);
        world.Loot.TryOpenNearest(world);
        world.Loot.TransferSelected(world, wholeStack: true);
        world.Loot.Close();

        // Capture save snapshot
        var snapshot = SaveGameService.Capture(world);
        check(snapshot != null, "Save snapshot captured successfully");

        // Restore into new world
        var restored = new WorldState();
        restored.Initialize();
        SaveGameService.Restore(restored, snapshot!);

        check(restored.Progress.HasFlag(Mq25Campaign.Complete), "MQ25_COMPLETE survives save/restore");
        check(restored.Progress.HasFlag(ActTwoCampaignSystem.MilestoneActTwo50Percent), "50% milestone survives save/restore");
        check(restored.Progress.HasFlag(ActTwoCampaignSystem.TitleMasterOfTrailsFlag), "Title survives save/restore");
        check(restored.Progress.Inventory.Count("sword-slavic-spatha") >= 1, "Awarded sword survives save/restore");

        // Verify container state persistence
        restored.SetPlayerPosition(hallChestPos);
        check(restored.Loot.TryOpenNearest(restored), "Can open chest in restored world");
        restored.Loot.Close();
    }
}
