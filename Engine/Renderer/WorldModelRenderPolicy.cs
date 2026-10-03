namespace SlavicGame.Engine.Renderer;

public static class WorldModelRenderPolicy
{
    private static readonly string[] ShortRangePrefixes =
    [
        "basket_",
        "bela_siana_",
        "campfire_",
        "garnek_",
        "kowadlo_",
        "misa_",
        "paliki_rytualne_",
        "pulapka_",
        "sack_",
        "slady_",
        "stojak_narzedzia_",
        "stos_drewna_",
        "swiece_rytualne_",
        "wnyk_"
    ];

    public static bool IsShortRangeProp(string assetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetPath);

        var fileName = Path.GetFileNameWithoutExtension(assetPath);
        return ShortRangePrefixes.Any(prefix =>
            fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
