# Regional Content / Asset Catalog — R0–R6

Status: production planning v0.1. Dokument jest regionalnym indeksem wykonawczym dla `ProductionContentCatalog.md` i production region bibles. Nie duplikuje kart questów ani research cards. Ilości wymagające level-designu, art locku lub profilowania pozostają jawnie otwarte.

## Kontrakt wspólny

Każdy region ma własny `ASSETSET_Rx_CORE` i katalog rodzin contentu. Konkretne modele, audio, VFX i warianty otrzymują osobne rekordy dopiero po wyborze produkcyjnym. Reuse między regionami jest domyślny dla materiałów natury, podstawowych propsów, humanoid rigów i wspólnych systemów; unikalność jest rezerwowana dla landmarków, kluczowych kultur i czytelności kampanii.

Każdy rekord regionu musi wskazywać owner document, stabilne ID, dependencies, research/art lock oraz minimalne QA. Finalne liczby NPC, encounterów, osad, wariantów i LOD nie są ustalane bez playtestu/profilowania.

## R0 — Żarnowiec

**Owner:** `world/RegionBibleR0Zarnowiec.md`. **Asset set:** `ASSETSET_R0_CORE`.

Content families: wieś/hub, las, bagno, kaplica/ruina, lokalne drogi i ścieżki, vertical-slice kontrakty, tutorialowe tracking/combat/alchemy, lokalni NPC i podstawowe creature encounters.

Asset families: zabudowa drewniana, płoty/palisady, leśne clutter sets, mokradła, droga/błoto/trawa/ściółka, ruina/kaplica, podstawowe narzędzia i propsy gospodarstwa, hunter equipment, bazowe VFX pogody/ognia/magii oraz ambient wieś–las–bagno.

Reuse strategy: R0 jest źródłem bazowego zestawu rural/forest dla R1/R2. Unikalne pozostają landmark vertical slice i presentation kluczowych NPC. Obowiązuje istniejący `VerticalSliceAssetList.md`.

## R1 — Nadborze

**Owner:** `world/RegionBibleR1Nadborze.md`. **Asset set:** `ASSETSET_R1_CORE`.

Content families: Dębrzyn, gród przeprawowy, osada młynarska, Dębrza i brzegi, stare miejsce sieci, stara droga, administracja/cła, transport rzeczny, kontrakty drogowe i wodne.

Asset families: gród/podgrodzie, zabudowa rolnicza, palisady/bramy, mosty/brody, młyny i infrastruktura wodna, przystanie/łodzie/magazyny, pola/płoty/drogi, propsy ceł i danin, warsztaty wyższego tieru, współczesny kult, stare drogi/kamienie/sieć, modularny riverbank set.

Reuse strategy: rural kit z R0 + nowy modularny river/civic kit. Dębrzyn dostaje unikalne landmark silhouettes; zwykłe domy i clutter mają korzystać z wariantów wspólnych.

## R2 — Wielki Bór

**Owner:** `world/RegionBibleR2WielkiBor.md`. **Asset set:** `ASSETSET_R2_CORE`.

Content families: głęboki las, polany/osady skrajne, stare miejsca, szlaki łowieckie, anomalie leśne, tracking-heavy encounters, relacje człowiek–las i forest-guardian slot.

Asset families: kilka warstw drzewostanu i understory, martwe drewno/mchy/grzyby, leśne ścieżki i przeprawy, małe obozowiska/szałasy, stare kamienie/znaki, naturalne landmarki, canopy/fog/light shafts, creature den props, leśny ambient i czytelne audio trackingowe.

Reuse strategy: bazowe drzewa i clutter R0 mogą wracać, ale R2 wymaga własnej kompozycji canopy, landmarków i gęstości. `forest-guardian` presentation pozostaje identity/research lock; nie tworzyć folklorystycznej nazwy na potrzeby assetu.

## R3 — Przymorze

**Owner:** `world/RegionBibleR3Przymorze.md`. **Asset set:** `ASSETSET_R3_CORE`.

Content families: wybrzeże/ujścia, portowe lub rybackie osadnictwo, szlaki wodne, wraki i strefy sztormowe, handel dalekiego zasięgu, coastal supernatural encounters.

Asset families: brzeg morski, klify/wydmy zależnie od finalnego level locku, pomosty/przystanie, łodzie i wyposażenie rybackie, magazyny, sieci/liny/beczki, mokre drewno/kamień, wraki, fale/piana/deszcz/mgła, ptaki i coastal ambient.

Reuse strategy: łodzie/wood props mogą dzielić bazę z R1, ale weather/water presentation i główne landmarki muszą odróżniać Przymorze. Finalna technologia i forma jednostek pływających wymaga material-culture research locku.

## R4 — Kamienne Wyżyny

**Owner:** `world/RegionBibleR4KamienneWyzyny.md`. **Asset set:** `ASSETSET_R4_CORE`.

Content families: wysokości, przełęcze, kamienne szlaki, wydobycie/obróbka surowców jeśli potwierdzone przez bible, izolowane osady, jaskinie/ruiny i traversal encounters.

Asset families: skały/urwiska/piargi, górskie drogi i mostki, kamienno-drewniana architektura zgodna z research lockiem, warsztaty surowcowe, wejścia do jaskiń, wysokogórska roślinność, śnieg/szron tylko tam gdzie owner świata go wymaga, wiatr i dalekie landmark silhouettes.

Reuse strategy: wspólne propsy humanoidów i część drewna; geologia, skyline i landmarki stanowią główny unikalny koszt. Nie ustalać snow coverage bez weather/level locku.

## R5 — Równiny Arel

**Owner:** `world/RegionBibleR5RowninyArel.md`. **Asset set:** `ASSETSET_R5_CORE`.

Content families: otwarte równiny, dalekie szlaki, kontakty kulturowe/polityczne, mobilność i widoczność na dystans, obozy/osady zgodne z finalnym culture package, encountery wykorzystujące otwartą przestrzeń.

Asset families: grassland biome, niska roślinność, drogi/szlaki, dalekie landmarki, modularne obozy i infrastruktura podróżna, zestawy kulturowe dopiero po research package, weather presentation dla otwartej przestrzeni, fauna i ambient równin.

Reuse strategy: techniczne materiały terenu mogą być wspólne, lecz architektura, stroje, ornamenty i propsy kulturowe są zablokowane do culture research package. Nie projektować ich przez analogię do stereotypowego „stepu”.

## R6 — Pustkowie Pierwszego Progu

**Owner:** `world/RegionBibleR6PustkowiePierwszegoProgu.md`. **Asset set:** `ASSETSET_R6_CORE`.

Content families: późnogrowa strefa kryzysu, relikty/progi sieci, anomalie, wysokiego ryzyka traversal/encounters, central-lore locations i presentation skutków destabilizacji świata.

Asset families: zdegradowany terrain kit, relikty sieci i unikalne landmarki, anomaly VFX, zmienione weather/lighting layers, zniszczone lub opuszczone ślady wcześniejszego użytkowania, high-tier creature presentation, finałowe audio motifs i czytelne safe-route markers.

Reuse strategy: świadomy reuse materiałów z R0–R5 pokazuje ciągłość świata, ale jest modyfikowany przez damage/anomaly variants. Unikalny budżet należy koncentrować na Progu, central-lore POI i efektach kryzysu, nie na nowych odpowiednikach zwykłych propsów.

## Wspólne katalogi przekrojowe

Do wszystkich regionów należą wspólne rodziny: `ASSETSET_SHARED_HUMANOID`, `ASSETSET_SHARED_CREATURE_BASE`, `ASSETSET_SHARED_WEATHER`, `ASSETSET_SHARED_UI_WORLD`, `ASSETSET_SHARED_INTERACTABLES` oraz wspólne materiały/decals. Regionalny katalog ma referencję do tych zestawów zamiast kopiowania assetów.

Creature assety muszą mapować się na `ProductionBestiaryRoster.md` i `ResearchCardIndex.md`. NPC presentation mapuje się na `ProductionNpcRoster.md`. Elementy kulturowe i religijne nie mogą uzyskać statusu `locked` bez właściwego research ownera.

## LOD, streaming i warianty

Każda rodzina środowiskowa powinna docelowo mieć politykę LOD/culling/collision zgodną z pomiarami. Na obecnym etapie wymagamy jedynie, aby rekord wskazywał, czy jest landmarkiem, elementem powtarzalnym, interactable czy dekoracją. Konkretne progi odległości, polycount, texture budget, liczby wariantów i memory residency pozostają performance/art lockiem.

Priorytet wariantów: najpierw silhouette/gameplay readability, następnie eliminacja widocznego powtarzania, dopiero potem kosmetyczna różnorodność. Reuse jest preferowany, jeśli nie niszczy tożsamości regionu.

## Minimalne QA katalogu regionalnego

- każdy asset family ma dokładnie jednego regionalnego lub wspólnego ownera;
- nie ma assetu wymaganego przez main quest bez fallbacku lub planu dostarczenia;
- historyczny/kulturowy asset ma research ref albo jawny lock;
- landmark nie jest jedynym nośnikiem krytycznej informacji bez alternatywnej czytelności;
- region można streamować bez utraty trwałego stanu quest/NPC/encounter;
- shared asset nie jest kopiowany pod nowym ID tylko dla wygody;
- finalne liczby i performance budgets nie są deklarowane przed pomiarami.

## Otwarte decyzje

1. konkretne modele/materials/animations/audio/VFX i ich finalne ID;
2. finalne liczby wariantów oraz POI/encounterów per region;
3. culture-locked presentation R5 oraz część material culture R1/R3/R4;
4. identity/art lock `forest-guardian`;
5. finalny layout i weather presentation poszczególnych stref;
6. LOD, streaming, texture, draw-call, AI i memory budgets po profilowaniu;
7. produkcyjny koszt i kolejność wykonania zależne od realnej przepustowości zespołu.

## Definition of Ready dla następnego etapu

Ten katalog zamyka rodzinny scope content/assets R0–R6 na poziomie planowania. Następny etap może tworzyć konkretne manifesty assetów, side-quest catalog i encounter rosters bez ponownego ustalania, jakie klasy zasobów należą do regionu. `locked` pozostaje zarezerwowane dla finalnych assetów po research/art/performance/QA.