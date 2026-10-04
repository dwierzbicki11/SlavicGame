# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ31. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ31 „Archiwum popiołu” — trzy niezależne fakty Archiwum, recovery, persistence/idempotencja i handoff do MQ32 — scalone po zielonym CI.

## Aktywny element

### MQ32 „Noc Zamkniętego Progu” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct3.md`:

1. wejście po `MQ31_COMPLETE`;
2. trwała kolekcja kategorii evidence: dokumenty Archiwum, echo, świadek/przekaz pośredni, zeznanie Wszebora, zapis Straży oraz recovery przez alternatywny zapis terenowy;
3. rekonstrukcja rdzenia wymaga minimum trzech kategorii i co najmniej jednego źródła spoza zainteresowanej frakcji;
4. sprzeczności są trwałymi, niekierunkowymi relacjami między zebranymi kategoriami i nie nadpisują źródeł;
5. `CLOSED_THRESHOLD_CORE_RECONSTRUCTED`, `MQ32_COMPLETE`, persistence/idempotencja i handoff do MQ33.

Runtime nie koduje finalnej treści źródeł ani jednej prawdziwej interpretacji Nocy. Provenance afiliacji jest przekazywane jawnie przy zapisie evidence, a nie zgadywane przez runtime.

## Kolejka po MQ32

Po pełnym zakończeniu MQ32 należy ponownie przeanalizować aktualny `main`, trzy dokumenty kolejki/coverage/readiness oraz karty Aktu III. MQ33 może rozpocząć się wyłącznie po zielonym CI i merge MQ32 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ32: finalna treść dokumentów, echa, zeznań i presentation pozostają content/art lockiem.
- MQ32: alternatywny zapis terenowy jest recovery dla pominiętego echa zgodnie z kartą.
- MQ32: sprzeczności pozostają równoległymi faktami; ich rozstrzygnięcie nie należy do tego questa.
