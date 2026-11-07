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

        var missingPrerequisite = NewWorld(includeAllActTwo: false, includeAllEvidence: true);
        Check(!new Mq25Campaign(missingPrerequisite.Progress).Begin(), "MQ25 requires all Act II quests completed");

        var missingEvidence = NewWorld(includeAllActTwo: true, includeAllEvidence: false);
        var mq25MissingEvidence = new Mq25Campaign(missingEvidence.Progress);
        Check(mq25MissingEvidence.Begin(), "MQ25 begins when Act II quests are complete");
        Check(!mq25MissingEvidence.ValidateCriticalPackages(), "MQ25 package validation requires all critical regional evidence");

        var world = NewWorld(includeAllActTwo: true, includeAllEvidence: true);
        var mq25 = new Mq25Campaign(world.Progress);
        Check(mq25.Begin(), "MQ25 begins with full Act II completion");
        Check(!mq25.RecordSharedConnection(Mq25SharedConnection.CrossRegionalRoute), "connections require validated packages first");
        Check(mq25.ValidateCriticalPackages(), "critical packages validate when regional evidence is present");
        Check(mq25.HasAllCriticalPackages, "all four regional data packages are marked");
        Check(!mq25.ConcludeSynthesis(), "synthesis requires shared connections");

        Check(mq25.RecordSharedConnection(Mq25SharedConnection.CrossRegionalRoute), "cross-regional route connection recorded");
        Check(!mq25.HasRequiredConnections, "single connection is insufficient for synthesis");

        var midSave = SaveGameService.Serialize(world);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, midSave);
        var resumed = new Mq25Campaign(restored.Progress);
        Check(resumed.HasAllCriticalPackages, "validated regional packages survive save/load");
        Check(resumed.RecordSharedConnection(Mq25SharedConnection.AnchorMaterialDistribution), "anchor material connection recorded after save/load");
        Check(resumed.HasRequiredConnections, "both shared connections are present");
        Check(resumed.ConcludeSynthesis(), "multicultural synthesis succeeds");
        Check(restored.Progress.HasFlag(Mq25Campaign.MulticulturalOrigin), "multicultural origin flag is durable");

        Check(resumed.CompleteQuest(), "MQ25 completes after synthesis");
        Check(restored.Progress.HasFlag(Mq25Campaign.Complete), "MQ25 completion flag is durable");
        Check(restored.Progress.Quests.Get(Mq25Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ25 completion unlocks MQ30");
        Check(!resumed.CompleteQuest(), "MQ25 completion is idempotent");

        var completedRestored = new WorldState(); completedRestored.Initialize();
        SaveGameService.Restore(completedRestored, SaveGameService.Serialize(restored));
        Check(completedRestored.Progress.HasFlag(Mq25Campaign.Complete), "MQ25 completion survives save/load");
        Check(completedRestored.Progress.HasFlag(Mq25Campaign.MulticulturalOrigin), "multicultural origin survives save/load");
        Check(completedRestored.Progress.Quests.Get(Mq25Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ30 handoff survives save/load");

        Console.WriteLine($"MQ25 campaign regression checks passed: {checks}");
        return checks;
    }

    private static WorldState NewWorld(bool includeAllActTwo, bool includeAllEvidence)
    {
        var world = new WorldState();
        world.Initialize();
        foreach (var flag in new[] { "MQ20_COMPLETE", "MQ21_COMPLETE", "MQ22_COMPLETE", "MQ23_COMPLETE" })
            world.Progress.SetFlag(flag);
        if (includeAllActTwo)
            world.Progress.SetFlag("MQ24_COMPLETE");

        world.Progress.SetFlag("NETWORK_HYPOTHESIS");
        world.Progress.SetFlag("MQ20_REMOTE_NODE_EVIDENCE");
        world.Progress.SetFlag("MQ22_ANCHOR_MATERIAL");
        if (includeAllEvidence)
            world.Progress.SetFlag("MQ24_ROUTE_RELATION");

        world.Progress.Quests.Get(Mq25Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
