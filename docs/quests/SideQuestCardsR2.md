# Side Quest Production Cards — R2 Wielki Bór v0.1

Status: implementation-ready first pass. Rozwija stabilne sloty `SQ_R2_01`–`SQ_R2_03` z `SideQuestCatalog.md`. Tytuły pozostają robocze F. Dokument nie rozstrzyga identity locku `forest-guardian`, finalnych dialogów, wartości nagród ani finalnego encounter composition.

## Wspólny kontrakt R2

Każdy quest zapisuje `quest_id`, fazę, cele, evidence provenance, decyzje, world-state deltas i checkpoint zgodnie z `QuestDesign.md`. Mutacje świata i nagrody są idempotentne. Utrata opcjonalnego givera nie tworzy softlocku. R2 może budować niepewność interpretacji lasu, ale nie może przedstawiać `forest-guardian` jako potwierdzonego historycznego lub folklorystycznego bytu przed zamknięciem research/identity locku.

## SQ_R2_01 — Drzewa znaczone dwa razy

- **Typ / funkcja:** badawczy; tracking, evidence provenance i sprzeczne interpretacje śladów.
- **Wejście:** swobodny dostęp do R2 i podstawowy tracking.
- **Trigger:** odkrycie co najmniej jednego drzewa z dwiema warstwami oznaczeń albo lead od lokalnego NPC.
- **Fazy:** `Offered/Active` → lokalizacja próbek → porównanie evidence → zebranie dwóch interpretacji → decyzja o zapisie wniosku → `Resolved`.
- **Cele:** ustalić kolejność powstania oznaczeń, oddzielić obserwację od interpretacji i zdecydować, czy ślad wskazuje działalność ludzi, zjawisko naturalne/anomalne czy pozostaje nierozstrzygnięty.
- **Evidence contract:** minimum trzy obserwacje z provenance; co najmniej jedna musi pozwalać zakwestionować pierwszą hipotezę. Silnik przechowuje evidence ID i confidence, nie „prawdę” wpisaną na sztywno w UI.
- **Legalne outcomes:** `human_pattern`, `anomalous_pattern`, `mixed_or_uncertain`. Outcome anomalny nie może automatycznie oznaczać konkretnej istoty.
- **Stan trwały:** `sq_r2_01_outcome`; `r2_marked_trees_knowledge`; lista odkrytych evidence IDs.
- **World-state:** dziennik i opcjonalne reakcje NPC; brak obowiązkowej zmiany geometrii świata.
- **Dependencies:** R2 bible, TrackingSystem, evidence/journal, persistence.
- **Fail-forward:** utrata jednego źródła interpretacji pozostawia terenowe evidence; brak pełnego zestawu pozwala zamknąć quest jako `mixed_or_uncertain`.
- **Nagrody:** knowledge/reputation lub dostęp do dodatkowego leadu; liczby pozostają playtest lockiem.
- **Save/load:** evidence i confidence wracają bez ponownego przyznawania knowledge flag.
- **Minimalne QA:** trzy outcomes osiągalne; provenance zachowane po reload; brak wymuszonego `forest-guardian`; brak blokady traversal/MQ.

## SQ_R2_02 — Puste sidła

- **Typ / funkcja:** kontrakt łowcy; ekologia/istota, investigation → preparation → rozwiązanie z opcją bez walki.
- **Wejście:** dostęp do R2, tracking i encounter framework.
- **Trigger:** seria pustych/uszkodzonych sideł albo zgłoszenie zmiany zachowania zwierzyny.
- **Fazy:** `Offered/Active` → oględziny sideł i szlaków → evidence set → hipoteza → przygotowanie → encounter/interwencja → `Resolved/TurnedIn`.
- **Creature/content lock:** `r2_trap_contract_target` jest data-driven. Może wskazać zatwierdzony slot bestiariusza albo jawny F po content locku. Nie może domyślnie wskazywać `forest-guardian`.
- **Evidence contract:** co najmniej dwa niezależne typy śladów oraz jedna obserwacja stanu środowiska; finalny package targetu dostarcza konkretne ślady i reakcje.
- **Legalne rozwiązania:** usunięcie przyczyny ludzkiej/środowiskowej; odstraszenie/odprowadzenie; neutralizacja, jeśli zezwala finalna karta targetu; pozostawienie strefy po udokumentowaniu ryzyka.
- **Stan trwały:** `sq_r2_02_outcome = cause_removed|nonlethal|lethal|documented_unresolved`; `r2_trap_zone_state`; `r2_trap_target_id`.
- **World-state:** wariant ambient fauna/hazard i reakcje lokalnych NPC; nie blokuje krytycznego szlaku regionu.
- **Dependencies:** R2 bible, production bestiary, TrackingSystem, encounter format, persistence.
- **Fail-forward:** utrata givera po aktywacji nie blokuje terenowego rozwiązania; target package musi mieć fallback, gdy creature NPC nie może się pojawić; `documented_unresolved` jest legalnym terminalnym wynikiem.
- **Nagrody:** hunter progression/reputation/material lub wiedza zależnie od outcome; wartości pozostają tuning lockiem.
- **Save/load:** target ID, evidence, preparation i zone-state są trwałe; encounter nie respawnuje jako nierozwiązany po terminalnym outcome.
- **Minimalne QA:** test co najmniej jednej ścieżki non-combat; reload przed/po encounter; brak zależności od niezatwierdzonego guardian lore; brak softlocku po utracie givera.

## SQ_R2_03 — Obozowisko, które wraca

- **Typ / funkcja:** eksploracyjny; traversal/anomaly i czytelna mutacja stanu miejsca.
- **Wejście:** dostęp do głębszej części R2; quest może rozpocząć się bez givera.
- **Trigger:** ponowne odnalezienie pozornie tego samego obozowiska w niespójnym miejscu albo wykrycie powtarzalnego zestawu landmarków.
- **Fazy:** `Active` → potwierdzenie powtórzenia → oznaczenie punktów odniesienia → test hipotezy → wybór interwencji → `Resolved`.
- **Cele:** dać graczowi narzędzia do odróżnienia błędu nawigacji od lokalnego zjawiska bez ujawniania centralnego author truth przed właściwymi reveal gates.
- **Anomaly contract:** konkretna przyczyna może pozostać F/content lockiem. Quest operuje na obserwowalnym zachowaniu: relacje landmarków, powtarzalność przedmiotów i zmiana ścieżki. Nie nazywa automatycznie Sieci/Czwartej Sfery.
- **Legalne rozwiązania:** oznaczyć i ominąć strefę; wykonać bezpieczną lokalną stabilizację, jeśli system ją wspiera; pozostawić udokumentowaną anomalię. Destrukcja źródła nie jest domyślnym wymaganiem.
- **Stan trwały:** `sq_r2_03_outcome = bypassed|locally_stabilized|documented`; `r2_returning_camp_state`; odkryte landmark/evidence IDs.
- **World-state:** strefa wybiera stabilny wariant traversal/ambient po rozwiązaniu; nie może losowo cofać terminalnego stanu po reload.
- **Dependencies:** R2 bible, traversal, anomaly presentation, journal/evidence, persistence; central lore reveal gates.
- **Fail-forward:** błędna hipoteza nie zabiera możliwości oznaczenia/ominięcia; brak opcjonalnego narzędzia stabilizacji pozostawia `bypassed` i `documented`.
- **Nagrody:** wiedza, bezpieczniejszy traversal albo opcjonalny resource access; wartości pozostają tuning lockiem.
- **Save/load:** camp variant, landmark observations i outcome odtwarzają się deterministycznie.
- **Minimalne QA:** brak przedwczesnego central-lore reveal; każdy terminalny outcome stabilny po reload; brak nieskończonej pętli teleport/traversal; quest działa bez givera.

## Zależności i locki R2

1. `forest-guardian` pozostaje identity/research lockiem i nie jest wymagany przez żadną z trzech kart.
2. `SQ_R2_02` wymaga finalnego `r2_trap_contract_target` przed content lockiem, ale state machine może być implementowana wcześniej.
3. `SQ_R2_03` może użyć fikcyjnego zachowania anomalii tylko jako jawnie oznaczonego F; nie może zmieniać `FourthSphereTruth.md`.
4. Finalne dialogi, reward values, evidence thresholds, encounter composition i POI coordinates pozostają odpowiednio dialogue/playtest/content lockiem.

## Definition of Ready

R2 jest gotowe do implementacji first-pass, gdy trzy state machines używają stabilnych ID i persistence keys, evidence zachowuje provenance, target `SQ_R2_02` jest wymienialny przez dane, a żaden test/content fixture nie awansuje `forest-guardian` do canon lore.
