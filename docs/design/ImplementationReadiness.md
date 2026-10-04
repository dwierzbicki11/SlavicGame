# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ32. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ32 „Noc Zamkniętego Progu” — kategorie evidence, niezależne provenance, sprzeczności, recovery i handoff do MQ33 — scalone po zielonym CI.

## Aktywny element

### MQ33 „Rozwierający” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct3.md`:

1. wejście po `MQ32_COMPLETE`;
2. jawny zapis stanowiska/stance Wszebora bez traktowania testimony jako author truth;
3. trwały outcome konfrontacji dla wariantów rozmowy, zerwania, czasowego układu, walki lub ucieczki;
4. pakiet danych dla MQ34 jest dostępny niezależnie od relacji i wyniku; przy braku danych bezpośrednich używany jest zabezpieczony recovery record;
5. `WSZEBOR_POSITION_KNOWN`, `MQ33_COMPLETE`, persistence/idempotencja i handoff do MQ34;
6. outcome pozostaje trwały dla późniejszego MQ52.

Runtime nie rozstrzyga prawdziwości interpretacji Wszebora, jego przeżycia ani finalnej treści dialogów. Stance i outcome są zapisywane jawnie przez warstwę content/dialogue, a nie zgadywane z fabuły.

## Kolejka po MQ33

Po pełnym zakończeniu MQ33 należy ponownie przeanalizować aktualny `main`, trzy dokumenty kolejki/coverage/readiness oraz karty Aktu III. MQ34 może rozpocząć się wyłącznie po zielonym CI i merge MQ33 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ33: finalne line writing, presentation i dokładne argumenty Wszebora pozostają content/VO lockiem.
- MQ33: runtime przechowuje stance/outcome, ale nie interpretuje testimony jako author truth.
- MQ33: zabezpieczony zapis/pośrednik jest recovery dla danych MQ34; kampania nie wymaga przeżycia, przyjaźni ani współpracy Wszebora.
