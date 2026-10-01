# Dokumentacja SlavicGame

Wersja projektu świata i designu: **0.1**, 2026-10-01.

Dokumentacja jest po polsku; nazwy typów i identyfikatory w kodzie pozostają po angielsku. SlavicGame jest trzecioosobowym Adventure / Action RPG w autorskim świecie inspirowanym kulturami słowiańskimi, folklorem i realiami życia materialnego mniej więcej IX–X wieku.

> **Ważne:** dokument v0.1 oznacza istniejącą specyfikację roboczą, nie zamknięty temat ani ukończoną implementację.

Najlepszy punkt wejścia do całej gry: [Pełny obraz gry v0.2](FullGameOverview.md).  
Kontrola kompletności: [Pokrycie dokumentacji](design/DocumentationCoverage.md).

## Fundament świata i lore

| Dokument | Zakres |
|---|---|
| [World Bible](world/WorldBible.md) | Filary świata, codzienność, Pogranicze Żarnowca |
| [Kosmologia](world/Cosmology.md) | Jawia, Nawia, sfera boska, Czwarta Sfera |
| [Czwarta Sfera — author truth](world/FourthSphereTruth.md) | Rzeczywista natura Splotu i zasady anomalii |
| [Śmierć boga — author rule](world/GodMortality.md) | Manifestacja, kotwice i prawdziwa śmierć |
| [Timeline](world/Timeline.md) | Struktura historii świata i wydarzenia startowego regionu |
| [Kultury](world/Cultures.md) | Zasady projektowania kultur i kultura pogranicza |
| [Królestwa i regiony](world/RegionsAndKingdoms.md) | Geografia, polityka, zasoby i konflikty |
| [Mapa świata](world/WorldMap.md) | Skala, topologia, regiony, fast travel i sfery |
| [Naming Rules](world/NamingRules.md) | Reguły nazw świata, kultur i postaci |
| [Kultury makro](world/MacroCultures.md) | Sześć głównych kontekstów kulturowych |
| [Siły polityczne](world/PoliticalPowers.md) | Główne państwa i organizacje |
| [Geografia makro](world/MacroGeography.md) | R0–R6 i osie podróży |
| [Gospodarka regionów](world/InterregionalEconomy.md) | Przepływ zasobów i skutki kryzysu |
| [Konflikty polityczne](world/PoliticalConflicts.md) | Centralizacja, cła, las, przełęcze i Arel |
| [Timeline makro](world/EraTimelineExpanded.md) | Główne epoki i wydarzenia BG |
| [Żarnowiec](locations/Zarnowiec.md) | Hub, strefy, dzień/noc i stany po queście |
| [Puszcza Żywia](locations/PuszczaZywia.md) | Trasy, zasoby, guardian i nawigacja |
| [Czarne Mokradła](locations/BlackSwamp.md) | Śledztwo, predator, apparition i leak zone |
| [Kamienny Krąg](locations/KamiennyKrag.md) | Nauka rytuału, znaki i opcjonalny divine encounter |
| [Day/Night Events](world/DayNightEvents.md) | Tabela zdarzeń i priorytety czasowe |
| [Weather Gameplay](world/WeatherGameplay.md) | Wpływ pogody na widoczność, tropy i eventy |

## Mitologia i research

| Dokument | Zakres |
|---|---|
| [Pantheon Bible](pantheon/PantheonBible.md) | 52 wpisy badawcze, warianty, klasy A–F |
| [Historie bogów](pantheon/GodHistories.md) | Karty projektowe 11+ postaci/tradycji z rozdzieleniem minimum źródłowego od F |
| [Panteon research 02](research/pantheon/MethodAndSources02.md) | Metoda i nowe punkty odniesienia |
| [Rod/Rodzanice](research/pantheon/RodRodzanice.md) | Ograniczenia tekstów polemicznych i model F |
| [Jarowit](research/pantheon/Jarowit.md) | Pierwszy pass bez sztucznego utożsamienia z Jarilem |
| [Radegast–Swarożyc](research/pantheon/RadegastSvarozic.md) | Spór nazwy bóstwa/miejsca/tradycji |
| [Siwa–Żywie](research/pantheon/SiwaZywie.md) | Rozdzielenie lokalnej Siwy od późnego Żywie |
| [Późny katalog polski](research/pantheon/LatePolishCatalogue.md) | Długosz i ryzyko anachronizmu |
| [Kandydaci literaccy](research/pantheon/LiteraryCandidates.md) | Karna, Żela, Div, Trojan i Białobóg |
| [Praktyki religijne](research/pantheon/ReligiousPracticePrinciples.md) | Reguły używania regionalnych źródeł |
| [Instytucje kultowe F](pantheon/CultInstitutions.md) | Lokalne wspólnoty, opiekunowie i reputacja |
| [Boskie umowy F](pantheon/DivineContracts.md) | Jawne warunki, wielu patronów i renegocjacja |
| [Matryca relacji bogów F](pantheon/RelationshipMatrix.md) | Konflikty interesów bez osi dobro/zło |
| [Źródła](pantheon/Sources.md) | Bibliografia i zakres konsultacji |
| [Polityka researchu](research/ResearchPolicy.md) | Zasady źródeł, rekonstrukcji i fikcji F |
| [Źródła kultury materialnej](research/material-culture/MaterialCultureSources.md) | Rejestr MC01–MC14 i zakres konsultacji |
| [Budownictwo](research/material-culture/Architecture.md) | Osady, półziemianki i granice rekonstrukcji |
| [Osada i rzemiosło](research/material-culture/SettlementCrafts.md) | Specjalizacje, wymiana i warsztaty |
| [Rolnictwo i narzędzia](research/material-culture/AgricultureTools.md) | Sierpy, żarna, radlice i rybołówstwo |
| [Żywność](research/material-culture/FoodSubsistence.md) | Stan badań archeobotanicznych/archeozoologicznych |
| [Transport](research/material-culture/Transport.md) | Mosty, drogi i dłubanki |
| [Ubiór i tekstylia](research/material-culture/ClothingTextiles.md) | Tekstylia, sakiewki i skóra |
| [Uzbrojenie](research/material-culture/Weapons.md) | Elitarność Lednicy i konsekwencje dla hunter gear |
| [Pochówki](research/material-culture/Burials.md) | Zmienność praktyk i chrystianizacja |
| [Handel i płatność](research/material-culture/TradeEconomy.md) | Targ, obce monety, srebro i barter |
| [Wnioski produkcyjne](research/material-culture/ProductionImplications.md) | Co art/design może już bezpiecznie stosować |

## Magia, istoty i bohater

| Dokument | Zakres |
|---|---|
| [Magic Bible](magic/MagicBible.md) | Źródła magii, czary, rytuały, alchemia, alfabet |
| [Bestiary Bible](bestiary/BestiaryBible.md) | Metoda kart istot, ekologia i alternatywy wobec walki |
| [Źródła bestiariusza](research/bestiary/BestiarySources.md) | Pakiet B01–B06, zakres i ograniczenia |
| [Rusałka](research/bestiary/Rusalka.md) | Wschodniosłowiański folklor i ograniczenia użycia |
| [Duch leśny](research/bestiary/ForestSpirit.md) | Zmienność tradycji i fit do forest-guardian |
| [Wodnik](research/bestiary/WaterSpirit.md) | Czeska tradycja kulturowa i późne transformacje |
| [Zmora](research/bestiary/Zmora.md) | Późny polski folklor nocny |
| [Strzygoń/strzyga](research/bestiary/Strzygon.md) | Materiał etnolingwistyczny o revenancie |
| [Fit vertical slice](research/bestiary/VerticalSliceFit.md) | Decyzja, które nazwy nie pasują do prototypów |
| [Bestiary pass 02](research/bestiary/BestiaryPass02Summary.md) | Polski materiał: wodnik/topielec, południca, boginki, upiór i ogniki |
| [Źródła bestiariusza 02](research/bestiary/BestiarySources02.md) | Rejestr B07–B14 |
| [Topielec/wodnik PL](research/bestiary/TopielecWaterSpiritPL.md) | Regionalne nazwy i późny folklor |
| [Południca](research/bestiary/Poludnica.md) | Pole, południe i ograniczenia źródłowe |
| [Boginka/mamuna](research/bestiary/BoginkaMamuna.md) | Nakładanie się regionalnych demonów |
| [Upiór](research/bestiary/Upior.md) | Revenant bez popkulturowego skrótu |
| [Błędne ogniki](research/bestiary/WillOWisps.md) | Typ zjawiska zamiast jednego gatunku |
| [Bohater](character/PlayerCharacter.md) | Kreator, zawód, style gry i progresja |
| [Tajemnica rodziny](character/FamilyMystery.md) | Player-facing struktura odkrywania |
| [Rodzina — author truth](character/FamilyTruth.md) | Prawdziwy udział rodziców i Noc Zamkniętego Progu |

## Fabuła

| Dokument | Zakres |
|---|---|
| [Główna historia](story/MainStory.md) | Struktura aktów i wymagania kampanii |
| [Przepływ kampanii](story/CampaignRegionalFlow.md) | Funkcja każdego dużego regionu w historii |
| [Prawda kryzysu](story/CrisisTruth.md) | Sieć Progów, przeciążenie i Noc Zamkniętego Progu |
| [Antagoniści](story/Antagonists.md) | Wszebor, Krąg Rozwarcia i Straż Zamknięcia |
| [Plan ujawniania](story/RevelationPlan.md) | Jak gracz składa prawdę w Aktach 0–V |
| [Chronologia tajemnicy](story/MysteryChronology.md) | Autor truth >1000 BG → finał |
| [Finał](story/Finale.md) | Etapy Pierwszego Progu |
| [Zakończenia](story/Endings.md) | Osie finału i techniczny model |
| [5 wariantów endingów](story/EndingVariants.md) | E1–E5 |
| [Epilogue Matrix](story/EpilogueMatrix.md) | Regiony, NPC, bogowie i bohater |

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
| [Teksty dowodów](quests/LightOverSwampEvidenceText.md) | Robocze wpisy Journal |
| [Macierz reakcji](quests/LightOverSwampReactionMatrix.md) | Reakcje stron na trzy rozwiązania |
| [Side questy regionu](quests/VerticalSliceSideQuests.md) | Pierwsza pula zadań pobocznych |
| [NPC vertical slice](character/VerticalSliceNPCs.md) | Pięć ról, wiedza, biasy i reakcje |
| [Dialogi vertical slice](dialogue/VerticalSliceDialogueGraphs.md) | Wspólna struktura grafów |
| [Dialog missing-family](dialogue/MissingFamilyDialogue.md) | Pełny graph roboczy |
| [Dialog crossing-keeper](dialogue/CrossingKeeperDialogue.md) | Pełny graph roboczy |
| [Dialog herbalist](dialogue/HerbalistDialogue.md) | Pełny graph roboczy |
| [Dialog community-guard](dialogue/CommunityGuardDialogue.md) | Pełny graph roboczy |
| [Dialog shrine-keeper](dialogue/ShrineKeeperDialogue.md) | Pełny graph roboczy |
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
| [Usługi regionu](world/VerticalSliceServices.md) | Zielarka, warsztat, odpoczynek, przeprawa i shrine |
| [Kryteria wydania](design/ReleaseCriteria.md) | Prototype → vertical slice → alpha → beta → 1.0 |
| [Roadmap](design/ProductionRoadmap.md) | 23 etapy projektu i bieżący stan |
| [Pokrycie dokumentacji](design/DocumentationCoverage.md) | Co istnieje i co nadal jest otwarte |
| [Kolejka dokumentacji](design/DocumentationWorkQueue.md) | Kolejność dalszej pracy od najprostszej do najtrudniejszej |
| [Konwencje ID](content/IdConventions.md) | Stabilne identyfikatory treści, save i assetów |
| [Szablon przedmiotu](content/ItemCardTemplate.md) | Format finalnych kart itemów |
| [Format equipment](content/EquipmentDefinitionFormat.md) | Broń i armor |
| [Szablon encounteru](content/EncounterCardTemplate.md) | Trigger, telegraph, outcomes i persistence |
| [Lokacja/interactable](content/LocationInteractableFormat.md) | Format miejsc i interakcji |
| [Timed event](content/TimedEventFormat.md) | Zdarzenia zależne od czasu |

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
