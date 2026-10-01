# Dialog: shrine-keeper — graph v0.1

**NPC ID:** `shrine-keeper`

## sk.intro.00

Text:
„Miejsce reaguje na granicę, ale reakcja nie mówi ci jeszcze, kto stoi po drugiej stronie.”

Choices:
- „Co to za miejsce?” → sk.place
- „Co wiesz o świetle?” → sk.light
- „Potrafisz je zamknąć?” → sk.ritual
- „Czy to działanie boga?” → sk.god

## sk.place

Text:
„Służyło do wyznaczania granic i składania zobowiązań. Starszych rzeczy nie będę zgadywać.”

Keeps deeper origin open.

## sk.light

Text:
„Światło może być śladem. Ślad może należeć do zmarłego albo tylko go przypominać.”

Effects:
- Journal interpretation about identity uncertainty.

## sk.ritual

If no keepsake/evidence:
„Najpierw znajdź coś, co łączy zjawisko z osobą. Inaczej będziesz zamykał coś, czego nie rozpoznałeś.”

If sufficient evidence:
→ sk.learn.ritual.

## sk.learn.ritual

Effects:
- unlock `ritual.release-bound-echo`;
- unlock first sign identity/recognition;
- unlock second sign boundary/closure;
- quest may enter Preparation.

Explains:
- place;
- time;
- ingredients;
- retry on failure.

## sk.god

Text:
„Nie każde poruszenie granicy jest głosem boga.”

Effects:
- explicitly prevents false certainty.

# Optional divine branch

## sk.divine.offer

Requirement:
- right time/state;
- optional encounter available.

Choices:
- „Wysłucham.” → divine encounter
- „Nie.” → no penalty
- „Jakie są warunki?” → explains before acceptance

No mandatory patron.

# Result ritual

Text:
„Rozpoznałeś więź, zanim ją zamknąłeś. Tak powinno być.”

Effects:
- trust/reputation institution + small.

# Result pact

If terms known:
„Umowa jest granicą tak długo, jak obie strony ją uznają.”

Reaction depends on content, not blanket rejection.

# Result destroy

If owner confirmed:
„Przerwałeś więź, ale razem z nią zniszczyłeś część świadectwa.”

If owner unknown:
„Zniszczyłeś odpowiedź, zanim zadałeś właściwe pytanie.”

Effects:
- negative institution reputation/Trust.

# Post quest

Can discuss:
- meaning of result;
- not global crisis truth;
- future ritual hooks.
