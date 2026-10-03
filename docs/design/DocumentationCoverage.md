# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Próg swobodnej implementacji — stan 2026-10-03

Projekt ma pełny szkielet designu i author truth, production bibles R0–R6 oraz production cards MQ00–MQ56. `story/ProductionNpcRoster.md` definiuje NPC lifecycle/persistence, `bestiary/ProductionBestiaryRoster.md` roster istot 1.0, `design/RegionalContentAssetCatalog.md` rodziny content/assets R0–R6, a `quests/SideQuestCardsR0.md`–`SideQuestCardsR6.md` zamykają implementacyjny first pass 21/21 side-questów. `design/RegionalEncounterRosters.md` zamyka family-level encounter pass R1–R6. Implementacja systemów i regionalnego content pipeline nie musi czekać na dalsze dopisywanie fabuły.

Nie oznacza to production lock. Otwarte pozostają finalny balans, dialogi/VO, exact historical-final locators dla konkretnych assetów, konkretne asset manifests i targety performance wymagające pomiarów.

| Obszar | Główny dokument | Stan |
|---|---|---|
| Wizja i pełny obraz | README.md + FullGameOverview.md | v0.2 |
| Świat/kosmologia | world/WorldBible.md + world/Cosmology.md + world/FourthSphereTruth.md | implementacyjny first pass + author truth |
| Historia/central lore | story/MainStory.md + story/CrisisTruth.md + story/MysteryChronology.md | v0.2 |
| Main quest | story/MainQuestSkeleton.md + quests/MainQuestCardsActs0To2.md + quests/MainQuestCardsAct3.md + quests/MainQuestCardsAct4.md + quests/MainQuestCardsAct5.md | MQ00–MQ56 production cards v0.1 |
| Side quest | quests/SideQuestCatalog.md + quests/SideQuestCardsR0.md–SideQuestCardsR6.md + QuestDesign.md | R0–R6, 21/21 production cards v0.1 |
| Regiony | world/RegionBibleIndex.md + R0–R6 bibles | 7/7 production bibles v0.1 |
| Panteon/religia | pantheon/* + research/pantheon/* | critical source-policy PASS v0.1; dalsze pełne lektury/locators nie blokują implementacji |
| Bestiariusz | bestiary/BestiaryBible.md + bestiary/ProductionBestiaryRoster.md + research/bestiary/ResearchCardIndex.md | scope 1.0 v0.1; południca region-fit PASS; forest-guardian CLOSED/F |
| Kultury | world/Cultures.md + world/MacroCultures.md + research/cultures/* | framework v0.1; R0–R5 evidence packages v0.1; exact art/technology locators otwarte per asset |
| Quest framework | design/QuestDesign.md | v0.1 |
| Combat | design/CombatDesign.md + MeleeCombat.md + BowCombat.md | v0.1 |
| Status/equipment/progression | design/StatusEffects.md + EquipmentSystem.md + Progression.md | v0.1 |
| Economy/vendors | design/EconomyPass01.md | v0.1; regional vendors/services final pass otwarty |
| Tracking | design/TrackingSystem.md | v0.1 |
| Encounter | design/EncounterDesign.md + design/RegionalEncounterRosters.md + content format | framework v0.1 + R1–R6 family-level implementation pass |
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
| Material culture research | research/material-culture/* + research/cultures/CULT_R0_ZARNOWIEC.md–CULT_R5_AREL.md | family-level production research PASS v0.1; L0–L3 ledger; exact locators per asset |
| Asset/content families full game | design/ProductionContentCatalog.md + design/RegionalContentAssetCatalog.md + region bibles | R0–R6 planning catalog v0.1; finalne manifesty/art lock otwarte |

## Jawne otwarte decyzje

1. finalne liczby balansu: damage, economy, drop rates, reputation, evidence thresholds;
2. finalne dialogi, VO i lokalizacja;
3. finalne personalia/łączenie slotów NPC, appearance i killability windows;
4. finalne targety kontraktów `SQ_R1_02`, `SQ_R2_02`, `SQ_R3_03` i `SQ_R4_02`; appearance/placement istot pozostaje art/data lockiem;
5. exact historical-final locators per asset, szczególnie costume/ornament/religion-material/naming i regionalne technology/art locki;
6. konkretne finalne modele/materials/animations/audio/VFX i manifesty poza vertical slice;
7. regional vendors/services oraz pełne item/equipment/recipe catalogs;
8. encounter: finalne weights, density, cooldown durations, POI placement, combat tuning i rewards; R1–R6 family tables są już zdefiniowane;
9. measured performance targets, streaming/VFX/shadow/AI budgets i wymagania sprzętowe;
10. tuning pogody, traversal, ekonomii, AI i encounterów po playtestach;
11. nazwy robocze F oraz elementy research/art/playtest/performance lock.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie dopiero, gdy każdy system ma owner/spec, wszystkie główne questy mają karty, każdy region ma bible, każda finalna istota ma research card albo jawny status F, każda finalna kultura ma research package, zakończenia są rozpisane, side-content finalnego scope ma production cards, asset listy istnieją dla pełnego scope, targety performance są zmierzone i release criteria są spełnione.

### Aktualna ocena

**Próg wstępnej kompletności wystarczającej do swobodnego programowania jest osiągnięty.** R0–R6 mają region bibles, 21/21 side-quest cards oraz family-level encounter coverage. R1–R6 mają teraz stabilne rodziny encounterów z filtrami, persistence, resolution paths i QA bez wymyślania niezmierzonych wag/gęstości. Culture/material-culture/bestiary/pantheon research ma jawne production gates. Nie deklarujemy jeszcze pełnej dokumentacji produkcyjnej.

## Kolejny priorytet

1. regional vendors/services final pass;
2. item/equipment/recipe catalogs dla pełnego scope;
3. dialogue packages po zamknięciu rosterów;
4. konkretne asset manifests po art/research lockach;
5. później balance/playtest/performance locks.
