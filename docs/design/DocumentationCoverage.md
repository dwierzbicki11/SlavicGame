# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Próg swobodnej implementacji — stan 2026-10-04

Projekt ma pełny szkielet designu i author truth, production bibles R0–R6, production cards MQ00–MQ56 oraz 21/21 side-quest cards. Encountery, regionalne usługi, item/equipment/recipe i dialogue package contract mają implementation-level pass. Asset manifests oraz kontrakty AVFX, streaming/memory i AI/encounter definiują stabilne punkty integracji, degradację i telemetry. Production estimates mają measurement-gated framework. `MeasurementPlaytestEvidence.md` definiuje wspólną provenance/lifecycle dowodu. Reprodukowalne P5 scenariusze istnieją dla combat, economy/progression, evidence/reputation, traversal/weather/day-night oraz performance/release bez fabrykowania wyników. Implementacja systemów i content pipeline nie musi czekać na dalsze dopisywanie fabuły ani arbitralne limity.

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
| Quest/framework/combat/progression | design/QuestDesign.md + design/CombatDesign.md + design/Progression.md + design/ItemEquipmentRecipeCatalog.md + design/BalancePlaytestScenarioManifests.md | implementation pass + reproducible P5 scenarios; final tuning open |
| Economy/vendors | design/EconomyPass01.md + design/RegionalVendorsServices.md + design/ItemEquipmentRecipeCatalog.md + design/BalancePlaytestScenarioManifests.md | framework + R0–R6 service pass + P5 scenarios; prices/restock remain playtest lock |
| Evidence/reputation | relevant quest/system specs + design/BalancePlaytestScenarioManifests.md | valid-path/fail-forward/service-gate/conflicting-action scenarios v0.1; final thresholds open |
| Encounter/AI density | design/EncounterDesign.md + design/RegionalEncounterRosters.md + design/AiEncounterDensityBudgetContract.md | framework + family pass + A0–A3/simulation/admission/telemetry; final density/CPU measurement-gated |
| NPC/dialog | design/NpcDialogueDesign.md + story/ProductionNpcRoster.md + dialogue/FullGameDialoguePackages.md | R0–R6/campaign package contract implementation-ready; final wording/VO/localization open |
| Save/content/input/UI | design/SavePersistence.md + content/* + design/ControlsAndInput.md + ui/* | v0.1 implementation contracts |
| Day/night/weather/traversal | world/DayNightEvents.md + WeatherGameplay.md + content/TimeEventFormat.md + design/TraversalWeatherDayNightScenarioManifests.md | behavior/data contract + reproducible P5 scenarios; final tuning open |
| Architektura/rendering | design/EngineArchitecture.md + technical/RenderingAndPlatform.md + design/StreamingMemoryBudgetContract.md | v0.1 + streaming/residency contract |
| Animation/VFX/audio budgets | design/AnimationVfxAudioBudgetContract.md + design/ProductionAssetManifests.md | planning v0.1; numeric ceilings open |
| Streaming/memory budgets | design/StreamingMemoryBudgetContract.md + design/ProductionAssetManifests.md | planning v0.1; numeric RAM/VRAM/IO targets open |
| AI/encounter budgets | design/AiEncounterDensityBudgetContract.md + design/RegionalEncounterRosters.md | planning v0.1; agent/CPU/query/density targets open |
| Measurement/playtest evidence | design/MeasurementPlaytestEvidence.md + design/BalancePlaytestScenarioManifests.md + design/TraversalWeatherDayNightScenarioManifests.md + design/PerformanceReleaseMeasurementManifests.md + technical/TestingAndPerformance.md | evidence contract + gameplay/world/performance/release manifests v0.1; real evidence still required |
| Production estimates | design/ProductionEstimatesFramework.md + design/ContentProduction.md | framework v0.1; velocity/capacity/person-days/dates measurement-gated |
| Testy/logging/debug | technical/TestingAndPerformance.md + LoggingPolicy.md + DeveloperOverlay.md + design/PerformanceReleaseMeasurementManifests.md | v0.1 + reproducible profiling manifests; measured targets open |
| Produkcja/release | design/ContentProduction.md + ReleaseCriteria.md + ScopeBoundaries.md + ProductionEstimatesFramework.md + MeasurementPlaytestEvidence.md + PerformanceReleaseMeasurementManifests.md | planning + evidence/manifests; release evidence and real throughput open |
| Material culture research | research/material-culture/* + research/cultures/* | family-level production research PASS v0.1; exact locators per asset |
| Asset/content families full game | design/ProductionContentCatalog.md + design/RegionalContentAssetCatalog.md + design/ProductionAssetManifests.md | R0–R6 concrete stable manifest records v0.1; final forms/art/performance lock open |

## Jawne otwarte decyzje

1. finalne liczby balansu: damage, economy, drop rates, reputation, evidence thresholds — scenariusze testowe istnieją, wyniki nie;
2. finalne line writing, VO, casting i lokalizacja;
3. finalne personalia/łączenie slotów NPC, appearance i killability windows;
4. finalne targety kontraktów wybranych side-questów oraz appearance/placement istot;
5. exact historical-final locators per asset, szczególnie costume/ornament/religion-material/naming i regionalne technology/art locki;
6. finalne modele/materials/textures, warianty i konkretne animation/audio/VFX records/art IDs;
7. item/equipment/recipe final stats/value/weight/durability, drop rates, quantities/craft time, botanika i culture-specific goods;
8. encounter/AI final weights, density, cooldowns, POI placement, group sizes, active-agent limits, update cadence, path/perception ceilings, tuning i rewards;
9. measured performance targets: frame-time, AVFX ceilings, RAM/VRAM, streaming IO/prefetch/cell, AI CPU/query/agent budgets i wymagania sprzętowe — manifesty pomiarowe istnieją, captures nie;
10. production estimates: velocity/capacity, czasy per domain/class, milestone ranges i release forecast wymagają rzeczywistych ukończonych próbek;
11. finalne tuning values pogody/traversal/day-night — P5 scenario manifests istnieją, wyniki nie;
12. release evidence dla konkretnego candidate SHA, w tym hardware qualification, save/fail-forward i long-session stability;
13. nazwy robocze F oraz elementy research/art/playtest/performance lock.

Wspólny format dowodu istnieje w `MeasurementPlaytestEvidence.md`; reprodukowalne gameplay/world scenarios istnieją w `BalancePlaytestScenarioManifests.md` i `TraversalWeatherDayNightScenarioManifests.md`, a performance/release w `PerformanceReleaseMeasurementManifests.md`. Nie oznacza to, że wyniki zostały już zmierzone.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie dopiero, gdy każdy system ma owner/spec, wszystkie główne questy mają karty, każdy region ma bible, każda finalna istota ma research card albo jawny status F, każda finalna kultura ma research package, zakończenia są rozpisane, side-content finalnego scope ma production cards, asset listy istnieją dla pełnego scope, targety performance są zmierzone i release criteria są spełnione.

### Aktualna ocena

**Próg wstępnej kompletności wystarczającej do swobodnego programowania jest osiągnięty.** P1–P4 mają implementation/planning-level pass. P5 ma evidence contract oraz reprodukowalne scenario manifests dla głównych domen gameplay/world i performance/release. Dokumentacyjna specyfikacja pomiarów jest gotowa; dalszy production lock zależy już od realnych uruchomień, playtestów i profiler captures. Research-sensitive szczegóły i liczby wymagające telemetry/playtestów pozostają jawnie zablokowane zamiast być zgadywane. Nie deklarujemy jeszcze pełnej dokumentacji produkcyjnej.

## Kolejny priorytet

1. wykonać realne balance/traversal/performance scenario runs i zapisać MEASURED/CANDIDATE evidence;
2. na ich podstawie zamknąć measured CPU/GPU/RAM/VRAM/streaming/AI/AVFX targets oraz gameplay thresholds;
3. zakwalifikować minimal/recommended hardware i zebrać release evidence dla konkretnego candidate SHA.