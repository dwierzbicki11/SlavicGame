# NPC pierwszego wycinka — karty v0.1

Nazwy osobowe pozostają otwarte. Poniższe ID są trwałymi rolami technicznymi.

## 1. missing-family

**Rola:** osoba z rodziny zaginionego  
**Cel:** dowiedzieć się, co naprawdę się stało  
**Lęk:** że łowca zamknie sprawę bez dowodu  
**Co oferuje:** zlecenie, opis osoby, możliwy przedmiot identyfikacyjny  
**Co ukrywa:** drobny konflikt rodzinny bez znaczenia dla śmierci, by nie każda tajemnica była głównym twistem

### Wiedza początkowa

Wie:
- kiedy zaginiony wyszedł;
- dokąd miał iść;
- jaki przedmiot należał do niego.

Nie wie:
- czy światło to zmarły;
- czy drapieżnik spowodował śmierć;
- czym jest nieszczelność.

### Reakcje

**Rytuał:** wdzięczność za potwierdzenie losu.  
**Umowa:** ambiwalencja; ulga i niepokój.  
**Zniszczenie kotwicy:** negatywna reakcja, szczególnie jeśli gracz nie zebrał dowodów.

## 2. crossing-keeper

**Rola:** opiekun przeprawy  
**Cel:** przywrócić bezpieczny ruch  
**Lęk:** utrata dochodu i izolacja regionu  
**Co oferuje:** informacje o drodze i fizycznych śladach  
**Bias:** interpretuje problem przede wszystkim praktycznie

### Wiedza

Wie:
- gdzie uszkodzona jest przeprawa;
- kiedy widziano światło;
- że zwierzęta zachowują się inaczej.

Może błędnie zakładać:
- że jedno zagrożenie odpowiada za wszystko.

### Reakcje

**Rytuał:** pozytywna, jeśli ruch wraca do normy.  
**Umowa:** ostrożnie negatywna, jeśli nocne ograniczenia pozostają.  
**Zniszczenie kotwicy:** pozytywna krótkoterminowo, jeśli światło znika.

## 3. herbalist

**Rola:** zielarka  
**Cel:** ochronić ludzi bez bezmyślnego niszczenia mokradeł  
**Lęk:** że strach doprowadzi do wypalenia/zniszczenia obszaru  
**Co oferuje:** składniki, recepturę, obserwacje biologiczne

### Wiedza

Potrafi rozróżnić:
- fizyczne obrażenia;
- nietypowe zachowanie zwierząt;
- część śladów związanych z drapieżnikiem.

Nie zna pełnej metafizyki zjawy.

### Reakcje

**Rytuał:** najbardziej pozytywna.  
**Umowa:** neutralna/ostrożnie pozytywna, jeśli warunki są kontrolowane.  
**Zniszczenie kotwicy:** zależna od szkód w środowisku i sposobu działania gracza.

## 4. community-guard

**Rola:** strażnik wspólnoty  
**Cel:** ograniczyć zagrożenie dla mieszkańców  
**Lęk:** kolejna osoba zaginie  
**Co oferuje:** informacje o bezpieczeństwie, nocnych patrolach i świadkach  
**Bias:** preferuje rozwiązanie łatwe do egzekwowania

### Wiedza

Wie:
- kto zgłaszał zdarzenia;
- jakie drogi zamknięto;
- czy ktoś widział fizycznego napastnika.

Nie musi wierzyć w wyjaśnienie nadnaturalne.

### Reakcje

**Rytuał:** akceptuje wynik, jeśli zagrożenie znika.  
**Umowa:** wymaga jasnych reguł nocnego ruchu.  
**Zniszczenie kotwicy:** praktycznie pozytywna, jeśli problem nie wraca.

## 5. shrine-keeper

**Rola:** opiekun miejsca kultowego  
**Cel:** zachować reguły Kamiennego Kręgu i ograniczyć niekontrolowane przejścia  
**Lęk:** że ktoś wykorzysta zjawisko bez zrozumienia  
**Co oferuje:** wiedzę o rytuale i opcjonalnym kontakcie boskim  
**Bias:** interpretuje wydarzenia przez swoją tradycję, która nie jest automatycznie pełną prawdą autorów

### Wiedza

Wie:
- jak wykonać bezpieczniejszy rytuał;
- kiedy miejsce jest aktywne;
- jakie znaki są używane w lokalnej praktyce F.

Nie wie automatycznie:
- czym jest Czwarta Sfera;
- kto wywołał globalny kryzys.

### Reakcje

**Rytuał:** pozytywna.  
**Umowa:** zależy od warunków umowy.  
**Zniszczenie kotwicy:** krytyczna, jeśli gracz zrobił to bez rozpoznania.

## Harmonogram v0.1

| NPC | Dzień | Noc |
|---|---|---|
| missing-family | dom / okolice wsi | dom |
| crossing-keeper | przeprawa | wieś |
| herbalist | warsztat / zbieranie blisko wsi | dom |
| community-guard | patrol dzienny | nocna warta |
| shrine-keeper | Kamienny Krąg | wieś / wyjątkowo krąg podczas eventu |

## Minimalne grafy dialogowe

Każdy NPC potrzebuje:
- rozmowy przed questem;
- rozmowy w Investigation;
- jednej reakcji po znalezieniu ważnego dowodu;
- reakcji po każdym z trzech rozwiązań;
- fallbacku po zakończeniu questa.

## Relacje

Vertical slice testuje przede wszystkim:
- Trust dla herbalist;
- Trust dla missing-family;
- reputację old-village.

Nie tworzymy jeszcze pełnego systemu romansów/towarzyszy w tej piątce.

## Asset needs

Każdy NPC potrzebuje docelowo:
- modelu/sylwetki;
- wariantu stroju;
- 3–5 podstawowych animacji;
- portretu tylko jeśli UI go wymaga;
- ikony interakcji;
- głosu opcjonalnie.

## Otwarte

- imiona;
- wiek;
- wygląd;
- dokładne pochodzenie;
- VO;
- finalne dialogi literackie.


## Runtime settler model pass

Pierwszy grywalny pass modeli mieszkańców korzysta ze wspólnego animowanego rigu humanoida, ale nie pokazuje już wszystkich NPC jako identycznej postaci.

### Pięć głównych NPC
Każda rola ma osobny `NpcVisualProfile`:
- inne proporcje ciała;
- własną przygaszoną paletę bazową;
- kolor akcentu;
- 1–2 dodatki sylwetki;
- lekko różne tempo animacji.

Dodatki rozpoznawcze:
- `missing-family` — chusta/szal + mała sakiewka;
- `crossing-keeper` — kij + zestaw narzędzi;
- `herbalist` — kaptur/chusta głowy + torba;
- `community-guard` — włócznia + sakiewka pasa;
- `shrine-keeper` — kaptur + kij.

Dodatki są generowane jako bardzo lekka geometria w istniejącym actor mesh passie, więc uczestniczą w tym samym depth/shadow path co postacie i nie wymagają osobnych draw calli.

### Ambient population
Żarnowiec otrzymuje dodatkowo 14 mieszkańców:
- dwóch rolników;
- cieślę;
- garncarza;
- handlarza;
- tragarza;
- starszego mieszkańca;
- podróżnego;
- pomocnika kowala;
- tkaczkę;
- pasterza;
- zbieraczkę;
- rybaka;
- młodego mieszkańca biegającego z drobnymi sprawami.

Mają osobne pozycje dzienne/nocne, role, proporcje, kolory i zestawy dodatków. Nowe sylwetki wykorzystują dodatkowo lekki proceduralny fartuch, wędkę i tobołek na ramię. W obecnym passie są widoczni i animowani, ale nie dostają fałszywego promptu dialogowego, dopóki nie powstanie dla nich właściwy graph rozmowy.

### Animation policy
Wspólny rig zachowuje istniejące klipy. Aktywności typu patrol, noszenie towaru lub przybycie do wsi korzystają z `Walk`; pozostałe prototypowo z `Idle`. Docelowe animacje pracy nadal pozostają P1.

### Research / art status
Ten pass jest **F / placeholder production art**, nie rekonstrukcją historycznego ubioru. Finalne stroje, fryzury, biżuteria, obuwie i wyposażenie muszą przejść osobny material-culture research lock przed oznaczeniem jako historycznie/reconstructed based.


## Runtime local routines pass

NPC nie stoją już wyłącznie w jednym punkcie harmonogramu.

`NpcRoutineMotion` daje deterministyczne lokalne trasy dla aktywności dziennych:
- `community-guard` — obchód obwodu Żarnowca;
- `missing-family` — krótka trasa szukania po wsi;
- `crossing-keeper` — obejście przeprawy;
- `herbalist` — ruch między stanowiskiem a miejscem przygotowań;
- `shrine-keeper` — obchodzenie Kamiennego Kręgu;
- rolnicy — dojście do pola i lokalne ścieżki pracy;
- cieśla/garncarz/handlarz — krótkie pętle stanowisk pracy;
- tragarz — droga magazyn/targ;
- starszy mieszkaniec — spacer po placu;
- podróżny — wejście od bramy do targu.

### Locomotion
Runtime aktora ma pole `IsMoving`. Gdy trasa przesuwa NPC, renderer wybiera klip `Walk`; przy postoju używany jest `Idle`.

### Dialogue freeze
Jeśli gracz rozpocznie rozmowę:
- rozmówca zachowuje ostatnią pozycję;
- `IsMoving=false`;
- obraca się do gracza;
- po zamknięciu dialogu wraca do normalnej rutyny.

Pozostali mieszkańcy kontynuują swoje trasy.

### Budżet
To nadal system bez navmesha i bez fizyki crowd:
- ręcznie dobrane krótkie ścieżki;
- deterministyczna interpolacja;
- proste resolve względem istniejących obstacle;
- brak osobnych pathfinding workerów.

Docelowy system nawigacji może później zastąpić tylko sampler tras bez zmiany kontraktu harmonogramu NPC.


## Runtime distinct NPC model families

Warstwa mieszkańców nie korzysta już z geometrii `player_hunter_animated.glb` jako wspólnego modelu dla wszystkich NPC.

Runtime ładuje i współdzieli pięć istniejących animowanych rodzin humanoidów:
- `npc_villager_a_animated.glb`;
- `npc_villager_b_animated.glb`;
- `npc_hunter_animated.glb`;
- `npc_merchant_animated.glb`;
- `npc_elder_animated.glb`.

Przypisanie jest deterministyczne i zależy od trwałego ID/roli NPC. Przykładowo:
- strażnik wspólnoty i opiekun przeprawy używają bardziej użytkowej sylwetki `npc_hunter`;
- shrine-keeper i starszy mieszkaniec używają `npc_elder`;
- handlarz i podróżny korzystają z `npc_merchant`;
- rolnicy, garncarz, tragarz i część głównych NPC są rozdzieleni między `villager_a` i `villager_b`.

Każda baza zachowuje własną geometrię i animacje `Idle/Walk`, a istniejące `NpcVisualProfile` nadal nakłada:
- różne proporcje;
- paletę;
- tempo animacji;
- lekkie proceduralne dodatki sylwetki.

Modele są ładowane po jednym egzemplarzu na rodzinę i współdzielone przez wiele instancji NPC. Dzięki temu 19 aktywnych NPC nie wymaga 19 kopii danych GLB. Ambientowa geometria mieszkańców jest dodatkowo pomijana poza 95 m od obserwatora.


## Runtime work-animation pass

NPC nie używają już wyłącznie `Idle` podczas pracy.

Polityka klipów:
- ruch po trasie: `Walk`;
- praca stacjonarna: `Interact`;
- odpoczynek/dom/bezczynność: `Idle`.

Na `Interact` przechodzą m.in.:
- `field-work`;
- `wood-work`;
- `craft-work`;
- `market-trade`;
- `trade-and-prepare`;
- `maintain-crossing`;
- `tend-shrine`;
- `home-and-search`.

Jeżeli NPC rzeczywiście porusza się w ramach danej aktywności, locomotion ma pierwszeństwo i renderer używa `Walk`. Po zatrzymaniu w punkcie pracy przechodzi na `Interact`.

Wszystkie pięć używanych rodzin NPC GLB jest walidowanych pod kątem obecności `Idle`, `Walk` i `Interact`.


## Runtime ambient community dialogue pass

The fourteen ambient settlers are no longer presentation-only actors. They now use the authored `DLG_R0_COMMUNITY` package and expose `E POROZMAWIAJ` through the same proximity contract as core NPCs.

Ambient start-node selection reacts to:
- current time of day;
- rain/storm state;
- active swamp investigation;
- physical predator resolution;
- apparition release;
- complete two-cause resolution.

The lines remain local observations and opinions. They do not grant unique mandatory evidence or mutate quest phase, so ignoring every ambient settler cannot block vertical-slice progression.

Dialogue freeze still affects only the current speaker; every other resident keeps following the local routine system.


## Runtime severe-weather routine pass

Ambient settlers now react physically to severe weather instead of only commenting on it.

When the active weather reaches `Storm` (or an equivalent rain intensity >= 0.85):
- all fourteen ambient Żarnowiec settlers switch from their current work/travel activity to `shelter-storm`;
- each NPC uses its own short deterministic route toward a nearby sheltered part of the village;
- residents remain spatially distributed instead of stacking at one shelter point;
- actor locomotion continues to use existing `Walk`/idle behavior and the same obstacle resolution;
- core quest NPCs keep their authored schedules so storm behavior cannot silently remove a progression-critical teacher/quest giver.

Ordinary `Rain` does not trigger full evacuation. When severe weather ends, ambient residents return to their normal schedule-selected activities on the next runtime update.

This is intentionally a lightweight situational override rather than a navmesh/crowd simulation.


## Runtime population expansion pass

Drugi pass populacji zwiększa ambient Żarnowca z 8 do 14 postaci bez dodawania kolejnych bazowych GLB.

Nowe role:
- `settler-smith-helper-01` — pomocnik kowala;
- `settler-weaver-01` — tkaczka;
- `settler-shepherd-01` — pasterz;
- `settler-gatherer-01` — zbieraczka;
- `settler-fisher-01` — rybak pracujący rano przy mokradłach/przeprawie;
- `settler-youth-01` — młody mieszkaniec roznoszący drobne sprawy po osadzie.

Każda z tych postaci ma własny profil proporcji/palety/akcesoriów, harmonogram, lokalną trasę pracy, reakcję na burzę i opcjonalny graf `DLG_R0_COMMUNITY`. Wszystkie pozostają contentem F/placeholder art do późniejszego research locku ubioru.


## Local crowd steering pass

R0 nadal nie używa pełnego navmesha ani fizyki crowd, ale NPC nie mogą już bezkarnie zajmować dokładnie tej samej przestrzeni.

`NpcCrowdSteering` działa po wyliczeniu deterministic routine pose:
- dwa krótkie przebiegi solvera;
- minimalny dystans centrum NPC: około 0.76 m;
- maksymalny korekcyjny push per pass: 0.40 m;
- korekta po każdym pushu przechodzi ponownie przez `WorldState.ResolveHorizontalPosition`, więc respektuje statyczne przeszkody i granice terenu;
- ruchomy NPC utrzymuje około 0.72 m przestrzeni od gracza;
- NPC aktualnie prowadzący dialog jest chroniony i nie jest przesuwany przez crowd solver;
- fallback dla idealnie nakładających się pozycji jest deterministyczny z ID postaci, bez losowości per-frame.

To pozostaje rozwiązaniem low-cost dla obecnej populacji kilkunastu mieszkańców. Złożoność par to O(n²), ale przy R0 jest to kilkaset prostych testów dystansu na update, bez path query, bez rigid-body solvera i bez navmesh rebuild.

Przejście do spatial grid/navmesh jest wymagane dopiero po pomiarze, jeśli docelowa gęstość regionów przekroczy budżet tego prostego solvera.
