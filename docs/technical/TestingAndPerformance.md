# Testy, diagnostyka i wydajność — v0.1

## Cel

Projekt ma rosnąć przez mierzalne, testowalne systemy. Nie ustalamy arbitralnych targetów bez sprzętu referencyjnego.

## Warstwy testów

### Regression CPU-only
Systemy możliwe do uruchomienia bez Vulkan:
- teren;
- kamera geometryczna;
- AI;
- inventory;
- questy;
- reputacje;
- save/load;
- czas;
- pogoda jako stan;
- logika interakcji.

### Integration
- input + movement;
- world + AI;
- quest + inventory + dialogue;
- save + pełny stan świata.

### Runtime graphical
- rendering;
- resize/fullscreen;
- utrata wejścia;
- shader compilation;
- GPU resource lifetime.

### Playtest
- czytelność;
- pacing;
- balans;
- rozpoznawanie śladów;
- skutki decyzji.

## CI

Minimum:
- Windows;
- Ubuntu;
- Release build;
- regression tests.

Zmiana nie powinna trafiać do main po czerwonym CI.

## Metryki

Zbieramy:
- frame time CPU;
- frame time GPU;
- FPS jako wartość pomocniczą;
- czas ładowania;
- peak memory;
- liczba draw calls;
- liczba aktywnych NPC/AI;
- czas save/load.

## Sprzęt referencyjny

Nie jest jeszcze zatwierdzony.

Po pierwszym vertical slice wybieramy:
- minimum;
- zalecane;
- urządzenie developerskie;
- potencjalny profil mobilny.

## Budżety

Budżety powstają dopiero po pomiarach.

Przykładowe kategorie:
- terrain;
- vegetation;
- characters;
- shadows;
- particles;
- UI;
- AI;
- streaming.

## Telemetria developerska

Plan:
- overlay FPS/frame time;
- region;
- pozycja;
- liczba aktorów;
- pogoda;
- czas świata;
- pamięć;
- log ostatnich eventów.

## Save/load

Każdy nowy trwały system powinien mieć test:
1. ustaw stan;
2. zapisz;
3. utwórz świeży świat;
4. wczytaj;
5. porównaj.

## Determinizm

Nie wymagamy pełnego deterministycznego świata, ale:
- testy używają jawnych seedów;
- losowość nie może czynić regresji niestabilnymi.

## Profilowanie przed optymalizacją

Nie optymalizujemy na podstawie intuicji, jeśli profiler może pokazać rzeczywiste wąskie gardło.
