# Kolejność dalszej dokumentacji — od najprostszej do najtrudniejszej

Ten dokument ustala praktyczną kolejność pogłębiania dokumentacji po zbudowaniu pełnego szkieletu v0.1.

## Poziomy 1–9 — first pass

Poziomy 1–9 mają kompletny first pass. Historyczne szczegóły pozostają w Git; aktywna kolejka poniżej jest źródłem prawdy dla dalszej pracy produkcyjnej.

Nadal jawnie otwarte z wcześniejszych poziomów:
- [ ] ogniki/błędne światła jako zjawisko — finalna karta researchowa;
- [ ] finalna nazwa/tożsamość `forest-guardian` — regionalny research lock;
- [x] regional flow głównej historii — `story/CampaignRegionalFlow.md`;
- [x] ostateczna chronologia tajemnicy — `story/MysteryChronology.md`.

## Faza 10 — production documentation

Cel krótkoterminowy: najpóźniej 2026-10-08 21:16 Europe/Warsaw dokumentacja ma być wstępnie kompletna do swobodnej implementacji systemów i contentu. Nie oznacza to finalnego balansu ani measured performance.

### P10-A — main quest cards
- [x] Akt 0–I — `quests/MainQuestCardsAct0I.md`;
- [ ] Akt II;
- [ ] Akt III;
- [ ] Akt IV;
- [ ] Akt V;
- [ ] globalna macierz zależności MQ, fail-safe i checkpointów.

### P10-B — region bibles
- [ ] R0 Pogranicze Żarnowca — rozszerzyć vertical-slice cards do bible regionu;
- [ ] Nadborze;
- [ ] Dębrzyn;
- [ ] Wielki Bór;
- [ ] Przymorze;
- [ ] Kamienne Wyżyny;
- [ ] Arel;
- [ ] R6 / Pustkowie Pierwszego Progu;
- [ ] macierz travel gates, services, factions, encounters, resources i quest hooks.

### P10-C — cast/content lock first pass
- [ ] recurring NPC roster per region;
- [ ] final creature roster per region z oznaczeniem F/H/S i research gaps;
- [ ] broń startowa i equipment baseline;
- [ ] finalniejsze listy itemów/receptur/usług;
- [ ] asset lists per region.

### P10-D — implementacyjne kontrakty systemów
- [ ] quest runtime state machine + condition/effect vocabulary;
- [ ] dialogue runtime data contract;
- [ ] reputation/faction state contract;
- [ ] world-state/event persistence contract;
- [ ] encounter spawning/despawning contract;
- [ ] AI archetype/behavior contract;
- [ ] combat tuning schema;
- [ ] animation state/event contract;
- [ ] audio event contract;
- [ ] localization/text-key contract;
- [ ] content validation/build pipeline.

### P10-E — research locks
- [ ] ogniki/błędne światła;
- [ ] forest-guardian final identity;
- [ ] krytyczne źródła panteonu wymagane do content lock;
- [ ] material-culture gaps używane przez region bibles.

### P10-F — production readiness (po implementacji/profilowaniu)
- [ ] production budgets;
- [ ] zmierzone targety performance;
- [ ] final balance pass;
- [ ] wymagania sprzętowe z pomiarów;
- [ ] release-criteria evidence.

## Reguła wykonywania

1. Zawsze zaczynaj od aktualnego `main`, tej kolejki i `DocumentationCoverage.md`.
2. Preferuj najniższy niezamknięty element, chyba że jest blokowany przez research/decyzję wyższego poziomu.
3. Nie duplikuj istniejących specyfikacji: karta produkcyjna linkuje do źródła prawdy i dodaje brakujący kontrakt implementacyjny.
4. Każdy nowy dokument ma jawne: scope, zależności, trwały stan, fail-safe/edge cases, minimalne QA oraz otwarte decyzje, jeśli dotyczą implementacji.
5. Research oddziela historyczne H/S od fikcyjnego F; brak źródła nie może być maskowany pewnym twierdzeniem.
6. Finalne liczby balansu i performance pozostają otwarte do czasu pomiarów/playtestów.
7. Merge do `main` dopiero po zielonym CI.

## Definicja progu „wstępnie kompletne do programowania”

Próg jest osiągnięty, gdy:
- wszystkie główne systemy mają implementacyjny kontrakt danych/stanu;
- MQ00–MQ56 mają produkcyjne karty lub jawnie współdzielony kontrakt bez luk krytycznych;
- każdy region kampanii ma bible z travel/content/system dependencies;
- główne lore potrzebne runtime jest zamknięte lub jawnie oznaczone jako decyzja otwarta bez blokowania kodu;
- formaty contentu pozwalają walidować dane bez zgadywania przez programistę;
- `DocumentationCoverage.md` nie ma nieoznaczonych luk blokujących implementację.

## Definicja pełnej dokumentacji produkcyjnej

Po progu implementacyjnym praca trwa dalej, aż kryteria z `DocumentationCoverage.md` są faktycznie spełnione: komplet questów, regionów, finalnych istot/kultur, asset list, zmierzone performance, balance i release readiness.
