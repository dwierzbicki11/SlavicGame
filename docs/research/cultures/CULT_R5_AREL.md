# CULT_R5_AREL — evidence package v0.1

Status: **implementation-supporting, research-open**. Owner: evidence policy for the fictional Arel cultural context used by R5. This package does not identify Arel with any historical people and does not make the region a reconstruction of a single Eurasian society.

## Scope

R5 requires a credible basis for a landscape where fixed settlements, seasonal camps, long-distance routes, local route knowledge and exchange coexist. The game remains fictional. Evidence from Central/Eurasian contexts is used comparatively to constrain possibilities, not to copy ethnicity, costume, religion, language or ornament.

Chronological target for historical claims remains the project's IX–X c. inspiration window. Sources spanning broader periods are **discovery/comparative evidence only** until a claim receives a period-appropriate locator.

## Existing F decisions

The following are already fictional canon from `world/RegionBibleR5RowninyArel.md` and related design documents:

- `F-R5-01`: Równiny Arel combine permanent settlements with a finite set of seasonal camp states.
- `F-R5-02`: local route knowledge differs from official Nadborze maps and is mechanically valuable.
- `F-R5-03`: old roads preserve a fragment of memory about the ancient threshold network.
- `F-R5-04`: Arel communities are internally diverse; mobility is not a universal ethnic trait.
- `F-R5-05`: R5 participates in interregional trade and offers guiding/logistics services.
- `F-R5-06`: `old-site` remains a neutral production token until archaeology/art research locks its form.

These decisions are worldbuilding, not claims about a real culture.

## Comparative research basis

The first basis intentionally uses several contexts rather than one template.

| source_id | basis | useful constraint | limitation |
|---|---|---|---|
| `AREL-S01` | UNESCO, Orkhon Valley Cultural Landscape | pastoral mobility can coexist with administrative/religious centres and trade networks | multi-period Mongolian/Central Asian landscape; not direct evidence for fictional Arel or every IX–X c. detail |
| `AREL-S02` | UNESCO, Silk Roads: Chang'an–Tianshan Corridor | settled, agrarian, pastoral and mobile communities interacted along route systems; routes moved goods, people and ideas | very broad chronology and geography; use comparatively, not as an asset locator |
| `AREL-S03` | UNESCO, Zarafshan–Karakum Corridor | route landscapes include towns, forts, way stations, river crossings and different settlement/land-use responses | broad 2nd c. BCE–16th c. CE corridor; does not justify copying Sogdian or oasis material culture |

This triangulation supports one negative production conclusion with high confidence: **open-country mobility must not be treated as evidence that a society lacks permanent settlements, institutions, trade infrastructure or cultural diversity.** It does not lock a specific Arel visual identity.

## Evidence ledger

| claim_id | class | claim | sources | confidence / transfer note |
|---|---|---|---|---|
| `AREL-E01` | R | Mixed mobility and permanent/central places are a plausible design combination. | S01, S02 | medium-high comparative; exact Arel settlement forms remain F/U |
| `AREL-E02` | R | Long-distance routes can connect communities with different subsistence and settlement patterns. | S02, S03 | high comparative; no direct Arel ethnic inference |
| `AREL-E03` | R | Route infrastructure may include fixed service/control nodes rather than only informal tracks. | S02, S03 | medium; exact IX–X c. Arel node form is U |
| `AREL-E04` | R | Water availability and terrain can structure routes and settlement choices. | S02, S03 | high general constraint; local implementation remains fictional geography |
| `AREL-E05` | F | Arel route memory is socially distributed and can contradict Nadborze administration. | project canon | gameplay/worldbuilding decision |
| `AREL-E06` | U | Exact seasonal camp architecture and movable furnishings. | — | requires period/regional archaeological locators |
| `AREL-E07` | U | Pack/riding animals, harness, wagons and load systems used in final Arel assets. | — | separate transport/zooarchaeology research required |
| `AREL-E08` | U | Costume, textile construction, jewellery and ornament. | — | must not be synthesized from generic "steppe" imagery |
| `AREL-E09` | U | Burial/old-site form and ritual interpretation. | — | archaeology + interpretation lock required |
| `AREL-E10` | U | Language, personal names and titles. | — | naming/language owner required; no ethnic shortcut |

## Material-culture constraints

### Settlements and camps

Implementation may build distinct **families** for permanent settlement and seasonal camp because coexistence is part of fictional R5 canon and is comparatively plausible. Final building plans, tent/shelter structures, hearths, storage, bedding and portable furniture remain `U`. Do not make every Arel dwelling a tent, and do not make every permanent settlement visually foreign to its mobile neighbours.

### Routes, transport and water

Road hierarchy, discovered route nodes and water-linked navigation may be implemented now as gameplay data. Exact road markers, bridges, carts/wagons, saddles, harness, pack systems and animal assignments remain research-locked. No horse-centric culture is implied by the word `Arel` or by open terrain.

### Clothing and visual identity

No final costume silhouette, hat, armour, jewellery set, tattoo/body-marking system, carpet pattern or colour palette is locked by this package. Art must not assemble a "steppe look" from unrelated Turkic, Mongolian, Iranian, Scythian/Sarmatian or modern ethnographic references. A final asset needs a named claim/source chain or an explicit `F` decision reviewed against the anti-stereotype rule.

### Economy

R5 may support route services, guiding, exchange and logistics. Specific livestock, animal products, crops, foodways and prestige goods remain content/research data. A long-distance trade route does not imply that all inhabitants are merchants or pastoralists.

## Social/legal constraints

- no single "Arel opinion" or universal mobility model;
- permanent and seasonal communities can have overlapping but non-identical interests;
- external checkpoints/tolls belong to explicit faction/world-state data, not assumed ethnicity;
- hospitality, hostage customs, marriage rules, inheritance and gender roles remain `U/F` until deliberately authored; none may be inferred from a generic nomad template;
- route expertise is a profession/social capability, not automatic mystical knowledge.

## Religion and old sites

Arel religion is not derived from the comparative sources above. `old-site` remains neutral. Burial mounds, ancestor cults, sky worship, shamanism, fire cults or other recognizable motifs are **not** unlocked by this package. Any such element requires its own source-strength pass and must distinguish archaeological observation, scholarly interpretation and fictional supernatural truth.

## Naming/language constraints

Do not borrow a real historical ethnonym or mix phonemes/suffixes from unrelated Central Asian languages to manufacture "Arel-sounding" names. Until `NamingRules.md` receives a dedicated Arel pass, production uses stable IDs and approved existing F names only. Language diversity can exist as a worldbuilding property, but concrete languages/registers remain unresolved.

## Asset/content consequences

Unlocked for implementation as data-driven families:

- permanent-settlement modules with placeholder-neutral art;
- seasonal-camp state/placement system;
- route/landmark/water-node graph;
- generic trade/logistics encounter roles;
- old-road discovery and route-knowledge evidence;
- `old-site` placeholder volumes/IDs without final archaeological form.

Still blocked from final art/content lock:

- culture-specific architecture and camp construction;
- costume/ornament;
- animals, harness and vehicles;
- ritual/burial/old-site appearance;
- named historical-style offices/customs;
- final Arel naming/language package.

## Open locks and owners

1. `AREL-U-TRANSPORT` — material-culture research: IX–X c. transport, animal use and equipment with locators.
2. `AREL-U-DWELLING` — archaeology/art research: settlement and seasonal shelter evidence with regional/chronological fit.
3. `AREL-U-COSTUME` — textile/art research: garment construction and accessories; no generic steppe collage.
4. `AREL-U-OLD-SITE` — archaeology + narrative: final old-site/burial forms and interpretation.
5. `AREL-U-ECONOMY` — content/economy: region-fit goods, foods, livestock and services.
6. `AREL-U-NAMING` — narrative/language: dedicated naming rules.
7. `AREL-U-RELIGION` — pantheon/research: any material religious practice used in final content.

None of these blocks the R5 topology, traversal, camp-state persistence, route evidence flow or generic encounter implementation. They do block claims that a final visual/detail is historically grounded.

## Sources — basis v0.1

- `AREL-S01`: UNESCO World Heritage Centre, **Orkhon Valley Cultural Landscape**, list 1081, https://whc.unesco.org/en/list/1081 — comparative landscape evidence; includes 6th c. and later archaeological remains but spans many periods.
- `AREL-S02`: UNESCO World Heritage Centre, **Silk Roads: the Routes Network of Chang'an–Tianshan Corridor**, list 1442, https://whc.unesco.org/en/list/1442 — comparative route/settlement interaction evidence; broad 2nd c. BCE–16th c. CE scope.
- `AREL-S03`: UNESCO World Heritage Centre, **Silk Roads: Zarafshan–Karakum Corridor**, list 1675, https://whc.unesco.org/en/list/1675 — comparative route-landscape diversity; broad 2nd c. BCE–16th c. CE scope.

Last research review: 2026-10-03.

## Status decision

`CULT_R5_AREL` now satisfies the **evidence-package v0.1 / implementation-supporting** threshold: fictional canon, comparative constraints, prohibited shortcuts and unresolved production locks are explicit. It is **not production-ready** under `CultureResearchFramework.md`, because final material-culture claims still need period-appropriate academic/archaeological sources with locators.