using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq12CampaignRegression
{
    [ModuleInitializer]
    internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        GameProgress Ready()
        {
            var p = new GameProgress();
            p.SetFlag(MainQuestCampaign.Mq11Complete);
            p.Quests.Get(MainQuestCampaign.Mq12Id).SetPhase(QuestPhase.Offered);
            for (var i = 1; i <= 3; i++)
                p.Tracking.Register($"mq12-ref-{i}", TrackCategory.EnvironmentalAnomaly, new Vector3(i, 0, i), TrackFreshness.Recent, $"MQ12-E0{i}");
            return p;
        }

        var progress = Ready();
        var mq12 = new Mq12Campaign(progress, "mq12-route", ["mq12-ref-1", "mq12-ref-2", "mq12-ref-3"]);
        Check(mq12.BeginRouteAnomaly(), "MQ12 route anomaly starts after MQ11");
        Check(progress.HasFlag(Mq12Campaign.RouteAnomaly), "route anomaly flag is durable");
        Check(mq12.DiscoverReference("mq12-ref-2"), "references may be found out of order");
        Check(!mq12.ConfirmForestChange(true), "one reference cannot progress MQ12");
        Check(mq12.DiscoverReference("mq12-ref-1"), "second reference is accepted");
        Check(mq12.HasRequiredReferences, "two of three references satisfy progression threshold");
        Check(mq12.ConfirmForestChange(false), "environmental fallback replaces missing witness");
        Check(progress.HasFlag(Mq12Campaign.EnvironmentalFallback), "fallback state is durable");
        Check(mq12.ConfirmOldNode("mq12-old-node"), "old node can be confirmed after forest evidence");
        Check(progress.HasFlag(Mq12Campaign.NodeConfirmed), "node confirmation is durable");

        var restored = Ready();
        restored.RestoreFlags(progress.CaptureFlags());
        restored.Tracking.Restore(progress.Tracking.Capture());
        restored.Navigation.Restore(progress.Navigation.Capture());
        restored.Quests.Restore(progress.Quests.Capture());
        var reloaded = new Mq12Campaign(restored, "mq12-route", ["mq12-ref-1", "mq12-ref-2", "mq12-ref-3"]);
        Check(reloaded.HasRequiredReferences, "save/load preserves two-of-three tracking threshold");
        Check(restored.Navigation.RouteAnomalyActive, "save/load preserves active route anomaly during investigation");
        Check(reloaded.CompleteQuest(), "MQ12 completes after persisted node confirmation");
        Check(!restored.Navigation.RouteAnomalyActive, "completion clears route anomaly");
        Check(restored.HasFlag(Mq12Campaign.Complete), "MQ12 completion is durable");
        Check(restored.Quests.Get(Mq12Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ12 unlocks MQ13");
        Check(!reloaded.CompleteQuest(), "MQ12 completion is idempotent");

        var exit = Ready();
        var exitQuest = new Mq12Campaign(exit, "mq12-route", ["mq12-ref-1", "mq12-ref-2", "mq12-ref-3"]);
        Check(exitQuest.BeginRouteAnomaly(), "route anomaly starts for exit recovery case");
        Check(exitQuest.LeaveRegionSafely(), "leaving region clears MQ12 route anomaly");
        Check(!exit.Navigation.RouteAnomalyActive, "region exit cannot trap player in anomaly state");

        return checks;
    }
}
