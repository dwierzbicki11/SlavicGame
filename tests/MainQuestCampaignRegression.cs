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

        var mq00 = progress.Quests.Get(MainQuestCampaign.Mq00Id);
        mq00.SetPhase(QuestPhase.Resolved);
        Check(mq00.ClaimReward(), "MQ00 can be turned in");
        campaign.SynchronizeAct0();
        Check(progress.HasFlag(MainQuestCampaign.Mq00Complete), "MQ00 turn-in writes durable completion flag");
        Check(progress.Quests.Get(MainQuestCampaign.Mq01Id).Phase == QuestPhase.Offered,
            "MQ00 completion offers MQ01");

        Check(campaign.MarkMq01ConsequenceSeen(), "MQ01 consequence beat can be persisted");
        Check(progress.HasFlag(MainQuestCampaign.Mq01ConsequenceSeen), "MQ01 consequence flag is durable");
        Check(campaign.GrantNadborzeLead(), "Primary path grants Nadborze lead");
        Check(campaign.EnsureNadborzeLead(), "Fallback lead path is idempotent");
        Check(progress.HasFlag(MainQuestCampaign.Mq01NadborzeLead), "Nadborze lead is durable");
        Check(progress.Quests.Get(MainQuestCampaign.Mq01Id).Phase == QuestPhase.Active,
            "Lead activates MQ01");

        var flags = progress.CaptureFlags();
        var restored = new GameProgress();
        restored.RestoreFlags(flags);
        restored.Quests.Restore(progress.Quests.Capture());
        var restoredCampaign = new MainQuestCampaign(restored);
        restoredCampaign.SynchronizeAct0();
        Check(restored.HasFlag(MainQuestCampaign.Mq01NadborzeLead), "Save/load keeps MQ01 lead");
        Check(restoredCampaign.DepartForNadborze(), "Travel boundary completes MQ01 once");
        Check(!restoredCampaign.DepartForNadborze(), "Travel boundary cannot complete MQ01 twice");
        Check(restored.HasFlag(MainQuestCampaign.Mq01Complete), "MQ01 completion is durable");
        Check(restored.ProgressQuestPhase(MainQuestCampaign.Mq10Id) == QuestPhase.Offered,
            "MQ01 completion unlocks MQ10 exactly through campaign state");

        return checks;
    }

    private static QuestPhase ProgressQuestPhase(this GameProgress progress, string id) => progress.Quests.Get(id).Phase;
}
