# Side Quest Production Cards — R5 Równiny Arel v0.1

Status: implementation-ready first pass. Rozwija stabilne sloty `SQ_R5_01`–`SQ_R5_03` z `SideQuestCatalog.md`. Tytuły są robocze F. Dokument świadomie nie rozstrzyga appearance, ornamentu, praktyk, norm, mobilności, nazewnictwa, finalnych dialogów ani reward values przed zamknięciem culture research package R5.

## Wspólny kontrakt R5

Każdy quest zapisuje `quest_id`, fazę, cele, decyzje, world-state deltas i checkpoint zgodnie z `QuestDesign.md`. Mutacje świata i nagrody są idempotentne. Culture-locked dane są referencjami (`culture_practice_id`, `route_pattern_id`, `norm_rule_id`, `faction_role_id`), a nie tekstem zaszytym w logice. Implementacja może używać neutralnych fixtures oznaczonych F, które nie stają się canonem.

## SQ_R5_01 — Droga między obozami

- **Typ / funkcja:** wspólnota; navigation, relacje i bezpieczny dostęp między dwoma lokalnymi node'ami.
- **Wejście:** dostęp do R5; aktywny traversal/world-state framework.
- **Trigger:** prośba o ustalenie lub przywrócenie przejścia między `r5_route_node_a` i `r5_route_node_b` albo odkrycie zerwanej trasy.
- **Fazy:** `Offered/Active` → rekonesans → identyfikacja przeszkody → wybór wariantu trasy → wykonanie → walidacja przejścia → `Resolved/TurnedIn`.
- **Culture lock:** quest nie zakłada, dlaczego grupy przemieszczają się, jak wyglądają obozy ani jak nazywają drogę. `route_pattern_id` i reakcje społeczne pochodzą z przyszłego package R5.
- **Legalne outcomes:** `route_restored`, `alternate_route`, `limited_access`. Każdy zachowuje dostęp do kampanii.
- **Stan trwały:** `sq_r5_01_outcome`; `r5_route_state`; `r5_route_pattern_id`; jednorazowy reward flag.
- **World-state:** aktywny wariant przejścia, dostępność lokalnych usług/NPC i neutralne znaczniki sceny.
- **Dependencies:** R5 bible, traversal/navigation, NPC lifecycle, persistence, `ASSETSET_R5_CORE`; finalna reprezentacja obozów i mobilności jest research/art lockiem.
- **Fail-forward:** utrata givera nie blokuje terenowego rozwiązania; brak culture package używa neutralnego route-state bez stereotypowego placeholdera.
- **Nagrody:** access/reputation/service/knowledge; liczby pozostają tuning lockiem.
- **Save/load:** wybrana trasa i world-state nie resetują się ani nie duplikują nagrody.
- **Minimalne QA:** wszystkie trzy outcomes; reload przed i po zmianie trasy; brak hardcoded praktyki kulturowej; MQ pozostaje dostępne.

## SQ_R5_02 — Pamięć trasy

- **Typ / funkcja:** osobisty NPC / badawczy; oral knowledge i evidence bez tworzenia obowiązkowego reveal dla MQ24/MQ25.
- **Wejście:** dostęp do R5 i co najmniej jeden dostępny narrator/witness slot z rosteru albo fallback evidence source.
- **Trigger:** rozmowa z `r5_route_memory_source_id` lub znalezienie neutralnego śladu powiązanego z trasą.
- **Fazy:** `Offered/Active` → zebranie relacji → terenowa weryfikacja → porównanie evidence → decyzja jak zachować/przekazać wiedzę → `Resolved/TurnedIn`.
- **Evidence contract:** co najmniej dwa niezależne evidence IDs; relacja NPC jest perspektywą, nie automatycznym author truth. Culture package definiuje dopiero finalny sposób przekazywania wiedzy i społeczne znaczenie relacji.
- **Legalne outcomes:** `preserved`, `shared`, `kept_private`, `inconclusive`. Żaden nie jest wymagany do zrozumienia osi MQ.
- **Stan trwały:** `sq_r5_02_outcome`; `r5_route_memory_source_id`; evidence IDs; `r5_route_knowledge_state`.
- **World-state:** knowledge/reputation hooks i opcjonalny marker trasy; bez retroaktywnej zmiany central lore.
- **Dependencies:** R5 bible, evidence/tracking, NPC roster/lifecycle, persistence, przyszły culture research package.
- **Fail-forward:** utrata narratora po aktywacji przełącza quest na fallback evidence source albo `inconclusive`; brak unikalnego krytycznego NPC.
- **Nagrody:** knowledge/reputation/access; finalne wartości i progi są playtest lockiem.
- **Save/load:** evidence i outcome są trwałe; relacji nie można farmić przez reload.
- **Minimalne QA:** narrator dostępny/niedostępny; sprzeczne evidence; reload w investigation; brak awansu pojedynczej relacji do author truth.

## SQ_R5_03 — Gość czy zakładnik

- **Typ / funkcja:** frakcja; konflikt interpretacji statusu osoby i konsekwencje reputacji/availability NPC.
- **Wejście:** dostęp do R5; aktywny `r5_subject_npc_id` oraz co najmniej dwa zainteresowane role/faction slots.
- **Trigger:** gracz otrzymuje sprzeczne informacje o ograniczonej dostępności/przemieszczaniu `r5_subject_npc_id`.
- **Fazy:** `Offered/Active` → zebranie perspektyw → evidence → wybór interwencji lub odmowy → rozstrzygnięcie availability → `Resolved/TurnedIn`.
- **Norm lock:** karta nie nazywa sytuacji historycznym zwyczajem i nie zakłada prawa gościnności, zakładnictwa, hierarchii ani sankcji. `norm_rule_id`, role i dopuszczalne reakcje muszą pochodzić z zatwierdzonego culture package; tytuł pozostaje F.
- **Legalne outcomes:** `released`, `stays_voluntarily`, `transferred`, `non_intervention`, `unresolved`. Semantyka finalna jest mapowana przez content data po research locku.
- **Stan trwały:** `sq_r5_03_outcome`; `r5_subject_npc_id`; `r5_subject_availability`; `r5_norm_rule_id`; faction/reputation delta IDs.
- **World-state:** availability NPC i reakcje frakcji; żadna ścieżka nie blokuje MQ ani nie ustanawia niezweryfikowanej normy jako canon.
- **Dependencies:** R5 bible, production NPC roster, reputation/evidence, persistence; finalny culture research package jest wymagany przed content lockiem.
- **Fail-forward:** śmierć/niedostępność subject NPC przed aktywacją oznacza `Unavailable`; po aktywacji fallback state kończy quest bez wymuszania fikcyjnej normy. Utrata givera nie blokuje evidence-based resolution.
- **Nagrody:** reputation/NPC availability/service/knowledge zależnie od outcome; wartości pozostają tuning lockiem.
- **Save/load:** availability i faction deltas są idempotentne; terminalny outcome nie resetuje konfliktu.
- **Minimalne QA:** wszystkie legalne outcomes; subject/giver loss; reload przed/po resolution; brak hardcoded norm, ornamentu, appearance lub stereotypowej analogii do „stepu”.

## Zależności i locki R5

1. Finalny culture research package jest ownerem appearance, ornamentu, praktyk, norm, mobilności, nazewnictwa i społecznej interpretacji zachowań.
2. Karty mogą być implementowane wcześniej dzięki neutralnym data slots, ale fixtures muszą być oznaczone F i wymienialne bez migracji state machine.
3. Finalne dialogi, reward values, reputation/evidence thresholds, encounter/POI placement oraz asset manifests pozostają dialogue/playtest/content/art lockiem.
4. Żaden quest R5 nie może być jedynym źródłem informacji wymaganej do MQ24/MQ25 ani zmieniać central author truth.

## Definition of Ready

R5 jest gotowe do implementacji first-pass, gdy trzy state machines używają stabilnych ID/persistence keys, culture-locked wartości są wstrzykiwane przez dane, utrata opcjonalnych NPC ma fallback, a test fixtures nie utrwalają stereotypowych lub nierozstrzygniętych cech kultury jako canon.