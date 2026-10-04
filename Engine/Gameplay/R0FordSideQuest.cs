using System.Numerics;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public enum R0FordOutcome
{
    None,
    Repaired,
    Bypass,
    ClosedSafe
}

public static class R0FordSideQuest
{
    public const string QuestId = "SQ_R0_01";
    public const string RepairTimberItemId = "ford-repair-timber";

    public const string FordInspectedFlag = "sq_r0_01.ford-inspected";
    public const string RepairTimberCollectedFlag = "sq_r0_01.repair-timber-collected";
    public const string BypassDiscoveredFlag = "sq_r0_01.bypass-discovered";
    public const string AccessFlag = "r0_ford_access";
    public const string CompletedFlag = "sq_r0_01.completed";

    public const string FordLandmarkId = "r0-broken-ford";
    public const string BypassLandmarkId = "r0-ford-bypass";

    public const string FordEvidenceId = "sq_r0_01.broken-ford";
    public const string BypassEvidenceId = "sq_r0_01.safe-bypass";

    public const float FordZ = WaterLandscape.R0FordZ;
    public const float BypassZ = WaterLandscape.R0BypassZ;

    public static string OutcomeFlag(R0FordOutcome outcome) =>
        $"sq_r0_01.outcome.{outcome.ToString().ToLowerInvariant()}";

    public static Vector3 FordWestBankSpot =>
        BankSpot(FordZ, westBank: true);

    public static Vector3 FordCenter =>
        new(WaterLandscape.CenterX(FordZ), 0f, FordZ);

    public static Vector3 BypassWestBankSpot =>
        BankSpot(BypassZ, westBank: true);

    public static Vector3 BypassCenter =>
        new(WaterLandscape.CenterX(BypassZ), 0f, BypassZ);

    public static Vector3 CloseMarkerSpot =>
        new(
            FordWestBankSpot.X,
            0f,
            FordZ + 6.5f);

    public static bool InspectFord(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (world.Progress.HasFlag(FordInspectedFlag) ||
            Outcome(world) != R0FordOutcome.None)
        {
            return false;
        }

        var quest = world.Progress.Quests.Get(QuestId);
        if (quest.Phase == QuestPhase.Unavailable)
            quest.SetPhase(QuestPhase.Investigation);
        else if (quest.Phase is QuestPhase.Offered or QuestPhase.Active)
            quest.SetPhase(QuestPhase.Investigation);

        world.Progress.SetFlag(FordInspectedFlag);
        world.Progress.Navigation.DiscoverLandmark(FordLandmarkId);

        quest.AddEvidence(new EvidenceEntry(
            FordEvidenceId,
            QuestId,
            KnowledgeKind.ConfirmedFact,
            "Lokalna przeprawa jest uszkodzona i nie nadaje się do bezpiecznego przejścia bez naprawy albo wyznaczenia alternatywy.",
            FordLandmarkId));

        return true;
    }

    public static bool CollectRepairTimber(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!world.Progress.HasFlag(FordInspectedFlag) ||
            world.Progress.HasFlag(RepairTimberCollectedFlag) ||
            Outcome(world) != R0FordOutcome.None)
        {
            return false;
        }

        world.Progress.Inventory.Add(RepairTimberItemId, 2);
        world.Progress.SetFlag(RepairTimberCollectedFlag);
        return true;
    }

    public static bool DiscoverBypass(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!world.Progress.HasFlag(FordInspectedFlag) ||
            world.Progress.HasFlag(BypassDiscoveredFlag) ||
            Outcome(world) != R0FordOutcome.None)
        {
            return false;
        }

        world.Progress.SetFlag(BypassDiscoveredFlag);
        world.Progress.Navigation.DiscoverLandmark(BypassLandmarkId);

        world.Progress.Quests.Get(QuestId).AddEvidence(new EvidenceEntry(
            BypassEvidenceId,
            QuestId,
            KnowledgeKind.ConfirmedFact,
            "Niżej rzeki znajduje się płytsze przejście, które można oznaczyć jako bezpieczne obejście uszkodzonego brodu.",
            BypassLandmarkId));

        return true;
    }

    public static bool Resolve(
        WorldState world,
        R0FordOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(world);

        var quest = world.Progress.Quests.Get(QuestId);
        if (outcome == R0FordOutcome.None ||
            Outcome(world) != R0FordOutcome.None ||
            quest.Phase is QuestPhase.Resolved or QuestPhase.TurnedIn)
        {
            return false;
        }

        switch (outcome)
        {
            case R0FordOutcome.Repaired:
                if (!world.Progress.Inventory.Contains(RepairTimberItemId, 2))
                    return false;
                world.Progress.Inventory.Remove(RepairTimberItemId, 2);
                break;

            case R0FordOutcome.Bypass:
            case R0FordOutcome.ClosedSafe:
                if (!world.Progress.HasFlag(BypassDiscoveredFlag))
                    return false;
                break;

            default:
                return false;
        }

        world.Progress.SetFlag(OutcomeFlag(outcome));
        world.Progress.SetFlag(AccessFlag);
        world.Progress.SetFlag(CompletedFlag);

        quest.Resolve(QuestResolution.Other);

        var (money, reputation) = outcome switch
        {
            R0FordOutcome.Repaired => (25, 6),
            R0FordOutcome.Bypass => (15, 4),
            R0FordOutcome.ClosedSafe => (10, 3),
            _ => (0, 0)
        };

        if (money > 0)
            world.Progress.Profile.ChangeMoney(money);
        if (reputation > 0)
        {
            world.Progress.Reputation.Change(
                ReputationScope.Village,
                "old-village",
                reputation);
        }

        quest.ClaimReward();
        return true;
    }

    public static R0FordOutcome Outcome(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        foreach (var outcome in Enum.GetValues<R0FordOutcome>())
        {
            if (outcome == R0FordOutcome.None)
                continue;

            if (world.Progress.HasFlag(OutcomeFlag(outcome)))
                return outcome;
        }

        return R0FordOutcome.None;
    }

    public static bool DirectFordOpen(WorldState world) =>
        Outcome(world) == R0FordOutcome.Repaired;

    public static bool BypassOpen(WorldState world) =>
        Outcome(world) is
            R0FordOutcome.Bypass or
            R0FordOutcome.ClosedSafe;

    public static Vector2 ResolveTraversal(
        WorldState world,
        Vector2 position,
        float radius)
    {
        ArgumentNullException.ThrowIfNull(world);

        if (!DirectFordOpen(world))
            position = BlockLocalCrossing(position, radius, FordZ);

        if (!BypassOpen(world))
            position = BlockLocalCrossing(position, radius, BypassZ);

        return position;
    }

    private static Vector2 BlockLocalCrossing(
        Vector2 position,
        float radius,
        float routeZ)
    {
        const float halfRouteLength = 5.4f;

        if (MathF.Abs(position.Y - routeZ) > halfRouteLength + radius)
            return position;

        var centerX = WaterLandscape.CenterX(routeZ);
        var blockedHalfWidth =
            WaterLandscape.SurfaceHalfWidth(routeZ) + 0.85f + radius;

        var offset = position.X - centerX;
        if (MathF.Abs(offset) >= blockedHalfWidth)
            return position;

        position.X =
            offset <= 0f
                ? centerX - blockedHalfWidth
                : centerX + blockedHalfWidth;

        return position;
    }

    private static Vector3 BankSpot(
        float z,
        bool westBank)
    {
        var center = WaterLandscape.CenterX(z);
        var side = westBank ? -1f : 1f;
        var x =
            center +
            side *
            (WaterLandscape.SurfaceHalfWidth(z) + 2.1f);

        return new Vector3(x, 0f, z);
    }
}
