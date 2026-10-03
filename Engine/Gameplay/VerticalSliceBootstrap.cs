using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public static class VerticalSliceBootstrap
{
    public const string ContractQuestId = "light-over-swamp";
    public const string SwampBoundaryId = "black-swamp-leak";

    public static void Apply(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var quest = world.Progress.Quests.Get(ContractQuestId);
        if (quest.Phase == QuestPhase.Unavailable)
        {
            quest.SetPhase(QuestPhase.Offered);
        }

        if (world.Cosmology.Find(SwampBoundaryId) is null)
        {
            world.Cosmology.Add(new BoundaryPhenomenon(
                SwampBoundaryId,
                "black-swamp",
                BoundaryState.Leaking,
                20.0,
                6.0));
        }

        EnsureMagicTraces(world);
        if (!world.Progress.HasFlag("starter-loadout-granted"))
        {
            world.Progress.Inventory.Add("simple-bandage", 2);
            world.Progress.SetFlag("starter-loadout-granted");
        }
        world.Progress.SetFlag("vertical-slice-prepared");
    }
    public static void EnsureMagicTraces(WorldState world)
    {
        RegisterMagicTrace(world, "trace.shrine-echo", new System.Numerics.Vector3(-85, 0, 55));
        RegisterMagicTrace(world, "trace.swamp-echo", new System.Numerics.Vector3(80, 0, 28));
    }

    private static void RegisterMagicTrace(WorldState world, string id, System.Numerics.Vector3 position)
    {
        if (world.Progress.Tracking.Tracks.Any(t => t.Id == id)) return;
        position.Y = world.Terrain.SampleHeight(position) + 0.8f;
        world.Progress.Tracking.Register(id, TrackCategory.SupernaturalTrace, position,
            TrackFreshness.Old, id + ".evidence", magicSignature: "echo");
    }
}
