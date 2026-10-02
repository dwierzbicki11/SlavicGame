# Production NPC roster — v0.1

Ten dokument zamienia role z `RecurringCast.md`, region bibles i kart głównych questów w jeden kontrakt produkcyjny. Nie zastępuje indywidualnych kart postaci ani dialogów. Nie nadaje finalnych imion tam, gdzie lore ich jeszcze nie ustaliło.

## Zasady

- Każdy krytyczny beat kampanii ma co najmniej jeden fallback niezależny od obecności konkretnego NPC.
- `essential_until` oznacza ostatni moment, do którego postać nie może zostać trwale usunięta bez równoważnego fallbacku.
- `replaceable` oznacza, że funkcję gameplayową może przejąć dokument, środowisko albo inna rola.
- Quest companion nie oznacza stałego członka drużyny.
- Relacje zapisujemy wielowymiarowo: zaufanie zawodowe, zobowiązanie, konflikt polityczny i dług; nie jednym paskiem sympatii.
- Finalne imię, model, voice i szczegółowy background są content lockiem, nie warunkiem implementacji kontraktu.

## Roster kampanii

| ID | Rola | Region / akty | Krytyczna funkcja | Lifecycle | Fallback |
|---|---|---|---|---|---|
| `foster-guardian` | opiekun/rodzina zastępcza | R0, późniejsze powroty | emocjonalny punkt odniesienia bohatera | chroniony do zamknięcia pierwszego osobistego callbacku; później outcome-driven | list, zapis wspomnienia lub inny członek rodziny zastępczej |
| `shrine-keeper` | opiekun miejsca kultowego | R0, callbacki | lokalna interpretacja anomalii i kultu | replaceable po MQ00 | zapis rytuału, świadectwo lokalne, środowisko |
| `hunter-archivist` | łowca-archiwista | R1, Akty I–V | archiwa, zawodowy kontrapunkt, instytucjonalna wiedza | essential do pierwszego dostępu do archiwum; później outcome-driven | archiwum + drugi łowca/kontakt Kolegium |
| `forest-guide` | przewodnik Wielkiego Boru | R2, Akt I + callback | lokalna nawigacja i reguły lasu | replaceable od pierwszego pełnego tutorialu trasy | tracking/environmental clues |
| `estuary-navigator` | nawigator Ligi Ujścia | R3, Akt II + callback | mapy, handel, portowe sieci informacji | replaceable po pozyskaniu krytycznego pakietu mapowego | kupieckie archiwum + drugi przewoźnik |
| `arel-pathfinder` | pathfinder Arel | R5, Akt II + callback | pamięć tras i praktyczna kultura przestrzeni | replaceable po nauczeniu gracza reguły relacyjnej trasy | znaczniki terenowe + przekaz innej grupy Arel |
| `closure-warden` | przedstawiciel Straży Zamknięcia | R6, Akty III–V | argument stabilności, dostęp do archiwów, frakcyjny kontrapunkt | chroniony do przedstawienia stanowiska Straży; później outcome-driven | dokument Straży + zastępca instytucjonalny |
| `wszebor` | Rozwierający | Akty III–V | główny ludzki antagonista / możliwy partner negocjacji | essential do MQ52 lub wcześniejszego jawnie udokumentowanego zastępstwa scenariuszowego | brak anonimowego zastępcy; jego nieobecność musi być osobnym branch state |
| `parent-b` | biologiczny rodzic B | Akt IV–V | osobiste potwierdzenie prawdy rodzinnej | opcjonalny; nigdy nie jest twardym gate kampanii | dokumenty, echo i wcześniejsze evidence truth |

## Role regionalne bez finalnych personaliów

Te sloty są wymagane produkcyjnie, ale nie wymagają jeszcze finalnych imion. Mogą zostać scalone z powracającą obsadą, jeżeli nie tworzy to single point of failure.

### R0 — Żarnowiec
Roster vertical slice pozostaje właścicielem pięciu istniejących NPC i ich grafów dialogowych. Nie duplikujemy kart tutaj.

### R1 — Nadborze
- `r1-authority-contact` — przedstawiciel władzy dla MQ11;
- `r1-hunter-contact` — niezależny głos środowiska łowców;
- `r1-archive-clerk` — operacyjny dostęp do dokumentów, jeśli `hunter-archivist` jest niedostępny.

### R2 — Wielki Bór
- `r2-local-witness` — świadek zmian tras;
- `r2-resource-stakeholder` — reprezentuje interes eksploatacyjny bez kopiowania konfliktu R1.

### R3 — Przymorze
- `r3-port-authority` — bezpieczeństwo i przepływ portu;
- `r3-merchant-holder` — strona posiadająca fragment danych/mapy;
- `r3-carrier-contact` — perspektywa przewoźników i fallback informacji.

### R4 — Kamienne Wyżyny
- `r4-mine-steward` — operacyjny kontakt kopalni;
- `r4-principality-a` i `r4-principality-b` — reprezentanci konfliktu MQ23;
- `r4-material-specialist` — opcjonalna analiza materiału, nigdy krytyczny gate.

### R5 — Równiny Arel
- `r5-camp-host` — wejście społeczne do obozu;
- `r5-route-memory-holder` — drugi niezależny nośnik wiedzy o trasach;
- żadna pojedyncza postać nie jest „głosem całych Arelów”.

### R6 — Pustkowie Pierwszego Progu
- `r6-expedition-lead` — bieżący interes ekspedycji;
- `r6-archive-specialist` — dostęp/interpretacja materiałów Archiwum Progów;
- `r6-closure-deputy` — instytucjonalny fallback Straży, bez przejmowania osobistej roli `closure-warden`.

## Boskie kontakty

Boskie interakcje używają trzech klas roli zamiast automatycznie materializować boga:

- `divine-messenger-*` — posłaniec;
- `divine-manifestation-*` — ograniczony przejaw;
- `cult-representative-*` — ludzki przedstawiciel kultu.

Konkretny patron nie jest obowiązkowy. Żaden krytyczny main-quest gate nie może wymagać zawarcia boskiego kontraktu.

## Companion scope

Dozwolone archetypy misji:
- escort bez obowiązkowej porażki przy chwilowym rozdzieleniu;
- guide z recovery po zgubieniu ścieżki;
- combat support z revive/retreat policy;
- investigation partner z możliwością zastąpienia evidence;
- temporary faction ally.

Każda implementowana misja z companionem musi jawnie ustalić: spawn/despawn, teleport/recovery, downed/death rule, friendly fire policy, save/load position oraz zachowanie przy zmianie regionu.

## Minimalny model persistence

Dla powracającego NPC zapisujemy tylko dane potrzebne systemowo:
- `npc_id`;
- `availability_state` (`available`, `away`, `injured`, `dead`, `unknown`);
- `professional_trust`;
- `political_alignment_state`;
- `obligation_state`;
- quest-specific flags przez właściciela questa.

Nie zapisujemy globalnego `affection` jako uniwersalnej relacji.

## Production locks / otwarte decyzje

Przed finalnym content lockiem nadal trzeba ustalić:
- finalne imiona i warianty językowe ról regionalnych;
- które sloty regionalne można bezpiecznie scalić w jedną postać;
- pełne indywidualne karty background/voice/appearance;
- dokładne killability windows po zakończeniu ich krytycznej funkcji;
- finalny voice budget i liczba wariantów reakcji;
- companion combat tuning oraz pathfinding po playtestach.

Te decyzje nie blokują implementacji ID, persistence, availability, fallbacków ani quest gatingu.

## QA kontraktu rosteru

- Każdy krytyczny quest lead ma test przy niedostępnym opcjonalnym NPC.
- Save/load zachowuje availability i relacje bez wskrzeszania lub duplikacji postaci.
- Zmiana regionu nie duplikuje powracającego NPC.
- NPC oznaczony jako optional nie staje się pośrednio mandatory przez dialogue trigger.
- Fallback nie ujawnia większego reveal budget niż podstawowa ścieżka.
- Epilog czyta snapshot końcowy, a nie obecność aktora w załadowanej scenie.
