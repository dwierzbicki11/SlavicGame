# Regional Content / Asset Catalog — R0–R6

Status: production planning v0.1. Dokument jest regionalnym indeksem wykonawczym dla `ProductionContentCatalog.md` i production region bibles. Nie duplikuje kart questów ani research cards. Ilości wymagające level-designu, art locku lub profilowania pozostają jawnie otwarte.

## Kontrakt wspólny

Każdy region ma własny `ASSETSET_Rx_CORE` i katalog rodzin contentu. Konkretne modele, audio, VFX i warianty otrzymują osobne rekordy dopiero po wyborze produkcyjnym. Reuse między regionami jest domyślny dla materiałów natury, podstawowych propsów, humanoid rigów i wspólnych systemów; unikalność jest rezerwowana dla landmarków, kluczowych kultur i czytelności kampanii.

Każdy rekord regionu musi wskazywać owner document, stabilne ID, dependencies, research/art lock oraz minimalne QA. Finalne liczby NPC, encounterów, osad, wariantów i LOD nie są ustalane bez playtestu/profilowania.

## R0 — Żarnowiec
**Owner:** `world/RegionBibleR0Zarnowiec.md`. **Asset set:** `ASSETSET_R0_CORE`.

Content: wieś/hub, las, bagno, kaplica/ruina, drogi, vertical slice, tracking/combat/alchemy, lokalni NPC i creature encounters. Assety: zabudowa drewniana, palisady, clutter leśny, mokradła, teren, ruina, gospodarstwo, hunter equipment, bazowe VFX i ambient. R0 jest źródłem bazowego rural/forest kitu dla R1/R2; landmarki vertical slice pozostają unikalne.

## R1 — Nadborze
**Owner:** `world/RegionBibleR1Nadborze.md`. **Asset set:** `ASSETSET_R1_CORE`.

Content: Dębrzyn, gród przeprawowy, osada młynarska, rzeka, stare miejsce sieci, administracja/cła, transport rzeczny. Assety: gród/podgrodzie, palisady, mosty/brody, młyny, przystanie/łodzie/magazyny, pola, warsztaty, kult, stare drogi i modularny riverbank. Reuse: rural kit R0 + nowy river/civic kit; landmarki Dębrzyna unikalne.

## R2 — Wielki Bór
**Owner:** `world/RegionBibleR2WielkiBor.md`. **Asset set:** `ASSETSET_R2_CORE`.

Content: głęboki las, polany, stare miejsca, szlaki łowieckie, anomalie, tracking-heavy encounters. Assety: warstwy drzewostanu, understory, martwe drewno, mchy/grzyby, ścieżki, obozowiska, stare znaki, naturalne landmarki, canopy/fog/light shafts i creature dens. `forest-guardian` pozostaje identity/research lockiem.

## R3 — Przymorze
**Owner:** `world/RegionBibleR3Przymorze.md`. **Asset set:** `ASSETSET_R3_CORE`.

Content: wybrzeże/ujścia, rybackie osadnictwo, szlaki wodne, wraki, sztormy i handel. Assety: brzeg, klify/wydmy zależnie od level locku, pomosty, łodzie, wyposażenie rybackie, magazyny, mokre materiały, wraki, fale/piana/deszcz/mgła i coastal ambient. Technologia jednostek pływających wymaga material-culture research locku.

## R4 — Kamienne Wyżyny
**Owner:** `world/RegionBibleR4KamienneWyzyny.md`. **Asset set:** `ASSETSET_R4_CORE`.

Content: wysokości, przełęcze, kamienne szlaki, izolowane osady, jaskinie/ruiny i traversal encounters. Assety: skały, urwiska, piargi, drogi, mostki, architektura zgodna z research lockiem, warsztaty, jaskinie, roślinność i wiatr. Geologia, skyline i landmarki są głównym kosztem unikalnym.

## R5 — Równiny Arel
**Owner:** `world/RegionBibleR5RowninyArel.md`. **Asset set:** `ASSETSET_R5_CORE`.

Content: otwarte równiny, szlaki, kontakty kulturowe/polityczne, mobilność i encountery otwartej przestrzeni. Assety: grassland, niska roślinność, drogi, landmarki, obozy i infrastruktura podróżna. Architektura, stroje i ornamenty są zablokowane do culture research package; nie projektować ich przez stereotypową analogię do „stepu”.

## R6 — Pustkowie Pierwszego Progu
**Owner:** `world/RegionBibleR6PustkowiePierwszegoProgu.md`. **Asset set:** `ASSETSET_R6_CORE`.

Content: późnogrowa strefa kryzysu, relikty/progi sieci, anomalie, high-risk traversal i central-lore locations. Assety: zdegradowany terrain kit, relikty, anomaly VFX, zmienione weather/lighting, ślady zniszczeń, high-tier creature presentation i finałowe audio motifs. Reuse R0–R5 ma podkreślać ciągłość świata przez damage/anomaly variants.

## Wspólne katalogi przekrojowe

Wspólne rodziny: `ASSETSET_SHARED_HUMANOID`, `ASSETSET_SHARED_CREATURE_BASE`, `ASSETSET_SHARED_WEATHER`, `ASSETSET_SHARED_UI_WORLD`, `ASSETSET_SHARED_INTERACTABLES`. Creature assety mapują się na `ProductionBestiaryRoster.md` i `ResearchCardIndex.md`; NPC na `ProductionNpcRoster.md`. Elementy kulturowe i religijne nie mogą być `locked` bez research ownera.

## LOD, streaming i warianty

Każda rodzina środowiskowa docelowo ma politykę LOD/culling/collision zgodną z pomiarami. Teraz rekord wskazuje jedynie landmark/powtarzalny/interactable/dekoracja. Progi odległości, polycount, texture budget, liczby wariantów i memory residency pozostają performance/art lockiem. Priorytet wariantów: silhouette/gameplay readability, eliminacja widocznego powtarzania, kosmetyczna różnorodność.

## Minimalne QA

- każdy asset family ma jednego regionalnego lub wspólnego ownera;
- main-quest asset ma fallback lub plan dostarczenia;
- historyczny/kulturowy asset ma research ref albo jawny lock;
- landmark nie jest jedynym nośnikiem krytycznej informacji;
- region można streamować bez utraty trwałego stanu;
- shared asset nie jest kopiowany pod nowym ID;
- performance budgets nie są deklarowane przed pomiarami.

## Otwarte decyzje

1. konkretne modele/materials/animations/audio/VFX i finalne ID;
2. finalne liczby wariantów oraz POI/encounterów per region;
3. culture-locked presentation R5 i część material culture R1/R3/R4;
4. identity/art lock `forest-guardian`;
5. finalny layout i weather presentation;
6. LOD, streaming, texture, draw-call, AI i memory budgets po profilowaniu;
7. produkcyjny koszt i kolejność zależne od realnej przepustowości zespołu.

## Definition of Ready

Katalog zamyka rodzinny scope content/assets R0–R6 na poziomie planowania. Można tworzyć konkretne manifesty, side-quest catalog i encounter rosters bez ponownego ustalania klas zasobów regionu. `locked` pozostaje zarezerwowane dla finalnych assetów po research/art/performance/QA.
