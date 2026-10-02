# Kolejność dalszej dokumentacji — production phase

Pierwszy szkielet designu i central lore jest ukończony. Kolejka od 2026-10-02 służy domykaniu dokumentacji produkcyjnej, bez duplikowania istniejących bible/speców.

## Osiągnięty próg implementacyjny

- [x] system design first pass;
- [x] central lore / author truth;
- [x] campaign regional flow;
- [x] region bibles R0–R6;
- [x] main quest skeleton MQ00–MQ56;
- [x] production cards MQ00–MQ56;
- [x] podstawowe content formats i trwałe ID;
- [x] persistence/save contract;
- [x] vertical slice content package.

Na tym poziomie można swobodnie implementować kolejne systemy, quest state machine i regionalny flow. Otwarte locki są jawnie zebrane w `DocumentationCoverage.md`.

## Kolejka produkcyjna — od najmniejszego ryzyka

### P1 — roster i katalogi
- [ ] production NPC/companion roster: finalne role, region, lifecycle, quest ownership, persistence;
- [ ] finalny bestiary roster używany przez scope 1.0;
- [ ] indeks finalnych research cards istot i brakujące karty;
- [ ] full-game content/asset catalog per R0–R6;
- [ ] side-quest catalog poza vertical slice.

### P2 — research packages
- [ ] culture research package dla każdego finalnego kontekstu kulturowego;
- [ ] domknięcie krytycznych źródeł panteonu;
- [ ] research lock material culture dla assetów, które rzeczywiście trafiają do produkcji;
- [ ] nazwa/identity lock `forest-guardian` albo jawne pozostawienie F;
- [ ] ogniki/błędne światła jako osobna karta zjawiska.

### P3 — content production
- [ ] side-quest production cards per region;
- [ ] regional encounter rosters/tables poza R0;
- [ ] regional vendors/services final pass;
- [ ] item/equipment/recipe catalogs dla pełnego scope;
- [ ] dialogue packages po zamknięciu rosterów.

### P4 — asset i budget lock
- [ ] asset lists per region z reuse/LOD/variant strategy;
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
3. Nowy dokument powstaje tylko, gdy nie istnieje już owner tego zakresu.
4. Zmiana central lore wymaga jawnego uzasadnienia i aktualizacji dokumentów zależnych.
5. Każdy production card wskazuje persistence, dependencies, fail-forward i minimalne QA tam, gdzie ma to zastosowanie.
6. Coverage jest aktualizowane po każdym większym pakiecie.

## Następny element

Najbliższy niezablokowany pakiet: **production NPC/companion roster**, potem **finalny bestiary roster**. Są potrzebne przed pełnymi regionalnymi asset/content listami i dialogami.