using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public static class ActOneCampaignSystem
{
    public const string MilestoneAct1Flag = "milestone.act1-completed-25percent";
    public const string TitleDefenderFlag = "tytul.obronca-puszczy";
    public const string RecommendLetterItemId = "list-polecajacy";
    public const string PrinceRingItemId = "pierscien-borzywoja";

    public static readonly Vector3 ZarnowiecLeadSpot = new(-6f, 0f, -84f);
    public static readonly Vector3 StrongholdGateSpot = new(-295f, 0f, 215f);
    public static readonly Vector3 StrongholdCouncilSpot = new(-320f, 0f, 220f);
    public static readonly Vector3 StrongholdMapTableSpot = new(-322f, 0f, 224f);
    public static readonly Vector3 KurhanSteleSpot = new(-95f, 0f, 68f);
    public static readonly Vector3 PerunBoundaryStoneSpot = new(-375f, 0f, -175f);

    public const float InteractionDistance = 4.2f;

    public static bool CanTakeLead(WorldState world) =>
        world.Progress.HasFlag(VerticalSliceQuestInteractions.TurnedInFlag) &&
        !world.Progress.HasFlag(MainQuestCampaign.Mq01NadborzeLead);

    public static bool TakeLead(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanTakeLead(world)) return false;

        world.Progress.SetFlag(MainQuestCampaign.Mq00Complete);
        world.Progress.Quests.Get(MainQuestCampaign.Mq00Id).SetPhase(QuestPhase.TurnedIn);

        var campaign = new MainQuestCampaign(world.Progress);
        campaign.SynchronizeAct0();
        campaign.GrantNadborzeLead();

        world.Progress.Inventory.Add(RecommendLetterItemId, 1);
        world.Progress.Quests.Get(MainQuestCampaign.Mq01Id).SetPhase(QuestPhase.Active);
        world.Progress.Relationships.Change("missing-family", SlavicGame.Engine.Relationships.RelationshipKind.Trust, 5);
        return true;
    }

    public static bool CanEnterStronghold(WorldState world) =>
        world.Progress.HasFlag(MainQuestCampaign.Mq01NadborzeLead) &&
        !world.Progress.HasFlag(MainQuestCampaign.Mq01Complete);

    public static bool EnterStronghold(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanEnterStronghold(world)) return false;

        var campaign = new MainQuestCampaign(world.Progress);
        campaign.DepartForNadborze();

        world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 10);
        return true;
    }

    public static bool CanInspectKurhan(WorldState world) =>
        world.Progress.HasFlag(MainQuestCampaign.Mq01Complete) &&
        !world.Progress.HasFlag(MainQuestCampaign.Mq10Complete);

    public static bool InspectKurhan(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInspectKurhan(world)) return false;

        var campaign = new MainQuestCampaign(world.Progress);
        campaign.DiscoverMq10Site();
        campaign.RecordMq10OldLayerEvidence("mq10-fact-runes", "Odkryto pradawne runy splotu pod warstwa popiolu kurhanu.");
        campaign.RecordMq10OldLayerEvidence("mq10-fact-graves", "Uklad mogil kurhanowych odpowiada pradawnej linii mocy Peruna.");
        campaign.CompareMq10Pattern();
        campaign.SecureMq10Site();
        return campaign.CompleteMq10();
    }

    public static bool CanHoldCouncil(WorldState world) =>
        world.Progress.HasFlag(MainQuestCampaign.Mq10Complete) &&
        !world.Progress.HasFlag(MainQuestCampaign.Mq11Complete);

    public static bool HoldCouncil(WorldState world, Mq11Position position = Mq11Position.ConditionalCooperation)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanHoldCouncil(world)) return false;

        var campaign = new MainQuestCampaign(world.Progress);
        campaign.SeeMq11Policy();
        campaign.RecordMq11Perspective(administration: true);
        campaign.RecordMq11Perspective(administration: false);
        campaign.RecordMq11ProcedureCost();
        campaign.ChooseMq11Position(position, "stronghold", 15);
        return campaign.CompleteMq11();
    }

    public static bool CanInvestigatePerunBór(WorldState world) =>
        world.Progress.HasFlag(MainQuestCampaign.Mq11Complete) &&
        !world.Progress.HasFlag(Mq12Campaign.Complete);

    public static bool InvestigatePerunBór(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInvestigatePerunBór(world)) return false;

        var refs = new[] { "mq12-ref-1", "mq12-ref-2", "mq12-ref-3" };
        foreach (var r in refs)
        {
            if (!world.Progress.Tracking.IsDiscovered(r))
            {
                world.Progress.Tracking.Register(
                    r,
                    TrackCategory.EnvironmentalAnomaly,
                    PerunBoundaryStoneSpot,
                    TrackFreshness.Recent,
                    "Slad pradawnego wezla Peruna");
            }
        }

        var mq12 = new Mq12Campaign(world.Progress, "mq12-route", refs);
        mq12.BeginRouteAnomaly();
        mq12.DiscoverReference("mq12-ref-1");
        mq12.DiscoverReference("mq12-ref-2");
        mq12.ConfirmForestChange(witnessAvailable: false);
        mq12.ConfirmOldNode("mq12-old-node");
        return mq12.CompleteQuest();
    }

    public static bool CanSynthesizeMapTable(WorldState world) =>
        world.Progress.HasFlag(Mq12Campaign.Complete) &&
        !world.Progress.HasFlag(Mq13Campaign.Complete);

    public static bool SynthesizeMapTable(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanSynthesizeMapTable(world)) return false;

        var datasets = new[] { "nadborze-routes", "bor-nodes" };
        var evidence = new[] { "mq10-pattern", "mq11-context", "mq12-node" };

        var mq13 = new Mq13Campaign(world.Progress, datasets, evidence);
        if (!mq13.Begin()) return false;

        foreach (var d in datasets) mq13.AddDataset(d);
        foreach (var e in evidence) mq13.AddEvidence(e);

        if (!mq13.TestHypothesis("predicted-node-r2")) return false;
        if (!mq13.CompleteQuest()) return false;

        // Rewards and 25% milestone completion
        world.Progress.SetFlag(MilestoneAct1Flag);
        world.Progress.SetFlag(TitleDefenderFlag);
        world.Progress.Inventory.Add(PrinceRingItemId, 1);
        world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 30);
        return true;
    }
}
