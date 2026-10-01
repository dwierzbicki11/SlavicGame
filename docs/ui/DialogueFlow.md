# Dialogue UI flow — v0.1

## Cel

Dialog ma być czytelny, szybki i wspierać informację oraz decyzje.

## Layout

Minimalne elementy:
- speaker;
- tekst;
- lista wyborów;
- opcjonalna informacja o wymaganiu;
- możliwość historii rozmowy.

## Choices

Wybór może być:
- neutralny;
- pytaniem;
- akcją;
- decyzją kończącą gałąź;
- opcją odblokowaną przez wiedzę/przedmiot/reputację.

## Requirements

Jeśli opcja jest niedostępna:
- domyślnie jej nie pokazujemy albo pokazujemy tylko wtedy, gdy brak jest informacją użyteczną;
- nie zdradzamy tajnego wymogu w stylu „Need Evidence X”, jeśli gracz nie zna jego istnienia.

## Knowledge choices

Opcja wynikająca z dowodu powinna być opisana treścią, nie ikoną „intelligence check”.

## Consequences

Jawny koszt:
- potwierdzenie przed trwałą umową;
- informacja o oddawanym przedmiocie;
- informacja o płatności.

Długoterminowy skutek może pozostać niejawny.

## Skip

- advance text;
- skip current line/animation;
- nie pomijamy automatycznie wyboru.

## History

Historia zawiera:
- ostatnie wypowiedzi;
- wybrane odpowiedzi;
- speaker.

## Timed choices

Nie używamy jako domyślnej mechaniki. Jeśli pojawią się później, mają accessibility option.

## Vertical slice

Potrzebne grafy:
- missing-family;
- crossing-keeper;
- herbalist;
- community-guard;
- shrine-keeper.

Po rozwiązaniu każdy z kluczowych NPC musi reagować na wynik.
