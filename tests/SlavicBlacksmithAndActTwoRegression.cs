using System.Numerics;
using SlavicGame.Engine.Combat;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class SlavicBlacksmithAndActTwoRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        var world = WorldGenerator.Generate();
        world.Time.SetTimeOfDay(12);
        world.Initialize();

        // 1. Blacksmith Forge Availability
        var smith = world.NpcWorld.Find("stronghold-smith") ?? throw new Exception("Stronghold smith missing");
        world.SetPlayerPosition(smith.Position);
        world.NpcWorld.Update(world);

        check(world.Blacksmith.CanOpenNearest(world), "Blacksmith forge is accessible near Stronghold smith Sambor");
        check(world.Blacksmith.TryOpenNearest(world), "Opening forge UI succeeds");
        check(world.Blacksmith.IsOpen, "Blacksmith UI is marked open");

        var lines = world.Blacksmith.BuildLines(world);
        check(lines.Count == 5, $"Blacksmith provides 5 Slavic martial recipes (found {lines.Count})");
        check(lines[0].RecipeId == "blacksmith.druzhina-sword", "First recipe is Druzhina Sword");
        check(!lines[0].CanCraft, "Cannot craft sword without iron and resin");

        // 2. Forging Druzhina Sword and War Axe
        world.Progress.Inventory.Add("iron-chunk", 10);
        world.Progress.Inventory.Add("forest-resin", 6);
        world.Progress.Inventory.Add("leather-strip", 4);
        world.Progress.Inventory.Add("plank-timber", 4);

        lines = world.Blacksmith.BuildLines(world);
        check(lines[0].CanCraft, "Can craft Druzhina Sword with acquired materials");

        var forgedSword = world.Blacksmith.Confirm(world);
        check(forgedSword, "Forging Druzhina Sword succeeded");
        check(world.Progress.Inventory.Contains("miecz-druzynnika"), "Player inventory contains 'miecz-druzynnika'");
        check(world.Progress.Inventory.Count("iron-chunk") == 8, "2x iron-chunk consumed");

        // Forge Topór Bojowy Peruna
        world.Blacksmith.MoveSelection(1);
        var forgedAxe = world.Blacksmith.Confirm(world);
        check(forgedAxe, "Forging Topor Bojowy Peruna succeeded");
        check(world.Progress.Inventory.Contains("topor-bojowy"), "Player inventory contains 'topor-bojowy'");

        // Forge Slavic Shield
        world.Blacksmith.MoveSelection(1);
        var forgedShield = world.Blacksmith.Confirm(world);
        check(forgedShield, "Forging Slavic Shield succeeded");
        check(world.Progress.Inventory.Contains("tarcza-slowianska"), "Player inventory contains 'tarcza-slowianska'");

        // Forge Chainmail
        world.Blacksmith.MoveSelection(1);
        var forgedMail = world.Blacksmith.Confirm(world);
        check(forgedMail, "Forging Slavic Chainmail succeeded");
        check(world.Progress.Inventory.Contains("kolczuga-slowianska"), "Player inventory contains 'kolczuga-slowianska'");

        world.Blacksmith.Close();
        check(!world.Blacksmith.IsOpen, "Blacksmith UI closed");

        // 3. Combat damage bonus verification with forged axe
        var enemy = world.Enemies.First();
        var enemyHpBefore = enemy.Health;
        world.SetPlayerPosition(enemy.Position - Vector3.UnitZ * 1.5f);
        world.Melee.TryStart(world);
        world.Melee.Update(world, Vector3.UnitZ, 0.5);

        // Standard light attack is 20 dmg, Topór Bojowy Peruna adds +18 dmg => 38 dmg total!
        var damageInflicted = enemyHpBefore - enemy.Health;
        check(damageInflicted >= 38f, $"Topor Bojowy Peruna inflicts boosted damage (+18): dealt {damageInflicted}");

        // 4. Act II Opener: MQ20 Blokada Ujścia i Przystań Rybacka
        // MQ20 requires Act I completion (MQ13_COMPLETE)
        check(!ActTwoCampaignSystem.CanBeginAct2(world), "Act 2 cannot begin before MQ13 completion");

        // Complete Act 1
        world.Progress.SetFlag(VerticalSliceQuestInteractions.TurnedInFlag);
        ActOneCampaignSystem.TakeLead(world);
        ActOneCampaignSystem.EnterStronghold(world);
        ActOneCampaignSystem.InspectKurhan(world);
        ActOneCampaignSystem.HoldCouncil(world);
        ActOneCampaignSystem.InvestigatePerunBór(world);
        ActOneCampaignSystem.SynthesizeMapTable(world);

        check(world.Progress.HasFlag(Mq13Campaign.Complete), "Act I (MQ13) is complete");
        check(world.Progress.HasFlag(ActOneCampaignSystem.MilestoneAct1Flag), "25% milestone flag confirmed");
        check(ActTwoCampaignSystem.CanBeginAct2(world), "Act 2 (MQ20) is now ready to begin");

        // Move to Estuary Barrier at Przystań Rybacka
        world.SetPlayerPosition(ActTwoCampaignSystem.EstuaryBarrierSpot);
        world.EnvironmentInteractions.Update(world);
        var targetMq20Begin = world.EnvironmentInteractions.Current;
        check(targetMq20Begin is not null && targetMq20Begin.Id == "act2.mq20.begin", "MQ20 begin prompt surfaces at estuary barrier");

        var begun = world.EnvironmentInteractions.TryInteract(world);
        check(begun, "Interacting with estuary barrier begins MQ20");
        check(world.Progress.HasFlag(Mq20Campaign.EstuaryBlocked), "MQ20 estuary blocked flag is durable");
        check(world.Progress.Quests.Get(Mq20Campaign.QuestId).Phase == QuestPhase.Investigation, "MQ20 enters investigation phase");

        // Negotiate with fishermen using 'Obrońca Puszczy' reputation
        check(ActTwoCampaignSystem.CanNegotiateEstuary(world), "Estuary negotiation is available");
        world.EnvironmentInteractions.Update(world);
        var targetNegotiate = world.EnvironmentInteractions.Current;
        check(targetNegotiate is not null && targetNegotiate.Id == "act2.mq20.negotiate", "Negotiate prompt surfaces");

        var negotiated = world.EnvironmentInteractions.TryInteract(world);
        check(negotiated, "Negotiating estuary passage succeeds");
        check(world.Progress.HasFlag("MQ20_MAP_ACCESS_REPUTATION"), "Map access granted via reputation route");

        // Inspect remote beacon overlooking the great river
        world.SetPlayerPosition(ActTwoCampaignSystem.RemoteBeaconSpot);
        world.EnvironmentInteractions.Update(world);
        var targetBeacon = world.EnvironmentInteractions.Current;
        check(targetBeacon is not null && targetBeacon.Id == "act2.mq20.beacon", "Remote beacon prompt surfaces on pier");

        var beaconInspected = world.EnvironmentInteractions.TryInteract(world);
        check(beaconInspected, "Inspecting remote beacon succeeds");
        check(world.Progress.HasFlag(Mq20Campaign.RemoteNodeEvidence), "Remote node evidence gathered");

        // Resolve estuary and complete MQ20
        world.SetPlayerPosition(ActTwoCampaignSystem.EstuaryBarrierSpot);
        world.EnvironmentInteractions.Update(world);
        var targetResolve = world.EnvironmentInteractions.Current;
        check(targetResolve is not null && targetResolve.Id == "act2.mq20.resolve", "Resolve estuary prompt surfaces");

        var resolved = world.EnvironmentInteractions.TryInteract(world);
        check(resolved, "Resolving estuary succeeds");
        check(world.Progress.HasFlag(Mq20Campaign.Complete), "MQ20 is marked complete");
        check(world.Progress.Quests.Get(Mq20Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ21 is unlocked and offered in Quest Journal!");

        // 5. Durability across Save/Load
        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        restored.Initialize();
        SaveGameService.Restore(restored, json);

        check(restored.Progress.HasFlag(Mq20Campaign.Complete), "MQ20 completion survives save/load");
        check(restored.Progress.Inventory.Contains("miecz-druzynnika"), "Forged sword survives save/load");
        check(restored.Progress.Inventory.Contains("topor-bojowy"), "Forged war axe survives save/load");
        check(restored.Progress.Inventory.Contains("tarcza-slowianska"), "Forged shield survives save/load");
        check(restored.Progress.Quests.Get(Mq20Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ21 offer survives save/load");
    }
}
