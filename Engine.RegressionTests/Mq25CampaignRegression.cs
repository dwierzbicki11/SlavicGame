using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq25CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = new WorldState();
        gated.Initialize();
        gated.Progress.Quests.Get(Mq25Campaign.QuestId).SetPhase(QuestPhase.Offered);
        Check(!new Mq25Campaign(gated.Progress).Begin(), "MQ25 is gated by completed Act II quests");

        var missingEvidence = NewWorld(includeCriticalEvidence: false);
        var missingCampaign = new Mq25Campaign(missingEvidence.Progress);
        Check(missingCampaign.Begin(), "MQ25 begins after MQ20-MQ24 completion");
        Check(!missingCampaign.ValidateCriticalPackages(), "MQ25 does not invent missing critical evidence");

        var world = NewWorld(includeCriticalEvidence: true);
        var mq25 = new Mq25Campaign(world.Progress);
        Check(mq25.Begin(), "MQ25 begins with completed Act II state");
        Check(mq25.ValidateCriticalPackages(), "four critical regional data packages validate");
        Check(mq25.HasAllCriticalPackages, "all four traditions are represented independently");
        Check(mq25.RecordSharedConnection(Mq25SharedConnection.AnchorMaterialDistribution), "first shared connection can be recorded");
        Check(mq25.RecordSharedConnection(Mq25SharedConnection.CrossRegionalRoute), "second shared connection can be recorded in either order");
        Check(mq25.HasRequiredConnections, "two shared connections are required");
        Check(mq25.ConcludeSynthesis(), "MQ25 synthesis reaches multicultural-origin conclusion");
        Check(world.Progress.HasFlag(Mq25Campaign.MulticulturalOrigin), "multicultural-origin evidence is recorded");
        Check(mq25.CompleteQuest(), "MQ25 completes after synthesis");
        Check(world.Progress.Quests.Get(Mq25Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ25 unlocks MQ30");
        Check(!mq25.CompleteQuest(), "MQ25 completion is idempotent");

        var persistent = NewWorld(includeCriticalEvidence: true);
        var campaign = new Mq25Campaign(persistent.Progress);
        campaign.Begin();
        campaign.ValidateCriticalPackages();
        campaign.RecordSharedConnection(Mq25SharedConnection.CrossRegionalRoute);
        var json = SaveGameService.Serialize(persistent);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var resumed = new Mq25Campaign(restored.Progress);
        Check(resumed.Begin(), "MQ25 archive can be reopened after save/load");
        Check(resumed.HasAllCriticalPackages, "normalized packages survive save/load");
        Check(resumed.RecordSharedConnection(Mq25SharedConnection.AnchorMaterialDistribution), "second connection can be added after save/load");
        Check(resumed.ConcludeSynthesis(), "restored archive can conclude synthesis");
        Check(resumed.CompleteQuest(), "restored MQ25 completes");

        var completedJson = SaveGameService.Serialize(restored);
        var completedRestored = new WorldState(); completedRestored.Initialize(); SaveGameService.Restore(completedRestored, completedJson);
        Check(completedRestored.Progress.HasFlag(Mq25Campaign.Complete), "MQ25 completion survives save/load");
        Check(completedRestored.Progress.HasFlag(Mq25Campaign.MulticulturalOrigin), "multicultural-origin conclusion survives save/load");
        Check(completedRestored.Progress.Quests.Get(Mq25Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ30 handoff survives save/load");
        return checks;
    }

    private static WorldState NewWorld(bool includeCriticalEvidence)
    {
        var world = new WorldState();
        world.Initialize();
        foreach (var quest in new[] { "MQ20", "MQ21", "MQ22", "MQ23", "MQ24" })
            world.Progress.SetFlag(quest + "_COMPLETE");
        if (includeCriticalEvidence)
        {
            world.Progress.SetFlag("NETWORK_HYPOTHESIS");
            world.Progress.SetFlag("MQ20_REMOTE_NODE_EVIDENCE");
            world.Progress.SetFlag("MQ22_ANCHOR_MATERIAL");
            world.Progress.SetFlag("MQ24_ROUTE_RELATION");
        }
        world.Progress.Quests.Get(Mq25Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
