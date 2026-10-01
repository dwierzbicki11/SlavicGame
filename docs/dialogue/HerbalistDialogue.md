# Dialog: herbalist — graph v0.1

**NPC ID:** `herbalist`

## hb.intro.00

Text:
„Mokradła nie zaczęły nagle nienawidzić ludzi. Najpierw sprawdź, co naprawdę się zmieniło.”

Choices:
- „Co rośnie przy przeprawie?” → hb.plants
- „Co sądzisz o świetle?” → hb.light
- „Potrzebuję przygotowania.” → hb.recipe
- „Są ślady dużej istoty.” if predator-tracks → hb.predator

## hb.plants

Effects:
- reveal `marsh-herb` gathering area;
- explain safe collection.

## hb.light

Text:
„Nie widziałam go. Widziałam za to zwierzęta omijające jedno miejsce.”

Effects:
- add observation lead, not confirmed supernatural fact.

## hb.recipe

If not unlocked:
Text:
„Mogę zrobić napar, który pomoże ci zauważyć to, co łatwo przeoczyć.”

Effects:
- unlock `recipe.marsh-sight-tonic`;
- explain ingredients.

If ingredients owned:
- craft option.

## hb.predator

Text:
„Pokaż trop. Jeśli jest zwierzęcy, powinien zostawić inne ślady niż zjawisko.”

Effects:
- strengthen physical/supernatural distinction;
- maybe bestiary observation.

# Wounds/body evidence

## hb.wounds

Requirement:
- relevant evidence.

Text:
„To obrażenia fizyczne. Nie powiem ci, co je zadało, ale nie zrobiło tego samo światło.”

Effects:
- confirmed physical cause category;
- not automatic predator guilt.

# Result ritual

Text:
„Dobrze. Miejsce ucichło bez palenia połowy mokradeł.”

Effects:
- Trust +;
- recipe/vendor unlock later.

# Result pact

If controlled terms:
„Jeśli naprawdę ma granice, obserwuj je. Umowa nie zatrzyma natury ani bestii.”

Effects:
- neutral/+.

# Result destroy

If environment unharmed:
„Szybkie rozwiązanie. Nie wiem, czy dobre.”

If player caused damage:
- stronger negative.

# Predator

If dead:
- may request examination/material cautiously.

If alive:
- warns crossing remains unsafe.

## Post quest services

- consumables;
- ingredients;
- recipes;
- future personal quest.
