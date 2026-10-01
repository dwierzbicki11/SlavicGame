# Dialog: missing-family — graph v0.1

**NPC ID:** `missing-family`  
**Quest:** `light-over-swamp`

Teksty są robocze. Node IDs i efekty są ważniejsze niż finalny wording.

## mf.offer.00 — greeting

Text:
„Szukam kogoś, kto pójdzie na mokradła. [Zaginiony] nie wrócił z przeprawy.”

Choices:
- `mf.offer.ask-when` → mf.offer.when
- `mf.offer.ask-route` → mf.offer.route
- `mf.offer.ask-item` → mf.offer.item
- `mf.offer.accept` → mf.offer.accept
- `mf.offer.decline` → END

## mf.offer.when

Text:
„Wyszedł jeszcze za dnia. Miał wrócić przed zmierzchem.”

Effects:
- add rumor/knowledge context;
- no quest phase change.

Next:
- mf.offer.00

## mf.offer.route

Text:
„Miał przejść przez przeprawę. Opiekun drogi widział go wcześniej.”

Effects:
- add evidence `light-over-swamp.last-route` only after independent confirmation OR mark as statement source pending confirmation;
- point to crossing-keeper.

Next:
- mf.offer.00

## mf.offer.item

Text:
„Miał przy sobie [charakterystyczny przedmiot]. Poznałbym go.”

Effects:
- player learns identification cue for keepsake.

Next:
- mf.offer.00

## mf.offer.accept

Text:
„Dowiedz się, co się stało. Nie chcę tylko kolejnej opowieści o świetle.”

Effects:
- quest → Active;
- world flag `light-over-swamp.accepted`;
- objective: talk to crossing-keeper;
- objective: inspect crossing.

END.

# Investigation

## mf.inv.no-keepsake

Condition:
- quest Investigation;
- keepsake not found.

Text:
„Masz coś pewnego?”

Choices:
- „Jeszcze nie.” → END
- „Znalazłem ślady dużej istoty.” if predator-tracks → mf.inv.predator
- „Przeprawa jest uszkodzona.” if broken-planks → mf.inv.crossing

## mf.inv.keepsake

Condition:
- owns `missing-person-keepsake`;
- owner not confirmed.

Text:
„To... pokaż.”

Choices:
- `mf.inv.show-keepsake` → mf.inv.identify
- `mf.inv.keep-hidden` → END

## mf.inv.identify

Text:
„Tak. To jego.”

Effects:
- add ConfirmedFact `light-over-swamp.keepsake-owner`;
- relationship Trust + small;
- objective update.

Choice:
- „Znalazłem to przy miejscu zdarzenia.” → neutral
- „To nie jedyny ślad.” if predator-tracks → mf.inv.predator

## mf.inv.predator

Text:
„Więc to coś go zabiło?”

Choices:
- „Jeszcze tego nie wiem.” → trust + small
- „Na pewno.” → set `mf.player-claimed-predator-cause`
- „Ślady nie pasują do światła.” if two-causes hypothesis → trust + small

No automatic confirmation of cause.

# Preparation

## mf.prep.01

Condition:
- quest Preparation.

Text varies:
- owner confirmed → „Wiesz już, czy to naprawdę on?”
- not confirmed → „Wciąż nie wiem, co mam myśleć.”

Choices:
- explain apparition evidence;
- explain predator;
- withhold details;
- ask about additional memory of keepsake.

# Resolution — ritual

## mf.result.ritual.00

Condition:
- `swamp.apparition-released`.

Text:
„Czy to koniec?”

Choices:
- if full evidence: „Wiem, co się stało.” → mf.result.ritual.full
- otherwise: „Wiem wystarczająco, by powiedzieć, że odszedł.” → mf.result.ritual.partial

## mf.result.ritual.full

Effects:
- Trust +;
- Village reputation +;
- reward unlock;
- set `missing-family.received-truth`.

## mf.result.ritual.partial

Effects:
- smaller Trust +;
- reward unlock;
- journal notes incomplete evidence.

# Resolution — pact

## mf.result.pact.00

Condition:
- `swamp.apparition-bound`.

Text:
„Mówisz, że nadal tam jest?”

Choices:
- explain terms if known → better reaction;
- say only „jest bezpiecznie” → set withheld flag;
- admit uncertainty → neutral.

Effects:
- reward unlock;
- reaction depends on terms disclosure.

# Resolution — destroyed

## mf.result.destroy.00

Condition:
- `swamp.anchor-destroyed`.

If owner confirmed:
„Zniszczyłeś jego przedmiot?”

If owner not confirmed:
„Nawet nie wiesz, czy to było jego.”

Choices:
- justify immediate safety;
- admit lack of certainty;
- blame supernatural danger — no automatic success.

Effects:
- Trust negative;
- shrine reaction separate;
- reward can still be paid because contract completed, but relationship differs.

# Turn-in

## mf.turnin.00

Requirement:
- quest Resolved;
- reward not claimed.

Effects:
- money reward;
- quest ClaimReward → TurnedIn;
- post-result dialogue state selected.

## Post quest fallback

Depends on result:
- ritual: family is quieter, thankful;
- pact: asks occasional questions about swamp;
- destroy: colder dialogue.

No repeated reward.
