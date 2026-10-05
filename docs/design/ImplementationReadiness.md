# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ43. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ43 „Czwarta nie jest miejscem” — reveal Splotu, recovery i handoff do MQ44 — scalone po zielonym CI.

## Aktywny element

### MQ44 „Powrót z wiedzą” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct4.md`:

1. wejście wymaga `MQ43_COMPLETE`, `MQ43_SPLOT_TRUTH_KNOWN` i trwałej kotwicy powrotnej MQ40;
2. powrót do Jawii zapisuje `MQ44_RETURNED_TO_JAWIA`;
3. eskalacja kryzysu następuje dopiero po powrocie i zapisuje `MQ44_CRISIS_ESCALATED`;
4. regionalne wiadomości mogą różnić treść/zasoby, ale krytyczna synteza potrzeb finału ma fallback niezależny od konkretnego posłańca;
5. `MQ44_FINAL_NEEDS_KNOWN` jest trwałe przez save/load;
6. completion zapisuje `MQ44_COMPLETE` i oferuje MQ50 dokładnie raz, bez resetowania późniejszego postępu;
7. ścieżki MQ42 Parent B i skip muszą obie prowadzić przez MQ44 bez softlocka.

## Kolejka po MQ44

Po pełnym zakończeniu MQ44 należy ponownie przeanalizować aktualny `main`, WorkQueue, Coverage, ten dokument oraz kartę Aktu V. MQ50 może rozpocząć się wyłącznie po zielonym CI i merge MQ44 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ44: konkretne regionalne wiadomości, posłańcy, line writing i staging pozostają content/VO lockiem; runtime zachowuje krytyczny fallback.
- MQ44: warianty regionalne zmieniają treść i zasoby, nie fakt odblokowania Aktu V.
- MQ44: finalne wartości zasobów i balansu pozostają playtest/measurement lockiem.
