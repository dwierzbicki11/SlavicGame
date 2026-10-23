# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ44. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ44 „Powrót z wiedzą” — powrót, eskalacja, final-needs synthesis i handoff do MQ50 — scalone po zielonym CI.

## Aktywny element

### MQ50 „Stare zobowiązania” — BLOCKED przed bezpieczną implementacją snapshotu

Kontrakt z `docs/quests/MainQuestCardsAct5.md` jest wystarczający dla gatingu, lifecycle i persistence, ale nie definiuje jeszcze deterministycznego sposobu wyliczenia czterech wymaganych tierów snapshotu z istniejącego stanu świata.

Twardy blocker do zamknięcia w ramach MQ50:

1. zdefiniować skończoną skalę/typ tieru dla `people`, `materials`, `knowledge`, `divine_terms`;
2. wskazać bazowe wkłady z main questu gwarantujące ścieżkę bez side questów;
3. wskazać istniejące trwałe flagi/stany, które są opcjonalnymi wzmocnieniami każdej kategorii;
4. zdefiniować deterministyczne mapowanie wkładów na tier bez finalnych wartości balansu wymagających playtestu;
5. określić zachowanie dla nieznanych/starych stanów save oraz potwierdzić, że snapshot po utworzeniu nie jest przeliczany.

Nie wolno implementować arbitralnego `tier = count(flags)`, domyślnych progów ani ręcznie wybranej listy flag bez owner spec. Po zamknięciu powyższego kontraktu wracamy do tego samego MQ50; MQ51 nie może zostać rozpoczęte.

## Kolejka po MQ50

Po pełnym zakończeniu MQ50 (implementacja, regresje, zielone CI i merge) należy ponownie przeanalizować aktualny `main`, WorkQueue, Coverage, ten dokument oraz kartę Aktu V. MQ51 może rozpocząć się dopiero wtedy.

## Otwarte decyzje implementacyjne

- MQ50: skala i deterministyczne mapowanie `MQ50_*_TIER` z trwałego world state — twardy blocker bieżącego zadania.
- MQ50: konkretne regionalne sceny powrotów, line writing i staging pozostają content/VO lockiem i nie blokują runtime po zamknięciu mapowania tierów.
- MQ50: finalne tuning values progów/korzyści pozostają playtest/measurement lockiem; kontrakt mapowania musi rozdzielać stabilną semantykę tierów od późniejszego tuningu.
