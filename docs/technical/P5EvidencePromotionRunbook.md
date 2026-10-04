# P5 Evidence Promotion Runbook

Status: v0.1 — operational companion to `design/MeasurementPlaytestEvidence.md`, `design/P5EvidenceLedger.md`, scenario manifests and `P5CaptureArtifactFormat.md`.

## Purpose

This runbook defines the repeatable path from a runnable P5 scenario to an auditable ledger promotion. It does not define balance or performance targets and must never be used to invent missing measurements.

## Preconditions

A run may start only when:

1. the scenario has a stable scenario ID in an approved manifest;
2. the intended target has a stable `EV-*` ID in `P5EvidenceLedger.md`;
3. the executable build is identified by exact commit SHA;
4. the recorder can emit the package required by `P5CaptureArtifactFormat.md`;
5. required collectors for the scenario are available, or every unavailable metric is explicitly represented as unavailable rather than zero;
6. hardware, graphics/gameplay configuration, seed and content/config revisions required by the scenario are captured.

If any mandatory precondition is absent, the attempt is `INVALID` and cannot promote evidence.

## Run procedure

### 1. Prepare

- checkout/build the exact candidate SHA;
- select one scenario ID and its linked `EV-*` targets;
- reset mutable test state required by the manifest;
- record hardware/OS/driver/runtime/configuration provenance;
- choose or record deterministic seed where the scenario supports one;
- create a unique `run_id`.

### 2. Capture

Start capture before scenario setup that can affect the measured state. Emit phase/event markers for setup, warm-up, measured interval, relevant transitions and teardown. Preserve raw samples and events; summary-only runs are not production evidence.

Do not silently drop unavailable collectors. A collector failure or unsupported metric must be visible in provenance/events and evaluated against the scenario's mandatory evidence requirements.

### 3. Validate

Run the capture validator before interpreting results. Validation must at minimum check:

- package structure and schema/version compatibility;
- matching `run_id` across files;
- exact build SHA and scenario ID;
- linked `EV-*` target IDs;
- timestamp monotonicity and declared units;
- required phase/event markers;
- mandatory collectors/raw samples;
- artifact hashes and referenced artifact existence;
- absence of placeholder/default-zero values masquerading as measurements.

A validation failure produces `INVALID`; fix instrumentation or rerun instead of editing raw evidence into compliance.

### 4. Review

For a valid run, compare observations only with acceptance inputs defined by the owning scenario/spec. Record anomalies, confounders and deviations. A surprising result is not invalid merely because it is undesirable; preserve it and rerun when needed.

Use multiple valid runs when the owning contract requires repeatability, variance characterization or cross-hardware coverage.

## Ledger promotion

### OPEN → MEASURED

Promotion requires at least one valid capture package that satisfies the target's required scenario/collector contract. The ledger entry must identify:

- exact build SHA;
- scenario ID and run ID(s);
- raw artifact locator(s);
- capture/schema version;
- measured value/range/observation with unit;
- hardware/configuration identity where relevant;
- known anomalies or limitations.

### MEASURED → CANDIDATE

Promotion requires enough valid evidence to propose a production value or qualitative lock according to the owning design contract. Record the candidate value/range/decision, supporting run set, rationale and any hardware/content scope.

A single convenient run must not be promoted when the contract requires repeatability or representative coverage.

### CANDIDATE → LOCKED

Promotion requires the acceptance conditions of the owning contract to be satisfied, regressions/required retests to pass, and all evidence locators to remain resolvable. For release-sensitive targets, the candidate SHA/hardware matrix must match the release evidence rules.

`LOCKED` is invalidated by changes identified by the evidence contract as materially affecting the target. Invalidated locks return to the appropriate earlier lifecycle state with the previous evidence retained as history.

## Rerun triggers

Rerun affected evidence after material changes to rendering, streaming/residency, AI scheduling/path/perception, combat/economy/progression formulas, reputation/evidence logic, traversal/weather/day-night behavior, save/persistence, content density, asset cost, platform/runtime/driver assumptions, or instrumentation itself.

A documentation-only edit that does not alter scenario semantics or acceptance criteria does not automatically invalidate a capture. A semantic change to a scenario does; increment/revise the scenario/capture contract as required and preserve old evidence provenance.

## Failed and partial runs

- crash/hang before mandatory measured interval: `INVALID`, retain crash artifacts;
- missing mandatory collector: `INVALID` for targets requiring it, even if other metrics are useful;
- optional collector unavailable: run may remain valid for unaffected targets if the manifest permits it;
- manual intervention outside scenario rules: `INVALID` unless explicitly recorded and allowed;
- incomplete hardware provenance for hardware qualification: cannot promote hardware targets;
- profiler/recorder overhead suspected to distort the target: retain the run, mark limitation, and collect a corrected run before promotion.

## CI and review expectations

Schema/validator tests may run in CI, but CI success is not measurement evidence by itself. Production evidence comes from valid scenario captures. Changes to ledger lifecycle state should be reviewable as ordinary repository diffs and must not remove prior evidence history merely to make a target appear green.

## Completion condition

P5 documentation can be considered production-locked only when every production-required `EV-*` target is `LOCKED` or explicitly dispositioned out of final scope by an approved scope decision, and release criteria for the selected candidate SHA are backed by valid raw evidence. Until then, `DocumentationCoverage.md` must continue to state that production lock is open.