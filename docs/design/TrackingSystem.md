# Tracking System — v0.1

## Cel

Tropienie pozwala graczowi budować hipotezę zamiast śledzić świecącą linię.

## Typy śladów

- footprint;
- broken vegetation;
- blood/body trace;
- object trace;
- sound clue;
- environmental anomaly;
- supernatural trace;
- witness account — trafia do Journal, nie jest fizycznym tropem.

## Dane śladu

Każdy ślad ma:
- ID;
- category;
- position;
- freshness;
- source entity/type jeśli autorzy znają;
- visible conditions;
- evidence output;
- optional magic signature.

## Freshness

Prosty model:
- Fresh;
- Recent;
- Old;
- Faded.

Nie musi być symulowany co sekundę w vertical slice.

## Observation

Interakcja ze śladem może dać:
- opis;
- dowód;
- hipotezę;
- wpis bestiary.

## Reveal trace spell

Wzmacnia magic signature.

Nie:
- tworzy quest GPS;
- reveal physical tracks through walls.

## Vertical slice — trzy główne ślady

1. predator tracks;
2. broken crossing / physical damage;
3. apparition trace przy keepsake/nieszczelności.

Gracz musi móc stwierdzić, że istnieją co najmniej dwa źródła problemu.

## UX

Ślad powinien mieć:
- czytelność wizualną bez czaru na bliskim dystansie;
- subtelny feedback interakcji;
- wpis do Journal.

## Failure

Nieusunięcie jednego opcjonalnego śladu nie blokuje questa, jeśli istnieje alternatywna droga dowodowa.


## Runtime transient footprints

R0 ma teraz osobną warstwę nietrwałych śladów gracza.

### Gdzie powstają
Ślad może zostać odciśnięty tylko wtedy, gdy lokalne podłoże ma odpowiednią podatność:
- błoto;
- mokradło;
- mokra ścieżka;
- wilgotny brzeg rzeki.

Kamień i twarde podłoże praktycznie blokują odciski. Pod wodą footprinty nie są stemplowane na dnie.

### Wilgoć i deszcz
`FootprintTrailState.Trackability` łączy typ podłoża z:
- mokrością gracza;
- intensywnością deszczu;
- karą za rock/grass.

Deszcz zwiększa podatność miękkiego podłoża, ale jednocześnie dużo szybciej postarza już istniejące footprinty. Ulewa może więc chwilowo produkować czytelniejsze świeże ślady, ale stary trop szybko znika.

### Lifetime i performance
- maksymalnie 96 aktywnych footprintów gracza;
- krok śladu około 0.72 m;
- lewa/prawa stopa naprzemiennie;
- żywotność zależy od błota i trackability;
- geometria: 4 wierzchołki / 2 trójkąty na footprint;
- brak tekstury, particle systemu i osobnego render passu.

### Tracking / evidence separation
Te footprinty są tylko transient world feedback:
- nie trafiają do `TrackingState`;
- nie tworzą evidence;
- nie zapisują się w save;
- nie są magic signature;
- `Reveal Trace` ich nie podświetla.

Durable tracking nadal służy śladom questowym, NPC/creature clues i supernatural traces.
