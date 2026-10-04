# Performance / release measurement manifests

Status: P5 v0.1 — reproducible measurement specification. This document defines **how** production performance and release evidence is collected; it does not invent numeric targets.

## Rules

Every run records: `scenarioId`, exact build/main SHA, platform/backend, OS/driver, CPU/GPU/RAM/VRAM, resolution/render preset/upscaler, save/content seed, region/cell, duration/warm-up, capture tool/version and artifact locations. A result without this provenance is diagnostic only and cannot move a lock in `MeasurementPlaytestEvidence.md` to CANDIDATE/LOCKED.

Runs must preserve gameplay correctness. Quality degradation may not remove quest/evidence cues, combat telegraphs, subtitles or required interactions. Cold and warm runs are separate evidence.

## Stable scenario manifest

| ID | Scope | Controlled setup | Required evidence | Candidate lock |
|---|---|---|---|---|
| `SCN_PERF_R0_BASELINE` | representative R0 village/vertical-slice baseline | fixed route, weather/time, NPC/encounter seed, cold + warm pass | frame-time trace, CPU/GPU breakdown, RAM/VRAM, streaming, AI counters | `LOCK_PERF_BASELINE` |
| `SCN_PERF_R1_DENSE_SETTLEMENT` | dense settlement/fort/service load | fixed population/services/route; no synthetic despawn | frame-time trace, active/near agents, path/perception queries, memory/streaming | `LOCK_AI_DENSITY`, `LOCK_PERF_CPU` |
| `SCN_PERF_R2_FOREST_TRAVERSAL` | vegetation + traversal + forest encounter | fixed route and encounter seed; cold/warm | frame-time, GPU counters, IO, residency/eviction, hitch log | `LOCK_STREAMING`, `LOCK_PERF_GPU` |
| `SCN_PERF_R3_WATER_PORT` | water/shore/boat/port composition | fixed camera/route/weather | CPU/GPU trace, VRAM, animation/VFX/audio concurrency | `LOCK_AVFX`, `LOCK_PERF_GPU` |
| `SCN_PERF_R4_WORKSITE` | rocks/worksite/AI activity | fixed workers/encounter/path set | CPU frame-time, AI queries, RAM, streaming | `LOCK_AI_DENSITY`, `LOCK_PERF_CPU` |
| `SCN_PERF_R5_CAMP` | culturally distinct settlement/camp | final-safe placeholder assets until art lock; fixed population | frame-time, RAM/VRAM, streaming, animation/audio concurrency | `LOCK_PERF_BASELINE` |
| `SCN_PERF_R6_ANOMALY_SPIKE` | anomaly/relic peak AVFX load | deterministic anomaly sequence; gameplay telegraphs preserved | GPU/CPU trace, particles/VFX, audio voices, RAM/VRAM, hitch log | `LOCK_AVFX`, `LOCK_PERF_GPU` |
| `SCN_PERF_CROSS_REGION_LONG_RUN` | leak/thrash/fragmentation | deterministic multi-cell R0→representative regions traversal, repeated loop | peak/steady RAM+VRAM, IO, evictions/reloads, handle errors, hitch percentiles | `LOCK_MEMORY`, `LOCK_STREAMING` |
| `SCN_PERF_COMBAT_SPIKE` | mixed combat worst representative spike | fixed mixed encounter + weather + AVFX | CPU/GPU trace, AI/query counts, animation/VFX/audio concurrency | `LOCK_PERF_CPU`, `LOCK_PERF_GPU`, `LOCK_AI_DENSITY` |
| `SCN_RELEASE_SAVE_ROUNDTRIP` | persistence/release correctness | save before/within/after region transition and quest state changes | save/load logs, state diff, stable-ID resolution, missing-content fallback | `LOCK_RELEASE_SAVE` |
| `SCN_RELEASE_DEGRADED_CONTENT` | fail-forward | intentionally unavailable optional asset/dialogue/service record | error log, fallback path, quest completion evidence | `LOCK_RELEASE_FAILFORWARD` |
| `SCN_RELEASE_LONG_SESSION` | long-session stability | representative traversal/combat/dialogue/save loop | crash/assert log, memory trend, handle/resource counts, save integrity | `LOCK_RELEASE_STABILITY` |

## Metrics required per domain

### Frame performance
Capture frame-time distribution, not average FPS alone. Preserve CPU and GPU frame times separately and record stalls/hitches with timestamps so they can be correlated with streaming, AI and AVFX events. Numeric percentile/ceiling targets remain `TBD-MEASURED` until representative captures exist.

### Memory and streaming
Capture steady and peak CPU RAM/VRAM, resident classes M0–M4, pending loads, read throughput, evictions/reloads and allocation/resource trends. Record cold-start, warm traversal and long-run evidence separately. Cell size, prefetch horizon and memory ceilings remain measurement-gated.

### AI/encounter
Capture active/near/background agent counts, update work, path/perception query counts and queue latency correlated to CPU frame time. Do not reduce quest-critical actors to satisfy a benchmark. Final agent/query/density limits remain measurement-gated.

### Animation/VFX/audio
Capture concurrent animation work, expensive rigs/retargeting where available, VFX/particle load and audio active/virtualized voice counts. Record which pressure/fallback state activated. Final ceilings remain measurement-gated.

## Hardware qualification matrix

No minimal/recommended specification is declared from vendor labels or intuition. A hardware row can be proposed only after the same release-candidate build executes the required manifest set with recorded configuration and artifacts.

Required rows:

- `HW_MIN_CANDIDATE`: lowest tested configuration satisfying locked correctness/performance criteria;
- `HW_RECOMMENDED_CANDIDATE`: tested configuration satisfying the chosen recommended quality/resolution criteria;
- at least one supported-driver/backend sanity configuration distinct from the primary measurement machine.

CPU/GPU model, RAM/VRAM, storage class, OS, driver, resolution/preset and observed evidence IDs are mandatory.

## Release evidence gates

A release candidate documentation pass requires all of the following evidence references, not prose assertions:

1. green automated build/regression run for the exact candidate SHA;
2. required performance manifests executed on qualified hardware rows;
3. no unresolved release-blocking crash/assert/save-corruption result;
4. save round-trip and missing/optional-content fail-forward evidence;
5. candidate/locked performance values traceable to raw captures;
6. balance/reputation/traversal locks traceable to their P5 scenario runs;
7. open research/art/line-writing decisions explicitly classified as non-blocking or resolved;
8. known issues list with severity/owner and release disposition.

## Result record

```yaml
measurementId: MEAS-YYYYMMDD-NNN
scenarioId: SCN_PERF_R0_BASELINE
buildSha: <exact sha>
hardwareId: <stable hardware row>
configuration: <preset/resolution/backend/driver>
runKind: cold|warm|long
artifacts:
  frameTrace: <path-or-url>
  telemetry: <path-or-url>
  logs: <path-or-url>
observations: []
invalidations: []
lockCandidates: []
reviewState: MEASURED
```

## Acceptance / invalidation

A run is invalid for locking when provenance is incomplete, debug instrumentation materially changes the workload without a paired representative capture, the content seed/setup differs without being recorded, a required gameplay element was disabled, or raw evidence is unavailable. Engine/content changes that can affect a locked metric require targeted re-measurement; the previous evidence remains historical rather than silently overwritten.

## Open measurement decisions

Still deliberately open until real captures exist: frame-time targets/percentiles, RAM/VRAM ceilings, IO/prefetch/cell targets, AI agent/query ceilings, AVFX/audio ceilings, minimal/recommended hardware, and release performance tolerances. Their owner is the measurement lifecycle in `MeasurementPlaytestEvidence.md`, not this manifest.