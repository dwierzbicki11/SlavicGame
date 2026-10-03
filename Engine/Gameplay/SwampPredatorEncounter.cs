using System.Numerics;
using SlavicGame.Engine.AI;
using SlavicGame.Engine.Quest;
using SlavicGame.Engine.World;

namespace SlavicGame.Engine.Gameplay;

public static class SwampPredatorEncounter
{
    public const string Id = "swamp-predator";
    public const string IdentifiedFlag = "swamp.predator.identified";
    public const string KilledFlag = "swamp.predator.killed";

    public static void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var record = world.Progress.Encounters.Get(Id);

        if (world.Progress.HasFlag(VerticalSliceQuestInteractions.PredatorTracksFlag))
            record.Observe();

        var predator = world.Enemies.FirstOrDefault(enemy =>
            string.Equals(enemy.Id, Id, StringComparison.Ordinal));

        if (predator is null)
            return;

        if (!predator.IsAlive)
        {
            MarkKilled(world);
            return;
        }

        var closeEnough = HorizontalDistance(predator.Position, world.PlayerPosition) <= 12f;
        var engaged = predator.State is EnemyState.Alert or EnemyState.Chase or EnemyState.Attack;

        if ((closeEnough || engaged) && record.Identify())
        {
            world.Progress.SetFlag(IdentifiedFlag);
            AddEvidenceOnce(
                world,
                "light-over-swamp.predator-seen",
                KnowledgeKind.Observation,
                "Na mokradle potwierdzono obecność dużego fizycznego drapieżnika.");
        }
    }

    public static void MarkKilled(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var record = world.Progress.Encounters.Get(Id);
        record.Identify();

        if (record.Resolve(EncounterResolution.Killed))
        {
            world.Progress.SetFlag(IdentifiedFlag);
            world.Progress.SetFlag(KilledFlag);
            AddEvidenceOnce(
                world,
                "light-over-swamp.predator-killed",
                KnowledgeKind.ConfirmedFact,
                "Fizyczny drapieżnik z mokradeł został zabity.");
        }
        else if (record.Resolution == EncounterResolution.Killed)
        {
            world.Progress.SetFlag(IdentifiedFlag);
            world.Progress.SetFlag(KilledFlag);
        }
    }

    public static void Synchronize(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var predator = world.Enemies.FirstOrDefault(enemy =>
            string.Equals(enemy.Id, Id, StringComparison.Ordinal));

        if (predator is not null && !predator.IsAlive)
            MarkKilled(world);
    }

    public static bool IsKilled(WorldState world) =>
        world.Progress.Encounters.Get(Id).Resolution == EncounterResolution.Killed;

    private static void AddEvidenceOnce(
        WorldState world,
        string id,
        KnowledgeKind kind,
        string text)
    {
        var quest = world.Progress.Quests.Get(VerticalSliceBootstrap.ContractQuestId);
        if (quest.Evidence.Any(entry => string.Equals(entry.Id, id, StringComparison.Ordinal)))
            return;

        quest.AddEvidence(new EvidenceEntry(
            id,
            quest.Id,
            kind,
            text,
            Id));
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b) =>
        Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
}
