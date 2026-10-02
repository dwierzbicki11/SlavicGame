using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;

namespace SlavicGame.RegressionTests;

internal static class MainQuestCampaignRegression
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

        var progress = new GameProgress();
        var campaign = new MainQuestCampaign(progress);
        Check(!campaign.GrantNadborzeLead(), "MQ01 lead stays locked before MQ00 completion");
        Check(!campaign.DiscoverMq10Site(), "MQ10 site stays locked before MQ01 completion");

        var mq00 = progress.Quests.Get(MainQuestCampaign.Mq00Id);
        mq00.SetPhase(QuestPhase.Resolved);
        Check(mq00.ClaimReward(), "MQ00 can be turned in");
        campaign.SynchronizeAct0();
        Check(progress.HasFlag(MainQuestCampaign.Mq00Complete), "MQ00 turn-in writes durable completion flag");
        Check(progress.Quests.Get(MainQuestCampaign.Mq01Id).Phase == QuestPhase.Offered, "MQ00 completion offers MQ01");

        Check(campaign.MarkMq01ConsequenceSeen(), "MQ01 consequence beat can be persisted");
        Check(campaign.GrantNadborzeLead(), "Primary path grants Nadborze lead");
        Check(campaign.EnsureNadborzeLead(), "Fallback lead path is idempotent");
        Check(campaign.DepartForNadborze(), "Travel boundary completes MQ01 once");
        Check(!campaign.DepartForNadborze(), "Travel boundary cannot complete MQ01 twice");
        Check(progress.Quests.Get(MainQuestCampaign.Mq10Id).Phase == QuestPhase.Offered, "MQ01 completion unlocks MQ10");

        Check(campaign.DiscoverMq10Site(), "MQ10 site discovery starts investigation");
        Check(progress.HasFlag(MainQuestCampaign.Mq10SiteFound), "MQ10 site discovery is durable");
        Check(progress.Quests.Get(MainQuestCampaign.Mq10Id).Phase == QuestPhase.Investigation, "MQ10 enters investigation phase");
        Check(!campaign.CompareMq10Pattern(), "MQ10 pattern cannot be compared before two independent facts");

        Check(campaign.RecordMq10OldLayerEvidence("MQ10-E01", "Older route mark", "environment.route"), "first MQ10 evidence is accepted");
        Check(!progress.HasFlag(MainQuestCampaign.Mq10OldLayerConfirmed), "one fact does not confirm old layer");
        Check(campaign.RecordMq10OldLayerEvidence("MQ10-E02", "Buried node material", "environment.node"), "second MQ10 evidence is accepted");
        Check(progress.HasFlag(MainQuestCampaign.Mq10OldLayerConfirmed), "two independent facts confirm old layer");
        Check(campaign.CompareMq10Pattern(), "confirmed layer enables MQ00 comparison");
        Check(progress.HasFlag(MainQuestCampaign.Mq10Pattern02), "MQ10 pattern comparison is durable");
        Check(campaign.SecureMq10Site(), "site can be secured after critical evidence is preserved");

        var flags = progress.CaptureFlags();
        var quests = progress.Quests.Capture();
        var restored = new GameProgress();
        restored.RestoreFlags(flags);
        restored.Quests.Restore(quests);
        var restoredCampaign = new MainQuestCampaign(restored);
        Check(restored.HasFlag(MainQuestCampaign.Mq10Pattern02), "save/load keeps MQ10 pattern state");
        Check(restored.Quests.Get(MainQuestCampaign.Mq10Id).Evidence.Count == 2, "save/load keeps MQ10 evidence");
        Check(restoredCampaign.CompleteMq10(), "secured MQ10 completes after reload");
        Check(!restoredCampaign.CompleteMq10(), "MQ10 completion is idempotent");
        Check(restored.HasFlag(MainQuestCampaign.Mq10Complete), "MQ10 completion flag is durable");
        Check(restored.Quests.Get(MainQuestCampaign.Mq11Id).Phase == QuestPhase.Offered, "MQ10 completion unlocks MQ11");

        Check(restoredCampaign.SeeMq11Policy(), "MQ11 policy beat activates quest");
        Check(!restoredCampaign.ChooseMq11Position(Mq11Position.ConditionalCooperation, "debrzyn-authority", 5), "MQ11 decision waits for required perspectives and test case");
        Check(restoredCampaign.RecordMq11Perspective(administration: true), "MQ11 administration perspective is durable");
        Check(restoredCampaign.RecordMq11Perspective(administration: false), "MQ11 community perspective is durable");
        Check(restoredCampaign.RecordMq11ProcedureCost(), "MQ11 procedure cost test case is durable");

        restored.Reputation.Change(ReputationScope.Faction, "debrzyn-authority", -40);
        Check(restoredCampaign.ChooseMq11Position(Mq11Position.ConditionalCooperation, "debrzyn-authority", 5), "MQ11 accepts a position from hostile reputation state");
        Check(restored.Reputation.Get(ReputationScope.Faction, "debrzyn-authority") == -35, "MQ11 applies configured faction reputation consequence once");
        Check(!restoredCampaign.ChooseMq11Position(Mq11Position.LocalArrangement, "debrzyn-authority", 20), "MQ11-D01 cannot be selected twice");
        Check(restored.Reputation.Get(ReputationScope.Faction, "debrzyn-authority") == -35, "repeated MQ11 choice cannot duplicate reputation consequence");

        var mq11Flags = restored.CaptureFlags();
        var mq11Quests = restored.Quests.Capture();
        var mq11Reputation = restored.Reputation.Capture();
        var mq11Reloaded = new GameProgress();
        mq11Reloaded.RestoreFlags(mq11Flags);
        mq11Reloaded.Quests.Restore(mq11Quests);
        mq11Reloaded.Reputation.Restore(mq11Reputation);
        var mq11Campaign = new MainQuestCampaign(mq11Reloaded);
        Check(mq11Reloaded.HasFlag(MainQuestCampaign.Mq11DecisionConditional), "save/load keeps MQ11-D01 choice");
        Check(mq11Reloaded.Reputation.Get(ReputationScope.Faction, "debrzyn-authority") == -35, "save/load keeps MQ11 faction consequence");
        Check(mq11Campaign.CompleteMq11(), "MQ11 completes after persisted decision");
        Check(!mq11Campaign.CompleteMq11(), "MQ11 completion is idempotent");
        Check(mq11Reloaded.HasFlag(MainQuestCampaign.Mq11Complete), "MQ11 completion flag is durable");
        Check(mq11Reloaded.Quests.Get(MainQuestCampaign.Mq12Id).Phase == QuestPhase.Offered, "every MQ11 result keeps MQ12 available");

        foreach (var position in Enum.GetValues<Mq11Position>())
        {
            var branch = new GameProgress();
            branch.RestoreFlags(mq11Flags.Where(flag => !flag.StartsWith("MQ11-D01_", StringComparison.Ordinal)));
            branch.Quests.Restore(mq11Quests);
            branch.Reputation.Change(ReputationScope.Faction, "debrzyn-authority", -20);
            var branchCampaign = new MainQuestCampaign(branch);
            Check(branchCampaign.ChooseMq11Position(position, "debrzyn-authority", 0), $"MQ11 position {position} is available");
            Check(branchCampaign.CompleteMq11(), $"MQ11 position {position} completes quest");
            Check(branch.Quests.Get(MainQuestCampaign.Mq12Id).Phase == QuestPhase.Offered, $"MQ11 position {position} unlocks MQ12");
        }

        return checks;
    }
}
