# Linux / Vulkan FSR 3.1.4 Frame Generation

Updated: 2026-10-05. This is the checkpoint for the dedicated FG work. A stage
is complete only after its implementation, relevant runtime proof, Linux and
Windows CI on the exact PR head, and merge into main. Offscreen generation is
not presentation and does not establish FPS or physical-GPU visual quality.

## Coordination and restart

Start from current `main`, inspect open renderer/FG PRs and their exact heads,
Actions, and this file on the active branch. Continue the active PR before
opening another. Do not depend on retained local files.

FG roadmap status: **stages 0–6 complete**. Final integration lives in
[PR #337](https://github.com/dwierzbicki11/SlavicGame/pull/337), branch
`fsr3/fg-final-validation`, based on main after stage 5. PR #337 is merged
only after Linux/Windows CI on its exact final head is green; the PR checks are
the authoritative final merge record.

Stage 5 merged through [PR #336](https://github.com/dwierzbicki11/SlavicGame/pull/336)
as `ccf1eee9361da3822a518c43814b09d6da1c1c43`. Its exact-head
[CI 37308987683](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37308987683)
is green on Linux/Windows: **1277 regressions**, two **47-check** scene proofs,
a **334-check** real-window presentation proof and **zero Vulkan validation
errors**. The shipping setting defaults OFF and distinguishes saved request
from active/restart-required/unavailable/failed runtime state. The adapter uses
#319's validated managed runtime; do not restore the superseded duplicate #306
ABI. No other automation is modified.

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

## 3. Presentation and spacing — complete

- Dependencies: stage 2 merged.
- Implementation: the production Linux/Vulkan renderer presents the
  AMD-generated frame before its matching rendered frame through real
  swapchain acquire/submit/present. Per-image render-finished semaphores,
  FIFO/IMMEDIATE present modes and monotonic half-frame CPU deadlines preserve
  ordering without a second simulation/input update or ordinary-frame
  device-idle.
- Runtime proof: the SDL/Xvfb/lavapipe window test captures the actual
  swapchain images before presentation and validates frame order/count,
  distinct moving interpolation, spacing, reset suppression, resize and VSync
  off/on. Synchronization validation is mandatory.
- Evidence: [PR #331](https://github.com/dwierzbicki11/SlavicGame/pull/331),
  exact head `56f2e0c07cdb75b34f5abade50e0551f603c0558`,
  [CI 37288592009](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37288592009)
  Linux/Windows green. Linux proves 17 rendered + 14 generated = 31 accepted
  presentations per VSync mode with zero basic/synchronization validation
  errors. Merged as `9879b7062713c2accb3694c6b28cb9248be705c2`.
- No physical scanout or Vega 7 performance result is claimed.
- Blocker: none.
- Next: stage 4.

## 4. UI composition and resource/history lifecycle — complete

- Dependencies: stage 3 merged.
- Implementation: menu geometry is prepared/uploaded once per simulation frame
  and drawn unchanged on generated and rendered presentations; hidden menu
  frames perform no menu-buffer upload. HUD/menu remain outside AMD
  interpolation.
- Lifecycle: menu/pause transitions, large camera/FOV cuts, VSync changes,
  resize and explicit resets suppress stale interpolation. Runtime FG failure
  destroys only interpolation resources; the existing semaphore-correct WSI
  presenter stays alive and presents rendered-only frames for the remainder of
  that renderer session, avoiding an unsafe mid-session synchronization-model
  switch. Shutdown/restart retires the presenter normally.
- Runtime proof extends the actual SDL/Vulkan WSI test with prepare-once /
  draw-twice UI assertions, pause/resume, camera cut and live generator failure
  recovery. The proof also retires every staging capture and checks device
  destruction under validation.
- Evidence: [PR #335](https://github.com/dwierzbicki11/SlavicGame/pull/335),
  exact head `f1c0cb457cf3072cd6cbe29ee74cf606b2574760`,
  [CI 37308453018](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37308453018)
  Linux/Windows green. Linux: **1267 regressions**, two **47-check** production
  scene proofs, **334-check** real-window presentation proof and **zero Vulkan
  validation errors**. Merged as
  `a34573c23f307bd8947d2ee8a9c99abf67beaa71`.
- Blocker: none.
- Next: stage 5.

## 5. Working saved FG option — complete

- Dependencies: stage 4 merged.
- Implementation on PR #336: persisted `GameSettings.FrameGeneration`,
  default OFF; `FRAME GENERATION: OFF/ON` in Post Processing; restart-required
  application; production renderer startup uses the saved setting rather than
  the developer scene-validation environment variable.
- Runtime truth is transient and not serialized. The frontend distinguishes
  `OFF`, `ON (RESTART)`, `ON`, `NIEDOST.` and `BLAD`; the detailed
  diagnostic reason remains available from the renderer/log. A rejected or
  failed FG request does not disable native FSR3 upscaling or normal rendering.
- The real-window proof clears the developer FG environment switch and enables
  FG through the same saved-option startup path used by the game.
- Evidence: [PR #336](https://github.com/dwierzbicki11/SlavicGame/pull/336),
  exact head `0f870dfcdd3ae5557ec02942f0106f6f6a2caea4`,
  [CI 37308987683](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37308987683)
  Linux/Windows green. Linux reports **1277 checks passed**, two **47-check**
  scene proofs, a **334-check** actual SDL/Vulkan presentation proof and
  **zero Vulkan validation errors**. Merged as
  `ccf1eee9361da3822a518c43814b09d6da1c1c43`.
- Blocker: none.
- Next: stage 6.

## 6. Final validation and user build instructions — complete

- Dependencies: all prior stages merged.
- Exit criterion: real scene motion/interpolation, frame order/count, distinct
  rendered/presented FPS, resets, resize/toggle, cleanup and fallback pass;
  required Linux/Windows CI is green on final main. AMD licenses retained.
- Evidence required: runtime logs/artifacts, final SHA/PR/CI and an explicit
  distinction between software-Vulkan proof and physical-GPU validation.
- Current implementation adds a real production-default OFF startup proof with
  the developer FG override cleared: no generator and no special WSI presenter
  may be allocated until the saved setting requests FG.
- Documentation and setup helpers now describe the shipping Linux enable path:
  install/build the source provider, choose FSR3, choose Frame Generation ON,
  restart. Windows is explicitly temporal-FSR3-only for now and reports FG as
  unavailable.
- Implementation evidence: [PR #337](https://github.com/dwierzbicki11/SlavicGame/pull/337),
  implementation head `f216becdeb3351e9aba3d5d5c1d6684e4e46c9d6`,
  [CI 37311113522](https://github.com/dwierzbicki11/SlavicGame/actions/runs/37311113522)
  Linux/Windows green. Linux reports **1277 regression checks**, two complete
  **47-check** scene proofs, explicit
  `Saved/default Frame Generation OFF creates no FG runtime or special presenter`,
  a **335-check** actual SDL/Vulkan presentation proof and **zero Vulkan
  validation errors**.
- Final merge policy: #337 is merged only after its exact final head passes
  Linux/Windows CI, both scene/window proofs and zero-error Vulkan validation.
  The final PR check suite is the authoritative merge record.
- This environment has no `/dev/dri` physical GPU; lavapipe validates Vulkan
  execution/presentation and lifecycle, not Vega 7 performance, physical
  scanout cadence or visual quality.
- Next after merge: execute the physical scenario below on Ryzen 5 5600G /
  Vega 7 and record measured FPS/cadence/artifacts without inventing results.

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
Compare FG off/on while moving through the village/forest and
turning the camera. Record rendered and presented FPS separately, cadence,
input response, moving silhouettes/vegetation, disocclusions and UI clarity.
Repeat with VSync on/off, FSR3 and ordinary scaling, resolution/fullscreen
changes, pause/resume and FG off/on. Check reset frames are not interpolated
and normal rendering continues after a rejected/failed FG context.

No physical Vega 7 result has been recorded as of this checkpoint.
