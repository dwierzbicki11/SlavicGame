# Production Content Catalog

Status: production contract v0.1. Ten dokument nie zastępuje kart questów, region bibles ani research cards. Ustala natomiast jeden spójny rejestr klas contentu, stabilnych ID, ownerów i warunków uznania listy za finalną.

## Cel

Każdy asset lub rekord danych używany przez runtime ma należeć do jawnej klasy contentu i mieć stabilne ID zgodne z `content/IdConventions.md`. Implementacja nie może zależeć od nazw plików, nazw wyświetlanych ani kolejności wpisów w katalogu.

## Źródła prawdy

- questy główne: `story/MainQuestSkeleton.md` i production cards MQ00–MQ56;
- regiony: region bibles R0–R6 oraz `design/RegionBibleIndex.md`;
- NPC: `design/ProductionNpcRoster.md`;
- przedmioty: `content/VerticalSliceItems.md` + format kart przedmiotów;
- encountery: `design/EncounterDesign.md` + encounter format;
- istoty: `bestiary/BestiaryBible.md` + indywidualne research cards;
- magia/alchemia: `magic/` i `alchemy/`;
- lokacje/interactables: karty lokacji + content format;
- UI/audio/VFX: odpowiednie specyfikacje systemowe i region bibles.

Jeżeli katalog i dokument źródłowy są sprzeczne, dokument źródłowy wygrywa, a katalog wymaga synchronizacji.

## Klasy contentu i wymagane ID

| Klasa | Prefiks/rodzina ID | Minimalny rekord produkcyjny | Owner |
|---|---|---|---|
| Quest | `MQxx`, `SQ_*` | ID, prerequisites, phases, outcomes, persistence | quest/story |
| NPC | `NPC_*` | ID, region/role, lifecycle, dialogue owner, persistence | NPC/dialogue |
| Creature | `CREATURE_*` | ID, research status, habitat, encounter role, combat profile | bestiary/combat |
| Item | `ITEM_*` | ID, category, stack/equipment rules, economy refs | inventory/economy |
| Weapon/armor | `WEAPON_*`, `ARMOR_*` | ID, equipment slot, moveset/stats refs, asset refs | combat/equipment |
| Recipe | `RECIPE_*` | ID, ingredients, output, unlock | alchemy |
| Spell/ritual | `SPELL_*`, `RITUAL_*` | ID, cost/conditions/effect, unlock | magic |
| Region | `R0`–`R6` | ID, bible, travel links, persistence namespace | world |
| Location | `LOC_*` | ID, parent region, streaming cell/POI role, state owner | world/level |
| Interactable | `INT_*` | ID, interaction type, state/persistence contract | gameplay |
| Encounter | `ENC_*` | ID, participants, conditions, resolution/fallback | encounter |
| Timed event | `TE_*` | ID, time window, priority/conflict group, repeat/persistence | world events |
| Dialogue | `DIA_*` | ID, speaker, conditions, nodes/outcomes | dialogue |
| Evidence | `EVID_*` | ID, acquisition source, journal text, quest dependency | quest/journal |
| Asset set | `ASSETSET_*` | ID, target region/system, LOD/collision requirements | art/technical |
| Audio cue | `AUD_*` | ID, trigger, spatial/loop policy, fallback | audio |
| VFX cue | `VFX_*` | ID, trigger, gameplay readability requirement | VFX/gameplay |

Dokładna składnia pozostaje podporządkowana `content/IdConventions.md`; powyższe nazwy są rodzinami katalogowymi, nie zgodą na tworzenie sprzecznego drugiego standardu.

## Manifest produkcyjny

Docelowy manifest contentu powinien dla każdego rekordu przechowywać co najmniej:

- `id` — stabilny klucz runtime;
- `type` — jedna z klas katalogu;
- `owner_document` — ścieżka do źródła prawdy;
- `scope` — vertical slice / act / region / global;
- `status` — `planned`, `specified`, `implementation-ready`, `implemented`, `validated`, `locked`;
- `dependencies` — inne stabilne ID wymagane przez rekord;
- `asset_refs` — opcjonalne ID assetów, nigdy luźna nazwa pliku jako jedyny kontrakt;
- `research_ref` — wymagane dla historycznego/folklorystycznego contentu, jeśli dotyczy;
- `open_decisions` — jawna lista blokad produkcyjnych;
- `qa_ref` — test/checklista lub wymaganie walidacji.

## Definition of Ready

Rekord może otrzymać `implementation-ready` tylko gdy:

1. ma stabilne ID i owner document;
2. zachowanie potrzebne runtime nie wymaga zgadywania;
3. wszystkie twarde zależności mają własne ID/spec;
4. kwestie researchowe wpływające na implementację są zamknięte albo jawnie odseparowane jako presentation lock;
5. persistence/save-load jest określone, jeśli rekord zmienia trwały stan;
6. istnieje fallback dla contentu krytycznego dla głównej kampanii;
7. można wskazać minimalne QA/invarianty.

## Definition of Locked

`locked` oznacza, że rekord ma finalną funkcję, tekst/dane potrzebne do wydania, wymagane assety, research approval, tuning po playteście oraz przeszedł QA. `specified` lub `implementation-ready` nie oznacza content locku.

## Zakres katalogów produkcyjnych

### Kampania

MQ00–MQ56 stanowią zamknięty szkielet ID głównej kampanii. Każda karta produkcyjna pozostaje ownerem faz, warunków, decyzji i persistence. Side questy muszą być katalogowane osobno i nie mogą przejmować krytycznych flag MQ bez jawnego kontraktu.

### Regiony

R0–R6 są zamkniętym first-pass zakresem regionów 1.0. Każdy POI, encounter family, NPC slot i asset family wymieniony w bible powinien zostać rozwinięty do osobnego rekordu przed finalnym content lockiem regionu.

### NPC

`ProductionNpcRoster.md` jest rejestrem ról krytycznych. Finalne imiona, VO i presentation background mogą pozostawać otwarte bez blokowania runtime, o ile stabilny NPC ID, lifecycle, fallback i quest ownership są ustalone.

### Bestiariusz

Do finalnego katalogu mogą wejść wyłącznie istoty posiadające wymaganą research card albo jawnie oznaczone jako autorskie F. `forest-guardian` nie może dostać finalnej folklorystycznej nazwy bez zamknięcia regionalnego researchu. `swamp-predator` pozostaje autorską istotą F.

### Assety

`VerticalSliceAssetList.md` pozostaje minimalnym katalogiem R0. Dla pełnej gry potrzebne są osobne listy asset families R0–R6, następnie konkretne rekordy modeli/materials/animations/audio/VFX. Region bible definiuje potrzebę, asset catalog definiuje dostarczany zasób.

## Walidacja i CI

Docelowa walidacja katalogu powinna wykrywać:

- duplikaty stabilnych ID;
- brak owner document;
- odwołania do nieistniejących zależności;
- rekord `implementation-ready` z otwartym twardym blockerem;
- rekord `locked` bez wymaganych QA/research/asset refs;
- krytyczny MQ/NPC/POI bez fallbacku, jeśli jego brak może stworzyć softlock;
- orphaned content: asset/rekord bez żadnego scope/ownera.

Walidator nie powinien wymuszać finalnych nazw presentation ani arbitralnych wartości balansu przed playtestem.

## Otwarte decyzje produkcyjne

Poniższe decyzje są jawnie otwarte i nie mogą być uzupełniane przez implementację na podstawie zgadywania:

- finalne nazwy i presentation profile części NPC;
- finalna nazwa/identyfikacja `forest-guardian`;
- finalny pełny roster istot poza kartami researchowymi;
- konkretne listy modeli, animacji, audio i VFX dla R1–R6;
- finalne statystyki, ceny, drop rates i częstotliwości encounterów;
- finalny scope VO i muzyki;
- zmierzone budżety performance i wynikające z nich LOD/streaming locks;
- finalne wymagania sprzętowe.

## Następne kroki

1. utworzyć konkretne asset/content manifests per R0–R6 na podstawie region bibles;
2. zamknąć brakujące finalne creature research cards;
3. rozwinąć side-quest catalog poza vertical slice;
4. po pierwszych playtestach zamknąć tuning/balance catalog;
5. po pomiarach sprzętowych zamknąć performance/LOD budgets.

Ten dokument daje wspólny kontrakt, dzięki któremu kolejne listy contentu można rozwijać bez duplikowania quest cards, region bibles i research cards.