# Lokacja: Czarne Mokradła — karta v0.1

**ID:** `black-swamp`  
**Typ:** swamp  
**Status:** F

## Funkcja

Główna lokacja śledztwa i encounteru vertical slice.

## Filary

- ograniczona widoczność;
- physical + supernatural traces;
- uszkodzona przeprawa;
- predator;
- apparition;
- nocna nieszczelność.

## Strefy

### Edge
Bezpieczniejsze wejście i pierwsze ślady.

### Crossing
Uszkodzona konstrukcja, gospodarczy powód istnienia lokacji.

### Death site
Pamiątka i dowody fizyczne.

### Predator territory
Obszar, w którym AI ma przewagę przestrzenną, ale istnieje droga odwrotu.

### Leak zone
Aktywne nocą miejsce zjawiska.

## Dzień

- lepsza widoczność;
- oględziny;
- predator tracks;
- brak pełnej nocnej materializacji zjawy; jej ślady można badać, ale sama forma nie utrzymuje się za dnia.

## Noc

- active `black-swamp-leak`;
- światło;
- apparition;
- wyższe napięcie;
- predator może nadal działać niezależnie.

## Bezpieczeństwo

Noc ma być ryzykowna, ale:
- oznaki pojawiają się przed wejściem;
- gracz może się wycofać;
- nie ma losowego one-shotu.

## Pogoda

Fog może wzmacniać klimat, ale quest-critical clue musi pozostać dostępny.

## Po queście

### ritual
Leak lokalnie stabilizuje się.

### pact
Leak działa w kontrolowanym/warunkowym stanie.

### destroyed anchor
Światło znika, ale stan metafizyczny nie musi oznaczać pełnego „uzdrowienia”.

## Otwarte

- finalny water rendering;
- poziom grzęźnięcia/ruchu;
- finalna identyfikacja predatora;
- research konstrukcji przeprawy.


## Runtime apparition pass

Nocą, po aktywacji questa i w pobliżu leak zone, może materializować się `ENTITY_MISSING_ECHO_F`.

Zasady:
- dzień: forma nie materializuje się;
- noc + aktywny quest + bliskość: stopniowa materializacja;
- pamiątka zaginionego wzmacnia reakcję i przyciąga echo bliżej gracza;
- samo zobaczenie zjawy nie przyznaje automatycznie evidence;
- podczas `ritual.release-bound-echo` forma pojawia się przy miejscu rytuału;
- po `swamp.apparition-released` stopniowo się rozprasza i nie wraca.

Zjawa nie ma HP i nie jest wariantem fizycznego predatora. To celowo oddziela supernatural investigation od combat encounteru.
