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
        new("village-cart-market", "models/static/wozek_reczny_r0_01.glb", -8f, -72f, new Vector3(0.92f), 0.62f, 0f, 2.0f, 1.3f, 1.2f),
        new("village-cart-south", "models/static/wozek_reczny_r0_01.glb", 12f, -102f, new Vector3(0.94f), -0.48f, 0f, 2.0f, 1.3f, 1.2f),
        new("village-hay-west-a", "models/static/bela_siana_r0_01.glb", -24f, -76f, new Vector3(0.92f), 0.12f, 0f, 1.15f, 0.95f, 0.95f),
        new("village-hay-west-b", "models/static/bela_siana_r0_01.glb", -21.5f, -74.5f, new Vector3(0.84f), 0.84f, 0f, 1.05f, 0.90f, 0.90f),
        new("village-hay-stable", "models/static/bela_siana_r0_01.glb", 24f, -80f, new Vector3(0.88f), -0.35f, 0f, 1.10f, 0.90f, 0.90f),
        new("village-wood-forge-a", "models/static/stos_drewna_r0_02.glb", 12.6f, -96.2f, new Vector3(0.92f), 0.18f, 0f, 1.8f, 1.5f, 1.3f),
        new("village-wood-house-d", "models/static/stos_drewna_r0_02.glb", -18f, -101f, new Vector3(0.86f), -0.42f, 0f, 1.7f, 1.4f, 1.2f),
        new("village-forge-anvil", "models/static/kowadlo_r0_01.glb", 19f, -88f, new Vector3(0.82f), 0.32f, 0f, 0.95f, 0.55f, 0.85f),
        new("village-forge-tools", "models/static/stojak_narzedzia_r0_01.glb", 24f, -89f, new Vector3(0.82f), -0.18f, 0f, 1.4f, 0.45f, 1.4f),
        new("village-market-basket-a", "models/static/basket_r0_01.glb", -4f, -73f, new Vector3(0.95f), 0.20f, 0.02f),
        new("village-market-basket-b", "models/static/basket_r0_01.glb", 2f, -73.5f, new Vector3(0.82f), -0.55f, 0.02f),
        new("village-market-sack-a", "models/static/sack_r0_01.glb", -3f, -78f, new Vector3(1.05f), 0.15f, 0.02f),
        new("village-market-sack-b", "models/static/sack_r0_01.glb", 3f, -78f, new Vector3(0.92f), 0.75f, 0.02f),
        new("village-market-pot-a", "models/static/garnek_gliniany_r0_01.glb", -1f, -73f, new Vector3(0.86f), 0.05f, 0.02f),
        new("village-market-pot-b", "models/static/garnek_gliniany_r0_01.glb", 4f, -75f, new Vector3(0.76f), 0.90f, 0.02f),
        new("village-firepit", "models/static/campfire_unlit_r0_01.glb", 0f, -88f, new Vector3(1.18f), 0f, 0.02f, 1.2f, 1.2f, 0.65f),
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
        new("shrine-grave-c", "models/static/znacznik_grobu_r0_01.glb", -73f, 48f, new Vector3(0.82f), 0.42f, 0f, 0.9f, 0.6f, 1.9f),

        // Gród Borzywoj — fortified Slavic town / stronghold on the western oak ridge
        new("stronghold-hall", "models/static/chata_r0_variant_01.glb", -320f, 235f, new Vector3(1.55f), 0.10f, 0f, 9.5f, 8.5f, 6.5f),
        new("stronghold-lodge-elder", "models/static/chata_r0_variant_02.glb", -338f, 226f, new Vector3(1.35f), -0.45f, 0f, 8.5f, 7.5f, 5.5f),
        new("stronghold-barracks", "models/static/chata_r0_variant_03.glb", -302f, 228f, new Vector3(1.35f), 0.52f, 0f, 8.5f, 7.5f, 5.5f),
        new("stronghold-craftsman", "models/static/chata_r0_variant_04.glb", -332f, 206f, new Vector3(1.25f), 0.28f, 0f, 7.5f, 7f, 5f),
        new("stronghold-barn", "models/static/stodola_r0_01.glb", -306f, 208f, new Vector3(1.20f), -0.22f, 0f, 10.5f, 7.5f, 5.8f),
        new("stronghold-stable", "models/static/stajnia_r0_01.glb", -335f, 242f, new Vector3(1.18f), 0.35f, 0f, 9.5f, 6.5f, 5.2f),
        new("stronghold-granary", "models/static/spichlerz_r0_01.glb", -310f, 246f, new Vector3(1.20f), -0.15f, 0f, 4.5f, 3.5f, 4.5f),
        new("stronghold-smithy", "models/static/kuznia_r0_01.glb", -298f, 218f, new Vector3(1.15f), 0.40f, 0f, 5.5f, 4.2f, 4.5f),
        new("stronghold-watchtower-west", "models/static/wieza_straznicza_r0_01.glb", -328f, 186f, new Vector3(1.10f), 0.05f, 0f, 4.8f, 4.8f, 9.5f),
        new("stronghold-watchtower-east", "models/static/wieza_straznicza_r0_01.glb", -308f, 186f, new Vector3(1.10f), -0.05f, 0f, 4.8f, 4.8f, 9.5f),
        new("stronghold-gate", "models/static/brama_wioskowa_r0_01.glb", -318f, 188f, new Vector3(1.25f), 0f, 0f, 5.5f, 2.5f, 4.5f),
        new("stronghold-market-stall-a", "models/static/stoisko_targowe_r0_01.glb", -324f, 214f, new Vector3(1.15f), 0.15f, 0f, 3.8f, 2.2f, 3.2f),
        new("stronghold-market-stall-b", "models/static/stoisko_targowe_r0_01.glb", -312f, 214f, new Vector3(1.15f), -0.15f, 0f, 3.8f, 2.2f, 3.2f),
        new("stronghold-wagon-gate", "models/static/wozek_reczny_r0_01.glb", -324f, 194f, new Vector3(1.10f), 0.35f, 0f, 2.5f, 1.5f, 1.3f),
        new("stronghold-wagon-market", "models/static/wozek_reczny_r0_01.glb", -316f, 210f, new Vector3(1.05f), -0.42f, 0f, 2.5f, 1.5f, 1.3f),
        new("stronghold-wagon-stable", "models/static/wozek_reczny_r0_01.glb", -338f, 236f, new Vector3(1.05f), 0.65f, 0f, 2.5f, 1.5f, 1.3f),
        new("stronghold-firepit", "models/static/campfire_unlit_r0_01.glb", -318f, 222f, new Vector3(1.25f), 0f, 0.02f, 1.3f, 1.3f, 0.7f),
        new("stronghold-hay-a", "models/static/bela_siana_r0_01.glb", -332f, 240f, new Vector3(0.95f), 0.2f, 0f, 1.15f, 0.95f, 0.95f),
        new("stronghold-hay-b", "models/static/bela_siana_r0_01.glb", -334f, 238f, new Vector3(0.90f), -0.3f, 0f, 1.10f, 0.90f, 0.90f),
        new("stronghold-wood-a", "models/static/stos_drewna_r0_02.glb", -300f, 214f, new Vector3(0.95f), 0.3f, 0f, 1.9f, 1.6f, 1.4f),
        new("stronghold-wood-b", "models/static/stos_drewna_r0_02.glb", -324f, 238f, new Vector3(0.90f), -0.2f, 0f, 1.8f, 1.5f, 1.3f),
        new("stronghold-smithy-anvil", "models/static/kowadlo_r0_01.glb", -295f, 216f, new Vector3(0.85f), 0.2f, 0f, 1.0f, 0.6f, 0.9f),
        new("stronghold-smithy-tools", "models/static/stojak_narzedzia_r0_01.glb", -296f, 220f, new Vector3(0.85f), -0.2f, 0f, 1.5f, 0.5f, 1.5f),
        new("stronghold-chest-market", "models/static/skrzynia_kufer_r0_01.glb", -321f, 212f, new Vector3(0.95f), 0.1f, 0.02f),
        new("stronghold-chest-hall", "models/static/kufer_r0_02.glb", -317f, 231f, new Vector3(0.90f), -0.1f, 0.02f),
        new("stronghold-sign", "models/static/drogowskaz_r0_01.glb", -318f, 182f, new Vector3(1.10f), 0.05f, 0f, 1f, 1f, 2.5f),

        // Przystań Rybacka & Most Rzeczny — fishing hamlet and wooden bridge crossing
        new("fishing-bridge-main", "models/static/most_drewniany_r0_01.glb", 179.25f, -180f, new Vector3(1.95f, 1.35f, 1.45f), 0f, 1.45f, 22.0f, 4.2f, 2.5f),
        new("fishing-pier-north", "models/static/pomost_r0_01.glb", 166f, -170f, new Vector3(1.30f), 0.15f, 0.40f, 7.5f, 2.2f, 2.2f),
        new("fishing-pier-south", "models/static/pomost_r0_01.glb", 166f, -192f, new Vector3(1.30f), -0.15f, 0.40f, 7.5f, 2.2f, 2.2f),
        new("fishing-hut-a", "models/static/chata_r0_variant_02.glb", 142f, -174f, new Vector3(1.25f), -0.25f, 0f, 8f, 6.5f, 5.2f),
        new("fishing-hut-b", "models/static/chata_r0_variant_03.glb", 136f, -190f, new Vector3(1.22f), 0.42f, 0f, 7.5f, 7.5f, 5.0f),
        new("fishing-granary", "models/static/spichlerz_r0_01.glb", 148f, -198f, new Vector3(1.12f), 0.10f, 0f, 4.2f, 3.2f, 4.2f),
        new("fishing-cart-pier", "models/static/wozek_reczny_r0_01.glb", 154f, -184f, new Vector3(1.02f), 0.55f, 0f, 2.2f, 1.4f, 1.2f),
        new("fishing-cart-hut", "models/static/wozek_reczny_r0_01.glb", 138f, -182f, new Vector3(0.95f), -0.30f, 0f, 2.0f, 1.3f, 1.2f),
        new("fishing-trap-a", "models/static/pulapka_rybacka_r0_01.glb", 167f, -174f, new Vector3(1.10f), 0.3f, 0.02f),
        new("fishing-trap-b", "models/static/pulapka_rybacka_r0_01.glb", 165f, -196f, new Vector3(1.05f), -0.2f, 0.02f),
        new("fishing-rope-a", "models/static/rope_coil_r0_01.glb", 160f, -172f, new Vector3(0.95f), 0.1f, 0.02f),
        new("fishing-rope-b", "models/static/rope_coil_r0_01.glb", 158f, -190f, new Vector3(0.90f), -0.4f, 0.02f),
        new("fishing-basket-a", "models/static/basket_r0_01.glb", 146f, -176f, new Vector3(0.90f), 0.2f, 0.02f),
        new("fishing-basket-b", "models/static/basket_r0_01.glb", 150f, -186f, new Vector3(0.85f), -0.3f, 0.02f),
        new("fishing-wood", "models/static/stos_drewna_r0_02.glb", 140f, -196f, new Vector3(0.90f), 0.15f, 0f, 1.8f, 1.5f, 1.3f),
        new("fishing-campfire", "models/static/campfire_unlit_r0_01.glb", 145f, -182f, new Vector3(1.10f), 0f, 0.02f, 1.2f, 1.2f, 0.6f),
        new("fishing-sign", "models/static/drogowskaz_r0_01.glb", 132f, -178f, new Vector3(1.05f), 0.20f, 0f, 1f, 1f, 2.5f),
        new("fishing-chest", "models/static/skrzynia_kufer_r0_01.glb", 144f, -178f, new Vector3(0.92f), 0.2f, 0.02f),

        // Osada Smolników — woodcutters and charcoal burners settlement at the edge of Bór Perunowy
        new("smolnik-hut", "models/static/chata_r0_variant_04.glb", -175f, -270f, new Vector3(1.25f), 0.18f, 0f, 7.5f, 7f, 5f),
        new("smolnik-barn", "models/static/stodola_r0_01.glb", -188f, -286f, new Vector3(1.12f), -0.35f, 0f, 10f, 7f, 5.5f),
        new("smolnik-woodpile-a", "models/static/stos_drewna_r0_02.glb", -164f, -276f, new Vector3(1.35f), 0.12f, 0f, 2.4f, 2.0f, 1.7f),
        new("smolnik-woodpile-b", "models/static/stos_drewna_r0_02.glb", -162f, -286f, new Vector3(1.25f), -0.45f, 0f, 2.2f, 1.8f, 1.6f),
        new("smolnik-woodpile-c", "models/static/stos_drewna_r0_02.glb", -182f, -266f, new Vector3(1.30f), 0.65f, 0f, 2.3f, 1.9f, 1.6f),
        new("smolnik-wagon-a", "models/static/wozek_reczny_r0_01.glb", -170f, -284f, new Vector3(1.12f), -0.25f, 0f, 2.5f, 1.5f, 1.3f),
        new("smolnik-wagon-b", "models/static/wozek_reczny_r0_01.glb", -180f, -276f, new Vector3(1.08f), 0.40f, 0f, 2.4f, 1.4f, 1.3f),
        new("smolnik-tools", "models/static/stojak_narzedzia_r0_01.glb", -172f, -274f, new Vector3(0.90f), 0.20f, 0f, 1.5f, 0.5f, 1.5f),
        new("smolnik-axe", "models/static/siekiera_r0_01.glb", -169f, -273f, new Vector3(0.95f), 0.10f, 0.02f),
        new("smolnik-campfire", "models/static/campfire_unlit_r0_01.glb", -174f, -280f, new Vector3(1.20f), 0f, 0.02f, 1.2f, 1.2f, 0.65f),
        new("smolnik-sign", "models/static/drogowskaz_r0_01.glb", -166f, -266f, new Vector3(1.05f), -0.30f, 0f, 1f, 1f, 2.5f),
        new("smolnik-chest", "models/static/skrzynia_kufer_r0_01.glb", -176f, -272f, new Vector3(0.92f), -0.3f, 0.02f),

        // Obóz Karawany Kupieckiej — merchant trade wagons along the Great West Road
        new("caravan-wagon-lead", "models/static/wozek_reczny_r0_01.glb", -152f, 93f, new Vector3(1.15f), 0.42f, 0f, 2.5f, 1.5f, 1.3f),
        new("caravan-wagon-rear", "models/static/wozek_reczny_r0_01.glb", -158f, 98f, new Vector3(1.10f), 0.35f, 0f, 2.5f, 1.5f, 1.3f),
        new("caravan-campfire", "models/static/campfire_unlit_r0_01.glb", -154f, 96f, new Vector3(1.12f), 0f, 0.02f, 1.2f, 1.2f, 0.6f),
        new("caravan-hay", "models/static/bela_siana_r0_01.glb", -150f, 91f, new Vector3(0.95f), 0.15f, 0f, 1.15f, 0.95f, 0.95f),
        new("caravan-chest-a", "models/static/skrzynia_kufer_r0_01.glb", -153f, 95f, new Vector3(0.95f), 0.5f, 0.02f),
        new("caravan-chest-b", "models/static/kufer_r0_02.glb", -156f, 96f, new Vector3(0.90f), -0.2f, 0.02f),
        new("caravan-sack", "models/static/sack_r0_01.glb", -155f, 94f, new Vector3(1.05f), 0.3f, 0.02f),
        new("caravan-basket", "models/static/basket_r0_01.glb", -157f, 95f, new Vector3(0.85f), -0.4f, 0.02f),
        new("caravan-sign", "models/static/drogowskaz_r0_01.glb", -148f, 88f, new Vector3(1.05f), 0.35f, 0f, 1f, 1f, 2.5f),

        // Swiety Chram Peruna na Debowym Grzbiecie
        new("shrine-perun-oak", "models/static/dab_stary_r0_01.glb", -250f, 160f, new Vector3(1.35f), 0.2f, 0f, 4.5f, 4.5f, 14f),
        new("shrine-perun-chapel", "models/static/kapliczka_ruina_r0_01.glb", -248f, 161f, new Vector3(1.15f), -0.3f, 0f, 1.8f, 1.8f, 3.2f),
        new("shrine-perun-bowl", "models/static/misa_ofiarna_r0_01.glb", -248f, 160f, new Vector3(0.95f), 0.1f, 0.02f),
        new("shrine-perun-candles", "models/static/swiece_rytualne_r0_01.glb", -247f, 160.5f, new Vector3(0.90f), 0f, 0.02f),
        new("shrine-perun-marker", "models/static/znak_ochronny_r0_01.glb", -252f, 159f, new Vector3(1.05f), 0.4f, 0f, 1f, 1f, 2.5f),

        // Swiete Uroczysko Welesa w Podmoklym Jarze
        new("shrine-weles-idol", "models/static/kamien_kultowy_r0_01.glb", 100f, -145f, new Vector3(1.25f), -0.15f, 0f, 2.2f, 2.2f, 3.5f),
        new("shrine-weles-stone", "models/static/kamien_runiczny_r0_01.glb", 98f, -147f, new Vector3(1.10f), 0.45f, 0f, 1.8f, 1.8f, 2.8f),
        new("shrine-weles-bowl", "models/static/misa_rytualna_r0_01.glb", 99.5f, -144f, new Vector3(0.95f), 0f, 0.02f),
        new("shrine-weles-stakes", "models/static/paliki_rytualne_r0_01.glb", 102f, -146f, new Vector3(1.00f), 0.2f, 0f, 1.5f, 1.5f, 1.8f),

        // Swiety Gaj Mokoszy
        new("shrine-mokosz-birch", "models/static/brzoza_r0_01.glb", -80f, -170f, new Vector3(1.20f), 0f, 0f, 3f, 3f, 12f),
        new("shrine-mokosz-chapel", "models/static/kapliczka_ruina_r0_01.glb", -78f, -169f, new Vector3(1.10f), 0.35f, 0f, 1.8f, 1.8f, 3.2f),
        new("shrine-mokosz-bowl", "models/static/misa_ofiarna_r0_01.glb", -78.5f, -170f, new Vector3(0.90f), -0.1f, 0.02f),
        new("shrine-mokosz-candles", "models/static/swiece_rytualne_r0_01.glb", -79f, -168.5f, new Vector3(0.85f), 0f, 0.02f),

        // Woz podrozny w Zarnowcu
        new("village-travel-wagon", "models/static/wozek_reczny_r0_01.glb", 12f, -112f, new Vector3(1.15f), -0.2f, 0f, 2.5f, 1.5f, 1.3f),

        // Trakt Smolarski — Miejsce napadu wilkow na woz smolnikow
        new("smolnik-tartrail-cart", "models/static/wozek_reczny_r0_01.glb", -192f, -185f, new Vector3(1.10f), 0.75f, 0f, 2.5f, 1.5f, 1.3f),
        new("smolnik-tartrail-barrel", "models/static/sack_r0_01.glb", -190f, -184f, new Vector3(1.05f), 0.3f, 0.02f),
        new("smolnik-tartrail-claws", "models/static/slady_pazurow_r0_01.glb", -191f, -186f, new Vector3(1.20f), 0.1f, 0.02f),
        new("smolnik-tartrail-tree", "models/static/powalone_drzewo_r0_01.glb", -194f, -187f, new Vector3(1.15f), -0.4f, 0f, 4.5f, 1.8f, 1.2f),

        // Miejsca Wezlowe Kampanii Aktu I (Kamienie Milowe 25% Gry)
        new("stronghold-map-table", "models/static/oltar_rytualny_r0_01.glb", -322f, 224f, new Vector3(0.95f), 0.1f, 0f, 1.6f, 1.6f, 1.0f),
        new("kurhan-ancient-stele", "models/static/kamien_runiczny_r0_01.glb", -95f, 68f, new Vector3(1.15f), 0.25f, 0f, 1.8f, 1.8f, 2.8f),
        new("perun-boundary-stone", "models/static/kamien_kultowy_r0_01.glb", -375f, -175f, new Vector3(1.20f), -0.2f, 0f, 2.0f, 2.0f, 3.2f),

        // Akt II — Przystan Rybacka i Blokada Ujscia (MQ20) oraz Stara Sztolnia (MQ22)
        new("estuary-pier", "models/static/pomost_r0_01.glb", 154f, -186f, new Vector3(1.10f), 0.35f, 0f, 3.0f, 1.8f, 1.2f),
        new("estuary-map-fragment", "models/static/fragment_mapy_mq20.glb", 153.5f, -185.5f, new Vector3(0.95f), 0f, 0.02f),
        new("estuary-driftwood-barrier", "models/static/powalone_drzewo_r0_01.glb", 148f, -182f, new Vector3(1.15f), -0.55f, 0f, 4.0f, 1.5f, 1.2f),
        new("mine-chest-gwarek", "models/static/kufer_r0_02.glb", -413f, 314f, new Vector3(0.95f), 0.4f, 0.02f),

        // Zagrody i inwentarz w osadach
        new("village-corral-hay", "models/static/bela_siana_r0_01.glb", 28f, -73f, new Vector3(0.95f), 0.2f, 0f, 1.15f, 0.95f, 0.95f),
        new("village-corral-trough", "models/static/kladka_bagienna_01.glb", 30f, -74f, new Vector3(0.85f), 0.6f, 0f, 2.0f, 0.8f, 0.5f),
        new("stronghold-corral-hay", "models/static/bela_siana_r0_01.glb", -312f, 212f, new Vector3(1.05f), -0.1f, 0f, 1.15f, 0.95f, 0.95f),
        new("stronghold-corral-trough", "models/static/kladka_bagienna_01.glb", -310f, 214f, new Vector3(0.90f), 0.4f, 0f, 2.0f, 0.8f, 0.5f),

        // Swiete zrodla i wiejska studnia
        new("village-well-bucket", "models/static/wiadro_r0_01.glb", -4.0f, -86.0f, new Vector3(0.95f), 0.2f, 0.02f),
        new("shrine-spring-bowl", "models/static/misa_ofiarna_r0_01.glb", -166.0f, -76.0f, new Vector3(1.10f), 0.1f, 0.02f),
        new("stronghold-spring-stone", "models/static/kamien_kultowy_r0_01.glb", -246.0f, 156.0f, new Vector3(1.05f), -0.2f, 0f, 1.8f, 1.8f, 2.5f),
        new("fishing-spring-pot", "models/static/garnek_gliniany_r0_01.glb", 149.0f, -166.0f, new Vector3(1.05f), 0.3f, 0.02f),

        // Kuznia Swaroga w Grodzie Borzywoja
        new("svarog-forge-anvil", "models/static/kowadlo_r0_01.glb", -295f, 218f, new Vector3(1.15f), 0.25f, 0f, 1.2f, 0.8f, 1.1f),
        new("svarog-forge-tools", "models/static/stojak_narzedzia_r0_01.glb", -297f, 219f, new Vector3(1.10f), -0.3f, 0f, 1.5f, 0.6f, 1.6f),
        new("svarog-forge-firepit", "models/static/campfire_unlit_r0_01.glb", -294f, 216f, new Vector3(1.25f), 0f, 0.02f, 1.5f, 1.5f, 0.8f),

        // Pustkowie Pierwszego Progu i Archiwum Dawnych Progow (Akt III)
        new("threshold-waste-ruin-stele", "models/static/kamien_runiczny_r0_01.glb", -460f, 400f, new Vector3(1.4f), 0.3f, 0f, 2.2f, 2.2f, 3.8f),
        new("threshold-archive-entrance", "models/static/kapliczka_ruina_r0_01.glb", -480f, 410f, new Vector3(1.5f), -0.2f, 0f, 3.0f, 3.0f, 4.5f),
        new("threshold-archive-records", "models/static/oltar_rytualny_r0_01.glb", -485f, 415f, new Vector3(1.2f), 0.1f, 0f, 2.0f, 2.0f, 1.2f),
        new("threshold-wszebor-camp", "models/static/wozek_reczny_r0_01.glb", -495f, 425f, new Vector3(1.1f), 0.45f, 0f, 2.5f, 1.5f, 1.3f)
    ];
}
