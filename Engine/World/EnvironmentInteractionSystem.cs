using System.Numerics;
using SlavicGame.Engine.Interaction;
using SlavicGame.Engine.Gameplay;

namespace SlavicGame.Engine.World;

public sealed class EnvironmentInteractionSystem
{
    public const string ResinCollectedFlag =
        "resource.forest-resin.first-collected";
    public const string MarshHerbCollectedFlag =
        "resource.marsh-herb.first-collected";

    private const float InteractionDistance = 3.6f;
    private static readonly Vector3 ResinSpot = new(31f, 0f, 15f);
    private static readonly Vector3 MarshHerbSpot = new(107f, 0f, 20f);
    private static readonly Vector3 MissingToolSpot = new(22f, 0f, 17f);
    private static readonly Vector3 MissingToolsWorksiteSpot = new(18.5f, 0f, 15.5f);
    private static readonly Vector3 FordRepairTimberSpot = new(16f, 0f, -96f);

    public InteractionTarget? Current { get; private set; }
    public string Message { get; private set; } = "";
    public string HudText => Current?.Prompt ?? Message;

    public void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        Current = FindNearest(world);
    }

    public bool TryInteract(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        Current = FindNearest(world);
        if (Current is null)
            return false;

        if (Current.Id == "resource.forest-resin")
        {
            if (world.Progress.HasFlag(ResinCollectedFlag))
                return false;

            world.Progress.Inventory.Add("forest-resin", 3);
            world.Progress.SetFlag(ResinCollectedFlag);
            Message = "ZDOBYTO: ZYWICA LESNA x3";
            Current = FindNearest(world);
            return true;
        }

        if (Current.Id == "resource.marsh-herb")
        {
            if (world.Progress.HasFlag(MarshHerbCollectedFlag))
                return false;

            world.Progress.Inventory.Add("marsh-herb", 2);
            world.Progress.SetFlag(MarshHerbCollectedFlag);
            Message = "ZDOBYTO: ZIOLO BAGIENNE x2";
            Current = FindNearest(world);
            return true;
        }

        if (Current.Id == "side-r0-missing-tools.tool")
        {
            var collected = MissingToolsSideQuest.RecordToolFound(world);
            if (collected)
            {
                Message = "ODNALEZIONO: SIEKIERA CIESLI";
                Current = FindNearest(world);
            }
            return collected;
        }

        if (Current.Id == "side-r0-missing-tools.worksite")
        {
            var inspected = MissingToolsSideQuest.RecordWorksiteContext(world);
            if (inspected)
            {
                Message = "SLAD: NARZEDZIE ZOSTAWIONO PRZY NIEDOKONCZONEJ PRACY";
                Current = FindNearest(world);
            }
            return inspected;
        }

        if (Current.Id == "sq-r0-01.inspect-ford")
        {
            var changed = R0FordSideQuest.InspectFord(world);
            if (changed)
                Message = "ZLAMANY BROD: PRZEJSCIE WYMAGA NAPRAWY ALBO OBEJSCIA";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.take-timber")
        {
            var changed = R0FordSideQuest.CollectRepairTimber(world);
            if (changed)
                Message = "ZDOBYTO: BELKI DO NAPRAWY x2";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.inspect-bypass")
        {
            var changed = R0FordSideQuest.DiscoverBypass(world);
            if (changed)
                Message = "ODKRYTO: PLYTSZE OBEJSCIE";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.repair")
        {
            var changed = R0FordSideQuest.Resolve(
                world,
                R0FordOutcome.Repaired);
            if (changed)
                Message = "BROD NAPRAWIONY / BEZPOSREDNIE PRZEJSCIE OTWARTE";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.mark-bypass")
        {
            var changed = R0FordSideQuest.Resolve(
                world,
                R0FordOutcome.Bypass);
            if (changed)
                Message = "OBEJSCIE OZNACZONE / ALTERNATYWNE PRZEJSCIE OTWARTE";
            Current = FindNearest(world);
            return changed;
        }

        if (Current.Id == "sq-r0-01.close-ford")
        {
            var changed = R0FordSideQuest.Resolve(
                world,
                R0FordOutcome.ClosedSafe);
            if (changed)
                Message = "USZKODZONY BROD ZAMKNIETY / BEZPIECZNA TRASA OZNACZONA";
            Current = FindNearest(world);
            return changed;
        }

        const string harvestPrefix = "wildlife.harvest.";
        if (Current.Id.StartsWith(
                harvestPrefix,
                StringComparison.Ordinal))
        {
            var wildlifeId =
                Current.Id[harvestPrefix.Length..];
            var actor =
                world.Wildlife.Actors.FirstOrDefault(item =>
                    string.Equals(
                        item.Id,
                        wildlifeId,
                        StringComparison.Ordinal));

            if (actor is null ||
                !world.Wildlife.TryHarvest(
                    world,
                    wildlifeId))
            {
                return false;
            }

            var lootText = string.Join(
                " / ",
                WildlifeHarvestCatalog.For(actor.Species)
                    .Select(item =>
                        $"{item.ItemId.ToUpperInvariant()} x{item.Quantity}"));

            Message =
                $"ZEBRANO LUP: {WildlifeCatalog.DisplayName(actor.Species)} / {lootText}";
            Current = FindNearest(world);
            return true;
        }

        const string prefix = "campfire.";
        if (Current.Id.StartsWith(prefix, StringComparison.Ordinal))
        {
            var fireId = Current.Id[prefix.Length..];
            if (world.Campfires.IsLit(world, fireId))
            {
                var changed = world.Campfires.Extinguish(
                    world,
                    fireId,
                    "OGNISKO ZGASZONE");
                Message = world.Campfires.Message;
                Current = FindNearest(world);
                return changed;
            }

            var lit = world.Campfires.TryLight(world, fireId);
            Message = world.Campfires.Message;
            Current = FindNearest(world);
            return lit;
        }

        return false;
    }

    private InteractionTarget? FindNearest(WorldState world)
    {
        var targets = new List<InteractionTarget>();

        if (!world.Progress.HasFlag(ResinCollectedFlag))
        {
            targets.Add(new InteractionTarget(
                "resource.forest-resin",
                Ground(world, ResinSpot),
                InteractionKind.Take,
                "E ZBIERZ ZYWICE LESNA"));
        }

        if (!world.Progress.HasFlag(MarshHerbCollectedFlag))
        {
            targets.Add(new InteractionTarget(
                "resource.marsh-herb",
                Ground(world, MarshHerbSpot),
                InteractionKind.Take,
                "E ZBIERZ ZIOLO BAGIENNE"));
        }

        if (MissingToolsSideQuest.CanInvestigate(world))
        {
            if (!world.Progress.HasFlag(MissingToolsSideQuest.ToolCollectedFlag))
            {
                targets.Add(new InteractionTarget(
                    "side-r0-missing-tools.tool",
                    Ground(world, MissingToolSpot),
                    InteractionKind.Take,
                    "E PODNIES SIEKIERE CIESLI"));
            }

            if (!world.Progress.HasFlag(MissingToolsSideQuest.WorksiteInspectedFlag))
            {
                targets.Add(new InteractionTarget(
                    "side-r0-missing-tools.worksite",
                    Ground(world, MissingToolsWorksiteSpot),
                    InteractionKind.Inspect,
                    "E OBEJRZYJ MIEJSCE PRACY"));
            }
        }

        if (R0FordSideQuest.Outcome(world) == R0FordOutcome.None)
        {
            if (!world.Progress.HasFlag(R0FordSideQuest.FordInspectedFlag))
            {
                targets.Add(new InteractionTarget(
                    "sq-r0-01.inspect-ford",
                    Ground(world, R0FordSideQuest.FordWestBankSpot),
                    InteractionKind.Inspect,
                    "E OBEJRZYJ USZKODZONY BROD"));
            }
            else
            {
                if (!world.Progress.HasFlag(
                        R0FordSideQuest.RepairTimberCollectedFlag))
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.take-timber",
                        Ground(world, FordRepairTimberSpot),
                        InteractionKind.Take,
                        "E WEZ BELKI DO NAPRAWY BRODU"));
                }

                if (world.Progress.Inventory.Contains(
                        R0FordSideQuest.RepairTimberItemId,
                        2))
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.repair",
                        Ground(world, R0FordSideQuest.FordWestBankSpot),
                        InteractionKind.Use,
                        "E NAPRAW BROD"));
                }

                if (!world.Progress.HasFlag(
                        R0FordSideQuest.BypassDiscoveredFlag))
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.inspect-bypass",
                        Ground(world, R0FordSideQuest.BypassWestBankSpot),
                        InteractionKind.Inspect,
                        "E SPRAWDZ PLYTSZE OBEJSCIE"));
                }
                else
                {
                    targets.Add(new InteractionTarget(
                        "sq-r0-01.mark-bypass",
                        Ground(world, R0FordSideQuest.BypassWestBankSpot),
                        InteractionKind.Activate,
                        "E OZNACZ OBEJSCIE JAKO TRASE"));

                    targets.Add(new InteractionTarget(
                        "sq-r0-01.close-ford",
                        Ground(world, R0FordSideQuest.CloseMarkerSpot),
                        InteractionKind.Activate,
                        "E ZAMKNIJ BROD I OZNACZ OBEJSCIE"));
                }
            }
        }

        var carcass =
            world.Wildlife.FindNearestHarvestable(
                world.PlayerPosition,
                InteractionDistance);
        if (carcass is not null)
        {
            targets.Add(new InteractionTarget(
                $"wildlife.harvest.{carcass.Id}",
                carcass.Position,
                InteractionKind.Take,
                $"E ZBIERZ LUP: {WildlifeCatalog.DisplayName(carcass.Species)}"));
        }

        foreach (var fire in CampfireSystem.Fires)
        {
            var position = Ground(
                world,
                new Vector3(
                    fire.Position.X,
                    0f,
                    fire.Position.Y));

            var lit = world.Campfires.IsLit(world, fire.Id);
            string prompt;
            if (lit)
            {
                prompt = "E ZGAS OGNISKO";
            }
            else if (!string.IsNullOrWhiteSpace(fire.IgnitionItem) &&
                     !world.Progress.Inventory.Contains(fire.IgnitionItem))
            {
                prompt = "E OGNISKO / POTRZEBNA ZYWICA";
            }
            else
            {
                prompt = "E ROZPAL OGNISKO";
            }

            targets.Add(new InteractionTarget(
                $"campfire.{fire.Id}",
                position,
                InteractionKind.Activate,
                prompt));
        }

        return InteractionSystem.FindNearest(
            world.PlayerPosition,
            targets,
            InteractionDistance);
    }

    private static Vector3 Ground(
        WorldState world,
        Vector3 position)
    {
        position.Y = world.Terrain.SampleHeight(position);
        return position;
    }
}
