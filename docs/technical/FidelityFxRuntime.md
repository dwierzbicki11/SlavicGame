# FidelityFX / FSR 3 Vulkan runtime

SlavicGame is preparing a real AMD FidelityFX Super Resolution 3 temporal
upscaling path rather than relabelling the existing FSR1 shader.

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

Run tools/setup-fidelityfx.ps1. The helper downloads AMD's official v1.1.4
release and extracts amd_fidelityfx_vk.dll into native/fidelityfx/.

### Linux / Vulkan

Run tools/setup-fidelityfx.sh. AMD does not ship the v1.1.4 Vulkan provider
as a native Linux .so. The helper therefore fetches the exact v1.1.4 source
instead of trying to load the Windows DLL. The renderer remains on FSR1 until
the source-built Linux bridge has been compiled and validated.

## UI rule

Do **not** expose an FSR3 setting merely because temporal inputs or the DLL
exist. The option becomes user-visible only after:

- a FidelityFX context is created successfully;
- color/depth/motion/reactive resources are dispatched through FSR;
- output is presented upright on Vulkan;
- resize/history reset works;
- CI plus real runtime validation passes.

Until then FSR1/native presentation remains the production fallback.
