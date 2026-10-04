#pragma once
#include <cstdint>

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

// Returns 0 on success. The context is deliberately independent from the
// production FSR3 upscaler context so an FG failure can never disable upscaling.
uint32_t slavicFgCreate(const SlavicFgCreateDesc* desc, SlavicFgContext** context);
void slavicFgDestroy(SlavicFgContext* context);

#ifdef __cplusplus
}
#endif
