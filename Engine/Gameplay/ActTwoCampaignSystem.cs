using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public static class ActTwoCampaignSystem
{
    // MQ20 — Estuary Barrier & Remote Beacon
    public static readonly Vector3 EstuaryBarrierSpot = new(148f, 0f, -182f);
    public static readonly Vector3 RemoteBeaconSpot = new(154f, 0f, -186f);

    // MQ21 — River Bridge Crossing & Camp Negotiations
    public static readonly Vector3 RiverCrossingBridgeSpot = new(179.25f, 0f, -180f);
    public static readonly Vector3 LeagueClaimSpot = new(175f, 0f, -178f);
    public static readonly Vector3 NadborClaimSpot = new(142f, 0f, -165f);
    public static readonly Vector3 TradeTreatySpot = new(160f, 0f, -172f);

    // MQ22 — Ancient Mine Breach in Dębowa Knieja
    public static readonly Vector3 MineBreachSpot = new(-410f, 0f, 310f);
    public static readonly Vector3 AncientTimberingSpot = new(-415f, 0f, 316f);
    public static readonly Vector3 RunicToolsSpot = new(-408f, 0f, 318f);
    public static readonly Vector3 AnchorOreSpot = new(-412f, 0f, 322f);
    public static readonly Vector3 StabilizationSpot = new(-410f, 0f, 312f);

    // MQ23 — Mine Resource Division (Council at Gród Borzywoja)
    public static readonly Vector3 MineCouncilTableSpot = new(-318f, 0f, 226f);
    public static readonly Vector3 MineImpactExtractionSpot = new(-320f, 0f, 228f);
    public static readonly Vector3 MineImpactSafetySpot = new(-316f, 0f, 227f);
    public static readonly Vector3 MineImpactStabilizationSpot = new(-318f, 0f, 224f);

    // MQ24 — Trial of Passage through Bór Perunowy
    public static readonly Vector3 LightningWaypostSpot = new(-260f, 0f, -315f);
    public static readonly Vector3 TerrainMossRockSpot = new(-305f, 0f, -355f);
    public static readonly Vector3 MistPassageTrialSpot = new(-375f, 0f, -395f);

    // MQ25 — Grand Synthesis of Slavic Trails (Act II Finale - 50% Milestone)
    public static readonly Vector3 GrandSynthesisSpot = new(-319f, 0f, 225f);

    public const string MilestoneActTwo50Percent = "MILESTONE_ACT_TWO_50_PERCENT";
    public const string TitleMasterOfTrailsFlag = "TITLE_POWIERNIK_SZLAKOW";

    public const float InteractionDistance = 4.2f;

    // --- MQ20 ---
    public static bool CanBeginAct2(WorldState world) =>
        world.Progress.HasFlag(Mq13Campaign.Complete) &&
        world.Progress.Quests.Get(Mq20Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginAct2(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginAct2(world)) return false;

        var mq20 = new Mq20Campaign(world.Progress);
        return mq20.Begin();
    }

    public static bool CanNegotiateEstuary(WorldState world)
    {
        var mq20 = new Mq20Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq20Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !mq20.HasMapAccess;
    }

    public static bool NegotiateEstuary(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanNegotiateEstuary(world)) return false;

        var mq20 = new Mq20Campaign(world.Progress);
        var success = mq20.GainMapAccess(
            world.Progress.HasFlag(ActOneCampaignSystem.TitleDefenderFlag)
                ? Mq20MapAccessRoute.Reputation
                : Mq20MapAccessRoute.Negotiation);

        if (success)
        {
            world.Progress.Reputation.Change(ReputationScope.Village, "przystan-rybacka", 10);
        }
        return success;
    }

    public static bool CanInspectRemoteBeacon(WorldState world)
    {
        var mq20 = new Mq20Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq20Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq20.HasMapAccess &&
               !world.Progress.HasFlag(Mq20Campaign.RemoteNodeEvidence);
    }

    public static bool InspectRemoteBeacon(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInspectRemoteBeacon(world)) return false;

        var mq20 = new Mq20Campaign(world.Progress);
        return mq20.CompareRemoteNode();
    }

    public static bool CanResolveEstuary(WorldState world)
    {
        var mq20 = new Mq20Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq20Campaign.QuestId).Phase == QuestPhase.Investigation &&
               world.Progress.HasFlag(Mq20Campaign.RemoteNodeEvidence) &&
               !world.Progress.HasFlag(Mq20Campaign.Complete);
    }

    public static bool ResolveEstuary(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanResolveEstuary(world)) return false;

        var mq20 = new Mq20Campaign(world.Progress);
        mq20.SetEstuaryOutcome(Mq20EstuaryOutcome.Resolved);
        var completed = mq20.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("simple-bandage", 3);
            world.Progress.Inventory.Add("arrow-basic", 16);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 20);
        }
        return completed;
    }

    // --- MQ21: SPÓR O TRAKT RZECZNY ---
    public static bool CanBeginMq21(WorldState world) =>
        world.Progress.HasFlag(Mq20Campaign.Complete) &&
        world.Progress.Quests.Get(Mq21Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq21(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq21(world)) return false;
        var mq21 = new Mq21Campaign(world.Progress);
        return mq21.Begin();
    }

    public static bool CanInspectLeagueClaim(WorldState world)
    {
        var mq21 = new Mq21Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq21Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag("MQ21_CLAIM_VERIFIED_LEAGUE");
    }

    public static bool InspectLeagueClaim(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInspectLeagueClaim(world)) return false;
        var mq21 = new Mq21Campaign(world.Progress);
        return mq21.VerifyClaim(Mq21ClaimSide.League);
    }

    public static bool CanInspectNadborClaim(WorldState world)
    {
        var mq21 = new Mq21Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq21Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag("MQ21_CLAIM_VERIFIED_NADBOR");
    }

    public static bool InspectNadborClaim(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInspectNadborClaim(world)) return false;
        var mq21 = new Mq21Campaign(world.Progress);
        return mq21.VerifyClaim(Mq21ClaimSide.Nadbor);
    }

    public static bool CanResolveTradeTreaty(WorldState world)
    {
        var mq21 = new Mq21Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq21Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq21.HasVerifiedBothClaims &&
               !mq21.HasDecision;
    }

    public static bool ResolveTradeTreaty(WorldState world, Mq21AccessDecision decision = Mq21AccessDecision.LimitedBoth)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanResolveTradeTreaty(world)) return false;
        var mq21 = new Mq21Campaign(world.Progress);
        var chosen = mq21.ChooseAccess(decision);
        if (!chosen) return false;
        var completed = mq21.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("iron-chunk", 3);
            world.Progress.Inventory.Add("forest-resin", 3);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 15);
            world.Progress.Reputation.Change(ReputationScope.Village, "przystan-rybacka", 15);
        }
        return completed;
    }

    // --- MQ22: WYŁOM W STAREJ SZTOLNI ---
    public static bool CanBeginMq22(WorldState world) =>
        world.Progress.HasFlag(Mq21Campaign.Complete) &&
        world.Progress.Quests.Get(Mq22Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq22(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq22(world)) return false;
        var mq22 = new Mq22Campaign(world.Progress);
        return mq22.Begin();
    }

    public static bool CanEnterMineBreach(WorldState world)
    {
        var mq22 = new Mq22Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq22Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag(Mq22Campaign.MineBreach);
    }

    public static bool EnterMineBreach(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanEnterMineBreach(world)) return false;
        var mq22 = new Mq22Campaign(world.Progress);
        return mq22.EnterMineBreach();
    }

    public static bool CanInspectTimbering(WorldState world)
    {
        var mq22 = new Mq22Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq22Campaign.QuestId).Phase == QuestPhase.Investigation &&
               world.Progress.HasFlag(Mq22Campaign.MineBreach) &&
               !world.Progress.HasFlag("MQ22_CONSTRUCTION_CLUE_MATERIALLAYER");
    }

    public static bool InspectTimbering(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInspectTimbering(world)) return false;
        var mq22 = new Mq22Campaign(world.Progress);
        return mq22.RecordConstructionClue(Mq22ConstructionClue.MaterialLayer);
    }

    public static bool CanInspectTools(WorldState world)
    {
        var mq22 = new Mq22Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq22Campaign.QuestId).Phase == QuestPhase.Investigation &&
               world.Progress.HasFlag(Mq22Campaign.MineBreach) &&
               !world.Progress.HasFlag("MQ22_CONSTRUCTION_CLUE_TOOLINGPATTERN");
    }

    public static bool InspectTools(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInspectTools(world)) return false;
        var mq22 = new Mq22Campaign(world.Progress);
        return mq22.RecordConstructionClue(Mq22ConstructionClue.ToolingPattern);
    }

    public static bool CanAnalyzeAnchorOre(WorldState world)
    {
        var mq22 = new Mq22Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq22Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq22.HasBothConstructionClues &&
               !world.Progress.HasFlag(Mq22Campaign.AnchorMaterial);
    }

    public static bool AnalyzeAnchorOre(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanAnalyzeAnchorOre(world)) return false;
        var mq22 = new Mq22Campaign(world.Progress);
        return mq22.AnalyzeAnchorMaterial();
    }

    public static bool CanStabilizeMine(WorldState world)
    {
        var mq22 = new Mq22Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq22Campaign.QuestId).Phase == QuestPhase.Investigation &&
               world.Progress.HasFlag(Mq22Campaign.AnchorMaterial) &&
               !mq22.HasStabilization;
    }

    public static bool StabilizeMine(WorldState world, Mq22Stabilization stabilization = Mq22Stabilization.SecureSite)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanStabilizeMine(world)) return false;
        var mq22 = new Mq22Campaign(world.Progress);
        var stabilized = mq22.Stabilize(stabilization);
        if (!stabilized) return false;
        var completed = mq22.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("iron-chunk", 4);
            world.Progress.Inventory.Add("forest-resin", 4);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 25);
        }
        return completed;
    }

    // --- MQ23: PODZIAŁ ZASOBÓW STAREJ SZTOLNI ---
    public static bool CanBeginMq23(WorldState world) =>
        world.Progress.HasFlag(Mq22Campaign.Complete) &&
        world.Progress.Quests.Get(Mq23Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq23(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq23(world)) return false;
        var mq23 = new Mq23Campaign(world.Progress);
        return mq23.Begin();
    }

    public static bool CanReviewImpact(WorldState world, Mq23Impact impact)
    {
        var mq23 = new Mq23Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq23Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag("MQ23_IMPACT_REVIEWED_" + impact.ToString().ToUpperInvariant());
    }

    public static bool ReviewImpact(WorldState world, Mq23Impact impact)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanReviewImpact(world, impact)) return false;
        var mq23 = new Mq23Campaign(world.Progress);
        return mq23.ReviewImpact(impact);
    }

    public static bool CanChooseResourceControl(WorldState world)
    {
        var mq23 = new Mq23Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq23Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq23.HasReviewedAllImpacts &&
               !mq23.HasDecision;
    }

    public static bool ChooseResourceControl(WorldState world, Mq23ResourceDecision decision = Mq23ResourceDecision.SupervisedSplit)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanChooseResourceControl(world)) return false;
        var mq23 = new Mq23Campaign(world.Progress);
        var chosen = mq23.ChooseResourceControl(decision);
        if (!chosen) return false;
        var completed = mq23.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("iron-chunk", 5);
            world.Progress.Inventory.Add("old-coin", 35);
            world.Progress.Inventory.Add("simple-bandage", 3);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 25);
            world.Progress.Reputation.Change(ReputationScope.Village, "old-village", 15);
        }
        return completed;
    }

    // --- MQ24: PRÓBA PRZEJŚCIA PRZEZ BÓR PERUNOWY ---
    public static bool CanBeginMq24(WorldState world) =>
        world.Progress.HasFlag(Mq23Campaign.Complete) &&
        world.Progress.Quests.Get(Mq24Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq24(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq24(world)) return false;
        var mq24 = new Mq24Campaign(world.Progress);
        return mq24.Begin();
    }

    public static bool CanInspectRouteClue(WorldState world, Mq24RouteClue clue)
    {
        var mq24 = new Mq24Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq24Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !world.Progress.HasFlag("MQ24_ROUTE_CLUE_" + clue.ToString().ToUpperInvariant());
    }

    public static bool InspectRouteClue(WorldState world, Mq24RouteClue clue)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanInspectRouteClue(world, clue)) return false;
        var mq24 = new Mq24Campaign(world.Progress);
        return mq24.RecordRouteClue(clue);
    }

    public static bool CanAttemptTrialPassage(WorldState world)
    {
        var mq24 = new Mq24Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq24Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq24.HasBothRouteClues &&
               !world.Progress.HasFlag(Mq24Campaign.RouteRelation);
    }

    public static bool AttemptTrialPassage(WorldState world, bool routeApplied = true)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanAttemptTrialPassage(world)) return false;
        var mq24 = new Mq24Campaign(world.Progress);
        var passed = mq24.AttemptTrialPassage(routeApplied);
        if (!passed) return false;
        var completed = mq24.CompleteQuest();
        if (completed)
        {
            world.Progress.Inventory.Add("forest-resin", 5);
            world.Progress.Inventory.Add("arrow-iron", 20);
            world.Progress.Inventory.Add("simple-bandage", 3);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 30);
            world.Progress.Reputation.Change(ReputationScope.Village, "osada-smolnikow", 25);
        }
        return completed;
    }

    // --- MQ25: WIELKA SYNTEZA SZLAKÓW SŁOWIAŃSKICH (50% KAMPANII) ---
    public static bool CanBeginMq25(WorldState world) =>
        world.Progress.HasFlag(Mq20Campaign.Complete) &&
        world.Progress.HasFlag(Mq21Campaign.Complete) &&
        world.Progress.HasFlag(Mq22Campaign.Complete) &&
        world.Progress.HasFlag(Mq23Campaign.Complete) &&
        world.Progress.HasFlag(Mq24Campaign.Complete) &&
        world.Progress.Quests.Get(Mq25Campaign.QuestId).Phase == QuestPhase.Offered;

    public static bool BeginMq25(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanBeginMq25(world)) return false;
        var mq25 = new Mq25Campaign(world.Progress);
        return mq25.Begin();
    }

    public static bool CanValidateCriticalPackages(WorldState world)
    {
        var mq25 = new Mq25Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq25Campaign.QuestId).Phase == QuestPhase.Investigation &&
               !mq25.HasAllCriticalPackages;
    }

    public static bool ValidateCriticalPackages(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanValidateCriticalPackages(world)) return false;
        var mq25 = new Mq25Campaign(world.Progress);
        return mq25.ValidateCriticalPackages();
    }

    public static bool CanRecordSharedConnection(WorldState world, Mq25SharedConnection connection)
    {
        var mq25 = new Mq25Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq25Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq25.HasAllCriticalPackages &&
               !world.Progress.HasFlag("MQ25_CONNECTION_" + connection.ToString().ToUpperInvariant());
    }

    public static bool RecordSharedConnection(WorldState world, Mq25SharedConnection connection)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanRecordSharedConnection(world, connection)) return false;
        var mq25 = new Mq25Campaign(world.Progress);
        return mq25.RecordSharedConnection(connection);
    }

    public static bool CanConcludeSynthesis(WorldState world)
    {
        var mq25 = new Mq25Campaign(world.Progress);
        return world.Progress.Quests.Get(Mq25Campaign.QuestId).Phase == QuestPhase.Investigation &&
               mq25.HasAllCriticalPackages &&
               mq25.HasRequiredConnections &&
               !world.Progress.HasFlag(Mq25Campaign.MulticulturalOrigin);
    }

    public static bool ConcludeSynthesis(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!CanConcludeSynthesis(world)) return false;
        var mq25 = new Mq25Campaign(world.Progress);
        var synthesized = mq25.ConcludeSynthesis();
        if (!synthesized) return false;
        var completed = mq25.CompleteQuest();
        if (completed)
        {
            world.Progress.SetFlag(MilestoneActTwo50Percent);
            world.Progress.SetFlag(TitleMasterOfTrailsFlag);
            world.Progress.Inventory.Add("iron-chunk", 10);
            world.Progress.Inventory.Add("forest-resin", 10);
            world.Progress.Inventory.Add("arrow-iron", 40);
            world.Progress.Inventory.Add("old-coin", 100);
            world.Progress.Inventory.Add("sword-slavic-spatha", 1);
            world.Progress.Reputation.Change(ReputationScope.Faction, "stronghold", 50);
            world.Progress.Reputation.Change(ReputationScope.Village, "old-village", 50);
            world.Progress.Reputation.Change(ReputationScope.Village, "przystan-rybacka", 50);
        }
        return completed;
    }
}
