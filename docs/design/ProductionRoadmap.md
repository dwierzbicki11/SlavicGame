# Kolejność produkcji

**Wersja 0.1.** Zachowujemy ustaloną kolejność projektowania gry. Prototyp techniczny już istnieje, ale jego istnienie nie oznacza ukończenia wcześniejszych dokumentów ani późniejszych systemów.

## Etapy i wyniki

| Etap | Zakres | Warunek zakończenia | Stan |
|---|---|---|---|
| 1 | Fundament świata | Spójne filary, zakres, codzienność i rola gracza | World Bible v0.1 zapisany; do dalszego rozwinięcia |
| 2 | Kosmologia | Reguły sfer, przejść, ograniczeń i świadomie otwarte tajemnice | Kosmologia v0.1 zapisana |
| 3 | Pełny katalog panteonu | Rejestr źródeł, wariantów, niepewności i uzasadnionych pominięć | Katalog v0.1: 52 pozycje; badania pozostają otwarte |
| 4 | Historie bogów | Karty wybranych bóstw, cele, sprzeczności kultów i relacje F | GodHistories v0.1: 6 kart + matryca relacji; pełna genealogia nadal celowo nieustalona |
| 5 | Historia świata | Osobno zdarzenia, świadectwa i konkurencyjne interpretacje | Timeline v0.1 istnieje; główne tajemnice pozostają otwarte |
| 6 | Kultury | Życie materialne, języki, wierzenia i wewnętrzne różnice | Cultures v0.1: framework + Pogranicze; research życia materialnego nadal wymagany |
| 7 | Królestwa i regiony | Geografia, zasoby, zależności, granice oraz konflikty | RegionsAndKingdoms v0.1; globalne państwa i finalna mapa pozostają otwarte |
| 8 | Magia | Źródła mocy, koszty, nauka, alfabet i ograniczenia | Magic Bible v0.1 zapisany; czary/rytuały wymagają implementacji i balansu |
| 9 | Potwory | Źródła, ekologia, zachowania i alternatywy wobec zabijania | Bestiary Bible v0.1; indywidualne folklorystyczne karty wymagają researchu |
| 10 | Bohater | Kreator, zawód, rozwój i dopuszczalne style gry | PlayerCharacter v0.1 zapisany |
| 11 | Rodzina bohatera | Tajemnica, dowody, tempo odkrywania i wpływ na wybory | FamilyMystery v0.1 definiuje strukturę; prawda końcowa celowo Otwarte |
| 12 | Główna historia | Konflikt, akty i ścieżka bez boskiego patrona | MainStory v0.1: struktura aktów; finałowe lore i antagonista otwarte |
| 13 | Zadania | Pełna pętla, stany, rozwiązania i sposób nagradzania | QuestDesign v0.1 + referencyjne „Światło nad mokradłem” |
| 14 | Decyzje | Wskazane strony, skutki i momenty odczuwalnej zmiany | DecisionModel v0.1 zapisany |
| 15 | Zakończenia | Wyniki zależne od historii działań i relacji | Endings v0.1 definiuje architekturę; finalne warianty nadal Otwarte |
| 16 | Mapa świata | Gęste regiony, połączenia, rytm odkrywania | WorldMap v0.1; finalna geografia nadal Otwarte |
| 17 | Projekt rozgrywki | Spójne systemy, ograniczenia i kryteria wycinka | GameDesignBible + systemowe specyfikacje v0.1 istnieją |
| 18 | Architektura silnika | Granice systemów, dane, zasoby, stan i zapis | Fundament v0.1 przygotowany: moduły systemów, wspólny postęp i wersjonowany checkpoint; backendy assetów/audio/animacji oraz pełne integracje pozostają do rozwoju |
| 19 | Vulkan | Działający rendering oraz określone ograniczenia sprzętowe | Podstawowy rendering działa; RenderingAndPlatform v0.1 określa dalsze warstwy |
| 20 | Prototyp 3D | Teren, kamera, ruch i podstawowe pomiary | Działa; TestingAndPerformance v0.1 określa sposób pomiarów |
| 21 | Grywalny wycinek | Wszystkie kryteria z VerticalSlice spełnione | Specyfikacja istnieje; implementacja planowana |
| 22 | Rozbudowa świata | Następne regiony o jakości potwierdzonej wycinkiem | ContentProduction v0.1 definiuje pipeline; produkcja kolejnych regionów jeszcze nie rozpoczęta |
| 23 | Pełna gra | Ukończona treść, testy, optymalizacja i wydanie | ReleaseCriteria v0.1 definiuje milestone'y; pełna produkcja pozostaje przyszłością |

„Pełny katalog” jest celem badawczym, a nie obietnicą odkrycia jednej zamkniętej listy historycznych bogów. Etap 3 można domknąć dla zakresu projektu po sprawdzeniu źródeł i jawnej ocenie kandydatów; nowe odkrycia pozostają możliwe.

## Najbliższy pakiet pracy

Dokumentacja ma obecnie pełny szkielet v0.1. Następna praca nie polega na tworzeniu kolejnych ogólnych „Bible”, tylko na **pogłębianiu konkretnych kart**:

1. Dokończyć kontrolę źródeł panteonu: Rod, Rodzanice, późny katalog polski, Jarowit, Radegast/Swarożyc, Siwa/Żywie.
2. Utworzyć indywidualne research cards pierwszych istot bestiariusza i dopiero potem nadać historyczną/folklorystyczną tożsamość prototypowi drapieżnika.
3. Przygotować research package życia materialnego dla Pogranicza Żarnowca: zabudowa, ubiór, żywność, narzędzia, transport, broń, pochówki i handel.
4. Rozpisać pięć NPC vertical slice jako indywidualne karty z dialogami, harmonogramami, stanami po trzech rozwiązaniach i asset needs.
5. Rozwinąć „Światło nad mokradłem” z dokumentu scenariuszowego do kompletnego quest graph z dowodami, warunkami i efektami.
6. Dopiero po testach vertical slice ustalać finalne wartości balansu, sprzęt referencyjny i wymagania sprzętowe.

## Warunki jakości dokumentacji

Przed zakończeniem pakietu sprawdzamy unikalne identyfikatory, warianty nazw, odsyłacze, zakres konsultacji źródeł i spójność terminów. Zmiana projektu F nie wymaga podnoszenia jej do rangi twierdzenia historycznego. Zmiana klasy świadectwa wymaga wyjaśnienia i źródła.

## Stałe techniczne

Projekt pozostaje na **.NET 11**. Podstawowy renderer używa **Vulkan przez Veldrid**; silnik i systemy gry powstają w C#. Najpierw PC, Android później po ocenie wydajności, wejścia i potrzeb platformy. Dokumentacja świata nie jest migracją frameworka ani deklaracją gotowości wszystkich wymienionych systemów.
