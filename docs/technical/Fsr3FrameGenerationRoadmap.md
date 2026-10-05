# Linux / Vulkan FSR 3.1.4 Frame Generation

Updated: 2026-10-05. This is the checkpoint for the dedicated FG work. A stage
is complete only after its implementation, relevant runtime proof, Linux and
Windows CI on the exact PR head, and merge into main. Offscreen generation is
not presentation and does not establish FPS or physical-GPU visual quality.

## Coordination and restart

Start from current `main`, inspect open renderer/FG PRs and their exact heads,
Actions, and this file on the active branch. Continue the active PR before
opening another. Do not depend on retained local files.

Active stage: **3 — presentation and spacing**, branch
`fsr3/fg-presentation`, based on current main
`91172e7dc1e4a8ac008b4998acaf58facecffd7f` (stage 2 merged).
Stages 0–2 are complete; continue this presentation branch before opening
another FG PR. The adapter uses #319's validated managed runtime; do not
restore the superseded duplicate #306 ABI.

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

## 2. Actual HUD-free scene input — complete

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
- Runtime defect and repair: the initial actual scene had finite inputs but
  2087 NaN RGB pixels at 640x360. Intermediate readback found every inpainting
  pyramid mip had zero support; AMD's normalization then divided by zero.
  SPIR-V reflection had classified Vulkan 1.0 `Uniform + BufferBlock` SSBOs as
  constant buffers, and did not distinguish `NonWritable` storage reads from
  writes. Reflection now recognizes both Vulkan 1.0 and 1.1 storage encodings
  and maps the SDK's read/write counters correctly. A CTest compiles real GLSL
  for both API targets and verifies distinct constant/read/write descriptors.
  The generated-header include path is corrected. Auditable optical-flow
  guards avoid zero-weight division and negative fractional powers; the
  original AMD interpolation and color inpainting run without debug overrides.
  The pinned SDK checkout and AMD license remain intact.
- Local evidence (2026-10-05): native Release build, descriptor-contract CTest
  and GPU smoke pass; Release game/renderer-test builds pass. Full scene proof
  passes **47 checks**, including 12 moving frames beyond AMD's ten-frame
  warmup, finite generated pixels on every frame, actual depth/motion, HUD/menu
  exclusion, reset, 640x360 → 800x450, render 720x405 → 640x360, FSR1/FSR3,
  pending-work cleanup and FG-only failure isolation. At frame 12 mean RGB
  differences were 0.0171683 between input frames, 0.0104042 generated/current,
  and 0.0105229 generated/previous. Existing regressions pass **1259 checks**.
  This is software Vulkan (llvmpipe LLVM 20.1.2 / Mesa 25.2.8), with no physical
  GPU or extra swapchain presentation proof. One earlier local process exited
  with SIGSEGV; four subsequent complete processes passed all 47 checks. CI
  runs two fresh scene-proof processes and fails on either failure, with no
  success-by-retry. Fresh CI reproduced SIGSEGV despite the successful local
  processes; those passes alone do not close the lifecycle gate.
- Initial CI: [run 37270745485](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37270745485),
  exact initial head above: Windows green; Linux scene proof failed on the
  2087-pixel assertion. The assertions were preserved and extended. Final fix
  revision and Linux/Windows CI are recorded in this PR before merge.
- Repaired-reflection head: `53ef553a1d4fe66002385748616561a0ba13122b`;
  [CI 37274239005](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37274239005):
  Windows, native CTests and 1259 regressions pass, but Linux's scene proof
  crashes after the 800x450 FSR1 recovery while changing render size. Local
  reproduction also crashes on the second renderer's initialization. A native
  trace places the fault (address 0x40) in lavapipe's GPU worker. Vulkan
  validation identifies `vkDestroyImage-image-01000`: RecreateSceneTarget
  creates a color image, queues its initialization clear, then immediately
  deletes it in the single-sample branch and creates another. The repair
  allocates exactly one color image with its final sampling usage, preserving
  MSAA behavior. Validation also reports SPIR-V 1.5 on a Vulkan 1.1 instance;
  scene shader compilation now targets 1.0, including the old-device fallback.
- Validation follow-up: head `171ccf462d8e497b888eb011fee50933253f5e12`,
  [CI 37276450036](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37276450036),
  completes both 47-check scene processes and Linux/Windows jobs. It is **not
  mergeable evidence**: validation logs report invalid motion formats, view
  retirement and FSR3 output layout. The shell log gate incorrectly accepted
  `rg: command not found` inside an `if`. A Python gate now fails on every
  validation error or a missing completed proof; its rejection was verified
  against the recorded failing log. No assertions or validation errors are
  excluded. The branch additionally repairs Veldrid's RG16F-to-RGBA16F mapping,
  which both created invalid AMD views and overran the RG16F staging buffer;
  retains prepare's views without advancing the SDK ring a second time in the
  same submitted FG frame; queues an explicit native FSR3 write-to-sample
  transition; and stops calling optional debug-utils labels whose instance
  extension was not enabled. Six additional scene submissions without
  intermediate readbacks expose view retirement hidden by readback fences.
  Local Release builds, both native CTests, all 1261 regressions on current
  main, and two complete 47-check scene
  processes now pass with zero Khronos validation errors, including the six
  pending submissions without readback. Exact-head Linux/Windows CI is still
  required before merge. No physical GPU or swapchain-presentation evidence yet.
- Final evidence: [PR #324](https://github.com/dwierzbicki11/SlavicGame/pull/324),
  exact head `3583254590fc11388d4cb70c80fcf578233172c6`,
  [CI 37279361654](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37279361654)
  Linux/Windows green. Logs confirm both native CTests, all 1261 regressions,
  and two complete 47-check scene processes with zero validation errors.
  Motion difference 0.0171683, generated/current 0.0109382,
  generated/previous 0.0109037. Merged on 2026-10-05 as
  `91172e7dc1e4a8ac008b4998acaf58facecffd7f`;
  [main CI 37279976589](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37279976589)
  is also green on Linux/Windows.
- Blocker: none for scene generation. No extra presentation or physical GPU
  performance/quality evidence is claimed by this stage.
- Next: stage 3 actual window presentation.

## 3. Presentation and spacing — active, runtime/CI gate

- Dependencies: stage 2 merged.
- Exit criterion: real swapchain acquire/submit/present shows interpolated then
  rendered frames with useful spacing, correct fences/semaphores and VSync.
  Simulation/input advance once per rendered frame. No per-frame device idle.
- Evidence required: count/order of actual presented frames, moving image,
  reset suppression and pacing timestamps, runtime SHA/PR and Linux/Windows CI.
- Implementation checkpoint: the Veldrid source patch adds a checked combined
  submit/present endpoint. The final draw signals a per-swapchain-image binary
  semaphore; present waits on it, including when graphics/present queues differ.
  Acquisition completes through the existing host fence. Resize retains old
  swapchains/semaphores until the first new present is proven complete by
  reacquiring that image, following the Khronos recreation sample. FG uses
  FIFO for VSync and IMMEDIATE where supported without VSync, avoiding MAILBOX
  replacement of intermediate frames. There is no ordinary-frame device idle.
  Renderer composes AMD's generated image then the real image into two actual
  acquired images, drawing the same current HUD/menu separately. The pacer
  enforces half-frame deadlines with monotonic timestamps; its preceding wait
  is excluded from the cadence estimate. Simulation/input run once per scene.
  This conservative CPU pacing adds latency; no performance gain is claimed.
- Runtime executable: `tests/renderer` with `--presentation` creates real SDL
  windows and Vulkan swapchains, moves the production scene, records accepted
  vkQueuePresentKHR outputs, copies those very images before presentation,
  and checks order/count, distinct interpolation, spacing, reset and resize
  with VSync off/on. Xvfb/lavapipe validates WSI execution, not physical scanout
  or Vega 7 FPS. Existing offscreen proofs and Windows CI remain required.
- Local checkpoint (2026-10-05): Release game and renderer-proof compilation
  pass. Local WSI execution is blocked before Vulkan surface creation because
  the sandbox prohibits X11 listening sockets: Xvfb reports "Cannot establish
  any listening sockets" and SDL falls back to its offscreen video driver.
  This is not a passing window proof. The Linux Actions job installs Xvfb/SDL,
  requires the X11 driver and runs the same production-window executable under
  Khronos validation. No tests are skipped or weakened to bypass this limit.
- Blocker: compile and execute the new presentation proof, inspect all Vulkan
  validation output, then require green Linux/Windows CI on the exact PR head.
- Next: continue this branch; repair observed runtime/CI errors before merge.

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
- Blocker: stages 3–5 incomplete. This environment has no `/dev/dri` physical
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
