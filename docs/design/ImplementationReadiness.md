# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ13. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna
Implementujemy dokładnie jeden element naraz. Następny rozpoczynamy dopiero po testach, zielonym CI i merge bieżącego PR do `main`. Blockery usuwamy minimalnie w ramach tego samego aktywnego zadania.

## Ukończone
- [x] Act 0: MQ00 → MQ01 → MQ10 handoff.
- [x] MQ10 „Znak pod drogą”.
- [x] MQ11 „Prawo łowcy”.
- [x] MQ12 „Las, który myli drogę” wraz z tracking/navigation runtime.
- [x] MQ13 „Dwie mapy” — `MapOverlayState`, jawne dataset/evidence IDs, predicted point, deterministyczna synteza `NETWORK_HYPOTHESIS`, persistence/save-load, idempotencja i regresje; scalone po zielonym CI.

## Aktywny element
### Synchronizacja kolejki po MQ13 — w toku
MQ13 jest zakończone. Ponowna analiza aktualnego `main`, DocumentationWorkQueue, DocumentationCoverage i kart Aktu II wskazuje MQ20 „Sól i milczenie” jako pierwszy następny kandydat. Nie rozpoczynamy kodu MQ20 w tym samym kroku synchronizacji readiness; najpierw ta zmiana przechodzi branch/PR/CI/merge.

## Kolejka po MQ13
1. **MQ20 „Sól i milczenie”** — `implementation-ready v0.1`; wymaga `MQ13_COMPLETE`, travel/region state, dialogue, reputation, journal/evidence i persistence. Na początku implementacji trzeba potwierdzić minimalne runtime contracts travel/region state oraz wielodrogowego dostępu do mapy. Brak któregoś jest blockerem MQ20 i wolno wtedy wykonać tylko minimalną pracę konieczną do jego usunięcia.
2. **MQ21 „Cena przejścia”** — po MQ20; faction/reputation, dialogue, travel permissions, economy/world state, persistence.
3. **MQ22 „Kamień pod kamieniem”** — po MQ21; exploration/tracking, environmental interaction, hazard/encounter state, journal/evidence, persistence.
4. **MQ23 „Żelazna Brama”** — po MQ22; faction/reputation, resource/world state, dialogue, persistence.
5. **MQ24 „Droga bez granicy”** — po MQ23; travel/navigation, dialogue, tracking, map/journal, persistence.
6. **MQ25 „Archiwum bez jednego języka”** — po MQ20–MQ24; journal/evidence, map overlay, language/context tags, persistence.

## Otwarte decyzje implementacyjne
- MQ20: finalne NPC, dialogi, encounter parametry, asset IDs i tuning są contentem; runtime nie może ich zgadywać.
- MQ20: dostęp do fragmentu mapy musi mieć co najmniej jedną działającą drogę spośród przysługi, negocjacji, reputacji lub dowodu środowiskowego, bez pojedynczego krytycznego NPC.
- MQ20: wynik konfliktu ujścia jest niezależny od trwałego `MQ20_REMOTE_NODE_EVIDENCE`; wybór polityczny nie może usunąć krytycznego evidence.
- Akt II: decyzje MQ21/MQ23 mogą zmieniać wsparcie i ekonomię, ale nie mogą zamknąć krytycznej trasy kampanii.
- MQ25: opcjonalny evidence może wzbogacać syntezę, ale minimalny zestaw musi wystarczać do ukończenia aktu.
