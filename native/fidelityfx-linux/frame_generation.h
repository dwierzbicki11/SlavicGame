#pragma once
#include <cstddef>
#include <cstdint>
#include <ffx_api/ffx_upscale.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct SlavicFgContext SlavicFgContext;

enum SlavicFgFlags : uint32_t {
    SLAVIC_FG_DEPTH_INVERTED = 1u << 0,
    SLAVIC_FG_DEPTH_INFINITE = 1u << 1,
    SLAVIC_FG_JITTER_MOTION_VECTORS = 1u << 2,
    SLAVIC_FG_DISPLAY_RESOLUTION_MOTION_VECTORS = 1u << 3,
    SLAVIC_FG_HDR_COLOR = 1u << 4,
};

typedef struct SlavicFgCreateDesc {
    void* vkDevice;
    void* vkPhysicalDevice;
    void* vkDeviceProcAddr;
    uint32_t maxRenderWidth;
    uint32_t maxRenderHeight;
    uint32_t displayWidth;
    uint32_t displayHeight;
    uint32_t backBufferFormat;
    uint32_t flags;
} SlavicFgCreateDesc;

typedef struct SlavicFgPrepareDesc {
    void* commandList;
    FfxApiResource depth;
    FfxApiResource motionVectors;
    uint32_t renderWidth;
    uint32_t renderHeight;
    float jitterX;
    float jitterY;
    float motionVectorScaleX;
    float motionVectorScaleY;
    float frameTimeDelta;
    float cameraNear;
    float cameraFar;
    float viewSpaceToMetersFactor;
    float cameraFovAngleVertical;
    uint64_t frameId;
    float cameraPosition[3];
    float cameraUp[3];
    float cameraRight[3];
    float cameraForward[3];
} SlavicFgPrepareDesc;

typedef struct SlavicFgDispatchDesc {
    void* commandList;
    FfxApiResource currentBackBuffer;
    FfxApiResource currentBackBufferHudless;
    FfxApiResource output;
    uint64_t frameId;
    uint32_t reset;
    uint32_t backBufferTransferFunction;
    float minLuminance;
    float maxLuminance;
} SlavicFgDispatchDesc;

// Returns 0 on success. The context is deliberately independent from the
// production FSR3 upscaler context so an FG failure can never disable upscaling.
uint32_t slavicFgCreate(const SlavicFgCreateDesc* desc, SlavicFgContext** context);
uint32_t slavicFgPrepare(SlavicFgContext* context, const SlavicFgPrepareDesc* desc);
uint32_t slavicFgDispatch(SlavicFgContext* context, const SlavicFgDispatchDesc* desc);
void slavicFgDestroy(SlavicFgContext* context);
uint32_t slavicFgIsSupported(void* vkPhysicalDevice);
size_t slavicFgAbiLayout(uint32_t entry);

#ifdef __cplusplus
}
#endif
