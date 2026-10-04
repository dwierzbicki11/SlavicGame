// SlavicGame native FSR 3.1.4 Frame Generation context foundation.
//
// This file deliberately keeps Frame Generation independent from the existing
// FSR3 upscaler bridge. A failure here can disable FG while leaving the
// production temporal upscaler untouched.
#include "frame_generation.h"

#include <FidelityFX/host/ffx_frameinterpolation.h>
#include <FidelityFX/host/ffx_opticalflow.h>
#include <FidelityFX/host/backends/vk/ffx_vk.h>

#include <cstdlib>
#include <cstring>
#include <cwchar>
#include <new>

namespace {

struct SharedResource {
    FfxResourceInternal resource{};
    bool created = false;
};

static void* allocateScratch(size_t size) {
    // C11 aligned_alloc requires size to be a multiple of alignment.
    constexpr size_t alignment = 32;
    size_t rounded = (size + alignment - 1) & ~(alignment - 1);
    void* result = std::aligned_alloc(alignment, rounded);
    if (result) std::memset(result, 0, rounded);
    return result;
}

static FfxErrorCode createResource(
    FfxInterface& backend,
    FfxUInt32 effectContextId,
    const FfxCreateResourceDescription& description,
    SharedResource& target)
{
    FfxErrorCode rc = backend.fpCreateResource(
        &backend, &description, effectContextId, &target.resource);
    if (rc == FFX_OK) target.created = true;
    return rc;
}

} // namespace

struct SlavicFgContext {
    FfxInterface sharedBackend{};
    FfxInterface fiBackend{};
    void* sharedScratch = nullptr;
    void* fiScratch = nullptr;
    FfxUInt32 sharedEffectContextId = 0;
    bool sharedBackendCreated = false;
    FfxOpticalflowContext opticalFlow{};
    bool opticalFlowCreated = false;
    FfxFrameInterpolationContext frameInterpolation{};
    bool frameInterpolationCreated = false;

    SharedResource opticalFlowVector{};
    SharedResource opticalFlowScd{};
    SharedResource dilatedDepth[2]{};
    SharedResource dilatedMotionVectors[2]{};
    SharedResource reconstructedPrevDepth[2]{};

    ~SlavicFgContext() {
        auto destroy = [this](SharedResource& resource) {
            if (resource.created) {
                sharedBackend.fpDestroyResource(
                    &sharedBackend, resource.resource, sharedEffectContextId);
                resource.created = false;
            }
        };

        // Shared surfaces can be referenced by both effects. Destroy them
        // before tearing down the effect contexts, after all GPU work is idle
        // (the caller owns that synchronization).
        destroy(opticalFlowVector);
        destroy(opticalFlowScd);
        for (int i = 0; i < 2; ++i) {
            destroy(dilatedDepth[i]);
            destroy(dilatedMotionVectors[i]);
            destroy(reconstructedPrevDepth[i]);
        }

        if (frameInterpolationCreated)
            ffxFrameInterpolationContextDestroy(&frameInterpolation);
        if (opticalFlowCreated)
            ffxOpticalflowContextDestroy(&opticalFlow);
        if (sharedBackendCreated)
            sharedBackend.fpDestroyBackendContext(
                &sharedBackend, sharedEffectContextId);

        std::free(fiScratch);
        std::free(sharedScratch);
    }
};

extern "C" __attribute__((visibility("default"))) uint32_t slavicFgCreate(
    const SlavicFgCreateDesc* desc, SlavicFgContext** outContext)
{
    if (!desc || !outContext || *outContext ||
        !desc->vkDevice || !desc->vkPhysicalDevice || !desc->vkDeviceProcAddr ||
        !desc->maxRenderWidth || !desc->maxRenderHeight ||
        !desc->displayWidth || !desc->displayHeight)
        return static_cast<uint32_t>(FFX_ERROR_INVALID_ARGUMENT);

    auto* context = new (std::nothrow) SlavicFgContext();
    if (!context)
        return static_cast<uint32_t>(FFX_ERROR_OUT_OF_MEMORY);

    auto fail = [&](FfxErrorCode rc) {
        delete context;
        return static_cast<uint32_t>(rc);
    };

    VkDeviceContext deviceContext{
        reinterpret_cast<VkDevice>(desc->vkDevice),
        reinterpret_cast<VkPhysicalDevice>(desc->vkPhysicalDevice),
        reinterpret_cast<PFN_vkGetDeviceProcAddr>(desc->vkDeviceProcAddr)
    };
    FfxDevice ffxDevice = ffxGetDeviceVK(&deviceContext);

    size_t sharedScratchSize = ffxGetScratchMemorySizeVK(
        reinterpret_cast<VkPhysicalDevice>(desc->vkPhysicalDevice), 1);
    context->sharedScratch = allocateScratch(sharedScratchSize);
    if (!context->sharedScratch)
        return fail(FFX_ERROR_OUT_OF_MEMORY);

    FfxErrorCode rc = ffxGetInterfaceVK(
        &context->sharedBackend, ffxDevice,
        context->sharedScratch, sharedScratchSize, 1);
    if (rc != FFX_OK) return fail(rc);

    size_t fiScratchSize = ffxGetScratchMemorySizeVK(
        reinterpret_cast<VkPhysicalDevice>(desc->vkPhysicalDevice), 2);
    context->fiScratch = allocateScratch(fiScratchSize);
    if (!context->fiScratch)
        return fail(FFX_ERROR_OUT_OF_MEMORY);

    rc = ffxGetInterfaceVK(
        &context->fiBackend, ffxDevice,
        context->fiScratch, fiScratchSize, 2);
    if (rc != FFX_OK) return fail(rc);

    rc = context->sharedBackend.fpCreateBackendContext(
        &context->sharedBackend, FFX_EFFECT_SHAREDRESOURCES, nullptr,
        &context->sharedEffectContextId);
    if (rc != FFX_OK) return fail(rc);
    context->sharedBackendCreated = true;

    FfxOpticalflowContextDescription opticalFlowDescription{};
    opticalFlowDescription.backendInterface = context->fiBackend;
    opticalFlowDescription.resolution = {
        desc->displayWidth, desc->displayHeight
    };
    rc = ffxOpticalflowContextCreate(
        &context->opticalFlow, &opticalFlowDescription);
    if (rc != FFX_OK) return fail(rc);
    context->opticalFlowCreated = true;

    FfxFrameInterpolationContextDescription fiDescription{};
    fiDescription.backendInterface = context->fiBackend;
    fiDescription.maxRenderSize = {
        desc->maxRenderWidth, desc->maxRenderHeight
    };
    fiDescription.displaySize = {
        desc->displayWidth, desc->displayHeight
    };
    fiDescription.backBufferFormat =
        static_cast<FfxSurfaceFormat>(desc->backBufferFormat);
    fiDescription.previousInterpolationSourceFormat =
        fiDescription.backBufferFormat;

    if (desc->flags & SLAVIC_FG_DEPTH_INVERTED)
        fiDescription.flags |= FFX_FRAMEINTERPOLATION_ENABLE_DEPTH_INVERTED;
    if (desc->flags & SLAVIC_FG_DEPTH_INFINITE)
        fiDescription.flags |= FFX_FRAMEINTERPOLATION_ENABLE_DEPTH_INFINITE;
    if (desc->flags & SLAVIC_FG_JITTER_MOTION_VECTORS)
        fiDescription.flags |= FFX_FRAMEINTERPOLATION_ENABLE_JITTER_MOTION_VECTORS;
    if (desc->flags & SLAVIC_FG_DISPLAY_RESOLUTION_MOTION_VECTORS)
        fiDescription.flags |= FFX_FRAMEINTERPOLATION_ENABLE_DISPLAY_RESOLUTION_MOTION_VECTORS;
    if (desc->flags & SLAVIC_FG_HDR_COLOR)
        fiDescription.flags |= FFX_FRAMEINTERPOLATION_ENABLE_HDR_COLOR_INPUT;

    rc = ffxFrameInterpolationContextCreate(
        &context->frameInterpolation, &fiDescription);
    if (rc != FFX_OK) return fail(rc);
    context->frameInterpolationCreated = true;

    FfxOpticalflowSharedResourceDescriptions ofResources{};
    rc = ffxOpticalflowGetSharedResourceDescriptions(
        &context->opticalFlow, &ofResources);
    if (rc != FFX_OK) return fail(rc);
    rc = createResource(
        context->sharedBackend, context->sharedEffectContextId,
        ofResources.opticalFlowVector, context->opticalFlowVector);
    if (rc != FFX_OK) return fail(rc);
    rc = createResource(
        context->sharedBackend, context->sharedEffectContextId,
        ofResources.opticalFlowSCD, context->opticalFlowScd);
    if (rc != FFX_OK) return fail(rc);

    FfxFrameInterpolationSharedResourceDescriptions fiResources{};
    rc = ffxFrameInterpolationGetSharedResourceDescriptions(
        &context->frameInterpolation, &fiResources);
    if (rc != FFX_OK) return fail(rc);

    wchar_t resourceName[256]{};
    for (uint32_t frame = 0; frame < 2; ++frame) {
        FfxCreateResourceDescription depth = fiResources.dilatedDepth;
        std::swprintf(resourceName, 255, L"%ls%u", fiResources.dilatedDepth.name, frame);
        depth.name = resourceName;
        rc = createResource(
            context->sharedBackend, context->sharedEffectContextId,
            depth, context->dilatedDepth[frame]);
        if (rc != FFX_OK) return fail(rc);

        FfxCreateResourceDescription motion = fiResources.dilatedMotionVectors;
        std::swprintf(resourceName, 255, L"%ls%u", fiResources.dilatedMotionVectors.name, frame);
        motion.name = resourceName;
        rc = createResource(
            context->sharedBackend, context->sharedEffectContextId,
            motion, context->dilatedMotionVectors[frame]);
        if (rc != FFX_OK) return fail(rc);

        FfxCreateResourceDescription reconstructed =
            fiResources.reconstructedPrevNearestDepth;
        std::swprintf(
            resourceName, 255, L"%ls%u",
            fiResources.reconstructedPrevNearestDepth.name, frame);
        reconstructed.name = resourceName;
        rc = createResource(
            context->sharedBackend, context->sharedEffectContextId,
            reconstructed, context->reconstructedPrevDepth[frame]);
        if (rc != FFX_OK) return fail(rc);
    }

    *outContext = context;
    return 0;
}

extern "C" __attribute__((visibility("default"))) void slavicFgDestroy(SlavicFgContext* context) {
    delete context;
}
