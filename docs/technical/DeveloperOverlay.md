# Developer Overlay — v0.1

## Cel

Szybko diagnozować runtime bez podłączania pełnego profilera do każdej drobnej sytuacji.

## Basic

- FPS;
- frame time;
- player position;
- current region;
- time of day;
- weather;
- player health/stamina.

## World

- active enemies;
- active NPCs;
- active interactions;
- current boundary phenomena;
- loaded scene/sector.

## Quest

- active quest IDs;
- phases;
- selected world flags;
- evidence count.

Nie pokazujemy wszystkich danych domyślnie; sekcje rozwijane.

## Input

Tryb input debug:
- held actions;
- pressed actions;
- mouse delta;
- controller state później.

## Rendering

- draw calls;
- vertices/indices;
- GPU memory jeśli dostępne;
- active pipeline/material count.

## Performance

- update ms;
- AI ms;
- render submit ms;
- save/load timing.

## Logging controls

- current log level;
- ostatnie warnings/errors;
- możliwość skopiowania diagnostycznego snapshotu później.

## Commands

Nie budujemy od razu pełnej konsoli developerskiej, ale overlay może mieć akcje:
- teleport do regionu;
- set time;
- set weather;
- heal;
- reload content;
- save checkpoint.

Takie akcje nie mogą być dostępne w release build bez flagi developerskiej.

## Screenshot diagnostics

Opcjonalnie zapis:
- obrazu;
- pozycji;
- timestampu;
- log tail;
- build SHA.

## Binding

Osobna akcja developerska, nie część finalnego keymap gracza.
