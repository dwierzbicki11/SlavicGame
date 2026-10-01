# Dialog: community-guard — graph v0.1

**NPC ID:** `community-guard`

## cg.intro.00

Text:
„Po zmroku nikt nie idzie na przeprawę sam. Nie dlatego, że wierzę każdej historii.”

Choices:
- „Kto widział światło?” → cg.witnesses
- „Kto widział napastnika?” → cg.attacker
- „Dlaczego zamknęliście drogę?” → cg.closure
- „Co robicie nocą?” → cg.patrol

## cg.witnesses

Effects:
- add `witness-light` rumor;
- identify multiple inconsistent accounts.

## cg.attacker

Text:
„Nikt, komu ufam, nie widział bestii i światła jako jednej rzeczy.”

Effects:
- no confirmed fact;
- supports investigation.

## cg.closure

Text:
„Jedna osoba zniknęła. Nie dam następnej.”

Effects:
- explains crossing restriction.

## cg.patrol

- night schedule information;
- optional warning.

# After physical tracks

## cg.predator

Text:
„To zmienia sprawę. Jeśli coś poluje w pobliżu, światło nie jest jedynym problemem.”

Effects:
- unlock predator objective/optional combat objective.

# Pre-night

## cg.prep

Choices:
- „Idę dziś w nocy.” → guard acknowledges; optional checkpoint;
- „Poczekam.” → no penalty.

# Result ritual

If predator dead:
„Dwie rzeczy rozwiązane. Zdejmę część wart.”

If predator alive:
„Jedna rzecz mniej. Warty zostają.”

# Result pact

If terms disclosed:
„Podaj mi godziny i zasady. Ludzie muszą wiedzieć.”

Effects:
- `swamp.pact-terms-known`;
- no trust loss.

If hidden:
„Nie mogę pilnować układu, którego nie znam.”

Effects:
- negative.

# Result destroy

„Jeśli światło znikło, dobrze. Ale chcę wiedzieć, co z tropami.”

Predator state controls safety response.

# Post quest

Guard dialogue reflects:
- crossing state;
- predator;
- night restrictions.
