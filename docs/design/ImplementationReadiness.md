# Implementation Readiness

Stan kolejki implementacyjnej po ukończeniu MQ22. Dokument wskazuje, co można bezpiecznie implementować bez zgadywania i jaki jest aktualny element sekwencyjnej pracy.

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

## Aktywny element

### MQ23 „Żelazna Brama” — IMPLEMENTING

Karta `docs/story/MainQuestCardsAct2.md` ma status `implementation-ready v0.1`. Runtime implementuje wyłącznie kontrakt kampanii bez zgadywania finalnych księstw, NPC, nazw złoża, wartości zasobów ani parametrów reputacji:

1. wejście po `MQ22_COMPLETE`;
2. ocena trzech jawnych skutków decyzji: wydobycie, bezpieczeństwo i potencjalna stabilizacja;
3. trwała, pojedyncza decyzja `MQ23-D01`: kontrola jednej z dwóch stron / podział nadzorowany / ograniczenie wydobycia i rezerwa stabilizacyjna;
4. każdy wynik zapisuje trwały region state i zachowuje krytyczną trasę kampanii;
5. `MQ23_COMPLETE`, idempotencja, persistence i gwarantowany handoff do MQ24.

Finalne IDs/nazwy stron i NPC, nazwy materiałów, ilości zasobów, ceny, progi reputacji oraz skutki wsparcia w Akcie V pozostają content/tuning lockiem.

## Kolejka po MQ23

Po pełnym zakończeniu MQ23 należy ponownie przeanalizować aktualny `main`, `DocumentationWorkQueue.md`, `DocumentationCoverage.md` oraz karty Aktu II. MQ24 może zostać rozpoczęte wyłącznie, jeśli nadal spełnia próg implementation-ready po merge MQ23.

## Otwarte decyzje implementacyjne

- MQ23: finalne tożsamości obu stron konfliktu są content data; runtime zachowuje dwa neutralne sloty claimantów bez wymyślania nazw.
- MQ23: wartości zasobów, koszt pozyskania, ceny i reputacyjne progi są tuning/content lockiem.
- MQ23: każdy wariant `MQ23-D01` musi zachować dostępność MQ24 i finału; późniejsze skutki są konsumowane dopiero przez odpowiednie questy Aktu V.
