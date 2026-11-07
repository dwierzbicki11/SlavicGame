using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Core;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Renderer;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class ActThreeCampaignFinaleRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        TestSvarogForgeShrineAndBlessing(check);
        TestThresholdWasteRegionAndMinimap(check);
        TestActThreeCampaignProgression(check);
        TestActThreeEnvironmentInteractions(check);
        TestSaveLoadDurability(check);
    }

    private static void TestSvarogForgeShrineAndBlessing(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // 1. Svarog Shrine spot and props
        check(SlavicShrineSystem.SvarogShrineSpot.X == -295f && SlavicShrineSystem.SvarogShrineSpot.Z == 218f,
            "Svarog's Forge Shrine is placed at the Great Smithy of Gród Borzywoja");
        check(RegionalPropLayout.Placements.Any(p => p.Id == "svarog-forge-anvil"),
            "Svarog forge anvil prop is placed in regional layout");
        check(RegionalPropLayout.Placements.Any(p => p.Id == "svarog-forge-firepit"),
            "Svarog forge firepit prop is placed in regional layout");
        check(MinimapRenderer.Pois.Any(p => p.Name == "KUŹNIA SWAROGA"),
            "Minimap displays Svarog's Forge landmark POI");

        // 2. Making offering to Svarog
        world.Progress.Inventory.Add("iron-chunk", 2);
        world.Player.TrySpendStamina(40f);
        var initialFavor = world.Progress.DivineRelationships.Get("Svarog").Favor;
        var offered = SlavicShrineSystem.MakeOfferingToSvarog(world, out var message);

        check(offered, "Offering to Svarog succeeds");
        check(message.Contains("SWAROG PRZYJAL OBIATE"), "Svarog acceptance message returned");
        check(world.Progress.HasFlag(SlavicShrineSystem.SvarogBlessingFlag), "Svarog blessing flag is granted");
        check(world.Progress.DivineRelationships.Get("Svarog").Favor > initialFavor, "Svarog divine favor increases");
        check(world.Player.Stamina == world.Player.MaxStamina, "Offering to Svarog restores player stamina");
        check(world.Progress.Inventory.Count("iron-chunk") == 1, "Offering consumes 1 iron chunk");

        // 3. Melee combat weapon and blessing scaling
        var melee = new PlayerMeleeCombat();
        var enemy = world.Enemies[0];
        world.SetPlayerPosition(enemy.Position - Vector3.UnitX * 1.0f);
        var enemyHpBefore = enemy.Health;

        // Player attacks with Slavic Spatha (+25) and Svarog blessing (+12) -> effective damage = 20 + 25 + 12 = 57
        world.Progress.Inventory.Add("sword-slavic-spatha", 1);
        melee.TryStart(world);
        // Advance past windup (0.18s) to deal damage
        melee.Update(world, Vector3.UnitX, 0.25);
        check(enemy.Health < enemyHpBefore - 50f, "Slavic Spatha and Svarog blessing apply full authentic damage boost");
    }

    private static void TestThresholdWasteRegionAndMinimap(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // 1. Region R6
        var thresholdRegion = world.Regions.FirstOrDefault(r => r.Id == "threshold-waste");
        check(thresholdRegion is not null, "Region 'threshold-waste' (Pustkowie Pierwszego Progu) is registered");
        check(thresholdRegion?.Name == "Pustkowie Pierwszego Progu", "Region has authentic Polish Slavic name");
        check(thresholdRegion?.CenterX == -460f && thresholdRegion?.CenterZ == 400f, "Region is situated in north-western anomaly zone");

        // 2. Minimap POIs
        check(MinimapRenderer.Pois.Any(p => p.Name == "PUSTKOWIE PROGU"), "Minimap POI for Pustkowie Progu exists");
        check(MinimapRenderer.Pois.Any(p => p.Name == "ARCHIWUM DAWNYCH PROGÓW"), "Minimap POI for Archiwum Dawnych Progów exists");

        // 3. Regional ruin props
        check(RegionalPropLayout.Placements.Any(p => p.Id == "threshold-waste-ruin-stele"), "Threshold waste ruin stele prop exists");
        check(RegionalPropLayout.Placements.Any(p => p.Id == "threshold-archive-entrance"), "Archive ruin entrance prop exists");
        check(RegionalPropLayout.Placements.Any(p => p.Id == "threshold-archive-records"), "Archive records stone altar prop exists");
        check(RegionalPropLayout.Placements.Any(p => p.Id == "threshold-wszebor-camp"), "Wszebor retreat camp prop exists");
    }

    private static void TestActThreeCampaignProgression(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // Setup prerequisite: Act II Finale completed
        world.Progress.SetFlag("MQ25_COMPLETE");
        world.Progress.SetFlag("NETWORK_MULTICULTURAL_ORIGIN");
        world.Progress.Quests.Get(Mq30Campaign.QuestId).SetPhase(QuestPhase.Offered);

        // --- MQ30: Pustkowie Pierwszego Progu ---
        check(ActThreeCampaignSystem.CanBeginMq30(world), "MQ30 is ready to begin after Act II finale");
        check(ActThreeCampaignSystem.BeginMq30(world), "BeginMq30 transitions quest to Investigation");
        check(world.Progress.HasFlag(Mq30Campaign.RegionEntered), "Entering R6 threshold waste is recorded");

        check(ActThreeCampaignSystem.CanRecordArchiveTrace(world, Mq30ArchiveTrace.StructuralRecord), "Can record structural archive trace");
        check(ActThreeCampaignSystem.RecordArchiveTrace(world, Mq30ArchiveTrace.StructuralRecord), "Structural archive trace recorded");

        check(ActThreeCampaignSystem.CanRecordArchiveTrace(world, Mq30ArchiveTrace.FieldRecord), "Can record field anomaly trace");
        check(ActThreeCampaignSystem.RecordArchiveTrace(world, Mq30ArchiveTrace.FieldRecord), "Field anomaly trace recorded");
        check(world.Progress.HasFlag(Mq30Campaign.ArchivePresenceConfirmed), "Archive presence confirmed after both traces");

        check(ActThreeCampaignSystem.CanEstablishSafeRoute(world), "Can establish safe route to archive");
        check(ActThreeCampaignSystem.EstablishSafeRoute(world), "Safe route established and MQ30 completed");
        check(world.Progress.HasFlag(Mq30Campaign.Complete), "MQ30_COMPLETE flag is set");
        check(world.Progress.Quests.Get(Mq31Campaign.QuestId).Phase == QuestPhase.Offered, "MQ31 is now Offered");

        // --- MQ31: Archiwum Popiołu ---
        check(ActThreeCampaignSystem.CanBeginMq31(world), "Can begin MQ31");
        check(ActThreeCampaignSystem.BeginMq31(world), "BeginMq31 transitions quest to Investigation");

        check(ActThreeCampaignSystem.CanRecordArchiveEvidence(world, Mq31ArchiveFact.ParentsArchiveRole), "Can inspect parents archive role");
        check(ActThreeCampaignSystem.RecordArchiveEvidence(world, Mq31ArchiveFact.ParentsArchiveRole), "Parents archive role recorded");

        check(ActThreeCampaignSystem.CanRecordArchiveEvidence(world, Mq31ArchiveFact.WszeborArchiveLink), "Can inspect Wszebor archive link");
        check(ActThreeCampaignSystem.RecordArchiveEvidence(world, Mq31ArchiveFact.WszeborArchiveLink), "Wszebor archive link recorded");

        check(ActThreeCampaignSystem.CanRecordArchiveEvidence(world, Mq31ArchiveFact.ClosedThresholdNightReference), "Can inspect closed threshold night chronicle");
        check(ActThreeCampaignSystem.RecordArchiveEvidence(world, Mq31ArchiveFact.ClosedThresholdNightReference), "Closed threshold night chronicle recorded");

        check(ActThreeCampaignSystem.CanConcludeMq31(world), "All critical facts known, can conclude MQ31");
        check(ActThreeCampaignSystem.ConcludeMq31(world), "MQ31 completed successfully");
        check(world.Progress.HasFlag(Mq31Campaign.Complete), "MQ31_COMPLETE flag is set");
        check(world.Progress.Quests.Get(Mq32Campaign.QuestId).Phase == QuestPhase.Offered, "MQ32 is now Offered");

        // --- MQ32: Noc Zamkniętego Progu ---
        check(ActThreeCampaignSystem.CanBeginMq32(world), "Can begin MQ32");
        check(ActThreeCampaignSystem.BeginMq32(world), "BeginMq32 transitions quest to Investigation");

        // 3 evidence categories with independent evidence and contradictions
        check(ActThreeCampaignSystem.CanRecordCrisisEvidence(world, Mq32EvidenceCategory.EventEcho), "Can record EventEcho");
        check(ActThreeCampaignSystem.RecordCrisisEvidence(world, Mq32EvidenceCategory.EventEcho, fromInterestedFaction: false), "EventEcho recorded as independent evidence");

        check(ActThreeCampaignSystem.CanRecordCrisisEvidence(world, Mq32EvidenceCategory.ClosureGuardRecord), "Can record ClosureGuardRecord");
        check(ActThreeCampaignSystem.RecordCrisisEvidence(world, Mq32EvidenceCategory.ClosureGuardRecord, fromInterestedFaction: true), "ClosureGuardRecord recorded as faction evidence");

        check(ActThreeCampaignSystem.CanRecordCrisisEvidence(world, Mq32EvidenceCategory.AlternativeFieldRecord), "Can record AlternativeFieldRecord");
        check(ActThreeCampaignSystem.RecordCrisisEvidence(world, Mq32EvidenceCategory.AlternativeFieldRecord, fromInterestedFaction: false), "AlternativeFieldRecord recorded as independent evidence");

        check(ActThreeCampaignSystem.CanMarkContradiction(world, Mq32EvidenceCategory.ClosureGuardRecord, Mq32EvidenceCategory.AlternativeFieldRecord), "Can mark contradiction between guard report and field record");
        check(ActThreeCampaignSystem.MarkContradiction(world, Mq32EvidenceCategory.ClosureGuardRecord, Mq32EvidenceCategory.AlternativeFieldRecord), "Contradiction marked");
        check(world.Progress.HasFlag(Mq32Campaign.CoreReconstructed), "Core crisis reconstruction achieved");

        check(ActThreeCampaignSystem.CanConcludeMq32(world), "Can conclude MQ32");
        check(ActThreeCampaignSystem.ConcludeMq32(world), "MQ32 completed successfully");
        check(world.Progress.HasFlag(Mq32Campaign.Complete), "MQ32_COMPLETE flag is set");
        check(world.Progress.Quests.Get(Mq33Campaign.QuestId).Phase == QuestPhase.Offered, "MQ33 is now Offered");

        // --- MQ33: Rozwierający ---
        check(ActThreeCampaignSystem.CanBeginMq33(world), "Can begin MQ33");
        check(ActThreeCampaignSystem.BeginMq33(world), "BeginMq33 transitions quest to Investigation");

        check(ActThreeCampaignSystem.CanRecordWszeborPosition(world), "Can assess Wszebor stance");
        check(ActThreeCampaignSystem.RecordWszeborPosition(world, Mq33WszeborStance.Guarded), "Wszebor guarded position recorded");

        check(ActThreeCampaignSystem.CanResolveWszeborConfrontation(world), "Can resolve confrontation with Wszebor");
        check(ActThreeCampaignSystem.ResolveWszeborConfrontation(world, Mq33ConfrontationOutcome.Conversation, Mq33WszeborStance.Cooperative, directDataAvailable: true), "Confrontation resolved cooperatively with direct data package");
        check(world.Progress.HasFlag(Mq33Campaign.Complete), "MQ33_COMPLETE flag is set");
        check(world.Progress.HasFlag(Mq33Campaign.DataPackageAvailable), "Data package for MQ34 secured");
        check(world.Progress.Quests.Get(Mq34Campaign.QuestId).Phase == QuestPhase.Offered, "MQ34 is now Offered");

        // --- MQ34: Trzy Projekty (Act III Finale - 75% Milestone) ---
        check(ActThreeCampaignSystem.CanBeginMq34(world), "Can begin MQ34");
        check(ActThreeCampaignSystem.BeginMq34(world), "BeginMq34 transitions quest to Investigation");

        check(ActThreeCampaignSystem.CanPresentModel(world, Mq34CrisisModel.HardClosure), "Can present Hard Closure model");
        check(ActThreeCampaignSystem.PresentModel(world, Mq34CrisisModel.HardClosure, optionalDetailAvailable: true), "Hard Closure model presented");

        check(ActThreeCampaignSystem.CanPresentModel(world, Mq34CrisisModel.DistributedRebuild), "Can present Distributed Rebuild model");
        check(ActThreeCampaignSystem.PresentModel(world, Mq34CrisisModel.DistributedRebuild, optionalDetailAvailable: true), "Distributed Rebuild model presented");

        check(ActThreeCampaignSystem.CanPresentModel(world, Mq34CrisisModel.Opening), "Can present Opening model");
        check(ActThreeCampaignSystem.PresentModel(world, Mq34CrisisModel.Opening, optionalDetailAvailable: true), "Opening model presented");

        check(ActThreeCampaignSystem.CanConcludeActThree(world), "All three models presented, ready to conclude Act III finale");
        check(ActThreeCampaignSystem.ConcludeActThree(world), "Act III finale concluded successfully");

        // Validate Milestone, Title, and Rewards
        check(world.Progress.HasFlag(ActThreeCampaignSystem.MilestoneActThree75Percent), "MILESTONE_ACT_THREE_75_PERCENT reached (75% campaign milestone)");
        check(world.Progress.HasFlag(ActThreeCampaignSystem.TitleArchiveMasterFlag), "TITLE_ARCHIWISTA_PROGOW earned");
        check(world.Progress.Inventory.Count("armor-reinforced-leather") == 1, "Awarded heroic reinforced leather armor");
        check(world.Progress.Inventory.Count("arrow-iron") >= 50, "Awarded 50+ iron arrows");
        check(world.Progress.Inventory.Count("old-coin") >= 150, "Awarded 150+ old coins");
        check(world.Progress.Profile.Money >= 150, "Profile currency credited");

        // Act IV Unlock
        check(world.Progress.HasFlag(Mq34Campaign.Act4NaviaLeadAvailable), "ACT4_NAVIA_LEAD_AVAILABLE is set");
        check(world.Progress.Quests.Get(Mq40Campaign.QuestId).Phase == QuestPhase.Offered, "Act IV MQ40 (Ku Nawii) is Offered");
        var mq40 = new Mq40Campaign(world.Progress);
        check(mq40.Begin(), "Act IV campaign smoothly transitions to Active/Investigation with Act III completed");
    }

    private static void TestActThreeEnvironmentInteractions(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // 1. Svarog shrine interaction
        world.SetPlayerPosition(SlavicShrineSystem.SvarogShrineSpot);
        world.Progress.Inventory.Add("iron-chunk", 1);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "shrine.svarog", "Nearest interaction is Svarog forge shrine");
        check(world.EnvironmentInteractions.TryInteract(world), "Interacting with Svarog shrine executes offering");
        check(world.EnvironmentInteractions.Message.Contains("SWAROG PRZYJAL OBIATE"), "Interaction HUD reports Svarog accepted offering");

        // 2. Act III MQ30 interactions
        world.Progress.SetFlag("MQ25_COMPLETE");
        world.Progress.SetFlag("NETWORK_MULTICULTURAL_ORIGIN");
        world.Progress.Quests.Get(Mq30Campaign.QuestId).SetPhase(QuestPhase.Offered);

        world.SetPlayerPosition(ActThreeCampaignSystem.ThresholdWasteEntranceSpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act3.mq30.begin", "Interaction target is act3.mq30.begin");
        check(world.EnvironmentInteractions.TryInteract(world), "Interacting begins MQ30");
        check(world.Progress.Quests.Get(Mq30Campaign.QuestId).Phase == QuestPhase.Investigation, "MQ30 is in Investigation");

        // Now structural trace appears
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act3.mq30.trace.structural", "Interaction target is structural trace");
        check(world.EnvironmentInteractions.TryInteract(world), "Interacting records structural trace");

        // Move to archive entrance for field trace
        world.SetPlayerPosition(ActThreeCampaignSystem.ArchiveRuinsEntranceSpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act3.mq30.trace.field", "Interaction target is field trace");
        check(world.EnvironmentInteractions.TryInteract(world), "Interacting records field trace");

        // Now safe route appears
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act3.mq30.route", "Interaction target is route establishment");
        check(world.EnvironmentInteractions.TryInteract(world), "Interacting establishes safe route and completes MQ30");
        check(world.Progress.HasFlag(Mq30Campaign.Complete), "MQ30 is completed via EnvironmentInteractionSystem");

        // 3. MQ31 begins at archive record chamber
        world.SetPlayerPosition(ActThreeCampaignSystem.ArchiveRecordChamberSpot);
        world.EnvironmentInteractions.Update(world);
        check(world.EnvironmentInteractions.Current?.Id == "act3.mq31.begin", "Interaction target is act3.mq31.begin");
        check(world.EnvironmentInteractions.TryInteract(world), "Interacting begins MQ31");
    }

    private static void TestSaveLoadDurability(Action<bool, string> check)
    {
        var world = new WorldState();
        world.Initialize();

        // Fast-forward through Act III to finale
        world.Progress.SetFlag("MQ25_COMPLETE");
        world.Progress.SetFlag("NETWORK_MULTICULTURAL_ORIGIN");
        world.Progress.Quests.Get(Mq30Campaign.QuestId).SetPhase(QuestPhase.Offered);

        ActThreeCampaignSystem.BeginMq30(world);
        ActThreeCampaignSystem.RecordArchiveTrace(world, Mq30ArchiveTrace.StructuralRecord);
        ActThreeCampaignSystem.RecordArchiveTrace(world, Mq30ArchiveTrace.FieldRecord);
        ActThreeCampaignSystem.EstablishSafeRoute(world);

        ActThreeCampaignSystem.BeginMq31(world);
        ActThreeCampaignSystem.RecordArchiveEvidence(world, Mq31ArchiveFact.ParentsArchiveRole);
        ActThreeCampaignSystem.RecordArchiveEvidence(world, Mq31ArchiveFact.WszeborArchiveLink);
        ActThreeCampaignSystem.RecordArchiveEvidence(world, Mq31ArchiveFact.ClosedThresholdNightReference);
        ActThreeCampaignSystem.ConcludeMq31(world);

        ActThreeCampaignSystem.BeginMq32(world);
        ActThreeCampaignSystem.RecordCrisisEvidence(world, Mq32EvidenceCategory.EventEcho, false);
        ActThreeCampaignSystem.RecordCrisisEvidence(world, Mq32EvidenceCategory.ClosureGuardRecord, true);
        ActThreeCampaignSystem.RecordCrisisEvidence(world, Mq32EvidenceCategory.AlternativeFieldRecord, false);
        ActThreeCampaignSystem.MarkContradiction(world, Mq32EvidenceCategory.ClosureGuardRecord, Mq32EvidenceCategory.AlternativeFieldRecord);
        ActThreeCampaignSystem.ConcludeMq32(world);

        ActThreeCampaignSystem.BeginMq33(world);
        ActThreeCampaignSystem.RecordWszeborPosition(world, Mq33WszeborStance.Guarded);
        ActThreeCampaignSystem.ResolveWszeborConfrontation(world);

        ActThreeCampaignSystem.BeginMq34(world);
        ActThreeCampaignSystem.PresentModel(world, Mq34CrisisModel.HardClosure);
        ActThreeCampaignSystem.PresentModel(world, Mq34CrisisModel.DistributedRebuild);
        ActThreeCampaignSystem.PresentModel(world, Mq34CrisisModel.Opening);
        ActThreeCampaignSystem.ConcludeActThree(world);

        SlavicShrineSystem.MakeOfferingToSvarog(world, out _);

        // Serialize
        var snapshot = SaveGameService.Capture(world);
        check(snapshot is not null, "WorldState successfully captured at 75% Act III Milestone");

        // Deserialize
        var restoredWorld = new WorldState();
        restoredWorld.Initialize();
        SaveGameService.Restore(restoredWorld, snapshot!);

        check(restoredWorld.Progress.HasFlag(ActThreeCampaignSystem.MilestoneActThree75Percent), "Restored world preserves MILESTONE_ACT_THREE_75_PERCENT");
        check(restoredWorld.Progress.HasFlag(ActThreeCampaignSystem.TitleArchiveMasterFlag), "Restored world preserves TITLE_ARCHIWISTA_PROGOW");
        check(restoredWorld.Progress.HasFlag(SlavicShrineSystem.SvarogBlessingFlag), "Restored world preserves Svarog's blessing");
        check(restoredWorld.Progress.Inventory.Count("armor-reinforced-leather") == 1, "Restored world preserves reinforced leather armor");
        check(restoredWorld.Progress.Quests.Get(Mq34Campaign.QuestId).Phase == QuestPhase.Resolved, "MQ34 is Resolved in restored world");
        check(restoredWorld.Progress.Quests.Get(Mq40Campaign.QuestId).Phase == QuestPhase.Offered, "MQ40 is Offered in restored world");
    }
}
