#!/usr/bin/env python3
"""Exercise the shader's actual GGX function with native float arithmetic.

Run with python3 tools/test-pbr-ggx.py. Requires a C++ compiler, no GPU.
The small adapter supplies only GLSL's vec3/dot/clamp operations.
"""

from pathlib import Path
import re
import subprocess
import tempfile


root = Path(__file__).resolve().parents[1]
shader = (root / "shaders/src/pbr.frag").read_text()
function = re.search(r"float DistributionGGX\([^}]+\}", shader).group(0)
pi = re.search(r"const float PI = [^;]+;", shader).group(0)
adapter = r"""
#include <algorithm>
#include <cmath>
#include <cstdio>
struct vec3 { float x, y, z; };
float dot(vec3 a, vec3 b) { return a.x*b.x + a.y*b.y + a.z*b.z; }
using std::max;
using std::clamp;
"""
# GLSL literals have float precision; preserve that in the C++ adapter.
glsl = re.sub(r"\b(\d+\.\d+)\b", r"\1f", pi + "\n" + function)
harness = r"""
int main()
{
    const vec3 n = {0, 0, 1};
    int checks = 0;
    for (float roughness : {0.06f, 0.08f, 0.1f, 0.25f, 0.5f, 1.0f})
    {
        for (float cosine : {0.0f, 0.1f, 0.5f, 0.9f, 0.99f, 0.9999f, 1.0f})
        {
            const vec3 h = {std::sqrt(1-cosine*cosine), 0, cosine};
            const float actual = DistributionGGX(n, h, roughness);
            const double a2 = std::pow(double(roughness), 4);
            const double c2 = double(cosine)*cosine;
            const double denominator = (1-c2) + c2*a2;
            const double expected = a2 / (double(PI)*denominator*denominator);
            if (!std::isfinite(actual) || actual <= 0 ||
                std::abs(actual-expected) > expected*0.0005)
            {
                std::fprintf(stderr, "GGX roughness=%g cosine=%g: got %g expected %g\n",
                             roughness, cosine, actual, expected);
                return 1;
            }
            ++checks;
        }
    }
    // Normalized float vectors can round slightly outside the cosine domain.
    const float peak = DistributionGGX(n, n, 0.06f);
    if (DistributionGGX(n, {0, 0, 1.000001f}, 0.06f) != peak)
        return 1;
    std::printf("PASS: %d GGX reference cases and cosine roundoff guard\n", checks);
}
"""
with tempfile.TemporaryDirectory(prefix="slavic-ggx-") as directory:
    source = Path(directory) / "ggx.cpp"
    binary = Path(directory) / "ggx-test"
    source.write_text(adapter + glsl + harness)
    subprocess.run(["c++", "-std=c++17", "-O2", str(source), "-o", str(binary)], check=True)
    subprocess.run([str(binary)], check=True)
