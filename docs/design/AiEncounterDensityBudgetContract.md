# AI / encounter density budget contract

Status: production planning v0.1  
Owner: gameplay/AI + world/encounter + performance  
Scope: R0–R6, shared runtime

## Cel

Ten dokument definiuje **kontrakt kosztu i degradacji** dla AI oraz encounter density bez wymyślania docelowej liczby aktywnych agentów ani CPU ms przed profilowaniem. `EncounterDesign.md` i `RegionalEncounterRosters.md` pozostają ownerami zachowania i zawartości; tutaj zamykamy sposób planowania, telemetrii i bezpiecznej redukcji kosztu.

## Zasady nadrzędne

1. Gęstość świata nie jest liczbą NPC na region. Jest wynikiem miejsca, stanu świata, czasu, pogody, questów, widoczności gracza i aktualnego budżetu runtime.
2. Quest-critical, combat-critical i evidence-critical aktorzy nie mogą zniknąć wskutek presji budżetu.
3. Symulacja poza aktywną strefą może być upraszczana, ale logiczny stan/persistence nie może zależeć od tego, czy obiekt jest aktualnie załadowany.
4. Nie blokujemy implementacji na arbitralnym limicie `N agents`. Finalne limity są `TBD-MEASURED`.
5. Encounter director nie może kompensować niskiego kosztu przez spawn spam ani naruszać cooldown/one-shot/persistence kontraktów encounterów.

## Klasy kosztu AI

| Klasa | Znaczenie | Typowe przykłady | Dozwolona degradacja |
|---|---|---|---|
| A0 | logic/presence only | odległy vendor, remembered NPC, inactive quest actor | event-driven state, brak ciągłego ticku |
| A1 | lightweight ambient | cywil, prosty animal/ambient actor | rzadsze sensory/update, prostszy locomotion |
| A2 | gameplay active | patrol, follower, standard hostile, investigation actor | ograniczenie dalekich sensorów i noncritical planning |
| A3 | critical/high-cost | boss, scripted pursuit, złożona grupa combat, companion w krytycznej scenie | tylko kosmetyczne redukcje; gameplay contract chroniony |

Klasa jest deklaracją planistyczną, nie gwarantowanym kosztem. Każdy profil musi zostać zweryfikowany pomiarem.

## Strefy symulacji

Runtime rozróżnia co najmniej:

- `OFFLINE` — tylko trwały stan i zdarzenia coarse-grain;
- `BACKGROUND` — uproszczona symulacja bez pełnych sensorów/pathfindingu;
- `NEAR` — ograniczona częstotliwość aktualizacji, gotowość do promocji;
- `ACTIVE` — pełne zachowanie wymagane przez gameplay;
- `CRITICAL` — jawnie chroniona scena/encounter/quest window.

Promocja i degradacja muszą być deterministyczne względem wymaganych flag świata oraz bezpieczne dla save/load. Nie wolno opierać quest completion na tym, że konkretny update tick zdążył wykonać się przed unloadem.

## Encounter density director

Każdy kandydat encounteru jest filtrowany co najmniej przez:

`region -> worldState -> place/POI -> quest gates -> time/weather -> cooldown/oneShot -> residency -> budget admission`.

Budget admission może **odroczyć** encounter ambient/cosmetic, ale nie może bezpośrednio oznaczyć go jako completed. Dla wymaganej sceny questowej director ma użyć rezerwacji budżetu lub jawnego fallbacku scenariusza.

### Priorytety admission

1. `critical` — quest progression, obowiązkowy combat/evidence/accessibility cue;
2. `gameplay` — standard encounter mający znaczenie systemowe;
3. `ambient` — budowanie życia regionu;
4. `cosmetic` — crowd/background flavor.

Przy presji odrzucamy/odraczamy od dołu. Nie redukujemy czytelności walki, telegraphów ani wymaganej informacji.

## Grupy i pathfinding

- grupa posiada jeden jawny encounter/group owner zamiast niezależnego spawnowania każdego członka;
- dalekie cele mogą współdzielić coarse route/goal, jeśli nie zmienia to obserwowalnego gameplayu;
- path requesty muszą mieć kolejkę i priorytet; brak natychmiastowej ścieżki nie może generować tight retry loop;
- stuck recovery ma limit prób i fallback;
- navmesh/graph streaming musi współpracować z `StreamingMemoryBudgetContract.md` — agent nie może wymuszać permanent residency całego regionu.

## Sensory, planning i update cadence

System powinien mierzyć osobno co najmniej:

- sensory/perception;
- decision/planning/behavior tree lub odpowiednik;
- navigation/path queries;
- locomotion/avoidance;
- combat targeting;
- encounter director/spawn admission;
- persistence promotion/demotion.

Cadence może zależeć od klasy i strefy. Gameplay-critical reakcje w ACTIVE/CRITICAL mają jawny maksymalny latency target dopiero po pomiarach; nie wpisujemy go teraz arbitralnie.

## Pressure policy

Współpracuje ze stanami `NORMAL`, `PRESSURE_1`, `PRESSURE_2`, `CRITICAL` z `StreamingMemoryBudgetContract.md`.

- `NORMAL`: pełny dopuszczony mix encounterów.
- `PRESSURE_1`: redukcja cosmetic, rzadsze background/near updates, ograniczenie nowych ambient admissions.
- `PRESSURE_2`: brak nowych cosmetic, silne ograniczenie ambient, uproszczenie A0/A1 i noncritical sensorów A2.
- `CRITICAL`: utrzymujemy tylko wymagane gameplay/quest/combat oraz minimalne bezpieczne tło; nowe niekrytyczne encountery są odraczane.

Presja nie może usuwać już zdobytego evidence, resetować reputacji, zmieniać outcome questów ani despawnować aktora aktualnie potrzebnego do zakończenia krytycznej interakcji.

## Regionalne scenariusze pomiarowe

| Region | Scenariusz reprezentatywny |
|---|---|
| R0 | hub/wioska: NPC usługowi + ambient + aktywny quest + przejście do walki |
| R1 | gród/przeprawa: patrol, kontrola, cywile i route traffic |
| R2 | las: ograniczona widoczność, guardian/misdirection, tracking i path queries |
| R3 | port/woda: vendorzy, transport, water actor i mixed shore navigation |
| R4 | worksite/open ground: workers + hostile/event group + weather interaction |
| R5 | route/camp: camp population, traveler/encounter admission i traversal |
| R6 | finał: scripted critical actors + anomaly/event load; ambient ma najniższy priorytet |

Dodatkowo obowiązuje test traversalu przez kilka cells/region transitions, aby wykrywać narastające kolejki pathfindingu, niezwolnione agent state i spawn/despawn thrashing.

## Telemetria obowiązkowa

Każdy profil reprezentatywny powinien raportować co najmniej:

- liczba agentów per A0–A3 i OFFLINE/BACKGROUND/NEAR/ACTIVE/CRITICAL;
- CPU time per AI subsystem oraz frame percentile;
- liczba perception queries i path requests; queue depth/latency;
- encounter candidates/admitted/deferred/rejected z powodem;
- promotions/demotions i churn per minute;
- stuck recoveries/path failures;
- aktywne grupy i ich rozmiary;
- memory footprint stanu AI, jeśli mierzalny;
- pressure-state transitions.

## Measurement gate

Następujące wartości pozostają `TBD-MEASURED`:

- maksymalna rekomendowana liczba ACTIVE/NEAR agentów;
- liczba agentów per encounter/group;
- CPU ms/percentile dla AI;
- perception/pathfinding query ceilings;
- update cadence per class/zone;
- spawn density i cooldown numbers;
- crowd/background density per region;
- hardware-dependent quality scaling.

Lock liczbowy wymaga pomiaru co najmniej scen R0–R6, sceny najgorszego przypadku oraz długiego traversalu. Wynik musi podać sprzęt, build, ustawienia, scenę i percentile; sama średnia FPS nie wystarcza.

## Minimalne QA

- budget pressure nie blokuje main/side quest completion;
- save/load zachowuje logical state niezależnie od strefy symulacji;
- odroczony encounter nie staje się completed;
- one-shot/cooldown nie resetuje się przez unload/reload;
- quest-critical actor nie jest usuwany przez density scaling;
- path failure nie powoduje nieskończonego retry/spike;
- długi traversal nie zwiększa stale agent/path queue/memory;
- quality preset może zmniejszać tło, ale nie outcome ani wymagane informacje.

## Stan locku

**PASS v0.1 dla implementacji i profilowania.** Architektura klas kosztu, admission, pressure/degradation, telemetria i measurement gate są zamknięte. Finalne liczby pozostają jawnie otwarte do P5 i nie blokują implementacji systemów AI/encounter.