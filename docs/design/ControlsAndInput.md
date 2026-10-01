# Sterowanie i input — v0.1

## Aktualne PC

- WASD — ruch;
- Left Shift — sprint;
- mouse — kamera;
- F11 — fullscreen;
- Escape — wyjście.

## Docelowy zestaw

Exploration:
- move;
- camera;
- sprint;
- crouch, jeśli zostanie przyjęty;
- interact;
- journal/map/inventory.

Combat:
- light attack;
- heavy attack;
- block;
- dodge;
- aim;
- ranged attack;
- spell/use item.

## Zasady

- ruch i kamera muszą działać jednocześnie;
- holding key nie może gubić mouse look;
- input nie zależy od FPS;
- UI może blokować gameplay input w jawny sposób.

## Mouse capture

Istnieją event/polled motion jako alternatywne źródła. Nie sumujemy ich, jeśli opisują ten sam ruch.

## Remapping

Docelowo wszystkie akcje gameplay mają nazwane bindings zamiast bezpośrednich Key checks rozrzuconych po systemach.

## Kontroler

Po stabilizacji KBM:
- left stick move;
- right stick camera;
- triggers/bumpers combat;
- dead zones;
- glyph switching.

## Android

Dotykowy input będzie osobnym profilem, nie symulacją klawiatury.

## Accessibility

Zgodnie z UXAccessibility:
- sensitivity;
- invert;
- toggle/hold;
- remapping.
