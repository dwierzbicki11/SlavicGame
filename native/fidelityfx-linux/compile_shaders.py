#!/usr/bin/env python3
"""Compile AMD FSR3/Frame-Interpolation/Optical-Flow GLSL to embedded SPIR-V.

The Linux provider intentionally builds FP32 Vulkan binaries from the exact
pinned FidelityFX SDK source. The backend advertises neither FP16 nor forced
wave64, so those permutations are not needed. FSR3 upscaling keeps its full
existing option matrix; Frame Interpolation needs three boolean options and
Optical Flow needs the HDR-input option.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import struct
import subprocess

FSR_PASSES = ['prepare_inputs', 'luma_pyramid', 'shading_change_pyramid',
              'shading_change', 'prepare_reactivity', 'luma_instability',
              'accumulate', 'rcas', 'debug_view', 'autogen_reactive']
FSR_DEFINES = ['REPROJECT_USE_LANCZOS_TYPE', 'HDR_COLOR_INPUT',
               'LOW_RESOLUTION_MOTION_VECTORS', 'JITTERED_MOTION_VECTORS',
               'INVERTED_DEPTH', 'APPLY_SHARPENING']

# Order must match FfxFrameInterpolationPass in SDK v1.1.4.
FI_PASSES = [
    'reconstruct_and_dilate',
    'setup',
    'reconstruct_previous_depth',
    'game_motion_vector_field',
    'optical_flow_vector_field',
    'disocclusion_mask',
    '__interpolation__',
    'compute_inpainting_pyramid',
    'inpainting',
    'compute_game_vector_field_inpainting_pyramid',
    'debug_view',
]
FI_DEFINES = [
    'LOW_RES_MOTION_VECTORS',
    'JITTER_MOTION_VECTORS',
    'INVERTED_DEPTH',
]

# Order must match FfxOpticalflowPass in SDK v1.1.4.
OF_PASSES = [
    'prepare_luma_pass',
    'compute_luminance_pyramid_pass',
    'generate_scd_histogram_pass',
    'compute_scd_divergence_pass',
    'compute_optical_flow_advanced_pass_v5',
    'filter_optical_flow_pass_v5',
    'scale_optical_flow_advanced_pass_v5',
]

GROUPS = ['cbv', 'srvTexture', 'uavTexture', 'srvBuffer', 'uavBuffer',
          'sampler', 'rtAccelStruct']


def bindings(data):
    words = struct.unpack('<%dI' % (len(data) // 4), data)
    if words[0] != 0x07230203:
        raise ValueError('Invalid SPIR-V magic')
    names, decorations, types, constants, variables = {}, {}, {}, {}, []
    non_writable_members = {}
    i = 5
    while i < len(words):
        n, op = words[i] >> 16, words[i] & 65535
        if not n or i + n > len(words):
            raise ValueError('Malformed SPIR-V instruction')
        a = words[i + 1:i + n]
        if op == 5:  # OpName
            names[a[0]] = struct.pack(
                '<%dI' % (len(a) - 1), *a[1:]).split(b'\0')[0].decode()
        elif op == 71:  # OpDecorate
            if a[1] in (33, 34):  # Binding / DescriptorSet
                decorations.setdefault(a[0], {})[a[1]] = a[2]
            elif a[1] in (2, 3, 24):  # Block / BufferBlock / NonWritable
                decorations.setdefault(a[0], {})[a[1]] = True
        elif op == 72 and a[2] == 24:  # OpMemberDecorate NonWritable
            non_writable_members.setdefault(a[0], set()).add(a[1])
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
            base = typ[0]
            op, typ = types[base]
        read_only = (dec.get(24) or decorations.get(base, {}).get(24) or
                     (op == 30 and len(non_writable_members.get(base, ())) == len(typ)))
        if storage == 2 and op == 30:
            # Vulkan 1.0 encodes SSBOs as Uniform + BufferBlock. Treating
            # these as constant buffers misbinds AMD's counters and prevents
            # its SPD pyramids from running.
            block = decorations.get(base, {})
            if block.get(3):
                group = 'srvBuffer' if read_only else 'uavBuffer'
            elif block.get(2):
                group = 'cbv'
            else:
                raise ValueError('Uniform struct without a block decoration')
        elif storage == 12:
            group = 'srvBuffer' if read_only else 'uavBuffer'
        elif storage == 0 and op == 26:
            group = 'sampler'
        elif storage == 0 and op == 25:
            group = 'srvTexture' if typ[5] == 1 else 'uavTexture'
        else:
            raise ValueError(
                f'Unhandled descriptor type {op}, storage {storage}')
        name = names.get(ident)
        if not name:
            raise ValueError(
                'Missing descriptor name needed by AMD resource mapping')
        result[group].append((name, dec[33], count))
    for values in result.values():
        values.sort(key=lambda x: x[1])
    return result


def emit_blob(chunks, unique, data, resources):
    digest = hashlib.sha256(data).hexdigest()
    if digest in unique:
        return unique[digest]

    symbol = f's{len(unique)}'
    unique[digest] = symbol
    words = struct.unpack('<%dI' % (len(data) // 4), data)
    chunks.append(
        f'static const uint32_t {symbol}_data[] = {{' +
        ','.join(hex(w) for w in words) + '};\n')
    for group in GROUPS:
        values = resources[group]
        if values:
            chunks.append(
                f'static const char* {symbol}_{group}_names[] = {{' +
                ','.join(json.dumps(x[0]) for x in values) + '};\n')
            for suffix, value in [('slots', 1), ('counts', 2)]:
                chunks.append(
                    f'static const uint32_t {symbol}_{group}_{suffix}[] = {{' +
                    ','.join(str(x[value]) for x in values) + '};\n')
            chunks.append(
                f'static const uint32_t {symbol}_{group}_spaces[] = {{' +
                ','.join('0' for _ in values) + '};\n')
    fields = [
        f'(const uint8_t*){symbol}_data',
        str(len(data)),
    ] + [str(len(resources[g])) for g in GROUPS]
    for group in GROUPS:
        fields += [
            f'{symbol}_{group}_{suffix}' if resources[group] else 'nullptr'
            for suffix in ['names', 'slots', 'counts', 'spaces']
        ]
    chunks.append(
        f'static const FfxShaderBlob {symbol} = {{' +
        ','.join(fields) + '};\n')
    return symbol


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--sdk', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--compiler', required=True)
    args = parser.parse_args()

    gpu = args.sdk / 'sdk/include/FidelityFX/gpu'
    vk_shaders = args.sdk / 'sdk/src/backends/vk/shaders'
    out = args.output / 'spirv'
    out.mkdir(parents=True, exist_ok=True)

    common = [
        'FFX_GPU=1',
        'FFX_GLSL=1',
        'FFX_HALF=0',
        'FFX_SPD_NO_WAVE_OPERATIONS=1',
    ]

    def run_compile(source, output, defines, target_env='vulkan1.0'):
        command = [
            args.compiler, '-V', '--target-env', target_env,
            '-S', 'comp', '-Os',
            '-I' + str(args.output / 'include/FidelityFX/gpu'),
            '-I' + str(gpu),
        ]
        command += ['-D' + d for d in common + defines]
        command += [str(source), '-o', str(output)]
        completed = subprocess.run(
            command, stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
            text=True)
        if completed.returncode != 0:
            raise RuntimeError(
                'glslangValidator failed for ' + str(source) + '\n' +
                completed.stdout)
        data = output.read_bytes()
        return data, bindings(data)

    fsr_base = [
        'FFX_FSR3UPSCALER_OPTION_UPSAMPLE_SAMPLERS_USE_DATA_HALF=0',
        'FFX_FSR3UPSCALER_OPTION_ACCUMULATE_SAMPLERS_USE_DATA_HALF=0',
        'FFX_FSR3UPSCALER_OPTION_REPROJECT_SAMPLERS_USE_DATA_HALF=1',
        'FFX_FSR3UPSCALER_OPTION_POSTPROCESSLOCKSTATUS_SAMPLERS_USE_DATA_HALF=0',
        'FFX_FSR3UPSCALER_OPTION_UPSAMPLE_USE_LANCZOS_TYPE=2',
    ]

    def compile_fsr(job):
        pass_name, bits = job
        output = out / f'fsr_{pass_name}_{bits}.spv'
        defines = fsr_base + [
            f'FFX_FSR3UPSCALER_OPTION_{name}={(bits >> bit) & 1}'
            for bit, name in enumerate(FSR_DEFINES)
        ]
        source = (vk_shaders / 'fsr3upscaler' /
                  f'ffx_fsr3upscaler_{pass_name}_pass.glsl')
        data, reflected = run_compile(source, output, defines)
        return pass_name, bits, data, reflected

    def fi_source(pass_name):
        if pass_name == '__interpolation__':
            return vk_shaders / 'frameinterpolation' / 'ffx_frameinterpolation_pass.glsl'
        return (vk_shaders / 'frameinterpolation' /
                f'ffx_frameinterpolation_{pass_name}_pass.glsl')

    def compile_fi(job):
        pass_index, pass_name, bits = job
        output = out / f'fi_{pass_index}_{bits}.spv'
        defines = [
            f'FFX_FRAMEINTERPOLATION_OPTION_{name}={(bits >> bit) & 1}'
            for bit, name in enumerate(FI_DEFINES)
        ]
        data, reflected = run_compile(fi_source(pass_name), output, defines)
        return pass_index, bits, data, reflected

    def compile_of(job):
        pass_index, pass_name, hdr = job
        output = out / f'of_{pass_index}_{hdr}.spv'
        defines = [f'FFX_OPTICALFLOW_OPTION_HDR_COLOR_INPUT={hdr}']
        source = (vk_shaders / 'opticalflow' /
                  f'ffx_opticalflow_{pass_name}.glsl')
        # Optical Flow's advanced passes use subgroupShuffleXor through
        # GL_KHR_shader_subgroup_basic. Those instructions are core in
        # SPIR-V 1.3 / Vulkan 1.1, which is the minimum for FSR3 FG.
        data, reflected = run_compile(
            source, output, defines, target_env='vulkan1.1')
        return pass_index, hdr, data, reflected

    # Keep memory bounded on developer laptops and GitHub runners.
    with ThreadPoolExecutor(max_workers=4) as executor:
        fsr_compiled = list(executor.map(
            compile_fsr,
            ((p, b) for p in FSR_PASSES for b in range(64))))
        fi_compiled = list(executor.map(
            compile_fi,
            ((i, p, b) for i, p in enumerate(FI_PASSES)
             for b in range(8))))
        of_compiled = list(executor.map(
            compile_of,
            ((i, p, hdr) for i, p in enumerate(OF_PASSES)
             for hdr in range(2))))

    chunks = [
        '#include <FidelityFX/host/ffx_fsr3upscaler.h>\n',
        '#include <FidelityFX/host/ffx_frameinterpolation.h>\n',
        '#include <FidelityFX/host/ffx_opticalflow.h>\n',
        '#include <ffx_shader_blobs.h>\n',
        '#include <cstring>\n',
    ]
    unique = {}

    fsr_table = {}
    for name, bits, data, resources in fsr_compiled:
        fsr_table[name, bits] = emit_blob(
            chunks, unique, data, resources)

    # FSR3 has two pass IDs which intentionally use the same accumulate shader.
    fsr_order = FSR_PASSES[:7] + ['accumulate'] + FSR_PASSES[7:]
    chunks.append('static const FfxShaderBlob* fsrShaders[11][64] = {\n')
    for pass_name in fsr_order:
        chunks.append(
            '{' + ','.join(
                '&' + fsr_table[pass_name, b] for b in range(64)) + '},\n')
    chunks.append('};\n')

    fi_table = {}
    for pass_index, bits, data, resources in fi_compiled:
        fi_table[pass_index, bits] = emit_blob(
            chunks, unique, data, resources)
    chunks.append('static const FfxShaderBlob* fiShaders[11][8] = {\n')
    for pass_index in range(len(FI_PASSES)):
        chunks.append(
            '{' + ','.join(
                '&' + fi_table[pass_index, b] for b in range(8)) + '},\n')
    chunks.append('};\n')

    of_table = {}
    for pass_index, hdr, data, resources in of_compiled:
        of_table[pass_index, hdr] = emit_blob(
            chunks, unique, data, resources)
    chunks.append('static const FfxShaderBlob* ofShaders[7][2] = {\n')
    for pass_index in range(len(OF_PASSES)):
        chunks.append(
            '{' + ','.join(
                '&' + of_table[pass_index, hdr] for hdr in range(2)) + '},\n')
    chunks.append('};\n')

    chunks.append(
        'extern "C" FfxErrorCode ffxGetPermutationBlobByIndex('
        'FfxEffect effect, FfxPass pass, FfxBindStage stage, uint32_t bits, '
        'FfxShaderBlob* blob) {\n'
        ' if(!blob || stage != FFX_BIND_COMPUTE_SHADER_STAGE) '
        'return FFX_ERROR_INVALID_ARGUMENT;\n'
        ' const FfxShaderBlob* selected = nullptr;\n'
        ' if(effect == FFX_EFFECT_FSR3UPSCALER && pass < 11) '
        'selected = fsrShaders[pass][bits & 63];\n'
        ' else if(effect == FFX_EFFECT_FRAMEINTERPOLATION && pass < 11) '
        'selected = fiShaders[pass][bits & 7];\n'
        ' else if(effect == FFX_EFFECT_OPTICALFLOW && pass < 7) '
        'selected = ofShaders[pass][(bits >> 2) & 1];\n'
        ' else return FFX_ERROR_INVALID_ARGUMENT;\n'
        ' std::memcpy(blob, selected, sizeof(*blob)); return FFX_OK; }\n'
        'extern "C" FfxErrorCode ffxIsWave64('
        'FfxEffect, uint32_t, bool& wave64) { '
        'wave64 = false; return FFX_OK; }\n')

    (args.output / 'fsr_shaders.cpp').write_text(''.join(chunks))
    total = len(fsr_compiled) + len(fi_compiled) + len(of_compiled)
    print(
        f'FSR3/FG: compiled {total} permutations, '
        f'embedded {len(unique)} unique FP32 shaders '
        f'(upscale={len(fsr_compiled)}, fi={len(fi_compiled)}, '
        f'of={len(of_compiled)})',
        flush=True)


if __name__ == '__main__':
    main()
