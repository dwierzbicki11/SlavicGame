#!/usr/bin/env python3
"""Build pinned Veldrid 4.9 sources with a narrow Vulkan creation API patch.

Upstream stays unmodified. All generated files live in the disposable cache.
The original backends, public API and dependency versions are retained.
"""
import hashlib
import os
from pathlib import Path
import shutil
import subprocess
import time

ROOT = Path(__file__).resolve().parent.parent
SHA = 'a121087cadf38755f28c397a5b3c42ff1c559a19'
CACHE = ROOT / '.cache'
SOURCE = Path(os.environ.get('SLAVICGAME_VELDRID_SOURCE', CACHE / 'veldrid-4.9.0-upstream'))
OUTPUT = CACHE / 'veldrid-4.9.0-patched'
FINGERPRINT = hashlib.sha256(Path(__file__).read_bytes()).hexdigest()


def replace_once(text, old, new):
    if text.count(old) != 1:
        raise RuntimeError('Pinned Veldrid source changed: ' + old[:100])
    return text.replace(old, new)


def prepare():
    if not (SOURCE / '.git').exists():
        subprocess.run(['git', 'clone', '--depth', '1', '--branch', 'v4.9.0',
                        'https://github.com/veldrid/veldrid.git', str(SOURCE)], check=True)
    actual = subprocess.check_output(['git', '-C', str(SOURCE), 'rev-parse', 'HEAD'], text=True).strip()
    dirty = subprocess.check_output(['git', '-C', str(SOURCE), 'status', '--porcelain',
                                     '--untracked-files=no'], text=True).strip()
    if actual != SHA or dirty:
        raise RuntimeError('Veldrid source must be clean v4.9.0 at ' + SHA)
    if (OUTPUT / 'stamp').exists() and (OUTPUT / 'stamp').read_text() == FINGERPRINT:
        return
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for project in ['Veldrid', 'Veldrid.OpenGLBindings', 'Veldrid.MetalBindings']:
        target = OUTPUT / 'src' / project
        if target.exists():
            shutil.rmtree(target)
        shutil.copytree(SOURCE / 'src' / project, target)

    path = OUTPUT / 'src/Veldrid/VulkanDeviceOptions.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '        public string[] DeviceExtensions;', '''        public string[] DeviceExtensions;

        // VK_MAKE_API_VERSION encoding. Zero preserves the upstream 1.0 default.
        public uint InstanceApiVersion;
        // Null preserves upstream feature selection. True enables the feature
        // only when supported; false is useful for capability/fallback tests.
        public bool? EnableShaderStorageImageExtendedFormats;''')
    text = replace_once(text, '            DeviceExtensions = deviceExtensions;', '''            DeviceExtensions = deviceExtensions;
            InstanceApiVersion = 0;
            EnableShaderStorageImageExtendedFormats = null;''')
    text += '''
namespace Veldrid
{
    public sealed class VulkanInstanceCreationException : VeldridException
    {
        public int ResultCode { get; }
        public VulkanInstanceCreationException(int result)
            : base("Vulkan instance creation failed with VkResult " + result)
        { ResultCode = result; }
    }
}
'''
    path.write_text(text)

    path = OUTPUT / 'src/Veldrid/Vk/VkGraphicsDevice.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '        public VkInstance Instance => _instance;', '''        public VkInstance Instance => _instance;
        public uint InstanceApiVersion { get; private set; }
        public uint PhysicalDeviceApiVersion => _physicalDeviceProperties.apiVersion;
        public bool ShaderStorageImageExtendedFormatsEnabled { get; private set; }''')
    # Only the production instance, never CheckIsSupported's throwaway probe.
    old = '            applicationInfo.apiVersion = new VkVersion(1, 0, 0);'
    if text.count(old) != 2:
        raise RuntimeError('Pinned Veldrid instance creation sites changed')
    text = text.replace(old, '''            InstanceApiVersion = options.InstanceApiVersion == 0
                ? (uint)new VkVersion(1, 0, 0) : options.InstanceApiVersion;
            applicationInfo.apiVersion = InstanceApiVersion;''', 1)
    text = replace_once(text, '            VkPhysicalDeviceFeatures deviceFeatures = _physicalDeviceFeatures;', '''            VkPhysicalDeviceFeatures deviceFeatures = _physicalDeviceFeatures;
            deviceFeatures.shaderStorageImageExtendedFormats =
                options.EnableShaderStorageImageExtendedFormats != false
                && _physicalDeviceFeatures.shaderStorageImageExtendedFormats;
            ShaderStorageImageExtendedFormatsEnabled = deviceFeatures.shaderStorageImageExtendedFormats;''')
    # Upstream CheckResult is Conditional(DEBUG); these creation checks must
    # also run in Release before any invalid instance/device handle is used.
    text = replace_once(text, '''            VkResult result = vkCreateInstance(ref instanceCI, null, out _instance);
            CheckResult(result);''', '''            VkResult result = vkCreateInstance(ref instanceCI, null, out _instance);
            if (result != VkResult.Success)
                throw new VulkanInstanceCreationException((int)result);''')
    text = replace_once(text, '''                VkResult result = vkCreateDevice(_physicalDevice, ref deviceCreateInfo, null, out _device);
                CheckResult(result);''', '''                VkResult result = vkCreateDevice(_physicalDevice, ref deviceCreateInfo, null, out _device);
                if (result != VkResult.Success)
                    throw new VeldridException("Vulkan device creation failed with VkResult " + result);''')
    text = replace_once(text, '''        private protected override void SwapBuffersCore(Swapchain swapchain)
        {''', '''        internal VulkanPresentationResult SubmitCommandsAndPresent(CommandList commands, Swapchain swapchain)
        {
            VkSwapchain sc = Util.AssertSubtype<Swapchain, VkSwapchain>(swapchain);
            VkSemaphore finished = sc.GetRenderFinishedSemaphore();
            // Acquisition has already completed on the host fence. Signal a
            // per-image semaphore so even a separate present queue waits for
            // this image's final composition and PRESENT layout transition.
            SubmitCommandList(commands, 0, null, 1, &finished, null);
            return PresentAndAcquire(sc, finished);
        }

        private protected override void SwapBuffersCore(Swapchain swapchain)
        {
            PresentAndAcquire(Util.AssertSubtype<Swapchain, VkSwapchain>(swapchain), VkSemaphore.Null);
        }

        private VulkanPresentationResult PresentAndAcquire(VkSwapchain vkSC, VkSemaphore finished)
        {''')
    text = replace_once(text, '''            VkSwapchain vkSC = Util.AssertSubtype<Swapchain, VkSwapchain>(swapchain);
            VkSwapchainKHR deviceSwapchain = vkSC.DeviceSwapchain;''', '''            VkSwapchainKHR deviceSwapchain = vkSC.DeviceSwapchain;''')
    text = replace_once(text, '''            presentInfo.pImageIndices = &imageIndex;

            object presentLock''', '''            presentInfo.pImageIndices = &imageIndex;
            if (finished != VkSemaphore.Null)
            {
                presentInfo.waitSemaphoreCount = 1;
                presentInfo.pWaitSemaphores = &finished;
            }
            ulong generation = vkSC.Generation;
            uint mode = vkSC.PresentMode;
            VkResult result;
            long timestamp;

            object presentLock''')
    text = replace_once(text, '''                vkQueuePresentKHR(vkSC.PresentQueue, ref presentInfo);
                if (vkSC.AcquireNextImage(_device, VkSemaphore.Null, vkSC.ImageAvailableFence))
                {
                    Vulkan.VkFence fence = vkSC.ImageAvailableFence;
                    vkWaitForFences(_device, 1, ref fence, true, ulong.MaxValue);
                    vkResetFences(_device, 1, ref fence);
                }
            }
        }''', '''                result = vkQueuePresentKHR(vkSC.PresentQueue, ref presentInfo);
                timestamp = Stopwatch.GetTimestamp();
            }
            bool presented = result == VkResult.Success || result == VkResult.SuboptimalKHR;
            if (presented) vkSC.NotifyPresented(imageIndex);
            else if (result != VkResult.ErrorOutOfDateKHR)
                throw new VeldridException("Vulkan present failed with VkResult " + result);
            if (result == VkResult.ErrorOutOfDateKHR || result == VkResult.SuboptimalKHR)
                vkSC.Resize(vkSC.Framebuffer.Width, vkSC.Framebuffer.Height);
            else if (vkSC.AcquireNextImage(_device, VkSemaphore.Null, vkSC.ImageAvailableFence))
                vkSC.WaitForImageAvailable();
            return new VulkanPresentationResult(presented, imageIndex, generation, mode, timestamp);
        }''')
    text = replace_once(text, '''                VkResult result = vkQueueSubmit(_graphicsQueue, 1, ref si, vkFence);
                CheckResult(result);''', '''                VkResult result = vkQueueSubmit(_graphicsQueue, 1, ref si, vkFence);
                if (result != VkResult.Success)
                    throw new VeldridException("Vulkan queue submission failed with VkResult " + result);''')
    path.write_text(text)

    path = OUTPUT / 'src/Veldrid/BackendInfoVulkan.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '        public IntPtr Instance => _gd.Instance.Handle;', '''        public IntPtr Instance => _gd.Instance.Handle;

        // Creation facts, deliberately distinct from advertised GPU/driver versions.
        public uint InstanceApiVersion => _gd.InstanceApiVersion;
        public uint PhysicalDeviceApiVersion => _gd.PhysicalDeviceApiVersion;
        public bool ShaderStorageImageExtendedFormatsEnabled => _gd.ShaderStorageImageExtendedFormatsEnabled;''')
    text = replace_once(text, '        public IntPtr Device => _gd.Device.Handle;', '''        public IntPtr Device => _gd.Device.Handle;

        // Submit this image's final draw, signal its render-finished semaphore,
        // queue present with that wait, and acquire the next image by host fence.
        public VulkanPresentationResult SubmitCommandsAndPresent(CommandList commands, Swapchain swapchain)
            => _gd.SubmitCommandsAndPresent(commands, swapchain);

        public void ConfigureFrameGenerationPresentation(Swapchain swapchain, bool enabled)
            => Util.AssertSubtype<Swapchain, VkSwapchain>(swapchain).ConfigureFrameGenerationPresentation(enabled);

        public uint GetSwapchainPresentMode(Swapchain swapchain)
            => Util.AssertSubtype<Swapchain, VkSwapchain>(swapchain).PresentMode;

        public bool CanReadSwapchainImages(Swapchain swapchain)
            => Util.AssertSubtype<Swapchain, VkSwapchain>(swapchain).CanReadImages;''')
    text = replace_once(text, '    public class BackendInfoVulkan', '''    public readonly struct VulkanPresentationResult
    {
        public readonly bool Presented;
        public readonly uint ImageIndex;
        public readonly ulong Generation;
        public readonly uint PresentMode;
        public readonly long Timestamp;
        internal VulkanPresentationResult(bool presented, uint imageIndex, ulong generation, uint mode, long timestamp)
        { Presented = presented; ImageIndex = imageIndex; Generation = generation; PresentMode = mode; Timestamp = timestamp; }
    }

    public class BackendInfoVulkan''')
    path.write_text(text)
    prepare_swapchain()

    path = OUTPUT / 'src/Veldrid/Vk/VkCommandList.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '''            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
                _currentFramebuffer.TransitionToFinalLayout(_cb);
            }

            vkEndCommandBuffer(_cb);''', '''            if (_activeRenderPass != VkRenderPass.Null)
            {
                EndCurrentRenderPass();
            }
            // CopyTexture may have ended the render pass and transitioned
            // its image to TRANSFER_SRC. End still owes the framebuffer's
            // final layout, especially PRESENT_SRC before queue presentation.
            _currentFramebuffer?.TransitionToFinalLayout(_cb);

            VkResult endResult = vkEndCommandBuffer(_cb);
            if (endResult != VkResult.Success)
                throw new VeldridException("Vulkan command buffer end failed: " + endResult);''')
    path.write_text(text)

    path = OUTPUT / 'src/Veldrid/Vk/VulkanUtil.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '''            else if (oldLayout == VkImageLayout.General && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.TransferRead;
                barrier.dstAccessMask = VkAccessFlags.ShaderRead;
                srcStageFlags = VkPipelineStageFlags.Transfer;
                dstStageFlags = VkPipelineStageFlags.FragmentShader;
            }''', '''            else if (oldLayout == VkImageLayout.General && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                // GENERAL may contain native AMD compute writes as well as
                // transfers. Make those writes visible to the actual sampler.
                barrier.srcAccessMask = VkAccessFlags.MemoryRead | VkAccessFlags.MemoryWrite;
                barrier.dstAccessMask = VkAccessFlags.ShaderRead;
                srcStageFlags = VkPipelineStageFlags.AllCommands;
                dstStageFlags = VkPipelineStageFlags.FragmentShader | VkPipelineStageFlags.ComputeShader;
            }''')
    text = replace_once(text, '''            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.General)
            {
                barrier.srcAccessMask = VkAccessFlags.ShaderRead;
                barrier.dstAccessMask = VkAccessFlags.ShaderRead;
                srcStageFlags = VkPipelineStageFlags.FragmentShader;
                dstStageFlags = VkPipelineStageFlags.ComputeShader;
            }''', '''            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.General)
            {
                barrier.srcAccessMask = VkAccessFlags.ShaderRead;
                barrier.dstAccessMask = VkAccessFlags.ShaderRead | VkAccessFlags.ShaderWrite;
                srcStageFlags = VkPipelineStageFlags.FragmentShader | VkPipelineStageFlags.ComputeShader;
                dstStageFlags = VkPipelineStageFlags.ComputeShader;
            }''')
    text = replace_once(text, '''            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.PresentSrcKHR)
            {
                barrier.srcAccessMask = VkAccessFlags.TransferWrite;''', '''            else if ((oldLayout == VkImageLayout.TransferDstOptimal || oldLayout == VkImageLayout.TransferSrcOptimal)
                && newLayout == VkImageLayout.PresentSrcKHR)
            {
                barrier.srcAccessMask = oldLayout == VkImageLayout.TransferDstOptimal
                    ? VkAccessFlags.TransferWrite : VkAccessFlags.TransferRead;''')
    text = replace_once(text, '                Debug.Fail("Invalid image layout transition.");',
                        '                throw new VeldridException("Unsupported Vulkan image layout transition: " + oldLayout + " -> " + newLayout);')
    path.write_text(text)
    shutil.copyfile(SOURCE / 'LICENSE', OUTPUT / 'LICENSE')

    # Upstream creates RGBA16F for an RG16F description. Besides invalid AMD
    # views, this doubles GPU readback size over the staging buffer allocation.
    path = OUTPUT / 'src/Veldrid/Vk/VkFormats.VdToVkPixelFormat.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '''                case PixelFormat.R16_G16_Float:
                    return VkFormat.R16g16b16a16Sfloat;''', '''                case PixelFormat.R16_G16_Float:
                    return VkFormat.R16g16Sfloat;''')
    path.write_text(text)
    (OUTPUT / 'stamp').write_text(FINGERPRINT)


def prepare_swapchain():
    path = OUTPUT / 'src/Veldrid/Vk/VkSwapchain.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '        private bool _disposed;', '''        private bool _disposed;
        private bool _frameGenerationPresentation;
        private VkSemaphore[] _renderFinished;
        private readonly System.Collections.Generic.List<Tuple<VkSwapchainKHR, VkSemaphore[]>> _retired
            = new System.Collections.Generic.List<Tuple<VkSwapchainKHR, VkSemaphore[]>>();
        private uint _firstPresentedImage = uint.MaxValue;
        public ulong Generation { get; private set; }
        public uint PresentMode { get; private set; }
        public bool CanReadImages { get; private set; }

        public void ConfigureFrameGenerationPresentation(bool enabled)
        {
            if (_frameGenerationPresentation == enabled) return;
            _frameGenerationPresentation = enabled;
            RecreateAndReacquire(_framebuffer.Width, _framebuffer.Height);
        }

        public VkSemaphore GetRenderFinishedSemaphore()
        {
            if (_renderFinished == null)
            {
                uint count = 0;
                VkResult result = vkGetSwapchainImagesKHR(_gd.Device, _deviceSwapchain, ref count, null);
                if (result != VkResult.Success) throw new VeldridException("Cannot enumerate presentation images: " + result);
                _renderFinished = new VkSemaphore[count];
                VkSemaphoreCreateInfo ci = VkSemaphoreCreateInfo.New();
                for (int i = 0; i < _renderFinished.Length; i++)
                {
                    result = vkCreateSemaphore(_gd.Device, ref ci, null, out _renderFinished[i]);
                    if (result != VkResult.Success) throw new VeldridException("Cannot create presentation semaphore: " + result);
                }
            }
            // Only an acquisition fence for this very image proves that its
            // previous present wait has completed. A command-ring fence does
            // not prove that, and must not select the presentation semaphore.
            return _renderFinished[_currentImageIndex];
        }

        public void NotifyPresented(uint imageIndex)
        {
            if (_firstPresentedImage == uint.MaxValue) _firstPresentedImage = imageIndex;
        }

        public void WaitForImageAvailable()
        {
            VkResult result = vkWaitForFences(_gd.Device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
            if (result != VkResult.Success) throw new VeldridException("Vulkan acquire fence failed: " + result);
            result = vkResetFences(_gd.Device, 1, ref _imageAvailableFence);
            if (result != VkResult.Success) throw new VeldridException("Vulkan acquire fence reset failed: " + result);
            if (_currentImageIndex == _firstPresentedImage)
            {
                // The first present on the new swapchain has completed. WSI
                // has also finished presenting the retired swapchains. This
                // follows the Khronos swapchain-recreation sample; WaitIdle
                // by itself would not establish present-wait retirement.
                foreach (var retired in _retired)
                {
                    DestroySemaphores(retired.Item2);
                    vkDestroySwapchainKHR(_gd.Device, retired.Item1, null);
                }
                _retired.Clear();
            }
        }

        private void DestroySemaphores(VkSemaphore[] semaphores)
        {
            if (semaphores == null) return;
            foreach (var semaphore in semaphores)
                if (semaphore != VkSemaphore.Null) vkDestroySemaphore(_gd.Device, semaphore, null);
        }''')
    text = replace_once(text, '''            vkCreateFence(_gd.Device, ref fenceCI, null, out _imageAvailableFence);

            AcquireNextImage(_gd.Device, VkSemaphore.Null, _imageAvailableFence);
            vkWaitForFences(_gd.Device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
            vkResetFences(_gd.Device, 1, ref _imageAvailableFence);''', '''            VkResult fenceResult = vkCreateFence(_gd.Device, ref fenceCI, null, out _imageAvailableFence);
            if (fenceResult != VkResult.Success) throw new VeldridException("Vulkan acquire fence creation failed: " + fenceResult);

            if (AcquireNextImage(_gd.Device, VkSemaphore.Null, _imageAvailableFence))
                WaitForImageAvailable();''')
    text = replace_once(text, '''            _framebuffer.SetImageIndex(_currentImageIndex);
            if (result == VkResult.ErrorOutOfDateKHR || result == VkResult.SuboptimalKHR)
            {
                CreateSwapchain(_framebuffer.Width, _framebuffer.Height);
                return false;
            }
            else if (result != VkResult.Success)''', '''            if (result == VkResult.ErrorOutOfDateKHR)
            {
                RecreateAndReacquire(_framebuffer.Width, _framebuffer.Height);
                return false;
            }
            else if (result != VkResult.Success && result != VkResult.SuboptimalKHR)''')
    text = replace_once(text, '''            return true;
        }

        private void RecreateAndReacquire''', '''            // SUBOPTIMAL is a successful acquisition and signals the fence.
            // Consume that signal before any possible recreation/reset.
            _framebuffer.SetImageIndex(_currentImageIndex);
            return true;
        }

        private void RecreateAndReacquire''')
    text = replace_once(text, '''                    vkWaitForFences(_gd.Device, 1, ref _imageAvailableFence, true, ulong.MaxValue);
                    vkResetFences(_gd.Device, 1, ref _imageAvailableFence);''', '''                    WaitForImageAvailable();''')
    text = replace_once(text, '''            if (_syncToVBlank)
            {''', '''            if (_frameGenerationPresentation)
            {
                // MAILBOX may drop the intermediate image. FIFO preserves
                // output order; IMMEDIATE requires the application's pacer.
                presentMode = !_syncToVBlank && presentModes.Contains(VkPresentModeKHR.ImmediateKHR)
                    ? VkPresentModeKHR.ImmediateKHR : VkPresentModeKHR.FifoKHR;
            }
            else if (_syncToVBlank)
            {''')
    text = replace_once(text, '''            swapchainCI.imageUsage = VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferDst;''', '''            swapchainCI.imageUsage = VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferDst;
            CanReadImages = (surfaceCapabilities.supportedUsageFlags & VkImageUsageFlags.TransferSrc) != 0;
            if (CanReadImages) swapchainCI.imageUsage |= VkImageUsageFlags.TransferSrc;''')
    text = replace_once(text, '''            CheckResult(result);
            if (oldSwapchain != VkSwapchainKHR.Null)
            {
                vkDestroySwapchainKHR(_gd.Device, oldSwapchain, null);
            }

            _framebuffer.SetNewSwapchain''', '''            if (result != VkResult.Success) throw new VeldridException("Vulkan swapchain creation failed: " + result);
            if (oldSwapchain != VkSwapchainKHR.Null)
                _retired.Add(Tuple.Create(oldSwapchain, _renderFinished));
            _renderFinished = null;
            _firstPresentedImage = uint.MaxValue;
            Generation++;
            PresentMode = (uint)presentMode;

            _framebuffer.SetNewSwapchain''')
    text = replace_once(text, '''            vkDestroyFence(_gd.Device, _imageAvailableFence, null);''', '''            // Terminal shutdown follows the unextended Vulkan convention;
            // ordinary resize retires through an actual reacquisition above.
            _gd.WaitForIdle();
            DestroySemaphores(_renderFinished);
            foreach (var retired in _retired)
            {
                DestroySemaphores(retired.Item2);
                vkDestroySwapchainKHR(_gd.Device, retired.Item1, null);
            }
            _retired.Clear();
            vkDestroyFence(_gd.Device, _imageAvailableFence, null);''')
    path.write_text(text)

    # Re-query image count after every recreation: surfaces may change it.
    path = OUTPUT / 'src/Veldrid/Vk/VkSwapchainFramebuffer.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '            if (_scImages == null)',
                        '            if (_scImages == null || _scImages.Length != scImageCount)')
    path.write_text(text)


if __name__ == '__main__':
    CACHE.mkdir(exist_ok=True)
    lock = CACHE / 'veldrid-prepare.lock'
    deadline = time.monotonic() + 180
    while True:
        try:
            lock.mkdir()
            break
        except FileExistsError:
            if time.monotonic() > deadline:
                raise RuntimeError('Timed out waiting for Veldrid source preparation')
            time.sleep(0.1)
    try:
        prepare()
    finally:
        lock.rmdir()
