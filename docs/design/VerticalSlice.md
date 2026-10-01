# Pierwszy grywalny wycinek

**Projekt F, wersja 0.1.** To specyfikacja przyszłego działania. Prototyp ma teren, kamerę, ruch, zegar, rozpoznawanie regionów, pierwszą pogodę oraz fundament zdrowia/staminy; nie ma jeszcze opisanych tutaj NPC, walki ani zadania.

## Cel

Gracz wykonuje jedno pełne zlecenie łowcy w Pograniczu Żarnowca. Bada ślady, przygotowuje się, wraca do znanego miejsca nocą, wybiera rozwiązanie i widzi konsekwencje po powrocie. Roboczy czas pierwszego przejścia: 30–45 minut, do sprawdzenia dopiero po implementacji.

## Przestrzeń i zawartość

| Miejsce | Minimum do zbudowania | Powód powrotu |
|---|---|---|
| Żarnowiec | Kilka gospodarstw, warsztat, zielarka, miejsce odpoczynku i 5 ról NPC | Oddanie zlecenia, zmieniony ruch na przeprawie, reakcja rodziny |
| Puszcza Żywia | Dwie trasy, obóz, tropy i jeden neutralny strażnik lasu | Nowy ślad po zmierzchu i składnik do obrzędu |
| Czarne Mokradła | Przeprawa, miejsce śmierci, zjawa i jeden silniejszy przeciwnik | Nocny kontakt i rozstrzygnięcie zadania |
| Kamienny Krąg | Miejsce nauki i wykonania obrzędu | Opcjonalna umowa oraz wynik przygotowania |

Neutralny strażnik lasu, zjawa i przeciwnik są roboczymi rolami bestiariusza F. Historyczne nazwy oraz konkretną inspirację wybieramy dopiero z udokumentowanej karty istoty.

## Zlecenie: „Światło nad mokradłem”

Ktoś z Żarnowca nie wrócił z przeprawy. Mieszkańcy widują światło i słyszą wołanie. Opiekun przeprawy oczekuje szybkiego usunięcia zagrożenia, rodzina chce poznać prawdę, a zielarka obawia się zniszczenia miejsca. Bohater przyjmuje zwykłe zlecenie zawodowe.

Stan faktyczny zadania F: zaginiona osoba zmarła; pozostawiony przedmiot wiąże jej zjawę z lokalnym echem. Osobna drapieżna istota korzysta ze zjawiska. Zjawa i przeciwnik nie są tym samym bytem. Ten lokalny problem nie ujawnia przyczyny całego kryzysu świata.

### Przebieg

1. **Rozmowy:** rodzina, opiekun przeprawy i zielarka przekazują różne wersje. Dziennik oznacza je jako wypowiedzi świadków.
2. **Oględziny za dnia:** gracz odnajduje tropy, uszkodzenie przeprawy i przedmiot zaginionego. Dowody potwierdzają, że istnieją dwa rodzaje śladów.
3. **Przygotowanie:** zebranie składnika, prostej mikstury i poznanie warunków rytuału. Umowa z patronem jest opcjonalna.
4. **Powrót po zmierzchu:** uprzednio widoczne oznaki wskazują aktywne miejsce. Przegapienie pory pozwala poczekać do kolejnego cyklu.
5. **Rozpoznanie:** kontakt ze zjawą i oddzielne spotkanie z przeciwnikiem ujawniają zależność między światłem, przedmiotem i nieszczelnością.
6. **Decyzja:** obrzęd, umowa lub zniszczenie kotwicy. Sposób postępowania z drapieżnikiem pozostaje osobnym wyborem taktycznym.
7. **Powrót:** dialog rodziny oraz stan przeprawy pokazują skutki. Zapis zachowuje wynik i zamyka zlecenie tylko raz.

### Trzy rozwiązania F

| Rozwiązanie | Wymaganie | Zysk | Koszt / widoczny skutek |
|---|---|---|---|
| Domknięcie obrzędem | Rozpoznanie zmarłego, przedmiot i składniki | Zjawa odchodzi; przeprawa wraca do zwykłego użycia | Czas i składniki; rodzina uzyskuje potwierdzony los bliskiego |
| Warunkowa umowa ze zjawą | Dowody tożsamości i wysłuchanie jej prośby | Zjawa pozostaje pomocna w określonych godzinach | Nocny ruch nadal ograniczony; lokalni przeciwnicy umowy tracą zaufanie |
| Zniszczenie kotwicy | Dotarcie do przedmiotu i wybór przerwania więzi | Natychmiastowe wygaszenie lokalnego światła | Utrata części świadectwa zmarłego i gorsza reakcja rodziny |

Wszystkie trzy drogi pozwalają ukończyć zlecenie. Żadna nie wymaga przynależności do kultu. Zabicie drapieżnika nie rozwiązuje samo więzi zjawy; wynik walki i wynik kontaktu są zapisywane oddzielnie. Kapłan lub posłaniec może oceniać rozwiązanie inaczej niż mieszkańcy.

## Systemy potrzebne do tego wycinka

| System | Minimum | Stan teraz |
|---|---|---|
| Ruch i kamera | Jednoczesne poruszanie, obrót, bieg, stabilna prędkość | Zaimplementowane; potrzebna dalsza weryfikacja na komputerze użytkownika |
| Teren i regiony | Przejście między czterema miejscami z kolizją | Teren i wykrywanie regionów istnieją; zabudowa oraz przeszkody planowane |
| Pora dnia | Czytelna zmiana oświetlenia i okna zdarzeń | Zegar istnieje; oświetlenie i zdarzenia planowane |
| Pogoda | Pogoda spokojna i mgła z czytelną widocznością | Pierwsza wersja działa: regionalne przejścia, mgła atmosferyczna, zachmurzenie i deszcz/storm jako stan; brak jeszcze cząsteczek opadu |
| Walka | Broń biała, blok, unik, stamina, zdrowie; osobno prosty łuk | Fundament zdrowia i staminy działa; sprint zużywa staminę, walka nadal planowana |
| AI | Patrol / ostrzeżenie / atak / powrót przeciwnika | Planowane |
| Tropienie | Trzy ślady i rozróżnienie obserwacji od plotki | Planowane |
| Ekwipunek i alchemia | Przedmiot zadania, składniki i jedna receptura | Planowane |
| Magia i rytuał | Jeden czar oraz jeden obrzęd z kosztem i warunkami | Planowane |
| NPC i dialog | Pięć ról, dwie pory harmonogramu i dialog po decyzji | Planowane |
| Zadania i reputacja | Trzy zakończenia z oddzielnym stanem stron | Planowane |
| Zapis | Checkpoint i odczyt całego stanu zadania | Planowane |

## Kolejność implementacji po dokumentacji

1. Postać, kolizje z przeszkodami, zdrowie i prosty przeciwnik; ruch oraz kamera działają również podczas walki.
2. Interakcja z przedmiotem i NPC, minimalny dialog, ekwipunek i stan zadania.
3. Zdarzenia zależne od zegara, oświetlenie, mgła i dwa harmonogramy NPC.
4. Tropienie, koszt obrzędu, pierwszy czar, receptura i rozwiązania zadania.
5. Reputacja stron, zmiana przeprawy, checkpoint i pełne przejście każdej ścieżki.
6. Prosty łuk, strojenie walki, dźwięk oraz ocena czytelności całego wycinka.

Ta kolejność jest planem technicznym przyszłego etapu 21. Nie zastępuje kolejności projektowania świata i historii.

## Kryteria odbioru

- Gracz wykonuje całą pętlę bez komend debugowania; wejście myszą i ruchem pozostaje jednoczesne w eksploracji i walce.
- Każdy z trzech wyników da się osiągnąć z nowego zapisu i zobaczyć po powrocie do wsi.
- Każdy wynik ma przynajmniej jedną zmianę dialogu i jedną zmianę zachowania lub stanu miejsca.
- Dzień i noc różnią się zdarzeniami oraz zachowaniem NPC. Nocne miejsce jest zapowiedziane przed wejściem w zagrożenie.
- Ślady pozwalają odróżnić zjawę od drapieżnika. Przygotowanie daje praktyczną korzyść.
- Przegapienie nocy, odmowa patrona i nieudany obrzęd nie blokują ukończenia gry.
- Śmierć przywraca spójny checkpoint; odczyt zachowuje czas, przedmioty, reputację i wynik zadania.
- Oddanie zadania, nagroda i zmiana reputacji następują najwyżej raz, także po zapisie i odczycie.
- Dokumentowane nazwy historyczne mają źródła; autorskie rytuały, znaki i przeciwnicy są oznaczone F w materiałach projektu.

Na tym etapie nie ustalamy minimalnego FPS bez pomiarów i wybranej maszyny odniesienia. Zbieramy czas klatki, czas ładowania i przypadki utraty wejścia przed strojeniem wydajności.
