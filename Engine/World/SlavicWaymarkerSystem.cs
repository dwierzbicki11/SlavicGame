using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record SlavicWaymarkerDefinition(
    string Id,
    string Title,
    string Inscription,
    Vector2 Position);

public static class SlavicWaymarkerSystem
{
    public static readonly IReadOnlyList<SlavicWaymarkerDefinition> Waymarkers =
    [
        new(
            "waymarker-zarnowiec",
            "ROZSTAJE ZARNOWIECKIE",
            "NA POLUDNIE: WIES ZARNOWIEC | NA POLNOC: PUSZCZA ZYWIA | NA WSCHOD: CZARNE MOKRADLA",
            new Vector2(-4f, -48f)),

        new(
            "waymarker-stronghold",
            "TRAKT WOJOW GRODOWYCH",
            "NA ZACHOD: WIELKI GROD BORZYWOJA | NA POLNOC: DEBOWA KNIEJA | NA POLUDNIE: BOR PERUNOWY",
            new Vector2(-242f, 138f)),

        new(
            "waymarker-river",
            "SZLAK KU PRZEPRAWIE RZECZNEJ",
            "NA POLUDNIOWY WSCHOD: PRZYSTAN RYBACKA I MOST RZECZNY | NA POLNOC: WIES ZARNOWIEC",
            new Vector2(118f, -152f)),

        new(
            "waymarker-shrine",
            "SWIETY SLUP PUSZCZY",
            "NA ZACHOD: KAMIENNY KRAG I SWIETY GAJ MOKOSZY | NA WSCHOD: PUSZCZA ZYWIA",
            new Vector2(-58f, 38f)),

        new(
            "waymarker-mine",
            "ZNAK PRADAWNEJ SZTOLNI",
            "NA POLNOC: STARA SZTOLNIA GOR I WYCINISKA DEBOWEJ KNIEI",
            new Vector2(-385f, 280f))
    ];

    public static string DiscoveredFlag(string id) => $"waymarker.{id}.discovered";

    public static bool Inspect(WorldState world, string id, out string message)
    {
        ArgumentNullException.ThrowIfNull(world);
        var waymarker = Waymarkers.FirstOrDefault(w => w.Id == id);
        if (waymarker is null)
        {
            message = "DROGOWSKAZ NIECZYTELNY";
            return false;
        }

        world.Progress.SetFlag(DiscoveredFlag(id));
        message = $"{waymarker.Title}: {waymarker.Inscription}";
        return true;
    }
}
