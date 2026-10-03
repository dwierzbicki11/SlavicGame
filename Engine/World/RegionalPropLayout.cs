using System.Numerics;

namespace SlavicGame.Engine.World;

public sealed record RegionalPropPlacement(
    string Id,
    string AssetPath,
    float X,
    float Z,
    Vector3 Scale,
    float YawRadians,
    float YOffset = 0f,
    float CollisionWidth = 0f,
    float CollisionDepth = 0f,
    float CollisionHeight = 0f)
{
    public bool BlocksMovement =>
        CollisionWidth > 0f &&
        CollisionDepth > 0f &&
        CollisionHeight > 0f;
}

public static class RegionalPropLayout
{
    public static IReadOnlyList<RegionalPropPlacement> Placements { get; } =
    [
        // Żarnowiec — more silhouette variety plus lived-in work/market props.
        new("village-hut-d", "models/static/chata_r0_variant_04.glb", -24f, -98f, new Vector3(1.28f), 0.46f, 0f, 7.5f, 7f, 5f),
        new("village-barn", "models/static/stodola_r0_01.glb", -29f, -86f, new Vector3(1.12f), -0.18f, 0f, 10f, 7f, 5.5f),
        new("village-stable", "models/static/stajnia_r0_01.glb", 27f, -76f, new Vector3(1.08f), 0.28f, 0f, 9f, 6f, 5f),
        new("village-watchtower", "models/static/wieza_straznicza_r0_01.glb", 25f, -104f, new Vector3(1.05f), -0.08f, 0f, 4.5f, 4.5f, 9f),
        new("village-cart-market", "models/static/wozek_reczny_r0_01.glb", -8f, -72f, new Vector3(0.92f), 0.62f),
        new("village-cart-south", "models/static/wozek_reczny_r0_01.glb", 12f, -102f, new Vector3(0.94f), -0.48f),
        new("village-hay-west-a", "models/static/bela_siana_r0_01.glb", -24f, -76f, new Vector3(0.92f), 0.12f),
        new("village-hay-west-b", "models/static/bela_siana_r0_01.glb", -21.5f, -74.5f, new Vector3(0.84f), 0.84f),
        new("village-hay-stable", "models/static/bela_siana_r0_01.glb", 24f, -80f, new Vector3(0.88f), -0.35f),
        new("village-wood-forge-a", "models/static/stos_drewna_r0_02.glb", 16f, -96f, new Vector3(0.92f), 0.18f),
        new("village-wood-house-d", "models/static/stos_drewna_r0_02.glb", -18f, -101f, new Vector3(0.86f), -0.42f),
        new("village-forge-anvil", "models/static/kowadlo_r0_01.glb", 19f, -88f, new Vector3(0.82f), 0.32f),
        new("village-forge-tools", "models/static/stojak_narzedzia_r0_01.glb", 24f, -89f, new Vector3(0.82f), -0.18f),
        new("village-market-basket-a", "models/static/basket_r0_01.glb", -4f, -73f, new Vector3(0.95f), 0.20f, 0.02f),
        new("village-market-basket-b", "models/static/basket_r0_01.glb", 2f, -73.5f, new Vector3(0.82f), -0.55f, 0.02f),
        new("village-market-sack-a", "models/static/sack_r0_01.glb", -3f, -78f, new Vector3(1.05f), 0.15f, 0.02f),
        new("village-market-sack-b", "models/static/sack_r0_01.glb", 3f, -78f, new Vector3(0.92f), 0.75f, 0.02f),
        new("village-market-pot-a", "models/static/garnek_gliniany_r0_01.glb", -1f, -73f, new Vector3(0.86f), 0.05f, 0.02f),
        new("village-market-pot-b", "models/static/garnek_gliniany_r0_01.glb", 4f, -75f, new Vector3(0.76f), 0.90f, 0.02f),
        new("village-firepit", "models/static/campfire_unlit_r0_01.glb", 0f, -88f, new Vector3(1.18f), 0f, 0.02f),
        new("village-road-sign", "models/static/drogowskaz_r0_01.glb", -3f, -113f, new Vector3(1.08f), 0.12f, 0f, 1f, 1f, 2.5f),

        // Starting forest — a small hunter camp and a terrain landmark.
        new("forest-hunter-firepit", "models/static/campfire_unlit_r0_01.glb", 37f, 21f, new Vector3(1.05f), 0.20f, 0.02f),
        new("forest-hunter-basket", "models/static/basket_r0_01.glb", 39f, 19f, new Vector3(0.82f), 0.80f, 0.02f),
        new("forest-hunter-sack", "models/static/sack_r0_01.glb", 35f, 19f, new Vector3(0.92f), -0.40f, 0.02f),
        new("forest-hunter-trap-a", "models/static/pulapka_lowiecka_r0_01.glb", 44f, 25f, new Vector3(0.88f), 0.55f, 0.02f),
        new("forest-hunter-trap-b", "models/static/wnyk_r0_01.glb", 48f, 18f, new Vector3(0.92f), -0.35f, 0.02f),
        new("forest-crossroad-sign", "models/static/drogowskaz_r0_01.glb", 27f, -17f, new Vector3(1.02f), -0.35f, 0f, 1f, 1f, 2.5f),
        new("forest-cave-entrance", "models/static/cave_entrance_r0_01.glb", -56f, 25f, new Vector3(1.75f), 0.26f, -0.15f, 6f, 3f, 4f),
        new("forest-cave-rock-a", "models/static/cliff_chunk_r0_01.glb", -61f, 23f, new Vector3(2.1f, 1.5f, 1.7f), 0.15f, -0.25f),
        new("forest-cave-rock-b", "models/static/cliff_chunk_r0_01.glb", -52f, 21f, new Vector3(1.6f, 1.25f, 1.8f), -0.48f, -0.25f),
        new("forest-cave-rock-c", "models/static/cliff_chunk_r0_01.glb", -58f, 30f, new Vector3(1.45f, 1.15f, 1.55f), 0.72f, -0.25f),

        // Black Swamp — traversal storytelling around the crossing and predator territory.
        new("swamp-broken-bridge", "models/static/bridge_broken_r0_01.glb", 94f, 43f, new Vector3(1.35f), 0.28f, 0.02f),
        new("swamp-damaged-boardwalk", "models/static/kladka_bagienna_uszkodzona_01.glb", 98f, 39f, new Vector3(1.16f), 0.22f, 0.02f),
        new("swamp-island-east", "models/static/wyspa_bagienna_r0_01.glb", 112f, 23f, new Vector3(1.22f), 0.18f, -0.08f),
        new("swamp-island-west", "models/static/wyspa_bagienna_r0_01.glb", 76f, 36f, new Vector3(1.05f), -0.38f, -0.08f),
        new("swamp-fish-trap-a", "models/static/pulapka_rybacka_r0_01.glb", 106f, 27f, new Vector3(0.92f), 0.44f, 0.02f),
        new("swamp-fish-trap-b", "models/static/pulapka_rybacka_r0_01.glb", 113f, 30f, new Vector3(0.82f), -0.22f, 0.02f),
        new("swamp-claw-tracks-a", "models/static/slady_pazurow_r0_01.glb", 99f, 34f, new Vector3(1.12f), 0.15f, 0.025f),
        new("swamp-claw-tracks-b", "models/static/slady_pazurow_r0_01.glb", 103f, 32f, new Vector3(0.92f), 0.52f, 0.025f),
        new("swamp-blood-trace", "models/static/slady_krwi_proxy_r0_01.glb", 84f, 29f, new Vector3(1.08f), -0.12f, 0.025f),
        new("swamp-abandoned-sack", "models/static/sack_r0_01.glb", 86f, 31f, new Vector3(0.90f), 0.60f, 0.02f),
        new("swamp-abandoned-basket", "models/static/basket_r0_01.glb", 88f, 33f, new Vector3(0.82f), -0.35f, 0.02f),

        // Kamienny Krąg — ruins and ritual dressing, kept lightweight and static.
        new("shrine-ruin-wall-a", "models/static/mur_ruina_r0_01.glb", -74f, 58f, new Vector3(1.28f), 0.32f, -0.05f, 4f, 1.2f, 3.2f),
        new("shrine-ruin-wall-b", "models/static/mur_ruina_r0_02.glb", -77f, 66f, new Vector3(1.18f), -0.42f, -0.05f, 4f, 1.2f, 3.2f),
        new("shrine-cult-stone-a", "models/static/kamien_kultowy_r0_01.glb", -91f, 47f, new Vector3(1.06f), 0.18f, 0f, 1.5f, 1.5f, 2.8f),
        new("shrine-cult-stone-b", "models/static/kamien_kultowy_r0_01.glb", -94f, 62f, new Vector3(0.92f), -0.50f, 0f, 1.4f, 1.4f, 2.6f),
        new("shrine-rune-stone-a", "models/static/kamien_runiczny_r0_01.glb", -79f, 49f, new Vector3(0.92f), 0.60f, 0f, 1.1f, 1.1f, 2.4f),
        new("shrine-rune-stone-b", "models/static/kamien_runiczny_r0_01.glb", -75f, 54f, new Vector3(0.82f), -0.10f, 0f, 1f, 1f, 2.2f),
        new("shrine-offering-bowl-a", "models/static/misa_ofiarna_r0_01.glb", -83.8f, 56.2f, new Vector3(0.72f), 0.15f, 0.03f),
        new("shrine-offering-bowl-b", "models/static/misa_ofiarna_r0_01.glb", -86.2f, 54.2f, new Vector3(0.68f), -0.35f, 0.03f),
        new("shrine-ritual-stakes", "models/static/paliki_rytualne_r0_01.glb", -88f, 58f, new Vector3(0.90f), 0.45f, 0.02f),
        new("shrine-candles-a", "models/static/swiece_rytualne_r0_01.glb", -84f, 54.8f, new Vector3(0.78f), 0.10f, 0.03f),
        new("shrine-candles-b", "models/static/swiece_rytualne_r0_01.glb", -86f, 55.7f, new Vector3(0.68f), -0.50f, 0.03f),
        new("shrine-grave-a", "models/static/znacznik_grobu_r0_01.glb", -98f, 58f, new Vector3(0.94f), 0.15f, 0f, 1f, 0.6f, 2.1f),
        new("shrine-grave-b", "models/static/znacznik_grobu_r0_01.glb", -96f, 64f, new Vector3(0.88f), -0.28f, 0f, 1f, 0.6f, 2f),
        new("shrine-grave-c", "models/static/znacznik_grobu_r0_01.glb", -73f, 48f, new Vector3(0.82f), 0.42f, 0f, 0.9f, 0.6f, 1.9f)
    ];
}
