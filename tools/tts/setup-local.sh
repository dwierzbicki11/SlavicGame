#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
VENV="$ROOT/.venv-tts"
PYTHON="${PYTHON:-python3}"
CHATTERBOX_VERSION="${CHATTERBOX_VERSION:-0.1.7}"

"$PYTHON" - <<'PY'
import sys
v = sys.version_info
if not ((3, 10) <= v[:2] < (3, 14)):
    raise SystemExit(
        f"Unsupported Python {v.major}.{v.minor}.{v.micro}. "
        "SlavicGame local TTS supports Python 3.10-3.13."
    )
print(f"[tts] Using Python {v.major}.{v.minor}.{v.micro}")
PY

# If an old virtual environment was created with another Python minor version,
# recreate it. Mixing Python 3.11/3.12 site-packages is not safe.
SELECTED_MM="$("$PYTHON" -c 'import sys; print(f"{sys.version_info.major}.{sys.version_info.minor}")')"
if [[ -f "$VENV/pyvenv.cfg" ]]; then
    VENV_MM="$("$VENV/bin/python" -c 'import sys; print(f"{sys.version_info.major}.{sys.version_info.minor}")' 2>/dev/null || true)"
    if [[ "$VENV_MM" != "$SELECTED_MM" ]]; then
        echo "[tts] Recreating .venv-tts: old Python=$VENV_MM, selected Python=$SELECTED_MM"
        rm -rf "$VENV"
    fi
fi

if [[ ! -x "$VENV/bin/python" ]]; then
    echo "[tts] Creating virtual environment at $VENV"
    "$PYTHON" -m venv "$VENV"
fi

VENV_PY="$VENV/bin/python"

echo "[tts] Updating Python packaging tools..."
"$VENV_PY" -m pip install --upgrade pip setuptools wheel

# Chatterbox 0.1.7 supports Python >=3.10. Installing NumPy first avoids
# historical build-order issues on Python 3.12 in tokenizer dependencies.
if "$VENV_PY" -c 'import sys; raise SystemExit(0 if sys.version_info < (3,13) else 1)'; then
    "$VENV_PY" -m pip install --upgrade "numpy>=1.24,<2"
else
    "$VENV_PY" -m pip install --upgrade "numpy>=2"
fi

echo "[tts] Installing chatterbox-tts==$CHATTERBOX_VERSION ..."
"$VENV_PY" -m pip install --upgrade "chatterbox-tts==$CHATTERBOX_VERSION"

echo "[tts] Verifying runtime imports..."
"$VENV_PY" - <<'PY'
import sys
from importlib.metadata import version
import numpy
import torch
import torchaudio
from chatterbox.mtl_tts import ChatterboxMultilingualTTS

print(f"[tts] Python:      {sys.version.split()[0]}")
print(f"[tts] Chatterbox:  {version('chatterbox-tts')}")
print(f"[tts] NumPy:       {numpy.__version__}")
print(f"[tts] Torch:       {torch.__version__}")
print(f"[tts] Torchaudio:  {torchaudio.__version__}")
print(f"[tts] CUDA:        {torch.cuda.is_available()}")
print("[tts] Import test: OK")
PY

echo
echo "Local Chatterbox TTS is ready."
echo "The first spoken spell downloads the open model weights; later lines use the local model/cache."
echo "Optional voice references: assets/voice/reference/"
