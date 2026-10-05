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
    path.write_text(text)

    path = OUTPUT / 'src/Veldrid/BackendInfoVulkan.cs'
    text = path.read_text(encoding='utf-8-sig')
    text = replace_once(text, '        public IntPtr Instance => _gd.Instance.Handle;', '''        public IntPtr Instance => _gd.Instance.Handle;

        // Creation facts, deliberately distinct from advertised GPU/driver versions.
        public uint InstanceApiVersion => _gd.InstanceApiVersion;
        public uint PhysicalDeviceApiVersion => _gd.PhysicalDeviceApiVersion;
        public bool ShaderStorageImageExtendedFormatsEnabled => _gd.ShaderStorageImageExtendedFormatsEnabled;''')
    path.write_text(text)
    shutil.copyfile(SOURCE / 'LICENSE', OUTPUT / 'LICENSE')
    (OUTPUT / 'stamp').write_text(FINGERPRINT)


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
