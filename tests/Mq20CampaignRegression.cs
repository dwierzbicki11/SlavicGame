using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;
internal static class Mq20CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();
    public static int Run()
    {
        var checks=0; void Check(bool condition,string name){if(!condition)throw new Exception(name);checks++;}
        foreach(var route in Enum.GetValues<Mq20MapAccessRoute>()){var world=NewWorld();var mq20=new Mq20Campaign(world.Progress);Check(mq20.Begin(),$"MQ20 begins for {route}");Check(world.Progress.HasFlag(Mq20Campaign.EstuaryBlocked),$"estuary crisis is recorded for {route}");Check(mq20.GainMapAccess(route),$"{route} grants map access");Check(mq20.HasMapAccess,$"{route} satisfies map-access fallback");Check(mq20.CompareRemoteNode(),$"{route} permits remote-node comparison");Check(world.Progress.HasFlag(Mq20Campaign.RemoteNodeEvidence),$"{route} writes durable remote-node evidence");}
        var gated=new WorldState();gated.Initialize();gated.Progress.Quests.Get(Mq20Campaign.QuestId).SetPhase(QuestPhase.Offered);Check(!new Mq20Campaign(gated.Progress).Begin(),"MQ20 is gated by MQ13 completion");
        var persistent=NewWorld();var campaign=new Mq20Campaign(persistent.Progress);campaign.Begin();campaign.GainMapAccess(Mq20MapAccessRoute.EnvironmentalEvidence);Check(campaign.CompareRemoteNode(),"environmental evidence path reaches map comparison");Check(!campaign.CompleteQuest(),"MQ20 cannot complete before estuary resolution or bypass");Check(campaign.SetEstuaryOutcome(Mq20EstuaryOutcome.Bypassed),"estuary can be bypassed fail-forward");
        var json=SaveGameService.Serialize(persistent);var restored=new WorldState();restored.Initialize();SaveGameService.Restore(restored,json);var restoredCampaign=new Mq20Campaign(restored.Progress);Check(restoredCampaign.HasMapAccess,"map access survives save/load");Check(restoredCampaign.HasEstuaryOutcome,"estuary outcome survives save/load");Check(restored.Progress.HasFlag(Mq20Campaign.RemoteNodeEvidence),"remote-node evidence survives save/load");Check(restoredCampaign.CompleteQuest(),"restored MQ20 completes");Check(restored.Progress.HasFlag(Mq20Campaign.Complete),"MQ20 completion flag written");Check(restored.Progress.Quests.Get(Mq20Campaign.NextQuestId).Phase==QuestPhase.Offered,"MQ20 completion unlocks MQ21");Check(!restoredCampaign.CompleteQuest(),"MQ20 completion is idempotent");
        var completedJson=SaveGameService.Serialize(restored);var completedRestored=new WorldState();completedRestored.Initialize();SaveGameService.Restore(completedRestored,completedJson);Check(completedRestored.Progress.Quests.Get(Mq20Campaign.NextQuestId).Phase==QuestPhase.Offered,"MQ21 handoff survives save/load");return checks;
    }
    private static WorldState NewWorld(){var world=new WorldState();world.Initialize();world.Progress.SetFlag("MQ13_COMPLETE");world.Progress.SetFlag("NETWORK_HYPOTHESIS");world.Progress.Quests.Get(Mq20Campaign.QuestId).SetPhase(QuestPhase.Offered);return world;}
}
