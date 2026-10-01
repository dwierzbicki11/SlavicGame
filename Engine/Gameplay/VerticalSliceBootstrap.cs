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

        world.Progress.SetFlag("vertical-slice-prepared");
    }
}
