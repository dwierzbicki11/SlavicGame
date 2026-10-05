#!/usr/bin/env python3
"""Verify descriptor reflection against real Vulkan 1.0/1.1 compiler output."""
import argparse
from pathlib import Path
import subprocess
import tempfile

from compile_shaders import bindings, GROUPS


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--compiler', required=True)
    args = parser.parse_args()
    shader = '''#version 450
layout(local_size_x = 1) in;
layout(set = 0, binding = 0, std140) uniform Constants {
    uint delta;
} constants;
layout(set = 0, binding = 1, std430) buffer Counters {
    uint value;
} rw_counters;
layout(set = 0, binding = 2, std430) readonly buffer Input {
    uint bias;
} r_counters;
void main() { atomicAdd(rw_counters.value, constants.delta + r_counters.bias); }
'''
    expected = {group: [] for group in GROUPS}
    expected['cbv'] = [('constants', 0, 1)]
    expected['uavBuffer'] = [('rw_counters', 1, 1)]
    expected['srvBuffer'] = [('r_counters', 2, 1)]
    with tempfile.TemporaryDirectory(prefix='slavic-spirv-reflection-') as directory:
        root = Path(directory)
        source = root / 'counter.comp'
        source.write_text(shader)
        for target in ['vulkan1.0', 'vulkan1.1']:
            binary = root / (target + '.spv')
            subprocess.run([args.compiler, '-V', '--target-env', target, '-Os',
                            str(source), '-o', str(binary)], check=True,
                           stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
            actual = bindings(binary.read_bytes())
            if actual != expected:
                raise AssertionError(f'{target} descriptor contract: {actual!r}')
            print(f'{target}: constants, read-only storage and writable storage are distinct')


if __name__ == '__main__':
    main()
