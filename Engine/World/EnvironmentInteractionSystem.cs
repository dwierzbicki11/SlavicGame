using System.Numerics;
using SlavicGame.Engine.Interaction;

namespace SlavicGame.Engine.World;

public sealed class EnvironmentInteractionSystem
{
    public const string ResinCollectedFlag =
        "resource.forest-resin.first-collected";

    private const float InteractionDistance = 3.6f;
    private static readonly Vector3 ResinSpot = new(31f, 0f, 15f);

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
