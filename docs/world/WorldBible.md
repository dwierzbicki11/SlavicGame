# World Bible

**Wersja 0.1 — fundament świata.** Zasady wynikające z wizji oznaczono jako ustalone; nowe szczegóły jako projekt F. Całość opisuje fikcyjny świat. Bibliografię inspiracji zawiera [rejestr źródeł](../pantheon/Sources.md).

## Ustalone założenia

Gracz jest człowiekiem i łowcą potworów. Rozpoczyna od pracy, pieniędzy na utrzymanie i lokalnych problemów. Nie otrzymuje na początku tytułu wybrańca. Tworzy postać, wybiera imię i rozwija jej umiejętności; z czasem odkrywa nieznaną historię biologicznej rodziny.

Świat ma własną geografię, kultury i politykę. Inspiracja IX–X wiekiem określa kierunek życia materialnego, nie rzeczywistą datę wydarzeń. Źródło spisane w XII albo XIX wieku zachowuje tę datę w dokumentacji, nawet kiedy jego motyw trafia do gry. Obok kultur inspirowanych Słowianami istnieją inne kultury.

Dzień ma pokazywać piękno i wiarygodną codzienność. Noc zmienia zachowanie świata: drogi, spotkania, ryzyko, usługi i dostępne informacje. Nie każda noc jest atakiem na wieś. Nie każdy duch jest wrogiem, nie każdy potwór zasługuje na zabicie, a każdy bóg ma własne interesy.

Eksploracja odbywa się w dużych, gęsto zaprojektowanych regionach. Najpierw powstaje jeden działający region. Jego rozbudowa wynika z jakości przygód, a nie z liczby kilometrów mapy.

## Filary i ich skutki dla rozgrywki

| Filar | Obietnica dla gracza | Warunek projektu |
|---|---|---|
| Zawód łowcy | Zrozumiem zagrożenie, zanim wybiorę rozwiązanie | Tropienie i przygotowanie mają znaczenie obok walki |
| Żyjąca wspólnota | Ludzie pamiętają moje czyny | Wynik zadania zmienia przynajmniej jedno zachowanie NPC lub stan miejsca |
| Niepewna wiedza | Mogę sprawdzić opowieść i odkryć pomyłkę | Plotka, obserwacja i potwierdzony fakt mają osobne wpisy w dzienniku |
| Noc i granice sfer | Znane miejsce może wymagać innego planu | Zmiana ma czytelne oznaki, reguły i możliwość odwrotu |
| Wiara i odpowiedzialność | Mogę przyjąć, negocjować lub odrzucić umowę | Główna ścieżka dopuszcza brak boskiego patrona |
| Rodzina i tożsamość | Odkrycie pochodzenia nie odbierze mi wyboru | Historia rodziny nie narzuca charakteru ani wszystkich decyzji bohatera |

## Codzienność — projekt F

Większość ludzi żyje z rolnictwa, hodowli, lasu, rybołówstwa, rzemiosła i handlu. Dostęp do drogi, bezpiecznej przeprawy, młyna czy warsztatu jest źródłem zależności między osadami. Grody i większe ośrodki wymagają zaplecza gospodarczego. Gotowy model konkretnej kultury musi później dostać osobne badanie ubioru, budownictwa, narzędzi i obrzędów.

Religia jest widoczna w gospodarstwach, podróży i życiu wspólnoty. Mieszkańcy mogą czcić kilku patronów, kierować się obyczajem albo odmawiać udziału w kulcie. Świątynia nie reprezentuje automatycznie wszystkich mieszkańców regionu. Bogaty ośrodek kultowy może także kontrolować handel, dostęp do ziemi i informacje.

Podróżnik spotyka różne języki, zwyczaje i sposoby traktowania obcych. Kultury nie odpowiadają prostym etykietom moralnym. Przyjaźń, konflikt i handel mogą przebiegać zarówno między nimi, jak i wewnątrz każdej z nich.

## Model regionu startowego — projekt F

Robocza nazwa regionu: **Pogranicze Żarnowca**. To nowy projekt lokalnej przestrzeni; nazwa całego świata i ostateczny tytuł gry pozostają otwarte. Nazwy miejsc poniżej już istnieją w `WorldState`; ich funkcje fabularne są planowane.

| Miejsce | Identyfikator w prototypie | Rola za dnia | Rola nocą |
|---|---|---|---|
| Żarnowiec | `old-village` | Zlecenia, targ, warsztat, pogłoski i odpoczynek | Zamknięte gospodarstwa, straż i świadkowie odmiennych zdarzeń |
| Puszcza Żywia | `starting-forest` | Tropienie, zbieranie, pracownicy lasu | Zmiana tropów, duchy, miejsca wymagające rozpoznania |
| Czarne Mokradła | `black-swamp` | Przeprawa, rybacy, pozostałości dawnych pochówków | Zwodnicze światła i lokalna nieszczelność granicy Nawii |
| Kamienny Krąg | `old-shrine` | Badanie znaków i nauka pierwszego obrzędu | Kontakt poprzez przejaw lub posłańca, zależny od przygotowania |

„Żywia” w nazwie puszczy jest nazwą fikcyjną. Nie stanowi dowodu kultu Siwy/Żywy ani rozstrzygnięcia jej związku z Żywie.

Docelowy układ zawiera bezpieczniejszą drogę między wsią i puszczą, krótszą ryzykowną przeprawę przez mokradła oraz boczną ścieżkę do kręgu. To projekt topologii do późniejszego zbudowania; obecne okręgi wykrywania regionów w kodzie nie są gotową mapą ani zabudową osad.

## Ludzie regionu — projekt F

Pierwszy region potrzebuje niewielkiej liczby osób z wyraźnymi celami. Robocze role, bez ostatecznych imion:

| Rola | Czego chce | Co może zmienić działanie gracza |
|---|---|---|
| Osoba zlecająca poszukiwanie | Poznać los zaginionego bliskiego | Zaufanie, sposób pochówku, wynagrodzenie i późniejsza pomoc |
| Opiekun przeprawy | Utrzymać ruch i dochód | Otwarcie drogi, ceny i stosunek do umów z istotami |
| Zielarka | Zabezpieczyć ludzi bez niszczenia mokradeł | Receptury, dostęp do składników i pomoc przy rytuale |
| Strażnik wspólnoty | Zapewnić bezpieczeństwo wsi | Warty, tolerancja wobec obcych i reakcja na nieumarłych |
| Opiekun miejsca kultowego | Zachować reguły danego miejsca | Interpretacja śladów, dostęp do obrzędu i ocena złamanych zobowiązań |

Nie wszyscy muszą przeżyć lub polubić bohatera. Pierwszy wycinek pokaże zmianę zachowania jednej wspólnoty, zanim powstanie pełna symulacja mieszkańców.

## Dzień, zmierzch, noc i świt — projekt F

Zmierzch daje ostrzeżenie: mieszkańcy wracają, ruch na drodze słabnie, a określone miejsca emitują oznaki zbliżającego się zdarzenia. Noc uruchamia zaplanowane spotkania i wybrane anomalie. Świt wygasza część zjawisk, ale pozostawia ślady i skutki decyzji.

Pierwszy wycinek używa stałego, czytelnego harmonogramu. Późniejsze zdarzenia mogą zależeć od pogody, stanu regionu, wcześniejszych rytuałów i wykonanych zadań. Teren nie zmienia się losowo pod stopami bez uprzedniej oznaki. Zegar i stan zadania muszą zapewniać ponowną okazję, jeśli gracz przegapi spotkanie.

## Pętla zawodu

**Zlecenie → rozmowy i oględziny → hipoteza → przygotowanie → spotkanie → rozwiązanie → powrót i konsekwencje.**

Zapłata może być pieniężna, rzeczowa lub przyjąć postać dostępu do usługi. Zmieniona sytuacja wsi ma być widoczna po powrocie. Dziennik przechowuje dowody oraz rozważane rozwiązania; nie musi od razu podawać poprawnej nazwy istoty.

## Granice zakresu

Pierwszy region demonstruje tropienie, walkę, rozmowę, nocne zdarzenie, jeden obrzęd i zapis konsekwencji. Pełna gospodarka, rozrost osad, romanse, wszystkie kultury, liczne boskie krainy i rozległa Nawia należą do dalszych etapów. Są częścią docelowej wizji, nie stanem obecnego prototypu.

Nie rozstrzygamy jeszcze genealogii bogów, końcowej geografii, pochodzenia bohatera ani przyczyny kryzysu granic. Następne dokumenty rozwijamy zgodnie z [kolejnością produkcji](../design/ProductionRoadmap.md).
