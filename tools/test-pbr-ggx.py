#!/usr/bin/env python3
"""Check the production GLSL GGX expression against its analytical limits (CPU)."""
import math
from pathlib import Path
import re
import unittest

SOURCE = Path(__file__).resolve().parents[1] / 'shaders/src/pbr.frag'
BODY = re.search(
    r'float DistributionGGX\([^)]*\)\s*\{([^}]+)\}',
    SOURCE.read_text(),
).group(1)
BODY = re.sub(r'//[^\n]*', '', BODY)


def distribution(cosine, roughness):
    # Evaluate the scalar statements from the shader, rather than a duplicate
    # implementation. dot(n,h) supplies the angle for unit input vectors.
    values = {
        'roughness': roughness, 'PI': math.pi,
        'dot': lambda n, h: cosine, 'n': None, 'h': None,
        'max': max, 'clamp': lambda x, lo, hi: min(max(x, lo), hi),
    }
    for statement in BODY.split(';'):
        statement = statement.strip()
        if not statement:
            continue
        if statement.startswith('return '):
            return eval(statement[7:], {'__builtins__': {}}, values)
        match = re.fullmatch(r'float (\w+)\s*=\s*(.+)', statement, re.S)
        if match is None:
            raise AssertionError(f'Unsupported GLSL statement: {statement}')
        values[match[1]] = eval(match[2], {'__builtins__': {}}, values)
    raise AssertionError('Missing return')


class GgxRegression(unittest.TestCase):
    def test_peak_preserves_smooth_material_highlights(self):
        for roughness in (0.06, 0.08, 0.1, 0.2, 0.5, 1.0):
            with self.subTest(roughness=roughness):
                expected = 1.0 / (math.pi * roughness ** 4)
                self.assertAlmostEqual(distribution(1.0, roughness) / expected, 1.0)

    def test_grazing_limit(self):
        for roughness in (0.06, 0.2, 0.5, 1.0):
            self.assertAlmostEqual(distribution(0.0, roughness), roughness ** 4 / math.pi)

    def test_finite_nonnegative_across_material_range(self):
        for roughness in (0.06, 0.08, 0.2, 0.5, 1.0):
            for i in range(1001):
                value = distribution(i / 1000.0, roughness)
                self.assertTrue(math.isfinite(value) and value >= 0.0)
        self.assertEqual(distribution(1.000001, 0.06), distribution(1.0, 0.06))


if __name__ == '__main__':
    unittest.main()
