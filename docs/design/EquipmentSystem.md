# Equipment System — v0.1

## Cel

Wyposażenie zmienia możliwości gracza, ale nie wymaga dziesiątek slotów.

## Proponowane sloty

- MainHand;
- OffHand;
- Bow/Ranged;
- Head;
- Body;
- Hands opcjonalnie;
- Legs/Feet opcjonalnie;
- Tool;
- QuickItem1;
- QuickItem2;
- Amulet/Artifact później.

Vertical slice może używać uproszczonego:
- MainHand;
- Ranged;
- Body;
- Tool;
- QuickItem.

## Weapon data

Broń ma:
- ID;
- weapon class;
- damage;
- damage type;
- stamina cost modifier;
- range;
- attack set;
- requirements opcjonalnie;
- asset IDs.

## Armor data

Armor ma:
- ID;
- slot;
- physical protection;
- resistances;
- mobility modifier opcjonalnie;
- asset.

## Equip rules

- jeden item nie może być w dwóch wykluczających slotach;
- quest item nie jest sprzętem bez jawnej definicji;
- zmiana broni może wymagać krótkiej animacji później.

## Durability

Otwarte. Brak w pierwszym vertical slice.

## Weight

Otwarte. Nie wprowadzamy encumbrance bez testu.

## Persistence

Save zapisuje:
- equipped item IDs;
- quick slot assignments;
- stan trwałości tylko jeśli system powstanie.

## Comparison

UI pokazuje różnice wielowymiarowo:
- damage;
- speed/stamina;
- range;
- resistances.

Nie ma jednej „gear score”.
