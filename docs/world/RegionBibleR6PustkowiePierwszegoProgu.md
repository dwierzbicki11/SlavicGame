# R6 — Pustkowie Pierwszego Progu: production region bible v0.1

## Status i rola

R6 jest obowiązkowym regionem Aktu III i późniejszego finału. To miejsce, w którym wcześniejsze dowody polityczne, rodzinne, boskie i materialne po raz pierwszy mogą zostać zestawione w jeden model wydarzeń Nocy Zamkniętego Progu.

Pustkowie nie jest martwą pustynią ani losowym postapokaliptycznym biomem. MacroGeography definiuje rzadkie osadnictwo, ruiny, niestabilne miejsca, ślady starszych systemów i konflikty ekspedycji. Region ma wyglądać jak przestrzeń historycznie użytkowana, częściowo opuszczona i ponownie eksploatowana.

## Cele produkcyjne

R6 ma dostarczyć:
- kulminację śledztwa prowadzonego od R0;
- czytelny kontrast między oficjalną/fałszywą historią a evidence w świecie;
- pierwszą przestrzeń, w której sieć dawnych progów jest badana jako system;
- konflikt kilku ekspedycji/frakcji o interpretację i kontrolę miejsca;
- wejście w prawdę o Nocy Zamkniętego Progu bez przedwczesnego ujawniania całego finału;
- region możliwy do ponownego użycia w Akcie V w wyraźnie zmienionym stanie.

## Topologia

### A. Korytarz wejściowy

Ostatnia względnie bezpieczna trasa prowadząca z zamieszkanych ziem. Zawiera punkt logistyczny, ostrzeżenia o stanie szlaków i pierwszy kontakt z ekspedycjami.

Funkcja:
- bezpieczny punkt powrotu;
- dostawy i naprawy w ograniczonym zakresie;
- przedstawienie konfliktu interpretacji miejsca.

### B. Pas rzadkiego osadnictwa

Niewielkie, trwałe lub półtrwałe skupiska ludzi żyjących na obrzeżu Pustkowia. Nie wszyscy są członkami ekspedycji.

Funkcja:
- pokazanie, że region nie jest pusty;
- lokalna pamięć niezależna od archiwów politycznych;
- side content i konsekwencje działań ekspedycji.

### C. Strefa ruin i dawnych tras

Warstwa materialnych pozostałości prowadzących ku Pierwszemu Progowi. Ruiny nie mogą być generowane jako przypadkowy zestaw monumentalnych budowli; konkretne formy wymagają zgodności z istniejącym research/content lockiem.

### D. Strefy niestabilne

Miejsca, w których granice i zachowanie świata odbiegają od normalnych reguł. Każda anomalia musi mieć:
- sygnał ostrzegawczy;
- deterministyczny stan;
- co najmniej jedną drogę wycofania;
- zapis/odtworzenie bez zmiany wyniku;
- jawny owner w systemie odpowiedzialnym za jej stan.

### E. Pierwszy Próg

Centralny węzeł śledztwa i późniejszego finału. W Akcie III gracz nie otrzymuje jeszcze pełnej kontroli nad mechanizmem. Dostęp jest warstwowy i zależy od evidence, nie od arbitralnego level gate.

### F. Zaplecza ekspedycji

Co najmniej dwa konkurencyjne punkty operacyjne lub reprezentacje interesów. Ich dokładna liczba zależy od finalnego rosteru frakcji, ale gameplay musi pozwalać na obserwację konfliktu, współpracę ograniczoną oraz odmowę podporządkowania się jednej stronie.

## Traversal i bezpieczeństwo

R6 korzysta z mapy, trackingu, weather/day-night oraz wcześniejszych kontraktów navigation/anomaly.

Zasady:
- wejście główną trasą jest zawsze recoverable;
- skróty i głębsze strefy mogą wymagać evidence lub lokalnej wiedzy;
- anomalia nie może teleportować gracza w nieodwracalny fail state;
- zamknięcie trasy przez stan fabularny musi otwierać jawny fallback;
- fast travel nie omija obowiązkowych progów śledztwa;
- traversal w Akcie V może różnić się od Aktu III, ale wcześniej odkryte dane pozostają zachowane.

## Evidence architecture

R6 nie tworzy prawdy od zera. Konsumuje pakiety z wcześniejszych regionów.

Minimalne rodziny evidence:
1. **political** — dokumenty, decyzje lub ślady manipulacji pamięcią;
2. **family** — materiał wiążący rodzinę bohatera z wydarzeniami;
3. **divine** — wiedza/świadectwo dotyczące udziału sił boskich, z zachowaniem niepewności źródła;
4. **material** — fizyczne dane o budowie i funkcji dawnej sieci;
5. **local** — pamięć ludzi i miejsca, która pozwala zweryfikować rozbieżności.

Main path musi działać z minimalnym zestawem obowiązkowym zdefiniowanym przez quest cards. Opcjonalne evidence może zmieniać pewność, dialogi i interpretację, ale nie może tworzyć softlocku.

## Główne śledztwo Aktu III

### Wejście

Gracz przybywa z hipotezą, że dawne miejsca tworzą sieć, a zachowane mapy i relacje są niepełne.

### Przebieg

1. Porównuje wcześniejsze pakiety evidence z fizycznym stanem Pustkowia.
2. Ustala, które elementy oficjalnej historii są sprzeczne z materiałem terenowym.
3. Identyfikuje funkcję co najmniej jednego elementu systemu Pierwszego Progu.
4. Konfrontuje konkurencyjne interpretacje ekspedycji/frakcji.
5. Odtwarza wystarczającą sekwencję Nocy Zamkniętego Progu, by ujawnić właściwy kierunek odpowiedzialności.
6. Uzyskuje wskazanie antagonisty/frakcji zgodnie z `Antagonists.md` i planem reveal, bez przepisywania author truth do dialogu ekspozycyjnego.

### Wyjście

Gracz kończy Akt III z potwierdzoną wersją kluczowych wydarzeń oraz powodem, by przejść do Nawii i śladu Czwartej Sfery. Nie zna jeszcze wszystkich konsekwencji potrzebnych do decyzji finałowej.

## Frakcje i ekspedycje

Region powinien reprezentować co najmniej trzy rodzaje interesu:
- kontrola polityczna/strategiczna;
- badanie lub odzyskanie wiedzy;
- lokalne przetrwanie i ograniczenie szkód.

Nazwy, liczba i personalny roster pochodzą z istniejących dokumentów frakcji/antagonistów i przyszłych quest cards. Region bible nie tworzy nowej centralnej frakcji ad hoc.

## Encounter families

1. **Expedition encounter** — negocjacja, kontrola dostępu, wymiana informacji.
2. **Ruin/evidence encounter** — rekonstrukcja zdarzenia lub funkcji miejsca.
3. **Anomaly encounter** — bezpieczne rozpoznanie niestabilnego obszaru.
4. **Local-memory encounter** — relacja lub ślad podważający wersję oficjalną.
5. **Resource/logistics encounter** — ograniczone zapasy, uszkodzona trasa, ewakuacja.
6. **Supernatural encounter** — wyłącznie z zatwierdzonego rosteru i zgodny z revelation gating.

Każdy obowiązkowy encounter ma fallback niebojowy lub alternatywną ścieżkę rozwiązania, jeśli walka nie jest jego jawnie zdefiniowanym celem.

## Side content

Kategorie:
- zaginiony członek ekspedycji;
- spór o zabezpieczenie ruin;
- pomoc mieszkańcom dotkniętym działaniem ekspedycji;
- błędna interpretacja znaleziska;
- odzyskanie logistycznego szlaku;
- lokalna historia miejsca, która poszerza obraz bez zastępowania main evidence.

Side quest nie może ujawniać author truth wcześniej niż przewiduje `RevelationPlan.md`.

## Persistence

Zapisywany stan regionu obejmuje co najmniej:
- odkryte landmarki i trasy;
- aktywowane/ustabilizowane anomalie;
- campaign evidence i jego źródła;
- relacje/stan ekspedycji istotne dla questów;
- ukończone encountery i side questy;
- stan dostępu do Pierwszego Progu;
- trwałe skutki decyzji;
- osobny late-game/finale state.

Load nie może ponownie przyznawać evidence, odtwarzać jednorazowych encounterów ani resetować stanu anomalii.

## Akt V — powrót i finał

R6 jest ponownie używany jako przestrzeń finałowa. Powrót powinien zmieniać:
- bezpieczeństwo i dostępność tras;
- obecność/stan frakcji;
- aktywność anomalii;
- dostęp do Pierwszego Progu;
- reakcje na wiedzę i decyzje gracza.

Finał nie jest pojedynczym liniowym bossem. Region musi umożliwić warianty wynikające z polityki, patronów, rodziny, wiedzy i stanu mechanizmu. Szczegółowy branching pozostaje własnością `Finale.md`, `EndingVariants.md` i quest cards Aktu V.

## Asset families

Minimalne rodziny:
- modularne ruiny i pozostałości dawnych tras;
- skromne zabudowania obrzeża;
- zestawy obozów/ekspedycji;
- propsy logistyczne i badawcze zgodne z settingiem;
- landmarki Pierwszego Progu;
- warianty terenu zdegradowanego bez estetyki pustyni;
- VFX/SFX anomalii o czytelnych telegraphach;
- elementy blokad, zabezpieczeń i śladów wcześniejszej działalności;
- warianty late-game tych samych rodzin zamiast budowania drugiego regionu od zera.

Unikalne assety centralnego mechanizmu powstają dopiero po jego art/technical locku.

## Streaming i performance

Region wymaga:
- streamingu ruin i obozów jako osobnych komórek/stref;
- limitów aktywnych efektów anomalii;
- odłączania symulacji od dalekiej reprezentacji ekspedycji;
- kontrolowanych shadow/VFX budgets;
- profilowania zarówno Aktu III, jak i cięższego stanu finałowego.

Konkretne liczby pozostają otwarte do pomiarów na docelowym rendererze.

## QA acceptance v0.1

Region jest implementowalny, jeśli:
1. główny evidence flow można ukończyć bez side contentu;
2. brak opcjonalnego evidence nie tworzy softlocku;
3. każda obowiązkowa anomalia ma telegraph, recovery i poprawny save/load;
4. oficjalna historia może zostać podważona przez jawne, identyfikowalne źródła evidence;
5. konflikt ekspedycji nie wymusza jednej frakcji bez alternatywy zapisanej w questach;
6. wejście do Pierwszego Progu jest deterministycznie gate'owane;
7. powrót w Akcie V zachowuje stan Aktu III;
8. region nie ujawnia prawdy Czwartej Sfery przed Aktem IV;
9. finałowe warianty mogą wykorzystać ten sam region bez resetu świata;
10. żadna niezweryfikowana forma archeologiczna nie jest przedstawiona jako fakt historyczny.

## Otwarte decyzje

- dokładna liczba osad obrzeża i obozów ekspedycji;
- finalny roster NPC i reprezentowanych interesów;
- szczegółowa forma ruin po content/art research locku;
- finalny roster supernatural/wildlife;
- konkretne questowe progi minimalnego evidence;
- geometria i art language Pierwszego Progu;
- parametry anomalii po playtestach;
- liczba side questów;
- dokładna macierz zmian między Aktem III i V;
- targety performance po pomiarach.

Żadna z tych decyzji nie blokuje implementacji topologii, persistence contractu ani szkieletu evidence flow v0.1.