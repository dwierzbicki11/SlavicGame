# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ33. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ33 „Rozwierający” — stance/outcome Wszebora, fail-forward data package i handoff do MQ34 — scalone po zielonym CI.

## Aktywny element

### MQ34 „Trzy projekty” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct3.md`:

1. wejście po `MQ33_COMPLETE`;
2. trzy niezależne trwałe modele: twarde Zamknięcie, rozproszona przebudowa/stabilizacja i Rozwarcie;
3. krytyczne streszczenie każdego modelu jest dostępne z trwałego stanu kampanii niezależnie od opcjonalnych notatek;
4. runtime nie oznacza żadnego modelu jako prawdziwego i nie wykonuje finałowego wyboru;
5. ukończenie wymaga wszystkich trzech modeli, zapisuje `MQ34_COMPLETE` i `ACT4_NAVIA_LEAD_AVAILABLE`;
6. handoff do MQ40 następuje dokładnie raz, z persistence/save-load i idempotencją.

## Kolejka po MQ34

Po pełnym zakończeniu MQ34 należy ponownie przeanalizować aktualny `main`, trzy dokumenty kolejki/coverage/readiness oraz kartę Aktu IV. MQ40 może rozpocząć się wyłącznie po zielonym CI i merge MQ34 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ34: finalne line writing, presentation, autorzy/zwolennicy i szczegółowe koszty pozostają content/VO lockiem tam, gdzie dokumentacja nie ustala ich jednoznacznie.
- MQ34: optional notes zwiększają szczegółowość, ale nie mogą usuwać żadnego modelu z krytycznego porównania.
- MQ34: prawdziwość modeli i metafizyczna odpowiedź pozostają zablokowane do Aktu IV; runtime zapisuje wyłącznie wiedzę gracza o trzech projektach i lead do Nawii.
