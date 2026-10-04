// SlavicGame Linux adapter for the AMD FidelityFX SDK 1.1.4 / FSR 3.1.4.
// Implements the upscaler subset of the five-function API used by our renderer.
// AMD algorithm sources are pinned; Linux backend adaptations are in prepare_sdk.py.
#include <ffx_api/ffx_upscale.h>
#include <ffx_api/vk/ffx_api_vk.h>
#include <FidelityFX/host/ffx_fsr3upscaler.h>
#include <FidelityFX/host/backends/vk/ffx_vk.h>
#include <cstdlib>
#include <memory>
#include <cstddef>

namespace {
struct Context {
    FfxInterface backend{};
    FfxFsr3UpscalerContext upscaler{};
    FfxResourceInternal shared[3]{};
    bool created = false;
    ~Context() {
        if (created) {
            for (auto resource : shared)
                backend.fpDestroyResource(&backend, resource, 0);
            ffxFsr3UpscalerContextDestroy(&upscaler);
        }
        std::free(backend.scratchBuffer);
    }
};
FfxResource wrap(const FfxApiResource& resource) {
    FfxResource result{};
    result.resource = resource.resource;
    result.state = static_cast<FfxResourceStates>(resource.state);
    const auto& d = resource.description;
    result.description = {static_cast<FfxResourceType>(d.type),
        static_cast<FfxSurfaceFormat>(d.format), d.width, d.height, d.depth, d.mipCount,
        static_cast<FfxResourceFlags>(d.flags), static_cast<FfxResourceUsage>(d.usage)};
    return result;
}
uint32_t result(FfxErrorCode code) {
    return code == FFX_OK ? FFX_API_RETURN_OK : FFX_API_RETURN_ERROR_RUNTIME_ERROR;
}
}

extern "C" __attribute__((visibility("default"))) uint32_t slavicFsrLinuxVersion() {
    return 0x030104;
}
extern "C" __attribute__((visibility("default"))) size_t slavicFsrAbiLayout(uint32_t entry) {
    switch(entry) {
    case 0: return sizeof(ffxApiHeader);
    case 1: return sizeof(ffxCreateBackendVKDesc);
    case 2: return sizeof(ffxCreateContextDescUpscale);
    case 3: return sizeof(FfxApiResource);
    case 4: return sizeof(ffxDispatchDescUpscale);
    case 5: return offsetof(ffxDispatchDescUpscale, reset);
    case 6: return offsetof(ffxDispatchDescUpscale, cameraNear);
    default: return 0;
    }
}

extern "C" ffxReturnCode_t ffxCreateContext(ffxContext* target,
    ffxCreateContextDescHeader* header, const ffxAllocationCallbacks* allocator) {
    if (!target || *target || !header || allocator) return FFX_API_RETURN_ERROR_PARAMETER;
    if (header->type != FFX_API_CREATE_CONTEXT_DESC_TYPE_UPSCALE)
        return FFX_API_RETURN_ERROR_UNKNOWN_DESCTYPE;
    auto* desc = reinterpret_cast<ffxCreateContextDescUpscale*>(header);
    if (!desc->maxRenderSize.width || !desc->maxRenderSize.height ||
        !desc->maxUpscaleSize.width || !desc->maxUpscaleSize.height)
        return FFX_API_RETURN_ERROR_PARAMETER;
    auto* next = header->pNext;
    if (!next || next->type != FFX_API_CREATE_CONTEXT_DESC_TYPE_BACKEND_VK || next->pNext)
        return FFX_API_RETURN_ERROR_UNKNOWN_DESCTYPE;
    auto* vk = reinterpret_cast<ffxCreateBackendVKDesc*>(next);
    if (!vk->vkDevice || !vk->vkPhysicalDevice || !vk->vkDeviceProcAddr)
        return FFX_API_RETURN_ERROR_PARAMETER;
    try {
        auto ctx = std::make_unique<Context>();
        VkDeviceContext device{vk->vkDevice, vk->vkPhysicalDevice, vk->vkDeviceProcAddr};
        size_t size = ffxGetScratchMemorySizeVK(vk->vkPhysicalDevice, 1);
        void* memory = std::aligned_alloc(32, size);
        if (!memory) return FFX_API_RETURN_ERROR_MEMORY;
        std::memset(memory, 0, size);
        auto rc = ffxGetInterfaceVK(&ctx->backend, ffxGetDeviceVK(&device), memory, size, 1);
        if (rc != FFX_OK) { std::free(memory); return result(rc); }
        FfxFsr3UpscalerContextDescription create{};
        create.backendInterface = ctx->backend;
        create.flags = desc->flags & 0x7f; // SDK flags 0..6 match API flags.
        if (desc->flags & FFX_UPSCALE_ENABLE_DEBUG_CHECKING)
            create.flags |= FFX_FSR3UPSCALER_ENABLE_DEBUG_CHECKING;
        create.maxRenderSize = {desc->maxRenderSize.width, desc->maxRenderSize.height};
        create.maxUpscaleSize = {desc->maxUpscaleSize.width, desc->maxUpscaleSize.height};
        // Linux wchar_t is native throughout this adapter; managed callbacks are
        // intentionally not accepted until their encoding contract is implemented.
        if (desc->fpMessage) return FFX_API_RETURN_ERROR_PARAMETER;
        rc = ffxFsr3UpscalerContextCreate(&ctx->upscaler, &create);
        ctx->created = true; // Also clean any partial initialization on failure.
        if (rc != FFX_OK) return result(rc);
        FfxFsr3UpscalerSharedResourceDescriptions descriptions{};
        rc = ffxFsr3UpscalerGetSharedResourceDescriptions(&ctx->upscaler, &descriptions);
        if (rc != FFX_OK) return result(rc);
        FfxCreateResourceDescription resources[] = {descriptions.dilatedDepth,
            descriptions.dilatedMotionVectors, descriptions.reconstructedPrevNearestDepth};
        for (uint32_t i = 0; i < 3; i++) {
            rc = ctx->backend.fpCreateResource(&ctx->backend, &resources[i], 0, &ctx->shared[i]);
            if (rc != FFX_OK) return result(rc);
        }
        *target = ctx.release();
        return FFX_API_RETURN_OK;
    } catch (...) { return FFX_API_RETURN_ERROR_MEMORY; }
}

extern "C" ffxReturnCode_t ffxDestroyContext(ffxContext* target,
    const ffxAllocationCallbacks* allocator) {
    if (!target || allocator) return FFX_API_RETURN_ERROR_PARAMETER;
    delete static_cast<Context*>(*target);
    *target = nullptr;
    return FFX_API_RETURN_OK;
}
extern "C" ffxReturnCode_t ffxDispatch(ffxContext* target, const ffxDispatchDescHeader* header) {
    if (!target || !*target || !header) return FFX_API_RETURN_ERROR_PARAMETER;
    if (header->type != FFX_API_DISPATCH_DESC_TYPE_UPSCALE || header->pNext)
        return FFX_API_RETURN_ERROR_UNKNOWN_DESCTYPE;
    const auto* input = reinterpret_cast<const ffxDispatchDescUpscale*>(header);
    if (!input->commandList || !input->color.resource || !input->depth.resource ||
        !input->motionVectors.resource || !input->output.resource)
        return FFX_API_RETURN_ERROR_PARAMETER;
    auto* ctx = static_cast<Context*>(*target);
    FfxFsr3UpscalerDispatchDescription d{};
    d.commandList = input->commandList;
    d.color = wrap(input->color);
    d.depth = wrap(input->depth);
    d.motionVectors = wrap(input->motionVectors);
    d.exposure = wrap(input->exposure);
    d.reactive = wrap(input->reactive);
    d.transparencyAndComposition = wrap(input->transparencyAndComposition);
    d.output = wrap(input->output);
    d.dilatedDepth = ctx->backend.fpGetResource(&ctx->backend, ctx->shared[0]);
    d.dilatedMotionVectors = ctx->backend.fpGetResource(&ctx->backend, ctx->shared[1]);
    d.reconstructedPrevNearestDepth = ctx->backend.fpGetResource(&ctx->backend, ctx->shared[2]);
    d.jitterOffset = {input->jitterOffset.x, input->jitterOffset.y};
    d.motionVectorScale = {input->motionVectorScale.x, input->motionVectorScale.y};
    d.renderSize = {input->renderSize.width, input->renderSize.height};
    d.upscaleSize = {input->upscaleSize.width, input->upscaleSize.height};
    d.enableSharpening = input->enableSharpening;
    d.sharpness = input->sharpness;
    d.frameTimeDelta = input->frameTimeDelta;
    d.preExposure = input->preExposure;
    d.reset = input->reset;
    d.cameraNear = input->cameraNear;
    d.cameraFar = input->cameraFar;
    d.cameraFovAngleVertical = input->cameraFovAngleVertical;
    d.viewSpaceToMetersFactor = input->viewSpaceToMetersFactor;
    d.flags = input->flags & FFX_UPSCALE_FLAG_DRAW_DEBUG_VIEW;
    return result(ffxFsr3UpscalerContextDispatch(&ctx->upscaler, &d));
}
extern "C" ffxReturnCode_t ffxQuery(ffxContext*, ffxQueryDescHeader* header) {
    if (!header) return FFX_API_RETURN_ERROR_PARAMETER;
    // Renderer owns jitter/quality sizing; reject unsupported query extensions.
    return FFX_API_RETURN_ERROR_UNKNOWN_DESCTYPE;
}
extern "C" ffxReturnCode_t ffxConfigure(ffxContext*, const ffxConfigureDescHeader* header) {
    if (!header) return FFX_API_RETURN_ERROR_PARAMETER;
    return FFX_API_RETURN_ERROR_UNKNOWN_DESCTYPE;
}

// SDK backend retains this callback even in an upscaler-only build. Frame
// generation/swapchain replacement is explicitly unsupported by this provider.
extern "C" FfxErrorCode ffxSetFrameGenerationConfigToSwapchainVK(const FfxFrameGenerationConfig*) {
    return FFX_ERROR_INVALID_ARGUMENT;
}
