using System.Numerics;
using SlavicGame.Engine.NPC;

namespace SlavicGame.Engine.World;

public sealed record NpcWorldActor(
    string Id,
    NpcRole Role,
    string Activity,
    string LocationId,
    Vector3 Position,
    float YawRadians);

public static class NpcPresentation
{
    public static string DisplayName(string id) => id switch
    {
        "missing-family" => "RODZINA ZAGINIONEGO",
        "crossing-keeper" => "OPIEKUN PRZEPRAWY",
        "herbalist" => "ZIELARKA",
        "community-guard" => "STRAZNIK WSPOLNOTY",
        "shrine-keeper" => "OPIEKUN KREGU",
        _ => id.Replace('-', ' ').ToUpperInvariant()
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

            var horizontal = PoseFor(npc.Id, slot.LocationId, slot.Activity);
            var position = new Vector3(horizontal.X, 0f, horizontal.Y);
            position.Y = world.Terrain.SampleHeight(position);

            var yaw = DefaultYaw(npc.Id, slot.LocationId);
            if (world.Dialogue.IsOpen &&
                string.Equals(world.Dialogue.SpeakerId, npc.Id, StringComparison.Ordinal))
            {
                yaw = Facing(position, world.PlayerPosition);
            }

            _actors.Add(new NpcWorldActor(
                npc.Id,
                npc.Role,
                slot.Activity,
                slot.LocationId,
                position,
                yaw));
        }
    }

    public NpcWorldActor? FindNearest(Vector3 position, float maxDistance = 3.8f)
    {
        var bestDistanceSquared = maxDistance * maxDistance;
        NpcWorldActor? best = null;

        foreach (var actor in _actors)
        {
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
        var actor = FindNearest(playerPosition);
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
