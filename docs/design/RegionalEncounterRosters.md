# Regional encounter rosters — R1–R6 v0.1

Owner wykonawczy dla regionalnych tabel encounterów poza R0. Dokument nie zastępuje `EncounterDesign.md`, region bibles ani `ProductionBestiaryRoster.md`; mapuje ich kontrakty na stabilne rodziny encounterów. Konkretne liczby wag, cooldownów, density i rewardów pozostają playtest lockiem.

## Wspólny kontrakt

Każdy rekord runtime ma: `encounterId`, `regionId`, `family`, `trigger`, `timeWindow`, opcjonalne `weatherTags`, `participants`, `telegraph`, `resolutionPaths`, `persistencePolicy`, `cooldownClass`, `questGate` i `evidenceOutputs`. Generator może wybrać tylko rekord zgodny ze stanem świata i nie może respawnować permanentnie rozwiązanej instancji.

| ID | Region | Rodzina | Trigger / warunki | Uczestnicy / rola | Minimalne rozwiązania | Persistence |
|---|---|---|---|---|---|---|
| `ENC_R1_ROAD_TOLL` | R1 | social/political | droga/przeprawa, aktywna kontrola | straż/celnicy/podróżni | zapłata, dokument/status, negocjacja, odwrót | zapisz wynik i relację |
| `ENC_R1_DISPUTED_CROSSING` | R1 | mixed | sporny most/bród | dwie grupy ludzi | mediacja, obejście, wsparcie strony; walka tylko po eskalacji | world-state scoped |
| `ENC_R1_ROAD_INCIDENT` | R1 | tracking/social | szlak poza osadą | podróżni, ślady, opcjonalni napastnicy F | pomoc, śledztwo, uniknięcie | cooldown; quest variant może być one-shot |
| `ENC_R2_GUARDIAN_BOUNDARY` | R2 | supernatural/social | przekroczenie oznaczonej granicy miejsca | `ENTITY_FOREST_GUARDIAN_F` | respekt reguły, obejście, interakcja; walka nie jest domyślna | rozwiązanie trwałe dla instancji |
| `ENC_R2_MISDIRECTION` | R2 | tracking/supernatural | gęsty las, właściwy route-state | `ENTITY_FOREST_MISDIRECTION`/fenomen | tracking, respekt/rytuał, odwrót | cooldown + remembered clue |
| `ENC_R2_FOREST_WORK` | R2 | environmental/social | strefa eksploatacji lasu | pracownicy/podróżni | pomoc, handel informacją, obejście zagrożenia | ambient cooldown |
| `ENC_R3_WATER_PASSAGE` | R3 | supernatural/mixed | szlak wodny/brzeg, lokalny gate | `ENTITY_WATER_ACTOR` | avoidance, offering/ritual, negotiation; combat tylko w wariancie uzasadnionym | zapis sposobu rozwiązania |
| `ENC_R3_SHORE_PREDATOR` | R3 | tracking/combat | mokradło/brzeg poza bezpiecznym portem | regionalny wariant `CREATURE_SWAMP_PREDATOR_F` | uniknięcie, odstraszenie, walka | ecology cooldown/dead-state |
| `ENC_R3_PORT_FRICTION` | R3 | social | port/targ, aktywna wymiana | kupcy, załogi, straż | rozmowa, usługa, reputacja, odejście | krótki cooldown; bez losowej przemocy |
| `ENC_R4_OPEN_GROUND_NOON` | R4 | environmental/supernatural | faktycznie otwarta przestrzeń + południe | `ENTITY_NOON_PHENOMENON` | schronienie, timing, uniknięcie; combat nie jest wymagany | day-window + resolved clue |
| `ENC_R4_WORKSITE_HAZARD` | R4 | environmental | wyrobisko/skład/transport | pracownicy + hazard | pomoc, zabezpieczenie, obejście | site-state scoped |
| `ENC_R4_ROUTE_PRESSURE` | R4 | tracking/social | transport surowców | konwój/przewoźnicy | pomoc, handel, śledzenie, obejście | ambient/quest gate |
| `ENC_R5_FIELD_NOON` | R5 | environmental/supernatural | tylko realne pole/uprawa + południe | `ENTITY_NOON_PHENOMENON` | shelter/timing/avoidance | osobna prezentacja kulturowa; nie kopia R4 |
| `ENC_R5_ROUTE_CAMP` | R5 | social/environmental | route/water/camp state | Arelowie/podróżni | rozmowa, wymiana, pomoc, odejście | zależne od seasonal camp state |
| `ENC_R5_OLD_ROUTE_TRACE` | R5 | tracking | stara droga/landmark | ślady/neutralne `old-site` | obserwacja, evidence, nawigacja | discovered flag |
| `ENC_R6_THRESHOLD_LEAK` | R6 | anomaly | crisis/threshold state | `ANOMALY_THRESHOLD_LEAK` | containment, evidence, route/state manipulation | permanent outcome per instance |
| `ENC_R6_ECHO_CALLBACK` | R6 | mixed | campaign flags z wcześniejszych regionów | zależne od zapisanych resolution types | evidence/interaction/retreat | one-shot lub phase-scoped |
| `ENC_R6_UNSTABLE_ROUTE` | R6 | environmental/anomaly | niestabilny route-state | środowisko/anomalia | obejście, stabilizacja, powrót | route state persisted |

## Reguły regionalne

**R1:** dominują ludzie, administracja, przeprawy i polityka. Nie dodajemy potwora tylko po to, aby region miał encounter nadnaturalny.

**R2:** co najmniej jedna rodzina pozostaje rozwiązywalna bez walki. Guardian zachowuje status świadomej fikcji F; misdirection jest osobnym aktorem/fenomenem.

**R3:** `WATER_ACTOR` wymaga research-backed wariantu regionalnego. Predator jest reuse systemowym z nowym habitat/telegraph, nie kopią R0. Portowe encountery nie mogą zamieniać wielokulturowości w losową przemoc.

**R4:** południca nie jest signature identity regionu; rekord południowy jest sytuacyjny. Worksite hazards nie służą do wymyślania niezweryfikowanych historycznych technologii.

**R5:** `NOON_PHENOMENON` wolno uruchomić tylko przy faktycznej uprawie; nie służy do „słowianizowania” Arelów. Camp/route encountery respektują permanent-settlement + seasonal-camp model.

**R6:** anomalie wynikają z central lore i campaign state. Nie otrzymują folklorystycznej nazwy gatunkowej.

## Selekcja i anty-powtórzenia

1. Najpierw filtruj po regionie, world/quest state, miejscu, czasie i pogodzie; dopiero potem losuj spośród legalnych rekordów.
2. Ten sam `family` nie może zostać wybrany ponownie przed swoim cooldownem.
3. One-shot/permanent resolution usuwa instancję z puli, ale może odblokować authored callback.
4. Generator nie tworzy uczestników za plecami gracza bez telegraphu.
5. Wagi i maksymalna gęstość pozostają symboliczne (`low/normal/high`) do playtest locku; dokument nie wymyśla liczb.

## Quest hooks

Side/main quest może rozszerzyć rodzinę przez `questGate`, ale nie zmienia jej ID semantycznego ani persistence bez jawnej karty questa. Finalne creature targets dla `SQ_R1_02`, `SQ_R2_02`, `SQ_R3_03` i `SQ_R4_02` pozostają osobnymi content lockami; tabela nie udaje, że zostały rozstrzygnięte.

## Minimalne QA

- każda rodzina ma legalną drogę odwrotu lub fail-forward;
- save/load zachowuje resolution type i cooldown/one-shot state;
- encounter nie łamie region bible ani research locku;
- brak spawn-behind-player bez sygnału;
- R2 guardian i R3 water actor mają ścieżkę niewymagającą walki;
- R4/R5 noon records nie aktywują się poza poprawnym time/place context;
- R6 callback czyta istniejące flagi zamiast przepisywać historię decyzji gracza.

## Otwarte locki

Finalne weights/density/cooldown durations, dokładne POI placement, combat tuning, loot/rewards, asset manifests oraz cztery quest-specific creature targets pozostają do playtest/content/art locku. Nie blokują implementacji tabel, filtrów, persistence ani authored encounter state machines.
