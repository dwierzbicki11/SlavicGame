# Region Asset Manifests R0–R6

Status: production planning v0.1. Dokument rozwija `ProductionContentCatalog.md` i region bibles do poziomu asset families. Nie zamraża nazw plików, polycountów ani liczby wariantów przed pomiarami i art pass.

## Zasady

- każdy wpis ma stabilne `ASSETSET_*` i region owner;
- rodzina opisuje potrzebę produkcyjną, nie pojedynczy plik;
- wspólne rodziny są współdzielone zamiast duplikowane między regionami;
- model runtime docelowo: glTF 2.0 (`.glb/.gltf`), źródła DCC pozostają source assets;
- każda rodzina 3D przed `locked` musi dostać collision policy, LOD policy, material set i measured budget;
- obiekt krytyczny dla questa musi mieć placeholder/fallback, aby brak finalnego artu nie blokował implementacji;
- historyczne elementy presentation odwołują się do właściwych research cards; implementacja nie dopowiada ich samodzielnie.

## Shared/global

| ID | Rodzina | Użycie | Lock przed finalem |
|---|---|---|---|
| `ASSETSET_SHARED_VEGETATION` | podstawowa roślinność | R0–R6 zależnie od biomu | species/art pass, LOD |
| `ASSETSET_SHARED_ROCKS` | skały i kamienie | wszystkie regiony | material variants, LOD |
| `ASSETSET_SHARED_ROADS` | drogi, ścieżki, koleiny | R0–R6 | terrain blending |
| `ASSETSET_SHARED_WATER` | rzeki, stawy, mokradła | regiony wodne | renderer/water budget |
| `ASSETSET_SHARED_WEATHER` | deszcz, mgła, śnieg/pył wg regionu | global | VFX/performance |
| `ASSETSET_SHARED_CAMP` | ogniska, namioty/prowizoryczne schronienia, pakunki | podróż/ekspedycje | research/art pass |
| `ASSETSET_SHARED_PROPS` | skrzynie, beczki, drewno, narzędzia, naczynia | osady | material-culture research |

## R0 — Pogranicze Żarnowca

| ID | Rodzina | Zakres |
|---|---|---|
| `ASSETSET_R0_ZARNOWIEC` | zabudowa Żarnowca | domy, gospodarcze, płoty, studnia, warsztaty |
| `ASSETSET_R0_FOREST` | Puszcza Żywia | drzewa, podszyt, martwe drewno, leśne landmarks |
| `ASSETSET_R0_SWAMP` | Czarne Mokradła | podmokły grunt, trzciny, kładki, woda, bagienne props |
| `ASSETSET_R0_SACRED` | Kamienny Krąg i lokalne miejsca kultowe | kamienie, znaki, offerings/props zgodne z research |
| `ASSETSET_R0_QUEST` | MQ00–MQ13 critical props | ślady, evidence, route/reference landmarks, map/evidence presentation |

`VerticalSliceAssetList.md` pozostaje dokładniejszym P0/P1/P2 ownerem dla pierwszego grywalnego wycinka; ten manifest nie duplikuje jego rekordów.

## R1 — Nadborze

| ID | Rodzina | Zakres |
|---|---|---|
| `ASSETSET_R1_DEBRZYN` | Dębrzyn | zabudowa grodu/osady, administracja, rzemiosło |
| `ASSETSET_R1_RIVER` | rzeka i brzegi | przeprawy, pomosty, łodzie tylko jeśli scope żeglugi zostanie zatwierdzony |
| `ASSETSET_R1_HUNTERS` | łowiectwo i teren | obozowiska, tropy, narzędzia presentation |
| `ASSETSET_R1_OLD_PLACES` | stare miejsca | quest/religion landmarks bez niezatwierdzonych rekonstrukcji |

## R2 — Wielki Bór

| ID | Rodzina | Zakres |
|---|---|---|
| `ASSETSET_R2_FOREST_DEPTHS` | Głębia I–III | warstwy lasu, coraz mniej antropogenicznych elementów |
| `ASSETSET_R2_ANOMALY` | anomalie nawigacyjne | landmarks i presentation wspierające czytelność bez GPS |
| `ASSETSET_R2_EXPLOITATION` | ślady eksploatacji | wyrąb, obozy, ślady konfliktu człowiek–las |
| `ASSETSET_R2_GUARDIAN` | forest-guardian presentation | placeholder do czasu finalnego research/name lock |

## R3 — Przymorze

| ID | Rodzina | Zakres |
|---|---|---|
| `ASSETSET_R3_COAST` | wybrzeże | plaża, wydmy/klif wg bible, driftwood, coastal vegetation |
| `ASSETSET_R3_SETTLEMENT` | osadnictwo nadmorskie | budynki, magazyny, warsztaty i portowe props zgodne z research |
| `ASSETSET_R3_WATERCRAFT` | jednostki/pomosty | wyłącznie zatwierdzony zakres transportu i tła |
| `ASSETSET_R3_STORM` | pogoda morska | wiatr, deszcz, mgła, fale/VFX w ramach renderer budget |

## R4 — Kamienne Wyżyny

| ID | Rodzina | Zakres |
|---|---|---|
| `ASSETSET_R4_HIGHLANDS` | skały, przełęcze, pionowość | terrain/rock kits, landmarks |
| `ASSETSET_R4_WYSOKAMIEN` | Wysokamień | regionalna zabudowa i infrastruktura |
| `ASSETSET_R4_MINES` | kopalnie | wejścia, podpory, narzędzia, wnętrza, hazards |
| `ASSETSET_R4_IRON_GATE` | Żelazna Brama | landmark/gate kit |
| `ASSETSET_R4_COLD` | zimno/pogoda | śnieg/szron/mgła tylko wg gameplay/weather spec |

## R5 — Równiny Arel

| ID | Rodzina | Zakres |
|---|---|---|
| `ASSETSET_R5_PLAINS` | otwarte równiny | trawy, małe landmarks, dalekie sylwetki |
| `ASSETSET_R5_SETTLEMENTS` | stałe osady | regionalna zabudowa i gospodarka |
| `ASSETSET_R5_CAMPS` | obozy sezonowe | modularny camp kit; forma zależna od culture research |
| `ASSETSET_R5_OLD_ROADS` | stare drogi i miejsca Arel | navigation landmarks; exact form pozostaje research lock |
| `ASSETSET_R5_WEATHER` | ekspozycja pogodowa | wiatr, burze, mgła/pył wg bible |

## R6 — Pustkowie Pierwszego Progu

| ID | Rodzina | Zakres |
|---|---|---|
| `ASSETSET_R6_WASTELAND` | bazowy teren | jałowe/niestabilne formacje zgodne z bible |
| `ASSETSET_R6_THRESHOLD` | Pierwszy Próg | centralny landmark; presentation podporządkowane author truth |
| `ASSETSET_R6_UNSTABLE` | niestabilne strefy | czytelne gameplayowo VFX/geometry cues + recovery cues |
| `ASSETSET_R6_EXPEDITION` | ekspedycje | camp/equipment variants i evidence props |
| `ASSETSET_R6_FINALE` | powrót w Akcie V | state variants bez tworzenia osobnej kopii regionu |

## Minimalny rekord implementacyjny asset family

Każda rodzina przechodząca do produkcji dostaje:

- `id`, `region`, `owner_document`, `scope`;
- listę wymaganych funkcji gameplay/presentation;
- `source_research` jeśli dotyczy;
- `runtime_format` i import settings;
- collision: none/simple/complex/custom;
- LOD/streaming policy po pomiarach;
- material/texture policy;
- placeholder/fallback dla elementów krytycznych;
- QA: scale/orientation, missing materials, collision, LOD/streaming, save-state variant jeśli dotyczy.

## Kolejność produkcji

1. R0 P0 z `VerticalSliceAssetList.md` — pierwszy grywalny świat;
2. shared kits potrzebne przez R0;
3. R1–R3 families zgodnie z kolejnością kampanii;
4. R4–R6;
5. warianty late-game/finale;
6. final LOD/material/streaming lock po profilowaniu.

## Otwarte decyzje

Nie blokują stworzenia placeholderów ani integracji asset loadera, ale blokują `locked`:

- konkretna liczba wariantów modeli w każdej rodzinie;
- finalne modele i tekstury dostarczone przez art;
- finalna nazwa i wygląd `forest-guardian` po researchu;
- dokładna forma starych miejsc Arel;
- zakres żeglugi i watercraft gameplay;
- VO/audio/VFX content lists;
- mierzone triangle/draw-call/VRAM/streaming budgets;
- target hardware i wynikające z niego LOD-y.

## Definition of Done dla asset list coverage

Asset-list coverage jest spełnione na poziomie planowania, gdy każdy region ma rodziny obejmujące jego terrain/biome, architekturę, krytyczne POI/quest props oraz regionalne VFX/audio dependencies. Production lock wymaga później konkretnych asset records i zmierzonych budżetów.
