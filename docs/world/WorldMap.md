# Mapa świata — założenia v0.2

Etap 16 roadmapy.

## Filozofia

Mapa ma być **gęsta**, a nie maksymalnie duża. Każdy region musi uzasadniać podróż, powrót i zmianę między dniem a nocą.

## Hierarchia

```text
Świat
└── obszar geograficzny
    └── region gry
        ├── osady
        ├── dzicz
        ├── miejsca specjalne
        ├── drogi / przeprawy
        └── wejścia do przestrzeni warunkowych
```

## Region startowy

Pogranicze Żarnowca jest testem docelowej filozofii mapy.

Topologia:
- Żarnowiec jako bezpieczniejszy hub;
- droga do Puszczy Żywia;
- ryzykowniejsza droga przez Czarne Mokradła;
- boczna trasa do Kamiennego Kręgu;
- skróty odblokowywane przez stan świata.

## Zasady projektowania drogi

Droga powinna mieć:
- punkt orientacyjny;
- decyzję nawigacyjną;
- możliwe zdarzenie;
- zmienność dnia/nocy;
- sens ekonomiczny lub społeczny.

Nie zapełniamy mapy losowymi skrzyniami co kilkadziesiąt metrów.

## Fast travel

Docelowo może istnieć między:
- odkrytymi bezpiecznymi punktami;
- osadami;
- specjalnymi węzłami.

Nie może omijać ważnego stanu zagrożenia bez wyjaśnienia.

## Sfery

Nawia i sfery boskie nie są zwykłymi regionami na tej samej mapie.

Dostęp wymaga:
- miejsca;
- warunku;
- ceny / przygotowania.

## Przyszłe regiony

Kolejność rozbudowy powinna wynikać z potrzeb historii i jakości vertical slice, nie z chęci szybkiego wypełnienia mapy.

## Narzędzia mapy

Mapa gracza może pokazywać:
- odkryte drogi;
- osady;
- miejsca zleceń;
- własne znaczniki;
- potwierdzone miejsca śladów.

Nie pokazuje automatycznie:
- wszystkich potworów;
- wszystkich sekretów;
- dokładnego rozwiązania zagadki.

## Stan geografii po makro-passie

**Makrogeografia kampanii jest ustalona jako first pass R0–R6.**

Nadal otwarte produkcyjnie są:
- dokładne granice map gry;
- rozmiary i streaming;
- lokalne drogi i osady poza R0;
- kolejność części opcjonalnych;
- cięcia/łączenia regionów wynikające z budżetu.

Zmiana rozmiaru regionu nie powinna usuwać jego funkcji fabularnej.


## Geografia makro v0.1

Szczegółowy układ znajduje się w [MacroGeography.md](MacroGeography.md).

Kolejność regionów kampanii:
1. Pogranicze Żarnowca;
2. Nadborze / Wielki Bór;
3. Przymorze / Kamienne Wyżyny;
4. Równiny Arel;
5. Pustkowie Pierwszego Progu;
6. przestrzenie Nawii i finałowy próg.

To **regiony produkcyjne**, nie obietnica jednej seamless mapy.
