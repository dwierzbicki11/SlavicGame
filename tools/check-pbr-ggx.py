#!/usr/bin/env python3
"""Check the scalar GLSL GGX function against the analytic distribution (CPU)."""
import math
from pathlib import Path
import re

source = (Path(__file__).resolve().parents[1] / "shaders/src/pbr.frag").read_text()
body = re.search(r"float DistributionGGX\([^)]*\)\s*\{([^}]+)\}", source).group(1)
# Execute the actual shader arithmetic; this deliberately supports only the
# straight-line scalar subset used by this function, not arbitrary GLSL.
body = re.sub(r"//[^\n]*", "", body)
lines = [re.sub(r"^float\s+", "", line.strip()).rstrip(";")
         for line in body.splitlines() if line.strip()]
namespace = {
    "PI": math.pi, "max": max,
    "clamp": lambda x, lo, hi: min(max(x, lo), hi),
    "dot": lambda n, h: sum(a * b for a, b in zip(n, h)),
}
exec("def distribution(n, h, roughness):\n" +
     "\n".join("    " + line for line in lines), namespace)
distribution = namespace["distribution"]

checks = 0
for roughness in (0.06, 0.08, 0.12, 0.25, 0.5, 0.82, 1.0):
    for cosine in (0.0, 0.1, 0.5, 0.9, 0.99, 0.999, 0.99999, 1.0):
        actual = distribution((0, 0, 1), (math.sqrt(1 - cosine**2), 0, cosine), roughness)
        alpha_squared = roughness**4
        expected = alpha_squared / (math.pi * (1 + cosine**2 * (alpha_squared - 1))**2)
        assert math.isfinite(actual) and actual > 0, (roughness, cosine, actual)
        assert math.isclose(actual, expected, rel_tol=1e-9), (roughness, cosine, actual, expected)
        checks += 1

# At N.H = 1 the peak must grow as roughness decreases, rather than collapse.
peaks = [distribution((0, 0, 1), (0, 0, 1), r) for r in (1.0, 0.5, 0.12, 0.06)]
assert all(a < b for a, b in zip(peaks, peaks[1:])), peaks
print(f"PASS: {checks} GGX reference samples and smooth-surface peak ordering")
