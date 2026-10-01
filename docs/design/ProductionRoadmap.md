# Kolejność produkcji

**Wersja 0.1.** Zachowujemy ustaloną kolejność projektowania gry. Prototyp techniczny już istnieje, ale jego istnienie nie oznacza ukończenia wcześniejszych dokumentów ani późniejszych systemów.

## Etapy i wyniki

| Etap | Zakres | Warunek zakończenia | Stan |
|---|---|---|---|
| 1 | Fundament świata | Spójne filary, zakres, codzienność i rola gracza | World Bible v0.1 zapisany; do dalszego rozwinięcia |
| 2 | Kosmologia | Reguły sfer, przejść, ograniczeń i świadomie otwarte tajemnice | Kosmologia v0.1 zapisana |
| 3 | Pełny katalog panteonu | Rejestr źródeł, wariantów, niepewności i uzasadnionych pominięć | Katalog v0.1: 52 pozycje; badania pozostają otwarte |
| 4 | Historie bogów | Karty wybranych bóstw, cele, sprzeczności kultów i relacje F | Następny etap; bez ustalonej pełnej genealogii |
| 5 | Historia świata | Osobno zdarzenia, świadectwa i konkurencyjne interpretacje | Planowane |
| 6 | Kultury | Życie materialne, języki, wierzenia i wewnętrzne różnice | Planowane |
| 7 | Królestwa i regiony | Geografia, zasoby, zależności, granice oraz konflikty | Planowane; model lokalnego regionu F istnieje |
| 8 | Magia | Źródła mocy, koszty, nauka, alfabet i ograniczenia | Zarys w kosmologii; pełny Magic Bible planowany |
| 9 | Potwory | Źródła, ekologia, zachowania i alternatywy wobec zabijania | Planowane |
| 10 | Bohater | Kreator, zawód, rozwój i dopuszczalne style gry | Założenia istnieją; szczegółowy projekt planowany |
| 11 | Rodzina bohatera | Tajemnica, dowody, tempo odkrywania i wpływ na wybory | Otwarte |
| 12 | Główna historia | Konflikt, akty i ścieżka bez boskiego patrona | Planowane |
| 13 | Zadania | Pełna pętla, stany, rozwiązania i sposób nagradzania | Jedno zadanie F rozpisane do wycinka; reszta planowana |
| 14 | Decyzje | Wskazane strony, skutki i momenty odczuwalnej zmiany | Zarys w wycinku; globalny model planowany |
| 15 | Zakończenia | Wyniki zależne od historii działań i relacji | Otwarte |
| 16 | Mapa świata | Gęste regiony, połączenia, rytm odkrywania | Planowane |
| 17 | Projekt rozgrywki | Spójne systemy, ograniczenia i kryteria wycinka | Specyfikacja wycinka v0.1 istnieje; pełny projekt planowany |
| 18 | Architektura silnika | Granice systemów, dane, zasoby, stan i zapis | Fundament v0.1 przygotowany: moduły systemów, wspólny postęp i wersjonowany checkpoint; backendy assetów/audio/animacji oraz pełne integracje pozostają do rozwoju |
| 19 | Vulkan | Działający rendering oraz określone ograniczenia sprzętowe | Podstawowy rendering działa |
| 20 | Prototyp 3D | Teren, kamera, ruch i podstawowe pomiary | Działa; weryfikacja wejścia na sprzęcie użytkownika nadal otwarta |
| 21 | Grywalny wycinek | Wszystkie kryteria z VerticalSlice spełnione | Specyfikacja istnieje; implementacja planowana |
| 22 | Rozbudowa świata | Następne regiony o jakości potwierdzonej wycinkiem | Planowane |
| 23 | Pełna gra | Ukończona treść, testy, optymalizacja i wydanie | Planowane |

„Pełny katalog” jest celem badawczym, a nie obietnicą odkrycia jednej zamkniętej listy historycznych bogów. Etap 3 można domknąć dla zakresu projektu po sprawdzeniu źródeł i jawnej ocenie kandydatów; nowe odkrycia pozostają możliwe.

## Najbliższy pakiet pracy

1. Sprawdzić brakujące wydania źródeł dla wpisów Rod, Rodzanice, późnego katalogu polskiego i kandydatów literackich. Uzupełnić miejsca tekstu, nie same linki do stron.
2. Zweryfikować kandydatów w kolejce oraz relacje Swarożyc–Radegast, Siwa–Żywie i Jarowit–Jarilo–Zeleni Jurij. Udokumentować rozbieżności zamiast scalać je domyślnie.
3. Rozpisać osobne karty Peruna, Welesa, Mokoszy, Świętowita, Swarożyca i Trygława. Każda karta oddziela minimum źródłowe od autorskiej osobowości, kultu i interesów.
4. Zbudować matrycę relacji F między tymi bóstwami: wspólne interesy, konflikty, neutralność i punkty sporne. Nie zakładać jednego podziału dobra i zła.
5. Po kartach bogów rozpocząć Timeline: wydarzenie, datowanie w świecie, świadek, pewność i skutki. Historia Czwartej Sfery i rodziny pozostaje otwarta do właściwych etapów.

## Warunki jakości dokumentacji

Przed zakończeniem pakietu sprawdzamy unikalne identyfikatory, warianty nazw, odsyłacze, zakres konsultacji źródeł i spójność terminów. Zmiana projektu F nie wymaga podnoszenia jej do rangi twierdzenia historycznego. Zmiana klasy świadectwa wymaga wyjaśnienia i źródła.

## Stałe techniczne

Projekt pozostaje na **.NET 11**. Podstawowy renderer używa **Vulkan przez Veldrid**; silnik i systemy gry powstają w C#. Najpierw PC, Android później po ocenie wydajności, wejścia i potrzeb platformy. Dokumentacja świata nie jest migracją frameworka ani deklaracją gotowości wszystkich wymienionych systemów.
