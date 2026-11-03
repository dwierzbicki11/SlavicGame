"""Exercise the Vulkan proof log gate without requiring a GPU or .NET."""
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


GATE = Path(__file__).resolve().parents[1] / 'tools' / 'check-vulkan-validation.py'
PROOFS = (
    ('Scene FG runtime proof:', []),
    ('Presentation FG runtime proof:', ['--presentation']),
    ('Upscaler orientation runtime proof:', ['--upscaler-orientation']),
)


class VulkanValidationRegression(unittest.TestCase):
    def run_gate(self, log, flags):
        with tempfile.TemporaryDirectory(prefix='slavic-validation-') as directory:
            path = Path(directory) / 'proof.log'
            path.write_text(log, encoding='utf-8')
            return subprocess.run(
                [sys.executable, str(GATE), str(path), *flags],
                capture_output=True, text=True, check=False)

    def test_completed_selected_proof(self):
        for proof, flags in PROOFS:
            for newline in ('\n', '\r\n'):
                with self.subTest(proof=proof, newline=newline):
                    result = self.run_gate(
                        f'driver diagnostics{newline}{proof} 36 checks passed; details{newline}', flags)
                    self.assertEqual(result.returncode, 0, result.stderr)

    def test_incomplete_or_unrelated_results_are_rejected(self):
        for proof, flags in PROOFS:
            other = next(name for name, _ in PROOFS if name != proof)
            for log in (
                '',
                f'{proof}\n{other} 47 checks passed;',
                f'{proof} 47\nchecks passed;',
                f'{proof} 0 checks passed;',
                f'{proof} -1 checks passed;',
                f'{proof} 47 checks passed',
                f'Expected summary: {proof} 47 checks passed;',
            ):
                with self.subTest(proof=proof, log=log):
                    result = self.run_gate(log, flags)
                    self.assertNotEqual(result.returncode, 0)
                    self.assertIn('did not complete', result.stderr)

    def test_validation_errors_override_completed_proof(self):
        for proof, flags in PROOFS:
            for before in (True, False):
                summary = f'{proof} 335 checks passed; details\n'
                error = 'Validation Error: [ VUID-regression ] invalid image\n'
                with self.subTest(proof=proof, before=before):
                    result = self.run_gate(
                        error + summary if before else summary + error, flags)
                    self.assertNotEqual(result.returncode, 0)
                    self.assertIn('Vulkan validation reported 1 errors.', result.stderr)


if __name__ == '__main__':
    unittest.main()
