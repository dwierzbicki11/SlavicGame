# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Próg swobodnej implementacji — stan 2026-10-04

Projekt ma pełny szkielet designu i author truth, production bibles R0–R6, production cards MQ00–MQ56 oraz 21/21 side-quest cards. Encountery, regionalne usługi, item/equipment/recipe i dialogue package contract mają implementation-level pass. `design/ProductionAssetManifests.md` dodaje konkretne stabilne rekordy integracyjne assetów R0–R6 i shared. `design/AnimationVfxAudioBudgetContract.md` dodaje wspólne klasy kosztu/priorytetu, reuse, fallback/degradation i measurement gate dla animation/VFX/audio bez udawania zmierzonych limitów. Implementacja systemów, content pipeline i integracja placeholderów nie musi czekać na dalsze dopisywanie fabuły.

Nie oznacza to production lock. Otwarte pozostają finalny balans, finalne line writing/VO/lokalizacja, exact historical-final locators i formy, final art/AVFX/audio IDs/warianty oraz targety performance wymagające pomiarów.

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
| Status/equipment/progression | design/StatusEffects.md + EquipmentSystem.md + Progression.md + ItemEquipmentRecipeCatalog.md | system v0.1 + full-scope stable item/equipment families; final stats/durability/weight otwarte |
| Economy/vendors | design/EconomyPass01.md + design/RegionalVendorsServices.md + design/ItemEquipmentRecipeCatalog.md | framework + R0–R6 service pass + stock/item families; ceny/restock pozostają lockiem |
| Tracking | design/TrackingSystem.md | v0.1 |
| Encounter | design/EncounterDesign.md + design/RegionalEncounterRosters.md + content format | framework v0.1 + R1–R6 family-level implementation pass |
| Alchemy/magia | alchemy/RecipesV01.md + design/ItemEquipmentRecipeCatalog.md + magic/* | recipe implementation catalog v0.1; warunkowe receptury mają jawne gates; tuning/research otwarte |
| NPC/dialog | design/NpcDialogueDesign.md + story/ProductionNpcRoster.md + dialogue/FullGameDialoguePackages.md + dialogue/* | R0–R6/campaign package contract implementation-ready; final wording/VO/localization otwarte |
| Inventory | design/InventoryEconomy.md + design/ItemEquipmentRecipeCatalog.md | v0.1 + stable item family catalog |
| Save/persistence | design/SavePersistence.md | v0.1 |
| Input/settings | design/ControlsAndInput.md + InputActionMap.md + SettingsMatrix.md | v0.1 |
| UI/UX | ui/* + design/UXAccessibility.md + SaveSlotUX.md | v0.1 |
| Day/night/weather | world/DayNightEvents.md + WeatherGameplay.md + content/TimeEventFormat.md | behavior + data contract v0.1 |
| Content IDs/formats | content/IdConventions.md + content/*Template.md + content/*Format.md | v0.1 |
| Architektura/rendering | design/EngineArchitecture.md + technical/RenderingAndPlatform.md | v0.1 |
| Animation/VFX/audio budgets | design/AnimationVfxAudioBudgetContract.md + design/ProductionAssetManifests.md | production planning v0.1: C0–C3, priority/reuse/fallback/degradation + measurement gate; numeric ceilings otwarte |
| Testy/logging/debug | technical/TestingAndPerformance.md + LoggingPolicy.md + DeveloperOverlay.md | v0.1; pomiary performance otwarte |
| Produkcja/release | design/ContentProduction.md + ReleaseCriteria.md + ScopeBoundaries.md | first pass |
| Material culture research | research/material-culture/* + research/cultures/CULT_R0_ZARNOWIEC.md–CULT_R5_AREL.md | family-level production research PASS v0.1; L0–L3 ledger; exact locators per asset |
| Asset/content families full game | design/ProductionContentCatalog.md + design/RegionalContentAssetCatalog.md + design/ProductionAssetManifests.md | R0–R6 planning + concrete stable manifest records v0.1; final forms/IDs/art/performance lock otwarte |

## Jawne otwarte decyzje

1. finalne liczby balansu: damage, economy, drop rates, reputation, evidence thresholds;
2. finalne line writing, VO, casting i lokalizacja;
3. finalne personalia/łączenie slotów NPC, appearance i killability windows;
4. finalne targety kontraktów wybranych side-questów oraz appearance/placement istot;
5. exact historical-final locators per asset, szczególnie costume/ornament/religion-material/naming i regionalne technology/art locki;
6. finalne modele/materials/textures, warianty i konkretne animation/audio/VFX records/art IDs; stabilne rodziny/integration IDs i AVFX budget contract są już zdefiniowane;
7. item/equipment/recipe: final stats/value/weight/durability, drop rates, recipe quantities/craft time, botanika i culture-specific goods;
8. encounter: finalne weights, density, cooldown durations, POI placement, combat tuning i rewards;
9. measured performance targets: numeric animation/VFX/audio ceilings, streaming/memory/shadow/AI budgets i wymagania sprzętowe;
10. tuning pogody, traversal, ekonomii, AI i encounterów po playtestach;
11. nazwy robocze F oraz elementy research/art/playtest/performance lock.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie dopiero, gdy każdy system ma owner/spec, wszystkie główne questy mają karty, każdy region ma bible, każda finalna istota ma research card albo jawny status F, każda finalna kultura ma research package, zakończenia są rozpisane, side-content finalnego scope ma production cards, asset listy istnieją dla pełnego scope, targety performance są zmierzone i release criteria są spełnione.

### Aktualna ocena

**Próg wstępnej kompletności wystarczającej do swobodnego programowania jest osiągnięty.** R0–R6 mają bibles, quest/content/system coverage, konkretne production asset manifest records i AVFX/audio planning contract. Research-sensitive szczegóły oraz liczby wymagające telemetry pozostają jawnie zablokowane zamiast być zgadywane. Nie deklarujemy jeszcze pełnej dokumentacji produkcyjnej.

## Kolejny priorytet

1. streaming/memory budget contract;
2. AI/encounter density budget contract;
3. production estimates po poznaniu faktycznej przepustowości zespołu;
4. później balance/playtest/performance locks i measured release evidence.