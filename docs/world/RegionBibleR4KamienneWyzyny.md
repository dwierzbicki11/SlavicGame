# Region Bible R4 — Kamienne Wyżyny

Status: production bible v0.1. Dokument rozwija `MacroGeography.md` i `CampaignRegionalFlow.md`; nie zastępuje owner-speców pogody, walki, traversal, ekonomii, encounterów ani researchu materialnego.

## 1. Rola regionu

Kamienne Wyżyny są regionem Aktu II-B. Ich funkcją jest pokazanie materialnego kosztu kryzysu starej sieci: współczesne wydobycie przecina starsze struktury, a materiały używane w dawnych progach prowadzą gracza do gór.

Temat: **koszt zasobów**. Konflikt nie sprowadza się do prostego „kopalnia zła / natura dobra”. Region pokazuje zależność osad od wydobycia, niebezpieczeństwo pracy oraz konsekwencje naruszenia struktur, których funkcji mieszkańcy nie rozumieją.

## 2. Granice i topologia

R4 jest osobnym dużym obszarem połączonym z resztą świata trasą przez południowe pasma. Grywalna topologia składa się z:

1. **Wysokamienia** — głównego huba osadniczego i usługowego;
2. **Żelaznej Bramy** — kontrolowanego przejścia i punktu logistycznego;
3. **dolnych stoków** — dróg, małych osad, składów i zaplecza wydobycia;
4. **stref kopalnianych** — aktywnych, zamkniętych i starszych wyrobisk;
5. **przełęczy** — tras zależnych od pogody i stanu świata;
6. **górskich miejsc kultowych** — punktów o funkcji społecznej lub religijnej, których nie należy automatycznie utożsamiać ze starą siecią;
7. **starszej struktury podziemnej** — warstwy odkrywanej w toku kampanii.

Region nie jest jednym ciągłym pionowym dungeonem. Powierzchnia, osady i podziemia mają odrębny rytm oraz czytelne punkty powrotu.

## 3. Wysokamień

Wysokamień jest głównym hubem R4. Zapewnia:
- nocleg i podstawowe usługi;
- handel żywnością, narzędziami i wyposażeniem;
- dostęp do informacji o kopalniach i przełęczach;
- NPC związanych z wydobyciem, transportem, lokalną władzą i kultem;
- miejsce, w którym konsekwencje decyzji kopalnianych stają się społecznie widoczne.

Nie zamrażamy jeszcze finalnej skali miasta ani liczby dzielnic. Layout ma wynikać z asset budgetu i testów traversal.

## 4. Żelazna Brama

Żelazna Brama jest kontrolowanym węzłem między bezpieczniejszą częścią regionu a trudniejszymi trasami. Pełni role:
- checkpointu podróży;
- miejsca kontroli ruchu ludzi i surowców;
- źródła informacji o stanie przełęczy;
- naturalnego punktu dla zmian politycznych i late-game state.

Nazwa nie przesądza materiału ani dokładnej konstrukcji bramy; finalna forma wymaga zgodności z research package kultury i architektury.

## 5. Kopalnie i podziemia

Kopalnie występują w co najmniej trzech stanach:
- **aktywne** — praca, transport, ryzyko i ekonomia;
- **zamknięte** — katastrofa, wyczerpanie, decyzja polityczna albo zagrożenie;
- **starsze wyrobiska/struktury** — przestrzenie, których pochodzenie i funkcja nie są oczywiste.

Główny dungeon/progression regionu wykorzystuje przecięcie współczesnego wyrobiska ze starszą strukturą. Gracz ma móc rozpoznać różnicę po geometrii, materiałach, śladach narzędzi i funkcji przestrzeni.

Podziemia nie mogą być labiryntem bez informacji. Orientację wspierają landmarki, ślady pracy, oznaczenia dróg i logiczna relacja szybów/korytarzy.

## 6. Vertical traversal

R4 jest pierwszym regionem, w którym różnica wysokości jest istotnym elementem regularnej eksploracji. Vertical traversal obejmuje:
- podejścia i zejścia;
- wąskie przejścia i półki;
- kontrolowane skróty odblokowywane z drugiej strony;
- szyby i konstrukcje kopalniane tylko tam, gdzie mają owner-spec i bezpieczny kontrakt gameplayowy.

Region Bible nie definiuje nowego systemu wspinaczki. Jeżeli istniejący movement/traversal runtime nie obsługuje wymaganej akcji, implementacja contentu musi zostać zablokowana do czasu powstania jawnej specyfikacji zamiast zgadywania sterowania.

## 7. Pogoda i zimno

Kamienne Wyżyny używają istniejącego weather systemu jako presji na trasę, widoczność i przygotowanie. Wyższe partie mogą mieć ostrzejsze warunki niż doliny.

Zasady:
- pogoda nie może losowo zabijać gracza bez ostrzeżenia;
- ryzyko ma być komunikowane wizualnie, dźwiękowo i przez NPC;
- zamknięcie przełęczy musi mieć alternatywę, recovery albo jasno komunikowany stan czasowy;
- save/load zachowuje istotny stan świata zgodnie z owner-specem pogody.

Finalne wartości temperatur, obrażeń/statusów i częstotliwości zjawisk pozostają tuningiem po playtestach.

## 8. Ekonomia zasobów

R4 dostarcza surowców ważnych dla gospodarki międzyregionalnej. Gameplay pokazuje łańcuch:

`wydobycie -> sortowanie/skład -> transport -> warsztat/handel`

Nie zakładamy automatycznie przemysłowej skali. Narzędzia, transport i organizacja pracy muszą pozostać zgodne z zatwierdzonym researchem materialnym.

Decyzje gracza mogą wpływać na:
- dostępność części kopalń;
- bezpieczeństwo pracy;
- ceny lub podaż wybranych kategorii materiałów;
- relacje z grupami zależnymi od wydobycia;
- późniejszy stan tras.

Dokładne mnożniki ekonomiczne pozostają poza Region Bible.

## 9. Kampania i dowody

Wejście fabularne do R4 następuje po uzyskaniu przesłanki, że materiały dawnych progów mają źródło w górach. Region musi dostarczyć graczowi dowody w kilku warstwach:

1. współczesne ślady wydobycia i obróbki;
2. materiał lub technika różniąca się od zwykłej eksploatacji;
3. fizyczne przecięcie starszej struktury;
4. dowód pozwalający powiązać materiał z siecią miejsc poznaną wcześniej.

Kluczowy reveal: **materiały używane w dawnych progach pochodziły z gór**. Region nie ujawnia jeszcze całej prawdy o Pierwszym Progu ani Czwartej Sferze.

Questy muszą korzystać z trwałych ID i persistence zgodnie z owner-specami. Region Bible nie wymyśla dodatkowych obowiązkowych MQ poza istniejącym skeletonem.

## 10. Encounter families

R4 używa pięciu rodzin encounterów:

1. **route/weather pressure** — warunki utrudniające przejście, z czytelną telemetrią i recovery;
2. **worksite incident** — wypadki, zablokowane przejścia, zaginieni lub spór o bezpieczeństwo;
3. **resource conflict** — kradzież, wymuszenie, kontrola transportu lub konflikt interesów;
4. **underground threat** — zagrożenia wynikające z ciemności, ograniczonej przestrzeni i naruszonego środowiska;
5. **old-structure anomaly** — supernatural/technical effects związane ze starą siecią.

Każdy encounter musi mieć obserwowalny setup, warunki zakończenia i bezpieczny fallback. Nie każdy encounter podziemny kończy się walką.

## 11. NPC i frakcje regionalne

Minimalne role produkcyjne:
- przedstawiciel lokalnej władzy;
- osoba odpowiedzialna za kopalnię lub zmianę roboczą;
- doświadczony pracownik/przewodnik;
- rzemieślnik zależny od surowców;
- osoba reprezentująca lokalną praktykę kultową;
- przewoźnik/organizator transportu;
- świadek lub badacz starszej struktury.

Finalne imiona, pochodzenie i liczba NPC powstają w rosterze contentowym. Nie należy kopiować konfliktów politycznych Nadborza pod innymi nazwami.

## 12. Miejsca kultowe

Górskie miejsca kultowe są częścią żywego regionu, a nie dekoracją fantasy. Ich forma, praktyki i wyposażenie muszą wynikać z researchu oraz zasad panteonu.

Stara struktura i miejsce kultowe mogą się przestrzennie nakładać tylko wtedy, gdy quest/lore jawnie wyjaśnia tę relację. Nie zakładamy, że każda dawna konstrukcja była świątynią.

## 13. Travel i gating

Pierwsze wejście do R4 powinno prowadzić przez czytelny szlak do huba. Dalszy gating może wynikać z:
- stanu przełęczy;
- zgody/dostępu do kopalni;
- odblokowanego skrótu;
- kampanii;
- bezpiecznego przygotowania do trudniejszej strefy.

Fast travel odblokowuje się dopiero po fizycznym poznaniu odpowiednich węzłów. Gating nie może niszczyć możliwości ukończenia głównego wątku po decyzji pobocznej.

## 14. Day/night i audio

Na powierzchni dzień/noc wpływa na widoczność, ruch ludzi i ryzyko tras. W podziemiach pora dnia nie zmienia sztucznie oświetlenia, ale może wpływać na dostępność ekip, usług i eventów przy wejściach.

Audio identity:
- wiatr i przestrzeń na wysokości;
- woda i echo w części podziemi;
- praca narzędzi w aktywnych strefach;
- drewniane konstrukcje, liny i transport;
- wyraźnie odmienny język dźwiękowy starszej struktury.

Finalna muzyka i zakres VO pozostają decyzją produkcyjną.

## 15. Persistence i late-game state

Trwały stan regionu obejmuje co najmniej:
- odkryte węzły podróży i skróty;
- stan kluczowych wejść kopalnianych;
- główne decyzje questowe;
- status dowodów kampanii;
- istotne zmiany bezpieczeństwa/pracy;
- stan wymagany przez late-game return.

Po kryzysie/fabularnym przełomie gracz wraca do R4 i widzi konsekwencje: zmienione trasy, pracę lub zamknięcie części miejsc, reakcje NPC i wpływ wcześniejszych decyzji. Powrót nie może być wyłącznie zmianą dialogu w jednym NPC.

## 16. Asset families

Region wymaga rodzin assetów, nie pojedynczych unikatów dla każdego miejsca:
- skały i powierzchnie górskie;
- ścieżki, osuwiska i przełęcze;
- zabudowa Wysokamienia;
- małe osady i zaplecze logistyczne;
- wejścia kopalń;
- modularne podpory, pomosty, drabiny/schody i elementy transportowe;
- narzędzia i propsy wydobywcze;
- składy i materiały;
- górskie miejsca kultowe;
- starsza struktura o odrębnym języku wizualnym;
- warianty weather dressing.

Asset catalog powinien wskazywać LOD/collision/material requirements dopiero po ustaleniu pipeline'u i pomiarach.

## 17. Streaming i wydajność

R4 należy dzielić na logiczne komórki: hub, dolne stoki, przełęcze, kopalnie i starszą strukturę. Podziemia mogą stanowić osobne przestrzenie streamingowe/loadingowe; nie ma wymagania seamless przejścia za cenę stabilności.

Production rules:
- nie renderować całej panoramy i wnętrz kopalń jako jednego zestawu aktywnych zasobów;
- ciężkie VFX starej struktury mają jawny fallback jakościowy;
- pogoda nie może wymuszać nieograniczonego overdraw;
- mierzone CPU/GPU/RAM/VRAM budgets zostają wpisane dopiero po działającym rendererze i profilowaniu.

## 18. QA acceptance

R4 jest gotowy do content production, gdy:

1. gracz potrafi dotrzeć do Wysokamienia i zidentyfikować główne kierunki bez debug mapy;
2. co najmniej jedna trasa demonstruje vertical traversal bez softlocka;
3. zmiana pogody nie tworzy nieodwracalnego zablokowania kampanii;
4. aktywna kopalnia i starsza struktura są wizualnie i funkcjonalnie rozróżnialne;
5. kampania dostarcza trwały dowód pochodzenia materiału dawnych progów;
6. save/load zachowuje kluczowy stan kopalni, dowodów i travel nodes;
7. decyzja poboczna nie blokuje głównego reveal;
8. late-game return pokazuje co najmniej jedną zmianę przestrzenną/systemową wynikającą ze stanu regionu.

## 19. Otwarte decyzje

Jawnie pozostają otwarte:
- finalna skala i layout Wysokamienia;
- dokładna forma Żelaznej Bramy;
- liczba kopalń i proporcja aktywnych/zamkniętych;
- finalny roster NPC i side questów;
- dokładny surowiec lub zestaw materiałów powiązany z dawnymi progami, jeśli nie został jeszcze zamrożony przez owner lore;
- szczegółowy traversal moveset wymagany przez najbardziej pionowe miejsca;
- wartości cold/weather/status tuning;
- economy/encounter tuning;
- finalny zakres VO i muzyki;
- asset counts, LOD distances i collision budgets;
- zmierzone targety CPU/GPU/RAM/VRAM.

Żadnej z tych decyzji implementacja nie powinna uzupełniać przez zgadywanie. Jeśli staje się blockerem, trafia do właściwego owner-specu lub kolejki dokumentacji.
