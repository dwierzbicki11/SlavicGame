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
- [x] MQ21 „Cena przejścia” — weryfikacja obu stron, trwała decyzja `MQ21-D01`, persistence/idempotencja i gwarantowany handoff do MQ22 — scalone po zielonym CI.

## Aktywny element

### MQ22 „Kamień pod kamieniem” — IMPLEMENTING

Karta `docs/story/MainQuestCardsAct2.md` ma status `implementation-ready v0.1`. Runtime implementuje wyłącznie kontrakt kampanii bez zgadywania finalnego encounteru, parametrów hazardu, NPC ani assetów:

1. wejście po `MQ21_COMPLETE`;
2. zapis `MQ22_MINE_BREACH` i aktywna faza śledztwa;
3. dwa niezależne ślady konstrukcji, możliwe do zebrania w dowolnej kolejności;
4. `MQ22_ANCHOR_MATERIAL` po analizie, z fallbackiem bez opcjonalnego NPC;
5. jeden z kontraktowych wariantów lokalnej stabilizacji: zabezpieczenie miejsca / obejście hazardu / ograniczenie wydobycia;
6. `MQ22_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ23.

Finalny encounter, liczby hazardu, NPC, asset IDs oraz tuning pozostają content/tuning lockiem. Krytyczny materiał nie może zostać zniszczony przez encounter.

## Kolejka po MQ22

Po pełnym zakończeniu MQ22 należy ponownie przeanalizować aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` oraz karty Aktu II. MQ23 ma kartę `implementation-ready v0.1`, ale nie wolno rozpoczynać go przed zielonym CI i merge MQ22.

## Otwarte decyzje implementacyjne

- MQ22: finalny encounter i parametry hazardu są content/tuning data i nie mogą blokować krytycznego materiału.
- MQ22: opcjonalny analityk/NPC nie może być single point of failure; wymagany jest fallback gracza/środowiska.
- MQ22: finalne asset IDs, dokładny wygląd konstrukcji i szczegóły wydobycia pozostają art/content/research lockiem.
