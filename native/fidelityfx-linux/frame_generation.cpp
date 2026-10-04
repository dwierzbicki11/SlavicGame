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

static FfxResource wrap(const FfxApiResource& resource) {
    FfxResource result{};
    result.resource = resource.resource;
    result.state = static_cast<FfxResourceStates>(resource.state);
    const auto& d = resource.description;
    result.description = {
        static_cast<FfxResourceType>(d.type),
        static_cast<FfxSurfaceFormat>(d.format),
        d.width, d.height, d.depth, d.mipCount,
        static_cast<FfxResourceFlags>(d.flags),
        static_cast<FfxResourceUsage>(d.usage)
    };
    return result;
}

struct PreparedFrame {
    bool valid = false;
    uint64_t frameId = 0;
    uint32_t sharedSlot = 0;
    uint32_t renderWidth = 0;
    uint32_t renderHeight = 0;
    float frameTimeDelta = 0;
    float cameraNear = 0;
    float cameraFar = 0;
    float viewSpaceToMetersFactor = 1;
    float cameraFovAngleVertical = 0;
};

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

    uint32_t displayWidth = 0;
    uint32_t displayHeight = 0;
    uint32_t opticalFlowWidth = 0;
    uint32_t opticalFlowHeight = 0;
    uint32_t sharedToggle = 0;
    PreparedFrame prepared[2]{};
    bool hasLastFrame = false;
    uint64_t lastFrameId = 0;

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

    context->displayWidth = desc->displayWidth;
    context->displayHeight = desc->displayHeight;
    context->opticalFlowWidth =
        ofResources.opticalFlowVector.resourceDescription.width;
    context->opticalFlowHeight =
        ofResources.opticalFlowVector.resourceDescription.height;

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

extern "C" __attribute__((visibility("default"))) uint32_t slavicFgPrepare(
    SlavicFgContext* context, const SlavicFgPrepareDesc* desc)
{
    if (!context || !desc || !desc->commandList ||
        !desc->depth.resource || !desc->motionVectors.resource ||
        !desc->renderWidth || !desc->renderHeight)
        return static_cast<uint32_t>(FFX_ERROR_INVALID_ARGUMENT);

    context->sharedToggle = (context->sharedToggle + 1u) & 1u;
    const uint32_t slot = context->sharedToggle;

    FfxFrameInterpolationPrepareDescription prepare{};
    prepare.commandList = desc->commandList;
    prepare.renderSize = {desc->renderWidth, desc->renderHeight};
    prepare.jitterOffset = {desc->jitterX, desc->jitterY};
    prepare.motionVectorScale = {
        desc->motionVectorScaleX, desc->motionVectorScaleY
    };
    prepare.frameTimeDelta = desc->frameTimeDelta;
    prepare.cameraNear = desc->cameraNear;
    prepare.cameraFar = desc->cameraFar;
    prepare.viewSpaceToMetersFactor = desc->viewSpaceToMetersFactor;
    prepare.cameraFovAngleVertical = desc->cameraFovAngleVertical;
    prepare.depth = wrap(desc->depth);
    prepare.motionVectors = wrap(desc->motionVectors);
    prepare.frameID = desc->frameId;
    prepare.dilatedDepth = context->sharedBackend.fpGetResource(
        &context->sharedBackend, context->dilatedDepth[slot].resource);
    prepare.dilatedMotionVectors = context->sharedBackend.fpGetResource(
        &context->sharedBackend, context->dilatedMotionVectors[slot].resource);
    prepare.reconstructedPrevDepth = context->sharedBackend.fpGetResource(
        &context->sharedBackend, context->reconstructedPrevDepth[slot].resource);
    std::memcpy(prepare.cameraPosition, desc->cameraPosition, sizeof(desc->cameraPosition));
    std::memcpy(prepare.cameraUp, desc->cameraUp, sizeof(desc->cameraUp));
    std::memcpy(prepare.cameraRight, desc->cameraRight, sizeof(desc->cameraRight));
    std::memcpy(prepare.cameraForward, desc->cameraForward, sizeof(desc->cameraForward));

    FfxErrorCode rc = ffxFrameInterpolationPrepare(
        &context->frameInterpolation, &prepare);
    if (rc != FFX_OK)
        return static_cast<uint32_t>(rc);

    PreparedFrame& saved = context->prepared[desc->frameId & 1u];
    saved.valid = true;
    saved.frameId = desc->frameId;
    saved.sharedSlot = slot;
    saved.renderWidth = desc->renderWidth;
    saved.renderHeight = desc->renderHeight;
    saved.frameTimeDelta = desc->frameTimeDelta;
    saved.cameraNear = desc->cameraNear;
    saved.cameraFar = desc->cameraFar;
    saved.viewSpaceToMetersFactor = desc->viewSpaceToMetersFactor;
    saved.cameraFovAngleVertical = desc->cameraFovAngleVertical;
    return 0;
}

extern "C" __attribute__((visibility("default"))) uint32_t slavicFgDispatch(
    SlavicFgContext* context, const SlavicFgDispatchDesc* desc)
{
    if (!context || !desc || !desc->commandList ||
        !desc->currentBackBuffer.resource || !desc->output.resource)
        return static_cast<uint32_t>(FFX_ERROR_INVALID_ARGUMENT);

    const PreparedFrame& prepared = context->prepared[desc->frameId & 1u];
    if (!prepared.valid || prepared.frameId != desc->frameId)
        return static_cast<uint32_t>(FFX_ERROR_INVALID_ARGUMENT);

    const bool disjoint = context->hasLastFrame &&
        desc->frameId != context->lastFrameId + 1u;
    const bool reset = desc->reset != 0 || disjoint;

    FfxResource opticalFlowVector = context->sharedBackend.fpGetResource(
        &context->sharedBackend, context->opticalFlowVector.resource);
    FfxResource opticalFlowScd = context->sharedBackend.fpGetResource(
        &context->sharedBackend, context->opticalFlowScd.resource);

    FfxOpticalflowDispatchDescription opticalFlow{};
    opticalFlow.commandList = desc->commandList;
    opticalFlow.color = desc->currentBackBufferHudless.resource
        ? wrap(desc->currentBackBufferHudless)
        : wrap(desc->currentBackBuffer);
    opticalFlow.opticalFlowVector = opticalFlowVector;
    opticalFlow.opticalFlowSCD = opticalFlowScd;
    opticalFlow.reset = reset;
    opticalFlow.backbufferTransferFunction =
        static_cast<int>(desc->backBufferTransferFunction);
    opticalFlow.minMaxLuminance = {
        desc->minLuminance, desc->maxLuminance
    };

    FfxErrorCode rc = ffxOpticalflowContextDispatch(
        &context->opticalFlow, &opticalFlow);
    if (rc != FFX_OK)
        return static_cast<uint32_t>(rc);

    FfxFrameInterpolationDispatchDescription interpolation{};
    interpolation.commandList = desc->commandList;
    interpolation.displaySize = {
        desc->currentBackBuffer.description.width,
        desc->currentBackBuffer.description.height
    };
    interpolation.renderSize = {
        prepared.renderWidth, prepared.renderHeight
    };
    interpolation.currentBackBuffer = wrap(desc->currentBackBuffer);
    interpolation.currentBackBuffer_HUDLess =
        wrap(desc->currentBackBufferHudless);
    interpolation.output = wrap(desc->output);
    interpolation.interpolationRect = {
        0, 0,
        desc->currentBackBuffer.description.width,
        desc->currentBackBuffer.description.height
    };
    interpolation.opticalFlowVector = opticalFlowVector;
    interpolation.opticalFlowSceneChangeDetection = opticalFlowScd;
    interpolation.opticalFlowBufferSize = {
        context->opticalFlowWidth, context->opticalFlowHeight
    };
    interpolation.opticalFlowScale = {
        1.0f / static_cast<float>(context->displayWidth),
        1.0f / static_cast<float>(context->displayHeight)
    };
    interpolation.opticalFlowBlockSize = 8;
    interpolation.cameraNear = prepared.cameraNear;
    interpolation.cameraFar = prepared.cameraFar;
    interpolation.cameraFovAngleVertical =
        prepared.cameraFovAngleVertical;
    interpolation.viewSpaceToMetersFactor =
        prepared.viewSpaceToMetersFactor;
    interpolation.frameTimeDelta = prepared.frameTimeDelta;
    interpolation.reset = reset;
    interpolation.backBufferTransferFunction =
        static_cast<FfxBackbufferTransferFunction>(
            desc->backBufferTransferFunction);
    interpolation.minMaxLuminance[0] = desc->minLuminance;
    interpolation.minMaxLuminance[1] = desc->maxLuminance;
    interpolation.frameID = desc->frameId;

    const uint32_t slot = prepared.sharedSlot;
    interpolation.dilatedDepth = context->sharedBackend.fpGetResource(
        &context->sharedBackend, context->dilatedDepth[slot].resource);
    interpolation.dilatedMotionVectors =
        context->sharedBackend.fpGetResource(
            &context->sharedBackend,
            context->dilatedMotionVectors[slot].resource);
    interpolation.reconstructedPrevDepth =
        context->sharedBackend.fpGetResource(
            &context->sharedBackend,
            context->reconstructedPrevDepth[slot].resource);

    rc = ffxFrameInterpolationDispatch(
        &context->frameInterpolation, &interpolation);
    if (rc != FFX_OK)
        return static_cast<uint32_t>(rc);

    context->hasLastFrame = true;
    context->lastFrameId = desc->frameId;
    return 0;
}

extern "C" __attribute__((visibility("default"))) void slavicFgDestroy(SlavicFgContext* context) {
    delete context;
}
