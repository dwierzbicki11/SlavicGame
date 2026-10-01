# Inventory UI flow — v0.1

## Cel

Ekwipunek ma wspierać przygotowanie i szybkie zrozumienie posiadanych narzędzi.

## Główne widoki

### Lista / grid
Każdy wpis pokazuje:
- ikonę;
- nazwę;
- ilość;
- kategorię;
- stan wyposażenia.

### Szczegóły
Po wyborze:
- opis;
- funkcja;
- wymagania;
- cena, jeśli istotna;
- akcje.

## Kategorie

- Weapons;
- Armor;
- Tools;
- Consumables;
- Ingredients;
- Quest;
- Artifacts;
- Misc.

## Akcje

Zależnie od typu:
- Equip;
- Unequip;
- Use;
- Inspect;
- Drop;
- Split stack;
- Move;
- Compare.

Quest item nie pokazuje destrukcyjnej akcji, jeśli nie jest ona świadomym rozwiązaniem questa.

## Compare

Dla broni/armor:
- pokazuje różnice cech;
- nie sprowadza wyboru wyłącznie do zielonej/czerwonej liczby.

## Sorting

Minimum:
- kategoria;
- nazwa;
- ilość.

Później:
- wartość;
- ostatnio zdobyte.

## Crafting entry point

Alchemy może być:
- osobną zakładką;
- albo aktywna tylko przy stanowisku.

Vertical slice: prosty dostęp przy odpowiednim NPC/stanowisku.

## Controller

UI musi nadawać się do sterowania focus-based, nawet jeśli kontroler wejdzie później.

## Persistence

UI nie przechowuje stanu gameplay. Po zamknięciu odczytuje aktualny InventoryState.

## Empty states

Brak przedmiotów:
- jasny komunikat;
- brak pustych interaktywnych elementów.

## Vertical slice

Potrzebne:
- listowanie 8–10 typów;
- użycie consumable;
- widoczność quest item;
- receptura naparu;
- equip starter weapon;
- brak przypadkowego zniszczenia pamiątki.
