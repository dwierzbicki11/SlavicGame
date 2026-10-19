#!/usr/bin/env python3
"""CPU numerical checks of the production GLSL GGX function (no GPU required)."""

import math
from pathlib import Path
import re
import unittest


SOURCE = (Path(__file__).resolve().parents[1] / "shaders/src/pbr.frag").read_text()
BODY = re.search(r"float DistributionGGX\([^)]*\)\s*\{([^}]+)\}", SOURCE).group(1)
PI = float(re.search(r"const float PI = ([0-9.]+);", SOURCE).group(1))


def distribution(ndoth, roughness):
    # Evaluate the scalar GLSL statements themselves, so a shader regression
    # cannot pass by leaving a separate Python implementation unchanged.
    scope = dict(PI=PI, roughness=roughness, n=(0, 0, 1), h=(0, 0, ndoth))
    functions = dict(max=max, clamp=lambda x, low, high: min(max(x, low), high),
                     dot=lambda a, b: sum(x * y for x, y in zip(a, b)))
    body = re.sub(r"//[^\n]*", "", BODY)
    for statement in body.split(";"):
        statement = statement.strip()
        if statement.startswith("return "):
            return eval(statement[7:], {"__builtins__": {}, **functions}, scope)
        if statement:
            name, expression = statement.removeprefix("float ").split("=", 1)
            scope[name.strip()] = eval(expression, {"__builtins__": {}, **functions}, scope)
    raise AssertionError("GGX function has no return")


class GgxRegression(unittest.TestCase):
    def test_smooth_material_peak(self):
        for roughness in (0.06, 0.08, 0.15, 0.3, 0.6, 1.0):
            with self.subTest(roughness=roughness):
                expected = 1.0 / (PI * roughness**4)
                self.assertAlmostEqual(distribution(1.0, roughness) / expected, 1.0, places=10)

    def test_finite_nonnegative_across_material_range(self):
        for roughness in (0.06, 0.08, 0.3, 0.6, 1.0):
            for ndoth in (0.0, 0.1, 0.5, 0.9, 0.99, 0.9999, 1.0, 1.000001):
                value = distribution(ndoth, roughness)
                self.assertTrue(math.isfinite(value) and value >= 0.0)

    def test_projected_hemisphere_normalization(self):
        # A microfacet distribution integrates to one under projected solid
        # angle: integral D(N.H) * (N.H) dOmega. Test broad and narrow lobes.
        steps = 10000
        for roughness in (0.3, 0.6, 1.0):
            total = sum(distribution((i + 0.5) / steps, roughness) *
                        ((i + 0.5) / steps) for i in range(steps))
            self.assertAlmostEqual(total * 2.0 * PI / steps, 1.0, delta=0.001)


if __name__ == "__main__":
    unittest.main()
