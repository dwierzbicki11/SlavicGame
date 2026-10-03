# Implementation Readiness

Stan kolejki implementacyjnej po ukończeniu Aktu I. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — scalone.
- [x] MQ11 „Prawo łowcy” — scalone po zielonym CI.
- [x] MQ12 „Las, który myli drogę” — scalone po zielonym CI.
- [x] MQ13 „Dwie mapy” — map overlay/evidence synthesis, `NETWORK_HYPOTHESIS`, persistence, idempotencja i handoff do MQ20/Aktu II — scalone po zielonym CI.

## Aktywny element

### MQ20 „Sól i milczenie” — IMPLEMENTING

Karta `docs/story/MainQuestCardsAct2.md` ma status `implementation-ready v0.1`. Runtime implementuje wyłącznie kontrakt kampanii bez zgadywania finalnych NPC, frakcji, dialogów ani parametrów encounteru:

1. wejście po `MQ13_COMPLETE` i trwały `MQ20_ESTUARY_BLOCKED`;
2. cztery równoważne drogi dostępu do mapy: przysługa, negocjacja, reputacja lub dowód środowiskowy;
3. porównanie z trwałą hipotezą Sieci i zapis `MQ20_REMOTE_NODE_EVIDENCE`;
4. niezależny wynik lokalnego kryzysu: rozwiązanie albo obejście, aby zachować fail-forward;
5. `MQ20_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ21.

Finalne IDs NPC/frakcji, tekst dialogów, ekonomiczne progi negocjacji i szczegóły encounteru pozostają content/tuning lockiem.

## Kolejka po MQ20

Po pełnym zakończeniu MQ20 należy ponownie przeanalizować aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` oraz karty Aktu II. MQ21 ma kartę `implementation-ready v0.1`, ale nie wolno rozpoczynać go przed zielonym CI i merge MQ20.

## Otwarte decyzje implementacyjne

- MQ20: finalne NPC/frakcje i progi dostępu do mapy są content data; cztery ścieżki muszą pozostać funkcjonalnie równoważnymi fallbackami.
- MQ20: wynik ujścia jest niezależny od pozyskania krytycznego evidence; lokalny konflikt nie może blokować kampanii.
- MQ20: finalne asset IDs i tuning encounteru pozostają poza runtime contract.
