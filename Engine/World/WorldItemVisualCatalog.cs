using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;

namespace SlavicGame.Engine.World;

public sealed record WorldItemVisualDefinition(
    string Id,
    string ItemId,
    string AssetPath,
    Vector3 Position,
    Vector3 Scale,
    float YawRadians,
    Vector3 Color,
    float YOffset = 0f);

public static class WorldItemVisualCatalog
{
    public static IReadOnlyList<WorldItemVisualDefinition> Definitions { get; } =
    [
        new(
            "visual.quest.keepsake",
            "missing-person-keepsake",
            "models/static/amulet_kosciany_r0_01.glb",
            new Vector3(82f, 0f, 29f),
            new Vector3(0.72f),
            0.42f,
            new Vector3(0.68f, 0.55f, 0.36f),
            0.055f),

        new(
            "visual.resource.resin",
            "forest-resin",
            "models/static/kamien_dowodowy_r0_01.glb",
            new Vector3(31f, 0f, 15f),
            new Vector3(0.38f, 0.26f, 0.38f),
            -0.18f,
            new Vector3(1.45f, 0.66f, 0.10f),
            0.10f),

        new(
            "visual.resource.marsh-herb",
            "marsh-herb",
            "models/static/peczek_ziol_r0_01.glb",
            new Vector3(107f, 0f, 20f),
            new Vector3(0.82f),
            0.74f,
            new Vector3(0.34f, 0.62f, 0.21f),
            0.045f),

        new(
            "visual.quest.ritual-thread",
            "ritual-thread",
            "models/static/rope_coil_r0_01.glb",
            new Vector3(6.5f, 0f, -91f),
            new Vector3(0.58f),
            -0.30f,
            new Vector3(0.66f, 0.48f, 0.28f),
            0.055f)
    ];

    public static bool IsVisible(
        WorldState world,
        WorldItemVisualDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(definition);

        return definition.Id switch
        {
            "visual.quest.keepsake" =>
                KeepsakeVisible(world),

            "visual.resource.resin" =>
                !world.Progress.HasFlag(
                    EnvironmentInteractionSystem.ResinCollectedFlag),

            "visual.resource.marsh-herb" =>
                !world.Progress.HasFlag(
                    EnvironmentInteractionSystem.MarshHerbCollectedFlag),

            "visual.quest.ritual-thread" =>
                RitualThreadVisible(world),

            _ => false
        };
    }

    private static bool KeepsakeVisible(
        WorldState world)
    {
        var quest =
            world.Progress.Quests.Get(
                VerticalSliceBootstrap.ContractQuestId);

        return (quest.Phase is
                    QuestPhase.Active or
                    QuestPhase.Investigation) &&
               !world.Progress.HasFlag(
                   VerticalSliceQuestInteractions.KeepsakeCollectedFlag) &&
               !world.Progress.Inventory.Contains(
                   "missing-person-keepsake");
    }

    private static bool RitualThreadVisible(
        WorldState world)
    {
        var quest =
            world.Progress.Quests.Get(
                VerticalSliceBootstrap.ContractQuestId);

        return quest.Phase ==
                   QuestPhase.Preparation &&
               world.Progress.HasFlag(
                   VerticalSliceRituals.LearnedFlag) &&
               !world.Progress.HasFlag(
                   VerticalSliceQuestInteractions.ThreadReceivedFlag) &&
               !world.Progress.Inventory.Contains(
                   "ritual-thread");
    }
}
