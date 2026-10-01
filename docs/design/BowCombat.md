# Bow Combat — v0.1

## Cel

Łuk ma być narzędziem polowania i walki dystansowej, nie automatycznym hitscanem.

## Stan

1. Equip bow
2. Aim
3. Draw
4. Hold
5. Release
6. Recover

## Aim

Podczas Aim:
- kamera przechodzi do czytelniejszego shoulder framing;
- pojawia się reticle;
- movement może być wolniejszy;
- RMB/LT utrzymuje aim.

## Draw

Naciąg trwa w czasie.

Parametry:
- draw time;
- min velocity;
- max velocity;
- stamina effect opcjonalny;
- accuracy spread.

## Release

Strzała jest projectile.

Musi mieć:
- velocity;
- gravity;
- collision;
- owner;
- damage;
- recoverable flag.

## Ammo

Pierwszy typ:
- `arrow-basic`.

Nie dodajemy elemental arrow przed stabilizacją podstaw.

## Retrieval

Opcja v0.1:
- część strzał wbitych w świat można odzyskać;
- strzała trafiająca niedostępny obszar przepada;
- dokładna szansa/zasada do testów.

## Damage

Damage zależy od:
- draw fraction;
- projectile hit;
- target resistance później.

Headshot/weak points dopiero po hitboxach przeciwników.

## Movement

Można:
- chodzić podczas aim;
- przerwać draw;
- wykonać dodge po anulowaniu aim.

## UX

HUD:
- ammo count;
- draw/stability cue;
- reticle.

## AI reaction

Miss w pobliżu może później:
- zaalarmować;
- przyciągnąć przeciwnika.

Pierwszy prototyp może tylko zadawać damage.

## QA

- trajectory niezależna od FPS;
- projectile nie trafia właściciela;
- release po minimalnym draw działa;
- cancel nie zużywa strzały;
- ammo zmniejsza się dokładnie raz;
- save/load nie musi przechowywać strzał lecących w powietrzu.
