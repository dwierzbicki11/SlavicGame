#!/usr/bin/env python3
"""Check the actual scalar/vector GGX shader function against analytic values.

Requires Python 3 and a C++ compiler; no GPU or GLSL compiler is needed.
This numerical check does not replace GLSL -> SPIR-V compilation.
"""

from pathlib import Path
import re
import subprocess
import tempfile


root = Path(__file__).resolve().parents[1]
shader = (root / "shaders/src/pbr.frag").read_text()
match = re.search(r"float DistributionGGX\([^)]*\)\s*\{[^}]*\}", shader)
if match is None:
    raise SystemExit("DistributionGGX function not found")

# Compile the production function unchanged. Only GLSL vector builtins are
# supplied by this small CPU harness; all shader arithmetic remains float32.
harness = r"""
#include <algorithm>
#include <cmath>
#include <cstdio>
using std::max;
constexpr float PI = 3.14159265359f;
struct vec3 { float x, y, z; };
float dot(vec3 a, vec3 b) { return a.x*b.x + a.y*b.y + a.z*b.z; }
vec3 cross(vec3 a, vec3 b) {
    return {a.y*b.z-a.z*b.y, a.z*b.x-a.x*b.z, a.x*b.y-a.y*b.x};
}
// GLSL allows max(float, floating literal) without C++ template deduction.
float max(float a, double b) { return std::max(a, float(b)); }
""" + match[0] + r"""
int main() {
    int checks = 0;
    for (float roughness : {0.06f, 0.08f, 0.12f, 0.25f, 0.5f, 0.82f, 1.0f}) {
        for (double angle : {0.0, 0.0001, 0.001, 0.01, 0.1, 0.5, 1.0, 1.5707963267948966}) {
            vec3 n{0, 0, 1};
            vec3 h{float(std::sin(angle)), 0, float(std::cos(angle))};
            float actual = DistributionGGX(n, h, roughness);
            double a2 = std::pow(double(roughness), 4);
            double denom = std::pow(std::sin(angle), 2) + a2 * std::pow(std::cos(angle), 2);
            double expected = a2 / (std::acos(-1.0) * denom * denom);
            if (!std::isfinite(actual) || actual <= 0 || std::abs(actual/expected - 1) > 0.00002) {
                std::fprintf(stderr, "FAIL roughness=%g angle=%g: actual=%g expected=%g\n",
                    roughness, angle, actual, expected);
                return 1;
            }
            ++checks;
        }
    }
    std::printf("PASS: %d GGX samples match the analytic distribution (including smooth peaks and grazing angles).\n", checks);
}
"""

with tempfile.TemporaryDirectory(prefix="slavicgame-ggx-") as directory:
    source = Path(directory) / "ggx.cpp"
    binary = Path(directory) / "ggx"
    source.write_text(harness)
    subprocess.run(["c++", "-std=c++17", "-O2", str(source), "-o", str(binary)], check=True)
    subprocess.run([str(binary)], check=True)
