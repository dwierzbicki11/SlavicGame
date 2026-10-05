# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ41. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

## Zasada sekwencyjna

Implementujemy dokładnie jeden element naraz. Następny element może rozpocząć się dopiero po ukończeniu testów, zielonym CI i scaleniu bieżącego PR do `main`.

## Ukończone

- [x] Act 0: MQ00 → MQ01 → MQ10 handoff — scalone.
- [x] MQ10 „Znak pod drogą” — scalone.
- [x] MQ11 „Prawo łowcy” — scalone po zielonym CI.
- [x] MQ12 „Las, który myli drogę” — scalone po zielonym CI.
- [x] MQ13 „Dwie mapy” — scalone po zielonym CI.
- [x] MQ20 „Sól i milczenie” — scalone po zielonym CI.
- [x] MQ21 „Cena przejścia” — scalone po zielonym CI.
- [x] MQ22 „Kamień pod kamieniem” — scalone po zielonym CI.
- [x] MQ23 „Żelazna Brama” — scalone po zielonym CI.
- [x] MQ24 „Droga bez granicy” — scalone po zielonym CI.
- [x] MQ25 „Archiwum bez jednego języka” — scalone po zielonym CI.
- [x] MQ30 „Pustkowie Pierwszego Progu” — scalone po zielonym CI.
- [x] MQ31 „Archiwum popiołu” — scalone po zielonym CI.
- [x] MQ32 „Noc Zamkniętego Progu” — scalone po zielonym CI.
- [x] MQ33 „Rozwierający” — scalone po zielonym CI.
- [x] MQ34 „Trzy projekty” — scalone po zielonym CI.
- [x] MQ40 „Droga umarłych” — scalone po zielonym CI.
- [x] MQ41 „Dwa echa” — oba echa, operacyjne rozróżnienie Nawia/Splot, recovery i handoff do MQ42 — scalone po zielonym CI.

## Aktywny element

### MQ42 „Ten, który pozostał” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct4.md`:

1. wejście wyłącznie po `MQ41_COMPLETE`;
2. ścieżka spotkania wymaga znalezienia Parent B, weryfikacji tożsamości i decyzji relacji/obietnicy;
3. alternatywna ścieżka skip zapisuje `MQ42_SKIPPED` i nie tworzy fałszywego spotkania;
4. `MQ42_PARENT_B_DECISION` i `MQ42_SKIPPED` są wzajemnie wykluczające;
5. obie legalne ścieżki zapisują `MQ42_COMPLETE` i oferują MQ43 dokładnie raz;
6. save/load zachowuje evidence, wybór i completion, a ponowne completion nie resetuje postępu MQ43;
7. brak Parent B nie blokuje głównego reveal — pełna prawda Splotu pozostaje w MQ43.

## Kolejka po MQ42

Po pełnym zakończeniu MQ42 należy ponownie przeanalizować aktualny `main`, WorkQueue, Coverage, ten dokument oraz kartę Aktu IV. MQ43 może rozpocząć się wyłącznie po zielonym CI i merge MQ42 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ42: wygląd/staging Parent B i finalne line writing pozostają content/art/VO lockiem.
- MQ42: konkretna treść decyzji relacji/obietnicy może zostać rozwinięta później; runtime zapisuje trwały fakt podjęcia decyzji bez zgadywania wariantów prezentacyjnych.
- MQ42 nie może zastąpić MQ43 jako źródła pełnej prawdy Splotu.
