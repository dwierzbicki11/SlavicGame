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

## Harmonogramy NPC — stan implementacji

`NpcScheduleSlot` obsługuje przedziały z włączonym początkiem i wyłączonym
końcem oraz zmiany przez północ, np. `18–6`. Jawny przedział obejmujący
co najmniej 24 godziny, np. `0–24` lub `6–30`, oznacza aktywność całodobową.
Identyczne godziny początku i końca oznaczają pusty przedział. Godziny
nieskończone i NaN nie pasują do harmonogramu. Regresje:
`tests/NpcScheduleRegression.cs`. Obecne harmonogramy mieszkańców nadal
dzielą dobę na pracę i odpoczynek.

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
