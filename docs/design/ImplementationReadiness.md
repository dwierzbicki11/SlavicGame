# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ11. Ten dokument nie zastępuje specyfikacji systemów ani questów; wskazuje wyłącznie, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`. Odkryte zależności trafiają tutaj jako blocker lub pozycja kolejki, bez rozpoczynania równoległej implementacji.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — trwały state contract, evidence gating, persistence, idempotencja i odblokowanie MQ11 — scalone.
- [x] MQ11 „Prawo łowcy” — trzy warianty `MQ11-D01`, trwała konsekwencja reputacji, persistence, regresje i bezwarunkowy handoff do MQ12 — scalone po zielonym CI.

## Aktywny element

### MQ12 „Las, który myli drogę” — BLOCKED (runtime dependencies)

Karta MQ12 jest narracyjnie `implementation-ready v0.1`, a wymagany timed-event runtime już istnieje jako `Engine/World/TimedEventSystem.cs`. Ponowna analiza aktualnego `main` nie wykazała jednak runtime ownera dla tracking ani kontraktu navigation/map potrzebnego do kontrolowanego zaburzenia trasy i trzech punktów odniesienia. Sama dokumentacja tracking istnieje (`design/TrackingSystem.md`), lecz nie ma odpowiadającej implementacji runtime; obecny map UI state nie jest kontraktem nawigacji świata.

Nie wolno implementować MQ12 przez same quest flagi, ponieważ zgadywałoby to semantykę `MQ12_ROUTE_ANOMALY`, punktów odniesienia, recovery po reloadzie i integracji z tracking/navigation.

### Minimalna praca konieczna do usunięcia blockera

W ramach tego samego aktywnego zadania MQ12 należy kolejno:

1. ustalić minimalny runtime contract tracking zgodny z `design/TrackingSystem.md` i persistence projektu;
2. ustalić minimalny navigation/route-anomaly contract: aktywacja/dezaktywacja kontrolowanego zaburzenia, trwałe odkrycie punktów odniesienia oraz bezpieczny restore/recovery po save/load;
3. zaimplementować te kontrakty z regresjami, bez budowania pełnego systemu mapy lub GPS;
4. dopiero po usunięciu blockera wrócić bezpośrednio do MQ12 i zaimplementować `MQ12_ROUTE_ANOMALY`, próg 2 z 3 punktów, fallback encounteru, `MQ12_NODE_CONFIRMED`, `MQ12_COMPLETE` i handoff do MQ13.

Timed events nie wymagają nowego systemu — należy użyć istniejącego `TimedEventSystem`.

## Kolejka po MQ12

Kolejność jest warunkowa i podlega ponownej analizie po każdym merge:

1. MQ13 „Dwie mapy” — wymaga MQ10–MQ12 oraz map overlay/evidence synthesis.
2. Następne elementy wyłącznie po ponownej analizie aktualnego `main`, DocumentationWorkQueue i DocumentationCoverage.

## Otwarte decyzje implementacyjne

- MQ12: konkretne content IDs trzech punktów odniesienia i opcjonalnego encounteru są danymi contentowymi; runtime ma przyjmować je jawnie zamiast kodować nazwy w silniku.
- MQ12: wizualna prezentacja zaburzenia trasy pozostaje content/UX; blocker dotyczy wyłącznie deterministycznego state/recovery contract.
