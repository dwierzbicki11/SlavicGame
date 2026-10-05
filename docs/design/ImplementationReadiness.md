# Implementation Readiness

Stan kolejki implementacyjnej po scaleniu MQ34. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ34 „Trzy projekty” — trzy trwałe modele, lead do Nawii i handoff do MQ40 — scalone po zielonym CI.

## Aktywny element

### MQ40 „Droga umarłych” — IMPLEMENTED, oczekuje na CI/merge

Kontrakt z `docs/quests/MainQuestCardsAct4.md`:

1. wejście po `MQ34_COMPLETE` i trwałym leadzie do Nawii;
2. przejście do Nawii przez stabilny próg oraz trwały `MQ40_ENTERED_NAVIA`;
3. obserwacja podstawowych reguł Nawii bez przedwczesnego ujawniania natury Splotu;
4. idempotentne ustanowienie trwałej kotwicy powrotnej `MQ40_RETURN_ANCHOR_SET`;
5. krytyczne instrukcje progu mają recovery niezależne od opcjonalnego przewodnika;
6. trop do dwóch podobnych ech domyka krytyczne beaty i zapisuje `MQ40_COMPLETE`;
7. handoff do MQ41 następuje dokładnie raz, z persistence/save-load i idempotencją.

## Kolejka po MQ40

Po pełnym zakończeniu MQ40 należy ponownie przeanalizować aktualny `main`, trzy dokumenty kolejki/coverage/readiness oraz kartę Aktu IV. MQ41 może rozpocząć się wyłącznie po zielonym CI i merge MQ40 oraz ponownym potwierdzeniu readiness.

## Otwarte decyzje implementacyjne

- MQ40: finalny staging przejścia, wygląd Nawii, przewodnik i line writing pozostają content/art/VO lockiem.
- MQ40: recovery instrukcji może być prezentowane przez Journal lub znaki przy progu; runtime zapisuje jedynie trwały fakt odzyskania instrukcji.
- MQ40: obserwacja reguł Nawii nie może ujawniać pełnej natury Splotu przed MQ43.
