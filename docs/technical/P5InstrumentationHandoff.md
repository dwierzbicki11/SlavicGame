# P5 instrumentation handoff

Status: v0.1 — implementation handoff for turning the documented P5 scenarios into reproducible evidence.

## Scope

This document does not define new balance or performance targets. It translates `design/P5ExecutionReadiness.md` into the minimum implementation interfaces needed by the scenario manifests. Numeric locks remain measurement-gated.

## Common recorder

All runners SHOULD write a directory named `<scenarioId>/<buildSha>/<runId>/` containing `run.json` plus domain artifacts. `run.json` owns provenance and artifact references; domain files own samples/events. A run without exact `scenarioId`, `buildSha`, seed and configuration is `DIAGNOSTIC`, never `MEASURED`.

Required common fields:

- `schemaVersion`, `runId`, `scenarioId`, `buildSha`;
- UTC start/end and measured duration;
- OS/backend, CPU/GPU/RAM/VRAM identity;
- resolution, preset, upscaler and deterministic seed;
- artifact path, type, checksum when practical;
- validity status and machine-readable invalidation reasons.

Writers must flush on normal scenario completion and attempt a best-effort flush on controlled failure. Capture failures must not silently turn missing samples into zeroes.

## Telemetry interfaces

### Frame/performance

Emit timestamped frame samples with CPU frame duration, GPU duration when supported, present/frame index and hitch marker. Unsupported GPU timing must be `null/unsupported`, not `0`.

### Memory/streaming

Emit periodic CPU memory, GPU residency when available, pending loads, bytes read, eviction/reload counts and current streaming pressure state. Counters must state whether they are instantaneous, cumulative or high-water marks.

### AI

Emit active/near/background agent counts and path/perception query counts/cost where instrumented. Samples must share the run clock so spikes can be correlated with frame telemetry.

### Animation/VFX/audio

Emit active animation workload, VFX workload/concurrency, audio voice/concurrency state and any quality-pressure/fallback transition exposed by the engine. Platform APIs that cannot expose a metric must mark it unsupported.

### Gameplay balance

Use structured events rather than log scraping. Combat events need actor/source/target, action or damage family, resource delta and timestamp. Economy/progression events need transaction/reward/source and before/after values. Evidence/reputation events need stable cause ID, affected track and before/after state.

### World traversal

Record region/cell transitions, scripted route checkpoints, time/weather state transitions and relevant traversal failures/recovery. Scenario controls must record requested state separately from observed state.

### Save/fail-forward

A scripted round-trip must preserve pre-save and post-load state snapshots or hashes for fields owned by the scenario. Mismatch reports must name the field/path; a successful load alone is insufficient evidence.

## Deterministic control surface

The runner needs a test-only control surface capable of selecting stable scenario ID and seed, loading the required region/setup, resetting owned gameplay state and, where the manifest requires it, controlling time/weather/inventory/vendor/encounter state. Test controls must not be reachable through normal player-facing progression in release builds.

## Validity rules

A capture is invalid when the requested scenario cannot be established, required artifacts are missing/corrupt, build SHA cannot be proven, the run changes configuration mid-scenario without the manifest allowing it, or a telemetry subsystem reports sample loss beyond its declared tolerance. Invalid runs remain useful diagnostics but cannot advance a lock.

## CI and local responsibilities

CI should verify schemas, deterministic setup smoke tests and artifact production on supported headless/test paths. Hardware qualification and representative GPU/VRAM measurements remain real-machine work; CI success is not a substitute for those captures.

## Definition of instrumentation-ready

A P5 domain may move from `SPECIFIED` to `RUNNABLE` only when:

1. its manifest can establish the documented initial state repeatably;
2. the common provenance record is produced;
3. every mandatory domain observation is exported or explicitly marked unsupported where the manifest permits it;
4. artifacts survive after the run and can be traced to the exact SHA;
5. an intentionally broken smoke run is rejected as invalid.

Only after that gate should the project spend time collecting candidate tuning/performance evidence.