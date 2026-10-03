# Side Quest Production Cards — R6 Pustkowie Pierwszego Progu v0.1

Status: implementation-ready first pass. Rozwija stabilne sloty `SQ_R6_01`–`SQ_R6_03` z `SideQuestCatalog.md`. Tytuły są robocze F. Dokument respektuje central author truth i reveal gates MQ30+; side content nie może ujawnić prawdy Splotu przed MQ43 ani zastąpić obowiązkowych revealów MQ31/MQ32.

## Wspólny kontrakt R6

Każdy quest zapisuje `quest_id`, fazę, cele, evidence, decyzje, world-state deltas i checkpoint zgodnie z `QuestDesign.md`. Mutacje świata i nagrody są idempotentne. Wiedza zależna od postępu kampanii ma `reveal_gate_id`; dane niewidoczne dla gracza mogą istnieć w author truth, ale prezentacja i interpretacja muszą być filtrowane przez aktualny gate. Neutralne fixtures F nie stają się canonem.

## SQ_R6_01 — Rejestr bez właściciela

- **Typ / funkcja:** badawczy; opcjonalne archiwalne evidence pogłębiające kontekst MQ31/MQ32 bez zastępowania obowiązkowych revealów.
- **Wejście:** dostęp do R6 po odpowiednim gate kampanii; aktywny evidence/journal framework.
- **Trigger:** odkrycie `r6_orphan_record_source_id` albo wskazanie go przez dostępny NPC/marker środowiskowy.
- **Fazy:** `Offered/Active` → lokalizacja źródła → zebranie fragmentów → provenance check → porównanie z posiadaną wiedzą → decyzja zachować/przekazać/oznaczyć jako niepewne → `Resolved/TurnedIn`.
- **Reveal contract:** karta przechowuje `reveal_gate_id` per fragment. Przed właściwym MQ gate UI może pokazać tylko obserwację dostępną bohaterowi, nigdy author-truth interpretację. Żaden fragment nie ujawnia prawdy Splotu przed MQ43.
- **Legalne outcomes:** `preserved`, `shared`, `flagged_uncertain`, `partial_record`. Wszystkie zachowują pełną dostępność main quest.
- **Stan trwały:** `sq_r6_01_outcome`; lista `r6_record_evidence_ids`; provenance flags; `r6_record_knowledge_state`; jednorazowy reward flag.
- **World-state:** journal/knowledge hooks i opcjonalna reakcja NPC; bez retroaktywnej zmiany central lore.
- **Dependencies:** R6 bible, central author truth, MQ30+ gates, evidence/journal, persistence, NPC lifecycle.
- **Fail-forward:** brak części źródeł daje `partial_record`; utrata opcjonalnego interpretatora nie blokuje zamknięcia — gracz może zachować evidence bez interpretacji.
- **Nagrody:** knowledge/reputation/support hook; wartości pozostają tuning lockiem.
- **Save/load:** evidence/provenance/outcome są trwałe i nie można duplikować wpisów ani nagrody.
- **Minimalne QA:** start przed/po dozwolonym gate; brak spoileru MQ43; brak interpretatora; partial/full record; reload w każdej fazie.

## SQ_R6_02 — Martwy węzeł

- **Typ / funkcja:** eksploracyjny; high-risk traversal/anomaly i opcjonalna stabilizacja lokalnego hazardu.
- **Wejście:** dostęp do odpowiedniej części R6 oraz aktywne anomaly/hazard i traversal systems.
- **Trigger:** wejście w zasięg `r6_dead_node_id`, obserwacja skutku anomalii albo bezpieczny sygnał z systemu eksploracji.
- **Fazy:** `Active` → obserwacja hazardu → zebranie evidence → wybór podejścia → przygotowanie → traversal/interaction → walidacja stanu → `Resolved`.
- **Lore contract:** state machine nie nazywa przyczyny anomalii prawdą o Splocie. `anomaly_profile_id`, prezentacja VFX/audio i dostępne interpretacje są danymi filtrowanymi przez reveal gates.
- **Legalne outcomes:** `stabilized`, `bypassed`, `mapped_only`, `abandoned_safe`. Żaden nie blokuje kampanii.
- **Stan trwały:** `sq_r6_02_outcome`; `r6_dead_node_state`; discovered safe route; evidence IDs; jednorazowy reward flag.
- **World-state:** hazard może zostać ograniczony, zachowany albo otrzymać oznaczony bezpieczny bypass; zmiana jest lokalna i nie naprawia globalnego kryzysu.
- **Dependencies:** R6 bible, anomaly/hazard system, traversal, central lore gates, persistence, przyszły finalny VFX/audio/asset manifest.
- **Fail-forward:** brak wymaganej metody stabilizacji zawsze pozostawia `bypassed` lub `mapped_only`; śmierć/porzucenie nie niszczy obowiązkowej trasy MQ.
- **Nagrody:** access/knowledge/resource/support; konkretne wartości są playtest lockiem.
- **Save/load:** hazard i odkryta trasa odtwarzają się deterministycznie; stabilizacja i nagroda są idempotentne.
- **Minimalne QA:** wszystkie outcomes; wejście bez narzędzia; reload podczas hazardu i po stabilizacji; MQ route nadal działa; brak przedwczesnego reveal.

## SQ_R6_03 — Ostatni posterunek

- **Typ / funkcja:** osobisty/frakcja; los ocalałej grupy wpływa na wsparcie, koszt i epilog Aktu V.
- **Wejście:** odpowiedni gate R6/Aktu V; istnieje `r6_outpost_group_id` albo jawny fallback terminal state.
- **Trigger:** kontakt z posterunkiem, sygnałem grupy albo evidence jej ostatniej aktywności.
- **Fazy:** `Offered/Active` → ocena sytuacji → rozmowy/evidence → wybór planu → przygotowanie → wykonanie → rozliczenie availability/support → `Resolved/TurnedIn`.
- **Character/lore contract:** finalny skład, personalia i dialogi pochodzą z NPC roster/dialogue package. Grupa może znać wyłącznie informacje dopuszczone przez aktualny reveal gate; jej relacje są perspektywą, nie automatycznym author truth.
- **Legalne outcomes:** `evacuated`, `holds_position`, `relocated`, `fragmented`, `lost_before_resolution`. Mapowanie na finał odbywa się przez support/cost/epilogue hooks, bez tworzenia osobnej obowiązkowej ścieżki kampanii.
- **Stan trwały:** `sq_r6_03_outcome`; `r6_outpost_group_availability`; `r6_act5_support_hook`; `r6_epilogue_hook`; jednorazowy reward flag.
- **World-state:** availability grupy/NPC, lokalne zabezpieczenie lub opuszczenie posterunku i flagi wsparcia Aktu V.
- **Dependencies:** R6 bible, NPC roster/lifecycle, reputation/support, Act V/finale hooks, persistence, central author truth.
- **Fail-forward:** utrata części grupy zmienia dostępne outcomes; utrata całej grupy po aktywacji daje terminalny `lost_before_resolution`, zapisuje konsekwencję i nie blokuje MQ. Utrata givera nie blokuje terenowego resolution.
- **Nagrody:** support/reputation/access/epilogue; wartości i koszt finału pozostają playtest lockiem.
- **Save/load:** group availability, support i epilogue hooks są trwałe i idempotentne; terminalny outcome nie resetuje grupy.
- **Minimalne QA:** wszystkie outcomes; partial/full group loss; giver loss; reload przed/po resolution; finale hook obecny tylko raz; brak spoileru przed odpowiednim MQ gate.

## Zależności i locki R6

1. Central author truth pozostaje nadrzędnym ownerem przyczyn kryzysu, Splotu i Czwartej Sfery; side questy nie rozszerzają go samodzielnie.
2. Wszystkie informacje prezentowane graczowi muszą przechodzić przez `reveal_gate_id`; przed MQ43 nie wolno ujawnić prawdy Splotu.
3. Finalne dialogi, NPC assignments, reward/support values, anomaly composition, POI placement oraz asset/VFX/audio manifests pozostają odpowiednimi dialogue/content/playtest/art lockami.
4. Żaden quest R6 nie jest wymagany do zrozumienia ani ukończenia MQ30–MQ56.
5. Fixtures używane przed finalnym content lockiem muszą być jawnie F i wymienialne bez migracji state machine.

## Definition of Ready

R6 jest gotowe do implementacji first-pass, gdy trzy state machines używają stabilnych ID/persistence keys, reveal gating jest testowalne niezależnie od content text, wszystkie opcjonalne NPC/grupy mają fail-forward, lokalne world-state mutations są idempotentne, a side content nie może ujawnić author truth wcześniej niż main quest.