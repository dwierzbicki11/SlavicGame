# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ31. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ22 „Kamień pod kamieniem” — dwa niezależne ślady konstrukcji, komponent kotwicy, recovery, persistence/idempotencja i handoff do MQ23 — scalone po zielonym CI.
- [x] MQ23 „Żelazna Brama” — trzy jawne skutki, `MQ23-D01`, persistence/idempotencja i handoff do MQ24 — scalone po zielonym CI.
- [x] MQ24 „Droga bez granicy” — dwa sposoby pamięci trasy, recoverable trial passage, `MQ24_ROUTE_RELATION` i handoff do MQ25 — scalone po zielonym CI.
- [x] MQ25 „Archiwum bez jednego języka” — cztery regionalne pakiety, dwie relacje między tradycjami, `NETWORK_MULTICULTURAL_ORIGIN`, persistence/idempotencja i handoff do MQ30 — scalone po zielonym CI.
- [x] MQ30 „Pustkowie Pierwszego Progu” — dwa niezależne ślady Archiwum, `ARCHIVE_PRESENCE_CONFIRMED`, bezpieczna oś dojścia, persistence/idempotencja i handoff do MQ31 — scalone po zielonym CI.
- [x] MQ31 „Archiwum popiołu” — trzy niezależne fakty Archiwum, primary/secondary recovery, `ARCHIVE_RECORDS_FOUND`, persistence/idempotencja i handoff do MQ32 — scalone po zielonym CI.

## Aktywny element

### MQ32 „Noc Zamkniętego Progu” — IMPLEMENTED, oczekuje na CI/merge

Karta `docs/quests/MainQuestCardsAct3.md` ma production documentation pass v0.1 i jednoznaczny kontrakt implementacyjny:

1. wejście po `MQ31_COMPLETE`;
2. niezależne kategorie evidence: dokumenty Archiwum, echo zdarzenia, świadek/przekaz pośredni, zeznanie Wszebora, zapis Straży Zamknięcia oraz recovery przez alternatywny zapis terenowy;
3. rekonstrukcja krytycznego rdzenia wymaga co najmniej trzech kategorii i co najmniej jednego źródła spoza zainteresowanej frakcji;
4. sprzeczności są zapisywane jako trwałe, niekierunkowe relacje między zebranymi kategoriami i nie nadpisują źródeł;
5. `CLOSED_THRESHOLD_CORE_RECONSTRUCTED`, `MQ32_COMPLETE`, persistence/idempotencja i handoff do MQ33 dopiero po spełnieniu minimalnego kontraktu evidence.

Runtime nie koduje finalnej treści zeznań ani jednej „prawdziwej” interpretacji Nocy. Potwierdza tylko minimalny zgodny rdzeń wymagany przez kampanię i zachowuje sprzeczności do późniejszych dialogów.

## Kolejka po MQ32

Po pełnym zakończeniu MQ32 należy ponownie przeanalizować aktualny `main`, trzy dokumenty kolejki/coverage/readiness oraz karty Aktu III. MQ33 może rozpocząć się wyłącznie po zielonym CI i merge MQ32 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ32: finalna treść dokumentów, echa, zeznań i prezentacja rekonstrukcji pozostają content/art lockiem.
- MQ32: provenance „spoza zainteresowanej frakcji” jest jawnie zapisywane przy kategorii evidence; runtime nie zgaduje afiliacji źródła.
- MQ32: alternatywny zapis terenowy jest kategorią recovery dla pominiętego echa, zgodnie z kartą questa.
- MQ32: sprzeczności pozostają równoległymi faktami; ich rozstrzygnięcie nie należy do tego questa.
