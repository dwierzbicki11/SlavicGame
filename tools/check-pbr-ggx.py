#!/usr/bin/env python3
"""Numerically check the actual GLSL GGX body using C float arithmetic.

Requires a C compiler. This CPU check does not replace SPIR-V/GPU validation.
"""
import os
from pathlib import Path
import re
import subprocess
import tempfile


root = Path(__file__).resolve().parents[1]
shader = (root / "shaders/src/pbr.frag").read_text()
body = re.search(r"float DistributionGGX\([^)]*\)\s*\{(.*?)\n\}", shader, re.S)
if body is None:
    raise SystemExit("DistributionGGX function not found")
body = re.sub(r"//[^\n]*", "", body.group(1))
# Scalar n/h stand for the dot product inputs. Suffix literals to reproduce
# GLSL's single precision, including intermediate arithmetic near the peak.
body = re.sub(r"\b(\d+\.\d*(?:e[+-]?\d+)?|\d+e[+-]?\d+)\b", r"\1f", body)
source = r"""
#include <math.h>
#include <stdio.h>
#define PI 3.14159265359f
#define dot(n,h) ((n)*(h))
#define max(a,b) fmaxf((a),(b))
#define clamp(x,a,b) fminf(fmaxf((x),(a)),(b))
float DistributionGGX(float n, float h, float roughness) {
""" + body + r"""
}
int main(void) {
    const float roughnesses[] = {0.06f, 0.08f, 0.1f, 0.2f, 0.5f, 0.82f, 1.0f};
    const float cosines[] = {0.0f, 0.1f, 0.5f, 0.9f, 0.99f, 0.999f, 0.9999f, 1.0f};
    int checks = 0;
    for (unsigned r = 0; r < sizeof(roughnesses)/sizeof(float); ++r) {
        for (unsigned c = 0; c < sizeof(cosines)/sizeof(float); ++c) {
            double rough = roughnesses[r], cosine = cosines[c];
            double alpha2 = pow(rough, 4.0);
            double denominator = 1.0 - cosine*cosine + cosine*cosine*alpha2;
            double expected = alpha2 / (acos(-1.0)*denominator*denominator);
            float actual = DistributionGGX(cosines[c], 1.0f, roughnesses[r]);
            if (!isfinite(actual) || actual <= 0 || fabs(actual/expected - 1.0) > 0.001) {
                fprintf(stderr, "GGX roughness=%g N.H=%g: got %g expected %g\n",
                        rough, cosine, actual, expected);
                return 1;
            }
            ++checks;
        }
    }
    printf("PASS: %d GGX samples match the double-precision reference (0.1%% tolerance).\n", checks);
    return 0;
}
"""
with tempfile.TemporaryDirectory(prefix="slavic-ggx-") as directory:
    src = Path(directory) / "ggx.c"
    binary = Path(directory) / "ggx"
    src.write_text(source)
    subprocess.run([os.environ.get("CC", "cc"), "-std=c99", "-Wall", "-Wextra",
                    str(src), "-lm", "-o", str(binary)], check=True)
    subprocess.run([str(binary)], check=True)
