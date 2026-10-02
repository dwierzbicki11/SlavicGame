# R5 — Równiny Arel: production region bible v0.1

## Status i rola

R5 jest opcjonalnym pełnym regionem albo dużym rozdziałem Aktu II. Jego funkcja fabularna jest obowiązkowa nawet wtedy, gdy zakres mapy zostanie zmniejszony: Arelowie zachowali inną część pamięci o dawnym systemie i znają stare drogi nieobecne na mapach Nadborza.

Region rozwija temat **granicy jako ruchu, nie linii**. Nie może być przedstawiony jako pusta przestrzeń pomiędzy ważniejszymi lokacjami.

## Cele produkcyjne

R5 ma dostarczyć:
- kontrast wobec grodowego Nadborza i zamkniętych stref Wielkiego Boru;
- eksplorację opartą na orientacji w otwartym terenie, ruchomych punktach odniesienia i wiedzy lokalnej;
- stałe osady oraz sezonowe obozy bez sugerowania, że cała kultura jest jednolicie osiadła albo jednolicie mobilna;
- szerokie trasy handlowe i stare drogi;
- materialny fragment wiedzy o sieci dawnych progów;
- możliwość skrócenia regionu bez utraty jego funkcji kampanijnej.

## Topologia

### A. Zachodni trakt

Wejście od strony Nadborza. Najbardziej czytelna infrastruktura: droga, posterunki lub punkty poboru, zajazd/stanica zależnie od finalnego content locku oraz ruch kupiecki.

Funkcja:
- wprowadzenie różnicy między mapą administracyjną a drogami używanymi faktycznie;
- pierwszy kontakt z lokalnymi przewodnikami;
- bezpieczny punkt powrotu.

### B. Pas osad stałych

Kilka skupisk osadniczych związanych z wodą, rzemiosłem i wymianą. Nie projektujemy jednego miasta jako substytutu całego regionu.

Funkcja:
- usługi i handel;
- polityka lokalna;
- informacje o zmianach tras;
- kontrakty poboczne.

### C. Szerokie równiny

Największa przestrzeń eksploracyjna. Widoczność jest daleka, ale orientacja nie może sprowadzać się do biegu po markerze HUD.

Punkty odniesienia:
- kępy drzew;
- cieki i podmokłe obniżenia;
- wyniesienia;
- pale/słupy drogowe lub inne znaczniki świata gry po potwierdzeniu ich formy;
- dym, światło i ruch obozów;
- sylwetki stałych miejsc w oddali.

### D. Trasy sezonowe i obozy

Obozy mogą zmieniać stan lub położenie w ramach skończonego zestawu przygotowanych wariantów. Nie wymagamy pełnej proceduralnej migracji NPC po kontynencie.

Zmiana wariantu musi być deterministyczna względem stanu świata i zapisywalna.

### E. Stare drogi

Drogi, których oficjalne mapy Nadborza nie pokazują. Część jest fizycznie czytelna dopiero po użyciu trackingu, rozmowie lub zestawieniu evidence.

Ich funkcja nie jest wyłącznie transportowa: przechowują fragment wiedzy o dawnym systemie.

### F. Stare stanowiska

MacroGeography dopuszcza kurhany/stare stanowiska, ale ich konkretna forma wymaga osobnego researchu kulturowego. Do czasu zamknięcia researchu używamy neutralnego terminu `old-site` i nie kopiujemy przypadkowych form archeologicznych.

## Traversal i nawigacja

R5 wykorzystuje istniejące systemy mapy, trackingu, pogody i day/night.

Zasady:
- główna droga daje prostą nawigację, ale nie prowadzi do wszystkich celów;
- lokalna wiedza może odsłonić trasę, punkt orientacyjny lub bezpieczniejszy wariant przejścia;
- burza, mgła przy gruncie, noc lub zalanie mogą czasowo zmieniać czytelność tras, lecz nie mogą losowo usuwać postępu;
- gracz zawsze ma recoverable route do bezpiecznego punktu;
- fast travel, jeśli dostępny, korzysta wyłącznie z odkrytych węzłów.

## Pogoda i pora dnia

Najważniejsze efekty gameplayowe:
- wiatr wpływa na czytelność dźwięku i efektów środowiskowych;
- ulewa może pogorszyć ślady i zmienić przejezdność niskich tras;
- noc zwiększa znaczenie ognia, obozów i stałych landmarków;
- pogoda nie może tworzyć nieodwracalnego fail state.

Dokładne częstotliwości i parametry pozostają do tuningu po playtestach.

## Ludzie i frakcje

Minimalne role contentowe:
- lokalny przewodnik znający stare drogi;
- przedstawiciel stałej osady;
- osoba związana z sezonowym obozem;
- kupiec lub przewoźnik łączący R5 z innymi regionami;
- przedstawiciel interesów Nadborza albo innej zewnętrznej władzy;
- lokalny specjalista od pamięci tras/miejsc, bez automatycznego robienia z niego kapłana.

Nie zakładamy jednej opinii „Arelów”. Konflikty mają wynikać z interesów, dostępu do tras, zasobów, pamięci i relacji z sąsiadami.

## Gospodarka

R5 uczestniczy w gospodarce międzyregionalnej przez:
- transport lądowy;
- zwierzęta i produkty odzwierzęce tylko w zakresie potwierdzonym finalnym pakietem kultury materialnej;
- żywność i surowce lokalne;
- usługi przewodnickie i logistyczne;
- wymianę informacji o drogach.

Ceny i dokładny katalog dóbr pozostają własnością economy/content data, nie region bible.

## Encounter families

1. **Road encounter** — podróżni, kontrola trasy, handel, prośba o pomoc.
2. **Weather encounter** — zmiana przejścia lub konieczność znalezienia osłony.
3. **Tracking encounter** — ślad prowadzący poza oficjalną drogę.
4. **Camp encounter** — stan zależny od relacji, czasu i wcześniejszych decyzji.
5. **Old-site encounter** — evidence, anomalia albo zagrożenie związane ze starą siecią.
6. **Wildlife/supernatural** — tylko z rosteru zatwierdzonego dla regionu; brak nowych stworzeń ad hoc.

Każda rodzina musi mieć co najmniej jeden wariant bez walki. Encounter nie może zakładać, że gracz posiada konkretną umiejętność bez fallbacku.

## Główna kampania

### Wejście

Gracz trafia do R5 z wiedzą, że znane mapy i polityczne archiwa opisują sieć niepełnie.

### Evidence flow

1. Oficjalna mapa lub relacja pokazuje oczekiwaną trasę.
2. Lokalna wiedza wskazuje drogę nieobecną w tym obrazie.
3. Tracking/eksploracja potwierdza, że stara droga jest fizycznie realna.
4. Stare stanowisko lub punkt techniczno-rytualny wiąże drogę z szerszą siecią.
5. Gracz zapisuje wniosek: inne kultury zachowały inny fragment pamięci o systemie.

### Wyjście

Funkcja kampanijna R5 jest spełniona po zdobyciu tego pakietu evidence. Nie wymaga oczyszczenia całej mapy ani wykonania side contentu.

## Side content

Kategorie:
- zaginiona lub spóźniona grupa na trasie;
- konflikt o dostęp do przejścia/wody;
- błędna oficjalna mapa kontra wiedza lokalna;
- pomoc przy zmianie miejsca obozu;
- kontrakt na zagrożenie wykorzystujący otwartą przestrzeń;
- mała historia starego miejsca niezwiązana bezpośrednio z główną tajemnicą.

Side quest nie może ujawniać centralnej prawdy wcześniej niż RevelationPlan.

## Persistence

Zapisywany stan regionu powinien obejmować co najmniej:
- odkryte landmarki i węzły podróży;
- poznane stare drogi;
- warianty sezonowych obozów istotne dla contentu;
- zakończone encountery i questy;
- trwałe skutki decyzji;
- campaign evidence;
- late-game state.

Zmiana wariantu obozu po loadzie nie może duplikować NPC, lootów ani quest giverów.

## Late-game state

Powrót do R5 przed finałem powinien pokazać co najmniej jedną czytelną konsekwencję kryzysu i wcześniejszych decyzji, np. zmianę bezpieczeństwa tras, przepływu ludzi albo dostępności obozów. Konkretna macierz zależy od questów regionalnych i nie jest tutaj zamrażana.

## Asset families

Minimalne rodziny assetów:
- modularne zabudowania stałych osad;
- zestaw sezonowego obozu;
- drogi, ścieżki i znaczniki nawigacyjne;
- roślinność równin i skupiska drzew;
- cieki/woda i podmokłe obniżenia;
- wozy, pakunki i propsy podróżne po zatwierdzeniu researchu;
- warianty old-site dopiero po research locku;
- landmarki dalekiego zasięgu;
- zestawy VFX/SFX wiatru, deszczu i nocnych punktów orientacyjnych.

Modele powinny preferować modularność i reużycie; region nie uzasadnia produkcji unikalnego assetu dla każdego obozu.

## Streaming i performance

Otwarte równiny wymagają:
- LOD/HLOD lub równoważnego systemu;
- kontrolowanego zasięgu aktywnej AI;
- rozdzielenia dalekiej reprezentacji obozu od pełnej symulacji NPC;
- limitów roślinności i shadow casters;
- profilowania widoków o bardzo dużym zasięgu.

Liczby pozostają otwarte do pomiarów na docelowym rendererze i sprzęcie testowym.

## QA acceptance v0.1

Region bible jest implementowalne, jeśli:
1. kampanijny evidence flow działa bez side questów;
2. gracz może odzyskać orientację po pogodzie/nocy;
3. stare drogi nie wymagają markera GPS do odkrycia;
4. warianty obozów są deterministyczne i poprawnie zapisują się/wczytują;
5. każdy obowiązkowy encounter ma fallback;
6. region można skrócić produkcyjnie bez utraty głównego odkrycia;
7. late-game return nie resetuje wcześniejszych decyzji;
8. nie wprowadzono niezweryfikowanej archeologii jako faktu historycznego.

## Otwarte decyzje

- czy R5 pozostaje pełnym regionem, czy dużym rozdziałem;
- dokładna liczba stałych osad i wariantów obozów;
- finalny regionalny roster zwierząt i istot;
- forma `old-site` po osobnym researchu;
- szczegółowy pakiet kultury materialnej Arelów;
- nazwy lokalnych miejsc i NPC po naming pass;
- liczba side questów;
- parametry pogody, odległości i performance po pomiarach;
- dokładna macierz late-game consequences.

Żadna z tych decyzji nie blokuje implementacji podstawowej topologii, traversal contractu ani campaign evidence flow v0.1.
