# Journal UI flow — v0.1

## Cel

Dziennik jest narzędziem śledztwa, nie tylko listą celów.

## Zakładki

1. Quests
2. Evidence
3. Bestiary
4. People
5. Places
6. Knowledge / Magic później

## Quest view

Pokazuje:
- tytuł;
- aktualną fazę;
- aktywne cele;
- wykonane cele;
- powiązane dowody;
- znane strony konfliktu.

Nie pokazuje przyszłych kroków.

## Evidence view

Każdy wpis ma wyraźny typ:
- Rumor;
- Observation;
- ConfirmedFact;
- Interpretation.

Wizualne oznaczenie typu nie może opierać się tylko na kolorze.

## Source

Dowód pokazuje źródło, jeśli gracz je zna:
- NPC;
- miejsce;
- przedmiot;
- własna obserwacja.

## Interpretations

Hipotezy mogą być:
- aktywne;
- podważone;
- wzmocnione;
- zastąpione mocniejszym potwierdzeniem.

Nie kasujemy historii dochodzenia bez powodu.

## Bestiary

Wpis rozwija się etapami:
- nazwa robocza/nieznana;
- obserwacje;
- reakcje;
- przygotowania;
- potwierdzona identyfikacja.

## People

Nie pokazujemy pełnych ukrytych statystyk relacji. Pokazujemy znane informacje i ważne fakty.

## Places

Miejsce może zawierać:
- opis;
- znane usługi;
- zagrożenia;
- wydarzenia;
- powiązane questy.

## Search/filter

Niepotrzebne w vertical slice, ale architektura UI nie powinna ich wykluczać.

## Vertical slice

P0:
- light-over-swamp;
- lista 6–8 dowodów;
- rozróżnienie Rumor/Observation/ConfirmedFact/Interpretation;
- aktualizacja po rozwiązaniu;
- brak spoilerów ukrytego stanu autorów.
