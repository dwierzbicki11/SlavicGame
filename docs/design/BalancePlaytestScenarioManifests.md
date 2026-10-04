# Balance and playtest scenario manifests

Status: v0.1 — P5 scenario owner for combat, economy/progression and evidence/reputation.

This document turns the generic evidence contract in `MeasurementPlaytestEvidence.md` into reproducible gameplay scenarios. It deliberately defines setup, observations and acceptance inputs, **not final numeric targets**. Results remain OPEN until real playtests produce evidence records.

## Common rules

Every run records exact build SHA, scenario version, start save/state, seed when applicable, tester class, selected equipment/build, completion state and raw telemetry/artifact reference. A balance change that changes setup semantics increments the scenario version. Do not tune acceptance after seeing a run; the lock owner records the candidate range/sample rule before the confirming retest.

A scenario may expose several metrics, but no single metric is automatically a production target. Qualitative exploit/softlock observations are mandatory even when quantitative results look acceptable.

## Combat manifests

### `SCN_COMBAT_BASELINE_MELEE_V1`

**Locks:** `LOCK_COMBAT_MELEE_BASELINE`, `LOCK_COMBAT_DAMAGE_PACING`  
**Owner:** `CombatDesign.md`.

Setup: controlled neutral arena/test cell; standard player progression checkpoint; baseline melee weapon and armor; representative common melee enemy; no companion, weather modifier or quest buff.

Procedure: execute normal attack/defend/dodge loop from full resources until encounter resolves; repeat with at least the supported defensive approaches rather than one optimized sequence.

Capture: outcome, time-to-resolution, attacks landed/received, damage dealt/taken, stamina/resource exhaustion events, healing used, stagger/control events, exploit notes.

Acceptance inputs: encounter must be resolvable by intended mechanics; defensive alternatives must remain viable; no infinite stun/safe-loop or unavoidable damage chain. Final TTR/damage/resource ranges remain OPEN.

### `SCN_COMBAT_RANGED_PRESSURE_V1`

**Locks:** `LOCK_COMBAT_RANGED_BASELINE`, `LOCK_COMBAT_COUNTERPLAY`.

Setup: representative outdoor cell with cover/LOS breaks; baseline ranged loadout; representative melee pressure enemy plus ranged-capable case if supported.

Capture: resolution time, ammunition/resource consumption, distance bands, LOS breaks, damage exchange, disengage/re-engage behavior, unreachable/safe-position exploit observations.

Acceptance inputs: ranged play has usable counterplay and cannot trivially bypass encounter ownership through geometry or infinite kiting. Exact damage/ammunition values remain OPEN.

### `SCN_COMBAT_MIXED_ENCOUNTER_V1`

**Locks:** `LOCK_COMBAT_GROUP_PACING`, `LOCK_COMBAT_HEALING_PRESSURE`.

Setup: representative regional encounter family with mixed roles and normal world geometry; progression-appropriate equipment; no debug stat overrides.

Capture: success/failure, encounter duration, target-switch frequency, healing/resource use, crowd-control uptime, AI stalls, retreat/reset behavior and dominant-strategy notes.

Acceptance inputs: intended role mix is legible and recoverable; group encounter cannot be solved by a reproducible AI reset or pathing exploit. Numeric group-size/pacing targets remain measurement-gated.

## Economy and progression manifests

### `SCN_ECON_EARLY_LOOP_V1`

**Locks:** `LOCK_ECON_EARLY_INCOME_SINK`, `LOCK_PROG_EARLY_PACING`.

Setup: clean early-game save at the documented progression checkpoint; normal regional vendor/service availability; no imported inventory.

Procedure: follow a normal quest/exploration loop, acquire resources, use at least one service/purchase path and one supported craft/consumable path, then reach the next documented checkpoint.

Capture: currency/resources earned and spent by source/sink category, purchases/crafts, inventory leftovers, blocked necessities, progression unlock events, elapsed active play time and farming repetitions.

Acceptance inputs: mandatory progression is achievable without debug grants; meaningful sinks are reachable; no repeatable positive-value loop produces unbounded wealth without corresponding cost/risk. Final prices/rewards/pacing remain OPEN.

### `SCN_ECON_ALTERNATE_PATHS_V1`

**Locks:** `LOCK_ECON_PATH_PARITY`, `LOCK_PROG_BUILD_ACCESS`.

Setup: same progression checkpoint from equivalent clean saves. Compare supported acquisition routes such as purchase, crafting, quest reward and exploration where each exists.

Capture: prerequisites, opportunity costs, resource/currency flow, time/steps to usable outcome, unavailable-route reasons and whether one route invalidates the others.

Acceptance inputs: no intended route is accidentally impossible and no route dominates solely through a zero-cost/infinite conversion. This does not require equal numeric efficiency.

### `SCN_ECON_RESTOCK_REPEAT_V1`

**Locks:** `LOCK_ECON_RESTOCK`, `LOCK_ECON_INFINITE_LOOP`.

Setup: vendor/service profile with restock or world-state refresh enabled.

Procedure: exercise purchase/sale/craft interactions across documented refresh transitions without time/debug skipping beyond supported player actions.

Capture: stock transitions, buy/sell deltas, crafted outputs, refresh triggers, persistence after save/load and any arbitrage cycle.

Acceptance inputs: refresh state persists correctly; quest/evidence items remain protected; no deterministic buy/sell/craft/restock cycle creates unlimited net value. Exact restock interval/quantities remain OPEN.

## Evidence and reputation manifests

### `SCN_EVIDENCE_MAIN_VALID_PATH_V1`

**Locks:** `LOCK_EVIDENCE_MAIN_THRESHOLD`, `LOCK_EVIDENCE_REACHABILITY`.

Setup: clean save before a representative evidence-gated main decision; only evidence obtainable through documented non-debug paths is permitted.

Procedure: follow a valid investigative path, record each evidence acquisition and attempt the gate at the intended decision point.

Capture: evidence IDs/categories, source state, ordering, duplicate handling, gate state, dialogue/quest effects and persistence across save/load.

Acceptance inputs: at least one intended valid path reaches the gate; duplicates or reloads cannot inflate evidence; missing optional content does not create an undocumented hard lock. Final threshold value remains OPEN.

### `SCN_EVIDENCE_FAIL_FORWARD_V1`

**Locks:** `LOCK_EVIDENCE_FAIL_FORWARD`, `LOCK_QUEST_NO_SOFTLOCK`.

Setup: state where an optional evidence source/NPC/encounter becomes unavailable through a supported consequence.

Procedure: lose or close that source, continue through documented fallback routes and attempt the affected quest decision.

Capture: unavailable source reason, replacement/fallback hooks, quest state, player-facing feedback, reachable outcomes and persistence.

Acceptance inputs: supported consequence does not strand the campaign; fallback is semantically valid rather than silently granting missing evidence. Alternative outcomes may be worse, but must remain intentional.

### `SCN_REPUTATION_SERVICE_GATES_V1`

**Locks:** `LOCK_REP_SERVICE_THRESHOLDS`, `LOCK_REP_STATE_CONSISTENCY`.

Setup: representative regional faction/service with reputation-sensitive availability. Use clean saves bracketing candidate states once candidate thresholds exist.

Capture: reputation events with source IDs, current state, service/dialogue availability, quest consequences, save/load behavior and contradictory-state observations.

Acceptance inputs: state transitions are deterministic from recorded events; no service required for main progression becomes permanently inaccessible without documented fail-forward. Numeric thresholds remain OPEN until playtest evidence exists.

### `SCN_REPUTATION_CONFLICTING_ACTIONS_V1`

**Locks:** `LOCK_REP_ACCUMULATION`, `LOCK_REP_CONSEQUENCE_LEGIBILITY`.

Setup: sequence containing both positive and negative reputation events supported by content.

Capture: ordered events, resulting state after each event, surfaced player feedback, dialogue/service/encounter changes and repeated-event behavior.

Acceptance inputs: repeated events obey documented stacking/one-shot rules, consequences are explainable from state, and no repeatable event can farm reputation indefinitely unless explicitly designed.

## Cross-domain regression

Before any of the above locks becomes `LOCKED`, run at least one representative scenario with normal quest/world state rather than only an isolated test cell. Combat rewards must feed economy correctly; economy/progression changes must not bypass evidence/reputation gates; reputation/service gating must not remove required combat recovery or quest-critical services.

## Evidence hand-off

Each run emits an `EVT-*` record following `MeasurementPlaytestEvidence.md`. Candidate values/ranges belong in the corresponding system owner or lock ledger, not in this scenario document. Failed and inconclusive trials remain in history. Scenario manifests are versioned whenever setup or metric semantics change.

## Open decisions

Still measurement/playtest-gated: sample sizes, candidate target ranges, final damage/healing/resource values, prices/rewards/restock, progression pacing, evidence thresholds and reputation thresholds. These scenarios make those unknowns testable without fabricating them.