# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Próg swobodnej implementacji — stan 2026-10-02

Projekt ma pełny szkielet designu i author truth oraz production bibles R0–R6. Main quest ma teraz production cards dla całego zakresu MQ00–MQ56: Acts 0–II w `quests/MainQuestCardsActs0To2.md`, Act III w `quests/MainQuestCardsAct3.md`, Act IV w `quests/MainQuestCardsAct4.md`, Act V w `quests/MainQuestCardsAct5.md`. Oznacza to, że implementacja quest state machine, persistence, objective graph i regionalnego flow nie musi czekać na dalsze dopisywanie fabuły.

Nie oznacza to production lock. Otwarte pozostają finalny balans, finalne dialogi/VO, część rosterów i assetów, pełne culture/research packages, finalne karty istot oraz targety performance wymagające pomiarów.

| Obszar | Główny dokument | Stan |
|---|---|---|
| Wizja i pełny obraz | README.md + FullGameOverview.md | v0.2 |
| Świat/kosmologia | world/WorldBible.md + world/Cosmology.md + world/FourthSphereTruth.md | implementacyjny first pass + author truth |
| Historia/central lore | story/MainStory.md + story/CrisisTruth.md + story/MysteryChronology.md | v0.2 |
| Main quest | story/MainQuestSkeleton.md + quests/MainQuestCardsActs0To2.md + quests/MainQuestCardsAct3.md + quests/MainQuestCardsAct4.md + quests/MainQuestCardsAct5.md | MQ00–MQ56 production cards v0.1 |
| Regiony | world/RegionBibleIndex.md + R0–R6 bibles | 7/7 production bibles v0.1 |
| Panteon/religia | pantheon/* + research/pantheon/* | first pass; krytyczne źródła nadal rozwijane |
| Bestiariusz | bestiary/BestiaryBible.md + research/bestiary/* | framework + research pass; finalny roster/karty niepełne |
| Kultury | world/Cultures.md + world/MacroCultures.md | framework/macro v0.1; pełne packages niegotowe |
| Quest framework | design/QuestDesign.md | v0.1 |
| Vertical slice quest | quests/LightOverSwamp*.md | pełny pakiet v0.1 |
| Combat | design/CombatDesign.md + MeleeCombat.md + BowCombat.md | v0.1 |
| Status/equipment/progression | design/StatusEffects.md + EquipmentSystem.md + Progression.md | v0.1 |
| Economy/vendors | design/EconomyPass01.md | v0.1 |
| Tracking | design/TrackingSystem.md | v0.1 |
| Encounter | design/EncounterDesign.md + content format | v0.1 |
| Alchemy/magia | alchemy/RecipesV01.md + magic/* | v0.1 |
| NPC/dialog | design/NpcDialogueDesign.md + dialogue/* | system + vertical slice v0.1; full roster otwarty |
| Inventory | design/InventoryEconomy.md | v0.1 |
| Save/persistence | design/SavePersistence.md | v0.1 |
| Input/settings | design/ControlsAndInput.md + InputActionMap.md + SettingsMatrix.md | v0.1 |
| UI/UX | ui/* + design/UXAccessibility.md + SaveSlotUX.md | v0.1 |
| Day/night/weather | world/DayNightEvents.md + WeatherGameplay.md + content/TimeEventFormat.md | behavior + data contract v0.1 |
| Content IDs/formats | content/IdConventions.md + content/*Template.md + content/*Format.md | v0.1 |
| Architektura/rendering | design/EngineArchitecture.md + technical/RenderingAndPlatform.md | v0.1 |
| Testy/logging/debug | technical/TestingAndPerformance.md + LoggingPolicy.md + DeveloperOverlay.md | v0.1; pomiary performance otwarte |
| Produkcja/release | design/ContentProduction.md + ReleaseCriteria.md + ScopeBoundaries.md | first pass |
| Material culture research | research/material-culture/* | first pass; production lock niepełny |
| Asset list vertical slice | design/VerticalSliceAssetList.md | v0.1 |
| Asset list full game | design/ProductionContentCatalog.md + region bibles | struktura istnieje; finalne ilości/listy otwarte |

## Jawne otwarte decyzje

Te punkty są celowo otwarte i nie wolno ich „zamykać” przez zgadywanie:

1. finalne liczby balansu: damage, economy, drop rates, reputation, evidence thresholds;
2. finalne dialogi, VO i lokalizacja;
3. finalny roster NPC/companionów oraz dokładne rozmieszczenie contentu w regionach;
4. finalny roster bestiariusza i research card każdej użytej istoty;
5. pełne culture research packages oraz research-lock szczegółów materialnych/religijnych;
6. finalne asset listy poza vertical slice i budżety produkcyjne;
7. art/level locks: layouty, liczby encounterów, landmarków, osad i side questów;
8. measured performance targets, streaming/VFX/shadow/AI budgets i wymagania sprzętowe;
9. tuning pogody, traversal, ekonomii, AI i encounterów po playtestach;
10. nazwy robocze oznaczone F oraz elementy jawnie opisane w dokumentach jako research/art/playtest/performance lock.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie dopiero, gdy:
- każdy system ma owner/spec;
- wszystkie główne questy mają karty;
- każdy region ma bible;
- każda finalna istota ma research card;
- każda kultura ma research package;
- zakończenia są rozpisane;
- asset listy istnieją dla pełnego scope;
- targety performance są zmierzone;
- release criteria są spełnione.

### Aktualna ocena

**Próg wstępnej kompletności wystarczającej do swobodnego programowania jest osiągnięty dla głównych systemów, regionów, central lore i main questu.** Dalsza dokumentacja ma charakter produkcyjnego uszczegóławiania, research locków, content locków, balansu i pomiarów. Nie deklarujemy jeszcze pełnej dokumentacji produkcyjnej.

## Kolejny priorytet

1. finalne research cards/roster bestiariusza;
2. culture research packages;
3. production NPC/companion roster;
4. full-game asset/content lists per region;
5. side-quest packages poza vertical slice;
6. później balance/playtest/performance locks.