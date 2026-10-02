# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ10. Ten dokument nie zastępuje specyfikacji systemów ani questów; wskazuje wyłącznie, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`. Odkryte zależności trafiają tutaj jako blocker lub pozycja kolejki, bez rozpoczynania równoległej implementacji.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — trwały state contract, evidence gating, persistence, idempotencja i odblokowanie MQ11 — scalone.

## Aktywny element

### MQ11 „Prawo łowcy” — BLOCKED

Karta `story/MainQuestCardsAct1.md` ma status `implementation-ready v0.1`, ale jej kontrakt wymaga zapisu **reputation/faction world state** jako konsekwencji `MQ11-D01`. W aktualnym kodzie nie istnieje runtime owner dla reputacji. Implementowanie MQ11 wyłącznie flagami questa oznaczałoby pominięcie jawnej zależności specyfikacji i stworzenie późniejszego długu/migracji save.

**Blocker:** brak minimalnego, trwałego kontraktu reputacji/faction standing w runtime oraz jego save/load API.

**Praca konieczna do odblokowania MQ11:**
1. potwierdzić owner/spec semantyki reputacji (zakres, identyfikatory frakcji, operacje i persistence);
2. zaimplementować minimalny runtime zgodny z tym kontraktem wraz z regresjami save/load;
3. dopiero po scaleniu tego blockera wrócić do MQ11 i zaimplementować `MQ11_POLICY_SEEN`, wymagane perspektywy/test case, trwałe `MQ11-D01`, konsekwencję reputacyjną, `MQ11_COMPLETE` i bezwarunkowy handoff do MQ12.

Nie należy wybierać MQ12 ani innego elementu podczas tego blockera.

## Kolejka po MQ11

Kolejność jest warunkowa i podlega ponownej analizie po każdym merge:

1. MQ12 „Las, który myli drogę” — spec istnieje, ale wymaga MQ11 oraz navigation/map + timed events.
2. MQ13 „Dwie mapy” — wymaga MQ10–MQ12 oraz map overlay/evidence synthesis.
3. Następne elementy wyłącznie po ponownej analizie aktualnego `main`, DocumentationWorkQueue i DocumentationCoverage.

## Otwarte decyzje implementacyjne

- Reputation/faction runtime: brak owner/spec wystarczającego do bezpiecznego kodowania konsekwencji MQ11. To jest twardy blocker bieżącego zadania, nie zgoda na ad-hoc wartości reputacji.
