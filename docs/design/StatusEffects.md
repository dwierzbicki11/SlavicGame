# Status Effects — v0.1

## Cel

Statusy tworzą przygotowanie i różnice między zagrożeniami bez spamowania ikon.

## Typ definicji

Każdy status ma:
- ID;
- source;
- duration;
- stack policy;
- magnitude;
- tick policy;
- tags;
- UI cue.

## Stack policy

Możliwe:
- RefreshDuration;
- StackMagnitude;
- StackCountLimited;
- IgnoreIfActive;
- ReplaceWeaker.

Każdy status wybiera jedną jawną politykę.

## Pierwsze statusy

### bleeding
- źródło: Physical;
- okresowy damage;
- możliwe usunięcie bandage.

### poisoned
- źródło: Poison;
- damage/debuff;
- antidote później.

### chilled
- źródło: Cold;
- spowolnienie regen/movement do testu.

### spirit-exposure
- źródło: Spirit/Nawia;
- mechaniczny debuff związany z nieszczelnością;
- nie jest „sanity meter”.

### protected
- buff rytualny/boski;
- konkretny efekt i czas.

## Reguły

- status nie ukrywa krytycznej mechaniki;
- damage-over-time nie dobija bez czytelnego feedbacku;
- źródło statusu trafia do logu developerskiego;
- statusy zapisujemy tylko jeśli ich trwanie ma sens po save/load.

## Vertical slice

P0 może używać jedynie:
- bleeding albo poisoned dla testu consumable;
- spirit-exposure jako lokalnego debuffu na mokradłach.

Nie potrzebujemy wszystkich na start.
