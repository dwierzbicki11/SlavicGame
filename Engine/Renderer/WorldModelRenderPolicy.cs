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
        "lawa_wnetrze_",
        "misa_",
        "naczynia_",
        "paliki_rytualne_",
        "pulapka_",
        "peczek_ziol_",
        "riverbank_rocky_",
        "rope_coil_",
        "sack_",
        "siekiera_",
        "slady_",
        "stojak_narzedzia_",
        "stol_warsztatowy_",
        "stos_drewna_",
        "taboret_",
        "swiece_rytualne_",
        "wiadro_",
        "ziola_suszone_",
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
