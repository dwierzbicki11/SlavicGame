// Test-only Vulkan 1.1 images/device. C# owns the production FG context and
// command ring; this fixture never calls FG prepare/dispatch for those tests.
#define SLAVIC_FG_FIXTURE_ONLY
#include "smoke.cpp"

namespace {
struct ManagedFixture {
    Test vk{false};
    Test::Image color{}, depth{}, motion{}, output{};

    void resize(uint32_t renderW, uint32_t renderH,
                uint32_t displayW, uint32_t displayH) {
        color = vk.image(VK_FORMAT_R16G16B16A16_SFLOAT, displayW, displayH);
        depth = vk.image(VK_FORMAT_D32_SFLOAT, renderW, renderH);
        motion = vk.image(VK_FORMAT_R16G16_SFLOAT, renderW, renderH);
        output = vk.image(VK_FORMAT_R16G16B16A16_SFLOAT, displayW, displayH);
        vk.begin();
        for (auto image : {color, depth, motion}) {
            vk.barrier(image, VK_IMAGE_LAYOUT_UNDEFINED,
                VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 0, VK_ACCESS_TRANSFER_WRITE_BIT);
            VkImageSubresourceRange range{
                image.format == VK_FORMAT_D32_SFLOAT ? VK_IMAGE_ASPECT_DEPTH_BIT : VK_IMAGE_ASPECT_COLOR_BIT,
                0, 1, 0, 1
            };
            if (image.format == VK_FORMAT_D32_SFLOAT) {
                VkClearDepthStencilValue value{0.5f, 0};
                vkCmdClearDepthStencilImage(vk.command, image.image,
                    VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, &value, 1, &range);
            } else {
                VkClearColorValue value{};
                if (image.image == color.image) {
                    value.float32[0] = 0.25f;
                    value.float32[1] = 0.50f;
                    value.float32[2] = 0.75f;
                    value.float32[3] = 1.0f;
                }
                vkCmdClearColorImage(vk.command, image.image,
                    VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, &value, 1, &range);
            }
            vk.barrier(image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL,
                VK_ACCESS_TRANSFER_WRITE_BIT, VK_ACCESS_SHADER_READ_BIT);
        }
        vk.barrier(output, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_GENERAL,
            0, VK_ACCESS_SHADER_WRITE_BIT);
        vk.submit();
    }
};

template<typename F> uint32_t guarded(F action) {
    try { action(); return 0; }
    catch (const std::exception& error) {
        std::cerr << "Managed FG fixture: " << error.what() << std::endl;
        return 1;
    }
}
}

extern "C" __attribute__((visibility("default"))) uint32_t slavicFgTestCreate(
    void** fixture, void** instance, void** physical, void** device, void** queue,
    uint32_t* family, uint32_t* apiVersion) {
    if (!fixture || !instance || !physical || !device || !queue || !family || !apiVersion)
        return 1;
    *fixture = nullptr;
    return guarded([&] {
        auto* created = new ManagedFixture();
        *fixture = created;
        *instance = created->vk.instance;
        *physical = created->vk.physical;
        *device = created->vk.device;
        *queue = created->vk.queue;
        *family = created->vk.family;
        *apiVersion = VK_API_VERSION_1_1;
    });
}

extern "C" __attribute__((visibility("default"))) uint32_t slavicFgTestResize(
    void* fixture, uint32_t renderW, uint32_t renderH, uint32_t displayW, uint32_t displayH,
    FfxApiResource* color, FfxApiResource* depth, FfxApiResource* motion, FfxApiResource* output) {
    if (!fixture || !renderW || !renderH || !displayW || !displayH ||
        !color || !depth || !motion || !output) return 1;
    return guarded([&] {
        auto& f = *static_cast<ManagedFixture*>(fixture);
        f.resize(renderW, renderH, displayW, displayH);
        *color = f.vk.resource(f.color, FFX_API_SURFACE_FORMAT_R16G16B16A16_FLOAT,
            FFX_API_RESOURCE_STATE_COMPUTE_READ, FFX_API_RESOURCE_USAGE_READ_ONLY);
        *depth = f.vk.resource(f.depth, FFX_API_SURFACE_FORMAT_R32_FLOAT,
            FFX_API_RESOURCE_STATE_COMPUTE_READ, FFX_API_RESOURCE_USAGE_DEPTHTARGET);
        *motion = f.vk.resource(f.motion, FFX_API_SURFACE_FORMAT_R16G16_FLOAT,
            FFX_API_RESOURCE_STATE_COMPUTE_READ, FFX_API_RESOURCE_USAGE_READ_ONLY);
        *output = f.vk.resource(f.output, FFX_API_SURFACE_FORMAT_R16G16B16A16_FLOAT,
            FFX_API_RESOURCE_STATE_UNORDERED_ACCESS, FFX_API_RESOURCE_USAGE_UAV);
    });
}

extern "C" __attribute__((visibility("default"))) uint32_t slavicFgTestVerify(void* fixture) {
    if (!fixture) return 1;
    return guarded([&] {
        auto& f = *static_cast<ManagedFixture*>(fixture);
        f.vk.readback(f.output);
        f.vk.begin();
        f.vk.barrier(f.output, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, VK_IMAGE_LAYOUT_GENERAL,
            VK_ACCESS_TRANSFER_READ_BIT, VK_ACCESS_SHADER_WRITE_BIT);
        f.vk.submit();
    });
}

extern "C" __attribute__((visibility("default"))) void slavicFgTestDestroy(void* fixture) {
    delete static_cast<ManagedFixture*>(fixture);
}
