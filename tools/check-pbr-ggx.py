#!/usr/bin/env python3
"""Numerical regression for the scalar GGX function in the production shader."""

import math
from pathlib import Path
import re
import unittest


SOURCE = Path(__file__).resolve().parents[1] / "shaders/src/pbr.frag"
BODY = re.search(
    r"float DistributionGGX\([^)]*\)\s*\{([^}]+)\}", SOURCE.read_text()
).group(1)
BODY = re.sub(r"//[^\n]*", "", BODY)


def shader_ggx(ndoth, roughness):
    # Evaluate the actual scalar GLSL expressions, so changes to the shader
    # participate in these checks. This is a CPU math check, not GPU execution.
    values = dict(PI=3.14159265359, roughness=roughness, n=ndoth, h=1.0,
                  dot=lambda n, h: n * h, max=max)
    for statement in BODY.split(";"):
        statement = statement.strip()
        if not statement:
            continue
        if statement.startswith("return "):
            return eval(statement[7:], {"__builtins__": {}}, values)
        match = re.fullmatch(r"float (\w+)\s*=\s*(.+)", statement)
        if match is None:
            raise AssertionError(f"Unsupported GGX statement: {statement}")
        values[match[1]] = eval(match[2], {"__builtins__": {}}, values)
    raise AssertionError("Missing GGX return")


class GgxRegression(unittest.TestCase):
    def test_peak_matches_analytic_ggx(self):
        for roughness in (0.06, 0.08, 0.1, 0.2, 0.5, 1.0):
            with self.subTest(roughness=roughness):
                expected = 1.0 / (math.pi * roughness**4)
                self.assertAlmostEqual(shader_ggx(1.0, roughness) / expected, 1.0, places=10)

    def test_finite_nonnegative_and_matches_reference_across_lobe(self):
        for roughness in (0.06, 0.08, 0.2, 0.5, 1.0):
            for ndoth in (0.0, 0.25, 0.75, 0.99, 0.9999, 1.0):
                with self.subTest(roughness=roughness, ndoth=ndoth):
                    actual = shader_ggx(ndoth, roughness)
                    expected = roughness**4 / (
                        math.pi * (1.0 + ndoth**2 * (roughness**4 - 1.0))**2)
                    self.assertTrue(math.isfinite(actual) and actual >= 0.0)
                    self.assertAlmostEqual(actual / expected, 1.0, places=9)


if __name__ == "__main__":
    unittest.main()
