# Lista assetów pierwszego vertical slice — v0.1

Dokument jest listą produkcyjną, nie finalnym art bible.

## Cel

Zminimalizować liczbę assetów potrzebnych do pełnej pętli 30–45 minut, zachowując reprezentatywną jakość.

## Environment — Żarnowiec

Minimum:
- 3 warianty prostych budynków;
- 1 warsztat / zielarka;
- 1 miejsce odpoczynku;
- ogrodzenia;
- studnia albo wspólny punkt osady;
- 5–8 małych propów gospodarczych;
- droga;
- palenisko / światła nocne.

## Environment — Puszcza Żywia

Minimum:
- 4–6 wariantów drzew;
- 3–4 krzewy;
- ściółka;
- kamienie;
- fallen trunk;
- mały obóz / miejsce pracy;
- collectible nodes;
- landmarks dla dwóch tras.

## Environment — Czarne Mokradła

Minimum:
- podłoże błotne;
- woda płytka;
- roślinność bagienna;
- uszkodzona przeprawa;
- standing stone / landmark;
- miejsce śmierci;
- światło zjawiska;
- ślady fizyczne i nadnaturalne.

## Environment — Kamienny Krąg

Minimum:
- kamienie kręgu;
- centralne miejsce rytuału;
- miejsce interakcji ze znakami;
- małe props rytualne F;
- wariant nocny/aktywny.

## Characters

### Player
- placeholder hunter model lub prosty rig;
- idle;
- walk;
- run;
- basic attack;
- block;
- dodge;
- bow aim później;
- interact.

### NPC
Pięć sylwetek może początkowo korzystać ze wspólnego bazowego rigu.

Potrzebne:
- 5 wariantów stroju/sylwetki;
- idle;
- walk;
- talk gesture;
- work animation minimum dla 2–3 ról.

### Predator
- 1 model;
- idle;
- locomotion;
- alert;
- attack;
- hit;
- death.

### Apparition
- 1 sylwetka/efekt;
- idle/float;
- react to keepsake;
- ritual reaction;
- fade/release.

### Neutral forest guardian
Może pozostać placeholderem do czasu research card.

## Items

Modele/ikony:
- keepsake;
- marsh herb;
- forest resin;
- ritual thread;
- tonic;
- bandage;
- starter weapon;
- simple bow;
- arrow.

## VFX

- trace reveal;
- apparition glow;
- boundary leak;
- ritual boundary;
- ritual success;
- ritual invalid/incomplete;
- weather rain;
- fog already partly systemic.

## UI

- interaction prompt;
- dialogue panel;
- choice list;
- inventory grid/list;
- item tooltip;
- quest journal;
- evidence type marker;
- ritual requirement panel;
- health/stamina HUD;
- simple crosshair/aiming for bow later.

## Audio

Ambience:
- village day/night;
- forest day/night;
- swamp day/night;
- shrine day/night;
- rain/storm.

SFX:
- footsteps terrain variants;
- interaction;
- inventory;
- predator;
- apparition;
- ritual;
- melee;
- bow later.

Music:
- exploration;
- tension;
- combat;
- ritual/encounter;
- aftermath.

## Asset priority

### P0 — potrzebne do pełnego przejścia
- czytelne bryły miejsc;
- pięć NPC jako placeholdery;
- predator;
- apparition;
- quest items;
- interaction/dialog UI;
- basic combat anims;
- essential audio cues.

### P1 — reprezentatywna jakość
- lepsze środowisko;
- finalniejsze stroje;
- VFX;
- ambience;
- quest journal polish.

### P2 — polish
- dodatkowe warianty dekoracji;
- secondary animations;
- richer music;
- advanced particles.

## Reuse

Dozwolone:
- wspólny rig NPC;
- wspólne materiały drewna/kamienia;
- modułowe budynki;
- wspólne UI components.

Niepożądane:
- identyczna sylwetka wszystkich pięciu NPC;
- kopiowanie tego samego budynku bez zmian;
- ten sam VFX dla Nawii, boga i Czwartej Sfery.

## Definition of Ready dla assetu

Asset ma:
- ID;
- właściciela/system użycia;
- skalę;
- pivot/origin;
- kolizję jeśli potrzebna;
- materiał;
- budżet jakości;
- informację czy jest placeholderem.

## Otwarte

- styl finalny postaci;
- poly/texture budgets;
- docelowy format asset importu;
- VO;
- finalny art direction po pierwszym asset pipeline.


## Runtime NPC asset status

P0 placeholder requirement for visible settlers is now implemented:
- five authored NPC silhouette variants on the shared animated rig;
- eight ambient Żarnowiec settlers;
- lightweight role accessories;
- idle/walk reuse from the shared rig;
- no fake dialogue prompt on ambient settlers.

Still open for P1:
- final researched clothing families;
- dedicated work animations;
- more face/hair variety;
- authored hand placement for held props;
- final texture/material pass.
