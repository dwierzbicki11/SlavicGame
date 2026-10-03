# CULT_R0_ZARNOWIEC — evidence package v0.1

Status: **implementation-ready / research-open**. Owner: `CULT_R0_ZARNOWIEC` evidence package. Policy owner: `CultureResearchFramework.md`.

## Scope

Kontekst obejmuje R0 Pogranicze Żarnowca i służy jako bazowy pakiet material-culture dla późniejszych różnic regionalnych R1–R4. Nie dowodzi jednej historycznej „kultury Żarnowca”: Żarnowiec jest fikcyjnym kontekstem gry. Inspiracja produkcyjna ma być zgodna z materiałem właściwym dla wczesnego średniowiecza, ale każde przeniesienie danych spoza właściwego czasu/obszaru pozostaje rekonstrukcją `R`.

## Existing F decisions

Decyzje świata zapisane w region bible, `world/Cultures.md`, `world/MacroCultures.md`, questach i rosterach pozostają `F`. Ten package nie awansuje fikcyjnych nazw, granic, rodów, konkretnych NPC, kultów ani wydarzeń do statusu historycznego.

## Evidence ledger

| claim_id | klasa | claim produkcyjny | evidence locator | confidence / ograniczenie |
|---|---|---|---|---|
| `R0_MC_ARCH_01` | R | Budownictwo R0 ma korzystać z rodzin konstrukcji opisanych w bazowym researchu, bez jednej „domyślnej chaty słowiańskiej”. | `../material-culture/Architecture.md`; źródła i locatory w `MaterialCultureSources.md` | medium; wariant zależy od miejsca i funkcji |
| `R0_MC_SETTLE_01` | R | Props osadnicze i warsztatowe mogą korzystać z rodzin rzemiosł udokumentowanych w researchu; zestaw konkretnej osady wynika z jej funkcji. | `../material-culture/SettlementCrafts.md`; `ProductionImplications.md` | medium |
| `R0_MC_TEXTILE_01` | R | Ubiór/NPC kit nie może używać jednego kompletnego „stroju narodowego”; elementy i technologie dobieramy osobno. | `../material-culture/ClothingTextiles.md`; `MaterialCultureSources.md` | medium; appearance lock pozostaje art/research decision |
| `R0_MC_FOOD_01` | R | Food/household props korzystają z udokumentowanych kategorii gospodarki żywnościowej, nie z nowoczesnego folkloru kulinarnego. | `../material-culture/FoodSubsistence.md` | medium |
| `R0_MC_AGRI_01` | R | Narzędzia rolnicze mogą wejść do asset family tylko w wariantach wspartych research card. | `../material-culture/AgricultureTools.md` | medium |
| `R0_MC_TRADE_01` | R | Handel i usługi R0 mogą wykorzystywać kategorie dóbr i wymiany z researchu; konkretne ceny i vendor stock są gameplay `F`/tuning. | `../material-culture/TradeEconomy.md`; `ProductionImplications.md` | medium |
| `R0_MC_TRANS_01` | R | Transport lądowy/wodny wymaga wariantu zgodnego z research, a nie generycznego „medieval vehicle”. | `../material-culture/Transport.md` | medium |
| `R0_MC_WEAPON_01` | R | Broń i wyposażenie bojowe są ograniczone do rodzin wspartych research; statystyki pozostają gameplay `F`. | `../material-culture/Weapons.md` | medium |
| `R0_MC_DEATH_01` | R | Burial/death imagery może korzystać z udokumentowanych praktyk tylko z zachowaniem rozdziału danych od interpretacji religijnej. | `../material-culture/Burials.md`; `MaterialCultureSources.md` | medium; religijne znaczenie nie jest automatycznie H |
| `R0_WORLD_01` | F | Żarnowiec, jego społeczności, NPC, lokalne konflikty i questy są fikcyjne. | region/world/story owners | explicit F |

`MaterialCultureSources.md` pozostaje bibliografią bazową. Przed finalnym `production-ready` każdy claim użyty do locku konkretnego modelu/materialu/costume ma otrzymać dokładny locator do publikacji/katalogu zgodnie z frameworkiem; samo odwołanie do research card wystarcza do implementacyjnego first pass, nie do finalnego history claim.

## Material culture consequences

- **Architecture:** kit modularny; funkcja budynku i lokalny kontekst wybierają wariant. Zakaz jednego uniwersalnego zestawu dla wszystkich osad.
- **Clothing/textiles:** system warstw/elementów i wariantów; nie projektować kompletów jako rekonstrukcji bez locatorów.
- **Food/subsistence:** props i loot categories wynikają z `FoodSubsistence.md`; recipes gry mogą być `F`, jeśli są tak oznaczone.
- **Tools/crafts:** narzędzia rolnicze i warsztatowe linkują do odpowiednich research cards.
- **Trade:** źródła ograniczają rodziny dóbr; ceny, rarity i restock są systemowym tuningiem.
- **Transport:** każdy finalny model ma wskazać research basis i region-fit.
- **Weapons:** visual family wymaga research basis; combat values nie są twierdzeniem historycznym.

## Social / legal / economic constraints

Questowe role, lokalne prawo, cła, reputacja i konkretne relacje społeczne pozostają `F`, dopóki osobny claim nie ma źródła. Nie wyprowadzamy struktury społecznej z pojedynczego typu grobu, ozdoby ani broni. R0 może być bazą gameplayową dla R1–R4, ale różnice regionalne muszą być zapisane w ich własnych packages.

## Religion / death

`Burials.md` może wspierać materialną stronę pochówku. Znaczenie rytuału, przypisanie go konkretnemu bóstwu i kosmologiczne wyjaśnienia należą do pantheon/world lore i pozostają `F` lub osobnym research claim. Nie używać znaleziska grobowego jako samodzielnego dowodu etniczności lub wyznania.

## Naming / language constraints

Ten package nie tworzy conlangu i nie historycyzuje fikcyjnych nazw. Finalne imiona/toponimy muszą przejść osobny naming/language pass; do tego czasu nazwy produkcyjne mogą pozostać `F`. Nie rekonstruować dialektu z nowoczesnych form ludowych bez jawnej metodologii.

## Asset/content consequences

Package odblokowuje implementacyjny first pass dla rodzin R0: settlement/building kits, household/agriculture/craft props, bazowych textile layers, food/trade props, transport i weapon visual families. Konkretne finalne modele, ornament, kolory, pełne kostiumy i warianty jakościowe pozostają za art/research lockiem, jeśli nie mają dokładnego locatora.

## Open locks

| lock | owner | warunek zamknięcia |
|---|---|---|
| `R0_LOCATORS_FINAL` | research | dokładne page/figure/catalog locators dla claimów użytych przez finalne assety |
| `R0_COSTUME_LOCK` | research + art | zatwierdzony zestaw elementów/warstw z proweniencją; bez „stroju narodowego” |
| `R0_ORNAMENT_LOCK` | research + art | każdy finalny motyw ma evidence albo jawny status F |
| `R0_RELIGION_MATERIAL_LOCK` | pantheon research | oddzielone dane materialne, interpretacja i worldbuilding F |
| `R0_NAMING_LOCK` | language/naming | osobny pass nazw i ograniczeń językowych |

Żaden z tych locków nie blokuje implementacji data modelu, modularnych asset families, quest state machines ani regionalnego content pipeline.

## Sources / research owners

Źródła bibliograficzne: `../material-culture/MaterialCultureSources.md`. Research cards: `Architecture.md`, `SettlementCrafts.md`, `ClothingTextiles.md`, `FoodSubsistence.md`, `AgricultureTools.md`, `TradeEconomy.md`, `Transport.md`, `Weapons.md`, `Burials.md`; konsekwencje produkcyjne: `ProductionImplications.md`.

Ostatni przegląd package: 2026-10-03. Następny package: `CULT_R1_NADBORZE`, zapisujący różnice i potrzeby produkcyjne względem tej bazy zamiast kopiowania R0.