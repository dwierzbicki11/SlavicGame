# Animation / VFX / audio budget contract

Status: production planning v0.1. Owner: content + engine/rendering + audio. This document defines **cost classes, reuse rules, degradation order and measurement gates**. It deliberately does not invent numeric CPU/GPU/RAM/VRAM/voice limits before representative builds are profiled.

## Goals

1. Give animation, VFX and audio records stable integration expectations before final assets exist.
2. Prevent one-off content from silently creating unbounded runtime cost.
3. Keep gameplay readability and quest-critical feedback above cosmetic fidelity.
4. Make low/medium/high/ultra profiles degradable without changing simulation truth.
5. Require measured budgets before production lock.

## Common record contract

Every production animation/VFX/audio record referenced by `ProductionAssetManifests.md` carries:

- `recordId` — stable content ID;
- `ownerAssetId` or `ownerSystem`;
- `costClass` — `C0`, `C1`, `C2`, `C3`;
- `priorityClass` — `critical`, `gameplay`, `ambient`, `cosmetic`;
- `reuseGroup` — shared rig/effect/event family when applicable;
- `streamingClass` — resident, regional, encounter, cinematic;
- `fallbackId` — cheaper valid representation where one exists;
- `measurementState` — `unmeasured`, `sampled`, `locked`;
- `qualityOverrides` — allowed presentation changes per quality profile;
- `qaTags` — deterministic checks required by the owning feature.

`C0–C3` are ordering classes, not milliseconds, megabytes, particle counts or voice counts. Numeric ceilings are created only from measurements on representative hardware.

## Animation

### Cost classes

- **C0:** shared locomotion, basic interaction, common hit/reaction and traversal clips expected to be reused broadly.
- **C1:** regional/common combat or profession sets with moderate reuse.
- **C2:** named creature, companion, ritual or encounter-specific sets.
- **C3:** rare bespoke/cinematic sequences, complex layered rigs or sequences with many synchronized actors.

### Rules

- Prefer shared humanoid skeletons, retargeting and additive layers before bespoke rigs.
- Gameplay timing/hit windows are data owned by gameplay; animation may present them but must not become the only source of truth.
- Off-screen and distant actors may reduce update/presentation frequency only when simulation remains correct.
- Quest-critical interactions require a valid fallback pose/clip path so missing polish cannot softlock progress.
- Root-motion use must be explicit per record and compatible with navigation/collision ownership.

## VFX

### Cost classes

- **C0:** reusable interaction/readability cues, simple impacts, selection/trace feedback.
- **C1:** common weather/local ambience and reusable combat families.
- **C2:** encounter signature effects, ritual/magic families, dense environmental moments.
- **C3:** rare hero/cinematic/anomaly effects with high overdraw, lighting, simulation or synchronization risk.

### Degradation order

When measured pressure exceeds the profile budget, reduce in this order unless the owning feature declares otherwise:

1. cosmetic secondary emitters and debris;
2. spawn density/range of ambient effects;
3. expensive distortion/lighting/shadow contribution;
4. simulation/update frequency for non-critical distant effects;
5. swap to `fallbackId`.

Never remove a telegraph, evidence cue, navigation cue or quest-state indicator required to understand gameplay. VFX cannot be the sole carrier of critical information where accessibility requires another channel.

## Audio

### Cost classes

- **C0:** UI, player feedback and quest/gameplay-critical events.
- **C1:** common Foley, combat and reusable environmental event families.
- **C2:** regional ambience layers, creature/ritual signatures and localized set pieces.
- **C3:** dense cinematic/multi-source scenes or unusually expensive processing/streaming cases.

### Voice and virtualization policy

- Priority is `critical > gameplay > ambient > cosmetic`.
- Ambient/cosmetic events must tolerate virtualization, concurrency limiting or replacement by cheaper beds.
- Dialogue needed for progression must remain intelligible; VO absence must have subtitle/text fallback.
- Repeated events use concurrency/retrigger policy to prevent stacking storms.
- Long regional ambience and music should stream where the measured platform profile supports it; short latency-sensitive events follow the engine's measured residency policy.

## Cross-domain spike control

A single encounter/cutscene must declare when C2/C3 animation, VFX and audio coincide. Content review checks combined pressure rather than approving each discipline in isolation. C3 is never a default for repeatable ambient content.

For R0–R6, regional identity should come primarily from reusable families plus controlled variants. Bespoke C2/C3 records are reserved for signature encounters, important rituals, named entities and campaign beats. R6 anomaly presentation may be visually distinctive but remains subject to the same fallback and accessibility rules.

## Quality-profile contract

Quality profiles may change presentation, not authored world state. Allowed knobs include animation presentation LOD/update cadence for non-critical distant actors, VFX density/range/secondary layers, audio ambience density and expensive processing. Critical telegraphs, dialogue text, interaction confirmation and quest/evidence feedback remain available on every supported profile.

## Measurement gate

Before any numeric budget becomes `locked`:

1. choose representative scenes: settlement, forest, port/water, open/worksite, R5 camp/route and R6 anomaly/campaign spike;
2. capture CPU frame time, GPU frame time, memory/VRAM residency/streaming pressure and audio/animation subsystem counters available in the engine;
3. test at least the intended minimum and recommended hardware classes once those are evidence-backed;
4. record worst sustained case and short spike separately;
5. derive profile ceilings with headroom rather than copying the observed maximum;
6. rerun after material changes to renderer, animation runtime, audio backend or encounter density.

Until this gate is complete, documentation may use cost/priority classes but must not claim final numeric limits.

## QA acceptance

- every C2/C3 record has an owner and measured-or-explicitly-unmeasured state;
- repeatable content does not depend on C3-only presentation;
- disabling cosmetic layers cannot break quest/combat comprehension;
- missing VO has text fallback;
- fallback records resolve without missing references;
- combined encounter/cinematic spikes are included in profiling scenes;
- quality profiles preserve gameplay state and critical feedback.

## Open locks

- numeric animation update/bone/actor ceilings;
- numeric VFX particle/draw/overdraw/light ceilings;
- numeric simultaneous voice/DSP/streaming ceilings;
- concrete final animation/VFX/audio record manifests and final art/audio IDs;
- platform-specific codec/residency choices;
- measured quality-profile thresholds and hardware targets.

These move to production lock only after representative assets and runtime telemetry exist.