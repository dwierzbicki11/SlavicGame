# Implementation Readiness

Stan kolejki implementacyjnej po ukończeniu MQ24. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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
- [x] MQ22 „Kamień pod kamieniem” — naruszenie kopalni, dwa niezależne ślady konstrukcji, materialny komponent kotwicy, trzy stabilizacje, fallback bez opcjonalnego NPC, persistence/idempotencja i handoff do MQ23 — scalone po zielonym CI.
- [x] MQ23 „Żelazna Brama” — trzy jawne skutki, trwała decyzja `MQ23-D01`, neutralne sloty stron, persistence/idempotencja i gwarantowany handoff do MQ24 — scalone po zielonym CI.
- [x] MQ24 „Droga bez granicy” — map mismatch, dwa niezależne sposoby pamięci trasy, recoverable trial passage, `MQ24_ROUTE_RELATION`, persistence/idempotencja i handoff do MQ25 — scalone po zielonym CI.

## Aktywny element

### MQ25 „Archiwum bez jednego języka” — IMPLEMENTING

Karta `docs/story/MainQuestCardsAct2.md` ma status `implementation-ready v0.1`. Runtime implementuje wyłącznie kontrakt syntezy Aktu II bez wymyślania finalnych języków, nazw, miar, tłumaczeń ani opcjonalnych dowodów:

1. wejście dopiero po `MQ20_COMPLETE`–`MQ24_COMPLETE`;
2. walidacja czterech krytycznych pakietów danych: Nadborze, Przymorze, Kamienne Wyżyny i Arel, oparta wyłącznie na istniejących evidence;
3. zachowanie pakietów jako odrębnych tradycji zamiast udawania jednego źródłowego języka;
4. wymaganie co najmniej dwóch zgodnych połączeń: relacji tras między regionami i dystrybucji materialnego komponentu kotwic;
5. zapis `NETWORK_MULTICULTURAL_ORIGIN`, `MQ25_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ30/Aktu III.

Finalne nazwy/miary/języki, wording archiwum, tłumaczenia, opcjonalne evidence i presentation pozostają content/localization lockiem.

## Kolejka po MQ25

Po pełnym zakończeniu MQ25 należy ponownie przeanalizować aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` oraz karty Aktu III. MQ30 może zostać rozpoczęte wyłącznie, jeśli nadal spełnia próg implementation-ready po merge MQ25.

## Otwarte decyzje implementacyjne

- MQ25: runtime nie tworzy jednego kanonicznego języka źródłowego; normalizacja zachowuje pochodzenie czterech pakietów danych.
- MQ25: minimalny evidence set używa wyłącznie krytycznych zapisów już gwarantowanych przez MQ13/MQ20/MQ22/MQ24; opcjonalne dowody nie mogą blokować syntezy.
- MQ25: wniosek wielokulturowy nie może ujawniać prawdy o rodzicach, Wszeborze, Nocy Zamkniętego Progu ani Splocie.
