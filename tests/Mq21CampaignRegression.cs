using System.Runtime.CompilerServices;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

namespace SlavicGame.RegressionTests;

internal static class Mq21CampaignRegression
{
    [ModuleInitializer] internal static void Initialize() => Run();

    public static int Run()
    {
        var checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }

        var gated = new WorldState(); gated.Initialize(); gated.Progress.Quests.Get(Mq21Campaign.QuestId).SetPhase(QuestPhase.Offered);
        Check(!new Mq21Campaign(gated.Progress).Begin(), "MQ21 is gated by MQ20 completion");

        foreach (var decision in Enum.GetValues<Mq21AccessDecision>())
        {
            var world = NewWorld(); var mq21 = new Mq21Campaign(world.Progress);
            Check(mq21.Begin(), $"MQ21 begins for {decision}");
            Check(!mq21.ChooseAccess(decision), $"{decision} requires claim verification");
            Check(mq21.VerifyClaim(Mq21ClaimSide.League), "League claim can be verified");
            Check(mq21.VerifyClaim(Mq21ClaimSide.Nadbor), "Nadbor claim can be verified");
            Check(mq21.HasVerifiedBothClaims, "both claims are verified");
            Check(mq21.ChooseAccess(decision), $"{decision} can be chosen");
            Check(!mq21.ChooseAccess(decision), "MQ21-D01 is single-shot");
            Check(mq21.CompleteQuest(), $"{decision} completes MQ21");
            Check(world.Progress.HasFlag(Mq21Campaign.Complete), "MQ21 completion flag written");
            Check(world.Progress.Quests.Get(Mq21Campaign.NextQuestId).Phase == QuestPhase.Offered, "every decision unlocks MQ22");
            Check(!mq21.CompleteQuest(), "MQ21 completion is idempotent");
        }

        var persistent = NewWorld(); var campaign = new Mq21Campaign(persistent.Progress); campaign.Begin();
        campaign.VerifyClaim(Mq21ClaimSide.League); campaign.VerifyClaim(Mq21ClaimSide.Nadbor); campaign.ChooseAccess(Mq21AccessDecision.LimitedBoth);
        var json = SaveGameService.Serialize(persistent); var restored = new WorldState(); restored.Initialize(); SaveGameService.Restore(restored, json);
        var restoredCampaign = new Mq21Campaign(restored.Progress);
        Check(restoredCampaign.HasVerifiedBothClaims, "claim verification survives save/load");
        Check(restoredCampaign.HasDecision, "MQ21-D01 survives save/load");
        Check(restoredCampaign.CompleteQuest(), "restored MQ21 completes");
        var completedJson = SaveGameService.Serialize(restored); var completedRestored = new WorldState(); completedRestored.Initialize(); SaveGameService.Restore(completedRestored, completedJson);
        Check(completedRestored.Progress.Quests.Get(Mq21Campaign.NextQuestId).Phase == QuestPhase.Offered, "MQ22 handoff survives save/load");
        return checks;
    }

    private static WorldState NewWorld()
    {
        var world = new WorldState(); world.Initialize(); world.Progress.SetFlag("MQ20_COMPLETE"); world.Progress.Quests.Get(Mq21Campaign.QuestId).SetPhase(QuestPhase.Offered); return world;
    }
}
