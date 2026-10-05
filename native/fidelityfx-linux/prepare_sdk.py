#!/usr/bin/env python3
"""Auditable Linux adaptations of the exact SDK v1.1.4, in build output only."""
import sys
from pathlib import Path
sdk, output = map(Path, sys.argv[1:])
# The SDK sizes opaque contexts for Windows' 16-bit wchar_t. Linux uses
# 32-bit wchar_t and therefore needs extra room for pipeline/resource names.
# Keep this adaptation isolated in the build output and patch every context
# which participates in FSR3 Frame Generation.
context_headers = [
    ('ffx_fsr3upscaler.h',
     '#define FFX_FSR3UPSCALER_CONTEXT_SIZE (FFX_SDK_DEFAULT_CONTEXT_SIZE)',
     '#define FFX_FSR3UPSCALER_CONTEXT_SIZE (2 * FFX_SDK_DEFAULT_CONTEXT_SIZE)'),
    ('ffx_frameinterpolation.h',
     '#define FFX_FRAMEINTERPOLATION_CONTEXT_SIZE (FFX_SDK_DEFAULT_CONTEXT_SIZE)',
     '#define FFX_FRAMEINTERPOLATION_CONTEXT_SIZE (2 * FFX_SDK_DEFAULT_CONTEXT_SIZE)'),
    ('ffx_opticalflow.h',
     '#define FFX_OPTICALFLOW_CONTEXT_SIZE (FFX_SDK_DEFAULT_CONTEXT_SIZE)',
     '#define FFX_OPTICALFLOW_CONTEXT_SIZE (2 * FFX_SDK_DEFAULT_CONTEXT_SIZE)'),
]
for filename, old, new in context_headers:
    header = (sdk / 'sdk/include/FidelityFX/host' / filename).read_text()
    assert header.count(old) == 1, f'Unexpected SDK context definition in {filename}'
    header = header.replace(old, new)
    path = output / 'include/FidelityFX/host' / filename
    path.parent.mkdir(parents=True, exist_ok=True)
    if not path.exists() or path.read_text() != header:
        path.write_text(header)
# Opposing optical-flow vectors are normal at disocclusions. The SDK's GLSL
# port raises their negative dot product to the fractional power 1.25 before
# clamping, and divides the first average by zero when all weights are zero.
# Both operations can produce NaN, which then poisons packed vector fields
# and the interpolated scene. Preserve AMD's filter on valid weights; zero
# support contributes zero, and negative agreement contributes zero weight.
# Apply only to generated shader includes, never to the pinned SDK checkout.
filename = 'ffx_frameinterpolation_optical_flow_vector_field.h'
header = (sdk / 'sdk/include/FidelityFX/gpu/frameinterpolation' / filename).read_text()
old = '    fOpticalFlowVector3x3Avg /= sw;'
assert header.count(old) == 1, 'Unexpected optical-flow averaging site'
header = header.replace(old, '''    if (sw > FFX_FRAMEINTERPOLATION_EPSILON)
        fOpticalFlowVector3x3Avg /= sw;''')
old = 'ffxPow(dot(fOpticalFlowVector3x3Avg, vs), 1.25f)'
assert header.count(old) == 1, 'Unexpected optical-flow directional weight'
header = header.replace(old, 'ffxPow(ffxMax(0.0f, dot(fOpticalFlowVector3x3Avg, vs)), 1.25f)')
path = output / 'include/FidelityFX/gpu/frameinterpolation' / filename
path.parent.mkdir(parents=True, exist_ok=True)
if not path.exists() or path.read_text() != header:
    path.write_text(header)
source = (sdk / 'sdk/src/backends/vk/ffx_vk.cpp').read_text()
# The SDK treats each unregister as a frame boundary. FI prepare and dispatch
# are two operations in ONE submitted frame; retiring twice can destroy views
# still referenced by a pending command buffer. Only interpolation advances
# the SDK's four-slot view ring. The caller's three-slot fence ring waits for
# that frame before its views can be retired. The flag belongs to this backend
# scratch allocation, not global state, and is cleared after prepare, on error
# as well as success. It never suppresses barriers or resource unregistering.
old = '    uint32_t maxEffectContexts;'
assert source.count(old) == 1
source = source.replace(old, old + '\n    bool slavicPrepareDispatch;')
old = '''    // destroy the views of the next frame
    effectContext.frameIndex = (effectContext.frameIndex + 1) % FFX_MAX_QUEUED_FRAMES;'''
assert source.count(old) == 1
source = source.replace(old, '''    if (backendContext->slavicPrepareDispatch)
        return FFX_OK;

''' + old)
source += '''
// Linux provider owns submissions and fences; prepare is not a frame boundary.
static_assert(FFX_MAX_QUEUED_FRAMES >= 4, "Three submitted frames need four view slots");
void slavicFfxSetPrepareDispatchVK(FfxInterface* backend, bool preparing)
{
    static_cast<BackendContext_VK*>(backend->scratchBuffer)->slavicPrepareDispatch = preparing;
}
'''
# The API does not pass the enabled instance-extension list to this backend.
# A nonnull loader function is not proof VK_EXT_debug_utils was enabled.
# Keep optional native markers disabled until that contract exists.
for name in ['vkCmdBeginDebugUtilsLabelEXT', 'vkCmdEndDebugUtilsLabelEXT']:
    old = f'        backendContext->vkFunctionTable.{name} = (PFN_{name})vkDeviceContext->vkDeviceProcAddr(backendContext->device, "{name}");'
    assert source.count(old) == 1, f'Unexpected debug marker load site: {name}'
    source = source.replace(old, f'        backendContext->vkFunctionTable.{name} = nullptr;')
# EffectContext has alignas(32), but the upstream scratch layout aligns slices
# to only four bytes. Optimized GCC uses aligned stores and can crash. Align
# the allocation and every slice consistently in both sizing and mapping.
for begin, end in [
    ('FFX_API size_t ffxGetScratchMemorySizeVK(', '// Create a FfxDevice'),
    ('        // Map all of our pointers', '        // Map gpu job array')]:
    start = source.index(begin)
    stop = source.index(end, start)
    part = source[start:stop]
    assert part.count('sizeof(uint32_t))') == 6
    part = part.replace('sizeof(uint32_t))', 'size_t(32))')
    part = part.replace('sizeof(BackendContext_VK) +', 'FFX_ALIGN_UP(sizeof(BackendContext_VK), size_t(32)) +')
    part = part.replace('sizeof(uint64_t));', 'size_t(32));')
    part = part.replace('(uint8_t*)((BackendContext_VK*)(backendContext + 1))',
                        '(uint8_t*)backendContext + FFX_ALIGN_UP(sizeof(BackendContext_VK), size_t(32))')
    source = source[:start] + part + source[stop:]
# UMA devices may expose device-local memory as host-visible. FSR must permit
# this memory, and all requested flags must be present when choosing a type.
old = '(memProperties.memoryTypes[i].propertyFlags & requestedProperties))'
assert source.count(old) == 1
source = source.replace(old, '(memProperties.memoryTypes[i].propertyFlags & requestedProperties) == requestedProperties)')
old = '''            // if just device-local memory is requested, make sure this is the invisible heap to prevent over-subscribing the local heap
            if (requestedProperties == VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT && (memProperties.memoryTypes[i].propertyFlags & VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT))
                continue;
'''
assert source.count(old) == 1
source = source.replace(old, '')
# An advertised extension need not be enabled by the engine. Vulkan 1.1+ has
# core aliases for the memory requirements functions; use those when needed.
for name in ['vkGetBufferMemoryRequirements2']:
    line = f'        backendContext->vkFunctionTable.{name}KHR = (PFN_{name}KHR)vkDeviceContext->vkDeviceProcAddr(backendContext->device, "{name}KHR");'
    assert source.count(line) == 1, f'Unexpected SDK load site: {name}'
    source = source.replace(line, line + f'\n        if (!backendContext->vkFunctionTable.{name}KHR)\n            backendContext->vkFunctionTable.{name}KHR = (PFN_{name}KHR)vkDeviceContext->vkDeviceProcAddr(backendContext->device, "{name}");')
# This provider uses only Vulkan 1.0, FP32 and AMD's no-wave SPD permutation.
# Veldrid does not enable optional float16/subgroup/device-coherent features.
# Do not confuse advertised extensions with enabled features or call 1.1 core
# physical-device queries on its 1.0 instance. Keep the SDK's minimum defaults.
start = source.index('    // check if extensions are enabled\n', source.index('FfxErrorCode GetDeviceCapabilitiesVK(FfxInterface* backendInterface, FfxDeviceCapabilities* deviceCapabilities)\n{'))
end = source.index('    return FFX_OK;', start)
source = source[:start] + '''    deviceCapabilities->dedicatedAllocationSupported =
        context->vkFunctionTable.vkGetBufferMemoryRequirements2KHR != nullptr;

''' + source[end:]
path = output / 'ffx_vk_linux.cpp'
if not path.exists() or path.read_text() != source:
    path.write_text(source)
