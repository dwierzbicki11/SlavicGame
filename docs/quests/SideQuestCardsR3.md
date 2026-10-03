# Side Quest Production Cards — R3 Przymorze v0.1

Status: implementation-ready first pass. Rozwija stabilne sloty `SQ_R3_01`–`SQ_R3_03` z `SideQuestCatalog.md`. Tytuły są robocze F. Dokument nie rozstrzyga finalnych jednostek, praktyk rybackich/salvage, creature targetu, dialogów, reward values ani encounter composition.

## Wspólny kontrakt R3

Każdy quest zapisuje `quest_id`, fazę, cele, decyzje, world-state deltas i checkpoint zgodnie z `QuestDesign.md`. Mutacje świata i nagrody są idempotentne. Szczegóły jednostek, sieci, połowu, napraw, prawa własności i odzysku ładunku muszą pochodzić z odpowiedniego culture/material-culture research package; do research locku runtime używa neutralnych slotów danych.

## SQ_R3_01 — Sieci po sztormie

- **Typ / funkcja:** wspólnota; weather + lokalny supply/service recovery.
- **Wejście:** dostęp do R3; aktywny weather/world-state framework.
- **Trigger:** stan po sztormie albo lead od lokalnego service/NPC slotu.
- **Fazy:** `Offered/Active` → inspekcja szkód → wybór priorytetu pomocy → wykonanie → walidacja dostępu/supply → `Resolved/TurnedIn`.
- **Cele:** przywrócić bezpieczny lokalny supply albo ustanowić trwały fallback bez hardcodowania historycznej techniki połowu czy naprawy.
- **Legalne outcomes:** `supply_restored`, `fallback_route`, `limited_service`. Żaden wynik nie blokuje MQ.
- **Stan trwały:** `sq_r3_01_outcome`; `r3_post_storm_service_state`; `r3_supply_access`.
- **World-state:** wariant usługi, dostępności lokalnego zasobu i scenografii po sztormie; odtwarzany deterministycznie po reload.
- **Dependencies:** R3 bible, weather, economy/vendors, persistence, `ASSETSET_R3_CORE`; finalne narzędzia/jednostki/praktyki są research lockiem.
- **Fail-forward:** brak givera nie blokuje terenowego rozwiązania; brak finalnego assetu naprawy używa neutralnego service-state zamiast pseudohistorycznego placeholdera.
- **Nagrody:** supply/service/reputation access; liczby pozostają tuning lockiem.
- **Save/load:** outcome, service state i jednorazowa nagroda nie resetują się.
- **Minimalne QA:** trzy outcomes; reload przed/po zmianie usługi; weather transition nie resetuje questa; brak niezatwierdzonych szczegółów material culture.

## SQ_R3_02 — Ładunek z wraku

- **Typ / funkcja:** eksploracyjny; weather risk + salvage + konflikt własności/reputacji.
- **Wejście:** dostęp do R3 i traversal/weather framework.
- **Trigger:** odkrycie wraku/ładunku albo neutralny lead od lokalnego NPC/frakcji.
- **Fazy:** `Offered/Active` → lokalizacja wraku → ocena ryzyka → odzysk lub oznaczenie → decyzja o losie ładunku → `Resolved/TurnedIn`.
- **Ownership lock:** `r3_wreck_claimant_id`, `r3_wreck_cargo_id` i reguły roszczenia są content/research data. Karta nie zakłada konkretnego prawa, zwyczaju ani towaru.
- **Legalne outcomes:** `returned`, `shared`, `kept`, `marked_unrecovered`. Każdy może wpływać na reputację/usługę, ale nie na możliwość ukończenia kampanii.
- **Stan trwały:** `sq_r3_02_outcome`; `r3_wreck_state`; `r3_wreck_claimant_id`; `r3_wreck_cargo_id`; reward flag.
- **World-state:** wrak/ładunek przechodzi do terminalnego wariantu; reakcje NPC/frakcji wynikają z danych contentowych.
- **Dependencies:** R3 bible, weather/traversal, economy, NPC lifecycle, persistence, material-culture package dla finalnego salvage/cargo.
- **Fail-forward:** utrata claimanta po aktywacji pozostawia `shared|kept|marked_unrecovered`; nie istnieje unikalny krytyczny przedmiot wymagany przez MQ.
- **Nagrody:** reputation/resource/service/knowledge zależnie od outcome; wartości pozostają playtest lockiem.
- **Save/load:** odzyskany ładunek nie duplikuje się, terminalny wrak nie wraca do stanu początkowego, claimant może być zastąpiony fallbackiem danych.
- **Minimalne QA:** wszystkie outcomes stabilne; reload podczas weather risk i po odzysku; brak duplikacji cargo; brak hardcoded prawa własności lub technologii salvage.

## SQ_R3_03 — Głos spod pomostu

- **Typ / funkcja:** kontrakt łowcy; investigation → evidence → preparation → interwencja przy wodnym POI.
- **Wejście:** dostęp do R3, tracking/evidence i encounter framework.
- **Trigger:** zgłoszenie powtarzalnego głosu/dźwięku albo obserwacja anomalii przy pomoście.
- **Fazy:** `Offered/Active` → oględziny → evidence set → hipoteza → przygotowanie → encounter/interwencja → `Resolved/TurnedIn`.
- **Creature lock:** `r3_water_contract_target` jest data-driven i wymaga research ownera przed content lockiem. Może wskazać zatwierdzoną istotę, naturalne wyjaśnienie albo jawny F/anomaly package; karta nie przypisuje bytu samodzielnie.
- **Evidence contract:** minimum dwa niezależne ślady i jedna obserwacja środowiskowa; target package definiuje finalne sensory, reakcje i kontrdowody.
- **Legalne rozwiązania:** rozpoznanie naturalnej przyczyny; odstraszenie/odprowadzenie; neutralizacja, jeśli target package ją dopuszcza; `documented_unresolved` jako legalny fail-forward.
- **Stan trwały:** `sq_r3_03_outcome = natural|nonlethal|lethal|documented_unresolved`; `r3_water_target_id`; evidence IDs; `r3_pier_hazard_state`.
- **World-state:** lokalny hazard/POI i reakcje NPC; quest nie tworzy nowego central-lore reveal ani nie wyprzedza bram MQ.
- **Dependencies:** R3 bible, production bestiary, bestiary research owner, TrackingSystem/evidence, encounter format, persistence.
- **Fail-forward:** utrata givera pozostawia terenowy quest; brak finalnego targetu blokuje wyłącznie content lock, nie implementację state machine; `documented_unresolved` kończy kontrakt bez fałszywego lore.
- **Nagrody:** hunter progression/reputation/knowledge/material zależnie od outcome; wartości pozostają tuning lockiem.
- **Save/load:** target/evidence/hazard state są trwałe; terminalny encounter nie respawnuje jako nierozwiązany.
- **Minimalne QA:** co najmniej jedna ścieżka non-combat; target wymienialny przez dane; reload przed/po encounterze; brak awansu niezweryfikowanej istoty do canon.

## Zależności i locki R3

1. Finalne jednostki, rybołówstwo, sieci, naprawy, salvage, towary i reguły własności pozostają culture/material-culture research lockiem.
2. `SQ_R3_03` wymaga finalnego `r3_water_contract_target` z research ownerem przed content lockiem; state machine i persistence są implementowalne wcześniej.
3. Finalne dialogi, reward values, evidence thresholds, weather timings, encounter composition, POI coordinates i asset manifests pozostają odpowiednio dialogue/playtest/content/art lockiem.
4. Żaden quest R3 nie może być jedynym źródłem krytycznego evidence dla MQ ani zmieniać central author truth.

## Definition of Ready

R3 jest gotowe do implementacji first-pass, gdy trzy state machines używają stabilnych ID/persistence keys, weather/service/world-state odtwarzają się deterministycznie, cargo/claimant i target `SQ_R3_03` są wymienialne przez dane, a fixtures/testy nie utrwalają nierozstrzygniętych praktyk lub istot jako canon.