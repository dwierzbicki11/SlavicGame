# Dialog: crossing-keeper — graph v0.1

**NPC ID:** `crossing-keeper`

## ck.intro.00

Text:
„Jeśli idziesz na mokradła, uważaj na przeprawę. Po zmroku nikt rozsądny tam nie zostaje.”

Choices:
- „Co ją uszkodziło?” → ck.damage
- „Widziałeś zaginionego?” → ck.missing
- „Widziałeś światło?” → ck.light
- „Jest inna droga?” → ck.route

## ck.damage

Text:
„Nie wiem. Deski są wyrwane, ale woda sama tego nie zrobiła.”

Effects:
- point player to broken-planks observation;
- no fact about cause.

## ck.missing

Text:
„Przechodził tędy. Jeszcze przed zmierzchem.”

Effects:
- if family statement already known → confirm `last-route`;
- otherwise add statement needing cross-check.

## ck.light

Text:
„Widziałem blask. Nie bestię. Ludzie mieszają jedno z drugim.”

Effects:
- rumor `witness-light`;
- small support for two-causes later.

## ck.route

Text:
„Można obejść mokradła, ale trwa to dłużej.”

Effects:
- optional map marker / alternate path later.

# After predator tracks

## ck.predator.00

Requirement:
- predator-tracks.

Text:
„To są prawdziwe tropy. Ale nie widziałem ich właściciela przy samym świetle.”

Choices:
- „Czyli dwa zagrożenia?” → add/strengthen hypothesis;
- „Może to jedno.” → no effect;
- „Zamknij drogę na noc.” → set request flag.

# Preparation

## ck.prep.00

Text:
„Jeśli chcesz wrócić po zmroku, powiedz straży. Nie będę po ciebie szedł w ciemność.”

Effects:
- telegraph risk;
- optional checkpoint suggestion.

# Result ritual

## ck.result.ritual

If predator dead:
„Światło znikło, a bestia też nie wróci. Otworzę przeprawę.”

Effects:
- `crossing.reopened = true`.

If predator alive:
„Światło znikło, ale tropy zostały. Jeszcze jej nie otworzę w pełni.”

Effects:
- partial state `crossing.restricted`.

# Result pact

## ck.result.pact

Text:
„Umowa z czymś, czego nie mogę zobaczyć? Potrzebuję zasad, nie wiary.”

If pact terms disclosed:
- reaction neutral;
- schedule gets night restriction;
- flag terms known.

If hidden:
- Trust/reputation small negative;
- later discovery can reduce further.

# Result destroy

## ck.result.destroy

If light gone:
„Jeśli droga jest spokojna, to dla mnie ważne.”

If predator alive:
„Ale bestia nadal tam jest.”

Effects depend on predator.

# Post quest

Services:
- crossing status;
- route info;
- later shortcuts.

No quest reward.
