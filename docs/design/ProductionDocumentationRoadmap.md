# Production Documentation Roadmap — v0.1

## Cel

Ten dokument zamienia zakończony first pass projektu w sekwencyjną kolejkę dokumentacji produkcyjnej. `DocumentationWorkQueue.md` pozostaje historią dojścia do pełnego obrazu gry; ta kolejka odpowiada na pytanie: **co musi zostać rozpisane następne, aby content i implementacja mogły rosnąć bez zgadywania?**

## Zasada pracy

1. Pracujemy od najniższego niezamkniętego pakietu.
2. Jeden pakiet powinien kończyć się konkretnymi dokumentami i kryteriami akceptacji.
3. Nie zamrażamy wartości balansowych bez danych z prototypu/playtestu.
4. Nie zamieniamy hipotez historycznych w fakty świata bez oznaczenia warstwy źródłowej lub fikcyjnej.
5. Nowy blocker odkryty podczas implementacji trafia do odpowiedniego pakietu, zamiast być rozwiązywany ad hoc w kodzie.

## P0 — synchronizacja istniejącego first pass

- [x] format eventów zależnych od czasu — `content/TimeEventFormat.md`;
- [x] błędne ogniki / światła bagienne — research card istnieje jako `research/bestiary/WillOWisps.md`; traktujemy je jako `phenomenon tag`, nie obowiązkowy gatunek potwora;
- [x] regional flow kampanii — `story/CampaignRegionalFlow.md`;
- [x] chronologia tajemnicy — `story/MysteryChronology.md`.

Kryterium: stare checkboxy nie mogą sugerować braku dokumentu, który już istnieje.

## P1 — main quest cards

Rozpisać MQ00–MQ56 z `story/MainQuestSkeleton.md` do kart produkcyjnych. Każda karta musi mieć co najmniej:

- trwałe ID i nazwę roboczą;
- region i wymagane lokacje;
- entry conditions i prerequisite IDs;
- cele/fazy oraz warunki przejść;
- NPC/encountery/interactable;
- evidence/reveal;
- decyzje i konsekwencje;
- state flags;
- fail/fallback/recovery;
- save/checkpoint boundaries;
- wymagane assety/audio/VFX;
- zależności od innych systemów;
- minimalne QA acceptance cases.

Najpierw przygotować wspólny `MainQuestCardTemplate.md`, następnie karty aktami 0→V. Nie wpisywać finalnych liczb balansu tam, gdzie wymagają playtestu.

Kryterium ukończenia: wszystkie MQ00–MQ56 mają kartę i żaden obowiązkowy przebieg kampanii nie zależy od nieopisanego przejścia.

## P2 — region bibles

Dla R0–R6 utworzyć produkcyjne bible obejmujące:

- granice i topologię;
- settlements/POI;
- biome i landmark language;
- kulturę, władzę, religię i gospodarkę;
- zagrożenia i encounter families;
- regionalny roster NPC;
- quest hooks oraz main-quest touchpoints;
- day/night/weather hooks;
- travel gates i streaming assumptions;
- asset families i audio identity;
- otwarte decyzje i research debt.

Kryterium: każdy region kampanii ma owner/spec wystarczający do world-buildingu bez dopowiadania podstawowych reguł.

## P3 — companion i NPC roster

Rozwinąć `story/RecurringCast.md` oraz NPC regionalnych do rosteru produkcyjnego:

- ID, rola, region/home base;
- funkcja gameplay/narracyjna;
- relacje i frakcje;
- availability state machine;
- quest involvement;
- combat/support role, jeśli dotyczy;
- dialogue package requirements;
- persistence/death/absence rules;
- wymagane assety.

Kryterium: każdy obowiązkowy NPC z main questów ma kartę, a recurring cast ma spójne reguły dostępności między regionami.

## P4 — final content catalogs

Zbudować katalogi produkcyjne dla:

- creatures/phenomena;
- weapons/armor/items;
- alchemy;
- spells/rituals;
- encounters;
- interactables;
- vendors/services;
- loot/rewards;
- journals/evidence/books;
- audio/VFX/animation families.

Każdy wpis używa trwałych ID i wskazuje owner/spec. Historycznie inspirowane stworzenia wymagają research card albo jawnego oznaczenia F.

Kryterium: implementacja i asset production mogą odwoływać się do jednej kanonicznej listy zamiast list lokalnych.

## P5 — production budgets

Po ustaleniu katalogów określić budżety:

- liczba unikalnych modeli/material families;
- animation sets;
- VFX/audio sets;
- dialogue/VO scope;
- encounter density;
- memory/streaming assumptions;
- save-size assumptions;
- targety CPU/GPU dopiero po pomiarach prototypu.

Kryterium: zakres 1.0 da się policzyć i porównać z realnym tempem produkcji. Wymagania sprzętowe pozostają `TBD measured`, dopóki benchmark ich nie potwierdzi.

## P6 — implementation/playtest closure

Dokumentować tylko na podstawie działającego builda:

- finalniejsze parametry balansu;
- telemetry/debug needs;
- performance budgets i measured targets;
- accessibility verification;
- save migration/versioning;
- release gates;
- regresje wynikające z playtestów.

Kryterium: `DocumentationCoverage.md` spełnia definicję pełnej dokumentacji projektu, a `ReleaseCriteria.md` nie zawiera krytycznych pól opartych wyłącznie na założeniach.

## Otwarte decyzje — rejestr nadrzędny

Do czasu zamknięcia odpowiednich pakietów jawnie pozostają otwarte:

- finalna nazwa regionalna `forest-guardian`;
- krytyczne wydania/źródła dla części panteonu;
- finalne parametry balansu walki, ekonomii i progresji;
- pełne karty questów poza vertical slice;
- finalne katalogi contentu i wynikające z nich budżety assetów;
- zmierzone targety performance i wymagania sprzętowe.

Otwarte decyzje nie mogą być cicho rozstrzygane przez implementację. Jeśli kod potrzebuje wartości tymczasowej, musi ona być oznaczona jako tunable/default, a nie jako finalny design lock.
