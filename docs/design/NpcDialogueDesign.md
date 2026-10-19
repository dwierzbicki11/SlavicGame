# NPC i dialog — v0.1

## NPC

Minimalny NPC posiada:
- ID;
- rolę;
- pozycję / miejsce;
- harmonogram;
- stan życia;
- relacje;
- dialogue graph;
- reakcje na questy;
- pamięć kluczowych flag.

## Harmonogram

Harmonogram opisuje **aktywność i miejsce**, nie pełną animację każdej sekundy.

Przedziały obejmują godzinę początku i wykluczają godzinę końca; `20–6`
przechodzi przez północ. Zakres o rozpiętości co najmniej 24 godzin (np. `0–24`
lub `6–30`) oznacza aktywność całodobową. Równe końce (`6–6`) oznaczają pusty
przedział. Godziny są normalizowane do doby; wartości niefinitywne nie aktywują NPC.

Pierwszy wycinek używa dwóch głównych zakresów:
- dzień;
- noc.

Później można dodać:
- świt;
- zmierzch;
- pracę;
- odpoczynek;
- wydarzenia questowe.

## Priorytety zachowania

Przykładowa kolejność:
1. stan krytyczny questa;
2. zagrożenie;
3. scripted event;
4. harmonogram;
5. idle.

## Dialog

Graf dialogowy posiada:
- node;
- speaker;
- tekst;
- choices;
- requirements;
- effects.

## Requirements

Mogą sprawdzać:
- quest phase;
- evidence;
- item;
- reputation;
- divine relationship;
- world flag;
- porę;
- stan NPC.

## Effects

Mogą:
- przesunąć quest;
- dodać dowód;
- przekazać/zabrać przedmiot;
- zmienić reputację;
- zmienić relację;
- ustawić flagę.

Dialog nie powinien bezpośrednio modyfikować renderera.

## Pamięć

NPC pamięta tylko rzeczy istotne dla jego zachowania. Nie zapisujemy każdej wypowiedzianej linii.

## Pięć ról wycinka

- rodzina zaginionego;
- opiekun przeprawy;
- zielarka;
- strażnik wspólnoty;
- opiekun miejsca kultowego.

Każda rola musi mieć:
- interes;
- wersję wydarzeń;
- co najmniej jedną reakcję po rozwiązaniu questa.

## Voice-over

Pełny VO pozostaje decyzją produkcyjną. System dialogu nie może wymagać nagrania każdej linii.

## Skip / history

Docelowo:
- pomijanie tekstu;
- historia dialogu;
- wyraźne oznaczenie wyborów;
- brak timed choices jako domyślnego rozwiązania.
