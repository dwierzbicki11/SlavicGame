# Main Quest Card Template — v0.1

Ten szablon jest kontraktem produkcyjnym dla kart MQ00–MQ56 z `MainQuestSkeleton.md`. Karta ma uszczegóławiać istniejący skeleton, a nie zmieniać centralne lore bez jawnej decyzji.

## Metadata

- **ID:** `MQxx`
- **Roboczy tytuł:**
- **Akt:**
- **Region / lokacje:**
- **Owner:** Narrative + Quest Design
- **Status:** draft / implementation-ready / content-lock
- **Zależności:** poprzednie MQ, wymagane systemy i region state

## Cel narracyjny

Krótko: czego gracz ma się dowiedzieć, poczuć lub błędnie założyć. Oddzielić wiedzę gracza od author truth.

## Cel gameplayowy

Jakie core loop/systemy quest ćwiczy lub łączy: investigation, tracking, combat, dialogue, traversal, ritual, reputation, economy, timed events itd.

## Wejście

- warunki uruchomienia;
- źródło questa;
- wymagany region/world state;
- zachowanie przy wcześniejszym odkryciu lokacji lub dowodu.

## Przebieg krytyczny

Numerowane beaty wymagane do ukończenia. Każdy beat podaje:

1. **Trigger** — co go rozpoczyna;
2. **Objective** — zadanie widoczne dla gracza;
3. **Gameplay** — używany system;
4. **State write** — trwała zmiana;
5. **Fail/fallback** — co dzieje się, gdy NPC, czas, walka lub kolejność odbiegają od happy path.

## Ścieżki opcjonalne

Dowody, rozmowy, skróty, alternatywne rozwiązania i side-content. Opcjonalna treść może zmieniać koszt, wiedzę, wsparcie, NPC lub epilog, ale nie może być jedynym sposobem zrozumienia podstawowego finału.

## Decyzje i konsekwencje

| Decision ID | Opcje | Natychmiastowy skutek | Trwały state | Późniejszy odczyt |
|---|---|---|---|---|
| `MQxx-D01` | TBD | TBD | TBD | TBD |

## Reveal / evidence budget

- **Must learn:** informacje wymagane do zrozumienia kampanii;
- **May learn:** pogłębienie i foreshadowing;
- **Must not reveal yet:** author truth zarezerwowany dla późniejszych aktów.

Każdy ważny reveal wskazuje źródło: dokument, NPC, środowisko, echo, istota, bóg lub obserwacja gameplayowa.

## NPC i frakcje

Lista uczestników wraz z rolą w queście, stanem wejściowym, możliwym stanem wyjściowym oraz fallbackiem, jeśli postać jest niedostępna.

## Encountery i lokacje

Odwołania do trwałych ID lokacji, encounterów, interactables i timed events. Karta questa nie duplikuje ich pełnych definicji.

## Rewards / costs

- progression;
- przedmioty/waluta;
- reputacja;
- wiedza/evidence;
- world/region state;
- koszt zasobów lub kontraktów.

## Save/load i persistence

Jawne checkpointy oraz stany, których nie wolno utracić lub powtórzyć po save/load. Quest musi być odporny na zapis między beatami oraz na ponowne wejście do regionu.

## Fail states i recovery

Preferowane są recoverable setbacks. Dla każdego twardego faila karta uzasadnia, dlaczego reload/game-over jest konieczny. Brak krytycznego NPC/przedmiotu musi mieć fallback albo jawny invariant systemowy.

## Integracje systemowe

Lista wymaganych systemów i ich kontraktów danych. Nie wolno oznaczyć karty jako `implementation-ready`, jeśli zależy od nieopisanej semantyki systemu.

## Asset / content hooks

Lista potrzebnych dialogów, VO placeholderów, animacji, VFX, audio, lokacji, propsów, UI/journal entries i cinematic beats. Szczegółowe budżety pozostają w asset listach.

## QA acceptance

Minimum:

- happy path;
- każda decyzja i alternatywne rozwiązanie;
- save/load na każdym checkpointcie;
- wejście w nietypowej kolejności;
- brak duplikacji rewardów/reveali;
- poprawny region/world state po ukończeniu;
- brak softlocka po utracie opcjonalnego NPC lub dowodu;
- poprawne zachowanie timed events, jeśli quest ich używa.

## Open decisions

Tylko rzeczy rzeczywiście nierozstrzygnięte. Każda pozycja ma ownera i informację, co blokuje. Puste `Open decisions` oznacza, że karta może kandydować do `implementation-ready` po QA review.

## Definition of Ready

Karta jest `implementation-ready`, gdy:

- wszystkie wymagane beaty i state writes są jawne;
- decyzje mają trwałe ID i konsekwencje;
- reveal budget nie przeczy `RevelationPlan` ani author truth;
- wszystkie zależne systemy mają istniejące specyfikacje;
- krytyczne fallbacki/save-load są opisane;
- wymagane content hooks są policzone przynajmniej jako lista;
- nie ma otwartej decyzji zmieniającej kontrakt implementacji.
