# Dokumentacja SlavicGame

Wersja projektu świata i designu: **0.1**, 2026-10-01.

Dokumentacja jest po polsku; nazwy typów i identyfikatory w kodzie pozostają po angielsku. SlavicGame jest trzecioosobowym Adventure / Action RPG w autorskim świecie inspirowanym kulturami słowiańskimi, folklorem i realiami życia materialnego mniej więcej IX–X wieku.

> **Ważne:** dokument v0.1 oznacza istniejącą specyfikację roboczą, nie zamknięty temat ani ukończoną implementację.

Najlepszy punkt kontroli kompletności: [Pokrycie dokumentacji](design/DocumentationCoverage.md).

## Fundament świata i lore

| Dokument | Zakres |
|---|---|
| [World Bible](world/WorldBible.md) | Filary świata, codzienność, Pogranicze Żarnowca |
| [Kosmologia](world/Cosmology.md) | Jawia, Nawia, sfera boska, Czwarta Sfera |
| [Timeline](world/Timeline.md) | Struktura historii świata i wydarzenia startowego regionu |
| [Kultury](world/Cultures.md) | Zasady projektowania kultur i kultura pogranicza |
| [Królestwa i regiony](world/RegionsAndKingdoms.md) | Geografia, polityka, zasoby i konflikty |
| [Mapa świata](world/WorldMap.md) | Skala, topologia, regiony, fast travel i sfery |
| [Day/Night Events](world/DayNightEvents.md) | Tabela zdarzeń i priorytety czasowe |
| [Weather Gameplay](world/WeatherGameplay.md) | Wpływ pogody na widoczność, tropy i eventy |

## Mitologia i research

| Dokument | Zakres |
|---|---|
| [Pantheon Bible](pantheon/PantheonBible.md) | 52 wpisy badawcze, warianty, klasy A–F |
| [Historie bogów](pantheon/GodHistories.md) | Karty Peruna, Welesa, Mokoszy, Świętowita, Swarożyca i Trygława |
| [Źródła](pantheon/Sources.md) | Bibliografia i zakres konsultacji |
| [Polityka researchu](research/ResearchPolicy.md) | Zasady źródeł, rekonstrukcji i fikcji F |

## Magia, istoty i bohater

| Dokument | Zakres |
|---|---|
| [Magic Bible](magic/MagicBible.md) | Źródła magii, czary, rytuały, alchemia, alfabet |
| [Bestiary Bible](bestiary/BestiaryBible.md) | Metoda kart istot, ekologia i alternatywy wobec walki |
| [Bohater](character/PlayerCharacter.md) | Kreator, zawód, style gry i progresja |
| [Tajemnica rodziny](character/FamilyMystery.md) | Struktura odkrywania bez zamrażania finałowej prawdy |

## Fabuła

| Dokument | Zakres |
|---|---|
| [Główna historia](story/MainStory.md) | Struktura aktów i wymagania kampanii |
| [Zakończenia](story/Endings.md) | Osie finału, epilogi i ścieżka bez patrona |

## Game design

| Dokument | Zakres |
|---|---|
| [Game Design Bible](design/GameDesignBible.md) | Core loop i pełny model systemów |
| [Quest Design](design/QuestDesign.md) | Stany, dowody, rozwiązania i nagrody |
| [Decision Model](design/DecisionModel.md) | Decyzje i konsekwencje bez good/evil |
| [Combat Design](design/CombatDesign.md) | Melee, stamina, blok, unik, łuk |
| [Melee Combat](design/MeleeCombat.md) | Stany, hit window, blok, unik i QA |
| [Bow Combat](design/BowCombat.md) | Aim, draw, projectile, ammo i retrieval |
| [Status Effects](design/StatusEffects.md) | Statusy, stack policy i pierwsze efekty |
| [Equipment System](design/EquipmentSystem.md) | Sloty, broń, armor i persistence |
| [Progression](design/Progression.md) | Kierunki rozwoju bez pustych +1% |
| [Ekonomia first pass](design/EconomyPass01.md) | Pieniądze, vendorzy, ceny i anti-exploit |
| [Tracking System](design/TrackingSystem.md) | Typy śladów, świeżość i dowody |
| [Encounter Design](design/EncounterDesign.md) | Karta encounteru, telegraph i repeat policy |
| [Receptury v0.1](alchemy/RecipesV01.md) | Pierwsze receptury przygotowawcze |
| [NPC i dialog](design/NpcDialogueDesign.md) | Harmonogramy, graf dialogowy, pamięć NPC |
| [Inventory i ekonomia](design/InventoryEconomy.md) | Przedmioty, pieniądze, crafting i ceny |
| [Save i persistence](design/SavePersistence.md) | Checkpointy, wersje save i stabilne ID |
| [Sterowanie](design/ControlsAndInput.md) | KBM, remapping, kontroler i Android |
| [UX i dostępność](design/UXAccessibility.md) | Napisy, kamera, input, UI, trudność |
| [Kierunek audio-wizualny](design/AudioVisualDirection.md) | Światło, noc, supernaturalność, audio |
| [HUD](ui/HudSpec.md) | Informacje stałe, kontekstowe i combat feedback |
| [Inventory UI](ui/InventoryFlow.md) | Widoki, akcje, sorting i crafting entry |
| [Journal UI](ui/JournalFlow.md) | Questy, dowody, bestiary i źródła wiedzy |
| [Dialogue UI](ui/DialogueFlow.md) | Choices, requirements, skutki i historia rozmowy |
| [Map UI](ui/MapFlow.md) | Discovery, markery i fast travel |
| [Input Action Map](design/InputActionMap.md) | Nazwane akcje i domyślne bindingi |
| [Settings Matrix](design/SettingsMatrix.md) | Grafika, kamera, audio, gameplay i accessibility |
| [Save Slot UX](design/SaveSlotUX.md) | Autosave/checkpoint/manual slot i corruption flow |
| [Pierwszy grywalny wycinek](design/VerticalSlice.md) | „Światło nad mokradłem” i kryteria odbioru |
| [Pełna karta questa](quests/LightOverSwamp.md) | Fazy, dowody, rozwiązania, checkpointy i QA |
| [NPC vertical slice](character/VerticalSliceNPCs.md) | Pięć ról, wiedza, biasy i reakcje |
| [Przedmioty vertical slice](content/VerticalSliceItems.md) | Quest items, składniki, broń i receptura |
| [Magia vertical slice](magic/VerticalSliceMagic.md) | Pierwszy czar, rytuał i dwa znaki F |
| [Lista assetów vertical slice](design/VerticalSliceAssetList.md) | P0/P1/P2 dla środowiska, postaci, VFX, UI i audio |

## Technologia i produkcja

| Dokument | Zakres |
|---|---|
| [Architektura silnika](design/EngineArchitecture.md) | Granice systemów, GameProgress, checkpoint |
| [Rendering i platformy](technical/RenderingAndPlatform.md) | Veldrid/Vulkan, PC, późniejszy Android |
| [Testy i wydajność](technical/TestingAndPerformance.md) | CI, regresje, profile i metryki |
| [Developer Overlay](technical/DeveloperOverlay.md) | Runtime diagnostyka i debug sekcje |
| [Logging Policy](technical/LoggingPolicy.md) | Severity, kategorie, crash package i retention |
| [Produkcja treści](design/ContentProduction.md) | Pipeline regionu, NPC, potwora i questa |
| [Kryteria wydania](design/ReleaseCriteria.md) | Prototype → vertical slice → alpha → beta → 1.0 |
| [Roadmap](design/ProductionRoadmap.md) | 23 etapy projektu i bieżący stan |
| [Pokrycie dokumentacji](design/DocumentationCoverage.md) | Co istnieje i co nadal jest otwarte |
| [Kolejka dokumentacji](design/DocumentationWorkQueue.md) | Kolejność dalszej pracy od najprostszej do najtrudniejszej |
| [Konwencje ID](content/IdConventions.md) | Stabilne identyfikatory treści, save i assetów |

## Jak czytać statusy

- **Ustalone założenie** — obowiązuje dalszy projekt.
- **Projekt v0.1 / F** — autorski materiał roboczy.
- **Otwarte** — decyzja celowo nie została jeszcze podjęta.
- **Zaimplementowane** — zachowanie faktycznie istnieje w kodzie.
- **Research otwarty** — dokument istnieje, ale źródła lub szczegóły wymagają dalszej pracy.

Klasy A–F opisują **pochodzenie materiału**, a statusy powyżej opisują **stan projektu**. To różne rzeczy.

## Zasady rozbudowy

1. Historyczne twierdzenie ma źródło, region, datę i niepewność.
2. Fikcja F nie staje się dowodem historycznym.
3. Nie sumujemy wszystkich wpisów panteonu jako „pewnych bogów”.
4. Quest zapisuje osobno interesy stron i konsekwencje.
5. Dokumentacja nie może udawać implementacji.
6. Tajemnice głównej historii pozostają jawnie oznaczone jako Otwarte do chwili ich świadomego rozstrzygnięcia.
7. Każdy duży system powinien mieć dokument projektu, testowalne kryteria oraz plan persistence.
