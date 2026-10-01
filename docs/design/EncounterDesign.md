# Encounter Design — v0.1

## Cel

Encounter to zaprojektowana sytuacja, nie losowy spawn przed graczem.

## Typy

- combat;
- social;
- tracking;
- supernatural;
- mixed;
- environmental.

## Karta encounteru

- ID;
- region;
- trigger;
- time window;
- weather conditions opcjonalnie;
- participants;
- setup;
- telegraph;
- goals;
- escape route;
- outcomes;
- persistence;
- cooldown/repeat policy.

## Combat encounter

Wymaga:
- miejsca na ruch;
- czytelnego wejścia przeciwnika;
- drogi odwrotu;
- braku spawn behind player bez sygnału.

## Supernatural encounter

Wymaga:
- oznak;
- reguły;
- warunku eskalacji;
- sposobu bezpiecznego wycofania.

## Vertical slice encounters

### swamp-predator-day
Możliwa obserwacja tropów bez pełnego starcia.

### swamp-predator-night
Pełny combat/chase.

### apparition-first-contact
Kontakt ze zjawą po aktywacji miejsca.

### shrine-learning
Kontrolowane spotkanie uczące rytuału/znaku.

### forest-guardian
Neutralne spotkanie uczące, że nie każdy byt jest wrogi.

## Repeat policy

- tutorial encounter: jednorazowy;
- predator: zależny od stanu alive/dead;
- ambient: może się powtarzać po cooldown;
- apparition: zależna od quest phase.

## Save

Trigger jednorazowy musi mieć flagę/persisted state.
