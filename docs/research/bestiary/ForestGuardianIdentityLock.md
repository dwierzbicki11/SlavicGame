# Identity lock: `ENTITY_FOREST_GUARDIAN_F` — v1.0

**Decyzja produkcyjna:** PASS jako świadoma fikcja `F`. Nie przypisujemy slotowi jednej historycznej nazwy folklorystycznej.

## Pytanie

Czy materiał badawczy uzasadnia zmianę `ENTITY_FOREST_GUARDIAN_F` w konkretną poświadczoną postać (np. „leszy”) dla R2 Wielki Bór?

## Evidence review

### H — bezpieczne twierdzenia źródłowe

- istnieją tradycje słowiańskich postaci/duchów związanych z lasem;
- materiał porównawczy pokazuje znaczną zmienność nazw, wyglądu i zachowania;
- leshii/lesovik jest poświadczony jako rosyjska postać leśna, ale samo istnienie późniejszego materiału rosyjskiego nie dowodzi identycznej postaci w fikcyjnym R2 ani w polskim kontekście IX–X wieku;
- istniejący research regionalny nie wskazuje jednego bezpiecznego polskiego odpowiednika dla obecnej funkcji guardian.

### R — rekonstrukcja dopuszczona w grze

Możemy wykorzystywać szeroką rodzinę motywów: lokalność, związek z granicą/użytkowaniem lasu, mylenie drogi, warunkową wrogość i możliwość pokojowego rozwiązania. Są to inspiracje projektowe, nie deklaracja historycznej klasyfikacji.

### F — author truth SlavicGame

`ENTITY_FOREST_GUARDIAN_F` jest lokalną istotą Wielkiego Boru. Jej funkcja produkcyjna to neutralny/warunkowo wrogi guardian uczący, że nadnaturalna istota nie musi być przeciwnikiem do zabicia. Lokalne NPC mogą używać diegetycznego imienia F, ale UI/research nie może przedstawiać go jako poświadczonego terminu historycznego.

### U — pozostaje otwarte

- finalne lokalne imię F;
- model, sylwetka, materiały, animacje, audio i VFX;
- dokładne taboo/rule set dla poszczególnych encounterów;
- weakness/combat tuning, jeśli konkretny encounter w ogóle dopuści walkę.

## Dlaczego nie „leszy”

Nazwa byłaby mocniejszym twierdzeniem niż dowody potrzebne obecnemu projektowi. Dostępny materiał wspiera istnienie zmiennych tradycji leśnych, lecz nie bezpieczną tezę „historycznie poprawny leszy R2 z IX–X wieku”. Identity lock nie wymaga wymuszenia nazwy folklorystycznej; poprawnym wynikiem jest zachowanie F.

## Kontrakt implementacyjny

Implementacja może traktować slot jako **identity-locked F** i budować bez dalszego researchu:

- trwałe ID `ENTITY_FOREST_GUARDIAN_F`;
- stany `hidden → observing → warning → misleading/escalated → resolved/departed`;
- reakcję na reguły miejsca zamiast automatycznego aggro;
- ścieżki `respect/retreat`, `restore/return`, `negotiation/help`, opcjonalnie `combat` tylko gdy karta encounteru to uzasadnia;
- evidence/journal zaczynający od „nieznanej istoty leśnej”;
- persistence zapisujące typ rozwiązania, nie tylko alive/dead.

`ENTITY_FOREST_MISDIRECTION` pozostaje osobnym slotem/mechanizmem. Ten lock nie scala obu ID.

## Guardrails art/lore

- nie kopiować współczesnego popkulturowego „leszego” jako domyślnej sylwetki;
- poroże, czaszka, korzenie lub „tree-man” nie są automatycznie historycznym atrybutem;
- nie przedstawiać mieszkańców R2 jako posiadających jednolitą, ogólnosłowiańską doktrynę o guardianie;
- lokalna nazwa F musi być oznaczona w dokumentacji jako fikcja;
- późniejszy research może dodać wariant regionalny, ale nie zmienia tego locku bez nowego evidence review.

## Źródła i provenance

Ownerami wewnętrznymi pozostają `ForestSpirit.md` (B02) i `RegionalForestFiguresPL.md` (B02/B10/B11). Dodatkowy sanity check literatury encyklopedycznej potwierdza rosyjskiego Leshii/Lesovik jako forest spirit, ale nie dostarcza podstawy do przeniesienia tej etykiety na R2. Z tego powodu wynik locku pozostaje F.

## Wynik

**IDENTITY LOCK: CLOSED / F.**

Nie potrzeba dalszego identity researchu, aby implementować `ENTITY_FOREST_GUARDIAN_F`. Dalszy research jest wymagany tylko wtedy, gdy projekt zechce zastąpić fikcyjną tożsamość konkretną historyczną/regionalną nazwą.