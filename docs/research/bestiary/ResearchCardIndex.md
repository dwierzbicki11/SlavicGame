# Indeks research cards bestiariusza — production v0.1

Ten indeks łączy `bestiary/ProductionBestiaryRoster.md` z materiałem badawczym. Jest bramką produkcyjną: obecność nazwy w źródłach lub starszym backlogu nie oznacza automatycznie wejścia do scope 1.0.

## Statusy

- **ready** — istnieje karta wystarczająca do bezpiecznej adaptacji na obecnym poziomie; konkretna karta contentowa nadal wskazuje wariant regionalny.
- **partial** — materiał istnieje, lecz nie wystarcza do finalnego identity/art locku.
- **F** — świadoma fikcja SlavicGame; nie wymaga udawanej folklorystycznej genezy.
- **candidate** — research istnieje lub jest rozpoczęty, ale istota nie jest obiecanym contentem 1.0.

## Roster 1.0 → research owner

| Production ID | Research owner | Status | Gate produkcyjny |
|---|---|---|---|
| `CREATURE_SWAMP_PREDATOR_F` | brak — świadoma fikcja | F | ecology/art lock, bez historyzacji |
| `ENTITY_MISSING_ECHO_F` | brak — central lore/fikcja | F | quest/evidence presentation |
| `ENTITY_FOREST_GUARDIAN_F` | `ForestSpirit.md`, `RegionalForestFiguresPL.md` | partial | zachować ID `F`; finalna historyczna tożsamość tylko po osobnym identity locku |
| `ENTITY_WATER_ACTOR` | `WaterSpirit.md`, `TopielecWaterSpiritPL.md` | ready na poziomie rodziny motywów | karta regionalna musi wskazać konkretną interpretację; nie sklejać automatycznie wodnika i topielca |
| `ENTITY_NOON_PHENOMENON` | `Poludnica.md` | ready v0.2 na poziomie rdzenia encounteru | source-strength i R4/R5 region-fit zamknięte; finalne appearance/placement nadal data/art lock |
| `ENTITY_NIGHT_PRESSURE` | `Zmora.md` | ready na poziomie motywu | finalna karta encounteru określa lokalną interpretację |
| `ENTITY_REVENANT_FAMILY` | `Strzygon.md`, `Upior.md` | ready na poziomie rodziny | nie utożsamiać terminów bez uzasadnienia regionalnego |
| `ENTITY_FOREST_MISDIRECTION` | `ForestSpirit.md`, `RegionalForestFiguresPL.md` | ready jako szeroki motyw, partial dla nazwy | mechanika może być implementowana bez finalnej nazwy folklorystycznej |
| `ANOMALY_THRESHOLD_LEAK` | central lore, nie folklor | F | zgodność z `FourthSphereTruth.md` i quest state |

## Kandydaci poza obowiązkowym rosterem

| Kandydat | Karta | Stan scope |
|---|---|---|
| rusałka | `Rusalka.md` | candidate; brak obietnicy 1.0 |
| boginka/mamuna | `BoginkaMamuna.md` | candidate; rozdzielić warianty przed użyciem |
| topielec jako osobny byt | `TopielecWaterSpiritPL.md` | candidate; nie wynika automatycznie z `ENTITY_WATER_ACTOR` |
| regionalne figury leśne | `RegionalForestFiguresPL.md` | candidate/identity evidence dla R2 |

`BestiarySources.md`, `BestiarySources02.md` i `BestiaryPass02Summary.md` pozostają indeksami/proweniencją researchu, a nie kartami istot. `VerticalSliceFit.md` jest dokumentem doboru contentu, nie źródłem folklorystycznym.

## Braki do domknięcia

1. **Forest guardian:** osobny identity-lock pass. Jeśli źródła nie uzasadnią jednej konkretnej tożsamości dla R2, `ENTITY_FOREST_GUARDIAN_F` pozostaje świadomie fikcyjnym guardianem i nie jest to błąd dokumentacji.
2. **Ogniki/błędne światła:** utworzyć kartę zjawiska dopiero, gdy konkretny quest/region awansuje je do finalnego scope; obecnie nie są wymaganym slotem rosteru 1.0.
3. Każdy nowy kandydat musi najpierw dostać wpis tutaj i decyzję scope, zanim powstanie kosztowny model/VFX/VO.

## Kryterium research-ready dla finalnej istoty

Karta finalna musi jawnie oddzielać: (a) co mówi źródło, (b) region i okres materiału, (c) poziom pewności, (d) późniejsze/popkulturowe naleciałości, (e) decyzję adaptacyjną gry. Nie wolno wypełniać luk współczesnymi listami internetowymi ani traktować rekonstrukcji jako bezspornego faktu IX–X wieku.

## Konsekwencje dla implementacji

Source-strength południcy nie blokuje już implementacji jej rdzenia encounteru (`time-of-day + cultivated field + telegraph + avoidance`). Pozostałe braki nie blokują wspólnego creature/encounter state model, persistence, evidence ani AI frameworku. Blokują wyłącznie finalne nazwy, art direction, weakness tables lub regionalną interpretację tych slotów, których dotyczą.