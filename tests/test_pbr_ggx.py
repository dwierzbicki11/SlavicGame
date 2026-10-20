"""CPU checks of the scalar GGX function read directly from the production GLSL.

Run with: python3 -m unittest discover -s tests -p 'test_pbr_ggx.py'
No Vulkan device or shader compiler is required.
"""
import math
from pathlib import Path
import re
import unittest


SOURCE = (Path(__file__).resolve().parents[1] / 'shaders/src/pbr.frag').read_text()
BODY = re.search(r'float DistributionGGX\([^)]*\)\s*\{([^}]+)\}', SOURCE).group(1)
BODY = re.sub(r'//[^\n]*', '', BODY)
STATEMENTS = []
for statement in BODY.split(';'):
    statement = statement.strip()
    if not statement:
        continue
    if statement.startswith('return '):
        STATEMENTS.append((None, compile(statement[7:], '<GLSL return>', 'eval')))
    else:
        match = re.fullmatch(r'float\s+(\w+)\s*=\s*(.+)', statement, re.S)
        if not match:
            raise AssertionError(f'Unsupported GLSL statement: {statement}')
        STATEMENTS.append((match[1], compile(match[2], '<GLSL scalar>', 'eval')))


def distribution(ndoth, roughness):
    # Interpret only scalar declarations/return from the actual shader, so a
    # regression in its denominator is tested rather than a duplicate formula.
    values = dict(PI=math.pi, n=ndoth, h=1.0, roughness=roughness,
                  dot=lambda n, h: n * h, max=max)
    for name, expression in STATEMENTS:
        value = eval(expression, {'__builtins__': {}}, values)
        if name is None:
            return value
        values[name] = value
    raise AssertionError('Missing GGX return')


class GgxRegression(unittest.TestCase):
    def test_peak_matches_analytic_value(self):
        for roughness in (0.06, 0.08, 0.1, 0.2, 0.5, 1.0):
            with self.subTest(roughness=roughness):
                expected = 1.0 / (math.pi * roughness ** 4)
                self.assertAlmostEqual(distribution(1.0, roughness) / expected, 1.0)

    def test_finite_nonnegative_across_supported_domain(self):
        for roughness in (0.06, 0.08, 0.2, 0.5, 1.0):
            for index in range(1001):
                value = distribution(index / 1000, roughness)
                self.assertTrue(math.isfinite(value) and value >= 0.0)

    def test_projected_hemisphere_integral_is_one(self):
        # Integrate D(N.H) * (N.H) over the hemisphere. Uniform sampling in
        # x=(N.H)^2 gives integral pi * integral_0^1 D(sqrt(x)) dx.
        count = 20000
        for roughness in (0.25, 0.5, 1.0):
            integral = math.pi * sum(
                distribution(math.sqrt((i + 0.5) / count), roughness)
                for i in range(count)) / count
            self.assertAlmostEqual(integral, 1.0, delta=0.0001)


if __name__ == '__main__':
    unittest.main()
