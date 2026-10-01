# Kolejność produkcji

**Wersja 0.1.** Zachowujemy ustaloną kolejność projektowania gry. Prototyp techniczny już istnieje, ale jego istnienie nie oznacza ukończenia wcześniejszych dokumentów ani późniejszych systemów.

## Etapy i wyniki

| Etap | Zakres | Warunek zakończenia | Stan |
|---|---|---|---|
| 1 | Fundament świata | Spójne filary, zakres, codzienność i rola gracza | World Bible + pełny obraz świata v0.2 istnieją; research/content nadal rozwijane |
| 2 | Kosmologia | Reguły sfer, przejść, ograniczeń i świadomie otwarte tajemnice | Kosmologia v0.2 + author truth Czwartej Sfery zapisane |
| 3 | Pełny katalog panteonu | Rejestr źródeł, wariantów, niepewności i uzasadnionych pominięć | Katalog v0.1: 52 pozycje; badania pozostają otwarte |
| 4 | Historie bogów | Karty wybranych bóstw, cele, sprzeczności kultów i relacje F | GodHistories + research pass 02 + matryca relacji; genealogia historyczna nadal celowo nieustalona |
| 5 | Historia świata | Osobno zdarzenia, świadectwa i konkurencyjne interpretacje | Timeline makro + pełna chronologia centralnej tajemnicy istnieją |
| 6 | Kultury | Życie materialne, języki, wierzenia i wewnętrzne różnice | Kultury makro v0.1 + research startowego regionu; kolejne pakiety źródłowe nadal potrzebne |
| 7 | Królestwa i regiony | Geografia, zasoby, zależności, granice oraz konflikty | Siły polityczne, R0–R6, gospodarka i konflikty mają pełny first pass |
| 8 | Magia | Źródła mocy, koszty, nauka, alfabet i ograniczenia | Magic Bible v0.1 zapisany; czary/rytuały wymagają implementacji i balansu |
| 9 | Potwory | Źródła, ekologia, zachowania i alternatywy wobec zabijania | Bestiary Bible + dwa pakiety researchowe; pełny finalny roster nadal do produkcji |
| 10 | Bohater | Kreator, zawód, rozwój i dopuszczalne style gry | PlayerCharacter v0.1 zapisany |
| 11 | Rodzina bohatera | Tajemnica, dowody, tempo odkrywania i wpływ na wybory | FamilyMystery + FamilyTruth: prawda autorów i plan ujawniania ustalone |
| 12 | Główna historia | Konflikt, akty i ścieżka bez boskiego patrona | MainStory v0.2: akty, Wszebor/Krąg Rozwarcia, kryzys i finał ustalone |
| 13 | Zadania | Pełna pętla, stany, rozwiązania i sposób nagradzania | QuestDesign v0.1 + referencyjne „Światło nad mokradłem” |
| 14 | Decyzje | Wskazane strony, skutki i momenty odczuwalnej zmiany | DecisionModel v0.1 zapisany |
| 15 | Zakończenia | Wyniki zależne od historii działań i relacji | Pięć głównych ending architectures + epilogue matrix ustalone |
| 16 | Mapa świata | Gęste regiony, połączenia, rytm odkrywania | MacroGeography R0–R6 + regional campaign flow ustalone |
| 17 | Projekt rozgrywki | Spójne systemy, ograniczenia i kryteria wycinka | GameDesignBible + systemowe specyfikacje v0.1 istnieją |
| 18 | Architektura silnika | Granice systemów, dane, zasoby, stan i zapis | Fundament v0.1 przygotowany: moduły systemów, wspólny postęp i wersjonowany checkpoint; backendy assetów/audio/animacji oraz pełne integracje pozostają do rozwoju |
| 19 | Vulkan | Działający rendering oraz określone ograniczenia sprzętowe | Podstawowy rendering działa; RenderingAndPlatform v0.1 określa dalsze warstwy |
| 20 | Prototyp 3D | Teren, kamera, ruch i podstawowe pomiary | Działa; TestingAndPerformance v0.1 określa sposób pomiarów |
| 21 | Grywalny wycinek | Wszystkie kryteria z VerticalSlice spełnione | Specyfikacja istnieje; implementacja planowana |
| 22 | Rozbudowa świata | Następne regiony o jakości potwierdzonej wycinkiem | ContentProduction v0.1 definiuje pipeline; produkcja kolejnych regionów jeszcze nie rozpoczęta |
| 23 | Pełna gra | Ukończona treść, testy, optymalizacja i wydanie | ReleaseCriteria v0.1 definiuje milestone'y; pełna produkcja pozostaje przyszłością |

„Pełny katalog” jest celem badawczym, a nie obietnicą odkrycia jednej zamkniętej listy historycznych bogów. Etap 3 można domknąć dla zakresu projektu po sprawdzeniu źródeł i jawnej ocenie kandydatów; nowe odkrycia pozostają możliwe.

## Najbliższy pakiet pracy

**Pełny obraz designu v0.2 został zbudowany od najprostszych warstw do author truth.**

Następne prace są już produkcyjnym pogłębianiem znanego projektu:
1. karty main questów aktów I–V;
2. region bibles R1–R6;
3. roster NPC/companionów;
4. finalniejsze bestiary i research kultury Arel;
5. asset budgets i pipeline;
6. implementacja vertical slice;
7. później pełna produkcja regionów.

Punkt wejścia: [FullGameOverview.md](../FullGameOverview.md).

## Warunki jakości dokumentacji

Przed zakończeniem pakietu sprawdzamy unikalne identyfikatory, warianty nazw, odsyłacze, zakres konsultacji źródeł i spójność terminów. Zmiana projektu F nie wymaga podnoszenia jej do rangi twierdzenia historycznego. Zmiana klasy świadectwa wymaga wyjaśnienia i źródła.

## Stałe techniczne

Projekt pozostaje na **.NET 11**. Podstawowy renderer używa **Vulkan przez Veldrid**; silnik i systemy gry powstają w C#. Najpierw PC, Android później po ocenie wydajności, wejścia i potrzeb platformy. Dokumentacja świata nie jest migracją frameworka ani deklaracją gotowości wszystkich wymienionych systemów.
