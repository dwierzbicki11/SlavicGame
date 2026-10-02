# Implementation Readiness

Stan kolejki implementacyjnej po domknięciu MQ13. Ten dokument nie zastępuje specyfikacji systemów ani questów; wskazuje wyłącznie, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`. Odkryte zależności trafiają tutaj jako blocker lub pozycja kolejki, bez rozpoczynania równoległej implementacji.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — trwały state contract, evidence gating, persistence, idempotencja i odblokowanie MQ11 — scalone.
- [x] MQ11 „Prawo łowcy” — trzy warianty `MQ11-D01`, trwała konsekwencja reputacji, persistence, regresje i bezwarunkowy handoff do MQ12 — scalone po zielonym CI.
- [x] MQ12 „Las, który myli drogę” — tracking/navigation runtime, deterministyczna anomalia trasy, próg 2/3 punktów, fallback encounteru, persistence/recovery, `MQ12_NODE_CONFIRMED`, completion i handoff do MQ13 — scalone po zielonym CI.
- [x] MQ13 „Dwie mapy” — `MapOverlayState`, jawne dataset/evidence IDs, deterministyczna synteza, przewidywany punkt, persistence/recovery, `NETWORK_HYPOTHESIS`, `MQ13_COMPLETE` oraz trwały handoff do MQ20/Aktu II — implementacja i regresje ukończone; merge wymaga zielonego CI bieżącego PR.

## Aktywny element

### MQ13 „Dwie mapy” — FINAL VALIDATION / MERGE

Blocker map overlay/evidence synthesis został usunięty bez budowania finalnego UI mapy. Runtime przyjmuje jawne ID contentu, zapisuje datasets/evidence, deterministycznie syntetyzuje hipotezę i przewidywany punkt oraz odtwarza stan po save/load. Campaign contract waliduje trzy wymagane pakiety evidence i zapisuje `NETWORK_HYPOTHESIS` / `MQ13_COMPLETE`.

Ostatnim brakującym kryterium karty było jawne „odblokowanie tropów prowadzących do Przymorza i Aktu II”. `CompleteQuest()` oferuje teraz `MQ20`, a regresje sprawdzają handoff, idempotencję i save/load po ukończeniu.

Nie rozpoczynać MQ20 przed zielonym CI i scaleniem tego PR do `main`.

## Kolejka po MQ13

Po merge MQ13 należy ponownie przeczytać aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` i karty Aktu II. Pierwszym kandydatem jest MQ20 „Sól i milczenie”, ale może zostać wybrany dopiero po ponownej analizie jego zależności systemowych i contentowych.

## Otwarte decyzje implementacyjne

- MQ13: konkretne IDs warstw mapy, trzech wymaganych pakietów evidence, opcjonalnych evidence oraz przewidywanych punktów pozostają danymi contentowymi; runtime przyjmuje je jawnie.
- MQ13: wizualny sposób nałożenia map, animacje i finalny UX pozostają po stronie content/UI i nie blokują zamkniętego kontraktu state/synthesis/persistence.
- MQ13: rozszerzony evidence set może wzbogacać prezentację, ale nie zmienia minimalnej możliwości ukończenia questa.
- MQ20: przed implementacją wymagana jest osobna analiza DoR po merge MQ13; nie zakładać semantyki blokady ujścia, kupców ani starej mapy bez potwierdzenia istniejących runtime ownerów.
