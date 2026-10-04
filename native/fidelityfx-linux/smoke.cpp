// Real, headless Vulkan FSR dispatch/readback. Works with Mesa lavapipe in CI.
#include <ffx_api/ffx_upscale.h>
#include <ffx_api/vk/ffx_api_vk.h>
#include "frame_generation.h"
#include <vulkan/vulkan.h>
#include <vector>
#include <stdexcept>
#include <iostream>
#include <cmath>
#include <cstring>

extern "C" uint32_t slavicFsrFrameGenerationComponents();

static void check(VkResult rc) {
    if (rc != VK_SUCCESS) throw std::runtime_error("Vulkan failure " + std::to_string(rc));
}
static void ffxCheck(uint32_t rc) {
    if (rc) throw std::runtime_error("FSR API failure " + std::to_string(rc));
}
struct Test {
    VkInstance instance{};
    VkPhysicalDevice physical{};
    VkDevice device{};
    VkQueue queue{};
    VkCommandPool pool{};
    VkCommandBuffer command{};
    VkPhysicalDeviceMemoryProperties memory{};
    uint32_t family{};
    struct Image { VkImage image; VkDeviceMemory memory; VkFormat format; uint32_t w, h; };
    std::vector<Image> images;
    VkBuffer staging{};
    VkDeviceMemory stagingMemory{};
    ffxContext context{};
    SlavicFgContext* fg{};
    Test() {
        VkApplicationInfo application{VK_STRUCTURE_TYPE_APPLICATION_INFO};
        application.apiVersion = VK_API_VERSION_1_2;
        VkInstanceCreateInfo create{VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO};
        create.pApplicationInfo = &application;
        check(vkCreateInstance(&create, nullptr, &instance));
        uint32_t count = 0;
        check(vkEnumeratePhysicalDevices(instance, &count, nullptr));
        if (!count) throw std::runtime_error("No Vulkan device for FSR smoke test");
        std::vector<VkPhysicalDevice> devices(count);
        check(vkEnumeratePhysicalDevices(instance, &count, devices.data()));
        physical = devices[0];
        VkPhysicalDeviceProperties properties{};
        vkGetPhysicalDeviceProperties(physical, &properties);
        std::cout << "Vulkan FSR device: " << properties.deviceName << std::endl;
        vkGetPhysicalDeviceMemoryProperties(physical, &memory);
        vkGetPhysicalDeviceQueueFamilyProperties(physical, &count, nullptr);
        std::vector<VkQueueFamilyProperties> families(count);
        vkGetPhysicalDeviceQueueFamilyProperties(physical, &count, families.data());
        for (; family < count; family++)
            if ((families[family].queueFlags & (VK_QUEUE_GRAPHICS_BIT | VK_QUEUE_COMPUTE_BIT))
                == (VK_QUEUE_GRAPHICS_BIT | VK_QUEUE_COMPUTE_BIT)) break;
        if (family == count) throw std::runtime_error("No graphics/compute queue");
        float priority = 1;
        VkDeviceQueueCreateInfo queues{VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO};
        queues.queueFamilyIndex = family; queues.queueCount = 1; queues.pQueuePriorities = &priority;
        VkPhysicalDeviceFeatures features{};
        vkGetPhysicalDeviceFeatures(physical, &features);
        VkDeviceCreateInfo deviceCreate{VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO};
        deviceCreate.queueCreateInfoCount = 1; deviceCreate.pQueueCreateInfos = &queues;
        deviceCreate.pEnabledFeatures = &features;
        check(vkCreateDevice(physical, &deviceCreate, nullptr, &device));
        vkGetDeviceQueue(device, family, 0, &queue);

        SlavicFgCreateDesc fgCreate{};
        fgCreate.vkDevice = device;
        fgCreate.vkPhysicalDevice = physical;
        fgCreate.vkDeviceProcAddr = reinterpret_cast<void*>(vkGetDeviceProcAddr);
        fgCreate.maxRenderWidth = 64;
        fgCreate.maxRenderHeight = 64;
        fgCreate.displayWidth = 128;
        fgCreate.displayHeight = 128;
        fgCreate.backBufferFormat = FFX_API_SURFACE_FORMAT_R16G16B16A16_FLOAT;
        fgCreate.flags = SLAVIC_FG_JITTER_MOTION_VECTORS;
        uint32_t fgRc = slavicFgCreate(&fgCreate, &fg);
        if (fgRc != 0 || !fg)
            throw std::runtime_error("FSR3 FG context creation failed " + std::to_string(fgRc));
        std::cout << "PASS FSR3 FG context/resources/pipelines create" << std::endl;

        VkCommandPoolCreateInfo pools{VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO};
        pools.queueFamilyIndex = family; pools.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
        check(vkCreateCommandPool(device, &pools, nullptr, &pool));
        VkCommandBufferAllocateInfo alloc{VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO};
        alloc.commandPool = pool; alloc.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY; alloc.commandBufferCount = 1;
        check(vkAllocateCommandBuffers(device, &alloc, &command));
    }
    ~Test() {
        if (device) vkDeviceWaitIdle(device);
        if (context) ffxDestroyContext(&context, nullptr);
        if (fg) slavicFgDestroy(fg);
        for (auto image : images) { vkDestroyImage(device, image.image, nullptr); vkFreeMemory(device, image.memory, nullptr); }
        if (staging) vkDestroyBuffer(device, staging, nullptr);
        if (stagingMemory) vkFreeMemory(device, stagingMemory, nullptr);
        if (pool) vkDestroyCommandPool(device, pool, nullptr);
        if (device) vkDestroyDevice(device, nullptr);
        if (instance) vkDestroyInstance(instance, nullptr);
    }
    uint32_t memoryType(uint32_t bits, VkMemoryPropertyFlags flags) {
        for (uint32_t i = 0; i < memory.memoryTypeCount; i++)
            if ((bits & (1u << i)) && (memory.memoryTypes[i].propertyFlags & flags) == flags) return i;
        throw std::runtime_error("Missing Vulkan memory type");
    }
    Image image(VkFormat format, uint32_t w, uint32_t h) {
        VkImageCreateInfo create{VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO};
        create.imageType = VK_IMAGE_TYPE_2D; create.format = format; create.extent = {w,h,1};
        create.mipLevels = 1; create.arrayLayers = 1; create.samples = VK_SAMPLE_COUNT_1_BIT;
        create.tiling = VK_IMAGE_TILING_OPTIMAL;
        create.usage = VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
        create.usage |= format == VK_FORMAT_D32_SFLOAT ? VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT : VK_IMAGE_USAGE_STORAGE_BIT;
        Image result{}; result.format = format; result.w = w; result.h = h;
        check(vkCreateImage(device, &create, nullptr, &result.image));
        VkMemoryRequirements requirements{};
        vkGetImageMemoryRequirements(device, result.image, &requirements);
        VkMemoryAllocateInfo allocate{VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO};
        allocate.allocationSize = requirements.size;
        allocate.memoryTypeIndex = memoryType(requirements.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
        check(vkAllocateMemory(device, &allocate, nullptr, &result.memory));
        check(vkBindImageMemory(device, result.image, result.memory, 0));
        images.push_back(result); return result;
    }
    void begin() {
        check(vkResetCommandBuffer(command, 0));
        VkCommandBufferBeginInfo begin{VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO};
        check(vkBeginCommandBuffer(command, &begin));
    }
    void submit() {
        check(vkEndCommandBuffer(command));
        VkSubmitInfo submit{VK_STRUCTURE_TYPE_SUBMIT_INFO};
        submit.commandBufferCount = 1; submit.pCommandBuffers = &command;
        check(vkQueueSubmit(queue, 1, &submit, VK_NULL_HANDLE));
        check(vkQueueWaitIdle(queue));
    }
    void barrier(Image image, VkImageLayout oldLayout, VkImageLayout newLayout,
                 VkAccessFlags sourceAccess, VkAccessFlags destAccess) {
        VkImageMemoryBarrier barrier{VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER};
        barrier.oldLayout = oldLayout; barrier.newLayout = newLayout;
        barrier.srcQueueFamilyIndex = barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
        barrier.image = image.image;
        barrier.subresourceRange = {image.format == VK_FORMAT_D32_SFLOAT ? VK_IMAGE_ASPECT_DEPTH_BIT : VK_IMAGE_ASPECT_COLOR_BIT,0,1,0,1};
        barrier.srcAccessMask = sourceAccess; barrier.dstAccessMask = destAccess;
        vkCmdPipelineBarrier(command, VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
                             0,0,nullptr,0,nullptr,1,&barrier);
    }
    FfxApiResource resource(Image image, uint32_t format, uint32_t state, uint32_t usage) {
        FfxApiResource resource{};
        resource.resource = reinterpret_cast<void*>(image.image);
        resource.description = {FFX_API_RESOURCE_TYPE_TEXTURE2D, format, image.w, image.h, 1, 1, 0, usage};
        resource.state = state; return resource;
    }
    void run(uint32_t w, uint32_t h) {
        if (context) ffxCheck(ffxDestroyContext(&context, nullptr));
        ffxCreateBackendVKDesc backend{};
        backend.header.type = FFX_API_CREATE_CONTEXT_DESC_TYPE_BACKEND_VK;
        backend.vkDevice = device; backend.vkPhysicalDevice = physical; backend.vkDeviceProcAddr = vkGetDeviceProcAddr;
        ffxCreateContextDescUpscale create{};
        create.header.type = FFX_API_CREATE_CONTEXT_DESC_TYPE_UPSCALE; create.header.pNext = &backend.header;
        create.maxRenderSize = {w,h}; create.maxUpscaleSize = {w*2,h*2};
        create.flags = FFX_UPSCALE_ENABLE_AUTO_EXPOSURE | FFX_UPSCALE_ENABLE_MOTION_VECTORS_JITTER_CANCELLATION;
        ffxCheck(ffxCreateContext(&context, &create.header, nullptr));
        auto color = image(VK_FORMAT_R8G8B8A8_UNORM,w,h);
        auto depth = image(VK_FORMAT_D32_SFLOAT,w,h);
        auto motion = image(VK_FORMAT_R16G16_SFLOAT,w,h);
        auto reactive = image(VK_FORMAT_R8_UNORM,w,h);
        auto output = image(VK_FORMAT_R16G16B16A16_SFLOAT,w*2,h*2);
        begin();
        for (auto img : {color,depth,motion,reactive}) {
            barrier(img,VK_IMAGE_LAYOUT_UNDEFINED,VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,0,VK_ACCESS_TRANSFER_WRITE_BIT);
            VkImageSubresourceRange range{img.format == VK_FORMAT_D32_SFLOAT ? VK_IMAGE_ASPECT_DEPTH_BIT : VK_IMAGE_ASPECT_COLOR_BIT,0,1,0,1};
            if (img.format == VK_FORMAT_D32_SFLOAT) {
                VkClearDepthStencilValue clear{0.5f,0};
                vkCmdClearDepthStencilImage(command,img.image,VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,&clear,1,&range);
            } else {
                VkClearColorValue clear{};
                if (img.image == color.image) { clear.float32[0]=0.25f; clear.float32[1]=0.5f; clear.float32[2]=0.75f; clear.float32[3]=1; }
                vkCmdClearColorImage(command,img.image,VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,&clear,1,&range);
            }
            barrier(img,VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL,VK_ACCESS_TRANSFER_WRITE_BIT,VK_ACCESS_SHADER_READ_BIT);
        }
        barrier(output,VK_IMAGE_LAYOUT_UNDEFINED,VK_IMAGE_LAYOUT_GENERAL,0,VK_ACCESS_SHADER_WRITE_BIT);
        submit();
        for (int frame=0; frame<3; frame++) {
            begin();
            ffxDispatchDescUpscale dispatch{};
            dispatch.header.type = FFX_API_DISPATCH_DESC_TYPE_UPSCALE;
            dispatch.commandList = command;
            dispatch.color = resource(color,FFX_API_SURFACE_FORMAT_R8G8B8A8_UNORM,FFX_API_RESOURCE_STATE_COMPUTE_READ,FFX_API_RESOURCE_USAGE_READ_ONLY);
            dispatch.depth = resource(depth,FFX_API_SURFACE_FORMAT_R32_FLOAT,FFX_API_RESOURCE_STATE_COMPUTE_READ,FFX_API_RESOURCE_USAGE_DEPTHTARGET);
            dispatch.motionVectors = resource(motion,FFX_API_SURFACE_FORMAT_R16G16_FLOAT,FFX_API_RESOURCE_STATE_COMPUTE_READ,FFX_API_RESOURCE_USAGE_READ_ONLY);
            dispatch.reactive = resource(reactive,FFX_API_SURFACE_FORMAT_R8_UNORM,FFX_API_RESOURCE_STATE_COMPUTE_READ,FFX_API_RESOURCE_USAGE_READ_ONLY);
            dispatch.output = resource(output,FFX_API_SURFACE_FORMAT_R16G16B16A16_FLOAT,FFX_API_RESOURCE_STATE_UNORDERED_ACCESS,FFX_API_RESOURCE_USAGE_UAV);
            dispatch.renderSize = {w,h}; dispatch.upscaleSize = {w*2,h*2};
            dispatch.motionVectorScale = {float(w),float(h)};
            dispatch.frameTimeDelta = 16.67f; dispatch.preExposure = 1;
            dispatch.cameraNear = 0.1f; dispatch.cameraFar = 100;
            dispatch.cameraFovAngleVertical = 1; dispatch.viewSpaceToMetersFactor = 1;
            dispatch.reset = frame == 0;
            dispatch.enableSharpening = frame != 0; dispatch.sharpness = 0.25f;
            ffxCheck(ffxDispatch(&context,&dispatch.header));
            submit();
        }
        readback(output);
        std::cout << "PASS AMD FSR 3.1.4 dispatch/readback " << w << "x" << h << " -> " << w*2 << "x" << h*2 << ", 3 frames + sharpening" << std::endl;
    }
    void runFrameGeneration() {
        const uint32_t renderW = 64, renderH = 64;
        const uint32_t displayW = 128, displayH = 128;
        auto depth = image(VK_FORMAT_D32_SFLOAT, renderW, renderH);
        auto motion = image(VK_FORMAT_R16G16_SFLOAT, renderW, renderH);
        auto present = image(VK_FORMAT_R16G16B16A16_SFLOAT, displayW, displayH);
        auto generated = image(VK_FORMAT_R16G16B16A16_SFLOAT, displayW, displayH);

        begin();
        for (auto img : {depth, motion, present}) {
            barrier(img, VK_IMAGE_LAYOUT_UNDEFINED,
                    VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 0,
                    VK_ACCESS_TRANSFER_WRITE_BIT);
            VkImageSubresourceRange range{
                img.format == VK_FORMAT_D32_SFLOAT
                    ? VK_IMAGE_ASPECT_DEPTH_BIT : VK_IMAGE_ASPECT_COLOR_BIT,
                0, 1, 0, 1};
            if (img.format == VK_FORMAT_D32_SFLOAT) {
                VkClearDepthStencilValue clear{0.5f, 0};
                vkCmdClearDepthStencilImage(
                    command, img.image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                    &clear, 1, &range);
            } else {
                VkClearColorValue clear{};
                if (img.image == present.image) {
                    clear.float32[0] = 0.25f;
                    clear.float32[1] = 0.50f;
                    clear.float32[2] = 0.75f;
                    clear.float32[3] = 1.0f;
                }
                vkCmdClearColorImage(
                    command, img.image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                    &clear, 1, &range);
            }
            barrier(img, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                    VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL,
                    VK_ACCESS_TRANSFER_WRITE_BIT, VK_ACCESS_SHADER_READ_BIT);
        }
        barrier(generated, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_GENERAL,
                0, VK_ACCESS_SHADER_WRITE_BIT);
        submit();

        for (uint64_t frame = 0; frame < 2; ++frame) {
            begin();

            SlavicFgPrepareDesc prepare{};
            prepare.commandList = command;
            prepare.depth = resource(
                depth, FFX_API_SURFACE_FORMAT_R32_FLOAT,
                FFX_API_RESOURCE_STATE_COMPUTE_READ,
                FFX_API_RESOURCE_USAGE_DEPTHTARGET);
            prepare.motionVectors = resource(
                motion, FFX_API_SURFACE_FORMAT_R16G16_FLOAT,
                FFX_API_RESOURCE_STATE_COMPUTE_READ,
                FFX_API_RESOURCE_USAGE_READ_ONLY);
            prepare.renderWidth = renderW;
            prepare.renderHeight = renderH;
            prepare.motionVectorScaleX = static_cast<float>(renderW);
            prepare.motionVectorScaleY = static_cast<float>(renderH);
            prepare.frameTimeDelta = 16.67f;
            prepare.cameraNear = 0.1f;
            prepare.cameraFar = 100.0f;
            prepare.viewSpaceToMetersFactor = 1.0f;
            prepare.cameraFovAngleVertical = 1.0f;
            prepare.frameId = frame;
            prepare.cameraUp[1] = 1.0f;
            prepare.cameraRight[0] = 1.0f;
            prepare.cameraForward[2] = -1.0f;
            uint32_t rc = slavicFgPrepare(fg, &prepare);
            if (rc)
                throw std::runtime_error(
                    "FSR3 FG prepare failed " + std::to_string(rc));

            SlavicFgDispatchDesc dispatch{};
            dispatch.commandList = command;
            dispatch.currentBackBuffer = resource(
                present, FFX_API_SURFACE_FORMAT_R16G16B16A16_FLOAT,
                FFX_API_RESOURCE_STATE_COMPUTE_READ,
                FFX_API_RESOURCE_USAGE_READ_ONLY);
            dispatch.output = resource(
                generated, FFX_API_SURFACE_FORMAT_R16G16B16A16_FLOAT,
                FFX_API_RESOURCE_STATE_UNORDERED_ACCESS,
                FFX_API_RESOURCE_USAGE_UAV);
            dispatch.frameId = frame;
            dispatch.reset = frame == 0;
            dispatch.backBufferTransferFunction =
                FFX_API_BACKBUFFER_TRANSFER_FUNCTION_SRGB;
            dispatch.minLuminance = 0.0f;
            dispatch.maxLuminance = 1.0f;
            rc = slavicFgDispatch(fg, &dispatch);
            if (rc)
                throw std::runtime_error(
                    "FSR3 FG dispatch failed " + std::to_string(rc));
            submit();
        }

        readback(generated);
        std::cout << "PASS AMD FSR 3.1.4 Frame Generation offscreen "
                  << displayW << "x" << displayH
                  << ", prepare + optical flow + interpolation + readback"
                  << std::endl;
    }

    static float half(uint16_t value) {
        int exponent=(value>>10)&31;
        float mantissa=float(value&1023)/1024;
        float result=exponent==0 ? std::ldexp(mantissa,-14) : exponent==31 ? INFINITY : std::ldexp(1+mantissa,exponent-15);
        return value&0x8000 ? -result : result;
    }
    void readback(Image output) {
        if (staging) { vkDestroyBuffer(device,staging,nullptr); vkFreeMemory(device,stagingMemory,nullptr); }
        VkDeviceSize size=output.w*output.h*8;
        VkBufferCreateInfo create{VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO};
        create.size=size; create.usage=VK_BUFFER_USAGE_TRANSFER_DST_BIT;
        check(vkCreateBuffer(device,&create,nullptr,&staging));
        VkMemoryRequirements requirements{}; vkGetBufferMemoryRequirements(device,staging,&requirements);
        VkMemoryAllocateInfo allocate{VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO};
        allocate.allocationSize=requirements.size;
        allocate.memoryTypeIndex=memoryType(requirements.memoryTypeBits,VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT|VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
        check(vkAllocateMemory(device,&allocate,nullptr,&stagingMemory));
        check(vkBindBufferMemory(device,staging,stagingMemory,0));
        begin();
        barrier(output,VK_IMAGE_LAYOUT_GENERAL,VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,VK_ACCESS_SHADER_WRITE_BIT,VK_ACCESS_TRANSFER_READ_BIT);
        VkBufferImageCopy copy{};
        copy.imageSubresource={VK_IMAGE_ASPECT_COLOR_BIT,0,0,1}; copy.imageExtent={output.w,output.h,1};
        vkCmdCopyImageToBuffer(command,output.image,VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,staging,1,&copy);
        submit();
        void* ptr=nullptr; check(vkMapMemory(device,stagingMemory,0,size,0,&ptr));
        const auto* data=static_cast<const uint16_t*>(ptr);
        double sum[3]{};
        for (uint32_t pixel=0; pixel<output.w*output.h; pixel++) for(int channel=0;channel<3;channel++) {
            float value=half(data[pixel*4+channel]);
            if(!std::isfinite(value) || value < -0.01f || value > 1.1f) throw std::runtime_error("Invalid FSR output pixel");
            sum[channel]+=value;
        }
        vkUnmapMemory(device,stagingMemory);
        for (int channel=0;channel<3;channel++)
            if(std::abs(sum[channel]/(output.w*output.h)-0.25*(channel+1))>0.06)
                throw std::runtime_error("FSR output does not reconstruct the known input color");
    }
};
int main() {
    try {
        if (slavicFsrFrameGenerationComponents() != 7u)
            throw std::runtime_error("FSR3 Frame Generation components are not linked");
        std::cout << "PASS FSR3 FG components: FSR3 + Frame Interpolation + Optical Flow" << std::endl;
        Test test;
        test.runFrameGeneration();
        test.run(64,64);
        test.run(80,48);
        return 0;
    }
    catch(const std::exception& error) { std::cerr << error.what() << std::endl; return 1; }
}
