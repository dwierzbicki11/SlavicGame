# Receptury alchemiczne — v0.1

Cały system gameplay alchemii jest F. Nazwy składników mogą później otrzymać dokładniejsze historyczne/botaniczne odpowiedniki po researchu.

## Format receptury

- ID;
- result;
- result quantity;
- ingredients;
- station;
- knowledge requirement;
- craft time;
- effect;
- duration;
- side effects;
- sell value później.

## recipe.marsh-sight-tonic

Result:
- 1 × `marsh-sight-tonic`.

Ingredients:
- 1 × `marsh-herb`;
- 1 × `forest-resin`.

Station:
- prosty stół zielarki albo crafting prompt.

Effect:
- poprawia czytelność śladów aktywnych na mokradłach;
- nie wskazuje celu;
- nie identyfikuje istoty.

Duration:
- krótka/średnia, do playtestu.

## recipe.simple-bandage

Jeśli crafting bandaża zostanie przyjęty:

Ingredients:
- cloth material;
- optional herbal additive później.

Effect:
- leczy albo zatrzymuje bleeding.

Vertical slice może dawać gotowe bandaże zamiast craftu.

## recipe.basic-antidote

Niepotrzebne w pierwszym przejściu, chyba że predator użyje poison.

Status:
- placeholder design;
- bez finalnych składników przed researchu.

## recipe.ritual-preparation

Nie jest „miksturą many”.

Możliwy preparat:
- zabezpiecza gracza przed krótkim spirit-exposure;
- potrzebny dopiero, jeśli playtest pokaże wartość.

## Zasada rozwoju

Najpierw każda receptura odpowiada konkretnemu problemowi gameplay.

Nie dodajemy 50 receptur tylko dla liczby.


## Runtime alchemy pass

Pierwsza receptura jest teraz grywalna przy stanowisku zielarki.

### Sterowanie
Przy dostępnej zielarce HUD pokazuje `K ALCHEMIA`.
- `K` — otwórz/zamknij panel;
- `W/S` — wybór receptury;
- `E` — wykonaj;
- `Esc` — zamknij.

Podczas craftingu ruch gracza jest zablokowany.

### recipe.marsh-sight-tonic

Knowledge gate:
- `recipe.marsh-sight-tonic.learned`.

Składniki:
- 1 × `marsh-herb`;
- 1 × `forest-resin`.

Wynik:
- 1 × `marsh-sight-tonic`.

Panel pokazuje owned/required dla każdego składnika.

### Atomicity
Craft:
1. sprawdza dostępność stanowiska;
2. sprawdza knowledge gate;
3. waliduje wszystkie składniki;
4. dopiero potem konsumuje cały koszt;
5. tworzy wynik.

Brak składnika nie konsumuje części kosztu. Recepturę można wykonywać wielokrotnie tylko z nowym kompletem składników.

### Availability
Pierwsze stanowisko jest związane z `herbalist` i aktywnością `trade-and-prepare`. Nocą/rest crafting nie jest dostępny.

### Persistence
Nie ma osobnego crafting snapshotu. Knowledge gate jest world flagą, a składniki/wynik korzystają z istniejącego inventory persistence.

### Economy safety
`marsh-sight-tonic` nie jest przyjmowany w sell path pierwszego vendora zielarki. Wartości ekonomiczne pozostają tuningiem, a test regresyjny pilnuje braku bezkosztowego craft-sell loop.
