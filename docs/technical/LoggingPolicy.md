# Logging i raportowanie błędów — v0.1

## Cele

Log ma:
- pomagać znaleźć przyczynę;
- nie zalewać dysku;
- nie ujawniać wrażliwych danych;
- posiadać kategorię i severity.

## Poziomy

- Trace — bardzo szczegółowe, developerskie;
- Debug — diagnostyka;
- Info — ważne etapy działania;
- Warning — problem z fallbackiem;
- Error — operacja nieudana;
- Critical — runtime nie może bezpiecznie kontynuować.

## Kategorie

- Core;
- Window;
- Input;
- Renderer;
- Assets;
- World;
- AI;
- Quest;
- Save;
- Dialogue;
- Audio;
- Network — tylko jeśli kiedyś powstanie.

## Format

Minimum:
- timestamp;
- severity;
- category;
- message;
- exception;
- build/version.

## Co logować

Start:
- version;
- platform;
- renderer backend;
- GPU;
- resolution;
- active config.

Save:
- version schema;
- slot/path logiczny;
- sukces/porażka;
- czas operacji.

Quest:
- tylko ważne transitions i invalid state.

Assets:
- missing;
- fallback;
- load failure.

## Czego nie logować

- pełnych prywatnych ścieżek użytkownika, jeśli niepotrzebne;
- danych kont;
- tokenów;
- wielkich dumpów każdego frame.

## Rate limiting

Powtarzający się warning w pętli:
- agregujemy;
- albo ograniczamy częstotliwość.

## Crash package

Docelowo może zawierać:
- log tail;
- build version;
- platform/GPU;
- stack trace;
- opcjonalny user note.

Wysyłanie automatyczne wymaga zgody użytkownika.

## Retention

Developersko: kilka rotowanych plików.

Release: ograniczony rozmiar i liczba.

## CI

Test failure powinien podawać:
- nazwę testu;
- exception;
- kluczowy stan;
- bez ogromnego niepowiązanego logu.
