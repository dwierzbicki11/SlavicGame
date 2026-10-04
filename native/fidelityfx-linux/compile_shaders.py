#!/usr/bin/env python3
"""Compile AMD GLSL with a portable build-time SPIR-V binding reflector.

The provider embeds FP32 Vulkan 1.0 binaries using AMD's shared-memory SPD path.
Wave64/FP16 requests use these binaries; no subgroup or shaderFloat16 is required.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import struct
import subprocess

PASSES = ['prepare_inputs', 'luma_pyramid', 'shading_change_pyramid',
          'shading_change', 'prepare_reactivity', 'luma_instability',
          'accumulate', 'rcas', 'debug_view', 'autogen_reactive']
DEFINES = ['REPROJECT_USE_LANCZOS_TYPE', 'HDR_COLOR_INPUT',
           'LOW_RESOLUTION_MOTION_VECTORS', 'JITTERED_MOTION_VECTORS',
           'INVERTED_DEPTH', 'APPLY_SHARPENING']
GROUPS = ['cbv', 'srvTexture', 'uavTexture', 'srvBuffer', 'uavBuffer', 'sampler', 'rtAccelStruct']
FIELDS = ['ConstantBuffer', 'SRVTexture', 'UAVTexture', 'SRVBuffer', 'UAVBuffer', 'Sampler', 'RTAccelerationStructure']
BIND_FIELDS = ['ConstantBuffers', 'SRVTextures', 'UAVTextures', 'SRVBuffers', 'UAVBuffers', 'Samplers', 'RTAccelerationStructures']

def bindings(data):
    words = struct.unpack('<%dI' % (len(data) // 4), data)
    if words[0] != 0x07230203:
        raise ValueError('Invalid SPIR-V magic')
    names, decorations, types, constants, variables = {}, {}, {}, {}, []
    i = 5
    while i < len(words):
        n, op = words[i] >> 16, words[i] & 65535
        if not n or i + n > len(words):
            raise ValueError('Malformed SPIR-V instruction')
        a = words[i + 1:i + n]
        if op == 5:  # OpName
            names[a[0]] = struct.pack('<%dI' % (len(a) - 1), *a[1:]).split(b'\0')[0].decode()
        elif op == 71 and a[1] in (33, 34):  # Binding / DescriptorSet
            decorations.setdefault(a[0], {})[a[1]] = a[2]
        elif op in (25, 26, 27, 28, 29, 30, 32):
            types[a[0]] = (op, a[1:])
        elif op == 43:
            constants[a[1]] = a[2]
        elif op == 59:
            variables.append(a[:3])
        i += n
    result = {g: [] for g in GROUPS}
    for pointer, ident, storage in variables:
        dec = decorations.get(ident, {})
        if 33 not in dec:
            continue
        if dec.get(34, 0) != 0:
            raise ValueError('Unexpected descriptor set')
        op, typ = types[pointer]
        if op != 32:
            raise ValueError('Descriptor without pointer type')
        base, count = typ[1], 1
        op, typ = types[base]
        if op == 28:
            count = constants[typ[1]]
            op, typ = types[typ[0]]
        if storage == 2 and op == 30:
            group = 'cbv'
        elif storage == 12:
            group = 'uavBuffer'
        elif storage == 0 and op == 26:
            group = 'sampler'
        elif storage == 0 and op == 25:
            group = 'srvTexture' if typ[5] == 1 else 'uavTexture'
        else:
            raise ValueError(f'Unhandled descriptor type {op}, storage {storage}')
        name = names.get(ident)
        if not name:
            raise ValueError('Missing descriptor name needed by AMD resource mapping')
        result[group].append((name, dec[33], count))
    for values in result.values():
        values.sort(key=lambda x: x[1])
    if not result['cbv']:
        raise ValueError('FSR shader missing constant buffer')
    return result

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--sdk', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--compiler', required=True)
    args = parser.parse_args()
    gpu = args.sdk / 'sdk/include/FidelityFX/gpu'
    source = args.sdk / 'sdk/src/backends/vk/shaders/fsr3upscaler'
    out = args.output / 'spirv'
    out.mkdir(parents=True, exist_ok=True)
    base = ['FFX_GPU=1', 'FFX_GLSL=1', 'FFX_HALF=0', 'FFX_SPD_NO_WAVE_OPERATIONS=1',
            'FFX_FSR3UPSCALER_OPTION_UPSAMPLE_SAMPLERS_USE_DATA_HALF=0',
            'FFX_FSR3UPSCALER_OPTION_ACCUMULATE_SAMPLERS_USE_DATA_HALF=0',
            'FFX_FSR3UPSCALER_OPTION_REPROJECT_SAMPLERS_USE_DATA_HALF=1',
            'FFX_FSR3UPSCALER_OPTION_POSTPROCESSLOCKSTATUS_SAMPLERS_USE_DATA_HALF=0',
            'FFX_FSR3UPSCALER_OPTION_UPSAMPLE_USE_LANCZOS_TYPE=2']
    def compile_one(job):
        pass_name, bits = job
        path = out / f'{pass_name}_{bits}.spv'
        options = base + [f'FFX_FSR3UPSCALER_OPTION_{name}={(bits >> bit) & 1}'
                          for bit, name in enumerate(DEFINES)]
        command = [args.compiler, '-V', '--target-env', 'vulkan1.0', '-S', 'comp', '-Os',
                   '-I' + str(gpu), '-I' + str(gpu / 'fsr3upscaler')]
        command += ['-D' + d for d in options]
        command += [str(source / f'ffx_fsr3upscaler_{pass_name}_pass.glsl'), '-o', str(path)]
        subprocess.run(command, check=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        data = path.read_bytes()
        return pass_name, bits, data, bindings(data)
    # Four compiler processes keeps memory bounded on developer laptops/CI.
    with ThreadPoolExecutor(max_workers=4) as executor:
        compiled = list(executor.map(compile_one, ((p, b) for p in PASSES for b in range(64))))
    chunks = ['#include <FidelityFX/host/ffx_fsr3upscaler.h>\n#include <ffx_shader_blobs.h>\n#include <cstring>\n']
    unique, table = {}, {}
    for name, bits, data, resources in compiled:
        digest = hashlib.sha256(data).hexdigest()
        if digest not in unique:
            symbol = f's{len(unique)}'
            unique[digest] = symbol
            words = struct.unpack('<%dI' % (len(data) // 4), data)
            chunks.append(f'static const uint32_t {symbol}_data[] = {{' + ','.join(hex(w) for w in words) + '};\n')
            for group in GROUPS:
                values = resources[group]
                if values:
                    chunks.append(f'static const char* {symbol}_{group}_names[] = {{' + ','.join(json.dumps(x[0]) for x in values) + '};\n')
                    for suffix, value in [('slots', 1), ('counts', 2)]:
                        chunks.append(f'static const uint32_t {symbol}_{group}_{suffix}[] = {{' + ','.join(str(x[value]) for x in values) + '};\n')
                    chunks.append(f'static const uint32_t {symbol}_{group}_spaces[] = {{' + ','.join('0' for _ in values) + '};\n')
            fields = [f'(const uint8_t*){symbol}_data', str(len(data))] + [str(len(resources[g])) for g in GROUPS]
            for g in GROUPS:
                fields += [f'{symbol}_{g}_{s}' if resources[g] else 'nullptr' for s in ['names','slots','counts','spaces']]
            chunks.append(f'static const FfxShaderBlob {symbol} = {{' + ','.join(fields) + '};\n')
        table[name, bits] = unique[digest]
    order = PASSES[:7] + ['accumulate'] + PASSES[7:]
    chunks.append('static const FfxShaderBlob* shaders[11][64] = {\n')
    for p in order:
        chunks.append('{' + ','.join('&' + table[p, b] for b in range(64)) + '},\n')
    chunks.append('};\nextern "C" FfxErrorCode ffxGetPermutationBlobByIndex(FfxEffect effect, FfxPass pass, FfxBindStage stage, uint32_t bits, FfxShaderBlob* blob) {\n'
                  ' if(effect != FFX_EFFECT_FSR3UPSCALER || pass >= 11 || !blob || stage != FFX_BIND_COMPUTE_SHADER_STAGE) return FFX_ERROR_INVALID_ARGUMENT;\n'
                  ' std::memcpy(blob, shaders[pass][bits & 63], sizeof(*blob)); return FFX_OK; }\n'
                  'extern "C" FfxErrorCode ffxIsWave64(FfxEffect, uint32_t, bool& wave64) { wave64 = false; return FFX_OK; }\n')
    (args.output / 'fsr_shaders.cpp').write_text(''.join(chunks))
    print(f'FSR3: compiled {len(compiled)} permutations, embedded {len(unique)} unique FP32 shaders', flush=True)

if __name__ == '__main__':
    main()
