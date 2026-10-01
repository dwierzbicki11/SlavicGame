# Przedmioty pierwszego wycinka — v0.1

Dokument definiuje minimalny zestaw przedmiotów potrzebny do „Światła nad mokradłem”. Nazwy wyświetlane są robocze; identyfikatory mają być stabilne.

## Zasady

Każdy przedmiot posiada:

- ID;
- kategorię;
- nazwę wyświetlaną;
- opis;
- źródło;
- funkcję gameplay;
- możliwość sprzedaży/zużycia;
- znaczenie dla save;
- powiązane questy.

## Lista minimalna

### missing-person-keepsake

**Kategoria:** Quest  
**Robocza nazwa:** Pamiątka zaginionego  
**Źródło:** miejsce oględzin na mokradłach  
**Funkcja:** kotwica łącząca zjawę z lokalnym echem

Zasady:
- nie można przypadkowo sprzedać;
- można użyć w rytuale;
- można zniszczyć jako jedno z rozwiązań;
- identyfikacja właściciela wymaga dowodu/dialogu;
- przedmiot pozostaje osobnym obiektem od samego zjawiska.

### marsh-herb

**Kategoria:** Ingredient  
**Robocza nazwa:** Ziele mokradeł  
**Źródło:** bezpieczniejsza część mokradeł / skraj Puszczy Żywia  
**Funkcja:** składnik preparatu przygotowawczego

Zasady:
- stackable;
- nie jest rzadkim „legendarnym” surowcem;
- może rosnąć w kilku miejscach, by nie tworzyć softlocka.

### forest-resin

**Kategoria:** Ingredient  
**Robocza nazwa:** Żywica leśna  
**Źródło:** Puszcza Żywia  
**Funkcja:** drugi składnik receptury i opcjonalny materiał rytualny

Zasady:
- stackable;
- możliwy do zdobycia za dnia;
- nie wymaga walki.

### ritual-thread

**Kategoria:** Tool / Quest  
**Robocza nazwa:** Nić obrzędowa  
**Źródło:** zielarka albo opiekun miejsca kultowego  
**Funkcja:** wyznaczenie granicy rytuału

To rozwiązanie jest F i nie jest prezentowane jako rekonstrukcja historycznego obrzędu.

### marsh-sight-tonic

**Kategoria:** Consumable  
**Robocza nazwa:** Napar tropiciela  
**Źródło:** alchemia  
**Funkcja:** czasowo zwiększa czytelność śladów na mokradłach

Efekt prototypowy:
- poprawia widoczność aktywnych śladów;
- nie wskazuje automatycznie celu;
- nie identyfikuje gatunku przeciwnika.

### simple-bandage

**Kategoria:** Consumable  
**Robocza nazwa:** Opatrunek  
**Źródło:** ekwipunek startowy / zielarka  
**Funkcja:** podstawowe leczenie poza natychmiastowym spamem w walce

Dokładny model leczenia pozostaje do testów.

### hunter-knife

**Kategoria:** Tool / Weapon  
**Robocza nazwa:** Nóż łowcy  
**Źródło:** start  
**Funkcja:** interakcje terenowe i awaryjna broń

Nie zastępuje podstawowej broni melee.

### starter-melee-weapon

**Kategoria:** Weapon  
**Robocza nazwa:** Broń łowcy  
**Źródło:** start  
**Funkcja:** podstawowy prototyp walki

Typ finalny zostanie wybrany po ustaleniu animacji, balansu i researchu uzbrojenia.

### simple-bow

**Kategoria:** Weapon  
**Źródło:** późniejsza część vertical slice / wyposażenie łowcy  
**Funkcja:** podstawowa walka dystansowa

Finalny typ łuku wymaga researchu materiałowego.

### arrow-basic

**Kategoria:** Ammunition  
**Źródło:** start / warsztat  
**Funkcja:** amunicja do łuku

Model odzyskiwania strzał pozostaje otwarty.

## Receptura v0.1

### recipe.marsh-sight-tonic

Składniki:
- 1 × `marsh-herb`;
- 1 × `forest-resin`;
- woda / baza nieśledzona osobno w pierwszym prototypie.

Wynik:
- 1 × `marsh-sight-tonic`.

Cel projektowy:
- pokazać crafting;
- nagrodzić przygotowanie;
- nie blokować questa, jeśli gracz z niego nie skorzysta.

## Przedmioty a rozwiązania questa

| Rozwiązanie | Wymagany przedmiot |
|---|---|
| Domknięcie obrzędem | pamiątka + nić obrzędowa + składniki |
| Umowa ze zjawą | pamiątka jako potwierdzenie tożsamości |
| Zniszczenie kotwicy | pamiątka |

## Persistence

Save przechowuje:
- ilości stacków;
- posiadanie quest item;
- zużycie preparatu;
- zniszczenie kotwicy jako stan świata/questa.

## Co pozostaje otwarte

- finalne nazwy;
- ikony;
- modele;
- ceny;
- waga;
- trwałość;
- dokładne statystyki broni;
- historyczne odpowiedniki finalnej broni i łuku.
