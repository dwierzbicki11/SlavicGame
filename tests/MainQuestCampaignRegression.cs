using System.Runtime.CompilerServices;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;

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

        return checks;
    }
}
