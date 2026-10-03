# Grafy dialogowe vertical slice — v0.1

Dokument definiuje strukturę treści, nie finalny literacki wording.

## Konwencja

Node:
`npc.context.number`

Choice:
`npc.context.number.choice`

Effects opisują systemy, nie gotowy kod.

# missing-family

## mf.offer.01

Speaker:
„Nie wrócił z przeprawy. Ludzie mówią o świetle.”

Choices:
- „Kiedy wyszedł?” → evidence `last-route`
- „Co zabrał ze sobą?” → clue do keepsake
- „Kto widział światło?” → kieruje do guard
- „Przyjmuję zlecenie.” → quest Active
- „Wrócę później.” → bez kary

## mf.investigation.01

Requirement:
- keepsake found.

Choices:
- „Znalazłem ten przedmiot.” → możliwość identyfikacji owner
- „Są też ślady dużej istoty.” → reakcja niepokoju
- „Jeszcze za wcześnie na wniosek.” → neutral

## mf.result.ritual

Reakcja:
- wdzięczność;
- potwierdzenie losu;
- reward path A.

## mf.result.pact

Reakcja:
- ulga + obawa;
- pytanie czy zjawa cierpi/pozostaje.

## mf.result.destroyed

Reakcja:
- negatywna;
- szczególnie jeśli brak owner evidence.

# crossing-keeper

## ck.intro.01

- opis uszkodzonej przeprawy;
- nacisk na szybkość;
- rumor: „coś wychodzi nocą”.

Choices:
- pytaj o damage;
- pytaj o ludzi;
- pytaj o drogę alternatywną.

## ck.evidence.predator

Requirement:
- predator tracks.

Reakcja:
- przyznaje, że nie widział światła i bestii jednocześnie;
- wzmacnia two-causes hypothesis.

## ck.result.*

Ritual:
- pozytywnie, jeśli crossing reopen.

Pact:
- chce jasnych godzin/warunków.

Destroyed:
- pozytywny praktycznie, ale ignoruje utratę świadectwa.

# herbalist

## hb.intro.01

- ostrzega przed niszczeniem mokradeł;
- oferuje recepturę po podstawowej rozmowie.

## hb.recipe.01

Effect:
- unlock `recipe.marsh-sight-tonic`.

## hb.evidence.wounds

Pomaga odróżnić physical attack od apparition.

## hb.result.*

Reakcje zależą od:
- environment damage;
- predator state;
- apparition outcome.

# community-guard

## cg.intro.01

- lista świadków;
- night watch;
- strefa zamknięta.

## cg.rumors.01

Może przekazać `witness-light`.

Nie potwierdza prawdziwości interpretacji.

## cg.result.pact

Wymaga:
- znanych zasad umowy.

Jeśli gracz nie zna/nie ujawni warunków:
- niższe zaufanie.

# shrine-keeper

## sk.intro.01

- rozróżnia „reakcję miejsca” od pewnej identyfikacji zmarłego;
- nie twierdzi, że zna przyczynę całego kryzysu.

## sk.ritual.learn

Requirements:
- quest Investigation/Preparation;
- keepsake lub dowód nieszczelności.

Effects:
- unlock ritual;
- unlock first signs.

## sk.divine.optional

Opcjonalna gałąź:
- manifestacja/posłaniec;
- accept/decline.

## sk.result.*

Ritual:
- pozytywna.

Pact:
- ocena zależna od zasad.

Destroy:
- krytyczna, jeśli gracz nie przeprowadził rozpoznania.

## Zasada

Finalny wording powstaje dopiero po:
- imionach NPC;
- tonie kultury;
- naming rules;
- voice direction.


# Runtime implementation pass

Vertical slice ma teraz wykonywalny runtime dialogów dla pięciu głównych ról NPC.

## World presence

NPC nie są już wyłącznie wpisami harmonogramu:
- runtime wylicza ich aktualną pozycję z pory dnia, lokacji i aktywności;
- w dzień i noc mogą przebywać w różnych miejscach;
- każdy NPC jest renderowany istniejącym humanoidalnym modelem placeholderowym z kolorem zależnym od roli;
- podczas rozmowy NPC obraca się w stronę gracza.

## Sterowanie

Po podejściu do NPC:
- `E` — rozpocznij rozmowę;
- `W/S` — zmień zaznaczoną odpowiedź;
- `E` — wybierz odpowiedź;
- `Esc` — zakończ rozmowę.

Dialog blokuje ruch, atak, czary i pozostały gameplay input.

## Choice gates

Opcje mogą być ukryte przez:
- quest phase;
- evidence;
- item;
- reputation;
- divine favor;
- world flag.

Runtime nie pokazuje opcji, której wymagań gracz nie spełnia.

## Effects

Wykonywane efekty wspierają:
- zmianę fazy questa;
- dodanie evidence;
- give/take item;
- reputację;
- relację z NPC;
- divine favor;
- world flag.

## Vertical-slice graphs

Pierwszy runtime obejmuje:
- `missing-family` — przyjęcie zlecenia, ślady, identyfikacja pamiątki, reakcja po wyniku;
- `crossing-keeper` — stan przeprawy i hipoteza dwóch przyczyn;
- `herbalist` — receptura oraz przygotowanie do nauki Mend;
- `community-guard` — relacje świadków i reakcja na rozpoznanego drapieżnika;
- `shrine-keeper` — znaczenie miejsca, niepewność identyfikacji, rytuał i przygotowanie nauczyciela magii.

## Teacher contract

`TeacherId` w `SpellLessonDefinition` nie jest już tylko metadanymi.

Aby użyć `L` do nauki:
1. gracz musi odbyć odpowiednią rozmowę z nauczycielem;
2. runtime zapisuje `dialogue.teacher.<npc-id>.ready`;
3. nauczyciel musi być fizycznie w pobliżu;
4. dopiero wtedy oceniane są pozostałe wymagania lekcji.

Dotyczy:
- Spark → shrine-keeper;
- Mend → herbalist;
- Reveal Trace → shrine-keeper.

Stan przygotowania nauczyciela jest world flagiem, więc przechodzi przez istniejący save/load.
