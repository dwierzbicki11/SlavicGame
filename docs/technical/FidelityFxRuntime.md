# FidelityFX / FSR 3 Vulkan runtime

SlavicGame integrates AMD FidelityFX Super Resolution 3.1.4 temporal
upscaling for Vulkan on Windows and Linux. It is selectable in the in-game
UPSCALER setting as FSR3. Frame generation can dispatch to offscreen images
through the Linux native and C# runtimes; extra frames are not yet presented
by the game.

## Pinned SDK

- AMD FidelityFX SDK **1.1.4**
- FSR **3.1.4**
- Vulkan backend

The pin is deliberate. The newer FSR SDK line currently does not provide the
same Vulkan integration path, while SDK 1.1.4 is the last v1 release with the
Vulkan FSR 3.1 integration used by this renderer.

## Runtime layers

1. TemporalFrameState — jitter/history matrices.
2. MotionVectorRenderer — camera + real dynamic object velocity.
3. ReactiveMaskRenderer — water, transient and spectral temporal response.
4. FidelityFxVulkanInterop — documented Veldrid Vulkan handles consumed by
   the native FidelityFX dispatch layer.

FidelityFxNativeLibrary loads the ABI-stable five-function FidelityFX API at
runtime and validates ffxCreateContext, ffxDestroyContext, ffxDispatch,
ffxQuery and ffxConfigure before the renderer may advertise FSR3.

## Platform policy

### Windows / Vulkan

Run `powershell -ExecutionPolicy Bypass -File tools/setup-fidelityfx.ps1`.
The helper downloads AMD's official signed v1.1.4 Vulkan provider directly
into `native/fidelityfx/`. Build again after installing it, then select **FSR3** in the graphics settings.
`SLAVICGAME_FSR3=1` (PowerShell: `$env:SLAVICGAME_FSR3='1'`) remains a
developer override that forces the temporal path regardless of the saved menu
selection.

When FSR3 is selected on Windows or Linux with a loadable provider, scene
MSAA is disabled for that run: FSR supplies temporal AA and needs single-sample
depth. The saved MSAA preference is preserved. If initialization or dispatch
fails, spatial presentation resumes and unused temporal passes stop, unless
`SLAVICGAME_TEMPORAL_INPUTS=1` explicitly requests input validation.

The Vulkan command ring waits only for successfully submitted work. A failed
recording or queue submission cannot make shutdown wait on a fence that no GPU
submission can signal.

### Linux / Vulkan

AMD's v1.1.4 packaged Vulkan provider is a Windows DLL. On Linux the helper
builds `libslavic_fsr3_vk.so` from the pinned AMD sources and installs it under
`native/fidelityfx/`. On Debian, Ubuntu or Mint:

```bash
sudo apt install git cmake g++ python3 libvulkan-dev glslang-tools mesa-vulkan-drivers
bash tools/setup-fidelityfx.sh --test
./run.sh
# Then choose UPSCALER -> FSR3 in the graphics menu.
```

Build the game again after installing the provider so the `.so` and AMD license
are copied to its output. `run.sh` does this automatically. The source checkout
must be unmodified SDK v1.1.4 at commit
`c6efa6bf7f2027b3ec94f28578bb5965eabb9e55`; generated adaptations remain in the
build directory. `SLAVICGAME_FFX_SDK_DIR` and `SLAVICGAME_FFX_BUILD_DIR` optionally
override the source/build cache paths.

The bridge implements the upscale create/destroy/dispatch API subset used by
the renderer. Unsupported queries, configuration extensions and frame
generation through that five-function upscale API are rejected. FG instead uses
the independent `slavicFgCreate/Prepare/Dispatch/Destroy` bridge. The loader checks its FSR version and native structure
sizes/offsets against C# marshalling before enabling Linux dispatch.

The embedded shaders use FP32 and AMD's shared-memory SPD variant, compiled for
Vulkan 1.0 to match Veldrid 4.9. They require neither float16 nor subgroup-size
features. The audited source adaptations reserve enough opaque context space
for Linux's native `wchar_t`, align SDK scratch slices to 32 bytes, accept UMA
device-local memory, require all requested memory flags, and avoid assuming
advertised optional Vulkan features are enabled by the engine.
The renderer also resolves the Vk package's legacy `libdl` import to
`libdl.so.2` on Linux, so current glibc systems need no manual symlink.

`--test` executes actual FSR shaders and validates a readback at two sizes,
including temporal history and sharpening. CI additionally dispatches through
the game's C#/Veldrid adapter, reads asymmetric color quadrants, and checks
orientation, context growth and history reset. Mesa lavapipe runs these tests
without a window or physical GPU. These checks do not establish in-game FPS or
visual quality on a Vega 7; real-hardware validation is still required before
making FSR3 the default in any graphics preset.

## Frame generation stages

The Linux provider links AMD Frame Interpolation and Optical Flow and executes
their real GPU passes. `FidelityFxFrameGeneration` now consumes that bridge
from C#, validates the separate FG ABI and owns a three-slot Vulkan command
ring. It submits prepare, optical flow and interpolation on the supplied
graphics queue, without waiting for device idle each frame. Context recreation
and disposal wait for its submitted work before releasing native resources.

Inputs are HUD-free color at display resolution and depth/motion at render
resolution; all input images must be shader-read-only, with the output in
GENERAL. The caller owns texture lifetimes and layout tracking and submits on
the render thread. Device creation must enable `shaderStorageImageExtendedFormats`;
both the current Veldrid device factory and the fixture enable that core feature.
Physical-device Vulkan version, compute subgroup arithmetic and extended
storage-format support are checked before FG allocates resources. Camera vectors
come from the inverse view matrix, motion vectors are converted from UV to
render pixels, and jitter follows the same Y convention as the upscaler.
The returned interpolation eligibility is false after startup, explicit or
camera-history reset, a nonconsecutive frame ID, or any render/display size
change. The next consecutive successful dispatch becomes eligible again.

Optical-flow shaders require Vulkan 1.1 / SPIR-V 1.3. The managed runtime
rejects a requested Vulkan 1.0 instance even if the physical GPU advertises
a newer version. The pinned Veldrid 4.9 source patch lets the production Linux
factory request Vulkan 1.1 and records that actual instance version and enabled
storage feature. A 1.0 loader/ICD remains usable with FG unavailable; Windows
keeps its existing instance contract. The native test fixture
creates a Vulkan 1.1 device; C# performs the actual context creation, prepare,
dispatch and queue submission, and tests read back known color at two sizes
plus a shrink, history resets and fence-ring reuse. The fixture is test-only
and is not copied into the game's native runtime directory.

The production-device regression additionally runs actual FG with a moving,
asymmetric image through the same factory as the renderer and rejects a 1.0
instance or a supported-but-disabled storage feature. Upscaling is checked on
both 1.0 and 1.1 devices. See `Fsr3FrameGenerationRoadmap.md` for stage status.

Remaining stages are actual scene wiring, generated-frame presentation and
pacing, independent HUD composition, then an in-game FG
setting after those paths are validated. Offscreen checks establish ABI and
GPU dispatch correctness for the test scene, not FG visual quality or FPS on
a physical GPU.

## UI behavior

The graphics menu exposes **BILINEAR**, **FSR1** and **FSR3**. Switching the
upscaler is marked as requiring a restart because temporal FSR changes the
startup MSAA/depth policy and creates native FidelityFX resources.

If the FSR3 provider is unavailable, context creation fails or a dispatch fails,
the renderer falls back to the existing FSR1 presentation path for that run.
The saved FSR3 preference is preserved so it can become active after the native
provider is installed. The environment override remains available for
diagnostics and CI.
