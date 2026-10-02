using System.Numerics;
using System.Runtime.CompilerServices;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq12RuntimeContractsRegression
{
    [ModuleInitializer]
    internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception(name);
            checks++;
        }

        var world = new WorldState();
        world.Initialize();
        world.Progress.Tracking.Register(
            "mq12-track-01",
            TrackCategory.EnvironmentalAnomaly,
            new Vector3(3f, 0f, 7f),
            TrackFreshness.Recent,
            "mq12-evidence-01",
            magicSignature: "route-distortion");
        Check(world.Progress.Tracking.Discover("mq12-track-01"), "track discovery changes durable state once");
        Check(!world.Progress.Tracking.Discover("mq12-track-01"), "track discovery is idempotent");

        Check(world.Progress.Navigation.ActivateRouteAnomaly("mq12-route-anomaly"), "route anomaly activates");
        Check(world.Progress.Navigation.DiscoverLandmark("mq12-landmark-a"), "landmark discovery persists");
        Check(!world.Progress.Navigation.DiscoverLandmark("mq12-landmark-a"), "landmark discovery is idempotent");

        var json = SaveGameService.Serialize(world);
        var restored = new WorldState();
        restored.Initialize();
        SaveGameService.Restore(restored, json);

        Check(restored.Progress.Tracking.IsDiscovered("mq12-track-01"), "save/load restores discovered track");
        Check(restored.Progress.Navigation.ActiveRouteAnomalyId == "mq12-route-anomaly", "save/load restores active route anomaly");
        Check(restored.Progress.Navigation.HasLandmark("mq12-landmark-a"), "save/load restores landmark");
        Check(restored.Progress.Navigation.ClearRouteAnomaly("mq12-route-anomaly"), "restored route anomaly can recover cleanly");
        Check(!restored.Progress.Navigation.RouteAnomalyActive, "route recovery clears anomaly state");

        return checks;
    }
}
