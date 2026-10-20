"""Numerical regression for the scalar GGX shader function (no GPU required).

Run: python3 -m unittest discover -s tests/renderer -p 'test_*.py'
Evaluates assignments from the actual GLSL body, rather than a copied formula.
This checks arithmetic, not GLSL compilation or GPU rendering.
"""
import math
from pathlib import Path
import re
import unittest

SHADER = Path(__file__).resolve().parents[2] / 'shaders/src/pbr.frag'


def distribution(roughness, cosine):
    source = SHADER.read_text()
    body = re.search(r'float DistributionGGX\([^)]*\)\s*\{([^}]+)\}', source).group(1)
    body = re.sub(r'//[^\n]*', '', body)
    values = dict(roughness=roughness, n=(0, 0, 1), h=(0, 0, cosine),
                  PI=3.14159265359, max=max,
                  dot=lambda a, b: sum(x * y for x, y in zip(a, b)))
    for statement in body.split(';'):
        statement = statement.strip()
        if statement.startswith('return '):
            return eval(statement[7:], {'__builtins__': {}}, values)
        if statement:
            name, expression = statement.removeprefix('float ').split('=', 1)
            values[name.strip()] = eval(expression, {'__builtins__': {}}, values)
    raise AssertionError('GGX return expression missing')


class GgxRegression(unittest.TestCase):
    def test_smooth_highlight_peak(self):
        for roughness in (0.06, 0.08, 0.12, 0.25, 0.5, 1.0):
            with self.subTest(roughness=roughness):
                expected = 1 / (math.pi * roughness ** 4)
                self.assertAlmostEqual(distribution(roughness, 1) / expected, 1, places=9)

    def test_lobe_matches_ggx_reference(self):
        for roughness in (0.06, 0.12, 0.5, 1.0):
            for cosine in (0, 0.2, 0.8, 0.99, 0.9999):
                with self.subTest(roughness=roughness, cosine=cosine):
                    alpha2 = roughness ** 4
                    expected = alpha2 / (math.pi * (1 + cosine ** 2 * (alpha2 - 1)) ** 2)
                    actual = distribution(roughness, cosine)
                    self.assertTrue(math.isfinite(actual))
                    self.assertGreater(actual, 0)
                    self.assertAlmostEqual(actual / expected, 1, places=9)

    def test_fully_rough_distribution_is_constant(self):
        for cosine in (0, 0.5, 1):
            self.assertAlmostEqual(distribution(1, cosine), 1 / math.pi)


if __name__ == '__main__':
    unittest.main()
