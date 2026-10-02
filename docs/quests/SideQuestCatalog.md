# Side Quest Catalog — full-game scope v0.1

Status: production planning. Ten dokument zamyka katalog slotów side-content poza referencyjnym vertical slice. Nie zastępuje `QuestDesign.md`, region bibles ani przyszłych production cards. Tytuły są robocze F. Konkretne dialogi, liczby nagród, progi reputacji/evidence i finalne encounter composition pozostają odpowiednio dialogue/playtest/content lockiem.

## Kontrakt katalogu

Każdy slot ma stabilne ID, region, typ, funkcję, wymagane zależności, trwały skutek i owner przyszłej production card. Side quest może zmienić wiedzę, wsparcie, reputację, NPC, koszt finału lub epilog, ale nie może być jedynym źródłem informacji koniecznej do zrozumienia podstawowej osi MQ00–MQ56.

Persistence: `Unavailable/Offered/Active/Investigation/Preparation/Encounter/Resolved/TurnedIn/Failed` zgodnie z `QuestDesign.md`; jednorazowe nagrody i world-state mutation są idempotentne. Każda production card musi jawnie opisać prerequisites, fail-forward, save/load, fallback krytycznego NPC i minimalne QA.

## R0 — Żarnowiec

| ID | Typ | Roboczy temat | Funkcja / trwały skutek |
|---|---|---|---|
| `SQ_R0_01` | wspólnota | Złamany bród | Naprawa lub obejście lokalnej przeprawy; uczy world-state i usług wspólnoty. |
| `SQ_R0_02` | osobisty NPC | Dług zielarki | Alchemia + relacja; odblokowuje alternatywny supply/fallback bez blokowania main quest. |
| `SQ_R0_03` | eksploracyjny | Stary kopiec | Mały evidence chain; wiedza o lokalnej pamięci miejsca, bez nowego central-lore reveal. |

Dependencies: R0 bible, NPC roster, alchemy, tracking, `ASSETSET_R0_CORE`. `LightOverSwamp` pozostaje osobnym referencyjnym vertical-slice questem i nie jest dublowany tutaj.

## R1 — Nadborze

| ID | Typ | Roboczy temat | Funkcja / trwały skutek |
|---|---|---|---|
| `SQ_R1_01` | frakcja | Cło bez pieczęci | Konflikt administracja–lokalny handel; reputacja i dostęp do usługi/skrótów. |
| `SQ_R1_02` | kontrakt łowcy | Ślad przy młynie | Investigation → preparation → encounter; lokalny creature slot wybierany dopiero z rosteru R1. |
| `SQ_R1_03` | wspólnota | Woda dla Dębrzyna | Infrastruktura/osada; trwały wariant world state i ceny/usługi, bez twardego economy tuningu. |

Dependencies: R1 bible, economy/vendors, production bestiary, NPC roster, material-culture lock dla detali młyna/infrastruktury.

## R2 — Wielki Bór

| ID | Typ | Roboczy temat | Funkcja / trwały skutek |
|---|---|---|---|
| `SQ_R2_01` | badawczy | Drzewa znaczone dwa razy | Tracking i sprzeczne interpretacje śladów; knowledge flag. |
| `SQ_R2_02` | kontrakt łowcy | Puste sidła | Presja na ekologię/istotę; rozwiązanie nie musi być walką. |
| `SQ_R2_03` | eksploracyjny | Obozowisko, które wraca | Traversal/anomaly side content; stan miejsca reaguje na decyzję gracza. |

Dependencies: R2 bible, TrackingSystem, encounter format. `forest-guardian` nie może być wymaganym historycznym/folklorystycznym bytem dopóki identity lock pozostaje otwarty; karta ma użyć jawnego F albo innego zatwierdzonego slotu.

## R3 — Przymorze

| ID | Typ | Roboczy temat | Funkcja / trwały skutek |
|---|---|---|---|
| `SQ_R3_01` | wspólnota | Sieci po sztormie | Weather + gospodarka rybacka; dostęp/supply po rozwiązaniu. |
| `SQ_R3_02` | eksploracyjny | Ładunek z wraku | Ryzyko pogodowe, salvage i konflikt własności; reputacja/frakcja. |
| `SQ_R3_03` | kontrakt łowcy | Głos spod pomostu | Investigation z lokalnym water-creature slotem; research owner obowiązkowy przed lockiem istoty. |

Dependencies: R3 bible, weather, economy, bestiary research, material-culture package jednostek/rybołówstwa.

## R4 — Kamienne Wyżyny

| ID | Typ | Roboczy temat | Funkcja / trwały skutek |
|---|---|---|---|
| `SQ_R4_01` | wspólnota | Zawalona ścieżka | Traversal i dostęp do usług/POI po zmianie world state. |
| `SQ_R4_02` | kontrakt łowcy | Echo w szybie | Investigation w kopalni/jaskini; creature/anomaly composition pozostaje content lockiem. |
| `SQ_R4_03` | osobisty NPC | Kamień dla kowala | Equipment/crafting + relacja; alternatywna nagroda wiedza/usługa zamiast obowiązkowego gear power spike. |

Dependencies: R4 bible, equipment, mining/material-culture research, encounter system.

## R5 — Równiny Arel

| ID | Typ | Roboczy temat | Funkcja / trwały skutek |
|---|---|---|---|
| `SQ_R5_01` | wspólnota | Droga między obozami | Navigation i relacje; pokazuje mobilność bez stereotypowego worldbuildingu. |
| `SQ_R5_02` | osobisty NPC | Pamięć trasy | Oral knowledge/evidence; dodatkowy kontekst do świata, nie wymagany do MQ24/MQ25. |
| `SQ_R5_03` | frakcja | Gość czy zakładnik | Konflikt norm i polityki; reputacja + availability NPC. |

Dependencies: R5 bible oraz finalny culture research package. Appearance, ornament, praktyki i nazewnictwo pozostają research lockiem; karta produkcyjna nie może ich wymyślać przez analogię do „stepu”.

## R6 — Pustkowie Pierwszego Progu

| ID | Typ | Roboczy temat | Funkcja / trwały skutek |
|---|---|---|---|
| `SQ_R6_01` | badawczy | Rejestr bez właściciela | Opcjonalne archiwalne evidence; pogłębia MQ31/MQ32 bez zastępowania ich obowiązkowych revealów. |
| `SQ_R6_02` | eksploracyjny | Martwy węzeł | High-risk traversal/anomaly; opcjonalna stabilizacja zmienia lokalny hazard. |
| `SQ_R6_03` | osobisty/frakcja | Ostatni posterunek | Los ocalałej grupy; wsparcie/koszt/epilog dla Aktu V. |

Dependencies: R6 bible, central author truth, anomaly systems, MQ30+ gating. Side quest nie może ujawnić prawdy Splotu przed przewidzianym revealem MQ43.

## Przekrojowe quest chains

Trzy rodziny mogą łączyć regiony bez tworzenia obowiązkowej drugiej kampanii:

- `SQC_HUNTER_*` — kontrakty pokazujące rozwój fachu łowcy; każdy odcinek samodzielny, meta-reward dopiero po playtest locku;
- `SQC_NETWORK_ECHO_*` — opcjonalne obserwacje skutków Sieci, zawsze respektujące main-story reveal gates;
- `SQC_COMPANION_*` — osobiste questy zatwierdzonych companion slotów z `ProductionNpcRoster.md`; availability i fallback wynikają z lifecycle NPC.

Łańcuch nie może wymagać ukończenia wszystkich regionów, aby zamknąć lokalny problem. Przerwanie przez śmierć/niedostępność NPC musi mieć jawny fallback albo świadomie zapisany terminal outcome.

## Production-card queue

Kolejność od najmniejszej liczby research locków: R0 → R1 → R2 → R4 → R3 → R5 → R6. Nie oznacza kolejności kampanii. W każdym regionie najpierw karta wspólnotowa/eksploracyjna, potem kontrakt wymagający finalnego creature/research ownera, a na końcu questy zależne od kultury/frakcji.

## Minimalne QA katalogu

- każde ID jest unikalne i nie koliduje z MQ;
- każdy region R0–R6 ma co najmniej trzy planowane sloty o różnych funkcjach;
- żadna krytyczna prawda main quest nie istnieje wyłącznie w side content;
- nagroda jednorazowa nie duplikuje się po save/load;
- world-state mutation ma stabilny persistence key;
- brak wymagania konkretnej istoty, praktyki kulturowej lub assetu pozostającego research/art lockiem;
- quest po utracie opcjonalnego NPC ma fallback albo terminal outcome opisany w production card;
- quest chain respektuje reveal gates MQ00–MQ56.

## Otwarte decyzje

1. finalne tytuły, dialogi, giver IDs i personalia;
2. konkretne creature slots dla kontraktów R1/R2/R3/R4;
3. culture-locked treść R5 oraz część material culture R1/R3/R4;
4. finalne reward values, reputation/evidence thresholds i economy tuning;
5. dokładne encounter composition, POI placement i asset manifests;
6. które sloty zostaną połączone lub rozbite po playtestach;
7. finalny zakres companion chains po zamknięciu companion cast.

## Definition of Ready

Katalog zamyka planistyczny zakres side-content dla wszystkich siedmiu regionów bez zgadywania research/playtest locków. Następnym krokiem są production cards — zaczynając od R0 — zawierające pełne prerequisites, phases, outcomes, persistence, fail-forward i QA.