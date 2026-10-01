# Weather Gameplay — v0.1

## Cel

Pogoda zmienia atmosferę i czasem plan, ale nie losowo psuje questów.

## Stany obecnego systemu

- Clear;
- Overcast;
- Fog;
- Rain;
- Storm.

## Visibility

Fog:
- obniża widoczność;
- wzmacnia klimat mokradeł;
- nie może ukrywać obowiązkowej interakcji bez alternatywy.

Rain:
- wpływa na ambience;
- może później skracać czytelność physical tracks.

Storm:
- rzadszy;
- może zwiększać ryzyko;
- nie używamy jako wymaganego warunku głównego questa bez systemu oczekiwania.

## Tracking

Możliwy model później:
- rain przyspiesza Faded dla części śladów;
- mud zwiększa footprint clarity;
- supernatural traces działają inaczej.

Vertical slice: mechanika może pozostać prostsza.

## Combat

Weather może wpływać na:
- visibility;
- audio masking;
- projectile handling później.

Nie wprowadzamy przypadkowych obrażeń od pioruna w podstawowym systemie.

## NPC

Schedule może mieć weather override później:
- ludzie chowają się w storm;
- część pracy jest przerwana.

Vertical slice: opcjonalne.

## Ritual

Rytuał pierwszego questa nie wymaga konkretnej losowej pogody.

## Determinism i QA

Quest-critical behavior nie może zależeć od niekontrolowanego RNG pogody.

Testy mogą ustawić weather jawnie.
