# NPC pierwszego wycinka — karty v0.1

Nazwy osobowe pozostają otwarte. Poniższe ID są trwałymi rolami technicznymi.

## 1. missing-family

**Rola:** osoba z rodziny zaginionego  
**Cel:** dowiedzieć się, co naprawdę się stało  
**Lęk:** że łowca zamknie sprawę bez dowodu  
**Co oferuje:** zlecenie, opis osoby, możliwy przedmiot identyfikacyjny  
**Co ukrywa:** drobny konflikt rodzinny bez znaczenia dla śmierci, by nie każda tajemnica była głównym twistem

### Wiedza początkowa

Wie:
- kiedy zaginiony wyszedł;
- dokąd miał iść;
- jaki przedmiot należał do niego.

Nie wie:
- czy światło to zmarły;
- czy drapieżnik spowodował śmierć;
- czym jest nieszczelność.

### Reakcje

**Rytuał:** wdzięczność za potwierdzenie losu.  
**Umowa:** ambiwalencja; ulga i niepokój.  
**Zniszczenie kotwicy:** negatywna reakcja, szczególnie jeśli gracz nie zebrał dowodów.

## 2. crossing-keeper

**Rola:** opiekun przeprawy  
**Cel:** przywrócić bezpieczny ruch  
**Lęk:** utrata dochodu i izolacja regionu  
**Co oferuje:** informacje o drodze i fizycznych śladach  
**Bias:** interpretuje problem przede wszystkim praktycznie

### Wiedza

Wie:
- gdzie uszkodzona jest przeprawa;
- kiedy widziano światło;
- że zwierzęta zachowują się inaczej.

Może błędnie zakładać:
- że jedno zagrożenie odpowiada za wszystko.

### Reakcje

**Rytuał:** pozytywna, jeśli ruch wraca do normy.  
**Umowa:** ostrożnie negatywna, jeśli nocne ograniczenia pozostają.  
**Zniszczenie kotwicy:** pozytywna krótkoterminowo, jeśli światło znika.

## 3. herbalist

**Rola:** zielarka  
**Cel:** ochronić ludzi bez bezmyślnego niszczenia mokradeł  
**Lęk:** że strach doprowadzi do wypalenia/zniszczenia obszaru  
**Co oferuje:** składniki, recepturę, obserwacje biologiczne

### Wiedza

Potrafi rozróżnić:
- fizyczne obrażenia;
- nietypowe zachowanie zwierząt;
- część śladów związanych z drapieżnikiem.

Nie zna pełnej metafizyki zjawy.

### Reakcje

**Rytuał:** najbardziej pozytywna.  
**Umowa:** neutralna/ostrożnie pozytywna, jeśli warunki są kontrolowane.  
**Zniszczenie kotwicy:** zależna od szkód w środowisku i sposobu działania gracza.

## 4. community-guard

**Rola:** strażnik wspólnoty  
**Cel:** ograniczyć zagrożenie dla mieszkańców  
**Lęk:** kolejna osoba zaginie  
**Co oferuje:** informacje o bezpieczeństwie, nocnych patrolach i świadkach  
**Bias:** preferuje rozwiązanie łatwe do egzekwowania

### Wiedza

Wie:
- kto zgłaszał zdarzenia;
- jakie drogi zamknięto;
- czy ktoś widział fizycznego napastnika.

Nie musi wierzyć w wyjaśnienie nadnaturalne.

### Reakcje

**Rytuał:** akceptuje wynik, jeśli zagrożenie znika.  
**Umowa:** wymaga jasnych reguł nocnego ruchu.  
**Zniszczenie kotwicy:** praktycznie pozytywna, jeśli problem nie wraca.

## 5. shrine-keeper

**Rola:** opiekun miejsca kultowego  
**Cel:** zachować reguły Kamiennego Kręgu i ograniczyć niekontrolowane przejścia  
**Lęk:** że ktoś wykorzysta zjawisko bez zrozumienia  
**Co oferuje:** wiedzę o rytuale i opcjonalnym kontakcie boskim  
**Bias:** interpretuje wydarzenia przez swoją tradycję, która nie jest automatycznie pełną prawdą autorów

### Wiedza

Wie:
- jak wykonać bezpieczniejszy rytuał;
- kiedy miejsce jest aktywne;
- jakie znaki są używane w lokalnej praktyce F.

Nie wie automatycznie:
- czym jest Czwarta Sfera;
- kto wywołał globalny kryzys.

### Reakcje

**Rytuał:** pozytywna.  
**Umowa:** zależy od warunków umowy.  
**Zniszczenie kotwicy:** krytyczna, jeśli gracz zrobił to bez rozpoznania.

## Harmonogram v0.1

| NPC | Dzień | Noc |
|---|---|---|
| missing-family | dom / okolice wsi | dom |
| crossing-keeper | przeprawa | wieś |
| herbalist | warsztat / zbieranie blisko wsi | dom |
| community-guard | patrol dzienny | nocna warta |
| shrine-keeper | Kamienny Krąg | wieś / wyjątkowo krąg podczas eventu |

## Minimalne grafy dialogowe

Każdy NPC potrzebuje:
- rozmowy przed questem;
- rozmowy w Investigation;
- jednej reakcji po znalezieniu ważnego dowodu;
- reakcji po każdym z trzech rozwiązań;
- fallbacku po zakończeniu questa.

## Relacje

Vertical slice testuje przede wszystkim:
- Trust dla herbalist;
- Trust dla missing-family;
- reputację old-village.

Nie tworzymy jeszcze pełnego systemu romansów/towarzyszy w tej piątce.

## Asset needs

Każdy NPC potrzebuje docelowo:
- modelu/sylwetki;
- wariantu stroju;
- 3–5 podstawowych animacji;
- portretu tylko jeśli UI go wymaga;
- ikony interakcji;
- głosu opcjonalnie.

## Otwarte

- imiona;
- wiek;
- wygląd;
- dokładne pochodzenie;
- VO;
- finalne dialogi literackie.
