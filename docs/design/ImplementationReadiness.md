# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ30. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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

## Aktywny element

### MQ31 „Archiwum popiołu” — IMPLEMENTED, oczekuje na CI/merge

Karta `docs/quests/MainQuestCardsAct3.md` ma production documentation pass v0.1 i jednoznaczny kontrakt implementacyjny:

1. wejście po `MQ30_COMPLETE`;
2. trzy niezależnie zapisywane fakty: rola rodziców, związek Wszebora z Archiwum i odniesienie do Nocy Zamkniętego Progu;
3. każdy krytyczny fakt może pochodzić z dokumentu podstawowego albo wtórnego rejestru, więc utrata opcjonalnego dokumentu nie blokuje kampanii;
4. zapis `ARCHIVE_RECORDS_FOUND` po pozyskaniu materiału Archiwum;
5. `MQ31_COMPLETE`, persistence/idempotencja i gwarantowany handoff do MQ32 dopiero po poznaniu wszystkich trzech faktów.

Runtime przechowuje fakty i provenance kanału, ale nie koduje finalnych tekstów dokumentów, scen, assetów ani interpretacji. Nie rozstrzyga losu Parent B ani prawdziwości interpretacji Nocy Zamkniętego Progu.

## Kolejka po MQ31

Po pełnym zakończeniu MQ31 należy ponownie przeanalizować aktualny `main`, trzy dokumenty kolejki/coverage/readiness oraz karty Aktu III. MQ32 może rozpocząć się wyłącznie po zielonym CI i merge MQ31 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ31: finalna treść dokumentów, layout archiwum i presentation pozostają content/art lockiem.
- MQ31: dokument podstawowy i wtórny rejestr są równoważnymi kanałami recovery dla każdego krytycznego faktu; żaden pojedynczy dokument nie może być blockerem.
- MQ31: fakty potwierdzają role/powiązania, ale nie ustalają jeszcze jednej prawdziwej interpretacji Nocy ani losu Parent B.
