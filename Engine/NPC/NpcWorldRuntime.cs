using System.Numerics;
using SlavicGame.Engine.NPC;

namespace SlavicGame.Engine.World;

public sealed record NpcWorldActor(
    string Id,
    NpcRole Role,
    string Activity,
    string LocationId,
    Vector3 Position,
    float YawRadians,
    bool IsMoving);

public static class NpcPresentation
{
    public static string DisplayName(string id) => id switch
    {
        "missing-family" => "RODZINA ZAGINIONEGO",
        "crossing-keeper" => "OPIEKUN PRZEPRAWY",
        "herbalist" => "ZIELARKA",
        "community-guard" => "STRAZNIK WSPOLNOTY",
        "shrine-keeper" => "OPIEKUN KREGU",
        "settler-farmer-01" => "ROLNIK",
        "settler-farmer-02" => "MIESZKANKA WSI",
        "settler-woodworker-01" => "CIESLA",
        "settler-potter-01" => "GARNCARZ",
        "settler-trader-01" => "HANDLARZ",
        "settler-carrier-01" => "TRAGARZ",
        "settler-elder-01" => "STARSZY MIESZKANIEC",
        "settler-traveler-01" => "PODROZNY",
        _ => id.Replace('-', ' ').ToUpperInvariant()
    };

    public static bool HasDialogue(string id) => id switch
    {
        "missing-family" or
        "crossing-keeper" or
        "herbalist" or
        "community-guard" or
        "shrine-keeper" => true,
        _ => false
    };

    public static Vector3 RoleColor(NpcRole role) => role switch
    {
        NpcRole.ContractGiver => new Vector3(0.46f, 0.31f, 0.18f),
        NpcRole.CrossingKeeper => new Vector3(0.28f, 0.36f, 0.22f),
        NpcRole.Herbalist => new Vector3(0.25f, 0.46f, 0.22f),
        NpcRole.CommunityGuard => new Vector3(0.36f, 0.32f, 0.28f),
        NpcRole.ShrineKeeper => new Vector3(0.30f, 0.28f, 0.45f),
        _ => new Vector3(0.34f, 0.30f, 0.24f)
    };
}

public sealed class NpcWorldRuntime
{
    private readonly List<NpcWorldActor> _actors = [];
    private readonly Dictionary<string, Vector3> _lastPositions =
        new(StringComparer.Ordinal);

    public IReadOnlyList<NpcWorldActor> Actors => _actors;

    public void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        _actors.Clear();

        foreach (var npc in world.Npcs)
        {
            var slot = npc.GetSchedule(world.Time.TimeOfDayHours);
            if (slot is null)
                continue;

            var fallback =
                PoseFor(
                    npc.Id,
                    slot.LocationId,
                    slot.Activity);
            var speaking =
                world.Dialogue.IsOpen &&
                string.Equals(
                    world.Dialogue.SpeakerId,
                    npc.Id,
                    StringComparison.Ordinal);

            NpcRoutineSample motion;
            if (speaking &&
                _lastPositions.TryGetValue(
                    npc.Id,
                    out var frozenPosition))
            {
                motion = new NpcRoutineSample(
                    new Vector2(
                        frozenPosition.X,
                        frozenPosition.Z),
                    Vector2.Zero,
                    false);
            }
            else
            {
                motion = NpcRoutineMotion.Sample(
                    npc.Id,
                    slot.Activity,
                    fallback,
                    world.Time.TimeOfDayHours);
            }

            var horizontal =
                world.ResolveHorizontalPosition(
                    motion.Position,
                    0.42f);
            var position =
                new Vector3(
                    horizontal.X,
                    0f,
                    horizontal.Y);
            position.Y =
                world.Terrain.SampleHeight(position);

            var yaw =
                motion.IsMoving &&
                motion.Forward.LengthSquared() > 0.000001f
                    ? WorldPlacementOrientation.YawFacing(
                        horizontal,
                        horizontal + motion.Forward)
                    : DefaultYaw(
                        npc.Id,
                        slot.LocationId);

            if (speaking)
            {
                yaw = Facing(
                    position,
                    world.PlayerPosition);
            }

            _lastPositions[npc.Id] = position;

            _actors.Add(new NpcWorldActor(
                npc.Id,
                npc.Role,
                slot.Activity,
                slot.LocationId,
                position,
                yaw,
                motion.IsMoving && !speaking));
        }
    }

    public NpcWorldActor? FindNearest(Vector3 position, float maxDistance = 3.8f) =>
        FindNearestMatching(position, maxDistance, static _ => true);

    public NpcWorldActor? FindNearestInteractive(
        Vector3 position,
        float maxDistance = 3.8f) =>
        FindNearestMatching(
            position,
            maxDistance,
            static actor => NpcPresentation.HasDialogue(actor.Id));

    private NpcWorldActor? FindNearestMatching(
        Vector3 position,
        float maxDistance,
        Func<NpcWorldActor, bool> predicate)
    {
        var bestDistanceSquared = maxDistance * maxDistance;
        NpcWorldActor? best = null;

        foreach (var actor in _actors)
        {
            if (!predicate(actor))
                continue;

            var delta = actor.Position - position;
            delta.Y = 0f;
            var distanceSquared = delta.LengthSquared();
            if (distanceSquared > bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            best = actor;
        }

        return best;
    }

    public NpcWorldActor? Find(string id) =>
        _actors.FirstOrDefault(actor =>
            string.Equals(actor.Id, id, StringComparison.Ordinal));

    public bool IsNearby(
        string id,
        Vector3 position,
        float maxDistance = 4.25f)
    {
        var actor = Find(id);
        if (actor is null)
            return false;

        var delta = actor.Position - position;
        delta.Y = 0f;
        return delta.LengthSquared() <= maxDistance * maxDistance;
    }

    public string HudPrompt(Vector3 playerPosition)
    {
        var actor = FindNearestInteractive(playerPosition);
        return actor is null
            ? ""
            : $"E POROZMAWIAJ: {NpcPresentation.DisplayName(actor.Id)}";
    }

    private static Vector2 PoseFor(
        string npcId,
        string locationId,
        string activity) =>
        (npcId, locationId, activity) switch
        {
            ("missing-family", "old-village", "home-and-search") => new(-6f, -84f),
            ("missing-family", "old-village", _) => new(-11f, -91f),

            ("crossing-keeper", "black-swamp", _) => new(91f, 41f),
            ("crossing-keeper", "old-village", _) => new(9f, -104f),

            ("herbalist", "old-village", "trade-and-prepare") => new(5f, -92f),
            ("herbalist", "old-village", _) => new(8f, -97f),

            ("community-guard", "old-village", "patrol") => new(1f, -106f),
            ("community-guard", "old-village", _) => new(-2f, -105f),

            ("shrine-keeper", "old-shrine", _) => new(-82f, 58f),
            ("shrine-keeper", "old-village", _) => new(-17f, -82f),

            ("settler-farmer-01", "old-village", "go-to-fields") => new(-25f, -70f),
            ("settler-farmer-01", "old-village", _) => new(-20f, -92f),

            ("settler-farmer-02", "old-village", "field-work") => new(-30f, -76f),
            ("settler-farmer-02", "old-village", _) => new(-24f, -98f),

            ("settler-woodworker-01", "old-village", "wood-work") => new(15f, -97f),
            ("settler-woodworker-01", "old-village", _) => new(20f, -101f),

            ("settler-potter-01", "old-village", "craft-work") => new(-7f, -74f),
            ("settler-potter-01", "old-village", _) => new(-13f, -90f),

            ("settler-trader-01", "old-village", "market-trade") => new(3f, -75f),
            ("settler-trader-01", "old-village", _) => new(8f, -88f),

            ("settler-carrier-01", "old-village", "carry-goods") => new(11f, -82f),
            ("settler-carrier-01", "old-village", _) => new(15f, -95f),

            ("settler-elder-01", "old-village", "village-square") => new(-3f, -84f),
            ("settler-elder-01", "old-village", _) => new(-12f, -86f),

            ("settler-traveler-01", "old-village", "arrive-and-trade") => new(2f, -109f),
            ("settler-traveler-01", "old-village", _) => new(18f, -76f),

            _ => LocationCenter(locationId)
        };

    private static Vector2 LocationCenter(string locationId) => locationId switch
    {
        "old-village" => new(0f, -85f),
        "black-swamp" => new(95f, 35f),
        "old-shrine" => new(-85f, 55f),
        "starting-forest" => Vector2.Zero,
        _ => Vector2.Zero
    };

    private static float DefaultYaw(string npcId, string locationId)
    {
        var target = locationId switch
        {
            "old-village" => new Vector2(0f, -85f),
            "black-swamp" => new Vector2(95f, 35f),
            "old-shrine" => new Vector2(-85f, 55f),
            _ => Vector2.Zero
        };
        var from = PoseFor(npcId, locationId, "");
        return WorldPlacementOrientation.YawFacing(from, target);
    }

    private static float Facing(Vector3 from, Vector3 target) =>
        WorldPlacementOrientation.YawFacing(
            new Vector2(from.X, from.Z),
            new Vector2(target.X, target.Z));
}
