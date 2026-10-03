using System.Numerics;
using SlavicGame.Engine.Gameplay;
using SlavicGame.Engine.Magic;
using SlavicGame.Engine.Quest;

namespace SlavicGame.Engine.World;

public sealed class SwampApparitionRuntime
{
    public static readonly Vector3 SwampAnchor =
        new(78f, 0f, 24f);

    private const float ActivationDistance = 58f;
    private const float KeepsakeResponseDistance = 22f;

    public float Materialization { get; private set; }
    public float KeepsakeResponse { get; private set; }
    public Vector3 Position { get; private set; }
    public bool IsVisible => Materialization > 0.025f;
    public bool IsReactingToKeepsake => KeepsakeResponse > 0.10f;

    public void Reset(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);

        Materialization = 0f;
        KeepsakeResponse = 0f;
        Position = GroundedAnchor(world);
    }

    public void Update(
        WorldState world,
        double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        var quest =
            world.Progress.Quests.Get(
                VerticalSliceBootstrap.ContractQuestId);
        var released =
            world.Progress.HasFlag(
                VerticalSliceRituals.ReleasedFlag);

        var ritualManifest =
            world.Rituals.IsPerforming ||
            world.Rituals.CompletionGlowRemaining > 0d;

        var anchor = GroundedAnchor(world);
        var playerDistance =
            HorizontalDistance(
                world.PlayerPosition,
                anchor);

        var questActive =
            quest.Phase is
                QuestPhase.Active or
                QuestPhase.Investigation or
                QuestPhase.Preparation or
                QuestPhase.Encounter;

        var nightVisible =
            IsNight(world.Time.TimeOfDayHours) &&
            questActive &&
            playerDistance <= ActivationDistance;

        var targetMaterialization =
            !released && (nightVisible || ritualManifest)
                ? 1f
                : 0f;

        var materialResponse =
            targetMaterialization > Materialization
                ? 2.4f
                : released
                    ? 1.15f
                    : 1.8f;

        Materialization =
            MoveExp(
                Materialization,
                targetMaterialization,
                materialResponse,
                deltaSeconds);

        var hasKeepsake =
            world.Progress.Inventory.Contains(
                "missing-person-keepsake");
        var closeEnoughForKeepsake =
            HorizontalDistance(
                world.PlayerPosition,
                anchor) <=
            KeepsakeResponseDistance;

        var targetResponse =
            !released &&
            !ritualManifest &&
            nightVisible &&
            hasKeepsake &&
            closeEnoughForKeepsake
                ? 1f
                : 0f;

        KeepsakeResponse =
            MoveExp(
                KeepsakeResponse,
                targetResponse,
                targetResponse > KeepsakeResponse
                    ? 2.8f
                    : 1.5f,
                deltaSeconds);

        if (ritualManifest)
        {
            var ritualPosition =
                world.Rituals.VisualOrigin;
            ritualPosition.Y =
                world.Terrain.SampleHeight(
                    ritualPosition) +
                0.12f;
            Position = ritualPosition;
            return;
        }

        Position =
            RespondToKeepsake(
                world,
                anchor,
                KeepsakeResponse);
    }

    private static Vector3 RespondToKeepsake(
        WorldState world,
        Vector3 anchor,
        float response)
    {
        if (response <= 0.001f)
            return anchor;

        var from =
            new Vector2(
                anchor.X,
                anchor.Z);
        var player =
            new Vector2(
                world.PlayerPosition.X,
                world.PlayerPosition.Z);
        var delta = player - from;
        if (delta.LengthSquared() < 0.001f)
            return anchor;

        var distance = delta.Length();
        var direction = delta / distance;
        var approachDistance =
            MathF.Max(0f, distance - 3.6f);
        var desired =
            from +
            direction *
            approachDistance *
            0.72f;

        var position2 =
            Vector2.Lerp(
                from,
                desired,
                Math.Clamp(response, 0f, 1f));

        var position =
            new Vector3(
                position2.X,
                0f,
                position2.Y);
        position.Y =
            world.Terrain.SampleHeight(position) +
            0.12f;
        return position;
    }

    private static Vector3 GroundedAnchor(
        WorldState world)
    {
        var anchor = SwampAnchor;
        anchor.Y =
            world.Terrain.SampleHeight(anchor) +
            0.12f;
        return anchor;
    }

    private static bool IsNight(double hour) =>
        hour >= 20.0 || hour < 6.0;

    private static float HorizontalDistance(
        Vector3 a,
        Vector3 b) =>
        Vector2.Distance(
            new Vector2(a.X, a.Z),
            new Vector2(b.X, b.Z));

    private static float MoveExp(
        float current,
        float target,
        float response,
        double deltaSeconds)
    {
        var blend =
            1f -
            MathF.Exp(
                -response *
                (float)Math.Max(
                    0d,
                    deltaSeconds));
        return current +
               (target - current) *
               blend;
    }
}
