using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record SlavicHerbDefinition(
    string ItemId,
    string DisplayName,
    string Description,
    int Yield);

public sealed record SlavicHerbSpawn(
    string Id,
    string ItemId,
    Vector2 Position,
    float Radius = 3.4f);

public static class SlavicHerbalismSystem
{
    public static readonly IReadOnlyDictionary<string, SlavicHerbDefinition> HerbCatalog =
        new Dictionary<string, SlavicHerbDefinition>(StringComparer.Ordinal)
        {
            ["ziolo-dziurawiec"] = new(
                "ziolo-dziurawiec",
                "DZIURAWIEC ZWYCZAJNY",
                "Zlote ziele sloneczne rosnace na polanach i przy swietych gajach. Przywraca sily zyciowe.",
                2),
            ["ziolo-piolun"] = new(
                "ziolo-piolun",
                "PIOLUN POLNY",
                "Gorzkie ziele ochronne o srebrzystych lisciach. Odpiera zle uroki i oczyszcza cialo.",
                2),
            ["ziolo-krwawnik"] = new(
                "ziolo-krwawnik",
                "KRWAWNIK POSPOLITY",
                "Ziele wojow i mysliwych rosnace przy traktach. Zasklepia rany i tamuje krwawienie.",
                2),
            ["ziolo-zywokost"] = new(
                "ziolo-zywokost",
                "KORZEN ZYWOKOSTU",
                "Gruby korzen z wilgotnych dolin rzecznych. Niezrownany w gojeniu ciezkich ran i zrastaniu kosci.",
                2)
        };

    public static readonly IReadOnlyList<SlavicHerbSpawn> Spawns =
    [
        // Dziurawiec — Polana startowa, okolice Żarnowca, Kamienny Krąg, Gród
        new("herb-dziurawiec-start", "ziolo-dziurawiec", new Vector2(-12f, 15f)),
        new("herb-dziurawiec-village", "ziolo-dziurawiec", new Vector2(-24f, -62f)),
        new("herb-dziurawiec-shrine", "ziolo-dziurawiec", new Vector2(-75f, 48f)),
        new("herb-dziurawiec-stronghold", "ziolo-dziurawiec", new Vector2(-285f, 165f)),

        // Piołun — Suche stoki, obrzeża borów, skraj karawany
        new("herb-piolun-clearing", "ziolo-piolun", new Vector2(22f, 32f)),
        new("herb-piolun-ridge", "ziolo-piolun", new Vector2(-180f, 85f)),
        new("herb-piolun-pine", "ziolo-piolun", new Vector2(-420f, -310f)),
        new("herb-piolun-caravan", "ziolo-piolun", new Vector2(-145f, 105f)),

        // Krwawnik — Trakty leśne, polne miedze, most rzeczny
        new("herb-krwawnik-trail", "ziolo-krwawnik", new Vector2(-5f, -25f)),
        new("herb-krwawnik-village", "ziolo-krwawnik", new Vector2(25f, -95f)),
        new("herb-krwawnik-bridge", "ziolo-krwawnik", new Vector2(130f, -160f)),
        new("herb-krwawnik-woodcutter", "ziolo-krwawnik", new Vector2(-155f, -265f)),

        // Żywokost — Dolina rzeki, Czarne Mokradła, Przystań
        new("herb-zywokost-river", "ziolo-zywokost", new Vector2(110f, -120f)),
        new("herb-zywokost-swamp", "ziolo-zywokost", new Vector2(82f, 42f)),
        new("herb-zywokost-fishing", "ziolo-zywokost", new Vector2(162f, -195f)),
        new("herb-zywokost-wetland", "ziolo-zywokost", new Vector2(460f, -310f))
    ];

    public static string CollectedFlag(string spawnId) => $"herb.{spawnId}.collected";

    public static bool CanHarvest(WorldState world, string spawnId)
    {
        ArgumentNullException.ThrowIfNull(world);
        return !world.Progress.HasFlag(CollectedFlag(spawnId));
    }

    public static bool Harvest(WorldState world, string spawnId, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);
        var spawn = Spawns.FirstOrDefault(s => s.Id == spawnId);
        if (spawn is null || !CanHarvest(world, spawnId))
        {
            message = "ZIOLO JUZ ZOSTALO ZEBRANE";
            return false;
        }

        var def = HerbCatalog[spawn.ItemId];
        world.Progress.Inventory.Add(spawn.ItemId, def.Yield);
        world.Progress.SetFlag(CollectedFlag(spawnId));
        message = $"ZDOBYTO: {def.DisplayName} x{def.Yield}";
        return true;
    }
}
