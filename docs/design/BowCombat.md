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

## Runtime pass — physical hunting bow

Pierwszy grywalny runtime łuku jest podpięty do vertical slice.

### Sterowanie
- **RMB held** — Aim;
- **LMB press + hold** podczas Aim — Draw;
- **LMB release** — Release;
- puszczenie RMB podczas Draw — Cancel bez utraty ammo;
- **E** przy strzale wbitej w teren — odzyskanie `arrow-basic`.

Poza Aim LMB nadal uruchamia melee.

### Starter loadout
Jednorazowy migration flag `starter-bow-loadout-granted` daje:
- `simple-bow` x1;
- `arrow-basic` x16.

Mechanizm działa również dla wcześniejszych save'ów po restore, bez dublowania loadoutu.

### Draw i damage
Prototype tuning:
- full draw: ~1.25 s;
- minimalny skuteczny draw fraction: 18%;
- velocity: ~22–52 m/s;
- damage: ~14–38;
- grawitacja: 9.81 m/s².

Ammo jest zdejmowane dokładnie przy Release, nie przy Aim/Draw.

### Projectile
Każda wypuszczona strzała ma własną pozycję, velocity, gravity, damage i lifetime.
Symulacja dzieli frame na kroki do około 1/90 s, więc szybka strzała nie jest frame-dependent hitscanem.

Collision obejmuje teren, żywego przeciwnika i ambient wildlife.
Trafienie EnemyAgent korzysta z normalnego damage/aggro path. Trafienie swamp-predator może domknąć jego istniejący encounter.

### Hunting wildlife
Pierwszy prototype health:
- raven: 10;
- deer: 45;
- wolf: 50;
- boar: 70.

Terminalny kill zapisuje `wildlife.dead.<id>` jako world flag. Zwierzę nie jest renderowane ani ponownie trafialne po save/load. Partial wildlife damage nie jest jeszcze zapisywany.

### Retrieval
Strzała trafiająca teren zostaje jako recoverable world arrow:
- maksymalnie 24 aktywne sztuki;
- dystans podniesienia: ~2.6 m;
- podniesienie zwraca dokładnie jedną `arrow-basic`.

Strzały trafiające target nie są w tym pass odzyskiwane.

### Presentation
Runtime używa `luk_r0_01.glb` oraz `strzala_r0_01.glb`.
Aim pokazuje bow model, nocked arrow podczas draw, reticle, ammo count i draw percentage.
Flying/stuck arrows używają tego samego actor bufferu.

### QA lock
Regresje wymagają cancel bez utraty ammo, ammo -1 dokładnie przy release, full draw, fizycznego trafienia przeciwnika, aggro po trafieniu, terrain stick + retrieval, kill wildlife + persistence oraz poprawnej geometrii bow/arrow.
