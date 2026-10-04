# P5 evidence ledger

Status: v0.1 — authoritative index of real P5 measurement/playtest evidence. This file records evidence status; it does **not** invent results.

## Rules

1. One row represents one stable evidence/lock target, not one arbitrary play session.
2. `MEASURED`, `CANDIDATE` and `LOCKED` require a valid run bundle conforming to `MeasurementPlaytestEvidence.md` and `technical/P5InstrumentationHandoff.md`.
3. Exact `buildSha`, scenario ID and raw-artifact locator are mandatory before promotion to `MEASURED`.
4. Missing instrumentation keeps the row `OPEN`; an FPS counter, screenshot or anecdotal observation is diagnostic only.
5. If a change hits an invalidation trigger from `P5ExecutionReadiness.md`, affected rows return to the appropriate pre-lock state while prior evidence remains traceable.
6. Numeric target/range fields remain `—` until supported by valid evidence.

## Status vocabulary

`OPEN` → `MEASURED` → `CANDIDATE` → `LOCKED`, with `INVALIDATED` for a previously evidenced target requiring re-run. `BLOCKED-INSTRUMENTATION` is an execution note, not a substitute for the evidence lifecycle.

## Gameplay lock ledger

| Evidence ID | Domain | Scenario family | Status | Current execution note | Candidate/locked value | Evidence locator |
|---|---|---|---|---|---|---|
| `EV-COMBAT-001` | combat baseline | balance/combat | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-COMBAT-002` | ranged/mixed pressure | balance/combat | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-ECON-001` | early economy/progression | balance/economy | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-ECON-002` | restock/arbitrage resilience | balance/economy | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-EVID-001` | evidence valid path/fail-forward | balance/evidence | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-REP-001` | reputation/service gates | balance/reputation | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-TRAV-001` | baseline/adverse traversal | traversal/world | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-WEATHER-001` | weather transition/gameplay | traversal/world | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-TIME-001` | day/night/time-gated behavior | traversal/world | OPEN | BLOCKED-INSTRUMENTATION | — | — |

## Performance/release lock ledger

| Evidence ID | Domain | Scenario family | Status | Current execution note | Candidate/locked value | Evidence locator |
|---|---|---|---|---|---|---|
| `EV-FRAME-001` | CPU/GPU frame-time + hitches | performance R0–R6/spikes | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-MEM-001` | RAM/VRAM/residency | performance R0–R6/long run | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-STREAM-001` | streaming IO/eviction/reload | performance traversal/long run | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-AI-001` | AI agents/query cost | performance encounter load | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-AVFX-001` | animation/VFX/audio workload | performance combat/world | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-SAVE-001` | save round-trip/fail-forward | release/save | OPEN | PARTIAL-INSTRUMENTATION | — | — |
| `EV-STABILITY-001` | long-session stability | release/long session | OPEN | BLOCKED-INSTRUMENTATION | — | — |
| `EV-HW-MIN-001` | minimum hardware qualification | release/hardware | OPEN | DEPENDS-ON-MEASURED-TARGETS | — | — |
| `EV-HW-REC-001` | recommended hardware qualification | release/hardware | OPEN | DEPENDS-ON-MEASURED-TARGETS | — | — |
| `EV-RELEASE-001` | candidate SHA release evidence | release/full manifest | OPEN | DEPENDS-ON-ALL-REQUIRED-EVIDENCE | — | — |

## Promotion record template

When a row advances, append a compact record below rather than overwriting provenance:

```yaml
evidenceId: EV-...
status: MEASURED|CANDIDATE|LOCKED|INVALIDATED
buildSha: <exact SHA>
scenarioId: <SCN_*>
runId: <stable run ID>
configurationId: <stable configuration/hardware profile>
summary: <measured observation or candidate/locked range>
artifacts:
  - locator: <durable path/url/artifact ID>
    checksum: <when practical>
validity:
  status: VALID
  notes: []
reviewedUtc: <timestamp>
```

No promotion records exist yet. The empty ledger is intentional: current documentation defines how to measure, while implementation still lacks the complete capture path needed for valid P5 evidence.
