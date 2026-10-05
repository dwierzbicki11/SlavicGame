# Linux / Vulkan FSR 3.1.4 Frame Generation

Updated: 2026-10-05. This is the checkpoint for the dedicated FG work. A stage
is complete only after its implementation, relevant runtime proof, Linux and
Windows CI on the exact PR head, and merge into main. Offscreen generation is
not presentation and does not establish FPS or physical-GPU visual quality.

## Coordination and restart

Start from current `main`, inspect open renderer/FG PRs and their exact heads,
Actions, and this file on the active branch. Continue the active PR before
opening another. Do not depend on retained local files.

Current stage: **1 — production Vulkan 1.1 device creation**, PR **#306**,
branch `render/fsr3-frame-generation-runtime`. It has been synchronized with
main `8e467a13b7fb23c2b1acabb59bd9548bc566adbd` (PR #319). The older #306
wrapper/ABI is superseded by the validated #319 managed runtime; do not restore
the duplicate ABI structs. Stage 2 will adapt the game to that current runtime.

Graphics Loop currently works on `perf/menu-retired-buffer` and MenuRenderer
buffer retirement. FG stage 1 does not edit MenuRenderer or its resource
lifetime policy. Recheck this before later HUD/presentation changes.

## 0. Existing generator and managed command ring — complete

- Dependencies: source-built AMD SDK 1.1.4 / FSR 3.1.4 provider.
- Exit criterion: real prepare, optical flow, interpolation, managed ABI and
  GPU readback, history/reset/recreation, three-slot command/fence reuse.
- Evidence: PR #319, head `e39c8cc00bdc561c3f1fe07a40834ebb7c6e2ca8`, merged
  as `8e467a13b7fb23c2b1acabb59bd9548bc566adbd`.
  [CI](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37261176800),
  Linux/Windows green; 1224 checks. 128x128 → 160x96 → 128x128 readback.
- Blocker: none for offscreen generation; production instance was Vulkan 1.0.
- Next: stage 1; do not reimplement this foundation.

## 1. Production Vulkan 1.1 and enabled device features — active

- Dependencies: stage 0.
- Implementation: a pinned source build of Veldrid 4.9.0 adds instance-version
  selection and actual creation metadata. Linux selects 1.1 only when the
  loader supports it; an incompatible old ICD retries 1.0. Windows preserves
  its 1.0 contract. Extended storage image formats are explicitly enabled when
  supported, and recorded separately from physical-device advertised support.
  Required instance/device creation errors are checked in Release as well.
- Gate: the GraphicsDevice entry point rejects a 1.0 instance, a 1.0 physical
  device or a disabled storage feature before calling FG's native capability
  query. The native provider then checks compute subgroup basic/arithmetic.
- Exit criterion: renderer and regression use the same VulkanDeviceFactory;
  actual 1.1 Veldrid FG dispatch/readback, moving asymmetric input, API/feature
  rejection and unchanged upscaling on 1.0 and 1.1; green Linux/Windows CI;
  merge PR #306.
- Evidence: local Release game/test builds and native `ctest` pass. Production
  Veldrid device generation/readback passes on lavapipe, including the moving
  image and orientation checks; 1259 regression checks pass. Upscaling passes
  on both Vulkan 1.0 and the production 1.1 factory. PR #306's final head and CI must be recorded in its reviewable
  description before merge; the previous #306 CI does not validate this code.
- Blocker: awaiting fresh Linux/Windows CI and merge on the exact PR head.
- Next: finish those checks and merge; then start stage 2 on the merged head.

## 2. Actual HUD-free scene input — pending

- Dependencies: stage 1 merged.
- Exit criterion: renderer supplies display-resolution scene color, render
  depth/motion, layouts/formats, camera/jitter, monotonic 64-bit frame ID and
  frame time to the #319 runtime. Same graphics queue orders scene → FG →
  consumer; no per-frame device-idle. FG remains independent of upscaling.
- Evidence required: actual renderer frames with movement, nonconstant output,
  resets, output lifetime, resize, failure isolation. Record SHA/PR/CI here.
- Blocker: stage 1 not yet merged; prior #306 adapter used obsolete ABI and
  tied FG to the FSR3 upscaler, so it is not an acceptable completed stage.
- Next: implement a Veldrid texture/layout adapter for FidelityFxFrameGeneration
  and a HUD-free display-color path for all supported scene upscaling modes.

## 3. Presentation and spacing — pending

- Dependencies: stage 2 merged.
- Exit criterion: real swapchain acquire/submit/present shows interpolated then
  rendered frames with useful spacing, correct fences/semaphores and VSync.
  Simulation/input advance once per rendered frame. No per-frame device idle.
- Evidence required: count/order of actual presented frames, moving image,
  reset suppression and pacing timestamps, runtime SHA/PR and Linux/Windows CI.
- Blocker: no generated-frame presentation in main.
- Next: choose and implement the presentation path around actual Veldrid
  swapchain ownership; do not merely present two frames back to back.

## 4. UI composition and resource/history lifecycle — pending

- Dependencies: stage 3 merged; coordinate MenuRenderer retirement work.
- Exit criterion: compose HUD/menu separately on each displayed image. Handle
  resize, resolution/quality changes, fullscreen, pause/resume, camera jumps,
  gaps, reset/recovery and safe resource retirement while GPU work is pending.
- Evidence required: UI excluded from interpolation; runtime transitions and
  cleanup under pending GPU work; SHA/PR/CI.
- Blocker: no split scene/generated/present UI path yet.
- Next: reuse UI geometry/update once, compose into both displayed frames.

## 5. Working saved FG option — pending

- Dependencies: stages 3 and 4 merged.
- Exit criterion: saved menu option actually enables generation + presentation;
  status/reason reflects enabled state. FG failure disables only FG, leaving
  FSR3 upscaling or the ordinary presentation fallback functional.
- Evidence required: off/on/reload/recovery, unavailable-device reasons,
  presented-frame counts, SHA/PR/CI. A developer offscreen toggle is insufficient.
- Blocker: presentation/UI path not yet implemented.
- Next: add the option after its complete runtime behavior is available.

## 6. Final validation and user build instructions — pending

- Dependencies: all prior stages merged.
- Exit criterion: real scene motion/interpolation, frame order/count, distinct
  rendered/presented FPS, resets, resize/toggle, cleanup and fallback pass;
  required Linux/Windows CI is green on final main. AMD licenses retained.
- Evidence required: runtime logs/artifacts, final SHA/PR/CI and an explicit
  distinction between software-Vulkan proof and physical-GPU validation.
- Blocker: stages 2–5 incomplete. This environment has no `/dev/dri` physical
  GPU; lavapipe validates Vulkan execution, not Vega 7 performance or quality.
- Next: use the available runtime tests, then execute the physical scenario
  below when the final FG option exists. Do not invent FPS or claim completion
  from constant-color offscreen output.

## Reproducible Linux Mint / Ryzen 5 5600G (Vega 7) scenario

Install .NET 11, Git, Python 3, CMake, g++, libvulkan-dev, Mesa Vulkan drivers,
glslang-tools and vulkan-tools. Clone current main, then run:

```bash
bash tools/setup-fidelityfx.sh --test
dotnet build SlavicGame.csproj -c Release
SLAVICGAME_TEST_FSR3_NATIVE=1 dotnet run --project tests/SlavicGame.RegressionTests.csproj -c Release
vulkaninfo --summary
dotnet run --project SlavicGame.csproj -c Release
```

The Veldrid source patch prepares automatically from its pinned commit;
details and offline-source override are in `third_party/veldrid/README.md`.
Do not set a lavapipe ICD override for the physical test. Capture device/driver,
creation-version/features diagnostics, resolution, render scale and settings.
After stage 5, compare FG off/on while moving through the village/forest and
turning the camera. Record rendered and presented FPS separately, cadence,
input response, moving silhouettes/vegetation, disocclusions and UI clarity.
Repeat with VSync on/off, FSR3 and ordinary scaling, resolution/fullscreen
changes, pause/resume and FG off/on. Check reset frames are not interpolated
and normal rendering continues after a rejected/failed FG context.

No physical Vega 7 result has been recorded as of this checkpoint.
