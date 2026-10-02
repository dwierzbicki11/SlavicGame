# Region Bible R3 — Przymorze

Status: production first pass v0.1

## Rola regionu

Przymorze jest regionem ujścia Dębrzy i wybrzeża. W kampanii pełni funkcję Aktu II-A: rozszerza skalę świata z konfliktów śródlądowych na handel, przepływ informacji i kontakty międzykulturowe. Region ma pokazać, że kryzys sieci starych miejsc nie jest lokalny.

Ten dokument integruje istniejące owner specs. Nie redefiniuje ekonomii, questów, kultur, łodzi ani głównego lore, jeśli mają własny dokument.

## Funkcje produkcyjne

R3 musi zapewnić:
- pierwszy duży port i czytelny ruch towarów;
- kontakt z wielokulturowymi NPC;
- regionalną gospodarkę zależną od ujścia Dębrzy;
- ograniczoną podróż łodzią bez obietnicy pełnej symulacji żeglugi;
- stare mapy kupieckie jako dowód rozszerzający znany obraz sieci;
- kontrast pomiędzy kontrolą informacji a praktyczną wiedzą kupców i żeglarzy;
- late-game return pokazujący konsekwencje kryzysu.

## Topologia

### Solny Bród

Główny hub regionu. Powinien łączyć:
- nabrzeże rzeczne;
- nabrzeże przybrzeżne;
- targ i składy;
- punkt poboru opłat lub kontroli ruchu;
- dzielnicę rzemieślniczą;
- miejsca noclegowe dla przyjezdnych;
- przestrzeń religijną odpowiadającą lokalnym kultom;
- czytelne dojście do szlaku lądowego w stronę Nadborza.

Hub ma być gęstszy od Żarnowca, ale nie może wymagać symulacji pełnego miasta portowego.

### Mniejsze porty

Co najmniej jeden mniejszy port służy jako kontrast dla Solnego Brodu. Funkcje możliwe do rozdzielenia między porty:
- rybołówstwo;
- lokalny handel;
- przemyt;
- naprawy łodzi;
- sezonowe postoje.

Finalna liczba portów pozostaje decyzją produkcyjną.

### Wyspy

Wyspy są selektywnymi przestrzeniami eksploracji, nie pełnym archipelagiem. Mogą przechowywać:
- stare miejsca;
- punkty nawigacyjne;
- małe osady lub sezonowe obozy;
- ślady dawnych tras.

Dostęp zależy od stanu pogody, łodzi i fabuły tam, gdzie wymaga tego owner questa.

### Słone mokradła

Strefa przejściowa pomiędzy wodą i lądem. Gameplayowo powinna wspierać:
- tracking;
- ograniczoną widoczność;
- ryzyko terenowe;
- gathering;
- encountery zależne od pory i pogody.

Nie kopiujemy Czarnych Mokradeł z R0: flora, audio, ekspozycja na wiatr i gospodarcze wykorzystanie terenu muszą odróżniać region.

### Klify

Klify budują sylwetę wybrzeża i zapewniają punkty obserwacyjne. Traversal ma pozostać zgodny z ogólnym movement spec; nie wprowadzamy osobnego systemu wspinaczki tylko dla R3 bez zatwierdzonego ownera.

## Przepływ kampanii

Akt II-A powinien prowadzić przez trzy warstwy informacji:
1. oficjalne dane portowe pokazują zakłócenia handlu i blokady tras;
2. praktyczna wiedza żeglarzy i kupców pokazuje rozbieżności względem oficjalnego obrazu;
3. stare mapy kupieckie ujawniają punkty odpowiadające znanej sieci również poza jej dotychczasowym centrum.

Reveal musi działać jako dowód, nie ekspozycyjny monolog. Konkretne flagi i kolejność scen należą do kart main questów.

## Gospodarka i handel

Region wizualizuje istniejącą gospodarkę międzyregionalną przez:
- sól i towary konserwowane;
- ryby i produkty wybrzeża;
- towary spławiane Dębrzą;
- importowane materiały i przedmioty;
- magazyny, cła, przewóz i ryzyko przerwania szlaków.

Ceny, marże i vendor inventory pozostają własnością economy/vendor specs. Bible określa tylko regionalny kontekst podaży i niedoborów.

## Łodzie i podróż wodna

Minimalny zakres produkcyjny:
- punkty wejścia/wyjścia;
- kontrolowane trasy;
- możliwość użycia krótkiej podróży, loadingu lub sekwencji przejścia;
- warunki blokady trasy przez pogodę lub fabułę;
- bezpieczny fallback do podróży lądowej tam, gdzie kampania nie wymaga łodzi.

Pełne swobodne sterowanie łodzią jest decyzją otwartą, a nie wymaganiem tego bible.

## Ludzie i kultura

R3 musi wiarygodnie pokazywać większą mieszankę pochodzenia niż regiony śródlądowe. Roster powinien zawierać funkcje takie jak:
- lokalny zarządca/urzędnik portowy;
- kupiec dalekiego zasięgu;
- przewoźnik lub sternik;
- rzemieślnik związany z wodą/łodziami;
- osoba utrzymująca wiedzę o dawnych trasach;
- mieszkańcy utrzymujący się z rybołówstwa i mokradeł;
- przyjezdni reprezentujący zatwierdzone kultury świata.

Nazwy, języki i pochodzenie muszą używać `NamingRules.md` i odpowiednich kart kultur; nie tworzymy ad hoc pseudo-historycznych etnonimów.

## Religia i stare miejsca

Miejsca kultowe mają wynikać z istniejących instytucji kultowych i regionalnych potrzeb ludzi związanych z wodą, podróżą i handlem. Nie przypisujemy historycznych praktyk bez research card.

Stare miejsca regionu powinny wspierać główny motyw sieci, ale nie każde jest świątynią. Ich materialne ślady muszą być kompatybilne z revealem z R2 i późniejszymi regionami.

## Encounter families

R3 powinno używać co najmniej następujących rodzin encounterów:
- port/social — spory, kontrole, kontrakty, informacja;
- route disruption — blokady, utracone dostawy, zmienione trasy;
- coast wilderness — klify, mokradła, wyspy;
- supernatural trace — anomalie powiązane z siecią;
- criminal/opportunistic — przemyt, rabunek lub wykorzystanie kryzysu tam, gdzie wspiera to quest/content spec.

Każdy konkretny encounter używa wspólnego encounter formatu. Bible nie ustala liczbowego tuningu walk.

## Dzień, noc i pogoda

Dzień wzmacnia handel, pracę portu i ruch cywilny. Noc ogranicza część usług, zwiększa znaczenie patroli, przemytniczych tras i supernatural encounters.

Pogoda powinna wpływać na:
- widoczność na wodzie;
- dostępność kontrolowanych tras łodzi;
- fale/wiatr jako prezentację i hazard zgodny z zatwierdzonym systemem;
- zachowanie mokradeł;
- ruch NPC i statków jako warstwę world state.

Nie tworzymy lokalnego systemu pogody konkurującego z `WeatherGameplay.md`.

## Travel i gating

Wejście z Nadborza powinno być czytelne lądowo/rzecznie. Region może odblokować dalsze przejścia dopiero po odpowiednim stanie kampanii.

Gating może opierać się na:
- stanie questa;
- dostępności przewozu;
- pogodzie, jeśli istnieje deterministyczny fallback;
- stanie politycznym lub blokadzie szlaku.

Gracz nie może zostać trwale uwięziony przez losową pogodę.

## Persistence i world state

Save musi zachowywać co najmniej:
- odkryte POI i trasy;
- stan głównych blokad handlowych;
- istotne decyzje questowe;
- odblokowane przeprawy;
- pozyskane dowody/mapy;
- regionalny late-game state.

Właścicielem technicznego formatu save pozostaje `SavePersistence.md` i runtime systems.

## Late-game return

Powrót do R3 ma pokazać konsekwencje wcześniejszych decyzji przez zmiany w:
- dostępności towarów;
- ruchu portowym;
- kontroli szlaków;
- obecności frakcji/NPC;
- bezpieczeństwie wybranych tras;
- reakcji mieszkańców na kryzys.

Nie wymaga to tworzenia drugiej mapy regionu; preferowane są warstwy stanu na tych samych przestrzeniach.

## Asset families

Minimalne rodziny assetów:
- architektura portowa i magazynowa;
- małe łodzie i elementy nabrzeży;
- sieci, beczki, skrzynie, liny i sprzęt rybacki;
- warianty zabudowy zgodne z zatwierdzonym research material culture;
- słone mokradła i roślinność wybrzeża;
- skały/klify;
- znaki handlowe, punkty kontroli i rekwizyty transportowe;
- stare miejsca zgodne z ich funkcją w lore.

Finalna asset list powinna później nadać każdemu elementowi priorytet P0/P1/P2 i ownera.

## Audio identity

R3 powinno odróżniać się przez:
- wiatr i otwartą przestrzeń;
- wodę rzeczną i przybrzeżną;
- pracę portu;
- ptaki i odgłosy mokradeł;
- zmianę gęstości ambience między hubem, wyspami i klifami.

Muzyka nie powinna używać nowego „etnicznego” języka tylko dlatego, że pojawiają się przyjezdni; instrumentarium wymaga osobnej decyzji audio/research.

## Streaming i performance

Region powinien być podzielony na logiczne komórki: Solny Bród, wybrzeże/klify, mokradła, porty pomocnicze i wyspy. Widoki przez wodę wymagają szczególnej kontroli LOD, cullingu i odbić.

Konkretne budżety CPU/GPU/RAM nie są zamrażane bez pomiarów. Po prototypie należy zapisać zmierzone targety w owner spec performance.

## QA — kryteria regionu

R3 jest gotowy do production lock dopiero gdy:
1. kampania może przejść przez region bez softlocka niezależnie od opcjonalnych encounterów;
2. stare mapy kupieckie dają jednoznaczny, trwały evidence state;
3. save/load zachowuje trasy, przeprawy, evidence i główne world-state changes;
4. pogoda nie może permanentnie zablokować wymaganej podróży;
5. late-game return odtwarza właściwy wariant regionu;
6. wielokulturowe NPC używają zatwierdzonych naming/culture rules;
7. wszystkie konkretne encountery korzystają ze wspólnego content formatu;
8. zmierzone sceny portu i widoków wodnych mieszczą się w później zatwierdzonych targetach performance.

## Otwarte decyzje

Przed finalnym production lock wymagają rozstrzygnięcia:
- finalna liczba mniejszych portów i grywalnych wysp;
- skala i gęstość Solnego Brodu;
- czy gracz kiedykolwiek dostaje swobodne sterowanie łodzią;
- finalny roster i pochodzenie wielokulturowych NPC;
- konkretna zawartość starych map kupieckich;
- lista finalnych istot/encounterów wybrzeża po odpowiednim researchu;
- tuning ekonomii, niedoborów i blokad;
- zakres VO i unikalnej muzyki;
- finalna P0/P1/P2 asset list;
- mierzone budżety streamingu, CPU, GPU i RAM.

Żadna z tych decyzji nie jest wymagana do utrzymania funkcji fabularnej R3; muszą jednak zostać zamknięte przed finalnym content/production lock.