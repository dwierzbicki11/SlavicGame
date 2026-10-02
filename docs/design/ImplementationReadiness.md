# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ10. Ten dokument nie zastępuje specyfikacji systemów ani questów; wskazuje wyłącznie, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`. Odkryte zależności trafiają tutaj jako blocker lub pozycja kolejki, bez rozpoczynania równoległej implementacji.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — trwały state contract, evidence gating, persistence, idempotencja i odblokowanie MQ11 — scalone.

## Aktywny element

### MQ11 „Prawo łowcy” — IMPLEMENTING

Ponowna analiza aktualnego `main` wykazała, że wcześniejszy blocker był błędny: runtime reputacji już istnieje jako `Engine/Reputation/ReputationSystem.cs`, jest właścicielem scope `Faction`, ma zakres -100..100 oraz `Capture`/`Restore`, a `GameProgress` posiada instancję `ReputationSystem`. Nie tworzymy więc drugiego systemu reputacji.

Bieżący PR implementuje wyłącznie MQ11: `MQ11_POLICY_SEEN`, dwie wymagane perspektywy, przypadek testowy kosztu procedury, trwałe trzy warianty `MQ11-D01`, jednorazową konsekwencję faction reputation, `MQ11_COMPLETE` oraz bezwarunkowy handoff do MQ12. Numericzne wartości reputacji i finalne faction IDs pozostają danymi content/tuningu przekazywanymi do kontraktu kampanii; kod nie zgaduje ich wartości.

MQ11 zostaje ukończone dopiero po regresjach, zielonym CI i merge tego PR do `main`.

## Kolejka po MQ11

Kolejność jest warunkowa i podlega ponownej analizie po każdym merge:

1. MQ12 „Las, który myli drogę” — spec istnieje, ale wymaga MQ11 oraz navigation/map + timed events.
2. MQ13 „Dwie mapy” — wymaga MQ10–MQ12 oraz map overlay/evidence synthesis.
3. Następne elementy wyłącznie po ponownej analizie aktualnego `main`, DocumentationWorkQueue i DocumentationCoverage.

## Otwarte decyzje implementacyjne

- MQ11: finalne faction IDs i wartości delta reputacji są content/tuning data; nie blokują kontraktu kampanii, ponieważ runtime przyjmuje je jawnie i utrwala wynik.
