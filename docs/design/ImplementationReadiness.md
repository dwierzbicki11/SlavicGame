# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ12. Ten dokument nie zastępuje specyfikacji systemów ani questów; wskazuje wyłącznie, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`. Odkryte zależności trafiają tutaj jako blocker lub pozycja kolejki, bez rozpoczynania równoległej implementacji.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — trwały state contract, evidence gating, persistence, idempotencja i odblokowanie MQ11 — scalone.
- [x] MQ11 „Prawo łowcy” — trzy warianty `MQ11-D01`, trwała konsekwencja reputacji, persistence, regresje i bezwarunkowy handoff do MQ12 — scalone po zielonym CI.
- [x] MQ12 „Las, który myli drogę” — tracking/navigation runtime, deterministyczna anomalia trasy, próg 2/3 punktów, fallback encounteru, persistence/recovery, `MQ12_NODE_CONFIRMED`, completion i handoff do MQ13 — scalone po zielonym CI.

## Aktywny element

### MQ13 „Dwie mapy” — BLOCKED (map overlay / evidence synthesis runtime)

Karta MQ13 jest `implementation-ready v0.1` i wymaga `MQ10_COMPLETE`, `MQ11_COMPLETE`, `MQ12_COMPLETE`, journal/evidence, map overlay oraz persistence. Pierwsze trzy zależności są już w `main`, podobnie jak trwały quest/journal progress i persistence. Ponowna analiza aktualnego `main` nie wykazała jednak runtime ownera/kontraktu dla map overlay ani deterministycznej syntezy wymaganych pakietów evidence.

Nie wolno implementować MQ13 wyłącznie przez quest flagi, ponieważ zgadywałoby to semantykę nałożenia map, wyboru przewidywanego punktu, minimalnego kontra rozszerzonego evidence set oraz zachowania overlay po ponownym otwarciu i save/load.

### Minimalna praca konieczna do usunięcia blockera

W ramach tego samego aktywnego zadania MQ13 należy kolejno:

1. ustalić minimalny runtime contract `MapOverlayState`/odpowiednika: jawne ID warstw/datasetów, aktywacja overlay, trwały wynik syntezy i bezpieczne ponowne otwarcie po save/load;
2. ustalić minimalny evidence-synthesis contract przyjmujący jawne wymagane evidence IDs i opcjonalne evidence, bez kodowania nazw contentu w silniku;
3. umożliwić zapis co najmniej jednego jawnie wybranego/przewidywanego punktu i deterministyczne potwierdzenie zależności;
4. zaimplementować kontrakty z persistence i regresjami; nie budować przy tym pełnego renderera mapy ani finalnego UI;
5. po usunięciu blockera wrócić bezpośrednio do MQ13 i zaimplementować walidację trzech pakietów evidence, overlay, test hipotezy, `NETWORK_HYPOTHESIS`, `MQ13_COMPLETE` oraz handoff do Aktu II.

## Kolejka po MQ13

Kolejność jest warunkowa i podlega ponownej analizie po każdym merge. Następny element wolno wybrać dopiero po pełnym zakończeniu MQ13 i ponownej analizie aktualnego `main`, `DocumentationWorkQueue.md` oraz `DocumentationCoverage.md`.

## Otwarte decyzje implementacyjne

- MQ13: konkretne IDs warstw mapy, trzech wymaganych pakietów evidence, opcjonalnych evidence oraz przewidywanych punktów są danymi contentowymi; runtime ma je przyjmować jawnie.
- MQ13: wizualny sposób nałożenia map, animacje i finalny UX pozostają po stronie content/UI; blocker dotyczy deterministycznego state/synthesis/persistence contract.
- MQ13: rozszerzony evidence set może wzbogacać prezentację, ale nie może zmieniać minimalnej możliwości ukończenia questa.
