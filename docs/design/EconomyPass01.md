# Ekonomia i vendorzy — first pass v0.1

## Cel

Pieniądz ma znaczenie dla zawodu łowcy, ale ekonomia vertical slice pozostaje prosta.

## Źródła pieniędzy

- kontrakty;
- drobne zlecenia;
- sprzedaż wybranych materiałów;
- barter/usługi.

## Wydatki

- consumables;
- składniki;
- amunicja;
- naprawy później;
- narzędzia;
- informacje/usługi;
- nocleg/odpoczynek opcjonalnie.

## Vendor roles

### Herbalist
Sprzedaje:
- składniki;
- consumables;
- receptury po spełnieniu warunków.

### Workshop / smith
Później:
- broń;
- naprawa;
- ulepszenia.

### Trader
Później:
- towary regionalne;
- informacje;
- barter.

## Price modifiers

Możliwe:
- reputation;
- quest state;
- route availability;
- scarcity.

Vertical slice:
- tylko prosta cena + ewentualna mała reakcja reputacji.

## Buyback

Niepotrzebny w vertical slice.

## Quest items

Nie można sprzedać, jeśli stworzyłoby softlock.

## Money scale

Nie ustalamy finalnych nominałów przed researchu gospodarczego świata i pierwszym balansem.

## Anti-exploit

- nagroda quest tylko raz;
- brak nieskończonego craft-sell loop bez kosztu;
- ceny muszą być testowane automatycznie, jeśli pojawią się recipes z wartością sprzedaży.


## Runtime R0 vendor pass

Pierwsza grywalna implementacja vendora obejmuje dwóch istniejących NPC:
- `vendor.r0.trader` → `settler-trader-01`;
- `vendor.r0.herbalist` → `herbalist`.

### Sterowanie
Przy dostępnym vendorze HUD pokazuje `T HANDEL`.
- `T` — otwórz/zamknij handel;
- `W/S` — wybór pozycji;
- `A/D` — przełącz `KUP` / `SPRZEDAJ`;
- `E` — zatwierdź transakcję;
- `Esc` — zamknij.

Podczas handlu ruch gracza jest zablokowany.

### Stock prototypowy

Trader:
- `simple-bandage`: 6;
- `arrow-basic`: 24;
- `forest-resin`: 4.

Herbalist:
- `marsh-herb`: 8;
- `simple-bandage`: 6;
- `marsh-sight-tonic`: 2, widoczny dopiero po `recipe.marsh-sight-tonic.learned`.

Liczby i ceny są **gameplay F / playtest tuning**. Nie są twierdzeniem o historycznych cenach, walucie ani sile nabywczej.

### Transaction contract
Zakup jest atomowy:
1. walidacja unlocku;
2. walidacja stocku;
3. walidacja pieniędzy;
4. pobranie pieniędzy;
5. dodanie itemu;
6. decrement stocku.

Sprzedaż jest atomowa w odwrotną stronę.

Quest-protected items, w tym `missing-person-keepsake` i `ritual-thread`, nie wchodzą do sell path.

### Reputation pricing
`old-village` reputation daje mały, ograniczony modyfikator:
- dodatnia reputacja lekko obniża buy price;
- dodatnia reputacja lekko podnosi sell price;
- ujemna reputacja działa odwrotnie.

To nie zastępuje przyszłego balance passu.

### Availability
Vendor działa tylko, gdy:
- jego NPC istnieje w runtime;
- wykonuje właściwą aktywność vendora;
- gracz stoi w zasięgu.

W nocy/rest vendor nie jest dostępny.

### Persistence
Stock vendora jest zapisany jako opcjonalna sekcja bieżącego save v3. Starszy save v3 bez tej sekcji dostaje bazowy stock. Otwarte UI handlu nie jest persistowane.
