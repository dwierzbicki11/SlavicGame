# Culture research framework — v0.1

Owner dokumentu: research policy dla finalnych kontekstów kulturowych. Ten plik nie zastępuje `world/Cultures.md` ani `world/MacroCultures.md`; definiuje sposób, w jaki ich fikcyjne (F) decyzje otrzymują bezpieczną podstawę badawczą przed production lockiem.

## Stabilne context IDs

| ID | Kontekst | Główny region | Status |
|---|---|---|---|
| `CULT_R0_ZARNOWIEC` | Pogranicze Żarnowca | R0 | package wymagany |
| `CULT_R1_NADBORZE` | Nadborze | R1 | package wymagany |
| `CULT_R2_WIELKI_BOR` | Wielki Bór | R2 | package wymagany |
| `CULT_R3_PRZYMORZE` | Przymorze | R3 | package wymagany |
| `CULT_R4_KAMIENNE_WYZYNY` | Kamienne Wyżyny | R4 | package wymagany |
| `CULT_R5_AREL` | Arel | R5 | osobna baza badawcza spoza inspiracji słowiańskiej wymagana |

R6 nie dostaje automatycznie osobnej kultury. Jeżeli content R6 potrzebuje kultury, musi wskazać jeden z powyższych kontekstów, kontekst mieszany albo uzasadnić nowy `CULT_*`.

## Klasy twierdzeń

Każdy finalizowany detal otrzymuje jedną klasę:

- **H** — historycznie poświadczony dla określonego miejsca/czasu; wymaga źródła;
- **R** — rekonstrukcja/wniosek oparty na źródłach; musi jawnie opisać krok inferencji i nie może być przedstawiany jako bezpośrednio poświadczony;
- **F** — fikcyjna decyzja świata; nie może być opisywana jako fakt historyczny;
- **U** — unresolved/research lock; nie wolno na tej podstawie zamykać finalnego assetu, dialogu ani lore.

## Source/evidence policy

1. Preferujemy publikacje archeologiczne, katalogi muzealne z proweniencją, opracowania akademickie i edycje źródeł. Popularne strony mogą służyć do discovery, nie jako jedyna podstawa krytycznego locku.
2. Każda pozycja źródłowa zapisuje autora/instytucję, tytuł, rok, zakres geograficzny, zakres chronologiczny, locator (strona/figura/katalog) i URL/DOI/ISBN, jeśli istnieje.
3. Źródło dotyczące innego czasu lub regionu nie jest automatycznie dowodem dla IX–X w. ani dla całego świata gry. Taki transfer musi być oznaczony `R` i uzasadniony.
4. Brak źródła nie jest dowodem nieistnienia. Zapisujemy `U`, zamiast wypełniać lukę stereotypem.
5. Tekst pisany opisujący obcych, przeciwników lub praktyki religijne wymaga oceny perspektywy autora; nie kopiujemy wartościujących etykiet do worldbuildingu.
6. Pojedynczy artefakt nie definiuje całej kultury. Asset/content lock powinien opierać się na zespole danych adekwatnym do twierdzenia.
7. Dla elementów religijnych, etnicznych i językowych oddzielamy dane materialne od interpretacji tożsamości.
8. `CULT_R5_AREL` nie może być finalizowany przez analogię „stepową” ani przez kopiowanie jednego realnego ludu. Najpierw powstaje jawnie dobrany, wieloźródłowy research basis.

## Minimalny format package

Każdy `CULT_*` package ma sekcje:

1. **Scope** — regiony, czas inspiracji, czego package nie dowodzi.
2. **Existing F decisions** — decyzje już obowiązujące w biblach, bez udawania ich historyczności.
3. **Evidence ledger** — `claim_id`, klasa H/R/F/U, claim, source IDs, locator, confidence, uwagi o transferze czasu/regionu.
4. **Material culture** — osady/budownictwo, ubiór/tekstylia, żywność, narzędzia, transport, rzemiosło, broń tylko w zakresie potrzebnym produkcji.
5. **Social/legal/economic constraints** — tylko to, co jest potrzebne questom/NPC/economy i ma podstawę lub jest oznaczone F.
6. **Religion/death** — osobno dane, interpretacja i F worldbuilding.
7. **Naming/language constraints** — nie tworzy pełnego conlangu; wskazuje dozwolone/zakazane założenia dla `NamingRules.md`.
8. **Asset/content consequences** — konkretne rodziny assetów, dialogów, usług, encounterów i questów, które package odblokowuje.
9. **Open locks** — lista U z ownerem i warunkiem zamknięcia.
10. **Sources** — pełna bibliografia i data ostatniego przeglądu.

## Warunek production lock

Package może otrzymać `production-ready` tylko gdy:

- wszystkie używane w finalnym contencie twierdzenia H/R mają źródła i locatory;
- wszystkie świadome decyzje F są oznaczone i nie są przedstawiane jako rekonstrukcja historyczna;
- każde U ma ownera oraz nie blokuje assetu/contentu deklarowanego jako finalny;
- region bible, questy, NPC roster i regional asset catalog nie są z package sprzeczne;
- wykonano pass przeciw stereotypizacji i nieuprawnionemu transferowi między regionami/epokami.

## Kolejność pracy

1. `CULT_R0_ZARNOWIEC` — wykorzystać istniejący pakiet `research/material-culture/` i zbudować evidence ledger zamiast powtarzać research.
2. `CULT_R1_NADBORZE` — tylko różnice względem R0 oraz dane wymagane przez grody, rzeki, cła i hierarchię.
3. `CULT_R2_WIELKI_BOR` — leśna gospodarka i lokalne wspólnoty bez romantyzowania „ludzi natury”.
4. `CULT_R3_PRZYMORZE` — porty, rybołówstwo, handel i wielojęzyczność.
5. `CULT_R4_KAMIENNE_WYZYNY` — górnictwo, metalurgia, pasterstwo i przełęcze po odpowiednim material-culture research.
6. `CULT_R5_AREL` — osobny research basis; żadnych placeholderów opartych na stereotypie.

## Integracja

`world/Cultures.md` pozostaje ownerem zasad kulturowych, `world/MacroCultures.md` ownerem fikcyjnych makrokultur, region bibles ownerami regionalnego gameplayu, a ten dokument jest ownerem evidence policy. Szczegółowe packages mają linkować do tych dokumentów zamiast kopiować ich treść.