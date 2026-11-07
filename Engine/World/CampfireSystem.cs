using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record CampfireDefinition(
    string Id,
    Vector2 Position,
    float HeatRadius,
    float VisualScale,
    bool DefaultLit,
    bool RainExposed,
    string? IgnitionItem);

public sealed class CampfireRuntime
{
    private readonly Dictionary<string, double> _rainExposureSeconds =
        new(StringComparer.Ordinal);

    public string Message { get; private set; } = "";

    public bool IsLit(WorldState world, string id)
    {
        ArgumentNullException.ThrowIfNull(world);
        var definition = CampfireSystem.Get(id);

        if (world.Progress.HasFlag(CampfireSystem.LitFlag(id)))
            return true;
        if (world.Progress.HasFlag(CampfireSystem.ExtinguishedFlag(id)))
            return false;

        return definition.DefaultLit;
    }

    public bool TryLight(WorldState world, string id)
    {
        ArgumentNullException.ThrowIfNull(world);
        var definition = CampfireSystem.Get(id);

        if (IsLit(world, id))
        {
            Message = "OGNISKO JUZ PLONIE";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(definition.IgnitionItem) &&
            !world.Progress.Inventory.Remove(definition.IgnitionItem, 1))
        {
            Message = "POTRZEBNA ZYWICA DO ROZPALENIA";
            return false;
        }

        world.Progress.SetFlag(CampfireSystem.LitFlag(id));
        world.Progress.SetFlag(CampfireSystem.ExtinguishedFlag(id), false);
        _rainExposureSeconds[id] = 0d;
        Message = "OGNISKO ROZPALONE";
        return true;
    }

    public bool Extinguish(WorldState world, string id, string message = "OGNISKO ZGASZONE")
    {
        ArgumentNullException.ThrowIfNull(world);
        CampfireSystem.Get(id);

        if (!IsLit(world, id))
            return false;

        world.Progress.SetFlag(CampfireSystem.LitFlag(id), false);
        world.Progress.SetFlag(CampfireSystem.ExtinguishedFlag(id));
        _rainExposureSeconds[id] = 0d;
        Message = message;
        return true;
    }

    public bool TryRest(WorldState world, string id, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!IsLit(world, id))
        {
            message = "NAJPIERW ROZPAL OGNISKO ABY PRZY NIM ODPOCZAC";
            return false;
        }

        world.Player.Restore();
        world.Time.SetTimeOfDay(world.Time.TimeOfDayHours + 6.0);
        world.NpcWorld.Update(world);
        message = "ODPOCZYNEK PRZY OGNISKU: PRZEWINIETO CZAS I W PELNI ODZYSKANO SILY";
        Message = message;
        return true;
    }

    public float HeatAt(WorldState world, Vector3 position)
    {
        ArgumentNullException.ThrowIfNull(world);

        var p = new Vector2(position.X, position.Z);
        var heat = 0f;

        foreach (var fire in CampfireSystem.Fires)
        {
            if (!IsLit(world, fire.Id))
                continue;

            var distance = Vector2.Distance(p, fire.Position);
            if (distance >= fire.HeatRadius)
                continue;

            var t = 1f - distance / fire.HeatRadius;
            var smooth = t * t * (3f - 2f * t);
            heat = MathF.Max(heat, smooth);
        }

        return Math.Clamp(heat, 0f, 1f);
    }

    public void Update(WorldState world, double deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            return;

        foreach (var fire in CampfireSystem.Fires)
        {
            if (!fire.RainExposed || !IsLit(world, fire.Id))
            {
                _rainExposureSeconds[fire.Id] = 0d;
                continue;
            }

            var rain = Math.Clamp(world.Weather.RainIntensity, 0f, 1f);
            if (rain < 0.58f)
            {
                _rainExposureSeconds[fire.Id] = Math.Max(
                    0d,
                    _rainExposureSeconds.GetValueOrDefault(fire.Id) -
                    deltaSeconds * 0.75d);
                continue;
            }

            var exposure =
                _rainExposureSeconds.GetValueOrDefault(fire.Id) +
                deltaSeconds * rain;
            _rainExposureSeconds[fire.Id] = exposure;

            if (exposure >= 10d)
            {
                Extinguish(
                    world,
                    fire.Id,
                    "ULEWA ZGASILA LESNE OGNISKO");
            }
        }
    }
}

public static class CampfireSystem
{
    public static IReadOnlyList<CampfireDefinition> Fires { get; } =
    [
        new(
            "village-firepit",
            new Vector2(0f, -88f),
            HeatRadius: 7.5f,
            VisualScale: 1.05f,
            DefaultLit: true,
            RainExposed: false,
            IgnitionItem: "forest-resin"),
        new(
            "forest-hunter-firepit",
            new Vector2(37f, 21f),
            HeatRadius: 6.5f,
            VisualScale: 0.92f,
            DefaultLit: false,
            RainExposed: true,
            IgnitionItem: "forest-resin"),
        new(
            "stronghold-courtyard-firepit",
            new Vector2(-320f, 222f),
            HeatRadius: 8.0f,
            VisualScale: 1.15f,
            DefaultLit: false,
            RainExposed: false,
            IgnitionItem: "forest-resin"),
        new(
            "fishing-hamlet-firepit",
            new Vector2(146f, -178f),
            HeatRadius: 7.0f,
            VisualScale: 1.0f,
            DefaultLit: false,
            RainExposed: true,
            IgnitionItem: "forest-resin"),
        new(
            "caravan-camp-firepit",
            new Vector2(-155f, 95f),
            HeatRadius: 7.0f,
            VisualScale: 1.0f,
            DefaultLit: false,
            RainExposed: true,
            IgnitionItem: "forest-resin"),
        new(
            "woodcutter-camp-firepit",
            new Vector2(-175f, -280f),
            HeatRadius: 7.5f,
            VisualScale: 1.05f,
            DefaultLit: false,
            RainExposed: true,
            IgnitionItem: "forest-resin"),
        new(
            "shrine-sacred-brazier",
            new Vector2(-88f, 56f),
            HeatRadius: 6.5f,
            VisualScale: 0.95f,
            DefaultLit: false,
            RainExposed: false,
            IgnitionItem: "forest-resin")
    ];

    public static CampfireDefinition Get(string id) =>
        Fires.FirstOrDefault(fire =>
            string.Equals(fire.Id, id, StringComparison.Ordinal))
        ?? throw new KeyNotFoundException($"Unknown campfire '{id}'.");

    public static string LitFlag(string id) =>
        $"campfire.{id}.lit";

    public static string ExtinguishedFlag(string id) =>
        $"campfire.{id}.extinguished";
}
