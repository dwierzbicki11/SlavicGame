# Side Quest Production Cards — R1 Nadborze v0.1

Status: implementation-ready first pass. Rozwija stabilne sloty `SQ_R1_01`–`SQ_R1_03` z `SideQuestCatalog.md`. Tytuły pozostają robocze F. Dokument nie rozstrzyga finalnych dialogów, wartości ekonomii, material culture ani konkretnej istoty dla `SQ_R1_02`; te elementy zachowują jawne locki.

## Wspólny kontrakt R1

Każdy quest zapisuje `quest_id`, fazę, ukończone cele, decyzje, world-state deltas i checkpoint zgodnie z `QuestDesign.md`. Nagrody i mutacje świata są idempotentne. Utrata opcjonalnego givera nie może tworzyć softlocku. Żaden slot nie jest wymagany do głównej kampanii ani nie może zmienić centralnego author truth.

## SQ_R1_01 — Cło bez pieczęci

- **Typ / funkcja:** frakcja/economia; konflikt administracja–lokalny handel i demonstracja trwałych konsekwencji usług/reputacji.
- **Wejście:** swobodny dostęp do R1 oraz lokalnego węzła handlowego; quest może zostać odkryty przez kontrolę drogi, kupca albo zapis administracyjny.
- **Trigger:** pierwsze zetknięcie z zakwestionowanym poborem lub informacją o sporze. Giver nie jest wymagany po aktywacji.
- **Fazy:** `Offered/Active` → zebranie dwóch perspektyw → weryfikacja uprawnienia/evidence → decyzja → `Resolved` → opcjonalne `TurnedIn`.
- **Cele:** ustalić, czy pobór ma lokalną podstawę, kto ponosi koszt i jakie rozwiązanie jest wykonalne bez wymyślania finalnego prawa historycznego.
- **Legalne rozwiązania:** poprzeć pobór po potwierdzeniu danych; wynegocjować/udokumentować lokalny wyjątek; ujawnić brak podstawy; odmówić rozstrzygnięcia po zachowaniu evidence. Konkretne normy i terminologia pozostają culture/research lockiem.
- **Stan trwały:** `sq_r1_01_outcome = upheld|exception|exposed|unresolved`; `r1_toll_service_state`; relacyjne/reputacyjne flagi odpowiednich slotów NPC.
- **World-state:** wariant dostępu/usługi/ambient reakcji przy punkcie handlowym; nie może blokować krytycznego przejścia regionu.
- **Dependencies:** R1 bible, economy/vendors, NPC roster, evidence/journal, persistence.
- **Fail-forward:** śmierć/niedostępność jednego rozmówcy pozostawia dokument/evidence albo drugą stronę; utrata opcjonalnego dowodu nie blokuje minimalnej ścieżki; gracz zawsze może zakończyć jako `unresolved` bez blokady kampanii.
- **Nagrody:** reputacja, wariant usługi/ceny lub skrót administracyjny; wartości pozostają economy/playtest lockiem.
- **Save/load:** evidence provenance, decyzja i service-state są atomowe; ponowne wejście nie nalicza opłaty/nagrody drugi raz.
- **Minimalne QA:** wszystkie outcomes osiągalne; brak softlocku po utracie NPC; reload zachowuje service-state; main quest i traversal pozostają dostępne.

## SQ_R1_02 — Ślad przy młynie

- **Typ / funkcja:** kontrakt łowcy; investigation → preparation → encounter bez przedwczesnego przypisania istoty.
- **Wejście:** dostęp do R1, tracking i encounter framework; lokalny problem przy młynie może zostać zgłoszony lub odkryty terenowo.
- **Trigger:** powtarzalny ślad/szkoda albo lead od lokalnego NPC. Konkretny opis śladów jest danymi contentowymi zależnymi od późniejszego creature assignment.
- **Fazy:** `Offered/Active` → oględziny → zebranie minimalnego evidence setu → hipoteza → przygotowanie → encounter → `Resolved/TurnedIn`.
- **Evidence contract:** minimum dwa niezależne typy evidence z provenance; content package przypisuje je dopiero razem z zatwierdzonym creature/anomaly slotem. Silnik nie koduje nazwy istoty.
- **Creature lock:** `r1_mill_contract_target` pozostaje jawnym content/research lockiem. Do czasu przypisania karta może być implementowana na testowym `F_TEST_TARGET`, który nie jest canonem i nie trafia do finalnego contentu.
- **Legalne rozwiązania:** odstraszenie/odprowadzenie, neutralizacja, usunięcie przyczyny albo inne rozwiązanie dozwolone przez finalny creature card. Walka nie jest automatycznie obowiązkowa.
- **Stan trwały:** `sq_r1_02_outcome = resolved_nonlethal|resolved_lethal|cause_removed|abandoned`; `r1_mill_hazard_state`; zapis hipotezy i użytego przygotowania.
- **Dependencies:** R1 bible, TrackingSystem, EncounterDesign, production bestiary + research owner, inventory/preparation, material-culture lock młyna.
- **Fail-forward:** błędna hipoteza może zwiększyć koszt/ryzyko, ale nie niszczy jedynej ścieżki; utrata givera pozostawia terenowy contract resolution; encounter ma bezpieczny reset/re-entry zgodny z systemem encounterów.
- **Research boundary:** finalna istota, jej zachowanie folklorystyczne, wygląd i praktyki ochronne wymagają zatwierdzonego research card albo jawnego F. Konstrukcja/terminologia młyna nie jest dopisywana jako fakt bez material-culture ownera.
- **Nagrody:** hunter reputation, usługa/supply lub wiedza zależna od rozwiązania; liczby i dropy są playtest lockiem.
- **Save/load:** evidence, hipoteza, przygotowanie i stan encounteru odtwarzają się bez respawnu jednorazowych dowodów/nagród.
- **Minimalne QA:** quest działa z testowym abstrakcyjnym targetem; podmiana targetu jest data-driven; co najmniej jedna ścieżka nonlethal, jeśli finalny target ją dopuszcza; brak zależności MQ od wyniku.

## SQ_R1_03 — Woda dla Dębrzyna

- **Typ / funkcja:** wspólnota/infrastruktura; trwały wariant world state wpływający na lokalną usługę i ambient życie.
- **Wejście:** osada/punkt Dębrzyn dostępny; problem może zostać odkryty przez stan infrastruktury lub rozmowę.
- **Trigger:** obserwacja niedostępnego/niestabilnego źródła zaopatrzenia albo lokalny lead.
- **Fazy:** `Offered/Active` → diagnoza → wybór wariantu → wykonanie → walidacja dostępu → `Resolved/TurnedIn`.
- **Cele:** rozpoznać przyczynę problemu; wybrać bezpieczny wariant rozwiązania; zapisać trwałą zmianę bez hardkodowania finalnej technologii/material culture.
- **Legalne rozwiązania:** przywrócić istniejący dostęp; utworzyć zaakceptowane obejście; ograniczyć użycie i zapewnić alternatywny supply. Konkretne konstrukcje i narzędzia są research/art lockiem.
- **Stan trwały:** `sq_r1_03_outcome = restored|bypass|rationed`; `r1_debrzyn_water_state`; opcjonalny service/economy modifier.
- **World-state:** odpowiedni wariant props/nav/ambient oraz usługi; żadna wersja nie odcina krytycznych NPC ani głównej trasy.
- **Dependencies:** R1 bible, interaction, traversal, inventory/economy, NPC reactions, material-culture research.
- **Fail-forward:** brak konkretnego materiału pozostawia alternatywę; zniszczenie opcjonalnego propsa nie usuwa informacji; niedostępność givera pozwala zakończyć przez world-state validation.
- **Nagrody:** lokalna reputacja, supply/service convenience i ambient reakcje; bez gwarantowanego power spike.
- **Save/load:** mutacja infrastruktury i service-state są idempotentne; zapis w połowie prac nie tworzy podwójnych propsów ani kosztów.
- **Minimalne QA:** trzy outcomes odtwarzają poprawny world state; brak givera nie blokuje turn-in; economy modifier nie duplikuje się; traversal kampanii pozostaje poprawny.

## R1 cross-quest constraints

- `SQ_R1_01` może zmieniać dostęp/ceny usług, ale nie może uczynić wymaganych zasobów kampanii niedostępnymi.
- `SQ_R1_02` nie może zostać finalnie oznaczony jako research-locked folklorystyczny byt bez research ownera; implementacja state machine pozostaje niezależna od target ID.
- `SQ_R1_03` może wpływać na supply, lecz nie zastępuje systemowego economy/vendor fallbacku.
- Outcomes mogą zasilać ambient reactions i regionalną reputację, ale nie są przeliczane ponownie z aktualnych propsów.
- Kolejność ukończenia trzech questów nie może tworzyć wzajemnego softlocku.

## Otwarte decyzje R1

1. finalne tytuły, giver IDs, personalia i dialogi;
2. finalny creature/anomaly assignment i research owner dla `SQ_R1_02`;
3. material-culture lock młyna, infrastruktury wodnej i terminologii administracyjnej;
4. dokładne POI placement, props i encounter composition;
5. finalne wartości cen, reputacji, nagród i kosztów;
6. presentation/VFX/audio oraz ambient reactions.

## Definition of Ready

`SQ_R1_01`–`SQ_R1_03` mają stabilne wejścia, fazy, outcomes, persistence, fail-forward, dependencies i QA. `SQ_R1_02` jawnie oddziela implementowalną state machine od niezablokowanego jeszcze finalnego creature assignment, więc programowanie może ruszyć bez tworzenia fałszywego canon/research.