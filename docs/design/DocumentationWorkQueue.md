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
- [x] culture research framework: stabilne `CULT_*` IDs, H/R/F/U oraz source/evidence policy (`research/cultures/CultureResearchFramework.md`);
- [x] culture research package dla każdego finalnego kontekstu kulturowego (R0–R5) na poziomie evidence package v0.1; R5 ma osobną wieloźródłową bazę porównawczą i jawne locki zamiast stereotypowej analogii;
- [x] krytyczny source-policy lock panteonu (`research/pantheon/CriticalSourceLock.md`): hierarchia świadectwo/opracowanie/rekonstrukcja, regionalność i H/R/F/U są kontraktem produkcyjnym;
- [ ] research lock material culture dla produkcyjnych assetów — następnie finalne locatory;
- [x] source-strength/region-fit pass południcy (`research/bestiary/Poludnica.md` v0.2): rozdzielone H/R/F/U, R4 nie jest signature fit, R5 tylko warunkowo na faktycznych polach;
- [x] identity lock `forest-guardian`: CLOSED/F (`research/bestiary/ForestGuardianIdentityLock.md`); świadoma fikcja zamiast wymuszonej etykiety folklorystycznej;
- [ ] ogniki/błędne światła tylko jeśli awansują do finalnego scope.

### P3 — content production
- [x] side-quest production cards per region; R0–R6 gotowe (`quests/SideQuestCardsR0.md`–`SideQuestCardsR6.md`), 21/21 planowanych slotów ma implementacyjny first pass;
- [ ] regional encounter rosters/tables poza R0;
- [ ] regional vendors/services final pass;
- [ ] item/equipment/recipe catalogs dla pełnego scope;
- [ ] dialogue packages po zamknięciu rosterów.

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

Culture evidence packages R0–R5, source-strength/region-fit południcy, identity lock `forest-guardian` i krytyczny source-policy lock panteonu są zamknięte na poziomie potrzebnym do implementacji. Następny najmniejszy niezablokowany pakiet P2: **finalne material-culture locators / production asset research lock**. Równolegle P3 może przejść do regionalnych encounter rosters/tables poza R0.