# Pokrycie dokumentacji

Stan roboczy v0.1. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

| Obszar | Główny dokument | Stan |
|---|---|---|
| Wizja | README.md | v0.1 |
| Świat | world/WorldBible.md | v0.1 |
| Kosmologia | world/Cosmology.md | v0.1 |
| Panteon | pantheon/PantheonBible.md | katalog v0.1; research otwarty |
| Źródła | pantheon/Sources.md | v0.1; krytyczne wydania do uzupełnienia |
| Historie bogów | pantheon/GodHistories.md | 6 kart v0.1 |
| Historia świata | world/Timeline.md | struktura v0.1; prawda głównej tajemnicy otwarta |
| Kultury | world/Cultures.md | framework + region startowy v0.1 |
| Regiony/królestwa | world/RegionsAndKingdoms.md | framework v0.1 |
| 4 lokacje vertical slice | locations/*.md | indywidualne karty v0.1 |
| Usługi vertical slice | world/VerticalSliceServices.md | v0.1 |
| Mapa | world/WorldMap.md | topologia v0.1 |
| Day/night events | world/DayNightEvents.md | v0.1 |
| Weather gameplay | world/WeatherGameplay.md | v0.1 |
| Magia | magic/MagicBible.md | v0.1; vertical-slice czar i rytuał mają osobną kartę |
| Magia vertical slice | magic/VerticalSliceMagic.md | pierwszy czar, rytuał i znaki F v0.1 |
| Bestiariusz | bestiary/BestiaryBible.md | framework v0.1; karty źródłowe do badań |
| Bohater | character/PlayerCharacter.md | v0.1 |
| Rodzina | character/FamilyMystery.md | struktura v0.1; rozwiązanie celowo otwarte |
| Główna historia | story/MainStory.md | akty v0.1; finałowe lore otwarte |
| Questy | design/QuestDesign.md | v0.1 |
| Światło nad mokradłem | quests/LightOverSwamp.md | pełna karta v0.1: fazy, dowody, wyniki, checkpointy, QA |
| Evidence text | quests/LightOverSwampEvidenceText.md | robocze wpisy Journal v0.1 |
| Reaction matrix | quests/LightOverSwampReactionMatrix.md | trzy rozwiązania + modyfikatory v0.1 |
| Side questy | quests/VerticalSliceSideQuests.md | pierwsza pula v0.1 |
| Decyzje | design/DecisionModel.md | v0.1 |
| Zakończenia | story/Endings.md | architektura v0.1 |
| Game design | design/GameDesignBible.md | v0.1 |
| Combat | design/CombatDesign.md | v0.1 |
| Melee | design/MeleeCombat.md | szczegóły v0.1 |
| Bow | design/BowCombat.md | szczegóły v0.1 |
| Status effects | design/StatusEffects.md | v0.1 |
| Equipment | design/EquipmentSystem.md | v0.1 |
| Progression | design/Progression.md | v0.1 |
| Economy/vendors | design/EconomyPass01.md | first pass v0.1 |
| Tracking | design/TrackingSystem.md | v0.1 |
| Encounter design | design/EncounterDesign.md | v0.1 |
| Alchemy recipes | alchemy/RecipesV01.md | pierwszy katalog v0.1 |
| NPC/dialog | design/NpcDialogueDesign.md | v0.1 |
| NPC vertical slice | character/VerticalSliceNPCs.md | 5 kart roboczych v0.1 |
| Dialogi 5 NPC | dialogue/*.md | kompletne grafy robocze vertical slice v0.1 |
| Inventory/economy | design/InventoryEconomy.md | v0.1 |
| Przedmioty vertical slice | content/VerticalSliceItems.md | minimalny katalog + receptura v0.1 |
| Save | design/SavePersistence.md | v0.1 |
| Input | design/ControlsAndInput.md | v0.1 |
| UX/accessibility | design/UXAccessibility.md | v0.1 |
| HUD | ui/HudSpec.md | v0.1 |
| Inventory UI | ui/InventoryFlow.md | v0.1 |
| Journal UI | ui/JournalFlow.md | v0.1 |
| Dialogue UI | ui/DialogueFlow.md | v0.1 |
| Map UI | ui/MapFlow.md | v0.1 |
| Input action map | design/InputActionMap.md | v0.1 |
| Settings | design/SettingsMatrix.md | v0.1 |
| Save-slot UX | design/SaveSlotUX.md | v0.1 |
| Audio/wizual | design/AudioVisualDirection.md | v0.1 |
| Vertical slice | design/VerticalSlice.md | v0.1 |
| Asset list vertical slice | design/VerticalSliceAssetList.md | P0/P1/P2 v0.1 |
| Konwencje ID | content/IdConventions.md | v0.1 |
| Kolejka dokumentacji | design/DocumentationWorkQueue.md | kolejność od łatwych do centralnego lore |
| Architektura | design/EngineArchitecture.md | v0.1 |
| Rendering/platformy | technical/RenderingAndPlatform.md | v0.1 |
| Testy/performance | technical/TestingAndPerformance.md | v0.1 |
| Developer overlay | technical/DeveloperOverlay.md | v0.1 |
| Logging | technical/LoggingPolicy.md | v0.1 |
| Research policy | research/ResearchPolicy.md | v0.1 |
| Produkcja treści | design/ContentProduction.md | v0.1 |
| Release | design/ReleaseCriteria.md | v0.1 |

## Co nadal nie jest „pełne”

Posiadanie dokumentu v0.1 nie znaczy zamknięcia tematu.

Najważniejsze dalsze prace:

1. krytyczne źródła dla panteonu;
2. indywidualne karty bestiariusza;
3. szczegółowe research cards życia materialnego;
4. finalne kultury i państwa;
5. prawdziwa wersja tajemnicy rodziny;
6. natura Czwartej Sfery;
7. finalny konflikt i zakończenia;
8. szczegółowe questy poza vertical slice;
9. finalne parametry balansu;
10. wymagania sprzętowe po pomiarach.

## Definicja „pełnej dokumentacji projektu”

Dokumentacja jest kompletna produkcyjnie, gdy:
- każdy system ma owner/spec;
- wszystkie główne questy mają karty;
- każdy region ma bible;
- każda finalna istota ma research card;
- każda kultura ma research package;
- zakończenia są rozpisane;
- asset listy istnieją;
- targety performance są zmierzone;
- release criteria są spełnione.

Obecny zestaw jest **pełnym szkieletem dokumentacji v0.1**, ale nie finalną dokumentacją gotowej gry.
