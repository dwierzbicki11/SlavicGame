# Implementation Readiness

Stan kolejki implementacyjnej po ukończeniu MQ25. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — scalone.
- [x] MQ11 „Prawo łowcy” — scalone po zielonym CI.
- [x] MQ12 „Las, który myli drogę” — scalone po zielonym CI.
- [x] MQ13 „Dwie mapy” — map overlay/evidence synthesis, `NETWORK_HYPOTHESIS`, persistence, idempotencja i handoff do MQ20/Aktu II — scalone po zielonym CI.
- [x] MQ20 „Sól i milczenie” — cztery równoważne ścieżki dostępu do mapy, `MQ20_REMOTE_NODE_EVIDENCE`, fail-forward ujścia, persistence/idempotencja i handoff do MQ21 — scalone po zielonym CI.
- [x] MQ21 „Cena przejścia” — niezależna weryfikacja argumentów obu stron, trwała decyzja `MQ21-D01`, persistence/idempotencja i gwarantowany handoff do MQ22 — scalone po zielonym CI.
- [x] MQ22 „Kamień pod kamieniem” — dwa niezależne ślady konstrukcji, komponent kotwicy, recovery, persistence/idempotencja i handoff do MQ23 — scalone po zielonym CI.
- [x] MQ23 „Żelazna Brama” — trzy jawne skutki, `MQ23-D01`, persistence/idempotencja i handoff do MQ24 — scalone po zielonym CI.
- [x] MQ24 „Droga bez granicy” — dwa sposoby pamięci trasy, recoverable trial passage, `MQ24_ROUTE_RELATION` i handoff do MQ25 — scalone po zielonym CI.
- [x] MQ25 „Archiwum bez jednego języka” — cztery regionalne pakiety, dwie relacje między tradycjami, `NETWORK_MULTICULTURAL_ORIGIN`, persistence/idempotencja i handoff do MQ30 — scalone po zielonym CI.

## Aktywny element

### MQ30 „Pustkowie Pierwszego Progu” — IMPLEMENTING

Karta `docs/quests/MainQuestCardsAct3.md` ma production documentation pass v0.1 i jednoznaczny kontrakt implementacyjny:

1. wejście po `MQ25_COMPLETE` i skonsolidowanej hipotezie Sieci;
2. zapis `R6_ENTERED` przy wejściu do regionu;
3. co najmniej dwa niezależne ślady działalności Archiwum Progów;
4. po dwóch śladach zapis `ARCHIVE_PRESENCE_CONFIRMED` i możliwość ustalenia bezpiecznej osi dojścia `FIRST_THRESHOLD_ROUTE_KNOWN`;
5. `MQ30_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ31.

Runtime nie koduje finalnej geometrii trasy, liczby encounterów, nazw scen, assetów ani presentation. Alternatywny ślad środowiskowy pozostaje równorzędnym kanałem recovery; krytyczne evidence jest trwałym stanem kampanii.

## Kolejka po MQ30

Po pełnym zakończeniu MQ30 należy ponownie przeanalizować aktualny `main`, trzy dokumenty kolejki/coverage/readiness oraz karty Aktu III. MQ31 może rozpocząć się wyłącznie po zielonym CI i merge MQ30 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ30: finalna geometria i presentation Pustkowia pozostają content/art lockiem.
- MQ30: krytyczne potwierdzenie Archiwum wymaga dwóch niezależnych kanałów; żaden pojedynczy NPC, encounter ani zniszczalny obiekt nie może być blockerem.
- MQ30: reveal kończy się na potwierdzeniu ludzkiej działalności przy Sieci; role rodziców, Wszebora i prawda Splotu pozostają zablokowane dla późniejszych questów.
