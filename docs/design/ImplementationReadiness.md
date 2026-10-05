# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ40. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — scalone.
- [x] MQ11 „Prawo łowcy” — scalone po zielonym CI.
- [x] MQ12 „Las, który myli drogę” — scalone po zielonym CI.
- [x] MQ13 „Dwie mapy” — scalone po zielonym CI.
- [x] MQ20 „Sól i milczenie” — scalone po zielonym CI.
- [x] MQ21 „Cena przejścia” — scalone po zielonym CI.
- [x] MQ22 „Kamień pod kamieniem” — scalone po zielonym CI.
- [x] MQ23 „Żelazna Brama” — scalone po zielonym CI.
- [x] MQ24 „Droga bez granicy” — scalone po zielonym CI.
- [x] MQ25 „Archiwum bez jednego języka” — scalone po zielonym CI.
- [x] MQ30 „Pustkowie Pierwszego Progu” — scalone po zielonym CI.
- [x] MQ31 „Archiwum popiołu” — scalone po zielonym CI.
- [x] MQ32 „Noc Zamkniętego Progu” — scalone po zielonym CI.
- [x] MQ33 „Rozwierający” — scalone po zielonym CI.
- [x] MQ34 „Trzy projekty” — scalone po zielonym CI.
- [x] MQ40 „Droga umarłych” — wejście do Nawii, kotwica powrotna, recovery i handoff do MQ41 — scalone po zielonym CI.

## Aktywny element

### MQ41 „Dwa echa” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct4.md`:

1. wejście wyłącznie po `MQ40_COMPLETE`;
2. oba echa mogą zostać znalezione w dowolnej kolejności i są zapisywane trwale;
3. operacyjna różnica Nawia/Splot wymaga obu ech oraz minimum dwóch niezależnych różnic;
4. porównanie w Journal zapisuje `MQ41_NAVIA_SPLOT_DISTINCTION`, bez ujawniania pełnej definicji Czwartej Sfery;
5. pominięte obserwacje mają recovery zapisujące te same trwałe evidence flags;
6. completion wymaga porównania i rozróżnienia, po czym zapisuje `MQ41_COMPLETE`;
7. MQ42 jest oferowane dokładnie raz, z persistence/save-load i idempotencją.

## Kolejka po MQ41

Po pełnym zakończeniu MQ41 należy ponownie przeanalizować aktualny `main`, WorkQueue, Coverage, ten dokument oraz kartę Aktu IV. MQ42 może rozpocząć się wyłącznie po zielonym CI i merge MQ41 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ41: wygląd obu ech, staging obserwacji i finalne line writing pozostają content/art/VO lockiem.
- MQ41: recovery może być środowiskowym śladem lub zapisem w pobliżu; runtime zapisuje ten sam trwały fakt evidence.
- MQ41: rozróżnienie jest wyłącznie operacyjne; pełna natura Splotu pozostaje zablokowana do MQ43.
