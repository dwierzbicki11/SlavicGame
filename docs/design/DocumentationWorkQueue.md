# Kolejność dalszej dokumentacji — production phase

Pierwszy szkielet designu i central lore jest ukończony. Kolejka od 2026-10-02 służy domykaniu dokumentacji produkcyjnej, bez duplikowania istniejących bible/speców.

## Osiągnięty próg implementacyjny

- [x] system design first pass;
- [x] central lore / author truth;
- [x] campaign regional flow;
- [x] region bibles R0–R6;
- [x] main quest skeleton i production cards MQ00–MQ56;
- [x] podstawowe content formats i trwałe ID;
- [x] persistence/save contract;
- [x] vertical slice content package;
- [x] production NPC/companion roster;
- [x] production bestiary roster scope 1.0.

Na tym poziomie można swobodnie implementować kolejne systemy. Otwarte locki są jawnie zebrane w `DocumentationCoverage.md`.

## Kolejka produkcyjna — od najmniejszego ryzyka

### P1 — roster i katalogi
- [x] production NPC/companion roster;
- [x] production bestiary roster;
- [x] indeks finalnych research cards istot;
- [x] full-game content/asset family catalog R0–R6 (`design/RegionalContentAssetCatalog.md`);
- [x] side-quest catalog poza vertical slice (`quests/SideQuestCatalog.md`).

### P2 — research packages
- [x] culture research framework i R0–R5 evidence packages v0.1;
- [x] krytyczny source-policy lock panteonu (`research/pantheon/CriticalSourceLock.md`);
- [x] material-culture production asset research lock v0.1 (`research/material-culture/ProductionAssetResearchLock.md`);
- [x] source-strength/region-fit pass południcy (`research/bestiary/Poludnica.md` v0.2);
- [x] identity lock `forest-guardian`: CLOSED/F (`research/bestiary/ForestGuardianIdentityLock.md`);
- [ ] ogniki/błędne światła tylko jeśli awansują do finalnego scope.

### P3 — content production
- [x] side-quest production cards per region; R0–R6, 21/21 slotów;
- [x] regional encounter rosters/tables poza R0 (`design/RegionalEncounterRosters.md`);
- [x] regional vendors/services final pass (`design/RegionalVendorsServices.md`);
- [x] item/equipment/recipe catalogs dla pełnego scope (`design/ItemEquipmentRecipeCatalog.md`);
- [x] dialogue packages implementation contract (`dialogue/FullGameDialoguePackages.md`).

### P4 — asset i budget lock
- [x] asset families per region z reuse/LOD/variant strategy na poziomie planowania;
- [x] konkretne production asset manifests / stable integration records R0–R6 (`design/ProductionAssetManifests.md`); exact historical-final forms i final art IDs pozostają jawnie research/art lockiem;
- [x] animation/VFX/audio budget contract (`design/AnimationVfxAudioBudgetContract.md`);
- [x] streaming i memory budget contract (`design/StreamingMemoryBudgetContract.md`);
- [x] AI/encounter density budget contract (`design/AiEncounterDensityBudgetContract.md`);
- [x] production estimates framework (`design/ProductionEstimatesFramework.md`).

### P5 — playtest/measurement lock
- [x] measurement/playtest evidence contract (`design/MeasurementPlaytestEvidence.md`);
- [x] centralny evidence ledger z trwałymi target IDs (`design/P5EvidenceLedger.md`); wartości pozostają puste do realnych captures;
- [x] reprodukowalne scenario manifests dla combat/economy/progression i evidence/reputation (`design/BalancePlaytestScenarioManifests.md`); finalne liczby pozostają playtest lockiem;
- [x] traversal/weather/day-night scenario manifests (`design/TraversalWeatherDayNightScenarioManifests.md`); tuning pozostaje measurement/playtest lockiem;
- [x] performance/release measurement manifests (`design/PerformanceReleaseMeasurementManifests.md`); targety liczbowe i hardware pozostają measurement lockiem;
- [ ] combat/economy/progression tuning — wykonać realne scenariusze i promować odpowiednie `EV-*` do MEASURED/CANDIDATE/LOCKED;
- [ ] evidence/reputation thresholds — wykonać realne scenariusze i promować odpowiednie `EV-*`;
- [ ] traversal/weather/day-night tuning — wykonać scenariusze i promować odpowiednie `EV-*`;
- [ ] measured CPU/GPU/RAM/VRAM/streaming/AI/AVFX targets z raw captures;
- [ ] minimal/recommended hardware po pomiarach pełnego manifest setu;
- [ ] release criteria evidence dla konkretnego candidate SHA.

## Reguły kolejki

1. Najpierw najniższy niezablokowany priorytet.
2. Nie wymyślamy wartości oznaczonych research/art/playtest/performance lock.
3. Nowy dokument powstaje tylko, gdy nie istnieje owner zakresu.
4. Zmiana central lore wymaga uzasadnienia i aktualizacji zależności.
5. Każdy production card wskazuje persistence, dependencies, fail-forward i minimalne QA.
6. Coverage aktualizujemy po każdym większym pakiecie.
7. Wynik P5 nie istnieje jako production evidence, dopóki nie ma wpisu/rekordu w `P5EvidenceLedger.md` wskazującego exact SHA i raw artifact locator.

## Następny element

P1–P4 mają implementation/planning-level pass. P5 ma evidence contract, centralny pusty ledger targetów oraz reprodukowalne scenario manifests dla gameplay/world i performance/release. Dokumentacyjny kontrakt pomiarów jest zamknięty; następny etap P5 wymaga już **implementacji brakującej instrumentacji, realnego wykonania scenariuszy/profilowania i zapisania MEASURED/CANDIDATE/LOCKED evidence w ledgerze**. Bez rzeczywistych captures nie deklarujemy targetów performance, hardware ani release evidence.