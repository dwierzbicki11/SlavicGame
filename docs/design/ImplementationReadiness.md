# Implementation Readiness

Stan kolejki implementacyjnej po ukończeniu MQ20. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — scalone.
- [x] MQ11 „Prawo łowcy” — scalone po zielonym CI.
- [x] MQ12 „Las, który myli drogę” — scalone po zielonym CI.
- [x] MQ13 „Dwie mapy” — map overlay/evidence synthesis, `NETWORK_HYPOTHESIS`, persistence, idempotencja i handoff do MQ20/Aktu II — scalone po zielonym CI.
- [x] MQ20 „Sól i milczenie” — cztery równoważne ścieżki dostępu do mapy, `MQ20_REMOTE_NODE_EVIDENCE`, fail-forward ujścia, persistence/idempotencja i handoff do MQ21 — scalone po zielonym CI.

## Aktywny element

### MQ21 „Cena przejścia” — IMPLEMENTING

Karta `docs/story/MainQuestCardsAct2.md` ma status `implementation-ready v0.1`. Runtime implementuje wyłącznie kontrakt kampanii bez zgadywania finalnych NPC, frakcyjnych nazw, cen ani parametrów encounteru:

1. wejście po `MQ20_COMPLETE`;
2. niezależna weryfikacja argumentów obu stron;
3. trwała, pojedyncza decyzja `MQ21-D01`: preferencja Ligi / preferencja Nadborza / ograniczony dostęp dla obu stron;
4. każdy wynik zachowuje krytyczną trasę kampanii;
5. `MQ21_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ22.

Finalne IDs NPC/frakcji, tekst dialogów, ceny, reputacyjne progi i szczegóły późniejszej pomocy pozostają content/tuning lockiem.

## Kolejka po MQ21

Po pełnym zakończeniu MQ21 należy ponownie przeanalizować aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` oraz karty Aktu II. MQ22 ma kartę `implementation-ready v0.1`, ale nie wolno rozpoczynać go przed zielonym CI i merge MQ21.

## Otwarte decyzje implementacyjne

- MQ21: finalne NPC/frakcje, ceny i reputacyjne progi są content data; nie mogą zmieniać trzech kontraktowych wyników `MQ21-D01`.
- MQ21: każda decyzja musi zachować dostępność MQ22 i późniejszej krytycznej trasy kampanii.
- MQ21: finalne skutki handlu, eskorty, informacji i wsparcia w Akcie V pozostają content/tuning lockiem, dopóki ich konsumenci nie zostaną implementowani.
