# R0 — Pogranicze Żarnowca: production region bible v0.1

## Rola i owner

- **Region ID:** `R0`
- **Nazwa:** Pogranicze Żarnowca
- **Owner spec:** ten dokument
- **Funkcja kampanii:** Akt 0 / onboarding zawodu łowcy, pierwszy pełny regionalny loop i punkt odniesienia dla późniejszych powrotów.
- **Powiązania:** `MacroGeography.md`, `CampaignRegionalFlow.md`, `MainQuestSkeleton.md`, `VerticalSlice.md`, karty lokacji i questów vertical slice.

Ten dokument nie zmienia owner specs systemów ani questów. Określa, jak istniejące reguły składają się w produkcyjny region.

## Granice i topologia

R0 jest małym, gęstym regionem na styku trzech czytelnych krawędzi:

1. **droga do Nadborza** — główne wyjście kampanii i najbardziej kontrolowany szlak;
2. **skraj Wielkiego Boru** — naturalna granica zachodnia/północno-zachodnia, z lokalnymi ścieżkami zamiast pełnego wejścia do R2;
3. **mokradła odpływów Dębrzy** — miękka, niebezpieczna granica terenu i główny obszar supernatural vertical slice.

Topologia produkcyjna ma formę zwartego huba z krótkimi odnogami, nie miniaturowego seamless kontynentu. Żarnowiec jest kotwicą usług i dialogów; las, mokradło oraz Kamienny Krąg tworzą trzy odmienne kierunki eksploracji. Droga do Nadborza jest początkowo funkcjonalną krawędzią kampanii, a nie niewidzialną ścianą bez uzasadnienia.

## Settlements i POI

### `LOC-ZARNOWIEC` — Żarnowiec

Główna osada i home base Aktu 0. Musi zawierać co najmniej:
- miejsce odpoczynku/save;
- podstawowego vendora/usługę;
- przestrzeń dla pięciu NPC vertical slice;
- czytelne wyjścia na drogę, las i mokradła;
- powierzchnie do późniejszego pokazania konsekwencji decyzji.

### Puszcza Żywia

Leśna strefa przejściowa. Funkcje:
- tracking tutorial i pierwsze tropy;
- kontrast bezpieczeństwa znanych ścieżek z niepewnością głębszego lasu;
- pierwszy kontakt z motywem lokalnego strażnika lasu bez zamrażania jego finalnej regionalnej nazwy.

### Czarne Mokradła

Główna strefa zagrożenia vertical slice. Funkcje:
- quest `Światło nad mokradłem`;
- pierwszy predator/zjawa i rytuał;
- mgła, woda, grząski teren i noc jako czytelne modyfikatory nawigacji;
- miejsce, w którym supernatural ma być obserwowalne przed pełnym wyjaśnieniem.

### Kamienny Krąg

Stare miejsce o znaczeniu rytualnym i evidence/reveal. Nie należy przedstawiać go automatycznie jako historycznie udokumentowanej „świątyni”; funkcja świata gry jest ważniejsza od nieuzasadnionej etykiety historycznej.

### Droga do Nadborza

Korytarz wyjściowy R0. Po spełnieniu warunków kampanii przekazuje gracza do R1. Musi zachować logiczny dostęp lokalny przed odblokowaniem podróży, ale nie pozwalać ominąć kontraktu MQ00/MQ01.

## Biome i landmark language

R0 powinien być rozpoznawalny przez zestaw powtarzalnych rodzin wizualnych:
- wilgotne łąki i rowy odpływowe bliżej mokradeł;
- pola, płoty, drogi gruntowe i ślady codziennej gospodarki wokół osady;
- gęstniejący drzewostan, martwe drewno i ograniczone linie widzenia przy skraju boru;
- kamień i starsze, niecodzienne układy terenu przy miejscach rytualnych.

Landmarki mają wspierać nawigację bez ciągłego HUD: sylweta Żarnowca, linia boru, charakterystyczne mokradłowe drzewa/rozlewiska i Kamienny Krąg powinny dawać orientację z kilku podejść.

## Kultura, władza, religia i gospodarka

R0 należy do peryferyjnego kontekstu Nadborza, ale codzienna kontrola jest lokalna i praktyczna. Autorytet powinien być widoczny przez obowiązki, bezpieczeństwo drogi, spory o zasoby i relacje mieszkańców, a nie przez rozbudowaną administrację właściwą R1.

Gospodarka opiera się na małej osadzie, lokalnym rzemiośle, żywności, pozyskiwaniu zasobów lasu/mokradeł oraz wymianie prowadzonej drogą do Nadborza. Vertical-slice vendor i usługi są kanonicznym minimum; nowe sklepy nie powinny być dodawane bez potrzeby contentowej.

Religia jest praktyką lokalną i częścią świata społecznego. Kamienny Krąg, domowe praktyki i opcjonalny boski kontakt nie mogą sugerować jednej zunifikowanej instytucji kultowej dla całego świata.

## Zagrożenia i encounter families

Kanoniczne rodziny R0:
- zwykłe zagrożenia terenowe i zwierzęce;
- `swamp-predator` jako jawnie autorska istota F;
- pierwsza zjawa/supernatural encounter;
- zjawiska świetlne/ogniki jako `phenomenon tag`, nie automatycznie osobny gatunek;
- lokalny leśny guardian jako kierunek zaakceptowany, lecz z otwartą finalną nazwą regionalną;
- encountery społeczne i śledcze związane z mieszkańcami.

Encounter density ma pozostać tunable do playtestu. R0 uczy rozpoznawania śladów i przygotowania, więc nie może zamienić się w ciąg losowych walk między POI.

## Regionalny roster NPC

Obowiązkowy roster produkcyjny stanowi pięć NPC z `VerticalSliceNPCs.md` i odpowiadających im grafów dialogowych. Ten bible nie duplikuje ich kart.

Dodatkowe role tła mogą obejmować mieszkańców, podróżnych i pracowników usług, ale nowe nazwane NPC wymagają własnego trwałego ID oraz wpisu do przyszłego production rosteru P3.

## Quest hooks i main-quest touchpoints

R0 obsługuje:
- MQ00 i MQ01 oraz handoff do MQ10/R1;
- pełny `Światło nad mokradłem`;
- side questy vertical slice;
- pierwszy rytuał i opcjonalny boski kontakt;
- late-game return state pokazujący konsekwencje wcześniejszych decyzji.

Krytyczny handoff do Nadborza nie może zależeć od opcjonalnego boskiego kontaktu, konkretnego rozwiązania side questa ani od zabicia istoty, którą design dopuszcza ominąć.

## Day/night/weather hooks

Region korzysta z globalnych owner specs `DayNightEvents.md`, `WeatherGameplay.md` i `TimeEventFormat.md`.

R0 powinien szczególnie eksponować:
- nocną zmianę czytelności mokradeł i supernatural;
- mgłę/deszcz wpływające na tropienie i widoczność, bez arbitralnego kasowania questowych dowodów;
- bezpieczniejsze i bardziej społeczne zachowanie Żarnowca za dnia;
- fallbacki eventów czasowych tak, aby sen, fast travel lub save/load nie blokowały kampanii.

## Travel gates i streaming assumptions

R0 jest jednym zwartym obszarem produkcyjnym z możliwością streamowania cięższych stref/POI. Nie zakładamy technicznie jednego seamless świata między R0 i R1.

Travel gates:
- lokalna eksploracja R0 pozostaje dostępna zgodnie z questowymi warunkami;
- przejście kampanijne do R1 następuje przez drogę do Nadborza po spełnieniu owner-spec warunków MQ01;
- późniejsze powroty mogą używać world map/travel transition;
- blokady muszą mieć stan gameplayowy/narracyjny, nie tylko niewidzialny collider.

Dokładne promienie streamingu, memory budgets i czasy loadingu pozostają `TBD measured` do P5/P6.

## Asset families

Minimum rodzin assetów:
- wiejska zabudowa, ogrodzenia, drogi i mała infrastruktura;
- pola/łąki i props codziennej pracy;
- skraj boru i leśne ground-cover sets;
- mokradła: woda, trzciny, błoto, martwe drzewa, mgła;
- kamienne/stare miejsca i quest interactables;
- podstawowe props usług, łowiectwa, rytuału i evidence;
- regionalne warianty pogody oraz VFX supernatural.

Szczegółowy vertical-slice asset scope pozostaje własnością `VerticalSliceAssetList.md`; ten dokument grupuje rodziny potrzebne przy skalowaniu R0.

## Audio identity

Warstwy audio R0:
- osada: praca, zwierzęta, ludzie, wiatr i odległe czynności;
- las: gęstsze ambience, ptaki/owady, kierunkowe sygnały tropienia;
- mokradła: woda, owady, trzcinowiska, ograniczona czytelność kierunku;
- supernatural: oszczędne motywy rozpoznawalne przed wizualnym potwierdzeniem;
- noc: mniejsza liczba ludzkich sygnałów i większe znaczenie przestrzeni.

Finalny VO/music budget pozostaje otwarty do P5.

## Persistence i late-game state

Region musi zachowywać:
- wyniki obowiązkowych i istotnych side questów;
- evidence/reveal zdobyte w R0;
- stan kluczowych NPC i usług;
- rozwiązanie `Światła nad mokradłem`;
- stan wymagany przez MQ01 handoff;
- zmiany używane przy późniejszym powrocie.

Powrót late-game powinien pokazać przynajmniej jeden czytelny skutek wcześniejszych decyzji, ale szczegółowa macierz zmian pozostaje zależna od finalnego content catalogu i rosteru.

## QA acceptance

R0 jest gotowy do world-buildingu, gdy:
1. wszystkie obowiązkowe POI mają trwałe ID lub jawnego ownera istniejącej karty;
2. z każdego głównego kierunku gracz może wrócić do Żarnowca bez softlocka;
3. MQ00/MQ01 można ukończyć niezależnie od opcjonalnego boskiego kontaktu;
4. rozwiązania `Światła nad mokradłem` zachowują poprawny persistence po save/load;
5. event czasowy ma fallback po pominięciu okna;
6. droga do R1 odblokowuje się wyłącznie przez właściwy campaign state;
7. late-game return nie resetuje wcześniejszych decyzji;
8. region nie wymaga finalnych wartości balansu ani niezmierzonych performance assumptions do rozpoczęcia greyboxu.

## Otwarte decyzje / research debt

Jawnie niezamknięte:
- finalna regionalna nazwa `forest-guardian`;
- finalne liczby encounter density, economy i combat balance;
- szczegółowy late-game dressing po ustaleniu finalnych katalogów;
- finalny VO/music scope;
- streaming/memory/performance budgets — tylko po pomiarach;
- dodatkowe nazwane NPC poza istniejącą piątką — do P3;
- dokładna lista assetów poza vertical slice — do P4/P5.

Żadna z tych pozycji nie blokuje greyboxu i implementacji podstawowego flow R0; nie wolno jednak cicho traktować wartości tymczasowych jako finalnego locku.

## Runtime environment dressing pass — 56 curated props

R0 ma dodatkową ręcznie rozmieszczoną warstwę modeli ponad proceduralne lasy i ground clutter.

### Żarnowiec
Dodano m.in.:
- czwarty wariant chaty;
- stodołę;
- stajnię;
- wieżę strażniczą;
- dwa wozy;
- bele siana i stosy drewna;
- kowadło i stojak narzędzi;
- kosze, worki i gliniane garnki;
- palenisko;
- drogowskaz przy południowym wyjściu.

Duże budynki i landmarki mają własne proste kolizje.

### Puszcza przy starcie
Dodano:
- mały obóz myśliwski;
- ognisko;
- kosz i worek;
- pułapki;
- drogowskaz;
- wejście do jaskini;
- otaczające je bloki skalne.

### Czarne Mokradła
Dodano:
- złamany fragment mostu;
- uszkodzoną kładkę;
- dwie wyspy bagienne;
- pułapki rybackie;
- ślady pazurów;
- ślad krwi;
- porzucone wyposażenie.

Elementy śledztwa wizualnie wspierają istniejące punkty evidence, ale same modele nie przyznają wiedzy bez właściwej interakcji gameplayowej.

### Kamienny Krąg
Dodano:
- dwa fragmenty ruin;
- kamienie kultowe i runiczne;
- misy ofiarne;
- paliki rytualne;
- świece;
- trzy znaczniki grobów.

### Performance contract
Małe rekwizyty są klasyfikowane przez `WorldModelRenderPolicy` jako short-range props i używają dystansu ground clutter. Budynki, ruiny, jaskinia i główne landmarki zachowują pełny world render distance. To pozwala zwiększać gęstość lokacji bez utrzymywania wszystkich drobnych modeli na dużym dystansie, szczególnie na Low/Balanced i słabszych iGPU.


## Runtime correction pass — orientation + river channel

### Object orientation contract

Ręcznie rozmieszczone obiekty nie polegają już wyłącznie na przypadkowych wartościach `YawRadians`.

`WorldPlacementOrientation` definiuje wspólną konwencję:
- statyczne modele źródłowe są Z-up;
- ich semantyczny front +Y po konwersji GLB odpowiada engine `-Z`;
- obiekty kierunkowe dostają yaw z docelowej kotwicy przestrzennej.

Przykłady:
- chaty/stodoła/stajnia/wieża → wnętrze Żarnowca;
- kuźnia i jej wyposażenie → stanowisko kuźni;
- rekwizyty rynku → środek targu;
- drogowskazy → kierunek podejścia;
- wejście jaskini → ścieżka dojścia;
- most/kładka → oś przeprawy;
- ślady pazurów → obszar drapieżnika;
- ślad krwi → rejon pamiątki;
- elementy Kamiennego Kręgu → jego środek.

Elementy naturalne i celowo losowe, np. drzewa/krzewy, zachowują proceduralny yaw.

### River terrain contract

Rzeka nie używa już jednej lewitującej płaszczyzny na stałej wysokości.

`WaterLandscape` definiuje teraz:
- lokalny poziom wody `WaterLevel(z)`;
- łagodny spadek w kierunku +Z;
- centralne dno około 1.30 m pod taflą;
- płytką strefę przy brzegu;
- suchy shoulder brzegu, który wraca do oryginalnego terenu zamiast być wciskany pod wodę;
- powierzchnię wody węższą od pełnego profilu koryta.

Woda jest generowana jako 5-kolumnowy ribbon co 4 m. Dwa przesuwające się downstream pasma fazy zmieniają kolor i dają niewielkie przemieszczenie wysokości, więc przepływ jest widoczny bez kosztownych odbić/SSR.

Oba brzegi dostają deterministyczny pas `riverbank_rocky_r0_01.glb`. Te modele używają short-range cullingu, dzięki czemu wizualne koryto nie obciąża mocno słabszych iGPU.


## River interaction pass

Po przebudowie koryta rzeka ma również lekką warstwę interakcji i ambience.

### Player ripples
- `WaterInteractionState` wykrywa, czy gracz stoi wewnątrz rzeczywistego wet ribbonu i czy grunt jest pod lokalnym poziomem wody;
- intensywność zależy od poziomej prędkości gracza;
- `WaterInteractionMesh` rysuje maksymalnie trzy rozszerzające się ringi po 24 segmenty;
- poza wodą geometria ripple nie jest generowana.

### Foam
Przy obu krawędziach nurtu generowane są krótkie, animowane smugi piany w odstępie 32 m, zbliżonym do rocky-bank dressing. To daje wizualne wiry przy przeszkodach bez cząsteczek, przezroczystości, SSR ani osobnego compute passu.

### Procedural ambience
`RiverAmbienceSynthesizer` generuje PCM16 mono 24 kHz z filtrowanego szumu i słabych składowych tonalnych. Głośność maleje płynnie do zera do około 78 m od tafli.

Playback rzeki korzysta z osobnego `SdlPcmPlayer`, niezależnego od TTS, więc ambience nie czyści kolejki głosu zaklęć. Jeśli dodatkowe urządzenie SDL nie może zostać otwarte, ambience wyłącza się łagodnie bez blokowania gry.

### Low-end budget
- brak systemu particle dla wody;
- brak reflection/refraction render target;
- ripple istnieją tylko w wodzie;
- foam to kilka quadów w pobliżu kamienistych brzegów;
- audio jest proceduralne, bez streamowania dużych assetów.


## Wading gameplay pass

Rzeka wpływa teraz na ruch gracza zależnie od faktycznej głębokości w miejscu brodzenia.

### Depth-driven movement
- do około 8 cm: brak kary;
- wraz z głębokością prędkość spada płynnie;
- około 35 cm: ruch jest wyraźnie wolniejszy;
- około 75 cm i głębiej: ruch spada w okolice 55–68% prędkości lądowej;
- sprint jest blokowany powyżej około 58 cm.

### Stamina
Płytsze brodzenie nadal pozwala sprintować, ale koszt staminy rośnie z głębokością nawet do około +85%. Regeneracja staminy jest wolniejsza w wodzie i lekko obniżona także po wyjściu, gdy ubranie jest jeszcze mokre.

### Wetness
`WaterInteractionState.Wetness` rośnie podczas wejścia do wody zależnie od głębokości i schodzi stopniowo po wyjściu. Pełne wyschnięcie trwa około półtorej minuty w neutralnych warunkach. Stan jest tymczasowy i nie jest zapisywany w save.

HUD pokazuje:
- głębokość wody w cm podczas brodzenia;
- procent mokrego ubrania;
- po wyjściu tylko wetness, dopóki stan jest istotny.

### Footstep splash
Przebyty dystans w wodzie generuje rytmiczne splash pulses. Pulse:
- wzmacnia ripple;
- uruchamia krótki transient szumu w proceduralnym ambience;
- ma cooldown, żeby pojedynczy krok nie generował wielu dźwięków na kolejnych frame'ach.

Całość pozostaje bez osobnego particle systemu i bez dodatkowych tekstur VFX.


## River current, rain and campfire exposure

### Current
The river now applies a real downstream drift to the player. Current strength is derived from local water depth:
- below ~28 cm: no meaningful drift;
- deeper water ramps non-linearly;
- near the center bed the current reaches roughly 1.6 m/s.

The drift is applied even with no movement input and still goes through normal world collision/terrain resolution. The wading HUD shows current strength in m/s whenever it becomes noticeable.

### Rain and wetness
Wetness is no longer only river-driven:
- `RainIntensity` wets the player while outside the river;
- stronger rain increases wetness faster;
- rain strongly suppresses passive drying.

This uses the existing regional weather system and does not add a second weather simulation.

### Chill
`WaterInteractionState.Chill` is a normalized exposure state driven by:
- wetness;
- water depth;
- rain;
- wind.

Chill does **not** directly damage health in this pass. It only reduces stamina recovery, so the feature affects traversal without turning the prototype into a survival game.

### Active campfires
The two existing R0 firepits are now active heat sources:
- Żarnowiec firepit at the village center;
- hunter camp firepit in the starting forest.

Heat:
- accelerates drying by up to roughly 10x near the flame;
- rapidly removes chill;
- partially offsets rain wetting at close range.

Both fires receive a tiny animated geometry effect (ground glow + two low-poly flame layers) in the existing actor pass. No particle system, transparency pass, volumetric light or extra render target is used.


## River current and exposure pass

Rzeka ma teraz lekki gameplayowy nurt oraz prosty model ekspozycji środowiskowej.

### Nurt

`WaterInteractionState.CurrentSpeedForDepth` uruchamia znoszenie dopiero po przekroczeniu około 28 cm głębokości. Siła rośnie nieliniowo wraz z głębokością i w najgłębszej części obecnego koryta dochodzi do około 1.6 m/s.

`PlayerController` dodaje wektor nurtu niezależnie od inputu gracza, więc:
- stojący gracz może zostać zniesiony downstream;
- ruch pod prąd realnie walczy z prędkością przepływu;
- przy brzegu nurt praktycznie zanika;
- system korzysta z `ResolveHorizontalPosition`, więc nadal respektuje teren i kolizje.

HUD w wodzie pokazuje także przybliżoną prędkość nurtu.

### Deszcz, moknięcie i wychłodzenie

Poza rzeką `Weather.RainIntensity` może dalej zwiększać `Wetness`. Wiatr i deszcz zwiększają presję wychłodzenia, szczególnie gdy ubranie jest mokre.

`Chill`:
- rośnie stopniowo, bez nagłego skoku;
- jest dodatkowo podbijany przez głębszą wodę;
- obniża regenerację staminy;
- nie zadaje obecnie obrażeń i nie jest systemem survivalowym;
- schodzi szybciej przy źródle ciepła.

### Ogniska

Dwa istniejące paleniska są teraz aktywnymi źródłami ciepła:
- `village-firepit`;
- `forest-hunter-firepit`.

`CampfireSystem.HeatAt` wylicza płynny falloff po dystansie. Blisko ogniska:
- schnięcie jest znacznie szybsze;
- wychłodzenie spada szybciej;
- deszcz jest częściowo kompensowany przez ciepło.

`CampfireEffectMesh` dodaje lekkie, animowane płomienie oraz ground glow do istniejącego actor passu. Nie ma osobnego particle systemu, shadow-castera ani dodatkowego render targetu.


## River current and exposure pass

Rzeka i pogoda wpływają teraz na pozycję oraz krótkoterminową kondycję gracza.

### Current
`WaterInteractionState.CurrentVelocityAt` wykorzystuje rzeczywisty kierunek `WaterLandscape.FlowDirection`.
- poniżej około 28 cm nurt nie przesuwa gracza;
- wraz z głębokością siła rośnie nieliniowo;
- w najgłębszej części obecnego brodu osiąga około 1.5–1.65 m/s;
- znoszenie jest stosowane również bez inputu, więc stanie w głębokim nurcie nie jest statyczne.

### Rain, wetness and chill
- deszcz moczy gracza również poza rzeką;
- wiatr i deszcz zwiększają presję wychłodzenia;
- mokre ubranie + głęboka woda budują `Chill`;
- wychłodzenie nie zadaje jeszcze obrażeń, ale obniża regenerację staminy;
- jest to celowo lekki stan eksploracyjny, nie pełna symulacja temperatury ciała.

### Campfire heat
Dwa istniejące paleniska są aktywnymi źródłami ciepła:
- Żarnowiec: `village-firepit`;
- obóz myśliwski: `forest-hunter-firepit`.

Ciepło:
- przyspiesza schnięcie nawet około 10x przy samym palenisku;
- szybko redukuje `Chill`;
- ogranicza ponowne moknięcie od deszczu w bezpośrednim sąsiedztwie.

Ogniska dostają animowane płomienie i ground glow generowane w istniejącym actor pass. Kolory >1.0 są traktowane jako prosty sygnał emissive, dzięki czemu płomień pozostaje czytelny również nocą bez osobnego light passu ani shadow-casting point light.


## River current and exposure pass

Rzeka oraz pogoda wpływają teraz na ekspozycję gracza, ale bez survivalowego damage-over-time.

### Current
- nurt zaczyna działać dopiero w wodzie głębszej niż około 28 cm;
- siła rośnie nieliniowo wraz z głębokością;
- w najgłębszym obecnym korycie osiąga około 1.65 m/s;
- kierunek zawsze pochodzi z `WaterLandscape.FlowDirection(z)`;
- przesunięcie przechodzi przez zwykłe `SetPlayerPosition`, więc nadal respektuje granice mapy i kolizje.

### Rain, wetness and chill
- deszcz moczy gracza również poza rzeką;
- wiatr i intensywność deszczu zwiększają presję wychłodzenia;
- mokre ubranie i głęboka woda budują `Chill`;
- wychłodzenie dodatkowo obniża regenerację staminy;
- nie ma automatycznych obrażeń ani śmierci od temperatury w tym etapie.

### Campfires
Aktywne źródła ciepła:
- palenisko w Żarnowcu;
- obóz myśliwski w lesie.

W zasięgu ogniska:
- suszenie jest wielokrotnie szybsze;
- wychłodzenie opada szybciej;
- deszcz nadal może przeciwdziałać suszeniu, ale heat ma wyraźny efekt.

Ogniska mają lekką animowaną geometrię płomienia i ground glow w istniejącym actor pass. Nie używają osobnego particle systemu, shadow light ani dodatkowego render targetu.

### HUD
HUD może pokazać:
- głębokość wody;
- wetness;
- wychłodzenie;
- `DESZCZ`;
- `PRZY OGNISKU`.

To zamyka podstawowy runtime rzeki: koryto, przepływ wizualny, ripple/foam/audio, brodzenie, nurt, wetness, deszcz, wychłodzenie i suszenie przy ogniu.
