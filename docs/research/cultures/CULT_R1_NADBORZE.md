# CULT_R1_NADBORZE — evidence package v0.1

Status: **implementation-ready / research-open**. Owner: `CULT_R1_NADBORZE`. Policy owner: `CultureResearchFramework.md`. Baseline: `CULT_R0_ZARNOWIEC.md`.

## Scope

Pakiet opisuje wyłącznie różnice i dodatkowe potrzeby produkcyjne R1 względem bazowego material-culture R0. Nadborze jest fikcyjnym kontekstem gry; nazwy, Związek Grodów, Dębrzyn, Zjazd Grodów, konflikty polityczne i konkretne prawo są `F`, o ile osobny claim nie ma źródła. Transfer rodzin material-culture z R0 jest rekonstrukcją `R`, nie dowodem jednej historycznej kultury obejmującej oba regiony.

## Evidence / decision ledger

| claim_id | klasa | claim produkcyjny | evidence / owner | confidence / ograniczenie |
|---|---|---|---|---|
| `R1_BASE_01` | R | Bazowe rodziny budownictwa, tekstyliów, narzędzi, żywności, transportu i broni mogą korzystać z R0 jako punktu startowego tylko tam, gdzie research card dopuszcza taki wariant. | `CULT_R0_ZARNOWIEC.md` + `../material-culture/*` | medium; każdy finalny asset wymaga region-fit |
| `R1_SETTLE_01` | R | R1 potrzebuje większej skali osadniczej i zróżnicowania funkcji obiektów; forma konkretnego grodu/podgrodzia nie może wynikać wyłącznie z gameplayowej etykiety „gród”. | `../material-culture/Architecture.md`; `SettlementCrafts.md` | medium; final fortification locator otwarty |
| `R1_RIVER_01` | R | Infrastruktura rzeczna, transport i magazynowanie muszą używać wariantów wspartych research zamiast generycznego zestawu „medieval river”. | `../material-culture/Transport.md`; `TradeEconomy.md` | medium |
| `R1_MILL_01` | R | Osada młynarska wymaga osobnego technology/chronology check przed lockiem konkretnego modelu młyna i mechanizmu. | `RegionBibleR1Nadborze.md`; material-culture owner | open research lock; nie traktować obecnego placeholdera jako H |
| `R1_TOLL_01` | F | Cła, kontrola przepraw i administracja danin są mechaniką/worldbuildingiem R1; ich dokładna procedura nie jest deklarowana jako rekonstrukcja historyczna. | `RegionBibleR1Nadborze.md`; `PoliticalConflicts.md`; economy owners | explicit F |
| `R1_POLITY_01` | F | Związek Grodów, Zjazd Grodów i naczelnik naczelników są fikcyjną strukturą polityczną. Research może ograniczać materialną prezentację elit/administracji, ale nie „udowadnia” tej hierarchii. | world/political owners | explicit F |
| `R1_MARKET_01` | R | Większy hub handlowy może używać szerszego zestawu kategorii dóbr niż R0 tylko po sprawdzeniu trade/economy basis; stock i ceny pozostają gameplay tuning. | `../material-culture/TradeEconomy.md`; `world/InterregionalEconomy.md` | medium |
| `R1_CULT_01` | F/R | Współczesne miejsca kultowe są częścią fikcyjnego świata; materialne elementy mogą dostać historyczne/reconstructed basis osobno. Stare miejsca sieci nie są automatycznie świątyniami. | pantheon research + region bible | mixed; wymaga osobnych locatorów |
| `R1_WORLD_01` | F | Dębrza, Dębrzyn, konkretne grody, rody, NPC, questy i konflikt informacyjny są elementami świata gry. | region/world/story owners | explicit F |

## Production consequences

- **Grody i podgrodzia:** projektować jako modularne rodziny funkcjonalne. Palisada, brama, wał, zabudowa i zaplecze gospodarcze nie tworzą jednego uniwersalnego historycznego zestawu bez locatorów.
- **Rzeka:** przystanie, łodzie, magazyny i transport mają wskazywać `Transport.md` / `TradeEconomy.md`; finalny model łodzi wymaga dokładnego basis.
- **Przeprawy i cła:** gameplay może już implementować punkty kontroli, state i ekonomiczne konsekwencje. Props/procedury przedstawiane jako historyczne wymagają osobnego claimu.
- **Młynarstwo:** system questowy i POI mogą istnieć teraz, lecz art lock konkretnej technologii młyna pozostaje otwarty do chronology/region-fit pass.
- **Hierarchia:** role administracyjne są `F`; nie wyprowadzać kostiumu/statusu z pojedynczego znaleziska, typu broni lub pochówku.
- **Religia:** zachować rozdział między materialnym evidence, interpretacją i fikcyjnym kultem. Stare miejsca sieci należą do central lore, nie do automatycznej rekonstrukcji religii.

## Asset/content consequences

Pakiet odblokowuje implementacyjny first pass dla: R1 settlement kit variants, większego hubu, infrastructure props, river/trade families, kontrolowanych przepraw, magazynów i wariantów warsztatowych. Reuse R0 jest preferowany tam, gdzie nie sugeruje fałszywej jednolitości. Finalne fortifications, mill technology, boat variants, elite/admin appearance, ornament i cult props pozostają za research/art lockami.

## Open locks

| lock | owner | warunek zamknięcia |
|---|---|---|
| `R1_FORTIFICATION_LOCK` | research + art | exact locators i region/chronology fit dla finalnych wałów, palisad, bram i zabudowy grodowej |
| `R1_RIVERCRAFT_LOCK` | research + art | zatwierdzone warianty łodzi/przystani/transportu z provenance |
| `R1_MILL_TECH_LOCK` | research + art | chronology + technology locator dla finalnego młyna/mechanizmu |
| `R1_ADMIN_APPEARANCE_LOCK` | research + art | status/appearance bez pseudo-historycznej „uniformizacji” fikcyjnej administracji |
| `R1_RELIGION_MATERIAL_LOCK` | pantheon research | oddzielone evidence materialne, interpretacja i F worldbuilding |
| `R1_NAMING_LOCK` | language/naming | osobny pass nazw i ograniczeń językowych |

Żaden lock nie blokuje quest state machines, politycznego gameplayu, ekonomii data-driven, streamingu ani implementacji modularnych asset families.

## QA / review checklist

- Każdy asset oznaczony jako historycznie ugruntowany ma claim i locator przed final art lockiem.
- `F` polityka/prawo nie jest przedstawiana w dokumentacji jako fakt historyczny.
- Transfer R0 → R1 jest oznaczony `R` i przechodzi region-fit.
- Młyn, łódź, fortyfikacja i administracyjny appearance nie są finalizowane z placeholdera.
- Quest/reveal dotyczący sieci nie używa research package jako źródła central lore.

Ostatni przegląd package: 2026-10-03. Następny package: `CULT_R2_WIELKI_BOR`, z naciskiem na osadnictwo skrajne/leśne, użytkowanie zasobów i rozdzielenie fikcyjnej tożsamości `forest-guardian` od material-culture research.