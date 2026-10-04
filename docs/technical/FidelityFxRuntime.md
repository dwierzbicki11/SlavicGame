# FidelityFX / FSR 3 Vulkan runtime

SlavicGame has an opt-in AMD FidelityFX Super Resolution 3.1.4 temporal
upscaler for Vulkan on Windows and Linux. Frame generation is not implemented.

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
into `native/fidelityfx/`. Build again after installing it, then launch with
`SLAVICGAME_FSR3=1` (PowerShell: `$env:SLAVICGAME_FSR3='1'`).

The native context and dispatch path are implemented, but remain opt-in pending
hardware validation. When requested on Windows or Linux with a loadable provider, scene
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
SLAVICGAME_FSR3=1 ./run.sh
```

Build the game again after installing the provider so the `.so` and AMD license
are copied to its output. `run.sh` does this automatically. The source checkout
must be unmodified SDK v1.1.4 at commit
`c6efa6bf7f2027b3ec94f28578bb5965eabb9e55`; generated adaptations remain in the
build directory. `SLAVICGAME_FFX_SDK_DIR` and `SLAVICGAME_FFX_BUILD_DIR` optionally
override the source/build cache paths.

The bridge implements the upscale create/destroy/dispatch API subset used by
the renderer. Unsupported queries, configuration extensions and frame
generation are rejected. The loader checks its FSR version and native structure
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
visual quality on a Vega 7; the runtime remains opt-in until hardware validation.

## UI rule

Do **not** expose an FSR3 setting merely because temporal inputs or the DLL
exist. The option becomes user-visible only after:

- a FidelityFX context is created successfully;
- color/depth/motion/reactive resources are dispatched through FSR;
- output is presented upright on Vulkan;
- resize/history reset works;
- CI plus real runtime validation passes.

Until then FSR1/native presentation remains the production fallback.
