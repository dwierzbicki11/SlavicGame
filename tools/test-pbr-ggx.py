#!/usr/bin/env python3
"""Check the production GLSL GGX function numerically without a GPU (needs c++)."""

from pathlib import Path
import re
import subprocess
import tempfile


root = Path(__file__).resolve().parents[1]
shader = (root / "shaders/src/pbr.frag").read_text()
function = re.search(r"float DistributionGGX\([^}]+\}", shader)
pi = re.search(r"const float PI = [^;]+;", shader)
if function is None or pi is None:
    raise SystemExit("Cannot locate production GGX function/PI")

# This scalar GLSL subset is also C++. Use float literals to retain GLSL's
# single precision, and supply only the vector operations used by the function.
production = re.sub(r"\b(\d+\.\d+(?:[eE][+-]?\d+)?)\b", r"\1f",
                    pi.group() + "\n" + function.group())
harness = r"""
#include <algorithm>
#include <cmath>
#include <iostream>
using std::clamp;
using std::max;
struct vec3 { float x, y, z; };
float dot(vec3 a, vec3 b) { return a.x*b.x + a.y*b.y + a.z*b.z; }
""" + production + r"""
int main()
{
    int checks = 0;
    float previousPeak = 0;
    for (float roughness : {1.0f, 0.82f, 0.5f, 0.2f, 0.1f, 0.08f, 0.06f})
    {
        const double alpha2 = std::pow(double(roughness), 4);
        const float peak = DistributionGGX({0,0,1}, {0,0,1}, roughness);
        if (!(peak > previousPeak)) return 1;
        previousPeak = peak;
        for (float cosine : {0.0f, 0.25f, 0.5f, 0.9f, 0.99f, 0.9999f, 1.0f})
        {
            // Double precision analytical reference, independent of the shader
            // denominator rearrangement and its former artificial floor.
            const double denominator = 1 + (alpha2 - 1) * double(cosine) * cosine;
            const double reference = alpha2 / (std::acos(-1.0) * denominator * denominator);
            const float actual = DistributionGGX({0,0,1}, {0,0,cosine}, roughness);
            if (!std::isfinite(actual) || actual <= 0 ||
                std::abs(actual - reference) > reference * 0.001)
            {
                std::cerr << "GGX mismatch: roughness=" << roughness
                          << " N.H=" << cosine << " actual=" << actual
                          << " expected=" << reference << '\n';
                return 1;
            }
            ++checks;
        }
    }
    std::cout << "PASS: " << checks << " GGX reference samples and 7 peak ordering checks\n";
}
"""
with tempfile.TemporaryDirectory(prefix="slavicgame-ggx-") as directory:
    source = Path(directory) / "ggx.cpp"
    binary = Path(directory) / "ggx"
    source.write_text(harness)
    subprocess.run(["c++", "-std=c++17", "-O2", str(source), "-o", str(binary)], check=True)
    subprocess.run([str(binary)], check=True)
