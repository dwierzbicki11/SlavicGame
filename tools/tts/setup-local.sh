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
    if ! "$PYTHON" -m venv "$VENV"; then
        echo
        echo "[tts] Could not create the Python virtual environment."
        echo "[tts] On Ubuntu/Linux Mint install:"
        echo "      sudo apt install python3-venv"
        echo "      # or:"
        echo "      sudo apt install python3.12-venv"
        exit 1
    fi
fi

VENV_PY="$VENV/bin/python"

echo "[tts] Updating packaging tools..."
"$VENV_PY" -m pip install --upgrade pip setuptools wheel

echo "[tts] Installing Python 3.12-safe numeric/runtime base..."
"$VENV_PY" -m pip install --upgrade     "numpy==1.26.4"     "torch==2.6.0"     "torchaudio==2.6.0"

echo "[tts] Installing Chatterbox runtime dependencies for Polish speech..."
"$VENV_PY" -m pip install --upgrade     "librosa==0.11.0"     "s3tokenizer"     "transformers==5.2.0"     "diffusers==0.29.0"     "conformer==0.3.2"     "safetensors==0.5.3"     "pyloudnorm"     "omegaconf"     "git+https://github.com/resemble-ai/Perth.git@master"

echo "[tts] Installing chatterbox-tts==$CHATTERBOX_VERSION without upstream dependency resolution..."
"$VENV_PY" -m pip install --upgrade --no-deps "chatterbox-tts==$CHATTERBOX_VERSION"

echo "[tts] Verifying Polish multilingual runtime..."
"$VENV_PY" - <<'PY'
import sys
from importlib.metadata import version
import numpy
import torch
import torchaudio
from chatterbox.mtl_tts import ChatterboxMultilingualTTS

langs = ChatterboxMultilingualTTS.get_supported_languages()
assert "pl" in langs, "Installed Chatterbox does not expose Polish language support"

print(f"[tts] Python:      {sys.version.split()[0]}")
print(f"[tts] Chatterbox:  {version('chatterbox-tts')}")
print(f"[tts] NumPy:       {numpy.__version__}")
print(f"[tts] Torch:       {torch.__version__}")
print(f"[tts] Torchaudio:  {torchaudio.__version__}")
print(f"[tts] Polish:      {langs['pl']}")
print(f"[tts] CUDA:        {torch.cuda.is_available()}")
print("[tts] Import test: OK")
PY

echo
echo "Local Chatterbox TTS is ready for Polish spell incantations."
echo "The first spoken spell downloads the open model weights; later lines use the local model/cache."
echo "Optional voice references: assets/voice/reference/"
