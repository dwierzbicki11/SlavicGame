# Lokacja: Żarnowiec — karta v0.1

**ID:** `old-village`  
**Typ:** hub / village  
**Status:** F

## Funkcja

Żarnowiec jest pierwszym bezpieczniejszym centrum gracza.

Zapewnia:
- zlecenia;
- odpoczynek;
- handel;
- alchemię;
- reakcje na decyzje;
- zmianę rytmu dzień/noc.

## Czytelność

Gracz powinien szybko rozpoznać:
- centralną drogę;
- dom rodziny zaginionego;
- zielarkę;
- miejsce straży;
- wyjście na przeprawę;
- drogę do puszczy.

## Strefy

### Wejście / droga
Pierwszy kontakt z ruchem mieszkańców.

### Plac / punkt wspólny
Quest hand-in, plotki, ruch NPC.

### Zielarka
Crafting i przygotowanie.

### Straż
Informacje o zagrożeniach.

### Gospodarstwa
Tło codzienności, nie wszystkie interaktywne.

## Dzień

- większość NPC dostępna;
- vendorzy działają;
- więcej ambient conversations;
- ruch między domami.

## Zmierzch

- mieszkańcy wracają;
- straż przejmuje nocny rytm;
- wizualny sygnał, że świat się zmienia.

## Noc

- mniej NPC;
- część drzwi zamknięta;
- night watch;
- ograniczone usługi.

## Harmonogramy NPC — implementacja

`NpcScheduleSlot` uwzględnia godzinę początku i pomija godzinę końca.
Przedział `19–6` przechodzi przez północ. Jawny przedział o długości co
najmniej 24 godzin (np. `0–24` lub `6–30`) oznacza całodobowy dyżur;
identyczne godziny początku i końca oznaczają pusty przedział.
Godziny spoza zakresu `0–24` są normalizowane, a wartości niefinitywne
są odrzucane. Obsługa pełnej doby nie zmienia istniejących dyżurów mieszkańców.

## Stan po queście

### ritual-closure
- spokojniejszy ruch;
- przeprawa może wrócić do normalnego działania;
- rodzina reaguje pozytywnie.

### conditional-pact
- nocne ostrzeżenia pozostają;
- rozmowy o układzie dzielą mieszkańców.

### destroy-anchor
- światła znikają, ale rodzina i shrine-keeper kwestionują sposób.

## Encountery

Żarnowiec nie ma być stale atakowany. Pierwszy region uczy, że bezpieczny hub ma wartość.

## Assety P0

- 3 budynki modułowe;
- zielarka/warsztat;
- guard post;
- ogrodzenia;
- paleniska;
- podstawowe props.

## Otwarte

- finalna liczba mieszkańców;
- plan architektoniczny po researchu;
- finalne nazwy NPC;
- polityczna przynależność osady.
