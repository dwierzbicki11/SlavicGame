# Regional Vendors / Services — R0–R6

Status: production implementation pass v0.1. Owner regionalnego rozmieszczenia usług; `EconomyPass01.md` pozostaje ownerem zasad ekonomii, a roster NPC ownerem konkretnych postaci. Dokument nie ustala finalnych cen, nominałów ani stock counts przed balansem.

## Kontrakt runtime

Każdy punkt handlu/usługi ma stabilne `serviceId`, `regionId`, `serviceFamily`, opcjonalny `npcSlotId`, `availabilityConditions`, `stockProfileId`, `reputationGate`, `questStateGate` i `persistencePolicy`. Stock jest profilem danych, nie listą zaszytą w NPC. Quest item nie może zostać sprzedany ani zużyty, jeśli tworzyłoby to softlock. Usługa krytyczna dla main questa musi mieć fallback: drugi provider, world interaction albo fail-forward quest path.

Dozwolone rodziny: `HERBALIST`, `HEALER`, `WORKSHOP`, `TRADER`, `FOOD_LODGING`, `INFORMATION`, `TRANSPORT`, `RITUAL_SERVICE`, `SALVAGE_EXCHANGE`. `RITUAL_SERVICE` nie oznacza automatycznie historycznej instytucji — presentation podlega odpowiedniemu lore/research lockowi.

## R0 — Żarnowiec

- `SVC_R0_HERBALIST`: składniki, podstawowe consumables, odblokowania receptur przez stan questa/reputację.
- `SVC_R0_WORKSHOP`: podstawowa konserwacja/naprawa ekwipunku; ulepszenia tylko jeśli system equipment je odblokuje.
- `SVC_R0_TRADER`: podstawowe towary podróżne i skup bezpiecznych materiałów.
- `SVC_R0_FOOD_LODGING`: odpoczynek/żywność jako convenience, nigdy obowiązkowy paywall kampanii.

R0 jest bazowym profilem usług i fallbackiem onboardingowym.

## R1 — Nadborze

- `SVC_R1_RIVER_TRADER`: towary regionalne i wymiana związana z ruchem rzecznym.
- `SVC_R1_WORKSHOP`: naprawy/narzędzia związane z osadnictwem, transportem i warsztatami.
- `SVC_R1_INFORMATION`: legalne/lokalne informacje o przeprawach, cłach i trasach; quest clues nie mogą zależeć wyłącznie od zakupu.
- `SVC_R1_TRANSPORT`: przeprawa/transport rzeczny po spełnieniu world-state gate; zawsze istnieje questowy fallback, jeśli transport jest wymagany.

## R2 — Wielki Bór

R2 celowo ma słabszą sieć handlową niż huby.

- `SVC_R2_FOREST_SUPPLY`: ograniczone zapasy podróżne/łowieckie w bezpiecznych punktach lub obozach.
- `SVC_R2_HERBALIST`: regionalne składniki tylko tam, gdzie provider ma sens diegetyczny.
- `SVC_R2_INFORMATION`: wskazówki o szlakach i zagrożeniach; nie sprzedaje automatycznego rozwiązania trackingu ani identity guardiana.

Brak obowiązkowego pełnego workshopu w głębokim lesie; gracz ma przygotować się wcześniej lub wrócić do bezpiecznego punktu.

## R3 — Przymorze

- `SVC_R3_PORT_TRADER`: towary portowe/regionalne, barter i bezpieczny skup salvage.
- `SVC_R3_FISHER_SUPPLY`: żywność i materiały użytkowe związane z wybrzeżem.
- `SVC_R3_SALVAGE_EXCHANGE`: skup wyłącznie przedmiotów oznaczonych jako sellable salvage; dowody/quest items są wykluczone.
- `SVC_R3_TRANSPORT`: kontrolowane podróże wodne zależne od pogody i quest/world state.
- `SVC_R3_INFORMATION`: informacje portowe/szlaki/plotki z jawnie określonym reliability tier w dialog data.

## R4 — Kamienne Wyżyny

- `SVC_R4_WORKSHOP`: naprawa i narzędzia; konkretne technologie pozostają material-culture lockiem.
- `SVC_R4_RESOURCE_TRADER`: handel dozwolonymi surowcami i zaopatrzeniem podróżnym.
- `SVC_R4_FOOD_LODGING`: punkt odpoczynku przy osadzie/szlaku.
- `SVC_R4_INFORMATION`: informacje o przełęczach, worksite i zagrożeniach; traversal-critical info ma darmowy fallback.

Nie tworzyć przemysłowego sklepu/kopalnianej gospodarki przez anachroniczny skrót artystyczny.

## R5 — Równiny Arel

- `SVC_R5_ROUTE_TRADER`: handel przy trasach i stałych osadach.
- `SVC_R5_CAMP_EXCHANGE`: wymiana zależna od sezonowego stanu obozu; nie definiuje etniczności ani historycznej gospodarki bez culture locku.
- `SVC_R5_TRAVEL_SUPPLY`: zaopatrzenie do podróży i podstawowe consumables.
- `SVC_R5_INFORMATION`: informacje o wodzie, trasach i landmarkach; old-site clue pozostaje quest/evidence data, nie płatnym hard gate.

Nazwy, wygląd, towary signature i formy barteru podlegają `CULT_R5_AREL`.

## R6 — Pustkowie Pierwszego Progu

R6 nie jest normalnym regionem gospodarczym. Domyślnie brak stałego pełnego marketu.

- `SVC_R6_EXPEDITION_SUPPLY`: ograniczony punkt zaopatrzenia zależny od stanu kampanii, jeśli staging area nadal działa.
- `SVC_R6_FIELD_SUPPORT`: questowy support/repair/recovery z istniejących zasobów ekspedycji; nie generuje nieskończonego stocku.

Brak losowych merchantów w strefie finałowej bez osobnego uzasadnienia fabularnego.

## Stock profiles i ekonomia

Profile stocku odwołują się do przyszłego pełnego item catalogu. Minimum: `STOCK_BASIC_TRAVEL`, `STOCK_HERBAL`, `STOCK_WORKSHOP`, `STOCK_REGIONAL_TRADE`, `STOCK_FOOD`, `STOCK_SALVAGE_BUY`. Profil definiuje kategorie, nie finalną liczbę sztuk/cenę. Scarcity, route state, reputation i quest state mogą modyfikować dostępność; finalne mnożniki są playtest lockiem.

## Persistence / fail-forward

- jednorazowe quest unlocks zapisują stan;
- merchant nie resetuje quest-limited reward przez reload;
- sezonowy/mobilny provider zapisuje logiczny state, nie przypadkową pozycję;
- śmierć/zniknięcie NPC nie może trwale zablokować main questa — usługa krytyczna przechodzi na fallback;
- transport zapisuje origin/destination/world state przed zmianą regionu;
- buy/sell transaction jest atomowa względem save.

## Minimalne QA

1. każdy `serviceId` jest unikalny i mapuje się na istniejący region;
2. critical path działa przy niedostępnym providerze;
3. quest/evidence items nie pojawiają się w sellable pool;
4. reload nie duplikuje nagród, stocku limitowanego ani pieniędzy;
5. reputation/quest gates deterministycznie odtwarzają dostępność;
6. R2 i R6 zachowują zamierzoną ograniczoną dostępność usług;
7. culture/research-locked presentation nie zostaje przypadkowo uznana za historical-final;
8. craft-buy-sell loop jest testowany po podpięciu finalnych cen/receptur.

## Otwarte locki

- finalne ceny, waluta/nominały, marże, restock timing i ilości;
- konkretni providerzy tam, gdzie roster pozostawia slot;
- finalne signature goods per region po item/research locku;
- appearance/naming/dialogue/VO;
- tuning scarcity/reputation/route modifiers;
- ekonomiczny balans craft/sell/repair po pełnym recipe catalogu.

## Definition of Ready

Każdy region ma jawny minimalny service footprint i zasady fallback/persistence. Można implementować registry usług, UI vendorów, availability gates i stock profiles bez ustalania niezmierzonych cen oraz bez tworzenia nowych NPC tylko po to, by wypełnić tabelę.