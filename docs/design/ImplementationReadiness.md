# Implementation Readiness

Stan kolejki implementacyjnej po ukończeniu MQ21. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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

## Aktywny element

### MQ22 „Kamień pod kamieniem” — IMPLEMENTING

Karta `docs/story/MainQuestCardsAct2.md` ma status `implementation-ready v0.1`. Runtime implementuje wyłącznie kontrakt kampanii bez zgadywania finalnych NPC, lokacji, materiałów ani parametrów encounteru:

1. wejście po `MQ21_COMPLETE`;
2. jawne naruszenie kopalni `MQ22_MINE_BREACH`;
3. dwa niezależne ślady konstrukcji, możliwe do zebrania w dowolnej kolejności;
4. materialny komponent kotwicy `MQ22_ANCHOR_MATERIAL` z fallbackiem analizy bez opcjonalnego NPC;
5. dokładnie jeden z trzech kontraktowych sposobów lokalnej stabilizacji: zabezpieczenie miejsca / obejście hazardu / ograniczenie wydobycia;
6. `MQ22_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ23.

Finalne IDs NPC/lokacji, nazwy materiałów, tekst dialogów, liczby, tuning hazardu i szczegóły encounteru pozostają content/tuning lockiem.

## Kolejka po MQ22

Po pełnym zakończeniu MQ22 należy ponownie przeanalizować aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` oraz karty Aktu II. MQ23 może zostać rozpoczęte wyłącznie, jeśli nadal spełnia próg implementation-ready po merge MQ22.

## Otwarte decyzje implementacyjne

- MQ22: finalne NPC/lokacje, nazwy materiałów i parametry hazardu są content data i nie mogą zmieniać kontraktu trzech stabilizacji.
- MQ22: opcjonalny NPC nie może być wymagany do analizy kotwicy; ścieżka krytyczna musi mieć fallback.
- MQ22: każdy kontraktowy sposób stabilizacji musi zachować dostępność MQ23 i dalszej krytycznej trasy kampanii.
