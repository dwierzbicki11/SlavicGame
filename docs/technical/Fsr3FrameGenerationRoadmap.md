# Linux / Vulkan FSR 3.1.4 Frame Generation

Updated: 2026-10-05. This is the checkpoint for the dedicated FG work. A stage
is complete only after its implementation, relevant runtime proof, Linux and
Windows CI on the exact PR head, and merge into main. Offscreen generation is
not presentation and does not establish FPS or physical-GPU visual quality.

## Coordination and restart

Start from current `main`, inspect open renderer/FG PRs and their exact heads,
Actions, and this file on the active branch. Continue the active PR before
opening another. Do not depend on retained local files.

Active stage: **2 — actual HUD-free scene input**, branch
`fsr3/fg-scene-input`, based on current main
`4055b885800d51a5e956732e5e81dac38e0b3f5b`. PR #306 is merged. Continue
this stage's PR before starting presentation work. Its adapter uses #319's
validated managed runtime; do not restore the superseded duplicate #306 ABI.

Graphics Loop's `perf/menu-retired-buffer` was at
`8e467a13b7fb23c2b1acabb59bd9548bc566adbd` when checked on 2026-10-05.
Its MenuRenderer work is not edited by this stage. Recheck the branch/open PRs
before later UI/presentation changes. No other automation is modified.

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

## 1. Production Vulkan 1.1 and enabled device features — complete

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
- Evidence: implementation head `c8360ec0177ad2a97bf2b5582065ecf3d28efbff`,
  source tree `5283d970dfeba313045128baf10bd316d4a36e60`.
  [Linux/Windows CI](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37267965134)
  is green on that exact head. The Linux job logs confirm the pinned source
  build, real production-device moving-image FG, orientation, API/feature
  rejection and all **1259 checks passed**. Upscaling readback passes on both
  1.0 and 1.1. Local Release builds and native `ctest` also pass.
- Final head: `68147474d465a6e737e48532e9f4bb508bcbfd9f`,
  [Linux/Windows CI](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37268267371)
  green on that exact head. Merged PR #306 as
  `4055b885800d51a5e956732e5e81dac38e0b3f5b` on 2026-10-05.
- Blocker: none for stage 1.
- Next: stage 2. Veldrid's layout transitions submit on the same
  graphics queue without device-idle waits. Its swapchain presentation is
  separate from scene/FG compute, so scene wiring must not be mistaken for
  extra-frame presentation. Keep Graphics Loop's MenuRenderer work separate.

## 2. Actual HUD-free scene input — active, runtime/CI gate

- Dependencies: stage 1 merged.
- Exit criterion: renderer supplies display-resolution scene color, render
  depth/motion, layouts/formats, camera/jitter, monotonic 64-bit frame ID and
  frame time to the #319 runtime. Same graphics queue orders scene → FG →
  consumer; no per-frame device-idle. FG remains independent of upscaling.
- Implementation: `FidelityFxSceneFrameGeneration` owns RGBA16F display-sized
  scene/generated images, adapts actual Veldrid depth/motion/layouts to #319,
  and retires images only after producer/consumer completion. VeldridRenderer
  orders scene → optional native upscale → HUD-free spatial/display capture →
  AMD FG → real-frame scene composition → HUD/menu on the same graphics queue.
  64-bit scene IDs advance once per renderer call; camera/history, upscale-mode
  and render/display-size changes reset FG. Failures disable only FG.
- Developer validation: `SLAVICGAME_FSR3_FG_SCENE=1` dispatches FG on actual
  game scene frames. This is deliberately **offscreen generation**, not a
  shipping FG setting or extra-frame presentation. Default rendering is unchanged.
- Runtime executable: `tests/renderer/SlavicGame.RendererTests.csproj` references
  the game assembly and runs the full production scene/HUD renderer into an
  externally owned framebuffer. Only the window/acquire/present endpoint is
  absent. Checks include movement, interpolation distinct from both adjacent
  images, real geometry depth/camera velocity, menu exclusion, resets, grow/
  shrink and render-size changes, spatial FSR1, native FSR3 and failure isolation.
  CI runs this proof on Linux; existing Linux/Windows regressions remain required.
- Initial runtime finding: actual scene/depth/motion are finite, but the native
  generated image had 2087 nonfinite pixels at 640x360. Constant-image tests
  did not detect this. Investigation isolated the issue before color inpainting.
  The SDK optical-flow vector-field filter also divides a zero-weight average and
  evaluates `pow(negative dot product, 1.25)` before clamping. An auditable,
  generated-include Linux adaptation guards both operations, preserving AMD's
  interpolation and valid filter weights. No copied-frame or lerp FG substitute
  is used. These guards have not removed the 2087-pixel failure; the remaining
  cause must be isolated from intermediate native resources. The pinned SDK
  checkout is unmodified; its license is retained.
- Evidence: native Release build / smoke passed; Release game and renderer-test
  builds passed. Full moving-scene runtime is being rerun after the numerical
  repair. PR/head and final Linux/Windows CI must be recorded before completion.
- Blocker: complete the moving-scene runtime checks, fix any failures, run full
  regressions and obtain green CI on the exact merge head.
- Next: continue this branch's tests/PR; only after merge, stage 3 presentation.

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
SLAVICGAME_TEST_FSR3_NATIVE=1 dotnet run --project tests/renderer/SlavicGame.RendererTests.csproj -c Release
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
