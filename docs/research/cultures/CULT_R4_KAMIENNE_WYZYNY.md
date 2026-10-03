# CULT_R4_KAMIENNE_WYZYNY — evidence package v0.1

Status: implementation-ready research mapping; **nie** production research lock. Owner: regional culture evidence dla R4. Dokument nie zastępuje `RegionBibleR4KamienneWyzyny.md`, economy/traversal specs, material-culture research ani ownerów geologii i technologii.

## Scope

Pakiet obejmuje Wysokamień, Żelazną Bramę, dolne stoki, osady i zaplecze wydobycia, aktywne/zamknięte wyrobiska oraz społeczności zależne od wydobycia, obróbki i transportu surowców. Chronologia inspiracji pozostaje IX–X w. R4 nie jest historycznym odpowiednikiem jednej społeczności górniczej; jego układ ekonomiczny i konflikt są fikcją gry.

## Existing F decisions

- Wysokamień jako hub usługowy R4;
- Żelazna Brama jako kontrolowany węzeł logistyczny;
- aktywne, zamknięte i starsze strefy podziemne jako struktura gameplayu;
- łańcuch `wydobycie -> sortowanie/skład -> transport -> warsztat/handel` jako model systemowy;
- zależność części lokalnej gospodarki od surowców;
- współczesne wyrobisko przecinające starszą strukturę jako centralny beat kampanii;
- górskie miejsca kultowe jako element żywego regionu, bez automatycznego utożsamienia ich ze starą siecią.

## Evidence ledger

| claim_id | klasa | claim / ograniczenie | basis | confidence / transfer |
|---|---|---|---|---|
| `R4-F-01` | F | hub, checkpoint, kopalnie i łańcuch zasobów są produkcyjną strukturą regionu | R4 bible | high jako canon gry; nie history claim |
| `R4-R-01` | R | zwykła zabudowa, pojemniki, tekstylia i podstawowe warsztaty mogą startować od wcześniejszego material-culture baseline | `research/material-culture/*`, `CULT_R0`–`R3` | medium; finalny wygląd wymaga region-fit |
| `R4-R-02` | R | gameplay wymaga rozróżnialnych rodzin propsów dla wydobycia, składowania, transportu i obróbki | R4 bible + asset catalog | high jako potrzeba produkcyjna; forma historyczna pozostaje U/R |
| `R4-U-01` | U | finalne techniki wydobycia, ślady narzędzi, obudowa wyrobisk i organizacja pracy | mining/material-culture research | open; wymagane źródła, locatory i time/region fit |
| `R4-U-02` | U | surowce/geologia regionu oraz historycznie wiarygodne metody rozpoznania, sortowania i wstępnej obróbki | geology + archaeometallurgy owner | open; nie wyprowadzać z nazwy „Żelazna Brama” |
| `R4-U-03` | U | transport urobku i materiałów po stokach, drogach i w wyrobiskach | transport/material-culture research | open |
| `R4-U-04` | U | metalurgia, kamieniarstwo i inne warsztaty zależne od lokalnych surowców | craft/archaeometallurgy research | open; konkretne procesy i piece wymagają locatorów |
| `R4-U-05` | U | costume, ornament, appearance i regionalne wyróżniki mieszkańców | culture/art owners | open; nie tworzyć „góralskiego” stereotypu ad hoc |
| `R4-U-06` | U | materialna strona górskich miejsc kultowych | pantheon/religion research | open; miejsce kultowe nie jest automatycznie kopalnią ani starą strukturą |
| `R4-U-07` | U | naming, terminologia pracy, status społeczny pracowników i organizacja kontroli surowców | naming/social research | open; role gameplayowe mogą pozostać F |

## Material culture consequences

### Kopalnie i praca

Można implementować modularne wejścia, korytarze, strefy robocze, składy i punkty bezpieczeństwa jako wymienialne rodziny. Finalne profile wyrobisk, podpory, narzędzia, ślady pracy, oświetlenie robocze i procedury nie mogą być oznaczone jako rekonstrukcja bez zamknięcia `R4-U-01`. Starsza struktura używa osobnego języka assetów i nie jest źródłem dowodu dla historycznej techniki górniczej.

### Geologia i surowce

Region może systemowo produkować kategorie `stone`, `ore`, `metal-input` i questowe próbki przez stabilne ID. Konkretne minerały, złoża, relacje geologiczne oraz pochodzenie materiału dawnych progów muszą być zamrożone przez lore/geology ownera. Nazwa Żelaznej Bramy nie dowodzi wydobycia żelaza.

### Transport i logistyka

Gameplay może mieć punkty załadunku, składy, blokady tras, przewoźników i state changes wpływające na podaż. Finalne wozy, sanie, nosidła, liny, urządzenia wyciągowe i organizacja transportu pozostają wymienialne do `R4-U-03`. Nie kopiować technologii z późniejszych kopalń tylko dlatego, że czytelnie komunikuje „górnictwo”.

### Rzemiosło

Rzemieślnik zależny od surowców jest zatwierdzoną rolą NPC. Konkretne piece, paleniska, kowadła, narzędzia kamieniarskie, etapy wzbogacania rudy i metalurgii wymagają claim-level evidence. Economy owner zachowuje ceny, yield i recipe balance.

## Social and visual constraints

Zależność osad od wydobycia jest F i nie oznacza historycznej klasy zawodowej o ustalonej nazwie, stroju czy prawach. R4 nie może być wyróżniane przez późny folklor „góralski”, przypadkowe futra, kilofy z epoki przemysłowej ani jednolity kostium zawodowy. Wyróżniki wizualne powinny wynikać z zatwierdzonych materiałów, warunków pracy i art direction po research locku.

## Religion / old structure

Górskie miejsca kultowe korzystają z zatwierdzonego panteonu i osobnego religion-material researchu. Nie zakładamy kultu kopalni, metalu, góry ani „duchów gór” bez ownera. Stara struktura jest centralnym lore F; jej materiały, geometria i anomalie nie są źródłem historycznym.

## Asset/content consequences

Pakiet odblokowuje implementacyjnie:

- modularne mine/worksite/storage/route families z wymienialnymi detalami;
- data slots dla kategorii surowców niezależne od finalnej geologii;
- resource-chain hooks dla economy, questów i world state;
- generic worker/transport/craft role IDs bez wymyślonej historycznej hierarchii;
- wizualne rozdzielenie współczesnego wyrobiska od starej struktury;
- prototyp traversal i worksite encounters bez zamrażania finalnej techniki.

Nie odblokowuje finalnych narzędzi, obudowy kopalń, urządzeń transportowych, procesów metalurgicznych, costume/ornamentu, kultowych propsów ani geologicznego claimu o konkretnych złożach.

## Open locks

1. `R4-U-01` — techniki/narzędzia/organizacja wydobycia; owner: mining/material-culture research.
2. `R4-U-02` — geologia, surowce i rozpoznanie materiału; owner: geology/archaeometallurgy + lore.
3. `R4-U-03` — transport urobku i logistyka; owner: transport/material-culture research.
4. `R4-U-04` — metalurgia/kamieniarstwo/warsztaty; owner: craft research.
5. `R4-U-05` — costume/ornament/appearance; owner: culture/art.
6. `R4-U-06` — materialna strona kultów; owner: pantheon/religion research.
7. `R4-U-07` — naming/terminologia/status pracy; owner: naming/social research.

Locki nie blokują content pipeline ani quest state machines; blokują oznaczenie zależnych assetów, technologii i lore jako finalnych.

## Sources / provenance

v0.1 mapuje istniejące owner specs: `RegionBibleR4KamienneWyzyny.md`, `research/material-culture/*`, wcześniejsze `CULT_*`, economy/traversal specs i centralną source policy. Pakiet celowo nie udaje nowej bibliografii górniczej. Zamknięcie `R4-U-*` wymaga źródeł z locatorami przypiętymi do konkretnych claimów, z kontrolą chronologii i region-fit; analogie z późniejszego górnictwa muszą być jawnie oznaczone jako rekonstrukcja, nie dowód.

Ostatni przegląd: 2026-10-03.
