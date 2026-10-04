# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Próg swobodnej implementacji — stan 2026-10-04

Projekt ma pełny szkielet designu i author truth, production bibles R0–R6, production cards MQ00–MQ56 oraz 21/21 side-quest cards. Encountery, regionalne usługi, item/equipment/recipe i dialogue package contract mają implementation-level pass. `design/ProductionAssetManifests.md` dodaje konkretne stabilne rekordy integracyjne assetów R0–R6 i shared. `design/AnimationVfxAudioBudgetContract.md`, `design/StreamingMemoryBudgetContract.md` i `design/AiEncounterDensityBudgetContract.md` definiują planistyczne kontrakty kosztu, degradacji i telemetry. `design/ProductionEstimatesFramework.md` definiuje E0–E4, throughput baseline, estimate states i forecasting bez wymyślonych osobodni. `design/MeasurementPlaytestEvidence.md` definiuje wspólną provenance i lifecycle dowodu dla liczbowych locków. Implementacja systemów i content pipeline nie musi czekać na dalsze dopisywanie fabuły ani arbitralne limity.

Nie oznacza to production lock. Otwarte pozostają finalny balans, finalne line writing/VO/lokalizacja, exact historical-final locators i formy, final art/AVFX/audio IDs/warianty oraz targety performance i estymaty kalendarzowe wymagające pomiarów.

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
| Quest/framework/combat/progression | design/QuestDesign.md + design/CombatDesign.md + design/Progression.md + design/ItemEquipmentRecipeCatalog.md | implementation pass; final tuning otwarty |
| Economy/vendors | design/EconomyPass01.md + design/RegionalVendorsServices.md + design/ItemEquipmentRecipeCatalog.md | framework + R0–R6 service pass; ceny/restock pozostają lockiem |
| Encounter/AI density | design/EncounterDesign.md + design/RegionalEncounterRosters.md + design/AiEncounterDensityBudgetContract.md | framework + family pass + A0–A3/simulation/admission/telemetry; final density/CPU measurement-gated |
| NPC/dialog | design/NpcDialogueDesign.md + story/ProductionNpcRoster.md + dialogue/FullGameDialoguePackages.md | R0–R6/campaign package contract implementation-ready; final wording/VO/localization otwarte |
| Save/content/input/UI | design/SavePersistence.md + content/* + design/ControlsAndInput.md + ui/* | v0.1 implementation contracts |
| Day/night/weather | world/DayNightEvents.md + WeatherGameplay.md + content/TimeEventFormat.md | behavior + data contract v0.1 |
| Architektura/rendering | design/EngineArchitecture.md + technical/RenderingAndPlatform.md + design/StreamingMemoryBudgetContract.md | v0.1 + streaming/residency contract |
| Animation/VFX/audio budgets | design/AnimationVfxAudioBudgetContract.md + design/ProductionAssetManifests.md | planning v0.1; numeric ceilings otwarte |
| Streaming/memory budgets | design/StreamingMemoryBudgetContract.md + design/ProductionAssetManifests.md | planning v0.1; numeric RAM/VRAM/IO targets otwarte |
| AI/encounter budgets | design/AiEncounterDensityBudgetContract.md + design/RegionalEncounterRosters.md | planning v0.1; agent/CPU/query/density targets otwarte |
| Measurement/playtest evidence | design/MeasurementPlaytestEvidence.md + technical/TestingAndPerformance.md | evidence contract v0.1; stable IDs, build/scenario/hardware provenance, artifact/acceptance/retest lifecycle |
| Production estimates | design/ProductionEstimatesFramework.md + design/ContentProduction.md | framework v0.1: E0–E4, baseline/sample/evidence, estimate states i forecasting; velocity/capacity/osobodni/daty pozostają measurement-gated |
| Testy/logging/debug | technical/TestingAndPerformance.md + LoggingPolicy.md + DeveloperOverlay.md | v0.1; pomiary performance otwarte |
| Produkcja/release | design/ContentProduction.md + ReleaseCriteria.md + ScopeBoundaries.md + ProductionEstimatesFramework.md + MeasurementPlaytestEvidence.md | planning + evidence contract; release evidence i real throughput otwarte |
| Material culture research | research/material-culture/* + research/cultures/* | family-level production research PASS v0.1; exact locators per asset |
| Asset/content families full game | design/ProductionContentCatalog.md + design/RegionalContentAssetCatalog.md + design/ProductionAssetManifests.md | R0–R6 concrete stable manifest records v0.1; final forms/art/performance lock otwarte |

## Jawne otwarte decyzje

1. finalne liczby balansu: damage, economy, drop rates, reputation, evidence thresholds;
2. finalne line writing, VO, casting i lokalizacja;
3. finalne personalia/łączenie slotów NPC, appearance i killability windows;
4. finalne targety kontraktów wybranych side-questów oraz appearance/placement istot;
5. exact historical-final locators per asset, szczególnie costume/ornament/religion-material/naming i regionalne technology/art locki;
6. finalne modele/materials/textures, warianty i konkretne animation/audio/VFX records/art IDs;
7. item/equipment/recipe final stats/value/weight/durability, drop rates, quantities/craft time, botanika i culture-specific goods;
8. encounter/AI final weights, density, cooldowns, POI placement, group sizes, active-agent limits, update cadence, path/perception ceilings, tuning i rewards;
9. measured performance targets: AVFX ceilings, RAM/VRAM, streaming IO/prefetch/cell, AI CPU/query/agent budgets i wymagania sprzętowe;
10. production estimates: framework istnieje, ale velocity/capacity, czasy per domain/class, milestone ranges i release forecast wymagają rzeczywistych ukończonych próbek;
11. tuning pogody, traversal, ekonomii, AI i encounterów po playtestach;
12. nazwy robocze F oraz elementy research/art/playtest/performance lock.

Wspólny format dowodu dla pozycji 1, 7–11 istnieje w `MeasurementPlaytestEvidence.md`; nie oznacza to, że wyniki zostały już zmierzone.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie dopiero, gdy każdy system ma owner/spec, wszystkie główne questy mają karty, każdy region ma bible, każda finalna istota ma research card albo jawny status F, każda finalna kultura ma research package, zakończenia są rozpisane, side-content finalnego scope ma production cards, asset listy istnieją dla pełnego scope, targety performance są zmierzone i release criteria są spełnione.

### Aktualna ocena

**Próg wstępnej kompletności wystarczającej do swobodnego programowania jest osiągnięty.** P1–P4 mają implementation/planning-level pass. P5 ma teraz wspólny evidence contract, dzięki czemu kolejne realne pomiary i playtesty mogą zamykać locki bez zmiany semantyki dokumentacji. Research-sensitive szczegóły oraz liczby wymagające telemetry/playtestów pozostają jawnie zablokowane zamiast być zgadywane. Nie deklarujemy jeszcze pełnej dokumentacji produkcyjnej.

## Kolejny priorytet

1. P5 scenario manifests dla combat/economy/progression i evidence/reputation;
2. realne balance/playtest/performance locks z evidence;
3. measured release evidence i wymagania sprzętowe.