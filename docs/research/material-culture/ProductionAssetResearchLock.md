# Material culture — production asset research lock

**Status:** PASS v0.1 for implementation-safe asset families; exact reconstruction remains locator/art locked where stated.  
**Owner:** `docs/research/material-culture/*` + `docs/research/cultures/CULT_R*.md`.  
**Purpose:** turn the existing material-culture research into an explicit production contract without inventing precision that the consulted sources do not support.

## 1. Lock rule

An asset may be presented as historically grounded only when its evidence row below has a locator strong enough for the claimed feature. Otherwise it must be one of:

- **R** — bounded reconstruction consistent with evidence, but not directly attested in the exact game context;
- **F** — deliberate fiction / visual design;
- **U** — unresolved; do not freeze a final historical-looking model from it.

A source title or bibliography entry alone is not a final locator. An abstract can justify only what the abstract actually states. A museum object catalogue can justify that object's documented properties, not population-wide prevalence.

## 2. Locator quality

| Grade | Meaning | Production use |
|---|---|---|
| L3 | exact catalogue object, page/figure/table or directly addressable museum record | may lock the specifically documented feature |
| L2 | named museum page or article page range plus a consulted passage/summary supporting the claim | may lock an asset family; exact geometry/details still require L3 where material |
| L1 | abstract, metadata or bibliography only | discovery/context; cannot freeze detailed reconstruction |
| L0 | no verified locator | U; no historical claim |

## 3. Current evidence ledger

| ID | Current locator | Grade | Safe production claim | Do not infer |
|---|---|---:|---|---|
| MC01 | Pawlak 2013, *Slavia Antiqua* 54, pp. 143–219; consulted abstract/bibliography | L1 | settlement feature families: sunken buildings, hearths, pits, pottery, quern/stone-related finds | exact house dimensions, roof/wall construction or a universal village plan |
| MC02 | Muzeum Pierwszych Piastów na Lednicy, named page `Podgrodzie` | L2 | dense multi-phase settlement/craft/market families for a high-status centre | ordinary-village density or universal craft mix |
| MC03 | museum page `Zabytki Ostrowa Lednickiego w Biskupinie!` | L2 | attested craft/tool/fishing and dugout-boat families | exact IX–X c. kit for every region |
| MC04 | museum page `Przyczółek mostu zachodniego` | L2 | timber bridge and dugout-boat families; large central-site precedent | copying Lednica scale into Żarnowiec or other minor crossings |
| MC05 | museum page `Przyczółek mostu wschodniego` | L2 | household wooden/ceramic vessel, tool, footwear/sheath families | a single-period standardized household kit |
| MC06/MC09 | Sikorski/Wrzesińska/Wrzesiński, WSA 23, pp. 71–78; consulted abstract/metadata | L1 | textile-use categories exist | complete costume silhouettes, cuts, colours or social uniformity |
| MC07 | Koszałka 2006 repository metadata | L1 | specialist archaeobotanical study exists | species lists, crop ratios, recipes |
| MC08 | Makowiecki 1997/2001 repository metadata | L1 | specialist animal-use study exists | species ratios, diet shares, herd composition |
| MC10 | museum page `Militaria` | L2 | elite/luxury framing for helmet/mail/swords; weapon-family presence | common issue of elite equipment or frequency from exceptional assemblage |
| MC11 | catalogue object `MPPL-MPP-A-4-275_62` | L3 | this specific X–mid-XI c. sword record and documented object properties | generic sword prevalence or standard loadout |
| MC12 | Kurasiński 2015, pp. 137–198; consulted abstract | L1 | bucket-burial phenomenon and interpretive complexity | universal pagan rite or a single religious meaning |
| MC13 | Bogucki 2025, named PAN article | L2 | foreign coin/silver context around early Piast state formation | coin-only village economy |
| MC14 | Pawlak 2021, *Slavia Antiqua* 62, DOI 10.14746/sa.2021.62.6; consulted abstract | L1 | regional settlement clusters relate to long-distance routes | exact road layout of a game settlement |

## 4. Asset-family decisions now unlocked

The following can proceed as modular families, with variants kept data-driven and without claiming a single pan-Slavic standard:

- settlement structures, hearth/pit/work-yard dressing;
- pottery, wooden vessel and basic household-prop families;
- craft-workspace families for woodworking, pottery, metal/organic-material work where already mapped by regional packages;
- quern/agriculture/fishing tool families at the level supported by the owner cards;
- simple crossing/bridge and dugout-boat families, scaled to region fiction rather than copied from Lednica;
- textile/sack/pouch functional families without freezing a full costume reconstruction;
- weapon families with elite/commonness separated from mere archaeological presence;
- trade/silver/foreign-coin props without forcing a coin economy.

These are **family locks**, not permission to reproduce an arbitrary museum reconstruction verbatim.

## 5. Remaining exact-locator locks

Before an asset is marked `historical-final`, obtain L3 (or a justified L2 where geometry is irrelevant) for the exact feature being claimed. Highest-value remaining work:

1. architecture: wall/roof/joinery/opening details and dimensions for selected settlement archetypes;
2. clothing: cuts, fastenings, fibre/weave, footwear and region/chronology-specific combinations;
3. agriculture/food: exact tool forms and archaeobotanical species claims;
4. animal economy: species/use claims where they affect visible husbandry or recipes;
5. transport: selected vehicle/boat/bridge construction details beyond family-level precedent;
6. weapons: exact typology/dimensions for final hero/NPC models and social availability;
7. burials/religious material: grave layout/object placement and interpretation kept separate;
8. R1–R5 differential technologies identified in their culture packages (river/port, forestry, mining, Arel mobility/dwellings) need their own region-fit evidence rather than inheriting R0 by default.

If a locator cannot be obtained, keep the feature R/F/U and record that choice in the asset manifest. Missing precision must never be silently promoted to H.

## 6. Required asset-manifest provenance fields

Every final historically framed model/material/prop record should carry:

```text
evidenceClass: H | R | F | U
sourceIds: [MCxx, ...]
locatorGrade: L0 | L1 | L2 | L3
regionFit: direct | comparative | fictional
chronologyFit: direct | adjacent | broad | unknown
claim: <what the evidence actually supports>
artDeviation: <intentional gameplay/readability deviation or none>
```

`sourceIds` without `claim` and `locatorGrade` are insufficient for historical-final status.

## 7. Production result

**PASS v0.1:** material-culture research no longer blocks implementation of modular production asset families. The project has an explicit boundary between attested family-level evidence and exact historical reconstruction. Exact appearance locks remain open only where the current evidence is L0/L1/L2-inadequate for the requested detail; they are now enumerable work rather than an implicit blocker.
