# Production bestiary roster — v0.1

Ten dokument jest kontraktem scope 1.0 pomiędzy `BestiaryBible.md`, research cards, region bibles i produkcją encounterów. Nie zmienia folklorystycznych hipotez w fakty: nazwa historyczna jest używana tylko tam, gdzie karta badawcza pozwala na bezpieczną adaptację. `F` oznacza świadomą fikcję SlavicGame.

## Statusy

- **locked-F** — rola gameplayowa jest produkcyjnie ustalona, nazwa/postać pozostaje fikcyjna;
- **research-backed** — istnieje karta badawcza, ale konkretna adaptacja nadal musi respektować wariant regionalny;
- **research-required** — slot jest potrzebny projektowo, lecz nie wolno nadawać mu folklorystycznej tożsamości przed research card.

## Roster obowiązkowy scope 1.0

| ID produkcyjne | Klasa | Regiony | Funkcja encounteru | Status źródłowy | Rozwiązania wymagane |
|---|---|---|---|---|---|
| `CREATURE_SWAMP_PREDATOR_F` | predator | R0, warianty mokradeł R3 | fizyczny trop, presja w śledztwie, walka opcjonalna zależnie od encounteru | locked-F | walka, odstraszenie/unikanie |
| `ENTITY_MISSING_ECHO_F` | echo/apparition | R0 | uczy rozdzielania zjawy od fizycznego drapieżnika | locked-F | obserwacja, rytuał, evidence resolution |
| `ENTITY_FOREST_GUARDIAN_F` | guardian | R2 | uczy, że istota nadnaturalna nie jest automatycznie przeciwnikiem | locked-F; identity lock otwarty | respekt reguły miejsca, obejście, negocjacyjna interakcja |
| `ENTITY_WATER_ACTOR` | intelligent/local water actor | R3 | konflikt szlaku wodnego i warunków miejsca | research-backed: wodnik/vodník package | avoidance, offering/ritual, negotiation; combat tylko jeśli karta encounteru uzasadnia |
| `ENTITY_NOON_PHENOMENON` | seasonal/time-bound | R4/R5 | presja pory dnia i otwartej przestrzeni | research-required: południca | traversal timing, shelter/avoidance; finalny combat lock po researchu |
| `ENTITY_NIGHT_PRESSURE` | apparition/night phenomenon | regionalnie | sen/noc, śledztwo bez prostego HP targetu | research-backed: zmora package | preparation, diagnosis, ritual/avoidance |
| `ENTITY_REVENANT_FAMILY` | dangerous revenant slot | późna kampania, lokalizacja zależna od questa | wysokie zagrożenie fizyczne + evidence przed identyfikacją | research-backed: strzygoń/strzyga package | preparation, combat lub quest-specific containment |
| `ENTITY_FOREST_MISDIRECTION` | forest phenomenon/actor | R2 | zmiana tras, dezorientacja, test tracking | research-backed broad forest-spirit package | tracking, respect/ritual, retreat |
| `ANOMALY_THRESHOLD_LEAK` | anomaly | R6 + callbacki | manifestuje kryzys sfer bez udawania folklorystycznego gatunku | locked-F / central lore | containment, evidence, route/state manipulation |

## Kandydaci, nie scope-lock

Rusałka, utopiec, kikimora, mamuna, boginka i domowik pozostają kandydatami do osobnych research cards lub side contentu. Nie wolno traktować ich jako obiecanej zawartości 1.0 tylko dlatego, że występują w kolejce badawczej `BestiaryBible.md`.

## Kontrakt regionalny

- **R0:** minimum `SWAMP_PREDATOR_F` + `MISSING_ECHO_F`; oba muszą dać rozróżnialne ślady.
- **R1:** nacisk na ludzkie/polityczne encountery; nadnaturalny roster nie jest obowiązkowo rozszerzany.
- **R2:** `FOREST_GUARDIAN_F` + `FOREST_MISDIRECTION`; co najmniej jeden encounter bez walki.
- **R3:** `WATER_ACTOR`; mokradłowy predator może wrócić wyłącznie jako wariant ekologiczny, nie copy-paste R0.
- **R4:** slot `NOON_PHENOMENON` dopiero po research locku; wcześniej używać placeholdera funkcjonalnego bez nazwy folklorystycznej.
- **R5:** time/open-land pressure może współdzielić system z R4, ale prezentacja kulturowa wymaga własnego kontekstu.
- **R6:** `THRESHOLD_LEAK`; anomalie są konsekwencją central lore, nie „nowym gatunkiem potwora”.

## Minimalna karta implementacyjna każdej istoty

Każdy rekord, zanim otrzyma status implementation-ready, musi wskazać: trwałe ID, owner research/design, habitat, activity window, neutral behavior, escalation/de-escalation, senses, traces/evidence, encounter role, dozwolone resolution paths, loot/reward policy, persistence po rozwiązaniu, save/load state, animation/VFX/audio dependencies oraz minimalne QA.

## Persistence

Nie zapisujemy wyłącznie `dead=true`. Minimalny stan encounteru to `unseen`, `observed`, `identified`, `resolved`, a resolution posiada typ (`killed`, `driven_off`, `appeased`, `contained`, `helped`, `avoided_persistently`). Quest może rozszerzyć stan, ale nie może utracić sposobu rozwiązania potrzebnego reputacji, journalowi i epilogowi.

## Research gates

1. `ENTITY_NOON_PHENOMENON` nie dostaje finalnej nazwy, modelu ani weakness table przed kartą południcy.
2. `ENTITY_FOREST_GUARDIAN_F` zachowuje ID F, dopóki osobny identity lock nie uzasadni konkretnej tradycji regionalnej.
3. `ENTITY_WATER_ACTOR`, `ENTITY_NIGHT_PRESSURE` i `ENTITY_REVENANT_FAMILY` korzystają z istniejących research packages, ale finalne warianty regionalne wymagają cytowalnego ownera w karcie contentowej.
4. Brak źródła nigdy nie jest uzupełniany współczesną listą internetową jako author truth.

## QA rosteru

- Każdy wymagany slot ma co najmniej jedną ścieżkę inną niż bezwarunkowa walka.
- Journal nie ujawnia nazwy researchowej przed evidence gate.
- Save/load zachowuje typ rozwiązania i nie respawnuje permanentnie rozwiązanej instancji.
- Regionalny reuse nie zmienia automatycznie kulturowej interpretacji istoty.
- Encounter nie blokuje main questu śmiercią opcjonalnej istoty, jeśli quest card nie deklaruje tego jawnie.
- Placeholder `F` nie może zostać przypadkiem wyświetlony jako historyczna klasyfikacja.

## Otwarte decyzje

- finalny identity lock `forest-guardian`;
- research card południcy i decyzja, czy slot trafia do R4, R5 czy obu jako różne interpretacje;
- które kandydaty side-content po researchu awansują do scope 1.0;
- finalne weakness tables, HP/damage, loot yields i spawn density po playtestach;
- finalne modele, warianty animacji, audio i VFX po asset/performance locku.

Te punkty nie blokują implementacji wspólnego modelu danych, evidence, persistence ani bazowych encounter state machines.