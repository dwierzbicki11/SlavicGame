# Konwencje identyfikatorów treści — v0.1

Ten dokument definiuje nazewnictwo trwałych identyfikatorów używanych przez kod, save, questy i dokumentację.

## Cel

Identyfikator ma być:

- stabilny;
- niezależny od języka interfejsu;
- czytelny w logach;
- odporny na zmianę nazwy wyświetlanej;
- jednoznaczny w save;
- możliwy do użycia jako klucz w danych.

## Zasady ogólne

1. Używamy małych liter ASCII.
2. Słowa rozdzielamy myślnikiem.
3. Nie używamy spacji, polskich znaków ani numerów bez znaczenia.
4. ID nie zmienia się po zmianie nazwy wyświetlanej.
5. ID nie koduje przypadkowych danych prezentacyjnych.
6. Raz opublikowane ID zapisane w save traktujemy jako trwałe.

Przykład:

```text
light-over-swamp
old-village
black-swamp
swamp-predator
```

## Namespace przez katalog/system

Nie dodajemy prefiksów typu `quest-` wszędzie, jeśli typ danych już wynika z miejsca użycia.

Poprawne:

```text
quest: light-over-swamp
item: missing-person-keepsake
npc: herbalist
region: black-swamp
```

## Questy

Format:

```text
<nazwa-kontraktu>
```

Przykłady:

- `light-over-swamp`
- `broken-crossing`
- `echo-in-the-grove`

Etapy questa nie dostają osobnych globalnych ID, jeśli są lokalne dla jednego questa. Jeżeli potrzebujemy trwałego znacznika, używamy:

```text
light-over-swamp.inspect-crossing
light-over-swamp.identify-apparition
```

## Dowody

Format:

```text
<quest>.<krótki-opis>
```

Przykłady:

- `light-over-swamp.witness-light`
- `light-over-swamp.broken-planks`
- `light-over-swamp.predator-tracks`
- `light-over-swamp.keepsake-owner`

Dowód nie powinien mieć ID opisującego błędną interpretację jako fakt.

Zamiast:
- `ghost-killed-missing-person`

używamy:
- `light-over-swamp.body-wounds`

## NPC

Dopóki postać nie ma finalnego imienia, ID opisuje rolę:

- `missing-family`
- `crossing-keeper`
- `herbalist`
- `community-guard`
- `shrine-keeper`

Po nadaniu imienia nie zmieniamy ID automatycznie.

## Przedmioty

Format:

```text
<nazwa-funkcjonalna>
```

Przykłady:

- `missing-person-keepsake`
- `marsh-herb`
- `ritual-thread`
- `tracking-tonic`

Unikamy ID typu:
- `item001`;
- `questitem2`;
- `green-herb`.

## Receptury

Format:

```text
recipe.<wynik-lub-funkcja>
```

Przykład:
- `recipe.marsh-sight-tonic`

## Czary

Format:

```text
spell.<funkcja>
```

Przykład:
- `spell.reveal-trace`

## Rytuały

Format:

```text
ritual.<funkcja>
```

Przykład:
- `ritual.release-bound-echo`

## Flagi świata

Format:

```text
<obszar>.<stan>
```

Przykłady:

- `crossing.inspected`
- `crossing.reopened`
- `swamp.apparition-released`
- `swamp.apparition-bound`
- `swamp.anchor-destroyed`
- `swamp.predator-dead`

Flaga opisuje fakt stanu świata, nie ocenę moralną.

## Relacje i reputacje

Nie tworzymy ręcznie osobnych stringów, jeśli typ danych posiada zakres.

Przykład logiczny:

```text
ReputationScope.Village + old-village
RelationshipKind.Trust + herbalist
DivineRelationship + perun
```

## Assety

Format:

```text
<typ>/<obszar>/<nazwa>
```

Przykłady:

- `models/characters/hunter-base`
- `models/props/missing-person-keepsake`
- `textures/terrain/black-swamp-mud`
- `audio/ambience/black-swamp-night`

## Migracja ID

Jeżeli ID musi zostać zmienione po powstaniu save:

1. stare ID pozostaje aliasem;
2. migrator save zamienia stare na nowe;
3. dopiero po migracji usuwamy alias;
4. zmiana jest odnotowana w dokumentacji.

## Zakazane wzorce

- ID zależne od kolejności w tablicy;
- ID generowane losowo dla treści projektowej;
- tłumaczone ID;
- ID zawierające finalną interpretację tajemnicy;
- duplikowanie tego samego ID w różnych definicjach tego samego typu.
