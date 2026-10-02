# Pokrycie dokumentacji

Stan roboczy v0.2. Dokument odpowiada na pytanie: **co już ma własną specyfikację, a co nadal wymaga rozwinięcia?**

## Stan wysokiego poziomu

Projekt ma pełny obraz designu i author truth first pass. Centralne lore nie jest już głównym blockerem implementacji. Aktywna luka to przejście od szkieletu do kontraktów produkcyjnych: szczegółowe karty main questów, region bibles, runtime data contracts i finalniejsze listy contentu.

| Obszar | Główny dokument | Stan |
|---|---|---|
| Wizja / pełny obraz | README.md + FullGameOverview.md | v0.2 |
| Świat / kosmologia | world/WorldBible.md + world/Cosmology.md + world/FourthSphereTruth.md | author truth v0.2 |
| Historia / kryzys | world/Timeline.md + story/MysteryChronology.md + story/CrisisTruth.md | v0.2 |
| Kampania regionalna | story/CampaignRegionalFlow.md | first pass kompletny |
| Main quest skeleton | story/MainQuestSkeleton.md | MQ00–MQ56, 29 jednostek |
| Main quest cards Akt 0–I | quests/MainQuestCardsAct0I.md | production first pass |
| Main quest cards Akt II–V | brak | **otwarte — priorytet** |
| Regiony makro | world/MacroGeography.md + world/RegionsAndKingdoms.md | v0.1 |
| Region bibles | częściowo locations/world docs | **otwarte — priorytet** |
| Panteon | pantheon/* + research/pantheon/* | broad first pass; krytyczne źródła do content lock otwarte |
| Bestiariusz | bestiary/BestiaryBible.md + research/bestiary/* | broad first pass; ogniki + forest-guardian lock otwarte |
| Material culture | research/material-culture/* | 9 kart first pass; szczegóły regionów do pogłębienia |
| Magia | magic/* | v0.1, vertical slice implementowalny |
| Combat / bow / status / equipment | design/*Combat.md + StatusEffects.md + EquipmentSystem.md | v0.1 |
| Progression | design/Progression.md | v0.1 |
| Economy/vendors | design/EconomyPass01.md + InventoryEconomy.md | v0.1 |
| Alchemy | alchemy/RecipesV01.md | first catalog v0.1 |
| Tracking | design/TrackingSystem.md | v0.1 |
| Encounter design | design/EncounterDesign.md + content encounter format | v0.1 |
| Day/night/weather | world/DayNightEvents.md + world/WeatherGameplay.md + content/TimeEventFormat.md | behavior + data contract v0.1 |
| NPC/dialog design | design/NpcDialogueDesign.md + character/VerticalSliceNPCs.md + dialogue/* | vertical slice v0.1 |
| Save/persistence | design/SavePersistence.md | design v0.1; runtime state contract do uszczegółowienia |
| Input/settings/UI | design/* + ui/* | v0.1 |
| Content formats | content/*Template.md + *Format.md | broad v0.1; runtime vocabularies nadal do domknięcia |
| Engine/rendering | design/EngineArchitecture.md + technical/RenderingAndPlatform.md | v0.1 |
| Testing/performance | technical/TestingAndPerformance.md | plan v0.1; measured targets otwarte |
| Logging/dev overlay | technical/LoggingPolicy.md + DeveloperOverlay.md | v0.1 |
| Release | design/ReleaseCriteria.md | kryteria v0.1; evidence później |

## Central lore / author truth

| Obszar | Dokument | Stan |
|---|---|---|
| Czwarta Sfera | world/FourthSphereTruth.md | ustalona |
| Przyczyna kryzysu | story/CrisisTruth.md | ustalona |
| Rodzina | character/FamilyTruth.md | ustalona |
| Antagoniści | story/Antagonists.md | Wszebor + frakcje v0.1 |
| Śmierć boga | world/GodMortality.md | reguła ustalona |
| Finał | story/Finale.md | przebieg v0.1 |
| Główne endingi | story/EndingVariants.md | E1–E5 |
| Epilogi | story/EpilogueMatrix.md | matryca v0.1 |
| Chronologia tajemnicy | story/MysteryChronology.md | ustalona |
| Reveal plan | story/RevelationPlan.md | akty 0–V |

## Luki blokujące swobodne programowanie kolejnych systemów

Poniższe elementy są obecnie najważniejsze:

1. produkcyjne karty MQ dla Aktów II–V;
2. region bibles dla kampanii;
3. quest runtime state machine i wspólny condition/effect vocabulary;
4. dialogue runtime contract;
5. reputation/faction state contract;
6. world-state/event persistence contract;
7. encounter spawn/despawn i AI archetype contracts;
8. animation/audio event contracts;
9. localization/text-key contract;
10. content validation/build pipeline;
11. finalniejsze regionalne creature/NPC/item/asset rosters.

Aktywna kolejność jest utrzymywana w `design/DocumentationWorkQueue.md`.

## Jawnie otwarte decyzje nieblokujące architektury

- finalna nazwa/tożsamość `forest-guardian`;
- pełna karta ogników/błędnych świateł;
- finalne nazwy części NPC/urzędów/lokacji;
- broń startowa content lock;
- konkretne liczby balansu;
- wymagania sprzętowe i targety performance po pomiarach.

Te decyzje muszą mieć stabilne placeholder IDs, aby kod nie zależał od późniejszej nazwy lub tuningu.

## Definicja „wstępnie kompletne do programowania”

Dokumentacja osiąga próg implementacyjny, gdy:
- każdy główny system ma owner/spec oraz jawny kontrakt danych i stanu;
- wszystkie główne questy mają production cards lub wspólny kontrakt bez krytycznych luk;
- każdy region kampanii ma bible;
- formaty contentu i walidacja pozwalają programować bez zgadywania;
- wszystkie otwarte decyzje blokujące kod są rozwiązane albo zastąpione stabilnym abstrakcyjnym kontraktem.

Target organizacyjny: **2026-10-08 21:16 Europe/Warsaw**.

## Definicja pełnej dokumentacji projektu

Dokumentacja jest kompletna produkcyjnie, gdy dodatkowo:
- wszystkie questy i finalne istoty mają karty;
- każda kultura ma research package;
- finalne asset lists są kompletne;
- balance jest oparty o playtesty;
- targety performance i wymagania sprzętowe są zmierzone;
- release criteria mają evidence;
- nie istnieje nieoznaczona luka, która wymaga od implementującego zgadywania intencji designu.

Obecny stan: **pełny szkielet + author truth + rozpoczęty production pass; jeszcze nie pełna dokumentacja produkcyjna**.
