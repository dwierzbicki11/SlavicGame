using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq30CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = NewWorld(ready: false);
        Check(!new Mq30Campaign(gated.Progress).Begin(), "MQ30 is gated by MQ25 synthesis");

        var world = NewWorld(ready: true);
        var mq30 = new Mq30Campaign(world.Progress);
        Check(mq30.Begin(), "MQ30 begins after MQ25 synthesis");
        Check(world.Progress.HasFlag(Mq30Campaign.RegionEntered), "entering MQ30 records R6 entry");
        Check(mq30.RecordArchiveTrace(Mq30ArchiveTrace.FieldRecord), "field trace can be recorded first");
        Check(!mq30.EstablishSafeRoute(), "one trace cannot establish critical route");
        Check(mq30.RecordArchiveTrace(Mq30ArchiveTrace.StructuralRecord), "independent structural trace can be recovered");
        Check(world.Progress.HasFlag(Mq30Campaign.ArchivePresenceConfirmed), "two traces confirm archive presence");
        Check(mq30.EstablishSafeRoute(), "two independent traces establish safe route");
        Check(mq30.CompleteQuest(), "MQ30 completes with route known");
        Check(world.Progress.Quests.Get(Mq30Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ30 unlocks MQ31");
        Check(!mq30.CompleteQuest(), "MQ30 completion is idempotent");

        var persistent = NewWorld(ready: true);
        var campaign = new Mq30Campaign(persistent.Progress);
        campaign.Begin();
        campaign.RecordArchiveTrace(Mq30ArchiveTrace.StructuralRecord);
        var json = SaveGameService.Serialize(persistent);
        var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var resumed = new Mq30Campaign(restored.Progress);
        Check(resumed.Begin(), "MQ30 can resume after save/load");
        Check(resumed.RecordArchiveTrace(Mq30ArchiveTrace.FieldRecord), "alternate trace remains available after restore");
        Check(resumed.EstablishSafeRoute(), "restored MQ30 can establish route");
        Check(resumed.CompleteQuest(), "restored MQ30 completes");
        Check(restored.Progress.HasFlag(Mq30Campaign.ArchivePresenceConfirmed), "critical evidence survives restore path");
        Check(restored.Progress.HasFlag(Mq30Campaign.RouteKnown), "route survives restore path");
        return checks;
    }

    private static WorldState NewWorld(bool ready)
    {
        var world = new WorldState();
        world.Initialize();
        if (ready)
        {
            world.Progress.SetFlag("MQ25_COMPLETE");
            world.Progress.SetFlag("NETWORK_MULTICULTURAL_ORIGIN");
        }
        world.Progress.Quests.Get(Mq30Campaign.QuestId).SetPhase(QuestPhase.Offered);
        return world;
    }
}
