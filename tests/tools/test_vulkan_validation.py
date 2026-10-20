"""Exercise the CI log gate without a GPU or native FSR provider."""
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

GATE = Path(__file__).resolve().parents[2] / 'tools/check-vulkan-validation.py'
PROOFS = (
    ([], 'Scene FG runtime proof:'),
    (['--presentation'], 'Presentation FG runtime proof:'),
    (['--upscaler-orientation'], 'Upscaler orientation runtime proof:'),
)


class VulkanValidationGateTests(unittest.TestCase):
    def run_gate(self, log, options):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'proof.log'
            path.write_text(log)
            return subprocess.run([sys.executable, str(GATE), str(path), *options],
                                  capture_output=True, text=True)

    def test_complete_proofs(self):
        for options, proof in PROOFS:
            for ending in ('\n', '\r\n', ''):
                with self.subTest(proof=proof, ending=repr(ending)):
                    result = self.run_gate(proof + ' 36 checks passed; details.' + ending, options)
                    self.assertEqual(result.returncode, 0, result.stderr)

    def test_incomplete_or_mismatched_proofs(self):
        for options, proof in PROOFS:
            for log in ('', proof + ' starting\nOther test: 36 checks passed;\n',
                        proof + '\n36 checks passed;\n', proof + ' 0 checks passed;\n',
                        'Diagnostic mentions ' + proof + ' 36 checks passed;\n',
                        'Unrelated runtime proof: 36 checks passed;\n'):
                with self.subTest(proof=proof, log=log):
                    self.assertNotEqual(self.run_gate(log, options).returncode, 0)

    def test_validation_errors_before_and_after_completion(self):
        for options, proof in PROOFS:
            completed = proof + ' 36 checks passed;\n'
            error = 'Validation Error: VUID-test\n'
            for log in (error + completed, completed + error):
                with self.subTest(proof=proof, log=log):
                    result = self.run_gate(log, options)
                    self.assertNotEqual(result.returncode, 0)
                    self.assertIn('Vulkan validation reported 1 errors.', result.stderr)


if __name__ == '__main__':
    unittest.main()
