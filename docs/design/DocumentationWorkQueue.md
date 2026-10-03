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
- [x] regional encounter rosters/tables poza R0 (`design/RegionalEncounterRosters.md`): R1–R6 family-level implementation pass, persistence/filters/QA; final weights, density i placement pozostają playtest/data lockiem;
- [x] regional vendors/services final pass (`design/RegionalVendorsServices.md`): R0–R6 service footprint, stable IDs, stock profiles, persistence/fail-forward; final prices/restock/signature goods pozostają balance/item/research lockiem;
- [x] item/equipment/recipe catalogs dla pełnego scope (`design/ItemEquipmentRecipeCatalog.md`): stable families/IDs, equipment mapping, recipe gates, regional source matrix, persistence/QA; final tuning i exact historical assets pozostają lockiem;
- [x] dialogue packages implementation contract (`dialogue/FullGameDialoguePackages.md`): R0–R6 + campaign families, stable IDs, knowledge gates, persistence/fail-forward/localization/VO separation; final line writing/VO pozostają content lockiem.

### P4 — asset i budget lock
- [x] asset families per region z reuse/LOD/variant strategy na poziomie planowania;
- [ ] konkretne asset manifests/model/material/animation/audio/VFX records po art/research lockach;
- [ ] animation/VFX/audio budgets;
- [ ] streaming i memory budgets;
- [ ] AI/encounter density budgets;
- [ ] production estimates zależne od faktycznej przepustowości zespołu.

### P5 — playtest/measurement lock
- [ ] combat/economy/progression tuning;
- [ ] evidence/reputation thresholds;
- [ ] traversal/weather/day-night tuning;
- [ ] measured CPU/GPU/RAM/VRAM/streaming targets;
- [ ] minimal/recommended hardware po pomiarach;
- [ ] release criteria evidence.

## Reguły kolejki

1. Najpierw najniższy niezablokowany priorytet.
2. Nie wymyślamy wartości oznaczonych research/art/playtest/performance lock.
3. Nowy dokument powstaje tylko, gdy nie istnieje owner zakresu.
4. Zmiana central lore wymaga uzasadnienia i aktualizacji zależności.
5. Każdy production card wskazuje persistence, dependencies, fail-forward i minimalne QA.
6. Coverage aktualizujemy po każdym większym pakiecie.

## Następny element

P1–P3 mają implementation-level pass dla wymaganych rosterów, questów, encounterów, usług, item/equipment/recipe i dialog package contract. Następny najmniejszy niezablokowany pakiet to **P4: konkretne asset manifests dla rodzin, które mają wystarczający research/art lock**, bez wymyślania finalnych assetów historycznych tam, gdzie locator nadal jest za słaby. Następnie budżety animation/VFX/audio oraz streaming/memory/AI.