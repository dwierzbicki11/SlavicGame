"""CPU-only regression tests for staging and rollback of shader publication."""
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import unittest

SCRIPT = Path(__file__).resolve().parents[1] / "tools" / "compile-shaders.sh"


class CompileShadersRegression(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory(prefix="slavic-shaders-")
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name)
        (self.root / "tools").mkdir()
        (self.root / "shaders" / "src").mkdir(parents=True)
        (self.root / "shaders" / "bin").mkdir(parents=True)
        shutil.copy2(SCRIPT, self.root / "tools" / "compile-shaders.sh")
        content = SCRIPT.read_text(encoding="utf-8")
        self.sources = re.findall(r'compile_shader "\$SRC_DIR/([^"\n]+)"', content)
        self.assertGreater(len(self.sources), 1, "No shader compile commands found")
        for source in self.sources:
            (self.root / "shaders" / "src" / source).write_text("void main() {}\n")
        fakebin = self.root / "fakebin"
        fakebin.mkdir()
        compiler = fakebin / "glslc"
        compiler.write_text("""#!/bin/sh
set -eu
source=''
output=''
while [ "$#" -gt 0 ]; do
    case "$1" in
        *.vert|*.frag) source="$1" ;;
        -o) shift; output="$1" ;;
    esac
    shift
done
name="${source##*/}"
if [ "${FAKE_SHADER_FAIL:-}" = "$name" ]; then
    printf partial > "$output"
    exit 23
fi
printf 'fresh:%s' "$name" > "$output"
""")
        compiler.chmod(0o755)
        for tool in ("dirname", "pwd", "mkdir", "mktemp", "rm", "mv", "cp"):
            (fakebin / tool).symlink_to(shutil.which(tool))
        self.env = os.environ.copy()
        self.env["PATH"] = str(fakebin)

    def run_build(self, fail_on=None):
        env = self.env.copy()
        if fail_on:
            env["FAKE_SHADER_FAIL"] = fail_on
        return subprocess.run(
            ["/bin/bash", str(self.root / "tools" / "compile-shaders.sh")],
            cwd=self.root, env=env, capture_output=True, text=True, check=False,
        )

    def test_failed_compile_preserves_all_existing_outputs(self):
        out = self.root / "shaders" / "bin"
        for source in self.sources:
            (out / (source + ".spv")).write_bytes(b"old")
        result = self.run_build(fail_on=self.sources[len(self.sources) // 2])
        self.assertNotEqual(result.returncode, 0, result.stdout)
        for source in self.sources:
            self.assertEqual((out / (source + ".spv")).read_bytes(), b"old", source)
        self.assertEqual(list(out.glob(".compile-shaders.*")), [])

    def test_glslang_validator_fallback(self):
        fakebin = self.root / "fakebin"
        (fakebin / "glslc").rename(fakebin / "glslangValidator")
        result = self.run_build()
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(len(list((self.root / "shaders" / "bin").glob("*.spv"))),
                         len(self.sources))

    def test_missing_compiler_preserves_existing_outputs(self):
        out = self.root / "shaders" / "bin"
        existing = out / (self.sources[0] + ".spv")
        existing.write_bytes(b"old")
        (self.root / "fakebin" / "glslc").rename(self.root / "fakebin" / "glslc.disabled")
        result = self.run_build()
        self.assertEqual(result.returncode, 127, result.stderr)
        self.assertEqual(existing.read_bytes(), b"old")
        self.assertEqual(list(out.glob(".compile-shaders.*")), [])

    def test_failed_publish_restores_previously_published_set(self):
        out = self.root / "shaders" / "bin"
        for source in self.sources:
            (out / (source + ".spv")).write_bytes(b"old")
        fakebin = self.root / "fakebin"
        (fakebin / "mv").unlink()
        (fakebin / "mv").write_text("""#!/bin/sh
case "$*" in
  *pbr.vert.spv*) exit 22 ;;
esac
exec /bin/mv "$@"
""")
        (fakebin / "mv").chmod(0o755)
        result = self.run_build()
        self.assertNotEqual(result.returncode, 0, result.stdout)
        for source in self.sources:
            self.assertEqual((out / (source + ".spv")).read_bytes(), b"old", source)
        self.assertEqual(list(out.glob(".compile-shaders.*")), [])

    def test_success_publishes_complete_set_after_failure(self):
        out = self.root / "shaders" / "bin"
        self.assertNotEqual(self.run_build(fail_on=self.sources[-1]).returncode, 0)
        self.assertEqual(list(out.glob("*.spv")), [])
        result = self.run_build()
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(len(list(out.glob("*.spv"))), len(self.sources))
        for source in self.sources:
            self.assertEqual((out / (source + ".spv")).read_bytes(),
                             b"fresh:" + source.encode(), source)
        self.assertEqual(list(out.glob(".compile-shaders.*")), [])


if __name__ == "__main__":
    unittest.main()
