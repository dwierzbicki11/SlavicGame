# CULT_R3_PRZYMORZE — evidence package v0.1

Status: implementation-ready research mapping; **nie** production research lock. Owner: regional culture evidence dla R3. Dokument nie zastępuje `RegionBibleR3Przymorze.md`, economy/vendor specs, research material culture ani naming/language ownerów.

## Scope

Kontekst obejmuje Solny Bród, mniejsze porty, wybrzeże, ujście Dębrzy i społeczności obsługujące rybołówstwo, transport, handel oraz krótką podróż wodną. Chronologia inspiracji pozostaje zgodna z ogólnym zakresem projektu IX–X w. Pakiet nie dowodzi jednej historycznej „kultury portowej”, nie przypisuje fikcyjnym mieszkańcom konkretnej etniczności i nie traktuje obecności przyjezdnych jako licencji na losowe mieszanie historycznych motywów.

## Existing F decisions

Poniższe elementy są decyzjami świata gry (**F**), nie twierdzeniami historycznymi:

- Solny Bród jako główny hub portowo-handlowy R3;
- nabrzeża, targ/składy, kontrola ruchu i mniejsze porty jako funkcje produkcyjne regionu;
- handel solą, rybami, produktami wybrzeża i towarami spławianymi Dębrzą;
- wielokulturowy roster NPC jako funkcja narracyjna regionu;
- stare mapy kupieckie i ślady dawnych tras jako evidence centralnego lore;
- ograniczona podróż łodzią bez wymogu pełnej symulacji żeglugi;
- przemyt, blokady tras i kryzys handlu jako rodziny encounterów/questów.

## Evidence ledger

| claim_id | klasa | claim / ograniczenie | basis | confidence / transfer |
|---|---|---|---|---|
| `R3-F-01` | F | Solny Bród, mniejsze porty i wielokulturowość są strukturą produkcyjną regionu | R3 bible | high jako canon gry; nie history claim |
| `R3-R-01` | R | zwykłe rodziny budownictwa, ubioru i pojemników mogą startować od material-culture baseline R0–R2 | `research/material-culture/*`, wcześniejsze `CULT_*` | medium; reuse techniczny, finalny wygląd wymaga region-fit |
| `R3-R-02` | R | port wymaga rodzin assetów dla magazynowania, przeładunku, lin, sieci, pojemników, napraw i kontroli ruchu | R3 bible + material-culture baseline | medium; potrzeba gameplayowa jest F, historyczna forma U/R do locku |
| `R3-U-01` | U | finalne typy małych jednostek, konstrukcja nabrzeży i wyposażenie łodzi | material-culture/maritime research owner | open; wymagane locatory i time/region fit |
| `R3-U-02` | U | narzędzia, techniki i sezonowość rybołówstwa oraz przetwarzania połowu | material-culture/food research owner | open |
| `R3-U-03` | U | historycznie wiarygodne formy pakowania, przeładunku, magazynowania i salvage | material-culture/trade research owner | open |
| `R3-U-04` | U | costume/ornament/appearance mieszkańców i przyjezdnych | culture/art owners | open; nie tworzyć „portowego miksu” z przypadkowych kultur |
| `R3-U-05` | U | języki, rejestry mowy, pochodzenie i naming przyjezdnych | language/naming/culture owners | open; wielojęzyczność jest F, konkretne języki nie |
| `R3-U-06` | U | materialna strona kultów związanych z wodą, podróżą i handlem | pantheon/religion research owner | open; funkcja narracyjna nie dowodzi praktyki historycznej |
| `R3-U-07` | U | prawo portowe, cła, własność wraków/salvage i status kupców | social/legal research owner | open; questowe reguły mogą pozostać jawnie F |

## Material culture consequences

### Port i zabudowa

Do implementacji można reużywać modularny baseline zwykłej zabudowy, lecz port wymaga wymienialnych rodzin nabrzeży, magazynów, punktów przeładunku i małych warsztatów. Finalne konstrukcje, łączenia, urządzenia portowe i ich skala pozostają zablokowane do `R3-U-01`/`R3-U-03`. Nie projektujemy pełnego miasta portowego na podstawie późnośredniowiecznych lub nowożytnych ikonografii bez oznaczenia rekonstrukcji.

### Łodzie i transport wodny

Runtime może już implementować boarding points, kontrolowane trasy, blokady pogodowe i fallback lądowy. Modele jednostek, olinowanie, wyposażenie, naprawy i animacje pracy muszą pozostać wymienialne do maritime locku. Obecność handlu dalekiego zasięgu w fiction nie dowodzi konkretnego typu statku.

### Rybołówstwo i żywność

Gameplay może używać połowu, konserwacji i handlu rybami jako regionalnego scope. Finalne sieci, haczyki, pułapki, łodzie robocze, techniki obróbki oraz sezonowość wymagają locatorów. Generic props mogą być prototypowane, ale nie oznaczane jako final reconstruction.

### Handel, magazynowanie i salvage

Skrzynie, beczki, liny, worki i towary są rodzinami produkcyjnymi. Konkretne formy opakowań, miary, zabezpieczenia ładunku, praktyki przeładunku i zasady własności wraku pozostają `U`, dopóki nie mają research ownera. Questy mogą używać fikcyjnych reguł portu, jeśli UI/dialog jasno nie przedstawia ich jako historycznej normy.

## Social/legal/economic constraints

R3 może przedstawiać zarządcę portu, pobór opłat, kupców, przewoźników, rzemieślników, rybaków i przyjezdnych jako funkcje gameplayowe. Nie przypisujemy im konkretnej historycznej hierarchii, prawa morskiego, gildii ani systemu monetarnego bez źródeł. Economy owner zachowuje ceny, marże i vendor inventory.

## Multiculturality / language constraints

Większa mieszanka pochodzenia niż w regionach śródlądowych jest **F requirement**. Konkretne pochodzenie NPC musi jednak wskazywać zatwierdzony culture package. Nie tworzymy pseudo-etnonimów, akcentów ani „egzotycznego” stroju ad hoc. Do czasu language/naming lock dialog może używać neutralnej polszczyzny lokalizacyjnej oraz technicznych ID; różnice językowe mogą być flagą danych, nie wymyślonym językiem.

## Religion / supernatural

Kulty związane funkcjonalnie z wodą, podróżą i handlem muszą korzystać z zatwierdzonego panteonu. Materialne rekwizyty wymagają religion/material research. Stare mapy, anomalie i sieć dawnych miejsc są centralnym lore (**F**) i nie mogą być przedstawiane jako rekonstrukcja wierzeń historycznych.

## Asset/content consequences

Pakiet odblokowuje implementacyjnie:

- modularny port/hub z wymienialnymi finalnymi detalami;
- generic dock, warehouse, cargo, rope/net i fish-processing families;
- boarding/route/salvage quest data niezależne od finalnego modelu jednostki;
- NPC culture/language slots wskazujące stabilne ID zamiast ad hoc etnonimów;
- economy/encounter hooks dla blokad, niedoborów, przemytu i utraconych ładunków;
- reuse wspólnych rodzin R0–R2 tam, gdzie nie sugeruje to fałszywej jednorodności kulturowej.

Nie odblokowuje finalnych jednostek, costume, ornamentu, języków, prawa portowego, ikonografii kultowej ani historycznych technik połowu.

## Open locks

1. `R3-U-01` — małe jednostki, nabrzeża i wyposażenie; owner: maritime/material-culture research; close: źródła + locatory + time/region fit.
2. `R3-U-02` — rybołówstwo i obróbka połowu; owner: material-culture/food research.
3. `R3-U-03` — magazynowanie, przeładunek, salvage; owner: material-culture/trade research.
4. `R3-U-04` — costume/ornament/appearance; owner: culture/art.
5. `R3-U-05` — języki, pochodzenie i naming; owner: language/naming/culture.
6. `R3-U-06` — materialna strona kultów; owner: pantheon/religion research.
7. `R3-U-07` — prawo/administracja portowa i własność wraku; owner: social/legal research lub jawny F lock.

Żaden z tych locków nie blokuje systemowego content pipeline, quest state machines ani prototypu regionu; blokuje oznaczenie zależnych assetów/lore jako finalnych.

## Sources / provenance

Pakiet v0.1 jest mappingiem istniejących owner specs, nie nową bibliografią. Korzysta z `research/material-culture/*`, wcześniejszych `CULT_*`, `RegionBibleR3Przymorze.md` i centralnej source policy. Pełne bibliograficzne locatory dla finalnych maritime/fishing/trade assetów należy dopisać przy zamykaniu `R3-U-*`, z przypisaniem źródła do konkretnego claimu zamiast kopiowania ogólnej bibliografii.

Ostatni przegląd: 2026-10-03.