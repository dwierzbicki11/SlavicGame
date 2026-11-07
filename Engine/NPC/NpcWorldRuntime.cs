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
    bool IsMoving,
    NpcReactionKind Reaction = NpcReactionKind.None);

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
        "settler-smith-helper-01" => "POMOCNIK KOWALA",
        "settler-weaver-01" => "TKACZKA",
        "settler-shepherd-01" => "PASTERZ",
        "settler-gatherer-01" => "ZBIERACZKA",
        "settler-fisher-01" => "RYBAK",
        "settler-youth-01" => "MLODY MIESZKANIEC",
        "stronghold-chieftain" => "KNIAZ BORZYWOJ",
        "stronghold-guard" => "WOJOMIR STRAZNIK",
        "stronghold-smith" => "KOWAL MSCIWOJ",
        "stronghold-trader" => "KUPIEC DOBROMIR",
        "fishing-elder" => "RYBAK SAMBOR",
        "fishing-ferryman" => "PRZEWOZNIK CHOCIMIR",
        "smolnik-woodcutter" => "DRWAL DRZEWOMYSL",
        "smolnik-burner" => "SMOLNIK ZELISLAW",
        "caravan-merchant" => "KUPIEC WITOSZ",
        _ => id.Replace('-', ' ').ToUpperInvariant()
    };

    public static bool HasDialogue(string id) =>
        id.StartsWith("settler-", StringComparison.Ordinal) ||
        id.StartsWith("stronghold-", StringComparison.Ordinal) ||
        id.StartsWith("fishing-", StringComparison.Ordinal) ||
        id.StartsWith("smolnik-", StringComparison.Ordinal) ||
        id.StartsWith("caravan-", StringComparison.Ordinal) ||
        id switch
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
    private readonly Dictionary<string, double> _routineTimeOffsets =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, double> _dialogueFreezeStarted =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, double> _lastRoutineWorldHours =
        new(StringComparer.Ordinal);
    private double _lastSampledWorldHours = double.NaN;

    public IReadOnlyList<NpcWorldActor> Actors => _actors;

    public void Update(WorldState world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var dtHours = double.IsNaN(_lastSampledWorldHours)
            ? 1.0
            : ForwardHours(_lastSampledWorldHours, world.Time.TimeOfDayHours);
        _lastSampledWorldHours = world.Time.TimeOfDayHours;
        var smoothStepActive = dtHours > 0.000001 && dtHours < 0.04;
        var maxStepDistance = smoothStepActive
            ? MathF.Max(0.015f, (float)(dtHours * 90.0))
            : float.MaxValue;

        _actors.Clear();

        foreach (var npc in world.Npcs)
        {
            var slot = npc.GetSchedule(world.Time.TimeOfDayHours);
            if (slot is null)
                continue;

            var effectiveActivity =
                NpcSituationalBehavior.ResolveActivity(
                    world,
                    npc,
                    slot);

            var fallback =
                PoseFor(
                    npc.Id,
                    slot.LocationId,
                    effectiveActivity);
            var speaking =
                world.Dialogue.IsOpen &&
                string.Equals(
                    world.Dialogue.SpeakerId,
                    npc.Id,
                    StringComparison.Ordinal);

            var routineOffset =
                _routineTimeOffsets.GetValueOrDefault(npc.Id);

            NpcRoutineSample motion;
            if (speaking &&
                _lastPositions.TryGetValue(
                    npc.Id,
                    out var frozenPosition))
            {
                if (!_dialogueFreezeStarted.ContainsKey(npc.Id))
                {
                    _dialogueFreezeStarted[npc.Id] =
                        _lastRoutineWorldHours.GetValueOrDefault(
                            npc.Id,
                            world.Time.TimeOfDayHours);
                }

                motion = new NpcRoutineSample(
                    new Vector2(
                        frozenPosition.X,
                        frozenPosition.Z),
                    Vector2.Zero,
                    false);
            }
            else
            {
                if (_dialogueFreezeStarted.Remove(
                        npc.Id,
                        out var frozenAtHours))
                {
                    routineOffset += ForwardHours(
                        frozenAtHours,
                        world.Time.TimeOfDayHours);
                    _routineTimeOffsets[npc.Id] =
                        routineOffset;
                }

                var routineTime =
                    world.Time.TimeOfDayHours -
                    routineOffset;

                motion = NpcRoutineMotion.Sample(
                    npc.Id,
                    effectiveActivity,
                    fallback,
                    routineTime);

                if (NpcWorkstationCatalog.TrySample(
                        npc.Id,
                        effectiveActivity,
                        routineTime,
                        out var workstationMotion))
                {
                    motion = workstationMotion;
                }

                _lastRoutineWorldHours[npc.Id] =
                    world.Time.TimeOfDayHours;
            }

            var reaction = NpcReactionSystem.Resolve(
                world,
                npc,
                motion.Position,
                motion.Forward,
                motion.IsMoving,
                speaking);

            var horizontal =
                world.ResolveHorizontalPosition(
                    reaction.Position,
                    0.42f);
            var isMoving = reaction.IsMoving && !speaking;
            var forward = reaction.Forward;

            if (smoothStepActive &&
                !speaking &&
                _lastPositions.TryGetValue(npc.Id, out var prevPos))
            {
                var prevHorizontal = new Vector2(prevPos.X, prevPos.Z);
                var delta = horizontal - prevHorizontal;
                var dist = delta.Length();
                if (dist > maxStepDistance && dist < 45f)
                {
                    var dir = delta / dist;
                    horizontal = world.ResolveHorizontalPosition(
                        prevHorizontal + dir * maxStepDistance,
                        0.42f);
                    forward = dir;
                    isMoving = true;
                }
            }

            var position =
                new Vector3(
                    horizontal.X,
                    0f,
                    horizontal.Y);
            position.Y =
                world.Terrain.SampleHeight(position);

            var yaw =
                forward.LengthSquared() > 0.000001f
                    ? WorldPlacementOrientation.YawFacing(
                        horizontal,
                        horizontal + forward)
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
                effectiveActivity,
                slot.LocationId,
                position,
                yaw,
                isMoving,
                reaction.Kind));
        }

        NpcCrowdSteering.Resolve(
            world,
            _actors,
            world.Dialogue.IsOpen
                ? world.Dialogue.SpeakerId
                : null);

        foreach (var actor in _actors)
            _lastPositions[actor.Id] = actor.Position;
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
            ("missing-family", "old-village", "home-and-search") => new(-5.5f, -83.5f),
            ("missing-family", "old-village", _) => new(-10f, -82.8f),

            ("crossing-keeper", "black-swamp", _) => new(94.2f, 40.8f),
            ("crossing-keeper", "old-village", _) => new(9f, -104f),

            ("herbalist", "old-village", "trade-and-prepare") => new(5.0f, -92.0f),
            ("herbalist", "old-village", _) => new(8.2f, -91.8f),

            ("community-guard", "old-village", "patrol") => new(0.5f, -105f),
            ("community-guard", "old-village", _) => new(-1.5f, -105f),

            ("shrine-keeper", "old-shrine", _) => new(-82f, 58f),
            ("shrine-keeper", "old-village", _) => new(-15f, -82f),

            ("settler-farmer-01", "old-village", "go-to-fields") => new(-29.0f, -69.4f),
            ("settler-farmer-01", "old-village", _) => new(-20f, -93.5f),

            ("settler-farmer-02", "old-village", "field-work") => new(-34.1f, -67.2f),
            ("settler-farmer-02", "old-village", _) => new(-19.2f, -97.5f),

            ("settler-woodworker-01", "old-village", "wood-work") => new(15.2f, -97.2f),
            ("settler-woodworker-01", "old-village", _) => new(17.5f, -95.0f),

            ("settler-potter-01", "old-village", "craft-work") => new(-6.7f, -74.8f),
            ("settler-potter-01", "old-village", _) => new(-5.2f, -89.5f),

            ("settler-trader-01", "old-village", "market-trade") => new(2.7f, -75.2f),
            ("settler-trader-01", "old-village", _) => new(8.0f, -88.0f),

            ("settler-carrier-01", "old-village", "carry-goods") => new(10f, -86f),
            ("settler-carrier-01", "old-village", _) => new(12.5f, -94.5f),

            ("settler-elder-01", "old-village", "village-square") => new(-3f, -84f),
            ("settler-elder-01", "old-village", _) => new(-9.5f, -83.2f),

            ("settler-traveler-01", "old-village", "arrive-and-trade") => new(0.8f, -109f),
            ("settler-traveler-01", "old-village", _) => new(17.8f, -75.5f),

            ("settler-smith-helper-01", "old-village", "forge-work") => new(19.6f, -87.2f),
            ("settler-smith-helper-01", "old-village", _) => new(17.5f, -88.5f),

            ("settler-weaver-01", "old-village", "weave-work") => new(-16.4f, -76.5f),
            ("settler-weaver-01", "old-village", _) => new(-15.2f, -80.0f),

            ("settler-shepherd-01", "old-village", "drive-flock") => new(21.5f, -74f),
            ("settler-shepherd-01", "old-village", "graze-flock") => new(29f, -64f),
            ("settler-shepherd-01", "old-village", _) => new(21.5f, -76.0f),

            ("settler-gatherer-01", "old-village", "gather-herbs") => new(-34f, -56f),
            ("settler-gatherer-01", "old-village", "sort-herbs") => new(7.0f, -90.0f),
            ("settler-gatherer-01", "old-village", _) => new(10.8f, -95.5f),

            ("settler-fisher-01", "black-swamp", "river-fishing") => new(111f, 27f),
            ("settler-fisher-01", "old-village", "mend-nets") => new(20.0f, -75.3f),
            ("settler-fisher-01", "old-village", _) => new(21.0f, -82.5f),

            ("settler-youth-01", "old-village", "run-errands") => new(-1f, -84f),
            ("settler-youth-01", "old-village", _) => new(4.5f, -91.8f),

            ("stronghold-chieftain", "grod-borzywoj", "court") => new(-320f, 227f),
            ("stronghold-chieftain", "grod-borzywoj", "feast") => new(-318f, 224f),
            ("stronghold-chieftain", "grod-borzywoj", _) => new(-320f, 233f),

            ("stronghold-guard", "grod-borzywoj", "gate-watch") => new(-318f, 191f),
            ("stronghold-guard", "grod-borzywoj", "patrol") => new(-326f, 190f),
            ("stronghold-guard", "grod-borzywoj", _) => new(-318f, 191f),

            ("stronghold-smith", "grod-borzywoj", "forge-work") => new(-296f, 217f),
            ("stronghold-smith", "grod-borzywoj", _) => new(-300f, 222f),

            ("stronghold-trader", "grod-borzywoj", "market-trade") => new(-322f, 214f),
            ("stronghold-trader", "grod-borzywoj", _) => new(-328f, 210f),

            ("fishing-elder", "przystan-rybacka", "pier-fishing") => new(164f, -171f),
            ("fishing-elder", "przystan-rybacka", "mend-nets") => new(145f, -178f),
            ("fishing-elder", "przystan-rybacka", _) => new(142f, -172f),

            ("fishing-ferryman", "przystan-rybacka", "bridge-watch") => new(168f, -180f),
            ("fishing-ferryman", "przystan-rybacka", _) => new(164f, -184f),

            ("smolnik-woodcutter", "osada-smolnikow", "wood-work") => new(-167f, -275f),
            ("smolnik-woodcutter", "osada-smolnikow", _) => new(-173f, -272f),

            ("smolnik-burner", "osada-smolnikow", "tar-burning") => new(-173f, -282f),
            ("smolnik-burner", "osada-smolnikow", _) => new(-176f, -276f),

            ("caravan-merchant", "oboz-kupiecki", "caravan-trade") => new(-154f, 94f),
            ("caravan-merchant", "oboz-kupiecki", _) => new(-153f, 97f),

            _ => LocationCenter(locationId)
        };

    private static Vector2 LocationCenter(string locationId) => locationId switch
    {
        "old-village" => new(0f, -85f),
        "black-swamp" => new(95f, 35f),
        "old-shrine" => new(-85f, 55f),
        "grod-borzywoj" => new(-320f, 220f),
        "przystan-rybacka" => new(148f, -180f),
        "osada-smolnikow" => new(-175f, -280f),
        "oboz-kupiecki" => new(-155f, 95f),
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
            "grod-borzywoj" => new Vector2(-320f, 220f),
            "przystan-rybacka" => new Vector2(148f, -180f),
            "osada-smolnikow" => new Vector2(-175f, -280f),
            "oboz-kupiecki" => new Vector2(-155f, 95f),
            _ => Vector2.Zero
        };
        var from = PoseFor(npcId, locationId, "");
        return WorldPlacementOrientation.YawFacing(from, target);
    }

    private static double ForwardHours(
        double from,
        double to)
    {
        var delta = to - from;
        while (delta < 0d)
            delta += 24d;
        while (delta >= 24d)
            delta -= 24d;
        return delta;
    }

    private static float Facing(Vector3 from, Vector3 target) =>
        WorldPlacementOrientation.YawFacing(
            new Vector2(from.X, from.Z),
            new Vector2(target.X, target.Z));
}
