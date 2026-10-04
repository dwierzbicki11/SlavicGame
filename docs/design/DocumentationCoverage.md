# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Próg swobodnej implementacji — stan 2026-10-04

Projekt ma pełny szkielet designu i author truth, production bibles R0–R6, production cards MQ00–MQ56 oraz 21/21 side-quest cards. Encountery, regionalne usługi, item/equipment/recipe i dialogue package contract mają implementation-level pass. `design/ProductionAssetManifests.md` dodaje konkretne stabilne rekordy integracyjne assetów R0–R6 i shared. `design/AnimationVfxAudioBudgetContract.md` definiuje wspólne klasy kosztu/priorytetu i degradation, `design/StreamingMemoryBudgetContract.md` definiuje residency M0–M4 i pressure/streaming contract, a `design/AiEncounterDensityBudgetContract.md` definiuje klasy A0–A3, strefy symulacji, encounter admission, pressure degradation i telemetry. Implementacja systemów, content pipeline, streamera, AI i integracja placeholderów nie musi czekać na dalsze dopisywanie fabuły ani na wymyślone limity wydajności.

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
| Encounter/AI density | design/EncounterDesign.md + design/RegionalEncounterRosters.md + design/AiEncounterDensityBudgetContract.md + content format | framework + R1–R6 family-level pass + A0–A3/simulation/admission/pressure/telemetry contract; final density/CPU/query ceilings measurement-gated |
| Alchemy/magia | alchemy/RecipesV01.md + design/ItemEquipmentRecipeCatalog.md + magic/* | recipe implementation catalog v0.1; warunkowe receptury mają jawne gates; tuning/research otwarte |
| NPC/dialog | design/NpcDialogueDesign.md + story/ProductionNpcRoster.md + dialogue/FullGameDialoguePackages.md + dialogue/* | R0–R6/campaign package contract implementation-ready; final wording/VO/localization otwarte |
| Inventory | design/InventoryEconomy.md + design/ItemEquipmentRecipeCatalog.md | v0.1 + stable item family catalog |
| Save/persistence | design/SavePersistence.md + design/StreamingMemoryBudgetContract.md | v0.1; logic state oddzielony od residency/cell lifetime |
| Input/settings | design/ControlsAndInput.md + InputActionMap.md + SettingsMatrix.md | v0.1 |
| UI/UX | ui/* + design/UXAccessibility.md + SaveSlotUX.md | v0.1 |
| Day/night/weather | world/DayNightEvents.md + WeatherGameplay.md + content/TimeEventFormat.md | behavior + data contract v0.1 |
| Content IDs/formats | content/IdConventions.md + content/*Template.md + content/*Format.md | v0.1 |
| Architektura/rendering | design/EngineArchitecture.md + technical/RenderingAndPlatform.md + design/StreamingMemoryBudgetContract.md | v0.1 + streaming/residency contract |
| Animation/VFX/audio budgets | design/AnimationVfxAudioBudgetContract.md + design/ProductionAssetManifests.md | production planning v0.1: C0–C3, priority/reuse/fallback/degradation + measurement gate; numeric ceilings otwarte |
| Streaming/memory budgets | design/StreamingMemoryBudgetContract.md + design/ProductionAssetManifests.md | production planning v0.1: M0–M4 residency, pressure states, lifecycle, R0–R6 scenarios, telemetry; numeric RAM/VRAM/IO targets otwarte |
| AI/encounter budgets | design/AiEncounterDensityBudgetContract.md + design/RegionalEncounterRosters.md | production planning v0.1: A0–A3, simulation zones, admission/pressure, R0–R6 scenarios i telemetry; agent count/CPU/query/density targets otwarte |
| Testy/logging/debug | technical/TestingAndPerformance.md + LoggingPolicy.md + DeveloperOverlay.md + design/StreamingMemoryBudgetContract.md + design/AiEncounterDensityBudgetContract.md | v0.1; pressure-state QA i measurement schema zdefiniowane; pomiary performance otwarte |
| Produkcja/release | design/ContentProduction.md + ReleaseCriteria.md + ScopeBoundaries.md | first pass; estimates wymagają rzeczywistej throughput baseline |
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
8. encounter/AI: finalne weights, density, cooldown durations, POI placement, group sizes, active-agent limits, update cadence, path/perception ceilings, combat tuning i rewards;
9. measured performance targets: numeric animation/VFX/audio ceilings, RAM/VRAM ceilings, streaming IO/prefetch/cell targets, AI CPU/query/agent budgets i wymagania sprzętowe; kontrakty telemetry/degradation są już zdefiniowane;
10. production estimates: osobodni/velocity/milestones dopiero po zebraniu rzeczywistej przepustowości zespołu;
11. tuning pogody, traversal, ekonomii, AI i encounterów po playtestach;
12. nazwy robocze F oraz elementy research/art/playtest/performance lock.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie dopiero, gdy każdy system ma owner/spec, wszystkie główne questy mają karty, każdy region ma bible, każda finalna istota ma research card albo jawny status F, każda finalna kultura ma research package, zakończenia są rozpisane, side-content finalnego scope ma production cards, asset listy istnieją dla pełnego scope, targety performance są zmierzone i release criteria są spełnione.

### Aktualna ocena

**Próg wstępnej kompletności wystarczającej do swobodnego programowania jest osiągnięty.** R0–R6 mają bibles, quest/content/system coverage, konkretne production asset manifest records oraz planistyczne kontrakty AVFX/audio, streaming/memory i AI/encounter density. Research-sensitive szczegóły oraz liczby wymagające telemetry pozostają jawnie zablokowane zamiast być zgadywane. Nie deklarujemy jeszcze pełnej dokumentacji produkcyjnej.

## Kolejny priorytet

1. production estimates framework oparty na rzeczywistej throughput baseline, bez wymyślonych terminów/osobodni;
2. później balance/playtest/performance locks;
3. measured release evidence i wymagania sprzętowe.