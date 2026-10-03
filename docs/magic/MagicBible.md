# Magic Bible — v0.2

Etap 8 roadmapy. Cały system mechaniczny magii jest **fikcją SlavicGame (F)**. Inspiracje historyczne dotyczą obrzędowości, symboliki i materiałów, ale magiczny alfabet, reguły energii i czary nie są prezentowane jako autentyczny system dawnych Słowian.

## Cele

Magia ma być:

- użyteczna, ale nie darmowa;
- możliwa do rozpoznania i przygotowania;
- związana z wiedzą i konsekwencjami;
- alternatywą lub wsparciem dla walki, nie jej zamiennikiem w każdej sytuacji;
- zrozumiała mechanicznie przed przyjęciem poważnego kosztu.

## Źródła mocy F

### Boskie

Moc wynika z relacji, błogosławieństwa lub umowy.

**Koszty:** zobowiązanie, ograniczenie użycia, przysługa, łaska.  
**Ograniczenie:** brak patrona nie blokuje ukończenia głównej historii.

### Przyroda

Moc wykorzystująca lokalne procesy, rośliny i środowisko.

**Koszty:** składniki, czas, zużycie zasobu, możliwy lokalny skutek ekologiczny.

### Dusze i Nawia

Kontakt, odczyt echa, ochrona przed wpływem zmarłych.

**Koszty:** przedmiot związany z osobą, przygotowanie, ryzyko błędnej identyfikacji.

### Dawna moc

Pozostałości dawnych technik i miejsc.

**Koszty:** ograniczona wiedza, zużycie narzędzia, nieprzewidywalność przy błędnym użyciu.

### Czwarta Sfera

Nie jest normalną szkołą magii. W pierwszych etapach gry występuje głównie jako **anomalia**. Gracz nie otrzymuje swobodnego drzewka „Fourth Sphere magic”.

## Trzy formy działania

### Czar

Szybka, wyuczona akcja.

Wymaga:
- poznanego wzorca;
- zasobu;
- możliwego celu;
- czasu użycia / cooldownu.

### Rytuał

Działanie zależne od miejsca, kolejności, czasu i przygotowania.

Wymaga:
- właściwego miejsca;
- warunku czasowego albo sytuacyjnego;
- składników;
- znajomości przebiegu;
- czasem dowodu lub przedmiotu związanego z celem.

Błąd rytuału powinien dać czytelną reakcję i możliwość poprawy. Nie tworzymy pułapek, w których jedno kliknięcie bez informacji niszczy wielogodzinną kampanię.

### Alchemia

System receptur i substancji.

Wymaga:
- poznanej receptury;
- składników;
- narzędzia lub stanowiska, jeżeli receptura tego wymaga.

## Zasoby gracza

Pierwszy model zakłada użycie:

- staminy;
- zdrowia;
- przedmiotów;
- czasu;
- łaski;
- zobowiązań;
- stanu środowiska.

Nie tworzymy jednej uniwersalnej „many”, dopóki nie okaże się potrzebna w testach.

## Pierwszy czar F

Robocza funkcja: **krótkie ujawnienie aktywnego śladu**.

Nie identyfikuje automatycznie potwora. Pokazuje, że ślad posiada inną naturę niż zwykły element otoczenia.

Cel:
- pomóc w tropieniu;
- nauczyć gracza różnicy między obserwacją a interpretacją;
- nie zastępować dziennika i dowodów.

## Pierwszy rytuał F

Dotyczy kontraktu „Światło nad mokradłem”.

Warunki:
- rozpoznanie zmarłego;
- kotwica/przedmiot;
- składniki;
- odpowiednia pora;
- właściwe miejsce.

Skutek:
- może domknąć lokalną więź zjawy;
- nie naprawia automatycznie wszystkich anomalii;
- nie zabija drapieżnika;
- nie wyjaśnia źródła globalnego kryzysu.

## Alchemia pierwszego wycinka F

Jedna receptura przygotowawcza powinna:

- użyć 2–3 składników;
- dawać praktyczną korzyść na mokradłach;
- nie być obowiązkowa do ukończenia zadania;
- pokazać różnicę między surowcem a przygotowanym środkiem.

## Magiczny alfabet F

Powstanie jako oryginalny system graficzny.

### Zasady

- nie nazywamy go „autentycznymi słowiańskimi runami”;
- znaki mają logikę kształtu i składania;
- gracz uczy się znaczeń stopniowo;
- kombinacja znaków może tworzyć instrukcję, a nie tylko dekoracyjny napis;
- błędna kolejność może zmienić wynik rytuału w przewidywalny sposób.

### Warstwy nauki

1. pojedynczy znak;
2. para znaków;
3. słowo / funkcja;
4. fraza rytualna;
5. złożony schemat.

## Balans

Każda moc powinna odpowiadać na pytania:

- jaki problem rozwiązuje?
- jaki koszt ponosi gracz?
- czego nie potrafi?
- jak przeciwnik lub świat może na nią odpowiedzieć?
- czy istnieje niemagiczna alternatywa?

## Język inkantacji F

Pierwsza grywalna warstwa używa sześciu całkowicie fikcyjnych słów. Nie są one rekonstrukcją języka ani rytuałów historycznych.

| Słowo | Funkcja mechaniczna |
|---|---|
| `ZAR` | siła / ciepło / zapłon |
| `VEK` | kierunek / uwolnienie |
| `ZIVA` | życie / żywy wzorzec |
| `DAR` | wymiana / odnowienie |
| `VEDA` | rozpoznanie / ujawnienie |
| `NAW` | echo / ślad zza granicy |

Pierwsze frazy to `ZAR VEK`, `ZIVA DAR` i `VEDA NAW`. W kodzie fraza składa się z osobnych słów, ma czas wypowiedzenia i identyfikatory `voice cue`, dzięki czemu dubbing lub przyszłe rozpoznawanie mowy można dołożyć bez zmiany reguł czaru. Podczas castu HUD ujawnia słowa kolejno, a przerwanie obrażeniem zatrzymuje inkantację.

## Status

Działa wykonywanie trzech czarów, koszty, cooldown, przerwanie obrażeniem, podstawowe VFX, ujawnianie magicznych śladów oraz warstwa inkantacji słowo-po-słowie. Model rytuałów i alchemii istnieje, ale ich pełne wykonywanie, crafting, nauka znaków w świecie i finalny audio/VFX pozostają do implementacji.


## Author truth: pochodzenie znaków i „dawnej mocy”

Po ustaleniu centralnego lore wiadomo, że najstarsza warstwa magicznego alfabetu jest uproszczonym potomkiem **notacji Sieci Progów**.

Pierwotnie znaki opisywały relacje takie jak:
- identity;
- boundary;
- direction;
- condition;
- exchange;
- closure;
- repetition.

To tłumaczy, dlaczego rytuały są skuteczniejsze, gdy:
- poprawnie rozpoznają osobę/cel;
- definiują granicę;
- podają warunek;
- nie próbują wymusić sprzecznego stanu.

Współczesne kultury świata interpretują te znaki religijnie, magicznie albo praktycznie, ale **nie muszą znać ich pierwotnej funkcji techniczno-rytualnej**.

Czwarta Sfera/Splot nie jest normalnym zasobem many ani „szkołą czarów”. Praca bezpośrednio na Splotcie jest późnogrowa, ryzykowna i może naruszać ciągłość tożsamości lub stabilność węzła.

Szczegóły: [FourthSphereTruth.md](../world/FourthSphereTruth.md).
