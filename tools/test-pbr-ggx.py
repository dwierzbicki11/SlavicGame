#!/usr/bin/env python3
"""Numerical regression of the scalar GGX function read from the GLSL source."""
import math
from pathlib import Path
import re
import unittest


SOURCE = (Path(__file__).resolve().parents[1] / "shaders/src/pbr.frag").read_text()
BODY = re.search(r"float DistributionGGX\([^)]*\)\s*\{([^}]+)\}", SOURCE).group(1)
BODY = re.sub(r"//[^\n]*", "", BODY)


def distribution(ndoth, roughness):
    # Interpret the shader's scalar expressions, rather than maintaining a
    # separate copy of the formula that could pass while GLSL regresses.
    values = {
        "n": ndoth, "h": 1.0, "roughness": roughness, "PI": math.pi,
        "dot": lambda n, h: n * h, "max": max,
        "clamp": lambda x, lo, hi: min(max(x, lo), hi),
    }
    for statement in BODY.split(";"):
        statement = statement.strip()
        if statement.startswith("return "):
            return eval(statement[7:], {"__builtins__": {}}, values)
        if statement:
            name, expression = statement.removeprefix("float ").split("=", 1)
            values[name.strip()] = eval(expression, {"__builtins__": {}}, values)
    raise AssertionError("GGX function has no return")


class GgxRegression(unittest.TestCase):
    def test_peak_matches_analytic_distribution(self):
        for roughness in (0.06, 0.08, 0.1, 0.2, 0.5, 1.0):
            with self.subTest(roughness=roughness):
                expected = 1.0 / (math.pi * roughness ** 4)
                self.assertAlmostEqual(distribution(1.0, roughness) / expected, 1.0)

    def test_finite_nonnegative_and_increasing_toward_peak(self):
        for roughness in (0.06, 0.08, 0.2, 0.68, 1.0):
            previous = 0.0
            for ndoth in (0.0, 0.25, 0.5, 0.9, 0.99, 0.9999, 1.0):
                value = distribution(ndoth, roughness)
                self.assertTrue(math.isfinite(value))
                self.assertGreaterEqual(value, previous)
                previous = value

    def test_rough_surface_is_uniform(self):
        for ndoth in (0.0, 0.5, 1.0):
            self.assertAlmostEqual(distribution(ndoth, 1.0), 1.0 / math.pi)

    def test_dot_roundoff_is_clamped(self):
        self.assertEqual(distribution(1.0000001, 0.06), distribution(1.0, 0.06))


if __name__ == "__main__":
    unittest.main()
