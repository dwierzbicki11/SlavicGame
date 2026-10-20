#!/usr/bin/env python3
"""Numerical regression for the actual scalar GGX shader function.

Uses a C++ compiler to evaluate the GLSL arithmetic as float32 without a GPU.
This does not replace GLSL/SPIR-V compilation or a Vulkan visual check.
"""

from pathlib import Path
import os
import re
import subprocess
import tempfile


root = Path(__file__).resolve().parents[1]
shader = (root / "shaders/src/pbr.frag").read_text()
function = re.search(r"float DistributionGGX\([^{}]+\)\s*\{[^{}]+\}", shader)
if function is None:
    raise RuntimeError("Cannot locate DistributionGGX in pbr.frag")

source = r"""
#include <algorithm>
#include <cmath>
#include <cstdio>
#include <cstdlib>
struct vec3 { float x, y, z; };
float dot(vec3 a, vec3 b) { return a.x*b.x + a.y*b.y + a.z*b.z; }
float max(float a, float b) { return std::max(a, b); }
float clamp(float v, float lo, float hi) { return std::clamp(v, lo, hi); }
constexpr float PI = 3.14159265359f;
""" + function.group() + r"""
int main() {
    int checks = 0;
    // The shader clamps authored roughness to [0.06, 1]. Include negative
    // dot products and slight normalization overshoot as boundary cases.
    for (int r = 0; r <= 94; ++r) {
        float roughness = 0.06f + r * 0.01f;
        for (int c = -1; c <= 1001; ++c) {
            float cosine = c * 0.001f;
            float actual = DistributionGGX({0, 0, 1}, {0, 0, cosine}, roughness);
            // Independent double-precision reference to the standard GGX NDF.
            double a2 = std::pow(double(roughness), 4);
            double ndoth = std::clamp(double(cosine), 0.0, 1.0);
            double denom = 1.0 + ndoth * ndoth * (a2 - 1.0);
            double expected = a2 / (std::acos(-1.0) * denom * denom);
            if (!std::isfinite(actual) || actual <= 0 ||
                std::abs(actual - expected) / expected > 0.0002) {
                std::fprintf(stderr, "GGX mismatch: roughness=%g NdotH=%g actual=%g expected=%g\n",
                             roughness, cosine, actual, expected);
                return EXIT_FAILURE;
            }
            ++checks;
        }
    }
    std::printf("PASS: %d GGX float32/reference comparisons\n", checks);
}
"""

with tempfile.TemporaryDirectory(prefix="slavic-ggx-") as directory:
    path = Path(directory)
    (path / "ggx.cpp").write_text(source)
    subprocess.run([os.environ.get("CXX", "c++"), "-std=c++17", "-O2",
                    str(path / "ggx.cpp"), "-o", str(path / "ggx")], check=True)
    subprocess.run([str(path / "ggx")], check=True)
