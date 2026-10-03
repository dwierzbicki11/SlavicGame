# Item / Equipment / Recipe Catalog — full scope v0.1

Status: implementation catalog. Owner dla klas danych i stabilnych ID; nie jest tabelą finalnego balansu ani substytutem researchu material culture.

## Zasady

- rekordy używają stabilnych ID; save zapisuje ID i stan, nie indeks w tabeli;
- `quest/evidence` nie może być sprzedane, zużyte ani zniszczone bez jawnej karty questa;
- `historical-final` wymaga locatora zgodnego z `research/material-culture/ProductionAssetResearchLock.md`;
- wartości damage/protection/value/drop-rate/durability pozostają balance lockiem;
- signature goods nie dostają historycznej etykiety bez culture/material-culture researchu;
- jeden rekord może wskazywać regionalne źródła, ale nie jest kopiowany pod innym ID tylko dla regionu.

## Runtime contract

Minimalny `item`:
`id`, `family`, `tags[]`, `stackPolicy`, `questPolicy`, `sourceRegions[]`, `researchState`, `assetRef`, `effectRef?`, `equipmentRef?`, `recipeRefs[]`.

Minimalny `equipment`:
`itemId`, `slot`, `class`, `attackSet?`, `damageType?`, `protectionProfile?`, `mobilityProfile?`, `requirements?`.

Minimalny `recipe`:
`id`, `result`, `ingredients[]`, `station`, `knowledgeGate`, `regionSources[]`, `effectRef?`.

## Stabilny katalog itemów

### Leczenie / survival
- `ITEM_BANDAGE_BASIC` — quick/consumable; leczenie/bleeding zgodnie z status systemem.
- `ITEM_HEALING_HERB_GENERIC` — ingredient; placeholder botaniczny do research locku.
- `ITEM_MARSH_HERB` — R0 wetland ingredient; nie przypisywać finalnego gatunku bez researchu.
- `ITEM_FOREST_RESIN` — R0/R2 ingredient/material.
- `ITEM_MARSH_SIGHT_TONIC` — tracking consumable; efekt istniejącej receptury.
- `ITEM_SPIRIT_PREPARATION` — opcjonalny late-game consumable; aktywny tylko jeśli encounter/content tego wymaga.

### Materiały i gospodarka
- `ITEM_CLOTH_MATERIAL` — shared crafting material.
- `ITEM_WOOD_MATERIAL` — shared material.
- `ITEM_RESIN_MATERIAL` — shared material; może mapować drop/harvest na `ITEM_FOREST_RESIN` po data locku.
- `ITEM_ORE_GENERIC` — R4 resource family placeholder; finalny minerał/geologia za research lockiem.
- `ITEM_STONE_WORKABLE` — R4 material family.
- `ITEM_SALT_TRADE` — R3 trade family; historyczno-regionalny status wymaga odpowiedniego locatora przed final-art lockiem.
- `ITEM_FISH_FOOD_GENERIC` — R3 food/trade family.
- `ITEM_ROUTE_SUPPLY` — R5 neutralna rodzina logistyczna, bez stereotypowego signature good.
- `ITEM_SALVAGE_GENERIC` — R3 wreck/salvage family; konkretny obiekt ma osobny record dopiero po content locku.

### Narzędzia
- `ITEM_TOOL_KNIFE` — shared tool/equipment family.
- `ITEM_TOOL_AXE` — shared tool/equipment family.
- `ITEM_TOOL_HUNTING` — tracking/hunting tool family; finalny model za research/art lockiem.
- `ITEM_TOOL_WORKSHOP` — service/crafting context, nie musi być ekwipowalny.

### Broń
- `ITEM_WEAPON_KNIFE` — MainHand, light melee family.
- `ITEM_WEAPON_AXE` — MainHand, melee family.
- `ITEM_WEAPON_SPEAR` — MainHand, reach family.
- `ITEM_WEAPON_BOW` — Ranged; korzysta z BowCombat.
- `ITEM_AMMO_ARROW` — ammo family; konkretna konstrukcja grotu/drzewca za material-culture lockiem.

### Pancerz / ubiór funkcjonalny
- `ITEM_ARMOR_BODY_LIGHT` — Body, light protection family.
- `ITEM_ARMOR_BODY_MEDIUM` — Body, medium protection family; dostępność zależy od content/research locku.
- `ITEM_ARMOR_HEAD_BASIC` — Head protection family.
- `ITEM_CLOTHING_TRAVEL` — Body/travel family bez automatycznej etykiety historycznej.

### Quest / evidence / ritual
- `ITEM_QUEST_EVIDENCE_GENERIC` — template family; finalne evidence records należą do kart questów.
- `ITEM_RITUAL_COMPONENT_GENERIC` — template family; nie oznacza potwierdzonej praktyki historycznej.
- `ITEM_OLD_SITE_FRAGMENT` — R5 old-site gameplay family, jawnie F dopóki konkretny rekord nie ma innego statusu.
- `ITEM_THRESHOLD_FRAGMENT` — R6 central-lore family; niesprzedawalny i chroniony przed utratą.

## Equipment mapping

| item | slot | class | lock |
|---|---|---|---|
| `ITEM_WEAPON_KNIFE` | MainHand | light-melee | stats balance |
| `ITEM_WEAPON_AXE` | MainHand | melee | stats + exact asset research |
| `ITEM_WEAPON_SPEAR` | MainHand | reach | stats + exact asset research |
| `ITEM_WEAPON_BOW` | Ranged | bow | stats + exact asset research |
| `ITEM_TOOL_HUNTING` | Tool | tracking | art/research |
| `ITEM_ARMOR_BODY_LIGHT` | Body | light-armor | stats + costume/material lock |
| `ITEM_ARMOR_BODY_MEDIUM` | Body | medium-armor | stats + material lock |
| `ITEM_ARMOR_HEAD_BASIC` | Head | head-protection | stats + material lock |

Hands/legs/feet i artifact slot nie dostają obowiązkowego contentu przed decyzją systemową. Durability i encumbrance pozostają otwarte zgodnie z `EquipmentSystem.md`.

## Recipe catalog

### Aktywne / implementation-ready
- `RECIPE_SIMPLE_BANDAGE`: `ITEM_CLOTH_MATERIAL` -> `ITEM_BANDAGE_BASIC`; station: simple/crafting prompt; knowledge: basic.
- `RECIPE_MARSH_SIGHT_TONIC`: `ITEM_MARSH_HERB` + `ITEM_FOREST_RESIN` -> `ITEM_MARSH_SIGHT_TONIC`; station: herbalist/simple table; efekt tylko zwiększa czytelność śladów.

### Warunkowe
- `RECIPE_SPIRIT_PREPARATION`: ingredient families zostają data/research lockiem; odblokować tylko jeśli spirit-exposure ma realny gameplay use.
- `RECIPE_BASIC_ANTIDOTE`: nie aktywować bez finalnego poison use-case.
- `RECIPE_FIELD_REPAIR`: nie aktywować dopóki durability nie zostanie przyjęte.

Nie dodajemy receptur wyłącznie dla liczby. Regional vendors mogą oferować item family powyżej, ale ceny, stock quantities, restock i signature variants pozostają ownerowane przez balance/data lock.

## Region source matrix

- R0: bandage, herbs/resin, hunting tools, shared weapons, basic clothing.
- R1: shared rural/travel supplies, transport/trade stock; bez nowych signature IDs bez potrzeby.
- R2: resin/forest ingredient families, hunting/travel supplies; ograniczony vendor footprint.
- R3: fish/trade/salt/salvage families i shared travel supplies.
- R4: ore/stone/workshop families i shared equipment.
- R5: route supplies i old-site fragments; culture-specific goods pozostają research lockiem.
- R6: expedition supplies + protected threshold/central-lore items; brak zwykłego pełnego marketu.

## Persistence i fail-forward

- inventory/equipment zapisują stable item IDs, stack/state i slot assignment;
- brakujący niekrytyczny content record ładuje placeholder diagnostyczny i loguje błąd;
- brakujący quest/evidence/threshold record jest save validation error i musi mieć migrację/fallback;
- crafting jest atomowy: składniki są konsumowane dopiero po walidacji wyniku;
- quest-protected item nie może wejść w vendor sell/crafting ingredient path bez jawnego override.

## Minimalne QA

1. każdy ID jest unikalny i resolvable;
2. każdy equipment item ma legalny slot i istniejący attack/effect profile;
3. recipe nie referuje nieistniejącego itemu/station;
4. quest-protected items nie są przypadkowo sprzedawalne/zużywalne;
5. region source nie obiecuje culture-specific assetu za research lockiem;
6. save/load zachowuje equipment i ingredient stacks;
7. brak finalnych liczb jest jawny, nie zastąpiony arbitralnym tuningiem.

## Otwarte locki

Finalne damage/protection/value/weight/durability, drop rates, recipe quantities/craft time, konkretne botaniczne odpowiedniki, historyczne modele broni/pancerza/narzędzi, culture-specific goods oraz finalne asset IDs pozostają odpowiednio balance/playtest/research/art lockiem.
