using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class ActOneCampaignMilestoneRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Initialize();

        // 1. Initial gating: cannot take Act 1 lead before MQ00 is turned in
        check(!ActOneCampaignSystem.CanTakeLead(world), "Act 1 lead is blocked before MQ00 completion");

        // Simulate MQ00 turn in
        world.Progress.SetFlag(VerticalSliceQuestInteractions.TurnedInFlag);
        world.Progress.Quests.Get(MainQuestCampaign.Mq00Id).SetPhase(QuestPhase.TurnedIn);
        check(ActOneCampaignSystem.CanTakeLead(world), "Act 1 lead opens immediately after MQ00 turn-in");

        // 2. Stage 1: Take lead in Żarnowiec
        world.SetPlayerPosition(ActOneCampaignSystem.ZarnowiecLeadSpot);
        world.EnvironmentInteractions.Update(world);
        var targetLead = world.EnvironmentInteractions.Current;
        check(targetLead is not null && targetLead.Id == "act1.mq01.lead", "Lead interaction prompt surfaces in Żarnowiec");

        var leadInteracted = world.EnvironmentInteractions.TryInteract(world);
        check(leadInteracted, "Interacting with lead prompt succeeds");
        check(world.Progress.HasFlag(MainQuestCampaign.Mq01NadborzeLead), "MQ01 Nadborze lead flag is set");
        check(world.Progress.Inventory.Contains(ActOneCampaignSystem.RecommendLetterItemId), "Player receives letter of recommendation");
        check(world.Progress.Quests.Get(MainQuestCampaign.Mq01Id).Phase == QuestPhase.Active, "MQ01 becomes active in Quest Journal");

        // 3. Arrive at Gród Borzywoj gate
        check(ActOneCampaignSystem.CanEnterStronghold(world), "Stronghold gate entry is available with letter");
        world.SetPlayerPosition(ActOneCampaignSystem.StrongholdGateSpot);
        world.EnvironmentInteractions.Update(world);
        var targetGate = world.EnvironmentInteractions.Current;
        check(targetGate is not null && targetGate.Id == "act1.mq01.gate", "Stronghold gate prompt surfaces at Gród Borzywoj");

        var gateInteracted = world.EnvironmentInteractions.TryInteract(world);
        check(gateInteracted, "Guard Radomir admits player into stronghold");
        check(world.Progress.HasFlag(MainQuestCampaign.Mq01Complete), "MQ01 is marked complete");
        check(world.Progress.Quests.Get(MainQuestCampaign.Mq10Id).Phase == QuestPhase.Offered, "MQ10 is offered upon gate entry");

        // 4. Stage 2: Kurhan archaeological & ritual investigation
        check(ActOneCampaignSystem.CanInspectKurhan(world), "Kurhan investigation is available for MQ10");
        world.SetPlayerPosition(ActOneCampaignSystem.KurhanSteleSpot);
        world.EnvironmentInteractions.Update(world);
        var targetKurhan = world.EnvironmentInteractions.Current;
        check(targetKurhan is not null && targetKurhan.Id == "act1.mq10.kurhan", "Kurhan stele prompt surfaces at burial mounds");

        var kurhanInteracted = world.EnvironmentInteractions.TryInteract(world);
        check(kurhanInteracted, "Inspecting ancient Kurhan stele succeeds");
        check(world.Progress.HasFlag(MainQuestCampaign.Mq10Complete), "MQ10 is marked complete");
        check(world.Progress.Quests.Get(MainQuestCampaign.Mq11Id).Phase == QuestPhase.Offered, "MQ11 is offered after Kurhan investigation");

        // 5. Stage 3: The Great Slavic Council at Kniaź Borzywoj's Court
        check(ActOneCampaignSystem.CanHoldCouncil(world), "Council is available for MQ11");
        world.SetPlayerPosition(ActOneCampaignSystem.StrongholdCouncilSpot);
        world.EnvironmentInteractions.Update(world);
        var targetCouncil = world.EnvironmentInteractions.Current;
        check(targetCouncil is not null && targetCouncil.Id == "act1.mq11.council", "Slavic Council prompt surfaces in Great Hall");

        var repBeforeCouncil = world.Progress.Reputation.Get(ReputationScope.Faction, "stronghold");
        var councilInteracted = world.EnvironmentInteractions.TryInteract(world);
        check(councilInteracted, "Holding Slavic Council succeeds");
        check(world.Progress.HasFlag(MainQuestCampaign.Mq11Complete), "MQ11 is marked complete");
        check(world.Progress.HasFlag(MainQuestCampaign.Mq11DecisionConditional), "Council stance persisted");
        check(world.Progress.Reputation.Get(ReputationScope.Faction, "stronghold") > repBeforeCouncil, "Faction reputation awarded from Council");
        check(world.Progress.Quests.Get(MainQuestCampaign.Mq12Id).Phase == QuestPhase.Offered, "MQ12 is offered after Council");

        // 6. Stage 4: Tracking anomaly in deep Bór Perunowy
        check(ActOneCampaignSystem.CanInvestigatePerunBór(world), "Perun Bór anomaly investigation is available for MQ12");
        world.SetPlayerPosition(ActOneCampaignSystem.PerunBoundaryStoneSpot);
        world.EnvironmentInteractions.Update(world);
        var targetPerun = world.EnvironmentInteractions.Current;
        check(targetPerun is not null && targetPerun.Id == "act1.mq12.perun", "Boundary stone prompt surfaces in Bór Perunowy");

        var perunInteracted = world.EnvironmentInteractions.TryInteract(world);
        check(perunInteracted, "Investigating boundary stone resolves MQ12");
        check(world.Progress.HasFlag(Mq12Campaign.Complete), "MQ12 is marked complete");
        check(world.Progress.Quests.Get(Mq13Campaign.QuestId).Phase == QuestPhase.Offered, "MQ13 is offered after Bór Perunowy node discovery");

        // 7. Stage 5: The Grand Synthesis at the Map Table (Act 1 Climax - 25% of game!)
        check(ActOneCampaignSystem.CanSynthesizeMapTable(world), "Map Table is ready for Act 1 synthesis");
        world.SetPlayerPosition(ActOneCampaignSystem.StrongholdMapTableSpot);
        world.EnvironmentInteractions.Update(world);
        var targetMapTable = world.EnvironmentInteractions.Current;
        check(targetMapTable is not null && targetMapTable.Id == "act1.mq13.map-table", "Map Table synthesis prompt surfaces in Stronghold Keep");
        check(targetMapTable!.Prompt.Contains("25% GRY", StringComparison.Ordinal), "Prompt highlights 25% game completion milestone");

        var synthesisInteracted = world.EnvironmentInteractions.TryInteract(world);
        check(synthesisInteracted, "Synthesizing Slavic territorial map succeeds");
        check(world.Progress.HasFlag(Mq13Campaign.Complete), "MQ13 is marked complete");
        check(world.Progress.HasFlag(ActOneCampaignSystem.MilestoneAct1Flag), "25% game milestone flag written");
        check(world.Progress.HasFlag(ActOneCampaignSystem.TitleDefenderFlag), "Player receives title 'Obrońca Puszczy'");
        check(world.Progress.Inventory.Contains(ActOneCampaignSystem.PrinceRingItemId), "Player receives Ring of Kniaź Borzywoj");

        // 8. Act II unlock!
        var mq20 = world.Progress.Quests.Get(Mq13Campaign.NextQuestId);
        check(mq20.Phase == QuestPhase.Offered, "MQ20 (Act II) is officially unlocked and offered in Quest Journal");

        // 9. Durability across Save/Load
        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        restored.Initialize();
        SaveGameService.Restore(restored, json);

        check(restored.Progress.HasFlag(ActOneCampaignSystem.MilestoneAct1Flag), "25% milestone flag survives save/load");
        check(restored.Progress.HasFlag(ActOneCampaignSystem.TitleDefenderFlag), "Title 'Obrońca Puszczy' survives save/load");
        check(restored.Progress.Inventory.Contains(ActOneCampaignSystem.PrinceRingItemId), "Prince's ring survives save/load");
        check(restored.Progress.Quests.Get(Mq13Campaign.NextQuestId).Phase == QuestPhase.Offered, "Act II unlock survives save/load");
    }
}
