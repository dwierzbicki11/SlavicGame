using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq22CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = new WorldState(); gated.Initialize(); gated.Progress.Quests.Get(Mq22Campaign.QuestId).SetPhase(QuestPhase.Offered);
        Check(!new Mq22Campaign(gated.Progress).Begin(), "MQ22 is gated by MQ21 completion");

        foreach (var stabilization in Enum.GetValues<Mq22Stabilization>())
        {
            var world = NewWorld(); var mq22 = new Mq22Campaign(world.Progress);
            Check(mq22.Begin(), $"MQ22 begins for {stabilization}");
            Check(mq22.EnterMineBreach(), "mine breach is recorded");
            Check(mq22.RecordConstructionClue(Mq22ConstructionClue.ToolingPattern), "second clue can be found first");
            Check(mq22.RecordConstructionClue(Mq22ConstructionClue.MaterialLayer), "first clue can be found second");
            Check(mq22.HasBothConstructionClues, "two independent construction clues are required");
            Check(mq22.AnalyzeAnchorMaterial(), "anchor material can be analyzed");
            Check(world.Progress.HasFlag(Mq22Campaign.AnchorMaterial), "anchor material evidence is persistent state");
            Check(mq22.Stabilize(stabilization), $"{stabilization} stabilizes local site");
            Check(!mq22.Stabilize(stabilization), "stabilization outcome is single-shot");
            Check(mq22.CompleteQuest(), $"{stabilization} completes MQ22");
            Check(world.Progress.HasFlag(Mq22Campaign.Complete), "MQ22 completion flag written");
            Check(world.Progress.Quests.Get(Mq22Campaign.NextQuestId).Phase == QuestPhase.Offered, "every stabilization unlocks MQ23");
            Check(!mq22.CompleteQuest(), "MQ22 completion is idempotent");
        }

        var fallbackWorld = NewWorld(); var fallback = new Mq22Campaign(fallbackWorld.Progress); fallback.Begin(); fallback.EnterMineBreach();
        fallback.RecordConstructionClue(Mq22ConstructionClue.MaterialLayer); fallback.RecordConstructionClue(Mq22ConstructionClue.ToolingPattern);
        Check(fallback.AnalyzeAnchorMaterial(optionalNpcAvailable: false), "anchor material has fallback without optional NPC");

        var persistent = NewWorld(); var campaign = new Mq22Campaign(persistent.Progress); campaign.Begin(); campaign.EnterMineBreach(); campaign.RecordConstructionClue(Mq22ConstructionClue.MaterialLayer);
        var json = SaveGameService.Serialize(persistent); var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var restoredCampaign = new Mq22Campaign(restored.Progress);
        Check(restored.Progress.HasFlag(Mq22Campaign.MineBreach), "active hazard survives save/load");
        Check(restoredCampaign.RecordConstructionClue(Mq22ConstructionClue.ToolingPattern), "investigation resumes after save/load");
        Check(restoredCampaign.HasBothConstructionClues, "clue progress survives save/load");
        Check(restoredCampaign.AnalyzeAnchorMaterial(optionalNpcAvailable: false), "fallback analysis works after restore");
        Check(restoredCampaign.Stabilize(Mq22Stabilization.BypassHazard), "restored MQ22 can stabilize site");
        Check(restoredCampaign.CompleteQuest(), "restored MQ22 completes");
        var completedJson = SaveGameService.Serialize(restored); var completedRestored = new WorldState(); completedRestored.Initialize(); SaveGameService.Restore(completedRestored, completedJson);
        Check(completedRestored.Progress.HasFlag(Mq22Campaign.AnchorMaterial), "critical material survives completed save/load");
        Check(completedRestored.Progress.Quests.Get(Mq22Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ23 handoff survives save/load");
        return checks;
    }

    private static WorldState NewWorld()
    {
        var world = new WorldState(); world.Initialize(); world.Progress.SetFlag("MQ21_COMPLETE"); world.Progress.Quests.Get(Mq22Campaign.QuestId).SetPhase(QuestPhase.Offered); return world;
    }
}