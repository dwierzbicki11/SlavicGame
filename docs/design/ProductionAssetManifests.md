# Production Asset Manifests — R0–R6

Status: P4 production manifest v0.1. Owner dla konkretnych rekordów assetów wynikających z `RegionalContentAssetCatalog.md`. Manifest nie ustala polycountów, rozdzielczości tekstur, finalnych wariantów ani kosztów pamięci przed profilowaniem. Asset historyczny/kulturowy może przejść do `historical-final` tylko zgodnie z `research/material-culture/ProductionAssetResearchLock.md`.

## Kontrakt rekordu

Każdy rekord posiada stabilne `assetId`, `familyId`, typ, region/reuse scope, owner, dependency/lock, stan oraz minimalne QA. Dozwolone stany: `placeholder`, `production-ready-family`, `art-lock`, `research-lock`, `performance-lock`, `final`. Brak dokładnego research locatora nie blokuje neutralnego placeholdera ani modularnej rodziny, ale blokuje twierdzenie o historycznej finalności.

Minimalny runtime/data record:

```text
assetId
familyId
kind                 # model/material/texture/rig/animation/audio/vfx
scope                # shared | R0..R6
ownerDoc
researchRef?
dependencies[]
state
lodPolicy             # measured-later dopóki brak profilu
streamingClass        # landmark/repeated/interactable/decorative
qaTags[]
```

## Shared — baza reuse

| assetId | Rodzina / typ | Scope | Lock | QA |
|---|---|---|---|---|
| `AST_SHARED_HUMANOID_RIG_01` | bazowy humanoid rig | shared R0–R6 | art-lock | retarget + equipment sockets |
| `AST_SHARED_INTERACTABLE_CONTAINER_01` | skrzynia/pojemnik modularny | shared | material-culture family pass | interaction/collision/save |
| `AST_SHARED_FIRE_HEARTH_01` | palenisko + VFX hook | shared | exact form research-lock | light/VFX/gameplay trigger |
| `AST_SHARED_ROAD_DIRT_01` | droga/ścieżka spline kit | R0/R1/R2/R5 | art-lock | terrain blend/navigation |
| `AST_SHARED_FOREST_GROUND_01` | forest ground material family | R0/R1/R2 | art-lock | tiling/streaming |
| `AST_SHARED_WEATHER_RAIN_01` | rain presentation hook | shared weather | performance-lock | visibility/gameplay/readability |
| `AST_SHARED_WEATHER_FOG_01` | fog presentation hook | shared weather | performance-lock | gameplay readability |

## R0 — Żarnowiec

| assetId | Rodzina / typ | Reuse | Lock |
|---|---|---|---|
| `AST_R0_RURAL_HOUSE_MOD_01` | modular rural house family | source dla R1 | exact construction research-lock |
| `AST_R0_PALISADE_MOD_01` | palisade/fence family | R1 | exact construction research-lock |
| `AST_R0_MARSH_SURFACE_01` | marsh water/ground material set | regional | art/performance-lock |
| `AST_R0_RUIN_LANDMARK_01` | vertical-slice ruin landmark | unique | art-lock; lore owner R0 |
| `AST_R0_HUNTER_PROPSET_01` | hunter/tool prop family | shared selective | exact historical forms per locator |

## R1 — Nadborze

| assetId | Rodzina / typ | Reuse | Lock |
|---|---|---|---|
| `AST_R1_GROD_MOD_01` | gród/podgrodzie structural family | regional | exact construction research-lock |
| `AST_R1_RIVERBANK_MOD_01` | modular riverbank | R3 selective | art/performance-lock |
| `AST_R1_CROSSING_MOD_01` | bridge/ford gameplay family | regional | exact structure research-lock |
| `AST_R1_LANDING_MOD_01` | landing/jetty family | R3 | material-culture research-lock |
| `AST_R1_STORAGE_PROPSET_01` | storage/trade props | R3/R4 | family pass; exact forms per locator |

## R2 — Wielki Bór

| assetId | Rodzina / typ | Reuse | Lock |
|---|---|---|---|
| `AST_R2_FOREST_CANOPY_01` | deep-forest canopy set | regional | performance-lock |
| `AST_R2_UNDERSTORY_01` | understory/deadwood set | R0 selective | art/performance-lock |
| `AST_R2_OLD_SIGN_PROPSET_01` | old signs/markers | regional | lore/art-lock; no false historicity |
| `AST_R2_NATURAL_LANDMARK_01` | natural landmark family | regional | level/art-lock |
| `AST_R2_GUARDIAN_PRESENTATION_01` | forest-guardian presentation hooks | unique | identity CLOSED/F; final appearance art-lock |

## R3 — Przymorze

| assetId | Rodzina / typ | Reuse | Lock |
|---|---|---|---|
| `AST_R3_COAST_GROUND_01` | coast/wet-ground materials | regional | art/performance-lock |
| `AST_R3_DOCK_MOD_01` | dock/landing family | R1 reuse | material-culture research-lock |
| `AST_R3_BOAT_FAMILY_01` | watercraft gameplay family | R1 selective | exact technology research-lock |
| `AST_R3_FISHING_PROPSET_01` | fishing prop family | regional | exact forms research-lock |
| `AST_R3_WRECK_MOD_01` | wreck family | regional | derives from locked watercraft, otherwise F/neutral |
| `AST_R3_WAVE_FOAM_VFX_01` | wave/foam VFX hook | regional | performance-lock |

## R4 — Kamienne Wyżyny

| assetId | Rodzina / typ | Reuse | Lock |
|---|---|---|---|
| `AST_R4_ROCK_CLIFF_01` | rock/cliff modular set | regional | geology/art-lock |
| `AST_R4_SCREE_GROUND_01` | scree/stone ground | regional | geology/art-lock |
| `AST_R4_WORKSITE_PROPSET_01` | extraction/storage/worksite family | regional | technology research-lock |
| `AST_R4_WORKSHOP_MOD_01` | workshop family | regional | material-culture research-lock |
| `AST_R4_CAVE_MOD_01` | cave kit | R6 selective | level/performance-lock |

## R5 — Równiny Arel

| assetId | Rodzina / typ | Reuse | Lock |
|---|---|---|---|
| `AST_R5_GRASSLAND_01` | grassland/low vegetation set | regional | art/performance-lock |
| `AST_R5_ROUTE_MARKER_01` | neutral route/landmark family | regional | culture art-lock for ornament |
| `AST_R5_CAMP_SYSTEM_01` | seasonal camp structural family | regional | culture/technology research-lock |
| `AST_R5_PERMANENT_SETTLEMENT_01` | permanent settlement family | regional | culture/architecture research-lock |
| `AST_R5_OLD_SITE_01` | neutral old-site family | regional | lore/art-lock |

Nie dodawać stereotypowych „stepowych” ornamentów, ubioru ani konstrukcji jako shortcutu. Do czasu mocniejszego locatora rekordy kulturowe pozostają neutralne/placeholder.

## R6 — Pustkowie Pierwszego Progu

| assetId | Rodzina / typ | Reuse | Lock |
|---|---|---|---|
| `AST_R6_DAMAGED_WORLD_VARIANTS_01` | damage variants R0–R5 | cross-region | art/performance-lock |
| `AST_R6_THRESHOLD_RELIC_01` | central-lore relic family | unique | author-truth + art-lock |
| `AST_R6_ANOMALY_VFX_01` | anomaly VFX family | unique | VFX/performance-lock |
| `AST_R6_CRISIS_WEATHER_01` | crisis weather presentation | unique | VFX/audio/performance-lock |
| `AST_R6_FINAL_LANDMARK_01` | final threshold landmark | unique | lore/level/art-lock |

## Rig / animation / audio / VFX manifest hooks

Concrete clips i cues dostają własne ID dopiero w następnym budget pass. Już teraz obowiązują rodziny: `ANIM_SHARED_HUMANOID_LOCOMOTION`, `ANIM_SHARED_INTERACTION`, `ANIM_CREATURE_*`, `AUD_AMB_Rx_*`, `AUD_FOLEY_SHARED_*`, `AUD_MUSIC_Rx_*`, `VFX_WEATHER_*`, `VFX_MAGIC_*`, `VFX_ANOMALY_*`. Manifest nie deklaruje liczby klipów ani emitters bez production/art locku.

## Dependencies i fail-forward

- brak finalnego modelu nie może blokować quest graphu: używany jest placeholder tej samej rodziny i ID logicznego;
- brak historycznego locatora utrzymuje `research-lock`, nie prowadzi do zgadywania detalu;
- main-quest landmark ma obowiązkowy placeholder/fallback przed integracją questa;
- asset ID nie koduje wersji artystycznej; warianty i LOD są children/metadanymi, nie nowym gameplay ID;
- shared asset nie jest kopiowany do regionu tylko dla zmiany materiału/zużycia;
- R6 preferuje damage/anomaly variants istniejących rodzin, żeby zachować ciągłość świata i ograniczyć koszt.

## Minimalne QA per rekord

1. owner i dependency istnieją;
2. research-sensitive rekord ma locator/ref albo jawny `research-lock`;
3. collision/navigation/interakcja są testowane, jeśli dotyczą gameplayu;
4. persistence nie zależy od konkretnego renderowanego wariantu;
5. landmark ma fallback i nie jest jedynym nośnikiem krytycznej informacji;
6. LOD/streaming nie otrzymują wymyślonych progów przed pomiarem;
7. zmiana placeholder → final nie zmienia stabilnego logicznego odwołania contentu.

## Otwarte locki

Finalne model/material/texture IDs, liczby wariantów, dokładne konstrukcje historyczne, costume/ornament, konkretne rig/clip/audio/VFX rekordy, polycount, texture resolution, LOD distance, draw calls i residency pozostają art/research/performance lockiem. Ten manifest zamyka natomiast **konkretne produkcyjne rodziny i stabilne punkty integracji R0–R6**, dzięki czemu art/content/engine mogą pracować bez ponownego definiowania scope.