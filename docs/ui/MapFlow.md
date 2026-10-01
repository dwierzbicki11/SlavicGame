# Map UI — v0.1

## Cel

Mapa pomaga w nawigacji po już poznanym świecie, ale nie rozwiązuje eksploracji za gracza.

## Warstwy

- terrain/shape;
- roads;
- settlements;
- discovered locations;
- quest-related known places;
- player marker;
- custom markers później.

## Discovery

Miejsce pojawia się po:
- odwiedzeniu;
- otrzymaniu wiarygodnej informacji;
- zdobyciu mapy/informacji, jeśli taki system zostanie dodany.

## Markers

Typy:
- settlement;
- service;
- shrine;
- danger;
- quest;
- custom.

Nie pokazujemy:
- dokładnej pozycji nieodkrytego dowodu;
- wszystkich potworów;
- wszystkich collectibles.

## Quest marker

Marker wskazuje:
- obszar znany z informacji;
- niekoniecznie dokładny punkt.

## Time/weather

Mapa nie musi pokazywać prognozy. Może pokazać aktualną porę.

## Fast travel

Jeśli aktywne:
- tylko między odkrytymi punktami;
- blokowane przez aktywny stan zagrożenia, jeśli logiczne;
- jasna informacja o upływie czasu.

## Vertical slice

Minimum:
- 4 nazwane miejsca;
- pozycja gracza;
- drogi;
- jeden quest area marker;
- stan przeprawy po rozwiązaniu.

## Otwarte

- minimapa;
- compass;
- custom marker;
- fog of war;
- dokładny styl kartograficzny.
