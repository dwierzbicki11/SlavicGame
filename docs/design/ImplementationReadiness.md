# Implementation Readiness

Stan kolejki implementacyjnej po ukończeniu MQ23. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ22 „Kamień pod kamieniem” — naruszenie kopalni, dwa niezależne ślady konstrukcji, materialny komponent kotwicy, trzy stabilizacje, fallback bez opcjonalnego NPC, persistence/idempotencja i handoff do MQ23 — scalone po zielonym CI.
- [x] MQ23 „Żelazna Brama” — trzy jawne skutki, trwała decyzja `MQ23-D01`, neutralne sloty stron, persistence/idempotencja i gwarantowany handoff do MQ24 — scalone po zielonym CI.

## Aktywny element

### MQ24 „Droga bez granicy” — IMPLEMENTING

Karta `docs/story/MainQuestCardsAct2.md` ma status `implementation-ready v0.1`. Runtime implementuje kontrakt kampanii bez egzotyzowania Arelów i bez zgadywania finalnych NPC, tekstów, znaków, nazw terenowych ani parametrów nawigacji:

1. wejście po `MQ23_COMPLETE` i zapis `MQ24_MAP_MISMATCH`;
2. dwa niezależne sposoby pamięci trasy: opowieść/znak oraz obserwacja terenowa, zbierane w dowolnej kolejności;
3. przejście próbne ma recovery po błędnej próbie i nie może wymagać jednego konkretnego przewodnika NPC;
4. poprawne zastosowanie wiedzy zapisuje `MQ24_ROUTE_RELATION`;
5. `MQ24_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ25.

Finalne NPC, treść opowieści/znaków, nazwy punktów, geometria trasy, tuning nawigacji i presentation pozostają content/art/tuning lockiem.

## Kolejka po MQ24

Po pełnym zakończeniu MQ24 należy ponownie przeanalizować aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` oraz karty Aktu II. MQ25 może zostać rozpoczęte wyłącznie, jeśli nadal spełnia próg implementation-ready po merge MQ24.

## Otwarte decyzje implementacyjne

- MQ24: system pamięci trasy jest praktyczną wiedzą kulturową; runtime nie może kodować Arelów jako „mistycznego klucza”.
- MQ24: krytyczna ścieżka nie może zależeć od jednego przewodnika NPC; poprawna relacja punktów musi mieć fallback przez wskazówki i obserwację terenową.
- MQ24: błędna próba trasy jest recoverable i nie może niszczyć krytycznych evidence ani blokować MQ25.
