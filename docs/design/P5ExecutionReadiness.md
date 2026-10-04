# P5 execution readiness

Status: v0.1 — bridge between the finished P5 measurement specification and executable evidence collection.

## Purpose

`PerformanceReleaseMeasurementManifests.md`, `BalancePlaytestScenarioManifests.md` and `TraversalWeatherDayNightScenarioManifests.md` define reproducible scenarios. This document records whether the current implementation can actually execute and capture each evidence class without pretending that specification equals measurement.

The current codebase contains substantial gameplay/render implementation, but `Engine/Diagnostics` currently exposes only the basic engine log. Therefore no P5 performance lock may be promoted merely from an interactive run or an FPS observation. Instrumentation and deterministic scenario control are implementation dependencies of evidence collection.

## Readiness states

- `SPECIFIED`: scenario and required evidence are documented, but no complete executable capture path exists.
- `PARTIAL`: some required counters/control exist, but provenance or evidence is incomplete.
- `RUNNABLE`: deterministic setup plus all mandatory capture outputs exist.
- `MEASURED`: at least one valid run exists for an exact SHA/configuration.
- `CANDIDATE` / `LOCKED`: use the lifecycle in `MeasurementPlaytestEvidence.md`.

A scenario cannot skip directly from `SPECIFIED` to `LOCKED`.

## Current readiness matrix

| Evidence domain | Scenario owners | Current state | Missing implementation/capture dependency |
|---|---|---|---|
| combat balance | `BalancePlaytestScenarioManifests.md` | SPECIFIED | deterministic encounter setup, structured damage/TTR/resource event export, run provenance |
| economy/progression | `BalancePlaytestScenarioManifests.md` | SPECIFIED | deterministic inventory/vendor state, transaction/progression event export, resettable seed |
| evidence/reputation | `BalancePlaytestScenarioManifests.md` | SPECIFIED | structured evidence/reputation transition log and deterministic starting state |
| traversal/weather/day-night | `TraversalWeatherDayNightScenarioManifests.md` | SPECIFIED | scripted route/time/weather controls plus traversal/event timing export |
| CPU/GPU frame performance | `PerformanceReleaseMeasurementManifests.md` | SPECIFIED | frame-time distribution capture split into CPU/GPU plus timestamped hitch correlation |
| RAM/VRAM/streaming | `PerformanceReleaseMeasurementManifests.md` | SPECIFIED | memory/residency/IO/eviction counters and long-run export |
| AI density/query cost | `PerformanceReleaseMeasurementManifests.md` | SPECIFIED | active/near/background agent and path/perception query telemetry correlated with frame time |
| animation/VFX/audio | `PerformanceReleaseMeasurementManifests.md` | SPECIFIED | concurrency/work counters and pressure/fallback-state telemetry |
| save/fail-forward | `PerformanceReleaseMeasurementManifests.md` | PARTIAL | implementation exists in project, but P5 still needs scripted round-trip/state-diff evidence and artifact export |
| long-session stability | `PerformanceReleaseMeasurementManifests.md` | SPECIFIED | repeatable loop runner plus crash/assert/resource trend and save-integrity artifact bundle |

These states describe evidence readiness, not feature completeness.

## Minimum capture envelope

Every future executable P5 runner must emit one machine-readable run record containing at least:

```yaml
scenarioId: <stable SCN_*>
buildSha: <exact git SHA>
startedUtc: <timestamp>
durationSeconds: <measured>
platform:
  os: <value>
  backend: <value>
hardware:
  cpu: <value>
  gpu: <value>
  ram: <value>
  vram: <value-or-unknown>
configuration:
  resolution: <value>
  preset: <value>
  upscaler: <value>
seed: <value>
artifacts: []
validity:
  status: VALID|INVALID|DIAGNOSTIC
  reasons: []
```

Domain telemetry may live in separate CSV/JSON/traces, but the envelope must point to it. Missing exact SHA or scenario ID makes a run diagnostic only.

## Implementation order for measurement support

1. Add a common run/provenance recorder before domain-specific tuning.
2. Add deterministic scenario controls (seed, region/setup, time/weather where relevant).
3. Add low-overhead frame-time and hitch telemetry.
4. Add domain counters for streaming/memory, AI and AVFX/audio.
5. Add structured gameplay events for combat/economy/evidence/reputation.
6. Add scripted save/fail-forward and long-session runners.
7. Only then execute representative manifests and promote results through `MEASURED → CANDIDATE → LOCKED`.

This order avoids producing attractive but non-reproducible numbers.

## Evidence storage rule

Raw captures must remain traceable from the measurement ledger by stable evidence ID and exact SHA. Summaries in Markdown are not substitutes for raw evidence. If artifacts are too large for Git, store a durable artifact locator plus checksum/metadata; do not paste generated profiler dumps into design specs.

## Invalidation triggers

Re-run affected scenarios when a change materially touches rendering passes, asset residency/streaming, AI scheduling/query behavior, animation/VFX/audio workload, combat/economy/reputation formulas, save serialization, world density, or quality/preset behavior. Documentation-only changes do not invalidate measurements unless they alter the scenario definition.

## Current conclusion

The documentation side of P5 is ready, but the repository is not yet evidence-runner-ready for the full manifest set. The next useful work is instrumentation/scenario-runner implementation followed by real captures. Until those captures exist, numeric performance, hardware and final gameplay locks remain intentionally open.