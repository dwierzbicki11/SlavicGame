# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Próg swobodnej implementacji — stan 2026-10-03

Projekt ma pełny szkielet designu i author truth, production bibles R0–R6 oraz production cards MQ00–MQ56. `story/ProductionNpcRoster.md` definiuje NPC lifecycle/persistence, `bestiary/ProductionBestiaryRoster.md` roster istot 1.0, `research/bestiary/ResearchCardIndex.md` research ownerów, `design/RegionalContentAssetCatalog.md` rodziny content/assets R0–R6, a `quests/SideQuestCatalog.md` zamyka planistyczny scope side-content dla wszystkich regionów. `quests/SideQuestCardsR0.md`, `SideQuestCardsR1.md` i `SideQuestCardsR2.md` zamykają implementacyjny first pass dziewięciu slotów R0–R2. Implementacja systemów i regionalnego content pipeline nie musi czekać na dalsze dopisywanie fabuły.

Nie oznacza to production lock. Otwarte pozostają finalny balans, dialogi/VO, source-strength/identity locki, culture packages, konkretne asset manifests, production cards side questów R3–R6 i targety performance wymagające pomiarów.

| Obszar | Główny dokument | Stan |
|---|---|---|
| Wizja i pełny obraz | README.md + FullGameOverview.md | v0.2 |
| Świat/kosmologia | world/WorldBible.md + world/Cosmology.md + world/FourthSphereTruth.md | implementacyjny first pass + author truth |
| Historia/central lore | story/MainStory.md + story/CrisisTruth.md + story/MysteryChronology.md | v0.2 |
| Main quest | story/MainQuestSkeleton.md + quests/MainQuestCardsActs0To2.md + quests/MainQuestCardsAct3.md + quests/MainQuestCardsAct4.md + quests/MainQuestCardsAct5.md | MQ00–MQ56 production cards v0.1 |
| Side quest | quests/SideQuestCatalog.md + quests/SideQuestCardsR0.md + SideQuestCardsR1.md + SideQuestCardsR2.md + QuestDesign.md | R0–R2 production cards v0.1; R3–R6 planning catalog v0.1 |
| Regiony | world/RegionBibleIndex.md + R0–R6 bibles | 7/7 production bibles v0.1 |
| Panteon/religia | pantheon/* + research/pantheon/* | first pass; krytyczne źródła nadal rozwijane |
| Bestiariusz | bestiary/BestiaryBible.md + bestiary/ProductionBestiaryRoster.md + research/bestiary/ResearchCardIndex.md | scope 1.0 v0.1; południca i forest-guardian mają jawne locki |
| Kultury | world/Cultures.md + world/MacroCultures.md | framework/macro v0.1; pełne packages niegotowe |
| Quest framework | design/QuestDesign.md | v0.1 |
| Vertical slice quest | quests/LightOverSwamp*.md | pełny pakiet v0.1 |
| Combat | design/CombatDesign.md + MeleeCombat.md + BowCombat.md | v0.1 |
| Status/equipment/progression | design/StatusEffects.md + EquipmentSystem.md + Progression.md | v0.1 |
| Economy/vendors | design/EconomyPass01.md | v0.1 |
| Tracking | design/TrackingSystem.md | v0.1 |
| Encounter | design/EncounterDesign.md + content format | v0.1 |
| Alchemy/magia | alchemy/RecipesV01.md + magic/* | v0.1 |
| NPC/dialog | design/NpcDialogueDesign.md + story/ProductionNpcRoster.md + dialogue/* | production roster v0.1; final dialogue/VO otwarte |
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
| Asset/content families full game | design/ProductionContentCatalog.md + design/RegionalContentAssetCatalog.md + region bibles | R0–R6 planning catalog v0.1; finalne manifesty/art lock otwarte |

## Jawne otwarte decyzje

1. finalne liczby balansu: damage, economy, drop rates, reputation, evidence thresholds;
2. finalne dialogi, VO i lokalizacja;
3. finalne personalia/łączenie slotów NPC, appearance i killability windows;
4. bestiariusz: source-strength/region-fit południcy i identity lock `forest-guardian`, plus finalne targety kontraktów `SQ_R1_02` i `SQ_R2_02`;
5. pełne culture research packages i research-lock szczegółów materialnych/religijnych;
6. konkretne finalne modele/materials/animations/audio/VFX i manifesty poza vertical slice;
7. side-quest production cards R3–R6, dalsze creature assignments, encounter/POI placement oraz decyzje o scaleniu slotów po playtestach;
8. measured performance targets, streaming/VFX/shadow/AI budgets i wymagania sprzętowe;
9. tuning pogody, traversal, ekonomii, AI i encounterów po playtestach;
10. nazwy robocze F oraz elementy research/art/playtest/performance lock.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie dopiero, gdy każdy system ma owner/spec, wszystkie główne questy mają karty, każdy region ma bible, każda finalna istota ma research card albo jawny status F, każda finalna kultura ma research package, zakończenia są rozpisane, side-content przeznaczony do finalnego scope ma production cards, asset listy istnieją dla pełnego scope, targety performance są zmierzone i release criteria są spełnione.

### Aktualna ocena

**Próg wstępnej kompletności wystarczającej do swobodnego programowania jest osiągnięty.** Regionalny scope rodzin content/assets i side-content jest zdefiniowany; R0–R2 mają implementacyjny first pass side-questów. Nie deklarujemy jeszcze pełnej dokumentacji produkcyjnej.

## Kolejny priorytet

1. side-quest production cards R4, następnie R3/R5/R6 bez przekraczania research locków;
2. culture research packages;
3. bestiary source/identity locki;
4. regional encounter/vendor/item catalogs;
5. konkretne asset manifests po art/research lockach;
6. później balance/playtest/performance locks.