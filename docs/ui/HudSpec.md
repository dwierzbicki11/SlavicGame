# HUD — specyfikacja v0.1

## Cel

HUD pokazuje tylko informacje potrzebne do bieżącej decyzji. Nie może zasłaniać świata ani zamieniać eksploracji w czytanie wskaźników.

## Elementy stałe

### Health
- czytelny pasek lub forma równoważna;
- wartość maksymalna nie musi być zawsze liczbowo wyświetlana;
- stan krytyczny ma sygnał poza samym kolorem.

### Stamina
- widoczna podczas ruchu/akcji kosztującej staminę;
- może przygasać, gdy pełna i nieużywana;
- wyczerpanie komunikowane animacją/ikoną, nie tylko kolorem.

## Elementy kontekstowe

### Interaction prompt
Pokazuje:
- akcję;
- binding;
- krótki obiekt/rolę.

Przykład:
`E — Porozmawiaj`

### Quest update
Krótki komunikat:
- nowy cel;
- nowy dowód;
- zmiana fazy.

Nie wyświetlamy ściany tekstu; pełna treść trafia do Journal.

### Status effect
Pokazuje:
- ikonę;
- nazwę;
- czas, jeśli mechanicznie istotny.

### Bow/aiming
Dopiero podczas celowania:
- reticle;
- stan naciągu/stabilności;
- amunicja.

## Combat feedback

- otrzymane obrażenia;
- trafienie;
- blok;
- brak staminy;
- target feedback tylko jeśli target lock zostanie przyjęty.

Nie używamy obowiązkowych floating damage numbers w stylu MMO bez playtestu.

## Tracking feedback

Ślady mają informację głównie w świecie. HUD może pokazać:
- nazwę akcji;
- krótki typ znalezionego śladu;
- potwierdzenie wpisu do dziennika.

## Quest markers

Domyślnie ograniczone.

Dozwolone:
- znany cel w osadzie;
- znane miejsce;
- punkt umówiony przez NPC.

Nie pokazujemy automatycznie dokładnej pozycji sekretnego dowodu.

## Accessibility

Każdy krytyczny sygnał powinien mieć minimum dwie z warstw:
- kolor;
- kształt;
- tekst/ikona;
- animacja;
- dźwięk.

## Debug HUD

Developerski overlay jest osobnym systemem i nie jest częścią finalnego HUD.

## Vertical slice minimum

P0:
- health;
- stamina;
- interaction prompt;
- quest update;
- journal notification;
- prosty reticle dla łuku później.

## Otwarte

- finalny layout;
- stylistyka;
- minimal HUD mode;
- target lock UI;
- floating damage numbers.
