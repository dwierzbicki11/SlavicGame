using System.Runtime.CompilerServices;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq13RuntimeContractsRegression
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
        var overlay = world.Progress.MapOverlay;
        Check(overlay.AddDataset("nadborze-routes"), "dataset is added once");
        Check(!overlay.AddDataset("nadborze-routes"), "dataset add is idempotent");
        overlay.AddDataset("bor-nodes");
        overlay.AddEvidence("mq10-pattern");
        overlay.AddEvidence("mq12-node");

        var datasets = new[] { "nadborze-routes", "bor-nodes" };
        var evidence = new[] { "mq10-pattern", "mq12-node", "mq12-context" };
        Check(!overlay.CanSynthesize(datasets, evidence), "missing required evidence blocks synthesis");
        overlay.AddEvidence("mq12-context");
        Check(overlay.CanSynthesize(datasets, evidence), "minimal required set enables synthesis");
        Check(overlay.Synthesize(datasets, evidence, "predicted-node-r2"), "hypothesis synthesis records predicted point");
        Check(overlay.Synthesize(datasets, evidence, "predicted-node-r2"), "same synthesis is idempotent");
        Check(!overlay.Synthesize(datasets, evidence, "different-node"), "completed synthesis cannot silently change prediction");

        var json = SaveGameService.Serialize(world);
        var restored = new WorldState();
        restored.Initialize();
        SaveGameService.Restore(restored, json);
        Check(restored.Progress.MapOverlay.HypothesisSynthesized, "save/load restores synthesis state");
        Check(restored.Progress.MapOverlay.PredictedPointId == "predicted-node-r2", "save/load restores predicted point");
        Check(restored.Progress.MapOverlay.HasDataset("nadborze-routes"), "save/load restores datasets");
        Check(restored.Progress.MapOverlay.HasEvidence("mq12-context"), "save/load restores evidence");

        return checks;
    }
}
