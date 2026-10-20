"""CPU numerical regression for the scalar GLSL GGX distribution (no GPU proof).

Run with: python3 -m unittest discover -s tests -p 'test_pbr_ggx.py'
The expressions come from the shader so changing its denominator is covered.
"""
import math
from pathlib import Path
import re
import unittest


SHADER = (Path(__file__).resolve().parents[1] / "shaders/src/pbr.frag").read_text()
BODY = re.search(r"float DistributionGGX\([^)]*\)\s*\{([^}]+)\}", SHADER).group(1)


def shader_distribution(cosine, roughness):
    scope = {"PI": math.pi, "roughness": roughness, "n": cosine, "h": 1.0,
             "dot": lambda n, h: n * h, "max": max}
    body = re.sub(r"//[^\n]*", "", BODY)
    for statement in body.split(";"):
        statement = statement.strip()
        if statement.startswith("float "):
            name, expression = statement[6:].split("=", 1)
            scope[name.strip()] = eval(expression.strip(), {"__builtins__": {}}, scope)
        elif statement.startswith("return "):
            return eval(statement[7:], {"__builtins__": {}}, scope)
    raise AssertionError("GGX shader has no return")


class GgxRegression(unittest.TestCase):
    def test_smooth_material_highlight_peak(self):
        for roughness in (0.06, 0.08, 0.2, 0.5, 1.0):
            with self.subTest(roughness=roughness):
                expected = 1.0 / (math.pi * roughness ** 4)
                self.assertAlmostEqual(shader_distribution(1.0, roughness) / expected, 1.0, places=6)

    def test_distribution_matches_ggx_across_materials_and_angles(self):
        for roughness in (0.06, 0.08, 0.2, 0.5, 1.0):
            for cosine in (-0.5, 0.0, 0.2, 0.8, 0.99, 0.9999, 1.0):
                with self.subTest(roughness=roughness, cosine=cosine):
                    alpha = roughness ** 2
                    c = max(cosine, 0.0)
                    expected = alpha ** 2 / (math.pi * (1 - c*c + c*c*alpha*alpha) ** 2)
                    actual = shader_distribution(cosine, roughness)
                    self.assertTrue(math.isfinite(actual))
                    self.assertGreater(actual, 0.0)
                    self.assertAlmostEqual(actual / expected, 1.0, places=6)


if __name__ == "__main__":
    unittest.main()
