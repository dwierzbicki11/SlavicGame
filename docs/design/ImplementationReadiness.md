# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ42. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ40 „Droga umarłych” — scalone po zielonym CI.
- [x] MQ41 „Dwa echa” — scalone po zielonym CI.
- [x] MQ42 „Ten, który pozostał” — Parent B/skip, persistence i handoff do MQ43 — scalone po zielonym CI.

## Aktywny element

### MQ43 „Czwarta nie jest miejscem” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct4.md`:

1. wejście wyłącznie po ukończeniu MQ42;
2. wejście w niestabilny wzorzec zapisuje trwały stan;
3. obserwacja złamania zwykłych reguł przestrzeni wymaga wejścia we wzorzec;
4. pełny reveal Splotu wymaga także `MQ41_NAVIA_SPLOT_DISTINCTION`;
5. przerwana prezentacja może zostać wznowiona z trwałych obserwacji bez ponownego naliczania konsekwencji;
6. `MQ43_SPLOT_TRUTH_KNOWN` jest trwałe przez save/load i nie może powstać przed spełnieniem gatingu;
7. completion zapisuje `MQ43_COMPLETE` i oferuje MQ44 dokładnie raz, bez resetowania późniejszego postępu.

## Kolejka po MQ43

Po pełnym zakończeniu MQ43 należy ponownie przeanalizować aktualny `main`, WorkQueue, Coverage, ten dokument oraz kartę Aktu IV. MQ44 może rozpocząć się wyłącznie po zielonym CI i merge MQ43 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ43: staging niestabilnego wzorca, finalne line writing i AVFX pozostają content/art/VO lockiem.
- MQ43: runtime zapisuje author-truth reveal jako trwały fakt systemowy; nie wybiera za gracza architektury finału E1–E5.
- MQ43: checkpoint/recovery dotyczy prezentacji revealu i nie może duplikować trwałych konsekwencji.
