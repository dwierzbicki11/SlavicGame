# Research card: południca — v0.2

## Status

**Source-strength pass: zamknięty na potrzeby produkcji.** Południca pozostaje finalnym bytem folklorystycznym, ale projekt rozdziela trzy warstwy: attestację językowo-historyczną, późniejszy folklor oraz decyzję adaptacyjną SlavicGame. Karta nie twierdzi, że kompletny późny obraz południcy jest poświadczony dla IX–X wieku.

## Źródła i siła dowodu

### B08 — Zuzanna Krótki, „Nazwy demonów powietrznych w historii języka polskiego”

**Siła: H/R dla nazwy i historycznej warstwy językowej; nie H dla kompletnego wyglądu/behaviour set.**

Bezpiecznie wspiera:
- historyczną obecność nazwy w polszczyźnie;
- związek pola/południa jako rdzeń semantyczny użyteczny przy rekonstrukcji;
- ostrożność wynikającą z luk tekstowych.

Nie wspiera samodzielnie:
- konkretnego stroju, broni lub modelu twarzy;
- pełnego zestawu późnych opowieści cofniętego do IX–X wieku;
- jednej uniwersalnej mechaniki zagadki/śmierci/choroby.

### B07 — Renata Dźwigoł, „Polskie ludowe słownictwo mitologiczne”

**Siła: R dla nazewnictwa i regionalnej zmienności; późny materiał etnograficzny.**

Używamy jako kontroli wariantów i dowodu, że późniejszej tradycji nie należy spłaszczać do jednego kanonu. Nie jest to kronika wczesnośredniowieczna.

Pełne rekordy bibliograficzne i ograniczenia: `BestiarySources02.md`.

## Evidence split H / R / F / U

### H — bezpieczne historyczno-językowe minimum
- nazwa ma historyczną obecność w języku polskim;
- rdzeń pola i południa jest wystarczająco mocny, by stanowić bazę projektu;
- źródła nie pozwalają odtworzyć kompletnego IX–X-wiecznego bestiary entry.

### R — rekonstrukcja produkcyjna
- encounter wiążemy z otwartą przestrzenią uprawną i wysokim słońcem;
- sygnały środowiskowe mogą używać upału, ciszy i przerwania normalnego rytmu pracy;
- lokalne ostrzeżenia NPC mogą różnić się regionalnie i nie muszą być obiektywną prawdą świata.

### F — decyzja SlavicGame
- południca jest realnym bytem nadnaturalnym w author truth gry;
- może mieć encounter unikany zmianą czasu/trasy;
- ochronny rytuał, konkretna kara, combat pattern i evidence resolution są mechaniką F, dopóki osobny research nie uzasadni konkretnego wariantu;
- appearance pozostaje art/research lockiem: nie kodujemy jako „historycznego” popularnego późnego wizerunku białej kobiety z sierpem.

### U — nadal nieustalone
- dokładny model/strój/rekwizyt;
- czy finalny encounter wymaga walki;
- konkretne lokalne formuły, zagadki lub tabu;
- relacja do innych demonów polnych poza gameplay taxonomy.

## Chronologia

Nie przenosimy późnego kompletnego wizerunku do IX–X wieku. Historyczność nazwy i rdzenia semantycznego nie dowodzi historyczności każdego późniejszego motywu folklorystycznego.

## Region-fit lock

### R4 — Kamienne Wyżyny: **nie jako regionalny signature encounter**

R4 jest zbudowane wokół wydobycia, transportu surowców i wyżynnego krajobrazu. Południca może pojawić się tylko na wiarygodnym lokalnym skrawku uprawnym/trakcie przy polach; nie przypisujemy jej do kopalni, kamieniołomu ani górskiego biomu dla samego zapełnienia rosteru.

### R5 — Równiny Arel: **warunkowy fit, nie domyślna kultura Arelów**

Otwarta równina sama nie wystarcza. Encounter wolno umieścić wyłącznie tam, gdzie world/content data ustanawia faktyczne pole/uprawę i lokalną tradycję kompatybilną z motywem. Nie używamy południcy do „słowianizowania” Arelów ani jako dowodu ich wierzeń; w R5 może być związana z konkretną osadą, migrantami albo lokalnym pograniczem, jeśli quest/region bible to ustanowi.

### Preferowany fit

Najmocniejszy produkcyjnie jest region lub subregion z czytelnym krajobrazem rolniczym, gdzie gracz rozumie regułę `miejsce + pora dnia` bez ekspozycyjnego tekstu. Jeżeli żaden finalny POI nie spełnia tego warunku, lepiej pozostawić południcę w rosterze bez wymuszonego encounter placement niż osłabić wiarygodność świata.

## Kontrakt implementacyjny

System encounterów może bez dalszego researchu zakładać:
- gating po `time-of-day`;
- wymagany tag terenu `cultivated_field` lub równoważny;
- telegraph środowiskowy przed wejściem w encounter;
- co najmniej jedną ścieżkę avoidance/fail-forward;
- oddzielenie lokalnych belief lines NPC od author truth.

Nie może bez kolejnego locku zakładać:
- finalnego appearance;
- sierpa jako obowiązkowego atrybutu;
- konkretnej zagadki lub formuły folklorystycznej;
- uniwersalnego przypisania do R4/R5.

## Decyzja produkcyjna

**Source-strength/region-fit: PASS v0.2.** Południca pozostaje w production bestiary. Jej rdzeń implementacyjny to czas + pole + telegraph + możliwość uniknięcia. Finalne placement i appearance pozostają data/art lockiem, ale nie blokują implementacji systemu encounterów.
