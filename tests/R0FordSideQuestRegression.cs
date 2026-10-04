using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.Reputation;
using SlavicGame.Engine.Save;
using SlavicGame.Engine.World;

internal static class R0FordSideQuestRegression
{
    public static void Run(Action<bool, string> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check(WaterLandscape.ChannelDepth(R0FordSideQuest.FordZ) < 0.35f &&
              WaterLandscape.ChannelDepth(R0FordSideQuest.BypassZ) < 0.45f &&
              WaterLandscape.ChannelDepth(0f) > 1.2f,
            "R0 traversal pass carves two local shallow crossings without flattening the whole river");

        RunRepair(check);
        RunBypass(check);
        RunClosedSafe(check);
    }

    private static void RunRepair(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();
        var quest = world.Progress.Quests.Get(R0FordSideQuest.QuestId);

        check(quest.Phase == QuestPhase.Unavailable &&
              R0FordSideQuest.Outcome(world) == R0FordOutcome.None,
            "SQ_R0_01 starts discoverable in the field without a mandatory giver");

        var blocked = world.ResolveHorizontalPosition(
            new Vector2(
                WaterLandscape.CenterX(R0FordSideQuest.FordZ),
                R0FordSideQuest.FordZ),
            world.PlayerRadius);
        check(MathF.Abs(
                  blocked.X -
                  WaterLandscape.CenterX(R0FordSideQuest.FordZ)) >
              WaterLandscape.SurfaceHalfWidth(R0FordSideQuest.FordZ),
            "Broken direct ford blocks local traversal before a legal outcome");

        InteractAt(world, R0FordSideQuest.FordWestBankSpot);
        check(quest.Phase == QuestPhase.Investigation &&
              world.Progress.HasFlag(R0FordSideQuest.FordInspectedFlag) &&
              world.Progress.Navigation.HasLandmark(R0FordSideQuest.FordLandmarkId) &&
              quest.Evidence.Any(entry =>
                  entry.Id == R0FordSideQuest.FordEvidenceId),
            "Inspecting the damaged ford starts SQ_R0_01 and records navigation/evidence state");

        InteractAt(world, new Vector3(16f, 0f, -96f));
        check(world.Progress.Inventory.Count(R0FordSideQuest.RepairTimberItemId) == 2,
            "Village material stock provides the prototype repair timber");

        var moneyBefore = world.Progress.Profile.Money;
        InteractAt(world, R0FordSideQuest.FordWestBankSpot);

        check(R0FordSideQuest.Outcome(world) == R0FordOutcome.Repaired &&
              quest.Phase == QuestPhase.TurnedIn &&
              quest.RewardClaimed &&
              world.Progress.HasFlag(R0FordSideQuest.AccessFlag) &&
              world.Progress.Inventory.Count(R0FordSideQuest.RepairTimberItemId) == 0 &&
              world.Progress.Profile.Money == moneyBefore + 25 &&
              world.Progress.Reputation.Get(
                  ReputationScope.Village,
                  "old-village") == 6,
            "Repair consumes materials and commits the best direct-access outcome once");

        var open = world.ResolveHorizontalPosition(
            new Vector2(
                WaterLandscape.CenterX(R0FordSideQuest.FordZ),
                R0FordSideQuest.FordZ),
            world.PlayerRadius);
        check(MathF.Abs(
                  open.X -
                  WaterLandscape.CenterX(R0FordSideQuest.FordZ)) < 0.01f,
            "Repaired outcome opens the shallow direct ford");

        var brokenVisual = FindVisual("visual.side.sq-r0-01.broken-ford");
        var repairedVisual = FindVisual("visual.side.sq-r0-01.repaired-ford");
        check(!WorldItemVisualCatalog.IsVisible(world, brokenVisual) &&
              WorldItemVisualCatalog.IsVisible(world, repairedVisual),
            "Repair swaps broken-ford presentation to the repaired crossing");

        var json = SaveGameService.Serialize(world);
        var restored = WorldGenerator.Generate();
        SaveGameService.Restore(restored, json);

        check(R0FordSideQuest.Outcome(restored) == R0FordOutcome.Repaired &&
              restored.Progress.Navigation.HasLandmark(R0FordSideQuest.FordLandmarkId) &&
              restored.Progress.Profile.Money == moneyBefore + 25 &&
              R0FordSideQuest.DirectFordOpen(restored),
            "Repaired ford traversal/outcome/reward survive save-load without recomputation");
    }

    private static void RunBypass(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();
        var quest = world.Progress.Quests.Get(R0FordSideQuest.QuestId);

        InteractAt(world, R0FordSideQuest.FordWestBankSpot);
        InteractAt(world, R0FordSideQuest.BypassWestBankSpot);

        check(world.Progress.HasFlag(R0FordSideQuest.BypassDiscoveredFlag) &&
              world.Progress.Navigation.HasLandmark(R0FordSideQuest.BypassLandmarkId) &&
              quest.Evidence.Any(entry =>
                  entry.Id == R0FordSideQuest.BypassEvidenceId),
            "Field exploration can discover a safe alternative without repair materials");

        var moneyBefore = world.Progress.Profile.Money;
        InteractAt(world, R0FordSideQuest.BypassWestBankSpot);

        check(R0FordSideQuest.Outcome(world) == R0FordOutcome.Bypass &&
              world.Progress.Profile.Money == moneyBefore + 15 &&
              R0FordSideQuest.BypassOpen(world) &&
              !R0FordSideQuest.DirectFordOpen(world),
            "Bypass outcome opens only the alternate shallow crossing");

        var direct = world.ResolveHorizontalPosition(
            new Vector2(
                WaterLandscape.CenterX(R0FordSideQuest.FordZ),
                R0FordSideQuest.FordZ),
            world.PlayerRadius);
        var bypass = world.ResolveHorizontalPosition(
            new Vector2(
                WaterLandscape.CenterX(R0FordSideQuest.BypassZ),
                R0FordSideQuest.BypassZ),
            world.PlayerRadius);

        check(MathF.Abs(
                  direct.X -
                  WaterLandscape.CenterX(R0FordSideQuest.FordZ)) >
              WaterLandscape.SurfaceHalfWidth(R0FordSideQuest.FordZ) &&
              MathF.Abs(
                  bypass.X -
                  WaterLandscape.CenterX(R0FordSideQuest.BypassZ)) < 0.01f,
            "Bypass leaves the damaged ford closed while opening the marked alternative");

        check(WorldItemVisualCatalog.IsVisible(
                  world,
                  FindVisual("visual.side.sq-r0-01.bypass-marker")),
            "Bypass outcome persists a visible navigation marker");
    }

    private static void RunClosedSafe(Action<bool, string> check)
    {
        var world = WorldGenerator.Generate();

        InteractAt(world, R0FordSideQuest.FordWestBankSpot);
        InteractAt(world, R0FordSideQuest.BypassWestBankSpot);
        var moneyBefore = world.Progress.Profile.Money;
        InteractAt(world, R0FordSideQuest.CloseMarkerSpot);

        check(R0FordSideQuest.Outcome(world) == R0FordOutcome.ClosedSafe &&
              world.Progress.Profile.Money == moneyBefore + 10 &&
              world.Progress.Reputation.Get(
                  ReputationScope.Village,
                  "old-village") == 3 &&
              R0FordSideQuest.BypassOpen(world) &&
              !R0FordSideQuest.DirectFordOpen(world),
            "Closed-safe outcome preserves the damaged ford but guarantees an alternate route");
    }

    private static void InteractAt(
        WorldState world,
        Vector3 position)
    {
        world.SetPlayerPosition(position);
        world.EnvironmentInteractions.Update(world);

        if (!world.EnvironmentInteractions.TryInteract(world))
        {
            throw new Exception(
                $"No SQ_R0_01 interaction at {position.X:0.0},{position.Z:0.0}: " +
                world.EnvironmentInteractions.HudText);
        }
    }

    private static WorldItemVisualDefinition FindVisual(string id) =>
        WorldItemVisualCatalog.Definitions.Single(item =>
            string.Equals(item.Id, id, StringComparison.Ordinal));
}
